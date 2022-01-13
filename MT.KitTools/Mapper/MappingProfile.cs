using MT.KitTools.TypeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MT.KitTools.Mapper.ExpressionCore;

namespace MT.KitTools.Mapper
{
    public enum MapperType
    {
        ClassObjectToClassObject,
        ClassObjectToDictionary,
        DictionaryToClassObject
    }
    public partial class MappingProfile<Source, Target> : Profiles
    {

        private Type sourceType;
        private Type targetType;

        private Type sourceElementType => sourceType.IsICollectionType() ? sourceType.GetCollectionElementType() : sourceType;
        private Type targetElementType => targetType.IsICollectionType() ? targetType.GetCollectionElementType() : targetType;

        public override IList<MappingRule> Rules { get; }
        protected Action<Source, Target> MapAction { get; set; }
        protected MapperConfig mapperConfig { get; } = MapperConfigProvider.GetMapperConfig();

        internal MappingProfile()
        {
            sourceType = typeof(Source);
            targetType = typeof(Target);
            Rules = new List<MappingRule>();
            AutoMap();
        }

        public MappingProfile<Source, Target> Mapping(Action<Source, Target> action)
        {
            MapAction = action;
            return this;
        }

        public void AutoMap()
        {
            if (sourceType.IsDictionary() || targetType.IsDictionary())
            {
                return;
            }
            Rules.Clear();
            var targetProps = targetElementType.GetProperties();
            var sourceProps = sourceElementType.GetProperties();
            foreach (var item in targetProps)
            {
                var sourceMember = sourceProps.FirstOrDefault(p => mapperConfig.Match(p, item));
                if (!Rules.Any(r => r.MapTo == item) && sourceMember != null)
                    AddMap(item, sourceMember);
            }
        }

        public override bool Exit(Type source, Type target)
        {
            var b1 = isTypeEquals();
            var b2 = isElementTypeEquals();
            return b1 || b2;

            bool isTypeEquals()
            {
                return ReferenceEquals(source, sourceType) && ReferenceEquals(target, targetType);
            }

            bool isElementTypeEquals()
            {
                var requestSourceType = source.IsICollectionType() ? source.GetCollectionElementType() : source;
                var requestTargetType = target.IsICollectionType() ? target.GetCollectionElementType() : target;
                return ReferenceEquals(requestSourceType, sourceElementType) && ReferenceEquals(requestTargetType, targetElementType);
            }
        }

        private void AddMap(PropertyInfo to, PropertyInfo from, string formatter = null)
        {
            MappingRule rule = new MappingRule(to, from, formatter);
            Rules.Add(rule);
        }

        public override Delegate CreateDelegate()
        {
            MapInfo p = new MapInfo();
            p.SourceType = sourceType;
            p.TargetType = targetType;
            p.SourceElementType = sourceElementType;
            p.TargetElementType = targetElementType;
            p.Rules = Rules;
            var lambda = CreateExpression.ExpressionBuilder(p);
            var del = lambda.Compile() as Func<object, Target>;
            Func<object, Target> newFunc = o =>
            {
                var t = del.Invoke(o);
                MapAction?.Invoke((Source)o, t);
                return t;
            };
            return del;
        }
    }
}