using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class HtmlDocumentHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool HasUserMention(this HtmlDocument doc)
        {
            var spans = doc.DocumentNode.SelectNodes("//span");

            if (spans != null)
            {
                foreach (var s in spans)
                {
                    if (s.Attributes.Count(a => a.Name == "class") > 0)
                    {
                        var cls = s.Attributes.FirstOrDefault(a => a.Name == "class");
                        if (cls.Value.Contains("userhint"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}