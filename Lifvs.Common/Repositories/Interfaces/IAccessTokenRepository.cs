using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface IAccessTokenRepository
    {
        User ValidateLoginUserCedential(AudienceCredentials credential);
        bool UpdateDeviceDetail(User audience);
        Guid CreateToken(User audience, string tokenType = "Login");
        AccessToken GetAccessToken(Guid token);
        bool UpdateToken(AccessToken accessToken);
        User GetUser(long userId);
        bool ExpireClientToken(string audienceId, Guid accessToken);
        void CheckOAuthUserExistsAndCheckEmailExists(string oauthId, string emailId, out bool oAuthExists, out bool emailExists);
        User GetUserByEmail(string emailId);
        void OAuthEmailUserUpdate(User audience);
        long CreateNewUser(User audienceCredentials);
        bool ValidEmail(string emailId);
        bool CreateNewRecoverCode(RecoverCode model);
        RecoverCode CheckValidRecoverCode(RecoveryCode model);
        bool ChangePassword(User model, long recoverId);
        bool UpdateVerifyFlagForUser(User model);
        User GetUserDetailByIdAndEmail(long userId, string email);
        SessionUser GetWebUser(LoginModel model);
        bool ValidWebEmail(string emailId);
        WebUser GetWebUserByEmail(string emailId);
        bool CreateNewWebUserRecoverCode(WebUserRecoveryCode model);
        WebUserRecoveryCode CheckValidWebRecoveryCode(RecoveryCode model);
        bool ChangeWebPassword(WebUser model, long recoverCodeId);
        User GetUserByIdAndPassword(long? userId, string oldPassword);
        bool UpdateUseroldPasswordToNewPassword(User model);
        bool CheckDuplicateEmail(long userId, string email);
        User GetUser(LoginModel model);
        User GetUserByBarCode(string barCode);
    }
}
