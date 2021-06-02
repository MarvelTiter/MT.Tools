using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Shared.Mapper.Core {
    public static class Mapper {

        private static IList<Profiles> cache = new List<Profiles>();

        public static Target Map<Source, Target>(Source source) {
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
        private static MappingProfile<Source, Target> internalCreate<Source, Target>() {
            var map = new MappingProfile<Source, Target>();
            bool contain = cache.Any(p => p.CheckExit(typeof(Source), typeof(Target)));
            if (contain) {
                throw new ArgumentException($"mapping between {typeof(Source).Name} and {typeof(Target).Name} had been created");
            }
            cache.Add(map);
            return map;
        }

        /// <summary>
        /// 泛型缓存
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        private static class MapperLink<Source, Target> {
            private static readonly Func<Source, Target> converter;
            private static MappingProfile<Source, Target> profile;
            static MapperLink() {
                //
                profile = (MappingProfile<Source, Target>)cache.FirstOrDefault(p => p.CheckExit(typeof(Source), typeof(Target)));
                if (profile == null) {
                    profile = internalCreate<Source, Target>();
                    profile.AutoMap();
                }
                //
                var parameter = Expression.Parameter(typeof(Source), "source");
                // get all field and property

                List<MemberBinding> bindings = new List<MemberBinding>();
                foreach (var rule in profile.Rules) {
                    Expression valueExp = GetValueExpression(parameter, rule);
                    MemberAssignment bind = Expression.Bind(rule.MapTo, valueExp);
                    bindings.Add(bind);
                }

                MemberInitExpression body = Expression.MemberInit(Expression.New(typeof(Target)), bindings);
                Expression<Func<Source, Target>> lambda = Expression.Lambda<Func<Source, Target>>(body, parameter);
                converter = lambda.Compile();
            }

            private static Expression GetValueExpression(ParameterExpression parameter, MappingRule rule) {
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

            public static Target Map(Source source) {
                return converter.Invoke(source);
            }
        }
    }
}
