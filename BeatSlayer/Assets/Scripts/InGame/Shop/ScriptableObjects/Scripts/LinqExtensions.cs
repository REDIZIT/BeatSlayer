using System;
using System.Collections.Generic;
using System.Linq;

namespace InGame
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static T Random<T>(this IEnumerable<T> list)
        {
            return list.ElementAt(UnityEngine.Random.Range(0, list.Count()));
        }
    }
}