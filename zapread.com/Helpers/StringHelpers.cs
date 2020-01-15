using System;
using System.Text;

namespace zapread.com.Helpers
{
    public static class StringHelpers
    {
        /// <summary>
        /// Cleans the string of any illegal scripts and html elements
        /// </summary>
        /// <param name="dirty"></param>
        /// <returns></returns>
        public static String SanitizeXSS(this string dirty)
        {
            // Fix for nasty inject with odd brackets
            byte[] bytes = Encoding.Unicode.GetBytes(dirty);
            dirty = Encoding.Unicode.GetString(bytes);
            var sanitizer = new Ganss.XSS.HtmlSanitizer();
            sanitizer.AllowedTags.Remove("button");
            sanitizer.AllowedTags.Remove("form");
            sanitizer.AllowedTags.Add("iframe");
            sanitizer.AllowedTags.Remove("script");
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("frameborder");
            sanitizer.AllowedAttributes.Add("allowfullscreen");
            sanitizer.AllowedAttributes.Add("seamless");
            sanitizer.AllowedAttributes.Remove("id");
            sanitizer.AllowedAttributes.Remove("onload");
            sanitizer.AllowedAttributes.Remove("onmousemove");
            var sanitizedComment = sanitizer.Sanitize(dirty);
            return sanitizedComment;
        }

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

        public static String RemoveUnicodeNonPrinting(this string dirty)
        {
            string normName = dirty.Normalize(System.Text.NormalizationForm.FormC);

            StringBuilder sb = new StringBuilder();

            foreach (char c in normName)
            {
                var uc = char.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.Format &&
                    uc != System.Globalization.UnicodeCategory.Control &&
                    uc != System.Globalization.UnicodeCategory.SpaceSeparator)
                {
                    sb.Append(c);
                }
            }

            var cleanName = sb.ToString();

            return cleanName;
        }
    }
}