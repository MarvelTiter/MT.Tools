using Shared.ReflectionUtils.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Shared.Mapper.Core {
    public static class Mapper {


        public static Target Map<Source, Target>(Source source) {
            //Type type = typeof(MapperLink<,>);
            //type = type.MakeGenericType(typeof(Source), typeof(Target));
            //return type.Invoke<Target>("Map", source);
            return MapperLink<Source, Target>.Map(source);
        }

        /// <summary>
        /// 配置映射
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <param name="context"></param>
        public static void CreateMap<Source, Target>(Action<MappingProfile<Source, Target>> context = null) {
            var map = internalCreate<Source, Target>();
            context?.Invoke(map);
            map.AutoMap();
        }
        /// <summary>
        /// 创建 MappingProfile，并检查是否重复
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <returns></returns>
        private static MappingProfile<Source, Target> internalCreate<Source, Target>(bool autoMap = false) {
            var map = new MappingProfile<Source, Target>();
            ProfileProvider.Cache(map, typeof(Source), typeof(Target));
            if (autoMap) map.AutoMap();
            return map;
        }

        /// <summary>
        /// 泛型缓存
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        private static class MapperLink<Source, Target> {
            private static readonly Func<Source, Target> converter;
            static MapperLink() {
                // 
                Type sourceType = typeof(Source);
                Type targetType = typeof(Target);
                if (sourceType.IsGenericType) {
                    var sf = sourceType.GenericTypeArguments;
                    var tf = targetType.GenericTypeArguments;
                    Type type = typeof(MapperLink<,>);
                    type.MakeGenericType(sf[0], tf[0]);                    
                }

                var profile = ProfileProvider.GetProfile(sourceType, targetType);
                if (profile == null) {
                    profile = internalCreate<Source, Target>(true);
                }
                //
                var parameter = Expression.Parameter(sourceType, "source");
                // get all field and property

                List<MemberBinding> bindings = new List<MemberBinding>();
                var direction = profile.GetDirection(sourceType, targetType);
                if (direction == Direction.Forward)
                    forwardBindings(ref bindings, parameter, profile.Rules);
                else
                    backwardBindings(ref bindings, parameter, profile.Rules);

                MemberInitExpression body = Expression.MemberInit(Expression.New(targetType), bindings);
                Expression<Func<Source, Target>> lambda = Expression.Lambda<Func<Source, Target>>(body, parameter);
                converter = lambda.Compile();

                void forwardBindings(ref List<MemberBinding> memberBindings, ParameterExpression parameterExpression, IList<MappingRule> rules) {
                    foreach (var rule in rules) {
                        Expression valueExp = GetForwardValueExpression(parameterExpression, rule);
                        MemberAssignment bind = Expression.Bind(rule.MapTo, valueExp);
                        memberBindings.Add(bind);
                    }
                }

                void backwardBindings(ref List<MemberBinding> memberBindings, ParameterExpression parameterExpression, IList<MappingRule> rules) {
                    foreach (var rule in rules) {
                        Expression[] valueExpArray = GetBackwardValueExpressions(parameterExpression, rule);
                        for (int i = 0; i < valueExpArray.Length; i++) {
                            var valueExp = valueExpArray[i];
                            var member = rule.MapFrom[i];
                            MemberAssignment bind = Expression.Bind(member, valueExp);
                            memberBindings.Add(bind);
                        }
                    }
                }
            }

            private static Expression GetForwardValueExpression(ParameterExpression parameter, MappingRule rule) {
                var member = rule.MapFrom;
                MemberExpression[] arr = new MemberExpression[member.Length];
                for (int i = 0; i < member.Length; i++) {
                    var name = member[i].Name;
                    arr[i] = Expression.PropertyOrField(parameter, name);
                }
                if (arr.Length == 1) return arr[0];
                else {
                    MethodInfo formatMethod = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) });
                    ConstantExpression formatString = Expression.Constant(rule.Formatter);
                    var unaries = arr.Select(e => Expression.Convert(e, typeof(object)));
                    NewArrayExpression formatArgs = Expression.NewArrayInit(typeof(object), unaries);
                    Expression expression = Expression.Call(null, formatMethod, formatString, formatArgs);
                    return expression;
                }
            }

            private static Expression[] GetBackwardValueExpressions(ParameterExpression parameter, MappingRule rule) {
                throw new NotImplementedException();
            }

            public static Target Map(Source source) {
                return converter.Invoke(source);
            }
        }
    }
}
