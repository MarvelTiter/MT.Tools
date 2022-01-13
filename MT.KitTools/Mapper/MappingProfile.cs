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
    public partial class MappingProfile<TFrom, TTarget> : Profiles
    {

        private Type sourceType;
        private Type targetType;

        private Type SourceElementType => sourceType.IsICollectionType() ? sourceType.GetCollectionElementType() : sourceType;
        private Type TargetElementType => targetType.IsICollectionType() ? targetType.GetCollectionElementType() : targetType;

        public override IList<MappingRule> Rules { get; }
        protected Action<TFrom, TTarget> MapAction { get; set; }
        protected MapperConfig MapperConfig { get; } = MapperConfigProvider.GetMapperConfig();

        internal MappingProfile()
        {
            sourceType = typeof(TFrom);
            targetType = typeof(TTarget);
            Rules = new List<MappingRule>();
            AutoMap();
        }

        public MappingProfile<TFrom, TTarget> Mapping(Action<TFrom, TTarget> action)
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
            var targetProps = TargetElementType.GetProperties();
            var sourceProps = SourceElementType.GetProperties();
            foreach (var item in targetProps)
            {
                var sourceMember = sourceProps.FirstOrDefault(p => MapperConfig.Match(p, item));
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
                return ReferenceEquals(requestSourceType, SourceElementType) && ReferenceEquals(requestTargetType, TargetElementType);
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
            p.SourceElementType = SourceElementType;
            p.TargetElementType = TargetElementType;
            p.Rules = Rules;
            var lambda = CreateExpression.ExpressionBuilder(p);
            var del = lambda.Compile() as Func<object, TTarget>;
            Func<object, TTarget> newFunc = o =>
            {
                var t = del.Invoke(o);
                MapAction?.Invoke((TFrom)o, t);
                return t;
            };
            return newFunc;
        }
    }
}