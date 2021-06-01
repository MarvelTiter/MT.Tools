using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shared.Mapper.Core {
    public class MappingRule {
        public MappingRule(MemberInfo[] sources, MemberInfo[] targets) {
            Sources = sources;
            Targets = targets;
        }
        public string Formatter { get; set; }
        public MemberInfo[] Sources { get; set; }
        public MemberInfo[] Targets { get; set; }
    }
}
