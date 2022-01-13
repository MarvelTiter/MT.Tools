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
    public class MappingProfile<TFrom, TTarget> : Profiles
    {
        public override IList<MappingRule> Rules { get; }
        protected Action<TFrom, TTarget> MapAction { get; set; }
        protected MapperConfig MapperConfig { get; } = MapperConfigProvider.GetMapperConfig();
        internal MappingProfile()
        {
            types = new[] { typeof(TFrom), typeof(TTarget) };
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
            if (SourceType.IsDictionary() || TargetType.IsDictionary())
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
                return ReferenceEquals(source, SourceType) && ReferenceEquals(target, TargetType);
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

        public override Delegate CreateDelegate(ActionType actionType)
        {
            MapInfo p = new MapInfo();
            p.SourceType = SourceType;
            p.TargetType = TargetType;
            p.SourceElementType = SourceElementType;
            p.TargetElementType = TargetElementType;
            p.Rules = Rules;
            p.ActionType = actionType;
            var lambda = CreateExpression.ExpressionBuilder(p);
            if (actionType == ActionType.NewObj)
            {
                var del = lambda.Compile() as Func<object, TTarget>;
                Func<object, TTarget> newFunc = o =>
                {
                    var t = del.Invoke(o);
                    MapAction?.Invoke((TFrom)o, t);
                    return t;
                };
                return newFunc;
            }
            else
            {
                var del = lambda.Compile() as Action<TFrom, TTarget>;
                Action<TFrom, TTarget> newAction = (f, t) =>
                 {
                     del.Invoke(f, t);
                     MapAction?.Invoke(f, t);
                 };
                return del;
            }
        }
    }
}