using MT.KitTools.ExpressionHelper;
using MT.KitTools.ReflectionExtension;
using MT.KitTools.TypeExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MT.KitTools.Mapper {

    public static class MapperExtensions {
        public static Target Map<Source, Target>(this Source source) {
            return Mapper.Default.Map<Source, Target>(source);
        }
    }

    public class Mapper {

        public static Mapper Default => new Mapper();

        private MapperConfig Config => new MapperConfig();

        private Mapper() { }

        public void Configuration(Action<MapperConfig> config) {
            config?.Invoke(Config);
        }

        /// <summary>
        /// 配置映射
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <param name="context"></param>
        public void CreateMap<Source, Target>(Action<MappingProfile<Source, Target>> context = null) {
            var map = CreateProfile<Source, Target>();
            context?.Invoke(map);
        }
        /// <summary>
        /// 创建 MappingProfile，并检查是否重复
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        /// <returns></returns>
        private static MappingProfile<Source, Target> CreateProfile<Source, Target>() {
            var map = new MappingProfile<Source, Target>();
            ProfileProvider.Cache(map, typeof(Source), typeof(Target));
            map.AutoMap();
            return map;
        }

        public Target Map<Source, Target>(Source source) {
            return MapperLink<Source, Target>.Map(source);
        }

        /// <summary>
        /// 泛型缓存
        /// </summary>
        /// <typeparam name="Source"></typeparam>
        /// <typeparam name="Target"></typeparam>
        private static class MapperLink<Source, Target> {
            private static readonly Func<object, Target> converter;
            private static Profiles profile = null;
            static MapperLink() {
                // 
                profile = ProfileProvider.GetProfile(typeof(Source), typeof(Target));
                if (profile == null) {
                    profile = CreateProfile<Source, Target>();
                }
                converter = (Func<object, Target>)profile.CreateDelegate();
            }


            public static Target Map(Source source) {
                return converter.Invoke(source);
            }
        }
    }
}
