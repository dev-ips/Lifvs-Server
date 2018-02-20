using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Lifvs.Common.Utility
{
    public static class StringExtensions
    {
        static readonly Regex re = new Regex(@"\{([^\}]+)\}", RegexOptions.Compiled);
        public static string ReplaceMatch(this string text, StringDictionary fields)
        {
            return re.Replace(text, match => fields[match.Groups[1].Value]);
        }
    }
}
