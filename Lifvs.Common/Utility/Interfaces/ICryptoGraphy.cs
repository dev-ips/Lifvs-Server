namespace Lifvs.Common.Utility.Interfaces
{
    public interface ICryptoGraphy
    {
        string EncryptString(string message);
        string DecryptString(string message);
        string GenerateCode();
        string GeneratePassword();
    }
}
