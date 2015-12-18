using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp.Extensions.MonoHttp;

namespace ALMRestClient
{
    public static class HTMLCleanUp
    {
        /// <summary>
        /// Cleanse HTML tags and other detritus from the string to make it plaintext
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Clean(string source)
        {
            string withoutLineBreaks = Regex.Replace(source, "\n", string.Empty);
            string withParagraphBreaks = Regex.Replace(withoutLineBreaks, "</?(p|br|div) *[^>]*>", "\n");
            string tagsRemoved = Regex.Replace(withParagraphBreaks, "<[^>]*>", string.Empty);
            string removeCrs = Regex.Replace(tagsRemoved, "\r", string.Empty);
            string multiLineBreaksRemoved = Regex.Replace(removeCrs, "\n+", "\n");
            string leadingLineBreakRemoved = Regex.Replace(multiLineBreaksRemoved, "^\n", string.Empty);
            string trailingLineBreakRemoved = Regex.Replace(leadingLineBreakRemoved, "\n$", string.Empty);
            // conversion for &...; s
            return HttpUtility.HtmlDecode(trailingLineBreakRemoved);
        }
    }
}
