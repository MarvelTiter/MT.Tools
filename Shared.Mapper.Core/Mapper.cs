using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Shared.Mapper.Core {
    public class Mapper {
        public static Target Map<Source, Target>(Source source) {
            return MapperLink<Source, Target>.Map(source);
        }

        public static void CreateMap<Source, Target>(Action<MappingProfile<Source, Target>> context) {

        }

        private static class MapperLink<Source, Target> {
            private static readonly Func<Source, Target> converter;
            static MapperLink() {
                var flags = BindingFlags.Public | BindingFlags.Instance;
                var parameter = Expression.Parameter(typeof(Source), "source");
                var members = typeof(Target).GetMembers(flags);
                List<MemberBinding> bindings = new List<MemberBinding>();
                foreach (var member in members) {
                    if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field) {
                        continue;
                    }
                    if (GetMappedFieldOrProperty(member, out var sMember)) {
                        MemberExpression valueExp = GetValueExpression(parameter, sMember);
                        MemberAssignment bind = Expression.Bind(member, valueExp);
                        bindings.Add(bind);
                    }
                }
                MemberInitExpression body = Expression.MemberInit(Expression.New(typeof(Target)), bindings);
                Expression<Func<Source, Target>> lambda = Expression.Lambda<Func<Source, Target>>(body, parameter);
                converter = lambda.Compile();
            }

            private static MemberExpression GetValueExpression(ParameterExpression parameter, params MemberInfo[] member) {
                MemberExpression[] arr = new MemberExpression[member.Length];
                for (int i = 0; i < member.Length; i++) {
                    var name = member[i].Name;
                    arr[0] = Expression.PropertyOrField(parameter, name);
                }
                if (arr.Length == 1) return arr[0];
                else return default;
            }

            private static bool GetMappedFieldOrProperty(MemberInfo member, out MemberInfo fieldInfo) {
                var f = typeof(Target).GetField(member.Name);
                var p = typeof(Target).GetProperty(member.Name);
                fieldInfo = null;
                if (f != null) {
                    fieldInfo = f;
                }
                if (p != null) {
                    fieldInfo = p;
                }
                return fieldInfo != null;
            }

            public static Target Map(Source source) {
                return converter.Invoke(source);
            }
        }
    }
}
