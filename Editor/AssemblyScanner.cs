using System;
using System.Collections.Generic;
using System.Linq;

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

        internal static void FindSubclasses<T>(List<Type> types, List<string> names = null, bool nicifyNames = true)
        {
            var baseType = typeof(T);

            AssemblyScannerCache cache;
            if (!s_Cache.TryGetValue(baseType, out cache))
            {
                cache = new AssemblyScannerCache();
                s_Cache[baseType] = cache;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes().Where(t => t.IsSubclassOf(baseType) && !t.IsGenericType && !t.IsAbstract))
                    {
                        string name = nicifyNames ? ObjectNames.NicifyVariableName(type.Name) : type.Name;
                        cache.names.Add(name);
                        cache.types.Add(type);
                    }
                }
            }

            types.AddRange(cache.types);

            if(names != null)
                names.AddRange(cache.names);
        }
    }
}