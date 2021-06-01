using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Shared.Mapper.Core {
    public class MappingProfile<Source, Target> : Profiles {

        private Type sourceType;
        private Type targetType;

        public MappingProfile() {
            sourceType = typeof(Source);
            targetType = typeof(Target);
        }

        public MappingProfile<Source, Target> Mapping(
            Expression<Func<Target, object>> mapToExp
            , Expression<Func<Source, object>> mapFromExp
            , string formatter = null) {

            return this;
        }

        public void AutoMap() {

        }

        public override bool CheckExit(Type source, Type target) {
            return ReferenceEquals(sourceType, source) && ReferenceEquals(targetType, target);
        }

        public override MappingRule GetRule(MemberInfo target) {
            throw new NotImplementedException();
        }
    }
}