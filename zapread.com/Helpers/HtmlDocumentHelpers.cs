using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace zapread.com.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class HtmlDocumentHelpers
    {
        private static string urlPattern = @"(?<url>(?:https?|ftp|file):\/\/|www\.|ftp\.)(?:\([-A-Z0-9+&@#/%=~_|$?!:,.]*\)|[-A-Z0-9+&@#/%=~_|$?!:,.])*(?:\([-A-Z0-9+&@#/%=~_|$?!:,.]*\)|[A-Z0-9+&@#/%=~_|$])";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string AutoLink(string content)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(content);

                //get all elements text propery except for anchor element 
                var textNodes = doc.DocumentNode.SelectNodes("//text()[not(ancestor::a)]") ?? new HtmlAgilityPack.HtmlNodeCollection(null);

                foreach (var node in textNodes)
                {
                    Regex urlRegex = new Regex(urlPattern, RegexOptions.IgnoreCase);
                    var newText = urlRegex.Replace(node.InnerText, "<a href=\"${0}\">${0}</a>")
                        .Replace("href=\"www", "href=\"http://www")
                        .Replace("href=\"ftp", "href=\"ftp://ftp");
                    node.InnerHtml = newText;
                }

                content = doc.DocumentNode.OuterHtml;
            }
            catch (Exception)
            {
                // If it failed, then no foul to sanitized comment
            }
            return content;
        }

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

        // From https://raw.githubusercontent.com/ceee/ReadSharp/master/ReadSharp/HtmlUtilities.cs
        /// <summary>
        /// Converts HTML to plain text / strips tags.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns></returns>
        public static string ConvertToPlainText(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            StringWriter sw = new StringWriter();
            ConvertTo(doc.DocumentNode, sw);
            sw.Flush();
            return sw.ToString();
        }

        /// <summary>
        /// Count the words.
        /// The content has to be converted to plain text before (using ConvertToPlainText).
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns></returns>
        public static int CountWords(string plainText)
        {
            return !String.IsNullOrEmpty(plainText) ? plainText.Split(' ', '\n').Length : 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Cut(string text, int length)
        {
            if (!String.IsNullOrEmpty(text) && text.Length > length)
            {
                text = text.Substring(0, length - 4) + " ...";
            }
            return text;
        }

        private static void ConvertContentTo(HtmlNode node, TextWriter outText)
        {
            foreach (HtmlNode subnode in node.ChildNodes)
            {
                ConvertTo(subnode, outText);
            }
        }

        private static void ConvertTo(HtmlNode node, TextWriter outText)
        {
            string html;
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    // don't output comments
                    break;

                case HtmlNodeType.Document:
                    ConvertContentTo(node, outText);
                    break;

                case HtmlNodeType.Text:
                    // script and style must not be output
                    string parentName = node.ParentNode.Name;
                    if ((parentName == "script") || (parentName == "style"))
                        break;

                    // get text
                    html = ((HtmlTextNode)node).Text;

                    // is it in fact a special closing node output as text?
                    if (HtmlNode.IsOverlappedClosingElement(html))
                        break;

                    // check the text is meaningful and not a bunch of whitespaces
                    if (html.Trim().Length > 0)
                    {
                        outText.Write(HtmlEntity.DeEntitize(html));
                    }
                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            // treat paragraphs as crlf
                            outText.Write("\r\n");
                            break;
                        case "br":
                            outText.Write("\r\n");
                            break;
                        case "img":
                            var src = node.GetAttributeValue("src", "");
                            var alt = node.GetAttributeValue("alt", "");
                            if (src.ToUpper().Contains("HTTP"))
                            {
                                outText.Write("[external image removed]");
                            }
                            else
                            {
                                outText.Write("[image: " + node.GetAttributeValue("src", "") + ";" + (!String.IsNullOrEmpty(alt) ? alt : "") +  "]");
                            }
                            break;
                    }

                    if (node.HasChildNodes)
                    {
                        ConvertContentTo(node, outText);
                    }
                    break;
            }
        }
    }
}