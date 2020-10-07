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
                rawPluginName = rawPluginName.Remove(startIndex, length+1);
            }
            while (rawPluginName.Contains("(") && rawPluginName.Contains(")"))
            {
                int startIndex = rawPluginName.IndexOf("(", StringComparison.Ordinal);
                int length = rawPluginName.IndexOf(")", StringComparison.Ordinal) - startIndex;
                rawPluginName = rawPluginName.Remove(startIndex, length+1);
            }
            rawPluginName = rawPluginName.Trim();
            if (rawPluginName.Contains("|"))
            {
                rawPluginName = rawPluginName.Remove(rawPluginName.IndexOf("|", StringComparison.Ordinal));
            }
            if (rawPluginName.Contains("-"))
            {
                rawPluginName = rawPluginName.Remove(rawPluginName.IndexOf("-", StringComparison.Ordinal));
            }
            rawPluginName = rawPluginName.Trim();

            return rawPluginName;
        }

        public static string PluginNameToJarName(string pluginName)
        {
            string jarName = BeautifyPluginName(pluginName);
            jarName = String.Concat(jarName.Where(c => !Char.IsWhiteSpace(c)));

            return jarName;
        }
    }
}