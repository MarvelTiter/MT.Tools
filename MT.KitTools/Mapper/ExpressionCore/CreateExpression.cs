using MT.KitTools.ExpressionHelper;
using MT.KitTools.TypeExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MT.KitTools.Mapper.ExpressionCore
{    
    internal partial class CreateExpression
    {
        internal static LambdaExpression ExpressionBuilder(MapInfo p)
        {
            var sourceParameter = Expression.Parameter(typeof(object), "sourceParameter");
            p.SourceExpression = Expression.Variable(p.SourceType, "source");
            var body = new List<Expression>();
            if (p.SourceType.IsValueType)
            {
                body.Add(Expression.Assign(p.SourceExpression, Expression.Unbox(sourceParameter, p.SourceType)));
            }
            else
            {
                body.Add(Expression.Assign(p.SourceExpression, Expression.TypeAs(sourceParameter, p.SourceType)));
            }
            var func = GetHandler(p);
            var expression = func.Invoke(p);
            body.Add(expression);
            BlockExpression block = Expression.Block(new[] { p.SourceExpression as ParameterExpression }, body);
            LambdaExpression lambda = Expression.Lambda(block, sourceParameter);
            return lambda;
        }
        internal static Func<MapInfo, Expression> GetHandler(MapInfo p)
        {
            var sourceType = p.SourceType;
            var targetType = p.TargetType;
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
    }

}
