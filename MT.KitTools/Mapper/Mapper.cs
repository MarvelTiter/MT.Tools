using MT.KitTools.ExpressionHelper;
using MT.KitTools.ReflectionExtension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MT.KitTools.Mapper {
    public static class Mapper {

        public static Target Map<Source, Target>(Source source) {
            return MapperLink<Source, Target>.Map(source);
        }
        private static MapperConfig Config => new MapperConfig();

        public static void Configuration(Action<MapperConfig> config) {
            config?.Invoke(Config);
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
        }
        /// <summary>
        /// 创建 MappingProfile，并检查是否重复
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <returns></returns>
        private static MappingProfile<Source, Target> internalCreate<Source, Target>() {
            var map = new MappingProfile<Source, Target>(Config);
            ProfileProvider.Cache(map, typeof(Source), typeof(Target));
            map.AutoMap();
            return map;
        }

        /// <summary>
        /// 泛型缓存
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        private static class MapperLink<Source, Target> {
            private static readonly Func<Source, Target> converter;
            private static Profiles profile = null;
            static MapperLink() {
                // 
                Type sourceType = typeof(Source);
                Type targetType = typeof(Target);

                profile = ProfileProvider.GetProfile(sourceType, targetType);
                if (profile == null) {
                    profile = internalCreate<Source, Target>();
                }
                //
                var parameter = Expression.Parameter(sourceType, "source");
                // get all field and property

                List<MemberBinding> bindings = new List<MemberBinding>();
                //var direction = profile.GetDirection(sourceType, targetType);
                //if (direction == Direction.Forward)
                forwardBindings(ref bindings, parameter, profile.Rules);
                //else
                //    backwardBindings(ref bindings, parameter, profile.Rules);

                MemberInitExpression body = Expression.MemberInit(Expression.New(targetType), bindings);
                Expression<Func<Source, Target>> lambda = Expression.Lambda<Func<Source, Target>>(body, parameter);
                converter = lambda.Compile();
                
                // 内部方法
                void forwardBindings(ref List<MemberBinding> memberBindings, ParameterExpression parameterExpression, IList<MappingRule> rules) {
                    foreach (var rule in rules) {
                        Expression valueExp = GetForwardValueExpression(parameterExpression, rule);
                        MemberAssignment bind = Expression.Bind(rule.MapTo, valueExp);
                        memberBindings.Add(bind);
                    }
                }

                //void backwardBindings(ref List<MemberBinding> memberBindings, ParameterExpression parameterExpression, IList<MappingRule> rules) {
                //    foreach (var rule in rules) {
                //        Expression[] valueExpArray = GetBackwardValueExpressions(parameterExpression, rule);
                //        for (int i = 0; i < valueExpArray.Length; i++) {
                //            var valueExp = valueExpArray[i];
                //            var member = rule.MapFrom[i];
                //            MemberAssignment bind = Expression.Bind(member, valueExp);
                //            memberBindings.Add(bind);
                //        }
                //    }
                //}
            }

            private static Expression GetForwardValueExpression(ParameterExpression parameter, MappingRule rule) {
                var member = rule.MapFrom;
                return Expression.Property(parameter, member);
                //MemberExpression[] arr = new MemberExpression[member.Length];
                //for (int i = 0; i < member.Length; i++) {
                //    var name = member[i].Name;
                //    arr[i] = Expression.PropertyOrField(parameter, name);
                //}
                //if (arr.Length == 1) return arr[0];
                //else {
                //    MethodInfo formatMethod = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) });
                //    ConstantExpression formatString = Expression.Constant(rule.Formatter);
                //    var unaries = arr.Select(e => Expression.Convert(e, typeof(object)));
                //    NewArrayExpression formatArgs = Expression.NewArrayInit(typeof(object), unaries);
                //    Expression expression = Expression.Call(null, formatMethod, formatString, formatArgs);
                //    return expression;
                //}
            }

            //private static Expression[] GetBackwardValueExpressions(ParameterExpression parameter, MappingRule rule) {
            //    var value = rule.MapTo;
            //    // 单映射
            //    if (rule.MapFrom.Length == 1 && rule.Formatter == null) {
            //        MemberExpression memberExpression = Expression.PropertyOrField(parameter, rule.MapFrom[0].Name);
            //        return new[] { memberExpression };
            //    }
            //    // 多映射
            //    var pattern = Regex.Replace(rule.Formatter, @"{\d*}", "(.*)");
            //    /*
            //     * var match = Regex.Match(source.XXX, pattern)
            //     * 
            //     */
            //    var m = Regex.Match("", "");
            //    var matchesMethod = typeof(Regex).GetMethod("Match", new Type[] { typeof(string), typeof(string) });
            //    MethodCallExpression matched = Expression.Call(matchesMethod, Expression.PropertyOrField(parameter, rule.MapTo.Name), Expression.Constant(pattern));

            //    MethodInfo getGroup = typeof(GroupCollection).GetMethod("get_Item", new Type[] { typeof(int) });
            //    var groups = Expression.Property(matched, "Groups");
            //    Expression[] result = new Expression[rule.MapFrom.Length];
            //    for (int i = 0; i < rule.MapFrom.Length; i++) {
            //        var grp = Expression.Call(groups, getGroup, Expression.Constant(i + 1));
            //        MemberExpression valueExp = Expression.Property(grp, "Value");
            //        var memberType = GetMemberDataType(rule.MapFrom[i]);
            //        result[i] = DataTypeConvert.GetConversionExpression(valueExp, typeof(string), memberType);
            //    }
            //    return result;

            //    Type GetMemberDataType(MemberInfo member) {
            //        switch (member.MemberType) {
            //            case MemberTypes.Field:
            //                return ((FieldInfo)member).FieldType;
            //            case MemberTypes.Property:
            //                return ((PropertyInfo)member).PropertyType;
            //            case MemberTypes.Method:
            //            case MemberTypes.TypeInfo:
            //            case MemberTypes.Custom:
            //            case MemberTypes.NestedType:
            //            case MemberTypes.All:
            //            case MemberTypes.Constructor:
            //            case MemberTypes.Event:
            //            default:
            //                throw new ArgumentException();
            //        }
            //    }
            //}

            public static Target Map(Source source) {
                var temp = converter.Invoke(source);
                profile.RunActions(source, temp);
                return temp;
            }
        }

        private static class MapperLinkArray<Source, Target>
            where Source : IEnumerable
            where Target : IEnumerable {
            private static readonly Func<Source, Target> converter;
            private static MethodInfo get_Enumerator = typeof(IEnumerable).GetMethod("GetEnumerator");
            private static MethodInfo method_MoveNext = typeof(IEnumerator).GetMethod("MoveNext");
            private static PropertyInfo prop_Current = typeof(IEnumerator).GetProperty("Current");
            private static Type GetElementType(Type type) {
                if (type.IsArray) {
                    return type.GetElementType();
                } else if (type.IsGenericType) {
                    return type.GenericTypeArguments[0];
                } else {
                    throw new ArgumentException($"unknow type {type.FullName}");
                }
            }

            static MapperLinkArray() {
                Type source = GetElementType(typeof(Source));
                Type target = GetElementType(typeof(Target));
                Type mapperLink = typeof(MapperLink<,>).MakeGenericType(source, target);
                mapperLink.Invoke(target, "Map");
                /*
                 * var enumerator = source.GetEnumerator();
                 * var list = new List<targetType>();
                 * L1;
                 * if (enumerator.MoveNext()) {
                 *     var p = (sourceType)enumerator.Current
                 *     (targetType)mapperLink.Invoke(target, "Map", p);
                 *     goto L1;
                 * }
                 */

            }

            public static Target Map(Source source) {
                return converter.Invoke(source);
            }
        }
    }
}
