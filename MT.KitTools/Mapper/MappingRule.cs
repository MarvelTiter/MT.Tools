using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MT.KitTools.Mapper {
    public class MappingRule {
        public MappingRule(PropertyInfo mapTo, PropertyInfo mapFrom, string formatter) {
            MapTo = mapTo;
            MapFrom = mapFrom;
            Formatter = formatter;
        }
        public int Index { get; set; }
        public string Formatter { get; set; }
        public PropertyInfo MapTo { get; set; }
        public PropertyInfo MapFrom { get; set; }
    }
}
