using Lifvs.Common.Utility.Interfaces;
using System.IO;
using System.Text.RegularExpressions;

namespace Lifvs.Common.Utility
{
    public class FileManager : IFileManager
    {
        private readonly ICacheHelper _cache;
        public FileManager(ICacheHelper cache)
        {
            _cache = cache;
        }
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        public string ReadFileContents(string filePath)
        {
            if (_cache.GetCachedItem(filePath) != null) return (string)_cache.GetCachedItem(filePath);
            string stringData;
            using (var schameReader = File.OpenText(filePath)) stringData = schameReader.ReadToEnd();
            var fileContent = Regex.Replace(stringData, @"\\", @"\\");
            _cache.AddToCache(filePath, fileContent);
            return fileContent;
        }
    }
}
