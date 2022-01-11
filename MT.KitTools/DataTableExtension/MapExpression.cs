using MT.KitTools.ExpressionHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MT.KitTools.DataTableExtension
{
    public static class MapExpression<T>
    {
        static Dictionary<string, Action<T, DataRow>> actions = new Dictionary<string, Action<T, DataRow>>();
        private static MethodInfo Datarow_getItem = typeof(DataRow).GetMethod("get_Item", new Type[] { typeof(string) });
        private static MethodInfo DataRow_IsNull = typeof(DataRow).GetMethod("IsNull", new Type[] { typeof(string) });
        public static Action<T, DataRow> Build(DataColumnCollection cols)
        {
            var columnNames = from col in cols.Cast<DataColumn>()
                              select col.ColumnName;
            var key = string.Join("_", columnNames);
            if(actions.TryGetValue(key, out var action))
            {
                return action;
            }
            ParameterExpression rowExp = Expression.Parameter(typeof(DataRow), "row");
            ParameterExpression tarExp = Expression.Parameter(typeof(T), "tar");
            var tarType = typeof(T);
            List<Expression> body = new List<Expression>();
            // properties
            var props = tarType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (DataColumn col in cols)
            {
                var prop = props.FirstOrDefault(p => p.Name.ToLower() == col.ColumnName.ToLower());
                if (prop != null && prop.CanWrite)
                {
                    var valueExp = GetTargetValueExpression(col, rowExp, prop.PropertyType);
                    MethodCallExpression propAssign = Expression.Call(tarExp, prop.SetMethod, valueExp);
                    body.Add(propAssign);
                }
            }
            var block = Expression.Block(body);
            var lambda = Expression.Lambda<Action<T, DataRow>>(block, tarExp, rowExp);
            action = lambda.Compile();
            actions.Add(key, action);
            return action;
        }

        private static Expression GetTargetValueExpression(DataColumn column, ParameterExpression parameterExpression, Type targetType)
        {
            MethodCallExpression rowObjExp = Expression.Call(parameterExpression, Datarow_getItem, Expression.Constant(column.ColumnName));
            MethodCallExpression checkNullExp = Expression.Call(parameterExpression, DataRow_IsNull, Expression.Constant(column.ColumnName));
            Expression realValueExp = DataTypeConvert.GetConversionExpression(rowObjExp, column.DataType, targetType);
            if (column.AllowDBNull)
            {
                return Expression.Condition(
                    checkNullExp,
                    Expression.Default(targetType),
                    realValueExp,
                    targetType
                    );
            }
            else
            {
                return realValueExp;
            }
        }
    }
}
