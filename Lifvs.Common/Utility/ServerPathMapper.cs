using Lifvs.Common.Utility.Interfaces;
using System;
using System.Web;

namespace Lifvs.Common.Utility
{
    public class ServerPathMapper : IPathMapper
    {
        public string MapPath(string relativePath)
        {
            return HttpContext.Current.Server.MapPath("~/bin/" + relativePath.TrimStart(new char[] { '/' }));
        }

        public string UrlCombine(string baseUrl, string relativeUrl)
        {
            var baseUri = new Uri(baseUrl);
            var myUri = new Uri(baseUri, relativeUrl);
            return myUri.ToString();
        }
    }
}
