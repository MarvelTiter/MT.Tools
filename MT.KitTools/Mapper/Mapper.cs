using MT.KitTools.ExpressionHelper;
using MT.KitTools.ReflectionExtension;
using MT.KitTools.TypeExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MT.KitTools.Mapper
{
    public class Mapper
    {
        public static TTarget Map<TFrom, TTarget>(TFrom source)
        {
            return Default.InnerMap<TFrom, TTarget>(source);
        }
        public static IEnumerable<TTarget> Map<TFrom, TTarget>(IEnumerable<TFrom> sources)
        {
            foreach (var item in sources)
            {
                yield return Map<TFrom, TTarget>(item);
            }
        }
        public static Mapper Default => new Mapper();

        private MapperConfig Config => new MapperConfig();

        private Mapper() { }

        public void Configuration(Action<MapperConfig> config)
        {
            config?.Invoke(Config);
        }

        /// <summary>
        /// 配置映射
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="context"></param>
        public void CreateMap<TFrom, TTarget>(Action<MappingProfile<TFrom, TTarget>> context = null)
        {
            var map = CreateProfile<TFrom, TTarget>();
            context?.Invoke(map);
        }
        /// <summary>
        /// 创建 MappingProfile，并检查是否重复
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        private static MappingProfile<TFrom, TTarget> CreateProfile<TFrom, TTarget>()
        {
            var map = new MappingProfile<TFrom, TTarget>();
            ProfileProvider.Cache(map, typeof(TFrom), typeof(TTarget));
            return map;
        }

        public TTarget InnerMap<TFrom, TTarget>(TFrom source)
        {
            return MapperLink<TFrom, TTarget>.Map(source);
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

            public static TTarget Map(TFrom source)
            {
                return converter.Invoke(source);
            }
        }
    }
}
