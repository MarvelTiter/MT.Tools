using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared.Mapper.Core {
    public static class ProfileProvider {
        private static IList<Profiles> cache = new List<Profiles>();

        public static void Cache(Profiles profiles, Type sourceType, Type targetType) {
            bool contain = cache.Any(p => p.CheckExit(sourceType, targetType));
            if (contain) {
                throw new ArgumentException($"mapping between {sourceType.Name} and {targetType.Name} had been created");
            }
            cache.Add(profiles);
        }

        public static Profiles GetProfile(Type sourceType, Type targetType) {
            return cache.FirstOrDefault(p => p.CheckExit(sourceType, targetType));
        }
    }
}
