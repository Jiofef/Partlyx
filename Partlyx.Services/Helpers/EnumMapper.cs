using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Partlyx.Services.Helpers
{
    public static class EnumMapper<TEnum> where TEnum : struct, Enum
    {
        // numeric -> array of alias names
        private static readonly Dictionary<int, string[]> ValueToKeysMap;
        // numeric -> array of enum values (if needed)
        private static readonly Dictionary<int, TEnum[]> ValueToValuesMap;
        // ordered numeric keys (optional)
        private static readonly int[] OrderedKeys;

        static EnumMapper()
        {
            // get all names (no allocations of name strings — runtime provides them)
            var names = Enum.GetNames(typeof(TEnum));
            // temporary dictionary numeric -> List<string> to accumulate
            var temp = new Dictionary<int, List<string>>(names.Length);

            foreach (var name in names)
            {
                // parse once — parsing small number of items is cheap
                if (!Enum.TryParse<TEnum>(name, out var enumValue))
                    continue;

                int numeric = Convert.ToInt32(enumValue);

                if (!temp.TryGetValue(numeric, out var list))
                {
                    list = new List<string>(2); // most cases: 1, sometimes 2+
                    temp[numeric] = list;
                }

                list.Add(name);
            }

            // finalize maps with arrays to reduce overhead
            ValueToKeysMap = new Dictionary<int, string[]>(temp.Count);
            ValueToValuesMap = new Dictionary<int, TEnum[]>(temp.Count);

            var keys = new int[temp.Count];
            int idx = 0;
            foreach (var kv in temp)
            {
                keys[idx++] = kv.Key;
                ValueToKeysMap[kv.Key] = kv.Value.ToArray();

                var arr = new TEnum[kv.Value.Count];
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    Enum.TryParse<TEnum>(kv.Value[i], out arr[i]);
                }
                ValueToValuesMap[kv.Key] = arr;
            }

            Array.Sort(keys);
            OrderedKeys = keys;
        }

        public static ReadOnlyDictionary<int, string[]> GetKeyValueGroups()
            => new ReadOnlyDictionary<int, string[]>(ValueToKeysMap);

        public static IReadOnlyList<string> GetKeysForValue(int value)
        {
            if (ValueToKeysMap.TryGetValue(value, out var arr))
                return arr;
            return Array.Empty<string>();
        }

        public static IReadOnlyList<TEnum> GetValuesForValue(int value)
        {
            if (ValueToValuesMap.TryGetValue(value, out var arr))
                return arr;
            return Array.Empty<TEnum>();
        }

        public static Dictionary<int, string[]> GetKeyValueGroupsForValues(IEnumerable<int> values)
        {
            var result = new Dictionary<int, string[]>();
            foreach (var v in values)
            {
                if (ValueToKeysMap.TryGetValue(v, out var arr))
                {
                    result[v] = arr;
                }
            }
            return result;
        }

        public static IReadOnlyList<int> GetOrderedKeys() => OrderedKeys;
    }
}
