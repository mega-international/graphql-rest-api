using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GraphQL.Execution;
using Mega.Macro.API;

namespace Hopex.Model
{
    public static class PropertyCache
    {
        private static readonly ConcurrentDictionary<int, object> _cache = new ConcurrentDictionary<int, object>();

        public static int HitCount { get; private set; }
        public static int MissCount { get; private set; }


        public static void ResetCache()
        {
            _cache.Clear();
        }

        public static bool TryGetValue<T>(out T value, MegaId objectId, MegaId propertyId, IDictionary<string, ArgumentValue> arguments = null, string format = null, string cacheType = null)
        {
            var key = ComputeKey(objectId, propertyId, arguments, format, cacheType);

            if (!_cache.TryGetValue(key, out var objectValue))
            {
                MissCount++;
                value = default(T);
                return false;
            }

            HitCount++;
            value = (T)objectValue;
            return true;
        }

        public static bool TryAdd<T>(T value, MegaId objectId, MegaId propertyId, IDictionary<string, ArgumentValue> arguments = null, string format = null, string cacheType = null)
        {
            var key = ComputeKey(objectId, propertyId, arguments, format, cacheType);
            return _cache.TryAdd(key, value);
        }

        private static int ComputeKey(MegaId objectId, MegaId propertyId, IDictionary<string, ArgumentValue> arguments, string format, string cacheType)
        {
            var argumentHashCode = 0;
            if (arguments != null)
            {
                argumentHashCode = arguments.GetHashCode();
            }
            var formatHashCode = 0;
            if (format != null)
            {
                formatHashCode = format.GetHashCode();
            }
            var cacheTypeHashCode = 0;
            if (cacheType != null)
            {
                cacheTypeHashCode = cacheType.GetHashCode();
            }
            return objectId.GetHashCode() ^ propertyId.GetHashCode() ^ argumentHashCode ^ formatHashCode ^ cacheTypeHashCode;
        }
    }
}
