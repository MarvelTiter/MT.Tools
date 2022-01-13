using MT.KitTools.ExpressionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MT.KitTools.Mapper
{
    public static class MapperExtensions
    {
        /// <summary>
        /// 为现有的对象赋值
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="self"></param>
        /// <param name="source"></param>
        public static void Map<TTarget, TFrom>(this TTarget self, TFrom source)
        {
            InnerMapper<TTarget,TFrom>.Invoke(self, source);
        }

        private static class InnerMapper<TTarget, TFrom>
        {
            private static readonly Action<TTarget, TFrom> action;
            static InnerMapper()
            {
                Type targetType = typeof(TTarget);
                Type fromType = typeof(TFrom);
                var targetProps = targetType.GetProperties();
                var fromProps = fromType.GetProperties();
                ParameterExpression targetExp = Expression.Parameter(targetType, "tar");
                ParameterExpression fromExp = Expression.Parameter(fromType, "from");
                List<Expression> body = new List<Expression>();
                foreach (var tar in targetProps)
                {
                    if (!tar.CanWrite) continue;
                    var from = fromProps.FirstOrDefault(f => f.Name.ToLower() == tar.Name.ToLower());
                    if (from == null) continue;
                    //MemberExpression tarProp = Expression.Property(targetExp, tar);
                    MemberExpression fromProp = Expression.Property(fromExp, from);
                    var converted = DataTypeConvert.GetConversionExpression(fromProp, tar.PropertyType, from.PropertyType);
                    MethodCallExpression setPropExp = Expression.Call(targetExp, tar.SetMethod, converted);
                    body.Add(setPropExp);
                }
                var lambda = Expression.Lambda(Expression.Block(body), new ParameterExpression[] { targetExp, fromExp });
                action = (Action<TTarget, TFrom>)lambda.Compile();
            }

            public static void Invoke(TTarget source, TFrom target)
            {
                action?.Invoke(source, target);
            }
        }
    }
}
