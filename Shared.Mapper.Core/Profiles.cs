using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Shared.Mapper.Core {
    public abstract class Profiles {
        public abstract bool CheckExit(Type source, Type target);
        public abstract IList<MappingRule> Rules { get; }
    }
}
