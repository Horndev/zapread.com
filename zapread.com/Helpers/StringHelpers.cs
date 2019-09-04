using System;
using System.Text;

namespace zapread.com.Helpers
{
    public static class StringHelpers
    {
        /// <summary>
        /// This function cleans extreme unicode diacritics used in Zalgo text.
        /// </summary>
        /// <param name="dirty"></param>
        /// <returns></returns>
        public static String CleanUnicode(this string dirty)
        {
            string normName = dirty.Normalize(System.Text.NormalizationForm.FormC);

            StringBuilder sb = new StringBuilder();

            foreach (char c in normName)
            {
                if (char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var cleanName = sb.ToString();

            return cleanName;
        }
    }
}