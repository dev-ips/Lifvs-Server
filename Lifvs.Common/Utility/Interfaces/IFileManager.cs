namespace Lifvs.Common.Utility.Interfaces
{
    public interface IFileManager
    {
        bool FileExists(string filePath);
        string ReadFileContents(string filePath);
    }
}
