using System;

namespace VideoLibrary.Extensions
{
    internal static class Text
    {
        public static string StringBetween(string prefix, string suffix, string parent)
        {
            int start = parent.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length;

            if (start < prefix.Length)
                return string.Empty;

            int end = parent.IndexOf(suffix, start, StringComparison.Ordinal);

            if (end == -1)
                end = parent.Length;

            return parent.Substring(start, end - start);
        }

        public static int SkipWhitespace(this string text, int start)
        {
            int result = start;
            while (char.IsWhiteSpace(text[result]))
                result++;
            return result;
        }
    }
}
