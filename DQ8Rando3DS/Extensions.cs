using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DQ8Rando3DS
{
    public static class Extensions
    {
        public static bool SetBytes(Span<byte> data, byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                data[i] = value[i];
            }
            return true;
        }

        // I copied this from a stackoverflow page. I don't really vibe with
        // using this as a standard enumerator because I know I'll be using
        // the same RNG for other things, and that might interfere with this
        // function, which uses the same RNG. So I'm mostly just going to be
        // converting to a List whenever I use it so that I just randomize
        // everything at the same time.
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i > 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                T tmp = elements[i];
                elements[i] = elements[swapIndex];
                elements[swapIndex] = tmp;
            }
            foreach (T element in elements)
            {
                yield return element;
            }
        }

        public static IEnumerable<T>[] Chunkify<T>(this IEnumerable<T> source, params double[] splits)
        {
            IEnumerable<T>[] result = new IEnumerable<T>[splits.Length];

            double splitstotal = splits.Sum();
            for (int i = 0; i < splits.Length; i++)
            {
                splits[i] = splits[i] / splitstotal;
            }

            double total = 0;
            int totalint = 0;

            for (int i = 0; i < splits.Length; i++)
            {
                double s = splits[i];
                total += source.Count() * s;
                int val = (int)Math.Round(total - totalint);

                result[i] = source.ToArray()[totalint..(totalint + val)];

                totalint += val;
            }

            return result;
        }
        public static int[] Chunkify(int sourceCount, params double[] splits)
        {
            double splitstotal = splits.Sum();
            int[] result = new int[splits.Length];
            for (int i = 0; i < splits.Length; i++)
            {
                splits[i] = splits[i] / splitstotal;
            }

            double total = 0;
            int totalint = 0;

            for (int i = 0; i < splits.Length; i++)
            {
                double s = splits[i];
                total += sourceCount * s;
                int val = (int)Math.Round(total - totalint);

                result[i] = val;

                totalint += val;
            }

            return result;
        }

        public static void WriteAllText(string path, string text)
        {
            Directory.CreateDirectory( Path.GetDirectoryName(path) );
            File.WriteAllText(path, text);
        }
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, bytes);
        }
        public static void WriteAllLines(string path, IEnumerable<string> contents)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, contents);
        }
        public static void Copy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destFileName));
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        public static bool NextBool(this Random rng)
        {
            return rng.Chance(0.5);
        }
        public static bool Chance(this Random rng, double chance)
        {
            return rng.NextDouble() < chance;
        }
        public static Random NextRandom(this Random rng)
        {
            return new Random(rng.Next(int.MinValue, int.MaxValue));
        }
        public static T Choice<T>(this Random rng, IEnumerable<T> values)
        {
            if (values.Count() == 0)
                return default;
            if (values.Count() == 1)
                return values.First();

            return values.ToArray()[rng.Next(0, values.Count())];
        }
        public static T Choice<T>(this Random rng, IEnumerable<T> values, Predicate<T> predicate)
        {
            List<T> copy = values.ToList();
            while (copy.Count > 0)
            {
                T choice = rng.Choice(copy);

                if (predicate(choice))
                    return choice;
                else
                    copy.Remove(choice);
            }

            return default;
        }
    }
}
