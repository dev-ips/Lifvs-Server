using Dapper;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Lifvs.Common.Repositories
{
    public class AccessTokenRepository : IAccessTokenRepository
    {
        private readonly IDbConnection _db;
        private readonly ILog _log;
        private readonly ICryptoGraphy _cryptoGraphy;
        public AccessTokenRepository(IDbConnection db, ILog log, ICryptoGraphy cryptoGraphy)
        {
            _db = db;
            _log = log;
            _cryptoGraphy = cryptoGraphy;
        }
        public User ValidateLoginUserCedential(AudienceCredentials credential)
        {
            var sqlQuery = LoginUserQuery();
            _log.DebugFormat("Excecuting validation User Query. Query : {0}", sqlQuery);
            using (var multi = _db.QueryMultipleAsync(sqlQuery, new
            {
                @email = credential.Username.Trim(),
                @password = _cryptoGraphy.EncryptString(credential.Password.Trim())
            }).Result)
            {
                var userDetail = multi.ReadAsync<User>().Result.FirstOrDefault();
                return userDetail;
            }
        }
        public bool UpdateDeviceDetail(User audience)
        {
            var sqlQuery = UpdateDeviceDetailQuery();
            var res = _db.Execute(sqlQuery, new
            {
                @deviceId = audience.DeviceId,
                @devicetype = audience.DeviceType,
                @id = audience.Id
            });
            return true;
        }
        public Guid CreateToken(User audience, string tokenType = "Login")
        {
            DateTime? expiredOn = null;
            var sqlQuery = CreateTokenQuery();
            var accessTokenVal = Guid.NewGuid();
            using (var multi = _db.QueryMultipleAsync(sqlQuery, new
            {
                @accessTokenVal = accessTokenVal,
                @audienceId = audience.Id.ToString(),
                @roleType = CommonConstants.RoleUser,
                @expiredOnValue = expiredOn,
                @tokenType = tokenType
            }).Result)
            {
                return accessTokenVal;
            }
        }
        public AccessToken GetAccessToken(Guid token)
        {
            const string sql =
                @"SELECT Id, AccessTokenVal, AudienceId, RoleType, Expired, ExpiredOn, TokenType
                                 FROM AccessToken
                                 WHERE AccessTokenVal = @AccessToken ";

            var accessToken = _db.Query(sql, new { AccessToken = token });

            var accessTokens = accessToken as IList<dynamic> ?? accessToken.ToList();
            if (!accessTokens.Any()) return null;

            return new AccessToken
            {
                Id = accessTokens[0].Id,
                AccessTokenVal = Guid.Parse(accessTokens[0].AccessTokenVal),
                AudienceId = accessTokens[0].AudienceId,
                RoleType = accessTokens[0].RoleType,
                Expired = accessTokens[0].Expired,
                ExpiredOn = accessTokens[0].ExpiredOn,
                TokenType = accessTokens[0].TokenType
            };
        }
        public bool UpdateToken(AccessToken accessToken)
        {
            //if ExpiredOn is null then no need to update that on every request
            if (accessToken.ExpiredOn != null)
            {
                var expiredOn = "20";
                DateTime? expiredOnValue = null;
                if (!string.IsNullOrEmpty(expiredOn))
                    expiredOnValue = DateTime.UtcNow.AddMinutes(Convert.ToInt32(expiredOn));

                string sqlQuery = " UPDATE AccessToken SET ExpiredOn = @ExpiredOn WHERE Id = @Id ";
                _db.Execute(sqlQuery,
                    new
                    {
                        @ExpiredOn = expiredOnValue,
                        @Id = accessToken.Id,
                    });
            }
            return true;
        }
        public User GetUser(long userId)
        {
            var user = GetUserQuery();
            var userObj = _db.Query<User>(user, new
            {
                @id = userId
            }).FirstOrDefault();

            return userObj;
        }
        public bool ExpireClientToken(string audienceId, Guid accessToken)
        {
            var sqlQuery = GetUserLogoutQuery();
            _db.ExecuteScalarAsync(sqlQuery, new
            {
                @audienceId = audienceId,
                @accessToken = accessToken
            });
            return true;
        }
        public void CheckOAuthUserExistsAndCheckEmailExists(string oauthId, string emailId, out bool oAuthExists, out bool emailExists)
        {
            var sqlQuery = GetOAuthUserExistsAndEmailExistsQuery();
            using (var multi = _db.QueryMultipleAsync(sqlQuery,
              new { @emailId = emailId, @oAuthId = oauthId }).Result)
            {
                long emailIdCount = multi.ReadAsync<int>().Result.FirstOrDefault();
                long oauthIdCount = multi.ReadAsync<int>().Result.FirstOrDefault();
                oAuthExists = oauthIdCount > 0;
                emailExists = emailIdCount > 0;
            }
        }
        public User GetUserByEmail(string emailId)
        {
            var user = GetUserEmailQuery();
            var userObj = _db.Query<User>(user, new
            {
                @emailId = emailId
            }).FirstOrDefault();

            return userObj;
        }
        public void OAuthEmailUserUpdate(User audience)
        {
            var sqlQuery = OAuthEmailUserQuery();
            _db.Execute(sqlQuery, new
            {
                @authtype = audience.AuthType,
                @authid = audience.AuthId,
                @deviceId = audience.DeviceId,
                @devicetype = audience.DeviceType,
                @id = audience.Id
            });
        }
        public long CreateNewUser(User audienceCredentials)
        {
            var sqlQuery = CreateNewUserQuery();
            var userId = _db.QueryAsync<long>(sqlQuery, new
            {
                @email = audienceCredentials.Email,
                @passWord = audienceCredentials.Password,
                @photoUrl = audienceCredentials.PhotoUrl,
                @streetAddress = audienceCredentials.StreetAddress,
                @postalAddress = audienceCredentials.PostalAddress,
                @areaAddress = audienceCredentials.AreaAddress,
                @countryId = audienceCredentials.CountryId,
                @authType = audienceCredentials.AuthType,
                @authId = audienceCredentials.AuthId,
                @deviceType = audienceCredentials.DeviceType,
                @deviceId = audienceCredentials.DeviceId,
                @isVerified = audienceCredentials.IsVerified,
                @createdDate = audienceCredentials.CreatedDate,
                @modifiedDate = audienceCredentials.ModifiedDate,
                @userCode = audienceCredentials.UserCode
            }, null, null, CommandType.Text).Result;

            return userId.FirstOrDefault();
        }
        public bool ValidEmail(string emailId)
        {
            var sqlQuery = EmailIdExistQuery();
            _log.DebugFormat("Executing following validateuseremail query: {0}", sqlQuery);
            var emailExist = _db.ExecuteScalar<int>(sqlQuery, new
            {
                @emailId = emailId
            });
            return emailExist > 0;
        }
        public bool CreateNewRecoverCode(RecoverCode model)
        {
            var addCodeQuery = InsertRecoveryCodeQuery();
            var insertCode = _db.Execute(addCodeQuery, new
            {
                @userId = model.UserId,
                @recoverycode = model.RecoveryCode,
                @expiredon = model.ExpiredOn
            });
            return true;
        }
        public RecoverCode CheckValidRecoverCode(RecoveryCode model)
        {
            var sqlQuery = CheckValidRecoverCodeQuery();
            var recoverCode = _db.Query<RecoverCode>(sqlQuery, new
            {
                @userId = model.UserId,
                @recoveryCode = model.RecoverCode
            }).FirstOrDefault();
            return recoverCode;
        }
        public bool ChangePassword(User model, long recoverId)
        {
            var sqlQuery = ChangePasswordQuery();
            _db.Execute(sqlQuery, new
            {
                @password = model.Password,
                @modifiedDate = model.ModifiedDate,
                @userId = model.Id,
                @recoverCodeId = recoverId
            });
            return true;
        }
        public bool UpdateVerifyFlagForUser(User model)
        {
            var sqlQuery = UpdateVerifyFlagForUserQuery();
            _db.Execute(sqlQuery, new
            {
                @modifiedDate = model.ModifiedDate,
                @id = model.Id
            });
            return true;
        }
        public User GetUserDetailByIdAndEmail(long userId, string email)
        {
            var sqlQuery = GetUserDetailByIdAndEmailQuery();
            var userDetail = _db.Query<User>(sqlQuery, new
            {
                @userId = userId,
                @email = email
            }).FirstOrDefault();
            return userDetail;
        }
        public SessionUser GetWebUser(LoginModel model)
        {
            var sqlQuery = GetWebUserQuery();

            _log.DebugFormat("Executing following validate web user credential query: {0}", sqlQuery);

            using (var multi = _db.QueryMultipleAsync(sqlQuery, new
            {
                @email = model.Email,
                @password = _cryptoGraphy.EncryptString(model.Password)
            }).Result)
            {
                SessionUser webUser = multi.ReadAsync<SessionUser>().Result.FirstOrDefault();
                return webUser;
            }
        }

        public User GetUser(LoginModel model)
        {
            var sqlQuery = GetWebViewUserQuery();

            _log.DebugFormat("Executing following validate user credential query: {0}", sqlQuery);
            using (var multi = _db.QueryMultipleAsync(sqlQuery, new
            {
                @email = model.Email,
                @password = _cryptoGraphy.EncryptString(model.Password)
            }).Result)
            {
                User user = multi.ReadAsync<User>().Result.FirstOrDefault();
                return user;
            }
        }

        public bool ValidWebEmail(string emailId)
        {
            var sqlQuery = ValidWebEmailQuery();
            var isEmailExist = _db.ExecuteScalar<int>(sqlQuery, new
            {
                @email = emailId
            });
            return isEmailExist > 0;
        }
        public WebUser GetWebUserByEmail(string emailId)
        {
            var sqlQuery = GetWebUserByEmailQuery();
            var webUser = _db.Query<WebUser>(sqlQuery, new
            {
                @email = emailId
            }).FirstOrDefault();
            return webUser;
        }
        public bool CreateNewWebUserRecoverCode(WebUserRecoveryCode model)
        {
            var sqlQuery = CreateNewWebUserRecoverCodeQuery();
            _db.Execute(sqlQuery, new
            {
                @webUserId = model.WebUserId,
                @recoveryCode = model.RecoveryCode,
                @expiredOn = model.ExpiredOn
            });
            return true;
        }
        public WebUserRecoveryCode CheckValidWebRecoveryCode(RecoveryCode model)
        {
            var sqlQuery = CheckValidWebRecoveryCodeQuery();
            var recoveryCode = _db.Query<WebUserRecoveryCode>(sqlQuery, new
            {
                @webUserId = model.UserId,
                @recoveryCode = model.RecoverCode
            }).FirstOrDefault();
            return recoveryCode;
        }
        public bool ChangeWebPassword(WebUser model, long recoverCodeId)
        {
            var sqlQuery = ChangeWebPasswordQuery();
            _db.Execute(sqlQuery, new
            {
                @passWord = model.Password,
                @modifiedDate = model.ModifiedDate,
                @webUserId = model.Id,
                @webUserRecoveryCodeId = recoverCodeId
            });
            return true;
        }
        public User GetUserByIdAndPassword(long? userId, string oldPassword)
        {
            var sqlQuery = GetUserByIdAndPasswordQuery();
            var user = _db.Query<User>(sqlQuery, new
            {
                @userId = userId,
                @oldPassWord = oldPassword
            }).FirstOrDefault();
            return user;
        }
        private static string GetUserByIdAndPasswordQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM [User] WHERE Id=@userId AND Password=@oldPassWord AND Active=1;");
            return sqlQuery.ToString();
        }
        public bool UpdateUseroldPasswordToNewPassword(User model)
        {
            var sqlQuery = UpdateUseroldPasswordToNewPasswordQuery();
            _db.Execute(sqlQuery, new
            {
                @newPassword = model.Password,
                @modifiedDate = model.ModifiedDate,
                @id = model.Id
            });
            return true;
        }
        public bool CheckDuplicateEmail(long userId, string email)
        {
            var sqlQuery = CheckDuplicateEmailQuery();
            var isDuplicateEmail = _db.ExecuteScalar<int>(sqlQuery, new
            {
                @email = email,
                @userId = userId
            });
            return isDuplicateEmail > 0;
        }
        private static string CheckDuplicateEmailQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT COUNT(Id) FROM [User] WHERE Email=@email AND Id!=@userId;");
            return sqlQuery.ToString();
        }
        private static string UpdateUseroldPasswordToNewPasswordQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE [User] SET Password=@newPassword,ModifiedDate=@modifiedDate WHERE Id=@id;");
            return sqlQuery.ToString();
        }
        private static string ChangeWebPasswordQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE WebUser SET Password=@passWord,ModifiedDate=@modifiedDate WHERE Id=@webUserId;");
            sqlQuery.Append(@"UPDATE WebUserRecoveryCode SET Active=0 WHERE Id=@webUserRecoveryCodeId;");
            return sqlQuery.ToString();
        }
        private static string CheckValidWebRecoveryCodeQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM WebUserRecoveryCode WHERE WebUserId=@webUserId AND RecoveryCode=@recoveryCode AND Active=1;");
            return sqlQuery.ToString();
        }
        private static string CreateNewWebUserRecoverCodeQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO WebUserRecoveryCode(WebUserId,RecoveryCode,ExpiredOn)
                              VALUES(@webUserId,@recoveryCode,@expiredOn);");
            return sqlQuery.ToString();
        }
        private static string GetUserDetailByIdAndEmailQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM [User] WHERE Id=@userId AND Email=@email AND Active=1;");
            return sqlQuery.ToString();
        }
        private static string LoginUserQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM [User] WHERE Email=@email AND Password=@password AND Active=1;");
            return sqlQuery.ToString();
        }
        private static string UpdateDeviceDetailQuery()
        {
            var lstQuery = new StringBuilder();
            lstQuery.Append(@"UPDATE [User] SET DeviceId=@deviceId,DeviceType=@devicetype WHERE Id=@id");
            return lstQuery.ToString();
        }
        private static string CreateTokenQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO AccessToken (AccessTokenVal,AudienceId,RoleType,Expired,ExpiredOn,TokenType) VALUES (@accessTokenVal,@audienceId,@roleType,0,@expiredOnValue,@tokenType);");
            return sqlQuery.ToString();
        }
        private static string GetUserQuery()
        {
            return
                "SELECT * FROM [User] WHERE Id=@id AND Active=1;";
        }

        private string GetUserLogoutQuery()
        {
            return "UPDATE AccessToken SET  Expired = 1 WHERE AudienceId = @audienceId and AccessTokenVal = @accessToken;";
        }
        private static string GetOAuthUserExistsAndEmailExistsQuery()
        {
            var lstQueries = new StringBuilder();

            lstQueries.Append("SELECT COUNT(Id) FROM [User] WHERE [Email]=@emailId AND [Active]=1 AND IsVerified=1;");
            lstQueries.Append("SELECT COUNT(Id) FROM [User] WHERE [AuthId]=@oAuthId AND [Active]=1 AND IsVerified=1;");
            return lstQueries.ToString();
        }
        private static string GetUserEmailQuery()
        {
            return
                "SELECT * FROM [User] WHERE Email=@emailId AND Active=1;";
        }
        private static string OAuthEmailUserQuery()
        {
            var lstQuery = new StringBuilder();
            lstQuery.Append(@"UPDATE [User] SET AuthType=@authtype,AuthId=@authid,DeviceId=@deviceId,DeviceType=@devicetype WHERE Id=@id");
            return lstQuery.ToString();
        }
        private static string CreateNewUserQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO [User] (Email,Password,PhotoUrl,StreetAddress,PostalAddress,AreaAddress,CountryId,AuthType,AuthId,DeviceType,DeviceId,IsVerified,CreatedDate,ModifiedDate,UserCode)
                              VALUES(@email,@passWord,@photoUrl,@streetAddress,@postalAddress,@areaAddress,@countryId,@authType,@authId,@deviceType,@deviceId,@isVerified,@createdDate,@modifiedDate,@userCode); SELECT SCOPE_IDENTITY();");
            return sqlQuery.ToString();
        }
        private static string EmailIdExistQuery()
        {
            return @"SELECT count(Id) FROM [User] WHERE Email=@emailId AND Active=1;";
        }
        private static string InsertRecoveryCodeQuery()
        {
            var lstQuery = new StringBuilder();
            lstQuery.Append(@"INSERT INTO [RecoverCode] (UserId,RecoveryCode,ExpiredOn) VALUES(@userId,@recoverycode,@expiredon)");
            return lstQuery.ToString();
        }
        private static string CheckValidRecoverCodeQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM RecoverCode WHERE UserId=@userId AND RecoveryCode=@recoveryCode AND Active=1;");
            return sqlQuery.ToString();
        }
        private static string ChangePasswordQuery()
        {
            var lstQuery = new StringBuilder();
            lstQuery.Append(@"UPDATE [User] SET Password=@password,ModifiedDate=@modifiedDate WHERE Id=@userId;");
            lstQuery.Append(@"UPDATE RecoverCode SET Active=0 WHERE Id=@recoverCodeId;");
            return lstQuery.ToString();
        }
        private static string UpdateVerifyFlagForUserQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE [User] SET IsVerified=1,ModifiedDate=@modifiedDate WHERE Id=@id;");
            return sqlQuery.ToString();
        }
        private static string GetWebUserQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM WebUser WHERE Email=@email AND Password=@password AND Active=1 AND Deleted=0;");
            return sqlQuery.ToString();
        }
        private static string GetWebViewUserQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM [User] WHERE Email=@email AND Password=@password AND Active=1 AND IsVerified=1;");
            return sqlQuery.ToString();
        }

        private static string ValidWebEmailQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT COUNT(Id) FROM WebUser WHERE Email=@email AND Active=1 AND Deleted=0;");
            return sqlQuery.ToString();
        }
        private static string GetWebUserByEmailQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM WebUser WHERE Email=@email AND Active=1 AND Deleted=0;");
            return sqlQuery.ToString();
        }
        public User GetUserByBarCode(string barCode)
        {
            var sqlQuery = GetUserByBarCodeQuery();

            var userByBarCode = _db.Query<User>(sqlQuery, new
            {
                @userCode = barCode
            }).FirstOrDefault();

            return userByBarCode;

        }
        private static string GetUserByBarCodeQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM [User] WHERE UserCode=@userCode;");
            return sqlQuery.ToString();
        }
    }
}
