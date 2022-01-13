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
        internal static Expression MapFromDictionary(MapInfo p)
        {
            List<Expression> body = new List<Expression>();
            var genericArgs = p.SourceType.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];
            if (keyType != typeof(string))
            {
                throw new ArgumentException("key type must be string");
            }
            throw new NotImplementedException();
        }
    }
}
