using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MT.KitTools.Mapper {
    public class MappingProfile<Source, Target> : Profiles {

        private Type sourceType;
        private Type targetType;

        private Type sourceElementType;
        private Type targetElementType;

        private bool isCollection;

        public override IList<MappingRule> Rules { get; }

        private IList<Action<Source, Target>> actions;
        private readonly MapperConfig mapperConfig;

        internal MappingProfile(MapperConfig mapperConfig) {
            sourceType = typeof(Source);
            targetType = typeof(Target);
            Rules = new List<MappingRule>();
            actions = new List<Action<Source, Target>>();
            this.mapperConfig = mapperConfig;
        }

        public MappingProfile<Source, Target> Mapping(Action<Source, Target> action) {
            actions.Add(action);
            return this;
        }

        public void AutoMap() {
            var BindingAttr = BindingFlags.Public | BindingFlags.Instance;
            var targetProps = targetType.GetProperties(BindingAttr);
            var sourceProps = sourceType.GetProperties(BindingAttr);
            foreach (var item in targetProps) {
                var sourceMember = sourceProps.FirstOrDefault(p => mapperConfig.Match(p, item));
                if (!Rules.Any(r => r.MapTo == item) && sourceMember != null)
                    AddMap(item, sourceMember);
            }
        }

        public override bool CheckExit(Type source, Type target) {
            return ReferenceEquals(sourceType, source) && ReferenceEquals(targetType, target) ||
                ReferenceEquals(sourceType, target) && ReferenceEquals(targetType, source);
        }

        private void AddMap(PropertyInfo to, PropertyInfo from, string formatter = null) {
            MappingRule rule = new MappingRule(to, from, formatter);
            Rules.Add(rule);
        }

        public override Direction GetDirection(Type source, Type target) {
            if (ReferenceEquals(sourceType, source) && ReferenceEquals(targetType, target)) {
                return Direction.Forward;
                ;
            } else if (ReferenceEquals(sourceType, target) && ReferenceEquals(targetType, source)) {
                return Direction.Backward;
            }
            throw new ArgumentException($"TypeError 1. {source.Name} 2. {target.Name}");
        }

        public override void RunActions(object source, object target) {
            var s = (Source)source;
            var t = (Target)target;
            foreach (var item in actions) {
                item.Invoke(s, t);
            }
        }
    }
}