using System.Collections.Generic;
using UnityEngine;

namespace LingeringHopes.Gameplay
{
    public static class IListExtensions
    {
        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int count = list.Count;
            for (int i = 0; i < count - 1; ++i)
            {
                int randomIndex = Random.Range(i, count);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}

