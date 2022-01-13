using System;
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
        internal static Expression MapToDictionary(MapInfo p)
        {
            List<Expression> body = new List<Expression>();
            var genericArgs = p.TargetType.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];
            if (keyType != typeof(string))
            {
                throw new ArgumentException("key type must be string");
            }
            var dicType = typeof(Dictionary<,>).MakeGenericType(genericArgs);
            MethodInfo addMethod = dicType.GetMethod("Add", genericArgs);
            var props = p.SourceType.GetProperties();
            // var dic = new Dictionary<string, object>();
            ParameterExpression dicExpression = Expression.Variable(dicType, "dic");
            //body.Add(dicExpression);
            body.Add(Expression.Assign(dicExpression, Expression.New(dicType)));
            // dic.Add();
            foreach (PropertyInfo property in props)
            {
                if (!property.CanRead) continue;
                if (valueType == typeof(object) || valueType == property.PropertyType)
                {
                    var key = Expression.Constant(property.Name, keyType);
                    var value = Expression.Property(p.SourceExpression, property);
                    MethodCallExpression callAdd = Expression.Call(dicExpression, addMethod, key, Expression.Convert(value, valueType));
                    body.Add(callAdd);
                }
            }
            // return dic;
            body.Add(Expression.Convert(dicExpression, p.TargetType));
            var block = Expression.Block(new[] { dicExpression }, body);
            return block;
        }
    }
}
