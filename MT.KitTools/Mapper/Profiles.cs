using MT.KitTools.Mapper;
using MT.KitTools.Mapper.ExpressionCore;
using MT.KitTools.TypeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MT.KitTools.Mapper
{
    public abstract class Profiles {

        protected Type[] types;
        protected Type SourceType => types[0];
        protected Type TargetType => types[1];
        protected Type SourceElementType => SourceType.IsICollectionType() ? SourceType.GetCollectionElementType() : SourceType;
        protected Type TargetElementType => TargetType.IsICollectionType() ? TargetType.GetCollectionElementType() : TargetType;
        public abstract bool Exit(Type source, Type target);
        public abstract IList<MappingRule> Rules { get; }
        public abstract Delegate CreateDelegate(ActionType actionType);
    }

    public enum Direction {
        Forward,
        Backward
    }
}
