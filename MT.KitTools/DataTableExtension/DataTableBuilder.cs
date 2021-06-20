using MT.KitTools.ExpressionHelper;
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
            Expression realValueExp = DataTypeConvert.GetConversionExpression(rowObjExp, column.DataType, targetType);
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
    }
}
