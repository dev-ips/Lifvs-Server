using iTextSharp.text.pdf;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services.Interfaces;
using Lifvs.Common.Utility;
using Lifvs.Common.Utility.Interfaces;
using Lifvs.Common.Validators.Interfaces;
using log4net;
using QRCoder;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace Lifvs.Common.Services
{
    public class AccessTokenService : IAccessTokenService
    {
        private readonly ILog _log;
        private readonly IAccessTokenRepository _accessTokenRepository;
        private readonly IValidatorService<AudienceCredentials> _loginUserValidators;
        private readonly IValidatorService<RegisterModel> _userRegistrationValidators;
        private readonly IValidatorService<RecoveryCode> _changePasswordValidators;
        private readonly IExceptionManager _exception;
        private readonly ICryptoGraphy _cryptoGraphy;
        private readonly IFileManager _fm;
        private readonly IPathMapper _path;
        private readonly IEmailNotifier _emailNotifier;
        private readonly IUserRepository _userRepository;
        public AccessTokenService(ILog log, IAccessTokenRepository accessTokenRepository, IValidatorService<AudienceCredentials> loginUserValidators, IValidatorService<RegisterModel> userRegistrationValidators, IValidatorService<RecoveryCode> changePasswordValidators, IExceptionManager exception, ICryptoGraphy cryptoGraphy, IFileManager fm, IPathMapper path, IEmailNotifier emailNotifier, IUserRepository userRepository)
        {
            _log = log;
            _accessTokenRepository = accessTokenRepository;
            _loginUserValidators = loginUserValidators;
            _userRegistrationValidators = userRegistrationValidators;
            _changePasswordValidators = changePasswordValidators;
            _exception = exception;
            _cryptoGraphy = cryptoGraphy;
            _fm = fm;
            _path = path;
            _emailNotifier = emailNotifier;
            _userRepository = userRepository;
        }
        public object ValidateAndCreateAccessToken(AudienceCredentials credentials)
        {
            User audience = null;

            _loginUserValidators.Validate(credentials);
            audience = _accessTokenRepository.ValidateLoginUserCedential(credentials);

            var userByEmail = _accessTokenRepository.GetUserByEmail(credentials.Username);

            if (audience != null)
            {
                if (!audience.IsVerified)
                {
                    var loginErrorResponse = new EmailUnverifiedResponse
                    {
                        UserId = audience.Id,
                        IsVerified = audience.IsVerified,
                        Message = "Din emailadress är inte verifierad än. Vill du att vi skickar ett nytt mail?"
                    };
                    var validateLoginErrorResponse = new LoginUnVerifiedEmailResponse
                    {
                        data = loginErrorResponse
                    };
                    return validateLoginErrorResponse;
                }

                credentials.Id = audience.Id;
                audience.DeviceId = credentials.DeviceId;
                audience.DeviceType = credentials.DeviceType;

                var loginMap = LoginMapper(credentials);
                _accessTokenRepository.UpdateDeviceDetail(loginMap);
                return TokenMapper(audience, _accessTokenRepository.CreateToken(audience));
            }
            else if (userByEmail != null)
            {
                if (string.IsNullOrEmpty(userByEmail.Password) && !string.IsNullOrEmpty(userByEmail.AuthId))
                {
                    var facebookUserErrorMessage = new FacebookLoginErrorResponse
                    {
                        Message = "Du är registrerad med Facebook. Försök att logga in med Facebook."
                    };
                    return facebookUserErrorMessage;
                }
                else
                {
                    throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogilitia inloggningsuppgifter.");
                }
            }
            else
            {
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogilitia inloggningsuppgifter.");
            }
        }
        public bool TryValidateToken(string authorizationParameter, out Audience audience)
        {
            audience = null;
            var token = _accessTokenRepository.GetAccessToken(Guid.Parse(authorizationParameter));
            var valid = Validate(token);
            if (!valid) return false;
            var resourceId = token.AudienceId;

            //update the accesstoken expiredOn
            _accessTokenRepository.UpdateToken(token);
            audience = UserAudienceMapper(_accessTokenRepository.GetUser(long.Parse(resourceId)));
            return true;
        }
        public bool LogoutUser(string userId, string accessToken)
        {
            return _accessTokenRepository.ExpireClientToken(userId, Guid.Parse(accessToken));
        }
        public bool Validate(AccessToken token)
        {
            if (token == null || token.Expired) return false;
            var dt = token.ExpiredOn;
            if (dt == null) return true;
            //return DateTime.Parse(dt.ToString()).Ticks >= DateTime.Now.Ticks;
            return DateTime.SpecifyKind(DateTime.Parse(dt.ToString()), DateTimeKind.Utc).Ticks >= DateTime.UtcNow.Ticks;
        }
        public object ValidateAndCreateUser(RegisterModel model)
        {
            var message = string.Empty;
            User authUser = null;
            //_userRegistrationValidators.Validate(model);

            if (!string.IsNullOrEmpty(model.AuthId))
            {
                var oAuthIdExist = false;
                var emailIdExist = false;
                _accessTokenRepository.CheckOAuthUserExistsAndCheckEmailExists(model.AuthId, model.UserName, out oAuthIdExist, out emailIdExist);
                if (emailIdExist)
                {
                    var user = _accessTokenRepository.GetUserByEmail(model.UserName);
                    if (user != null)
                    {
                        model.Id = user.Id;
                        var loginMap = LoginWithFacebookMapperForOAuthUserEmail(model);
                        _accessTokenRepository.OAuthEmailUserUpdate(loginMap);
                        authUser = user;
                        var emailToken = TokenMapper(authUser as User, _accessTokenRepository.CreateToken(authUser));
                        return emailToken;
                    }
                }
                else
                {

                    var user = UserAudienceCredentialsMapper(model);
                    user.IsVerified = true;
                    user.UserCode = Convert.ToString(Guid.NewGuid());
                    SaveUserCode(user.UserCode);
                    var userId = _accessTokenRepository.CreateNewUser(user);
                    user.Id = userId;
                    authUser = user;
                    message = "Användare tillagd.";
                    var emailToken = TokenMapper(authUser as User, _accessTokenRepository.CreateToken(authUser), message);
                    return emailToken;
                }
            }
            else
            {
                _userRegistrationValidators.Validate(model);
                var user = UserAudienceCredentialsMapper(model);
                user.UserCode = Convert.ToString(Guid.NewGuid());
                SaveUserCode(user.UserCode);
                var userId = _accessTokenRepository.CreateNewUser(user);
                user.Id = userId;
                authUser = user;
                message = "Användare tillagd.";
            }
            var webUrl = ConfigurationManager.AppSettings["WebUrl"];

            var fields = new StringDictionary
            {
                {"signUpUrl",string.Format("{0}{1}{2}{3}",Convert.ToString(webUrl),"/Email/ConfirmEmail?id=",_cryptoGraphy.EncryptString(Convert.ToString(authUser.Id)),"&email="+_cryptoGraphy.EncryptString(authUser.Email)) }
            };

            message = "Vi har sänt dig ett verifikationsemail, vänligen verifiera din emailadress.";

            var htmlBody = _fm.ReadFileContents(GetMailerTemplatePath("html", "CreateUser")).ReplaceMatch(fields);
            _emailNotifier.SendEmail(authUser.Email, htmlBody, "Verify Link");
            var registerResponse = new RegisterResponse
            {
                UserId = authUser.Id,
                Message = message
            };
            //var token = TokenMapper(authUser as User, _accessTokenRepository.CreateToken(authUser));
            //token.Message = message;
            return registerResponse;
        }
        private void SaveUserCode(string userCode)
        {
            using (var ms = new MemoryStream())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(userCode, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (var bitMap = qrCode.GetGraphic(20))
                {
                    bitMap.Save(ms, ImageFormat.Png);
                    Image img = Image.FromStream(ms);
                    img.Save(HttpContext.Current.Server.MapPath("~/UserCodes/") + userCode + ".jpeg", ImageFormat.Jpeg);
                }
            }
            //Barcode128 code = new Barcode128();
            //code.Code = userCode;
            //Image barCode = code.CreateDrawingImage(Color.Black, Color.White);
            //barCode.Save(HttpContext.Current.Server.MapPath("~/UserCodes/") + userCode + ".jpeg", ImageFormat.Jpeg);
        }
        public int ValidateEmailAndSendCode(string emailId)
        {
            var expiredOn = Convert.ToInt32(ConfigurationManager.AppSettings["ExpiredValue"]);
            var isEmailExist = _accessTokenRepository.ValidEmail(emailId);
            if (!isEmailExist)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Emailadressen finns inte.");

            var user = _accessTokenRepository.GetUserByEmail(emailId);
            var recoverCode = new RecoveryCode();
            recoverCode.UserId = user.Id;
            recoverCode.Email = user.Email;
            recoverCode.RecoverCode = _cryptoGraphy.GenerateCode();
            recoverCode.ExpiredOn = DateTime.Now.AddMinutes(expiredOn);

            var recoveryCode = RecoveryCodeMapper(recoverCode);
            _accessTokenRepository.CreateNewRecoverCode(recoveryCode);
            var fields = new StringDictionary
            {
                {"Name",user.Email },
                {"RecoveryCode",recoveryCode.RecoveryCode },
                {"ExpiredOn",Convert.ToString(expiredOn) }
            };
            var htmlBody = _fm.ReadFileContents(GetMailerTemplatePath("html", "RecoveryCode")).ReplaceMatch(fields);
            _emailNotifier.SendEmail(emailId, htmlBody, "Recovery Code");
            var minutes = (expiredOn * 60) / 60;
            return minutes;
        }
        public bool ChangePassword(RecoveryCode model)
        {
            _changePasswordValidators.Validate(model);

            var user = _accessTokenRepository.GetUserByEmail(model.Email);
            if (user == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Användare finns inte.");

            model.UserId = user.Id;
            var recoverCode = _accessTokenRepository.CheckValidRecoverCode(model);

            if (recoverCode == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Ogiltig kod.");

            if (recoverCode.ExpiredOn < DateTime.Now)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Koden har gått ut.");

            var changePassword = ChangePasswordMapper(model);
            return _accessTokenRepository.ChangePassword(changePassword, recoverCode.Id);
        }
        public EmailVerificationResponse VerifyUser(string userId, string email)
        {
            userId = userId.Replace(" ", "+");
            var user = _cryptoGraphy.DecryptString(userId);
            email = email.Replace(" ", "+");

            var userEmail = _cryptoGraphy.DecryptString(email);


            var emailVerificationResponse = new EmailVerificationResponse();
            var message = string.Empty;

            User authUser = null;
            authUser = _accessTokenRepository.GetUserDetailByIdAndEmail(Convert.ToInt64(user), userEmail);

            if (authUser == null)
            {
                emailVerificationResponse.MessageType = "Fel";
                emailVerificationResponse.Message = "Ogiltig förfrågan.";
            }
            else if (!authUser.IsVerified)
            {
                _accessTokenRepository.UpdateVerifyFlagForUser(authUser);
                emailVerificationResponse.MessageType = "Framgång";
                emailVerificationResponse.Message = "Din emailadress är verifierad.";
                message = "Användare tillagd.";
            }
            else
            {
                emailVerificationResponse.MessageType = "Info";
                emailVerificationResponse.Message = "Email are redan verifierad.";
            }
            //var token = TokenMapper(authUser as User, _accessTokenRepository.CreateToken(authUser));
            //token.Message = message;
            return emailVerificationResponse;
        }
        public bool ResendEmail(long userId)
        {
            var user = _accessTokenRepository.GetUser(userId);

            if (user == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Användare finns inte.");
            var webUrl = ConfigurationManager.AppSettings["WebUrl"];

            var fields = new StringDictionary
            {
                {"signUpUrl",string.Format("{0}{1}{2}{3}",Convert.ToString(webUrl),"/Email/ConfirmEmail?id=",_cryptoGraphy.EncryptString(Convert.ToString(user.Id)),"&email="+_cryptoGraphy.EncryptString(user.Email)) }
            };

            var htmlBody = _fm.ReadFileContents(GetMailerTemplatePath("html", "CreateUser")).ReplaceMatch(fields);
            _emailNotifier.SendEmail(user.Email, htmlBody, "Verify Link");


            return true;
        }
        public SessionUser GetWebUser(LoginModel model)
        {
            var webUser = _accessTokenRepository.GetWebUser(model);
            return webUser;
        }
        public User GetUser(LoginModel model)
        {
            return _accessTokenRepository.GetUser(model);
        }

        public User GetUserByEmail(string email)
        {
            return _accessTokenRepository.GetUserByEmail(email);
        }


        public int ValidateWebEmailAndSendCode(string emailId)
        {
            var expiredOn = Convert.ToInt32(ConfigurationManager.AppSettings["ExpiredValue"]);
            var isEmailExist = _accessTokenRepository.ValidWebEmail(emailId);
            if (!isEmailExist)
                throw new Exception("Emailadressen finns inte.");

            var webUser = _accessTokenRepository.GetWebUserByEmail(emailId);
            var webUserRecoverCode = new WebUserRecoveryCode
            {
                WebUserId = webUser.Id,
                RecoveryCode = _cryptoGraphy.GenerateCode(),
                ExpiredOn = DateTime.Now.AddMinutes(expiredOn)
            };
            _accessTokenRepository.CreateNewWebUserRecoverCode(webUserRecoverCode);
            var fields = new StringDictionary
            {
                {"name",webUser.Name },
                {"RecoveryCode",webUserRecoverCode.RecoveryCode },
                {"ExpiredOn",expiredOn.ToString() }
            };
            var htmlBody = _fm.ReadFileContents(GetMailerTemplatePath("html", "RecoveryCode")).ReplaceMatch(fields);

            _emailNotifier.SendEmail(webUser.Email, htmlBody, "Återställningskod");
            var minutes = (expiredOn * 60) / 60;
            return minutes;
        }
        public bool ChangeWebPassword(RecoveryCode model)
        {
            var webUser = _accessTokenRepository.GetWebUserByEmail(model.Email);
            if (webUser == null)
                throw new Exception("Emailadressen finns inte.");

            model.UserId = webUser.Id;
            var checkValidRecoveryCode = _accessTokenRepository.CheckValidWebRecoveryCode(model);
            if (checkValidRecoveryCode == null)
                throw new Exception("Ogiltig kod.");

            if (checkValidRecoveryCode.ExpiredOn < DateTime.Now)
                throw new Exception("Koden har gått ut.");

            webUser.Password = _cryptoGraphy.EncryptString(model.NewPassword);
            webUser.ModifiedDate = DateTime.Now;
            return _accessTokenRepository.ChangeWebPassword(webUser, checkValidRecoveryCode.Id);
        }
        public bool ChangeOldPassword(ChangePasswordModel model)
        {
            if (string.IsNullOrEmpty(model.OldPassword))
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Det gamla lösenordet kan inte vara tomt.");

            if (string.IsNullOrEmpty(model.NewPassword))
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Det nya lösenordet kan inte vara tomt.");

            var user = _accessTokenRepository.GetUser(model.UserId.Value);
            if (user == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Användare finns inte.");

            var userByIdAndPassword = _accessTokenRepository.GetUserByIdAndPassword(model.UserId.Value, _cryptoGraphy.EncryptString(model.OldPassword));
            if (userByIdAndPassword == null)
                throw _exception.ThrowException(System.Net.HttpStatusCode.BadRequest, "", "Det gamla lösenordet matchar inte det befintliga lösenordet.");

            userByIdAndPassword.Password = _cryptoGraphy.EncryptString(model.NewPassword);
            userByIdAndPassword.ModifiedDate = DateTime.Now;

            return _accessTokenRepository.UpdateUseroldPasswordToNewPassword(userByIdAndPassword);
        }
        public User GetUserByBarCode(string barCode)
        {
            return _accessTokenRepository.GetUserByBarCode(barCode);
        }
        private string GetMailerTemplatePath(string text, string templateName)
        {
            var path = _path.MapPath(string.Format("mailtemplate/{0}/{1}.{2}", "default", templateName, text));
            return path;
        }
        private Audience UserAudienceMapper(User user)
        {
            return new Audience
            {
                AudienceId = user.Id.ToString(),
                AudienceType = CommonConstants.RoleUser,
                Email = user.Email
            };
        }
        private User LoginMapper(AudienceCredentials credential)
        {
            return new User
            {
                Id = credential.Id,
                DeviceId = credential.DeviceId,
                DeviceType = credential.DeviceType
            };
        }
        private AudienceResponse TokenMapper(User audience, Guid token, string message = "")
        {
            var webUrl = ConfigurationManager.AppSettings["WebUrl"] + "/";
            var userCardDetail = _userRepository.GetUserCardDetailsByUserId(audience.Id);
            UserDetail userDetailReturn = new UserDetail();
            userDetailReturn.UserId = audience.Id;
            userDetailReturn.Email = audience.Email;
            userDetailReturn.IsVerified = audience.IsVerified;
            userDetailReturn.AccessToken = token;
            userDetailReturn.Message = message;
            userDetailReturn.IsCardRegistered = userCardDetail != null ? true : false;
            userDetailReturn.UserCode = webUrl + "UserCodes/" + audience.UserCode + ".jpeg";

            var validateResponse = new AudienceResponse
            {
                data = userDetailReturn,
            };
            return validateResponse;
        }
        private User LoginWithFacebookMapperForOAuthUserEmail(RegisterModel model)
        {
            return new User
            {
                AuthId = model.AuthId,
                AuthType = model.AuthType,
                DeviceId = model.DeviceId,
                DeviceType = model.DeviceType,
                Id = model.Id
            };
        }
        private User UserAudienceCredentialsMapper(RegisterModel model)
        {
            return new User
            {
                Email = model.UserName,
                Password = !string.IsNullOrEmpty(model.Password) ? _cryptoGraphy.EncryptString(model.Password) : null,
                PhotoUrl = model.PhotoUrl,
                StreetAddress = model.StreetAddress,
                PostalAddress = model.PostalAddress,
                AreaAddress = model.AreaAddress,
                CountryId = model.CountryId,
                AuthType = model.AuthType,
                AuthId = model.AuthId,
                DeviceType = model.DeviceType,
                DeviceId = model.DeviceId,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }
        private RecoverCode RecoveryCodeMapper(RecoveryCode model)
        {
            return new RecoverCode
            {
                UserId = model.UserId,
                RecoveryCode = model.RecoverCode,
                ExpiredOn = model.ExpiredOn
            };
        }
        private User ChangePasswordMapper(RecoveryCode model)
        {
            return new User
            {
                Id = model.UserId.Value,
                Password = _cryptoGraphy.EncryptString(model.NewPassword),
                ModifiedDate = DateTime.Now
            };
        }

    }
}
