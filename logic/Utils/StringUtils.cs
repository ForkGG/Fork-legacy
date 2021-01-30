using System;
using System.Linq;

namespace Fork.Logic.Utils
{
    public class StringUtils
    {
        public static string BeautifyPluginName(string rawPluginName)
        {
            rawPluginName = rawPluginName.Trim();
            while (rawPluginName.Contains("[") && rawPluginName.Contains("]"))
            {
                int startIndex = rawPluginName.IndexOf("[", StringComparison.Ordinal);
                int length = rawPluginName.IndexOf("]", StringComparison.Ordinal) - startIndex;
                rawPluginName = rawPluginName.Remove(startIndex, length + 1);
            }

            while (rawPluginName.Contains("(") && rawPluginName.Contains(")"))
            {
                int startIndex = rawPluginName.IndexOf("(", StringComparison.Ordinal);
                int length = rawPluginName.IndexOf(")", StringComparison.Ordinal) - startIndex;
                rawPluginName = rawPluginName.Remove(startIndex, length + 1);
            }

            rawPluginName = rawPluginName.Trim();
            if (rawPluginName.Contains("|"))
                rawPluginName = rawPluginName.Remove(rawPluginName.IndexOf("|", StringComparison.Ordinal));

            if (rawPluginName.Contains("-"))
                rawPluginName = rawPluginName.Remove(rawPluginName.IndexOf("-", StringComparison.Ordinal));

            rawPluginName = rawPluginName.Trim();

            return rawPluginName;
        }

        public static string PluginNameToJarName(string pluginName)
        {
            string jarName = BeautifyPluginName(pluginName);
            jarName = string.Concat(jarName.Where(c => !char.IsWhiteSpace(c)));

            return jarName;
        }

        //Source: https://stackoverflow.com/a/9454016/10188858
        /// <summary>
        ///     Computes the Damerau-Levenshtein Distance between two strings, represented as arrays of
        ///     integers, where each integer represents the code point of a character in the source string.
        ///     Includes an optional threshold which can be used to indicate the maximum allowable distance.
        /// </summary>
        /// <param name="source">An array of the code points of the first string</param>
        /// <param name="target">An array of the code points of the second string</param>
        /// <param name="threshold">Maximum allowable distance</param>
        /// <returns>Int.MaxValue if threshold exceeded; otherwise the Damerau-Leveshteim distance between the strings</returns>
        public static int DamerauLevenshteinDistance(string source, string target, int threshold)
        {
            int length1 = source.Length;
            int length2 = target.Length;

            // Return trivial case - difference in string lengths exceeds threshold
            if (Math.Abs(length1 - length2) > threshold) return int.MaxValue;

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2)
            {
                Swap(ref target, ref source);
                Swap(ref length1, ref length2);
            }

            int maxi = length1;
            int maxj = length2;

            int[] dCurrent = new int[maxi + 1];
            int[] dMinus1 = new int[maxi + 1];
            int[] dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (int i = 0; i <= maxi; i++) dCurrent[i] = i;

            int jm1 = 0, im1 = 0, im2 = -1;

            for (int j = 1; j <= maxj; j++)
            {
                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int minDistance = int.MaxValue;
                dCurrent[0] = j;
                im1 = 0;
                im2 = -1;

                for (int i = 1; i <= maxi; i++)
                {
                    int cost = source[im1] == target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    int min = del > ins ? ins > sub ? sub : ins : del > sub ? sub : del;

                    if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                        min = Math.Min(min, dMinus2[im2] + cost);

                    dCurrent[i] = min;
                    if (min < minDistance) minDistance = min;

                    im1++;
                    im2++;
                }

                jm1++;
                if (minDistance > threshold) return int.MaxValue;
            }

            int result = dCurrent[maxi];
            return result > threshold ? int.MaxValue : result;
        }

        private static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }
    }
}