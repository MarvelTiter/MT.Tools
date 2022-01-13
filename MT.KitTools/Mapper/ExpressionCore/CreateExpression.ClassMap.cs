using MT.KitTools.ExpressionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MT.KitTools.Mapper.ExpressionCore
{
    internal partial class CreateExpression
    {
        internal static Expression ClassMap(MapInfo p)
        {
            var source = p.SourceExpression as ParameterExpression;
            List<MemberBinding> bindings = new List<MemberBinding>();
            initBindings(ref bindings, source, p.Rules);
            MemberInitExpression body = Expression.MemberInit(Expression.New(p.TargetElementType), bindings);
            return body;
        }

        internal static MemberInitExpression ClassMap(List<MappingRule> rules, Type sourceType, Type targetType, object source)
        {
            ParameterExpression parameterExpression = Expression.Variable(sourceType, "source");
            Expression.Assign(parameterExpression, Expression.Constant(source, sourceType));
            List<MemberBinding> bindings = new List<MemberBinding>();
            initBindings(ref bindings, parameterExpression, rules);
            return Expression.MemberInit(Expression.New(targetType), bindings);
        }

        private static void initBindings(ref List<MemberBinding> memberBindings, ParameterExpression parameterExpression, IList<MappingRule> rules)
        {
            foreach (var rule in rules)
            {
                Expression valueExp = GetValueExpression(parameterExpression, rule);
                MemberAssignment bind = Expression.Bind(rule.MapTo, valueExp);
                memberBindings.Add(bind);
            }
        }
        private static Expression GetValueExpression(ParameterExpression parameter, MappingRule rule)
        {
            var prop = rule.MapFrom;
            var bind = Expression.Property(parameter, prop);
            Expression convertedBind = DataTypeConvert.GetConversionExpression(bind, rule.MapFrom.PropertyType, rule.MapTo.PropertyType);
            return convertedBind;
        }
    }
}
