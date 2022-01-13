using MT.KitTools.ExpressionHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MT.KitTools.Mapper
{
    public static partial class MapperExtensions
    {
        

        /// <summary>
        /// 创建 MappingProfile，并检查是否重复
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        internal static MappingProfile<TFrom, TTarget> CreateProfile<TFrom, TTarget>()
        {
            var map = new MappingProfile<TFrom, TTarget>();
            ProfileProvider.Cache(map, typeof(TFrom), typeof(TTarget));
            return map;
        }

        /// <summary>
        /// 泛型缓存
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        internal static class MapperLink<TFrom, TTarget>
        {
            private static readonly Func<object, TTarget> converter;
            private static Profiles profile = null;
            static MapperLink()
            {
                // 
                Type sourceType = typeof(TFrom);
                Type targetType = typeof(TTarget);
                profile = ProfileProvider.GetProfile(sourceType, targetType);
                if (profile == null)
                {
                    profile = CreateProfile<TFrom, TTarget>();
                }
                converter = (Func<object, TTarget>)profile.CreateDelegate();
            }

            public static TTarget Create(TFrom source)
            {
                return converter.Invoke(source);
            }
        }
        /// <summary>
        /// 为现有的对象赋值
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="self"></param>
        /// <param name="source"></param>
        public static void Map<TTarget, TFrom>(this TTarget self, TFrom source)
        {

        }

        private static class InnerMapper<TTarget, TFrom>
        {
            private static readonly Action<TTarget, TFrom> action;
            private static Profiles profile = null;

            static InnerMapper()
            {
                //Type targetType = typeof(TTarget);
                //Type fromType = typeof(TFrom);
                //var targetProps = targetType.GetProperties();
                //var fromProps = fromType.GetProperties();
                //ParameterExpression targetExp = Expression.Parameter(targetType, "tar");
                //ParameterExpression fromExp = Expression.Parameter(fromType, "from");
                //List<Expression> body = new List<Expression>();
                //foreach (var tar in targetProps)
                //{
                //    if (!tar.CanWrite) continue;
                //    var from = fromProps.FirstOrDefault(f => f.Name.ToLower() == tar.Name.ToLower());
                //    if (from == null) continue;
                //    //MemberExpression tarProp = Expression.Property(targetExp, tar);
                //    MemberExpression fromProp = Expression.Property(fromExp, from);
                //    var converted = DataTypeConvert.GetConversionExpression(fromProp, tar.PropertyType, from.PropertyType);
                //    MethodCallExpression setPropExp = Expression.Call(targetExp, tar.SetMethod, converted);
                //    body.Add(setPropExp);
                //}
                //var lambda = Expression.Lambda(Expression.Block(body), new ParameterExpression[] { targetExp, fromExp });
                //action = (Action<TTarget, TFrom>)lambda.Compile();

                Type sourceType = typeof(TFrom);
                Type targetType = typeof(TTarget);
                profile = ProfileProvider.GetProfile(sourceType, targetType);
                if (profile == null)
                {
                    profile = CreateProfile<TFrom, TTarget>();
                }
                action = (Action<TTarget, TFrom>)profile.CreateDelegate();
            }

            public static void Invoke(TTarget source, TFrom target)
            {
                action?.Invoke(source, target);
            }
        }
    }
}
