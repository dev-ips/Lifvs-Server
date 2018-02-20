namespace Lifvs.Common.Utility.Interfaces
{
    public interface IPathMapper
    {
        string MapPath(string relativePath);
        string UrlCombine(string baseUrl, string relativeUrl);
    }
}
