using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MT.KitTools.Mapper {
    public static class ProfileProvider {
        private static IList<Profiles> cache = new List<Profiles>();

        public static void Cache(Profiles profiles, Type sourceType, Type targetType) {
            bool contain = cache.Any(p => p.CheckExit(sourceType, targetType));
            if (!contain) {
                cache.Add(profiles);
                //throw new ArgumentException($"mapping between {sourceType.Name} and {targetType.Name} had been created");
            }
        }

        public static Profiles GetProfile(Type sourceType, Type targetType) {
            var profile = cache.FirstOrDefault(p => p.CheckExit(sourceType, targetType));
            return profile;
        }
    }
}
