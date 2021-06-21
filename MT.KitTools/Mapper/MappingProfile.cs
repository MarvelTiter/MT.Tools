using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MT.KitTools.Mapper {
    public class MappingProfile<Source, Target> : Profiles {

        private Type sourceType;
        private Type targetType;
        public override IList<MappingRule> Rules { get; }

        internal MappingProfile() {
            sourceType = typeof(Source);
            targetType = typeof(Target);
            Rules = new List<MappingRule>();
        }

        public MappingProfile<Source, Target> Mapping(
            Expression<Func<Target, object>> mapToExp
            , Expression<Func<Source, object>> mapFromExp) {
            MemberInfo to = ResolveMapToMemberInfo(mapToExp.Body);
            MemberInfo[] from = ResolveMapFromMemberInfo(mapFromExp);
            AddMap(to, from);
            return this;
        }

        public MappingProfile<Source, Target> Mapping(
              Expression<Func<Target, object>> mapToExp
            , string formatter
            , params Expression<Func<Source, object>>[] mapFromExp) {
            MemberInfo to = ResolveMapToMemberInfo(mapToExp.Body);
            MemberInfo[] from = ResolveMapFromMemberInfo(mapFromExp);
            AddMap(to, from, formatter);
            return this;
        }

        public void AutoMap() {
            var BindingAttr = BindingFlags.Public | BindingFlags.Instance;
            var members = targetType.GetMembers(BindingAttr)
                .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property);
            foreach (var item in members) {
                var sourceMember = sourceType.GetMember(item.Name, MemberTypes.Field | MemberTypes.Property, BindingAttr);
                if (!Rules.Any(r => r.MapTo == item) && sourceMember.Length > 0)
                    AddMap(item, sourceMember);
            }
        }

        public override bool CheckExit(Type source, Type target) {
            return ReferenceEquals(sourceType, source) && ReferenceEquals(targetType, target) ||
                ReferenceEquals(sourceType, target) && ReferenceEquals(targetType, source);
        }

        private MemberInfo[] ResolveMapFromMemberInfo(params Expression<Func<Source, object>>[] mapFromExp) {
            return mapFromExp.Select(exp => {
                return GetMemberInfo(exp.Body);
            }).ToArray();
        }

        private MemberInfo GetMemberInfo(Expression body) {
            if (body is MemberExpression member) {
                return member.Member;
            } else if (body is UnaryExpression unary) {
                MemberExpression uMember = unary.Operand as MemberExpression;
                return uMember.Member;
            }
            return null;
        }

        private MemberInfo ResolveMapToMemberInfo(Expression body) {
            return GetMemberInfo(body);
        }

        private void AddMap(MemberInfo to, MemberInfo[] from, string formatter = null) {
            MappingRule rule = new MappingRule(to, from, formatter);
            Rules.Add(rule);
        }

        public override Direction GetDirection(Type source, Type target) {
            if (ReferenceEquals(sourceType, source) && ReferenceEquals(targetType, target)) {
                return Direction.Forward;
                ;
            } else if (ReferenceEquals(sourceType, target) && ReferenceEquals(targetType, source)) {
                return Direction.Backward;
            }
            throw new ArgumentException($"TypeError 1. {source.Name} 2. {target.Name}");
        }
    }
}