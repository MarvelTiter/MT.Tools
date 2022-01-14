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
            return Mapper.Default.NewMap<TFrom, TTarget>(source);
        }
        public static IEnumerable<TTarget> Map<TFrom, TTarget>(IEnumerable<TFrom> sources)
        {
            foreach (var item in sources)
            {
                yield return Map<TFrom, TTarget>(item);
            }
        }
        /// <summary>
        /// 创建 MappingProfile，并检查是否重复
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <returns></returns>
        //internal static MapperRule<TFrom, TTarget> CreateMappingRule<TFrom, TTarget>()
        //{
        //    var rule = new MapperRule<TFrom, TTarget>();
        //    MapRuleProvider.Cache(rule, typeof(TFrom), typeof(TTarget));
        //    return rule;
        //}
        public static Mapper Default => new Mapper();

        private MapperConfig Config => MapperConfigProvider.GetMapperConfig();
        private Mapper() { }

        public Mapper Configuration(Action<MapperConfig> config)
        {
            config?.Invoke(Config);
            return this;
        }

        /// <summary>
        /// 配置映射
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="context"></param>
        public Mapper CreateMap<TFrom, TTarget>(Action<MapperRule<TFrom, TTarget>> context = null)
        {
            var map =  (MapperRule<TFrom, TTarget>)MapRuleProvider.GetMapRule<TFrom, TTarget>();
            context?.Invoke(map);
            return this;
        }
        public TTarget NewMap<TFrom, TTarget>(TFrom source)
        {
            return MapperExtensions.MapperLink<TFrom, TTarget>.Create(source);
        }
    }
}
