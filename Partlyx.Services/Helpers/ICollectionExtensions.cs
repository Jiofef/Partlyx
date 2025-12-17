using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.UI.Avalonia.Helpers
{
    public static class ICollectionExtensions
    {
        public static void ClearAndDispose<T>(this ICollection<T> collection) where T: IDisposable
        {
            foreach (T  item in collection)
                item.Dispose();

            collection.Clear();
        }

        public static void ClearAndTryDispose<T>(this ICollection<T> collection)
        {
            foreach (T item in collection)
                if (item is IDisposable d)
                    d.Dispose();

            collection.Clear();
        }

        public static void ClearAndDispose<T1, T2>(this ICollection<KeyValuePair<T1, T2>> collection) where T2: IDisposable
        {
            foreach (KeyValuePair<T1, T2> pair in collection)
                pair.Value.Dispose();

            collection.Clear();
        }
        public static void ClearAndTryDispose<T1, T2>(this ICollection<KeyValuePair<T1, T2>> collection)
        {
            foreach (KeyValuePair<T1, T2> pair in collection)
                if (pair.Value is IDisposable d)
                    d.Dispose();

            collection.Clear();
        }
        public static void TryAddIfNotNull<T>(this ICollection<T> collection, T? value)
        {
            if (value != null)
                collection.Add(value);
        }
        public static bool IsNullOrEmpty<T>(this ICollection<T>? collection)
            => collection == null || collection.Count == 0;
    }
}
