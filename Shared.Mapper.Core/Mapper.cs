﻿using System;
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

        public static void CreateMap<Source, Target>(Action<MappingProfile<Source, Target>> context = null) {
            var map = new MappingProfile<Source, Target>();
            if (context == null) {
                map.AutoMap();
            } else {
                context.Invoke(map);
            }
            bool contain = cache.Any(p => p.CheckExit(typeof(Source), typeof(Target)));
            if (!contain) {
                cache.Add(map);
            }
        }

        private static class MapperLink<Source, Target> {
            private static readonly Func<Source, Target> converter;
            private static MappingProfile<Source, Target> profile;
            static MapperLink() {
                //
                profile = (MappingProfile<Source, Target>)cache.First(p => p.CheckExit(typeof(Source), typeof(Target)));
                //
                var flags = BindingFlags.Public | BindingFlags.Instance;
                var parameter = Expression.Parameter(typeof(Source), "source");
                var members = typeof(Target).GetMembers(flags)
                    .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property);
                List<MemberBinding> bindings = new List<MemberBinding>();
                foreach (var member in members) {
                    if (GetMappedFieldOrProperty(member, out var rule)) {
                        Expression valueExp = GetValueExpression(parameter, rule);
                        MemberAssignment bind = Expression.Bind(member, valueExp);
                        bindings.Add(bind);
                    }
                }
                MemberInitExpression body = Expression.MemberInit(Expression.New(typeof(Target)), bindings);
                Expression<Func<Source, Target>> lambda = Expression.Lambda<Func<Source, Target>>(body, parameter);
                converter = lambda.Compile();
            }

            private static Expression GetValueExpression(ParameterExpression parameter, MappingRule rule) {
                var member = rule.Targets;
                MemberExpression[] arr = new MemberExpression[member.Length];
                for (int i = 0; i < member.Length; i++) {
                    var name = member[i].Name;
                    arr[0] = Expression.PropertyOrField(parameter, name);
                }
                if (arr.Length == 1) return arr[0];
                else {
                    Expression expression = default;
                    MethodInfo formatMethod = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) });
                    List<Expression> param = new List<Expression>();
                    param.Add(Expression.Constant(rule.Formatter));
                    param.AddRange(arr.Select(Expression.Constant));
                    expression = Expression.Call(null, formatMethod, param);
                    return expression;
                }
            }

            private static bool GetMappedFieldOrProperty(MemberInfo member, out MappingRule rule) {
                rule = profile.GetRule(member);
                return rule != null;
            }

            public static Target Map(Source source) {
                return converter.Invoke(source);
            }
        }
    }
}
