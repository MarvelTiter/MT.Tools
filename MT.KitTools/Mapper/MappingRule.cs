using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MT.KitTools.Mapper {
    public class MappingRule {
        public MappingRule(MemberInfo mapTo, MemberInfo[] mapFrom, string formatter) {
            MapTo = mapTo;
            MapFrom = mapFrom;
            Formatter = formatter;
        }
        public int Index { get; set; }
        public string Formatter { get; set; }
        public MemberInfo MapTo { get; set; }
        public MemberInfo[] MapFrom { get; set; }
    }
}
