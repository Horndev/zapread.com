using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace zapread.com.Helpers
{
    /// <summary>
    /// Static method with Language Utilities
    /// </summary>
    public static class LanguageHelpers
    {
        /// <summary>
        /// Return a list of known languages
        /// </summary>
        /// <param name="includeNative">default true - return also the language in native name</param>
        /// <returns></returns>
        public static List<string> GetLanguages(bool includeNative = true)
        {
            // List of languages known
            var languagesEng = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                .GroupBy(ci => ci.TwoLetterISOLanguageName)
                .Select(g => g.First())
                .Select(ci => ci.Name + ":" + ci.EnglishName).ToList();

            if (includeNative)
            {
                var languagesNat = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                .GroupBy(ci => ci.TwoLetterISOLanguageName)
                .Select(g => g.First())
                .Select(ci => ci.Name + ":" + ci.NativeName).ToList();

                var languages = languagesEng.Concat(languagesNat).ToList();
                return languages;
            }

            return languagesEng;
        }

        /// <summary>
        /// Convert a string language name to 2 letter ISO
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string NameToISO(string name)
        {
            var cult = CultureInfo.GetCultures(CultureTypes.NeutralCultures).Skip(1)
                .FirstOrDefault(ci => ci.Name.ToUpperInvariant() == name.ToUpperInvariant() || ci.NativeName.ToUpperInvariant() == name.ToUpperInvariant());

            if (cult == null)
            {
                return "";
            }
            return cult.TwoLetterISOLanguageName;
        }
    }
}