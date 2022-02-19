using System;
using System.Text;

namespace zapread.com.Helpers
{
    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirty"></param>
        /// <returns></returns>
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

        /// <summary>
        /// // From https://stackoverflow.com/questions/25259/how-does-stack-overflow-generate-its-seo-friendly-urls/25486#25486
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static String MakeURLFriendly(this string title)
        {
            if (title == null) return "";

            const int maxlen = 80;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '(' || c == ')' || c == ':' ||
                    c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'Þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }
    }
}