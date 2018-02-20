using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;

namespace Lifvs.Common.Services.Interfaces
{
    public interface IAccessTokenService
    {
        object ValidateAndCreateAccessToken(AudienceCredentials credentials);
        bool TryValidateToken(string authorizationParameter, out Audience audience);
        bool LogoutUser(string userId, string accessToken);
        object ValidateAndCreateUser(RegisterModel model);
        int ValidateEmailAndSendCode(string emailId);
        bool ChangePassword(RecoveryCode model);
        EmailVerificationResponse VerifyUser(string userId, string email);
        bool ResendEmail(long userId);
        SessionUser GetWebUser(LoginModel model);
        int ValidateWebEmailAndSendCode(string emailId);
        bool ChangeWebPassword(RecoveryCode model);
        bool ChangeOldPassword(ChangePasswordModel model);

        //User GetUserByEmail(string emailId);
        User GetUser(LoginModel model);
        User GetUserByBarCode(string barCode);
    }
}
