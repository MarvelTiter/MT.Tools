using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MT.KitTools.Mapper {
    internal class MapInfo {
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
        public Type SourceElementType { get; set; }
        public Type TargetElementType { get; set; }
        public Expression SourceExpression { get; set; }
        public IList<MappingRule> Rules { get; set; }
    }
}