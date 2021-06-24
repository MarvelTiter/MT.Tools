using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MT.KitTools.Mapper {
    public abstract class Profiles {
        public abstract bool CheckExit(Type source, Type target);
        public abstract IList<MappingRule> Rules { get; }
        public abstract Direction GetDirection(Type source, Type target);
        public abstract void RunActions(object source, object target);
    }

    public enum Direction {
        Forward,
        Backward
    }
}
