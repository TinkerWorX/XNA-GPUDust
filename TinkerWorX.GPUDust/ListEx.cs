using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinkerWorX.GPUDust
{
    static class ListEx
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void Shuffle<T>(this IList<T> list, Random seed)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = seed.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void AddUnique<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }
    }
}
