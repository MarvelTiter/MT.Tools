using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MT.KitTools.Mapper {
    public abstract class Profiles {
        public abstract bool Exit(Type source, Type target);
        public abstract IList<MappingRule> Rules { get; }
        public abstract Delegate CreateDelegate();
    }

    public enum Direction {
        Forward,
        Backward
    }
}
