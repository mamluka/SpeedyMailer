using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SpeedyMailer.Core.Utilities.Extentions
{
    public static class LinqExtentions
    {
		public static IEnumerable<T> EmptyIfNull<T> (this IEnumerable<T> target )
		{
			return target ?? Enumerable.Empty<T>();
		}
		
		public static IDictionary<T1,T2> EmptyIfNull<T1,T2>(this IDictionary<T1,T2> target )
		{
			return target ?? new Dictionary<T1, T2>();
		}

	    public static IEnumerable<IEnumerable<T>> Clump<T>(this IEnumerable<T> source, int size)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (size < 1)
                throw new ArgumentOutOfRangeException("size", "size must be greater than 0");

            return ClumpIterator(source, size);
        }

        private static IEnumerable<IEnumerable<T>> ClumpIterator<T>(IEnumerable<T> source, int size)
        {
            Debug.Assert(source != null, "source is null.");

            var items = new T[size];
            var count = 0;
            foreach (var item in source)
            {
                items[count] = item;
                count++;

                if (count == size)
                {
                    yield return items;
                    items = new T[size];
                    count = 0;
                }
            }
            if (count > 0)
            {
                if (count == size)
                    yield return items;
                else
                {
                    var tempItems = new T[count];
                    Array.Copy(items, tempItems, count);
                    yield return tempItems;
                }
            }
        }

    }
}
