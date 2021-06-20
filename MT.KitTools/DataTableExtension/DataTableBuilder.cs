using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MT.KitTools.DataTableExtension {
    public class DataTableBuilder {
        private static Dictionary<Type, Func<DataRow, object>> cache;

        private static MethodInfo Datarow_getItem = typeof(DataRow).GetMethod("get_Item", new Type[] { typeof(string) });
        private static MethodInfo DataRow_IsNull = typeof(DataRow).GetMethod("IsNull", new Type[] { typeof(string) });

        private static Func<DataRow, object> GetFunc(Type type) {
            if (cache == null) {
                cache = new Dictionary<Type, Func<DataRow, object>>();
            }
            cache.TryGetValue(type, out var func);
            return func;
        }
        public static Func<DataRow, object> GetCreator(Type type, DataColumnCollection dataColumn, bool mustMapAll = false) {
            Func<DataRow, object> func = GetFunc(type);
            if (func == null) {
                func = CreateFunc(type, dataColumn, mustMapAll);
                cache.Add(type, func);
            }

            return func;

            Func<DataRow, object> CreateFunc(Type type1, DataColumnCollection cols, bool mapAll) {
                // Datarow row; 
                ParameterExpression rowExp = Expression.Parameter(typeof(DataRow), "row");
                List<MemberBinding> bindings = new List<MemberBinding>();
                // fields
                var fields = type1.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo field in fields) {
                    void work() {
                        if (cols.Contains(field.Name)) {
                            DataColumn col = cols[field.Name];
                            var valueExp = GetTargetValueExpression(col, rowExp, field.FieldType);
                            MemberAssignment memberAssignment = Expression.Bind(field, valueExp);
                            bindings.Add(memberAssignment);
                            return;
                        }
                        if (mapAll) {
                            throw new ArgumentException($"Property {field.Name} is not matched by any column in datatable");
                        }
                    }
                    work();
                }

                // properties
                var props = type1.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo prop in props) {
                    void work() {
                        if (cols.Contains(prop.Name)) {
                            if (!prop.CanWrite) return;
                            DataColumn col = cols[prop.Name];
                            var valueExp = GetTargetValueExpression(col, rowExp, prop.PropertyType);
                            MemberAssignment memberAssignment = Expression.Bind(prop, valueExp);
                            bindings.Add(memberAssignment);
                            return;
                        }
                        if (mapAll) {
                            throw new ArgumentException($"Property {prop.Name} is not matched by any column in datatable");
                        }

                    }
                    work();
                }

                MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(type1), bindings);
                var lambda = Expression.Lambda<Func<DataRow, object>>(memberInitExpression, rowExp);
                return lambda.Compile();
            }

        }
        private static Expression GetTargetValueExpression(DataColumn column, ParameterExpression parameterExpression, Type targetType) {
            MethodCallExpression rowObjExp = Expression.Call(parameterExpression, Datarow_getItem, Expression.Constant(column.ColumnName));
            MethodCallExpression checkNullExp = Expression.Call(parameterExpression, DataRow_IsNull, Expression.Constant(column.ColumnName));
            Expression realValueExp = GetConversionExpression(rowObjExp, column.DataType, targetType);
            if (column.AllowDBNull) {
                return Expression.Condition(
                    checkNullExp,
                    Expression.Default(targetType),
                    realValueExp,
                    targetType
                    );
            } else {
                return realValueExp;
            }
        }

        private static Expression GetConversionExpression(Expression source, Type sourceType, Type targetType) {
            if (ReferenceEquals(sourceType, targetType)) {
                return Expression.Convert(source, sourceType);
            } else if (ReferenceEquals(sourceType, typeof(string))) {
                return GetParseExpression(source, targetType, CultureInfo.CurrentCulture);
            } else if (ReferenceEquals(targetType, typeof(string))) {
                return Expression.Call(source, sourceType.GetMethod("ToString", Type.EmptyTypes));
            } else if (ReferenceEquals(targetType, typeof(bool))) {
                MethodInfo ToBooleanMethod = typeof(Convert).GetMethod("ToBoolean", new[] { sourceType });
                return Expression.Call(ToBooleanMethod, source);
            } else if (ReferenceEquals(sourceType, typeof(byte[]))) {
                return GetArrayHandlerExpression(source, targetType);
            } else {
                return Expression.Convert(source, targetType);
            }
        }

        private static Expression GetArrayHandlerExpression(Expression sourceExpression, Type targetType) {
            Expression TargetExpression = default;
            if (ReferenceEquals(targetType, typeof(byte[]))) {
                TargetExpression = sourceExpression;
            } else if (ReferenceEquals(targetType, typeof(MemoryStream))) {
                ConstructorInfo ConstructorInfo = targetType.GetConstructor(new[] { typeof(byte[]) });
                TargetExpression = Expression.New(ConstructorInfo, sourceExpression);
            } else {
                throw new ArgumentException("Cannot convert a byte array to " + targetType.Name);
            }
            return TargetExpression;
        }

        private static Expression GetParseExpression(Expression SourceExpression, Type TargetType, CultureInfo Culture) {
            Type UnderlyingType = Nullable.GetUnderlyingType(TargetType) ?? TargetType;
            if (UnderlyingType.IsEnum) {
                MethodCallExpression ParsedEnumExpression = GetEnumParseExpression(SourceExpression, UnderlyingType);
                //Enum.Parse returns an object that needs to be unboxed
                return Expression.Unbox(ParsedEnumExpression, TargetType);
            } else {
                Expression ParseExpression = default;
                switch (UnderlyingType.FullName) {
                    case "System.Byte":
                    case "System.UInt16":
                    case "System.UInt32":
                    case "System.UInt64":
                    case "System.SByte":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Double":
                    case "System.Decimal":
                        ParseExpression = GetNumberParseExpression(SourceExpression, UnderlyingType, Culture);
                        break;
                    case "System.DateTime":
                        ParseExpression = GetDateTimeParseExpression(SourceExpression, UnderlyingType, Culture);
                        break;
                    case "System.Boolean":
                    case "System.Char":
                        ParseExpression = GetGenericParseExpression(SourceExpression, UnderlyingType);
                        break;
                    default:
                        throw new ArgumentException(string.Format("Conversion from {0} to {1} is not supported", "String", TargetType));
                }
                if (Nullable.GetUnderlyingType(TargetType) == null) {
                    return ParseExpression;
                } else {
                    //Convert to nullable if necessary
                    return Expression.Convert(ParseExpression, TargetType);
                }
            }
            Expression GetGenericParseExpression(Expression sourceExpression, Type type) {
                MethodInfo ParseMetod = type.GetMethod("Parse", new[] { typeof(string) });
                MethodCallExpression CallExpression = Expression.Call(ParseMetod, new[] { sourceExpression });
                return CallExpression;
            }
            Expression GetDateTimeParseExpression(Expression sourceExpression, Type type, CultureInfo culture) {
                MethodInfo ParseMetod = type.GetMethod("Parse", new[] { typeof(string), typeof(DateTimeFormatInfo) });
                ConstantExpression ProviderExpression = Expression.Constant(culture.DateTimeFormat, typeof(DateTimeFormatInfo));
                MethodCallExpression CallExpression = Expression.Call(ParseMetod, new[] { sourceExpression, ProviderExpression });
                return CallExpression;
            }

            MethodCallExpression GetEnumParseExpression(Expression sourceExpression, Type type) {
                //Get the MethodInfo for parsing an Enum
                MethodInfo EnumParseMethod = typeof(Enum).GetMethod("Parse", new[] { typeof(Type), typeof(string), typeof(bool) });
                ConstantExpression TargetMemberTypeExpression = Expression.Constant(type);
                ConstantExpression IgnoreCase = Expression.Constant(true, typeof(bool));
                //Create an expression the calls the Parse method
                MethodCallExpression CallExpression = Expression.Call(EnumParseMethod, new[] { TargetMemberTypeExpression, sourceExpression, IgnoreCase });
                return CallExpression;
            }

            MethodCallExpression GetNumberParseExpression(Expression sourceExpression, Type type, CultureInfo culture) {
                MethodInfo ParseMetod = type.GetMethod("Parse", new[] { typeof(string), typeof(NumberFormatInfo) });
                ConstantExpression ProviderExpression = Expression.Constant(culture.NumberFormat, typeof(NumberFormatInfo));
                MethodCallExpression CallExpression = Expression.Call(ParseMetod, new[] { sourceExpression, ProviderExpression });
                return CallExpression;
            }
        }
    }
}
