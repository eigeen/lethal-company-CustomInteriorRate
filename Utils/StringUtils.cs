using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Utils
{
    public class StringUtils
    {
        public static string TrimStart(string source, string toTrim)
        {
            if (source.StartsWith(toTrim))
            {
                return source[toTrim.Length..];
            }
            return source;
        }

        public static string TrimEnd(string source, string toTrim)
        {
            if (source.EndsWith(toTrim))
            {
                return source[..^toTrim.Length];
            }
            return source;
        }
    }
}
