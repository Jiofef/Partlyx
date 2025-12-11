namespace Partlyx.Services.Helpers
{
    public static class ListExtensions
    {
        public static bool RemoveFirst<T>(this IList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }
}
