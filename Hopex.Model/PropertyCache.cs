using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GraphQL.Execution;
using Mega.Macro.API;

namespace Hopex.Model
{
    public static class PropertyCache
    {
        private static readonly ConcurrentDictionary<ulong, object> _cache = new ConcurrentDictionary<ulong, object>();

        public static int HitCount { get; private set; }
        public static int MissCount { get; private set; }


        public static void ResetCache()
        {
            HitCount = 0;
            MissCount = 0;
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

        private static ulong ComputeKey(MegaId objectId, MegaId propertyId, IDictionary<string, ArgumentValue> arguments, string format, string cacheType)
        {
            var argumentsCount = arguments != null ? arguments.Count * 2 : 0;
            var hashData = new int[4 + argumentsCount];
            if (argumentsCount != 0)
            {
                var i = 4;
                foreach (var kvp in arguments)
                {
                    hashData[i++] = kvp.Key.GetHashCode();
                    hashData[i++] = kvp.Value.Value?.GetHashCode() ?? 0;
                }
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

            hashData[0] = objectId.GetHashCode();
            hashData[1] = propertyId.GetHashCode();
            hashData[2] = formatHashCode;
            hashData[3] = cacheTypeHashCode;

            return HashDepot.XXHash.Hash64(MemoryMarshal.Cast<int, byte>(hashData));
        }
    }
}
