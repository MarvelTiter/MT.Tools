using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shared.Mapper.Core {
    public class MappingProfile<Source, Target> {

        public MappingProfile<Source, Target> Add(MemberInfo source, params MemberInfo[] target) {
            return this;
        }
    }

    public class MappingRule {

    }
}
