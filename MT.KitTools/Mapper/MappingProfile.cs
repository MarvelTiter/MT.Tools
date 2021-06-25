using MT.KitTools.TypeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MT.KitTools.Mapper {
    public enum MapperType {
        ClassObjectToClassObject,
        ClassObjectToDictionary,
        DictionaryToClassObject
    }
    public class MappingProfile<Source, Target> : Profiles {

        private Type sourceType;
        private Type targetType;

        private Type sourceElementType;
        private Type targetElementType;

        public override IList<MappingRule> Rules { get; }
        protected Action<Source, Target> MapAction { get; set; }
        protected MapperConfig mapperConfig { get; } = MapperConfigProvider.GetMapperConfig();

        internal MappingProfile() {
            sourceType = typeof(Source);
            targetType = typeof(Target);
            Rules = new List<MappingRule>();
            sourceElementType = sourceType.IsICollectionType() ? sourceType.GetCollectionElementType() : null;
            targetElementType = targetType.IsICollectionType() ? targetType.GetCollectionElementType() : null;
        }

        public MappingProfile<Source, Target> Mapping(Action<Source, Target> action) {
            MapAction = action;
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
            var b1 = ReferenceEquals(source, sourceType) && ReferenceEquals(target, targetType);
            var b2 = ReferenceEquals(source, sourceType) && target.IsDictionary();
            var b3 = source.IsDictionary() && ReferenceEquals(target, targetType);
            return b1 || b2 || b3;
        }

        private void AddMap(PropertyInfo to, PropertyInfo from, string formatter = null) {
            MappingRule rule = new MappingRule(to, from, formatter);
            Rules.Add(rule);
        }

        public override Delegate CreateDelegate() {
            var del = ExpressionBuilder().Compile() as Func<object, Target>;
            Func<object, Target> newFunc = o => {
                var t = del.Invoke(o);
                MapAction?.Invoke((Source)o, t);
                return t;
            };
            return newFunc;
        }
        private LambdaExpression ExpressionBuilder() {
            var sourceParameter = Expression.Parameter(typeof(object), "sourceParameter");
            var source = Expression.Variable(sourceType, "source");
            var body = new List<Expression>();
            if (sourceType.IsValueType) {
                body.Add(Expression.Assign(source, Expression.Unbox(sourceParameter, sourceType)));
            } else {
                body.Add(Expression.Assign(source, Expression.TypeAs(sourceParameter, sourceType)));
            }
            var func = GetHandler();
            var expressions = func.Invoke(source);
            body.AddRange(expressions);
            BlockExpression block = Expression.Block(new[] { source }, body);
            LambdaExpression lambda = Expression.Lambda(block, sourceParameter);
            return lambda;
        }

        private Func<ParameterExpression, IEnumerable<Expression>> GetHandler() {
            if (sourceType.IsDictionary())
                return MapFromDictionary;
            else if (targetType.IsDictionary())
                return MapToDictionary;
            else if (sourceType.IsClass && targetType.IsClass)
                return ClassMap;
            else if (sourceType.IsICollectionType() && targetType.IsICollectionType())
                return CollectionMap;
            throw new NotImplementedException($"not implement map between {sourceType.Name} and {targetType.Name}");
        }

        private Expression[] MapFromDictionary(ParameterExpression source) {
            return null;
        }

        private Expression[] MapToDictionary(ParameterExpression source) {
            return null;
        }

        private Expression[] ClassMap(ParameterExpression source) {
            List<MemberBinding> bindings = new List<MemberBinding>();
            initBindings(ref bindings, source, Rules);

            MemberInitExpression body = Expression.MemberInit(Expression.New(targetType), bindings);
            return new[] { body };
            // 内部方法
            void initBindings(ref List<MemberBinding> memberBindings, ParameterExpression parameterExpression, IList<MappingRule> rules) {
                foreach (var rule in rules) {
                    Expression valueExp = GetValueExpression(parameterExpression, rule);
                    MemberAssignment bind = Expression.Bind(rule.MapTo, valueExp);
                    memberBindings.Add(bind);
                }
            }
            Expression GetValueExpression(ParameterExpression parameter, MappingRule rule) {
                var prop = rule.MapFrom;
                return Expression.Property(parameter, prop);
            }
        }

        private Expression[] CollectionMap(ParameterExpression source) {
            return null;
        }

    }
}