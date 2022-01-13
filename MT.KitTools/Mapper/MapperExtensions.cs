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
        /// 为现有的对象赋值
        /// </summary>
        /// <typeparam name="TTarget"></typeparam>
        /// <typeparam name="TFrom"></typeparam>
        /// <param name="self"></param>
        /// <param name="source"></param>
        public static void Map<TFrom, TTarget>(this TTarget self, TFrom source)
        {
            InnerMapper<TFrom, TTarget>.Invoke( source, self);
        }

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
                converter = (Func<object, TTarget>)profile.CreateDelegate(ActionType.NewObj);
            }

            public static TTarget Create(TFrom source)
            {
                return converter.Invoke(source);
            }
        }
        
        internal static class InnerMapper<TFrom, TTarget>
        {
            private static readonly Action<TFrom, TTarget> action;
            private static Profiles profile = null;

            static InnerMapper()
            {               
                Type sourceType = typeof(TFrom);
                Type targetType = typeof(TTarget);
                profile = ProfileProvider.GetProfile(sourceType, targetType);
                if (profile == null)
                {
                    profile = CreateProfile<TFrom, TTarget>();
                }
                action = (Action<TFrom, TTarget>)profile.CreateDelegate(ActionType.Assign);
            }

            public static void Invoke(TFrom source, TTarget target)
            {
                action?.Invoke(source, target);
            }
        }
    }
}
