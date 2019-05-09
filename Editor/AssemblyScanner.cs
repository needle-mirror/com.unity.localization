using System;
using System.Collections.Generic;

#if !UNITY_2019_2_OR_NEWER
using System.Linq;
#endif

namespace UnityEditor.Localization
{
    class AssemblyScanner
    {
        class AssemblyScannerCache
        {
            public List<Type> types = new List<Type>();
            public List<string> names = new List<string>();
        }
        static Dictionary<Type, AssemblyScannerCache> s_Cache = new Dictionary<Type, AssemblyScannerCache>();

        static void AddFoundTypesToCache(AssemblyScannerCache cache, IList<Type> foundTypes)
        {
            foreach (var type in foundTypes)
            {
                if (type.IsGenericType || type.IsAbstract)
                    continue;

                cache.names.Add(ObjectNames.NicifyVariableName(type.Name));
                cache.types.Add(type);
            }
        }

        internal static void FindSubclasses<T>(List<Type> types, List<string> names = null)
        {
            var baseType = typeof(T);

            if (!s_Cache.TryGetValue(baseType, out var cache))
            {
                cache = new AssemblyScannerCache();
                s_Cache[baseType] = cache;

                #if UNITY_2019_2_OR_NEWER
                var foundTypes = TypeCache.GetTypesDerivedFrom<T>();
                AddFoundTypesToCache(cache, foundTypes);
                #else
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var foundTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
                    AddFoundTypesToCache(cache, foundTypes.ToList());
                }
                #endif
            }

            types.AddRange(cache.types);

            names?.AddRange(cache.names);
        }
    }
}