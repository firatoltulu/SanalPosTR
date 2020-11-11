using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SimplePayTR.Core.Extensions
{
    public static class StringExtension
    {
        private static Dictionary<String, Regex> cache = new Dictionary<string, Regex>();

        private static Regex cacheRegex(string r)
        {
            if (!cache.ContainsKey(r))
                cache[r] = new Regex(r, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            return cache[r];
        }

        public static Match Match(this string s, string regex)
        {
            Regex r = cacheRegex(regex);
            return r.Match(s);
        }

        /// <summary>
        /// İstenilen tağı kaldıracak
        /// </summary>
        /// <param name="s">metin</param>
        /// <param name="tag">kaldıralacak etiket adı</param>
        /// <returns></returns>
        public static string Remove(this string s, string tag)
        {
            string reg = @"<[\/]{0,1}(" + tag + ")[^><]*>";
            Regex r = cacheRegex(reg);

            return r.Replace(s, string.Empty);
        }

        public static bool IsEmpty(this string value)
        {
            if (value == null || value.Length == 0)
                return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }

            return true;
        }

        public static bool IsNullOrEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            return IsEmpty(value);
        }


    }
}