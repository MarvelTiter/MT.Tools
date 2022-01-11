using MT.KitTools.ExpressionHelper;
using MT.KitTools.TypeExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MT.KitTools.Mapper.ExpressionCore {
    internal class CreateExpression {
        internal static LambdaExpression ExpressionBuilder(MapInfo p) {
            var sourceParameter = Expression.Parameter(typeof(object), "sourceParameter");
            p.SourceExpression = Expression.Variable(p.SourceType, "source");
            var body = new List<Expression>();
            if (p.SourceType.IsValueType) {
                body.Add(Expression.Assign(p.SourceExpression, Expression.Unbox(sourceParameter, p.SourceType)));
            } else {
                body.Add(Expression.Assign(p.SourceExpression, Expression.TypeAs(sourceParameter, p.SourceType)));
            }
            var func = GetHandler(p);
            var expression = func.Invoke(p);
            body.Add(expression);
            BlockExpression block = Expression.Block(new[] { p.SourceExpression as ParameterExpression }, body);
            LambdaExpression lambda = Expression.Lambda(block, sourceParameter);
            return lambda;
        }
        internal static Func<MapInfo, Expression> GetHandler(MapInfo p) {
            var sourceType = p.SourceType;
            var targetType = p.TargetType;
            if (sourceType.IsDictionary())
                return MapFromDictionary;
            else if (targetType.IsDictionary())
                return MapToDictionary;
            else if (sourceType.IsClass && targetType.IsClass)
                return ClassMap;
            else if (sourceType.IsICollectionType() && targetType.IsICollectionType())
                return CollectionMap;
            throw new NotImplementedException($"not implement map between {sourceType.Name} and {targetType.Name}");
        }

        internal static Expression MapFromDictionary(MapInfo p) {
            List<Expression> body = new List<Expression>();
            var genericArgs = p.SourceType.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];
            if (keyType != typeof(string)) {
                throw new ArgumentException("key type must be string");
            }
            throw new NotImplementedException();
        }

        internal static Expression MapToDictionary(MapInfo p) {
            List<Expression> body = new List<Expression>();
            var genericArgs = p.TargetType.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];
            if (keyType != typeof(string)) {
                throw new ArgumentException("key type must be string");
            }
            var dicType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
            MethodInfo addMethod = dicType.GetMethod("Add", genericArgs);
            var props = p.SourceType.GetProperties();
            // var dic = new Dictionary<string, object>();
            ParameterExpression dicExpression = Expression.Variable(dicType, "dic");
            //body.Add(dicExpression);
            body.Add(Expression.Assign(dicExpression, Expression.New(dicType)));
            // dic.Add();
            foreach (PropertyInfo property in props) {
                if (!property.CanRead) continue;
                if (valueType == typeof(object) || valueType == property.PropertyType) {
                    var key = Expression.Constant(property.Name, keyType);
                    var value = Expression.Property(p.SourceExpression, property);
                    MethodCallExpression callAdd = Expression.Call(dicExpression, addMethod, key, Expression.Convert(value, valueType));
                    body.Add(callAdd);
                }
            }
            // return dic;
            body.Add(Expression.Convert(dicExpression, p.TargetType));
            var block = Expression.Block(new[] { dicExpression }, body);
            return block;
        }

        internal static Expression ClassMap(MapInfo p) {
            var source = p.SourceExpression as ParameterExpression;
            List<MemberBinding> bindings = new List<MemberBinding>();
            initBindings(ref bindings, source, p.Rules);
            MemberInitExpression body = Expression.MemberInit(Expression.New(p.TargetElementType), bindings);
            return body;
        }

        internal static MemberInitExpression ClassMap(List<MappingRule> rules, Type sourceType, Type targetType, object source) {
            ParameterExpression parameterExpression = Expression.Variable(sourceType, "source");
            Expression.Assign(parameterExpression, Expression.Constant(source, sourceType));
            List<MemberBinding> bindings = new List<MemberBinding>();
            initBindings(ref bindings, parameterExpression, rules);
            return Expression.MemberInit(Expression.New(targetType), bindings);
        }

        private static void initBindings(ref List<MemberBinding> memberBindings, ParameterExpression parameterExpression, IList<MappingRule> rules) {
            foreach (var rule in rules) {
                Expression valueExp = GetValueExpression(parameterExpression, rule);
                MemberAssignment bind = Expression.Bind(rule.MapTo, valueExp);
                memberBindings.Add(bind);
            }
        }
        private static Expression GetValueExpression(ParameterExpression parameter, MappingRule rule) {
            var prop = rule.MapFrom;
            var bind = Expression.Property(parameter, prop);
            Expression convertedBind = DataTypeConvert.GetConversionExpression(bind, rule.MapFrom.PropertyType, rule.MapTo.PropertyType);
            return convertedBind;
        }

        internal static Expression CollectionMap(MapInfo p) {
            List<Expression> body = new List<Expression>();
            var source = p.SourceExpression as ParameterExpression;

            var moveNext = typeof(IEnumerator).GetMethod("MoveNext");
            var getEnumerator = p.SourceType.GetMethod("GetEnumerator");
            if (getEnumerator == null) {
                getEnumerator = typeof(IEnumerable<>).MakeGenericType(p.SourceElementType).GetMethod("GetEnumerator");
            }
            /*
            * var enumerator = source.GetEnumerator();
            * var ret = new T()
            * while(true) {
            *   if (!enumerator.MoveNext()) {
            *     goto endLabel;   
            *   }
            *   var t = enumerator.Current;
            *   MapperLink<,>.Map(t);
            * }
            * endLabel:
            */
            LabelTarget endLabel = Expression.Label("end");

            //var linkMap = typeof(Mapper.MapperLink<,>).MakeGenericType(p.SourceElementType, p.TargetElementType).GetMethod("Map");
            var classMap = typeof(CreateExpression).GetMethod("ClassMap"
                , BindingFlags.NonPublic | BindingFlags.Static
                , null
                , new[] { typeof(List<MappingRule>), typeof(Type), typeof(Type), typeof(object) }
                , null);
            var listType = typeof(List<>).MakeGenericType(p.TargetElementType);
            var addMethod = listType.GetMethod("Add");
            ParameterExpression listExpression = Expression.Variable(listType, "list");
            ParameterExpression temp = Expression.Variable(p.TargetElementType, "temp");
            body.Add(Expression.Assign(listExpression, Expression.New(listType)));

            if (p.TargetType.IsICollectionType()) {
                List<Expression> loopBody = new List<Expression>();
                MethodCallExpression enumerator = Expression.Call(source, getEnumerator);
                ConditionalExpression loopCondition = Expression.IfThen(
                    Expression.IsFalse(Expression.Call(enumerator, moveNext)),
                    Expression.Break(endLabel)
                    );
                MemberExpression current = Expression.Property(enumerator, "Current");
                var targetValue = Expression.Call(classMap
                    , Expression.Constant(p.Rules, typeof(List<MappingRule>))
                    , Expression.Constant(p.SourceElementType, typeof(Type))
                    , Expression.Constant(p.TargetElementType, typeof(Type))
                    , current);
                Expression.Assign(temp, targetValue);
                var listAdd = Expression.Call(listExpression, addMethod, temp);
                loopBody.Add(loopCondition);
                loopBody.Add(listAdd) ;
                var loop = Expression.Loop(Expression.Block(loopBody), endLabel);
                body.Add(loop);
                //body.Add(Expression.Label(endLabel));
            }
            body.Add(Expression.Convert(listExpression, p.TargetType));
            BlockExpression block = Expression.Block(new[] { listExpression }, body);
            return block;
        }
    }

}
