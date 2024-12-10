
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Extensions
{
    public static class Extensions
    {

        // Algoritmo de mezclado Fisher-Yates
        public static void Shuffle(this int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, i + 1);
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        public static T GetRandom<T>(this List<T> list)
        {
            return list[RandomNumberGenerator.GetInt32(0, list.Count)];
        }

        /// <summary>
        /// return a random element of the list or default if list is empty
        /// </summary>
        /// <param name="e"></param>
        /// <param name="weightSelector">
        /// return chances to be picked for the element. A weigh of 0 or less means 0 chance to be picked.
        /// If all elements have weight of 0 or less they all have equal chances to be picked.
        /// </param>
        /// <returns></returns>
        public static T AnyOrDefault<T>(this IList<T> e, Func<T, double> weightSelector)
        {
            if (e.Count < 1)
                return default(T);
            if (e.Count == 1)
                return e[0];
            var weights = e.Select(o => Math.Max(weightSelector(o), 0)).ToArray();
            var sum = weights.Sum(d => d);

            var rnd = new Random().NextDouble();
            for (int i = 0; i < weights.Length; i++)
            {
                //Normalize weight
                var w = sum == 0
                    ? 1 / (double)e.Count
                    : weights[i] / sum;

                if (rnd < w)
                    return e[i];

                rnd -= w;
            }
            throw new Exception("Should not happen");
        }


        


    }
}
