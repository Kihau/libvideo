using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace VideoLibrary.Extensions
{
    internal static class Html
    {
        // TODO: Refactor?
        public static string GetNode(string name, string source) =>
            WebUtility.HtmlDecode(
                Text.StringBetween(
                    '<' + name + '>', "</" + name + '>', source));

        public static IEnumerable<string> GetUrisFromManifest(string source)
        {
            string opening = "<BaseURL>";
            string closing = "</BaseURL>";
            int start = source.IndexOf(opening, StringComparison.Ordinal);
            if (start != -1)
            {
                string temp = source.Substring(start);
                var uris = temp.Split(new string[] { opening }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Substring(0, v.IndexOf(closing, StringComparison.Ordinal)));
                return uris;
            }
            throw new NotSupportedException();
        }
    }
}
