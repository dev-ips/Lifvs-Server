using Dapper;
using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories
{
    public class UserCodeRepository : IUserCodeRepository
    {
        public readonly ILog _log;
        private readonly IDbConnection _db;

        public UserCodeRepository(ILog log, IDbConnection db)
        {
            _log = log;
            _db = db;
        }

        public GetUserCodeModel GenerateUserCode(long userId, long storeId)
        {
            var data = GenerateCode(userId, storeId);
            return data;
        }

        private GetUserCodeModel GenerateCode(long userId, long storeId)
        {
            var data = GetExistingUserCode(userId, storeId);
            GetUserCodeModel userCodeModel = new GetUserCodeModel();

            if (data != null && !string.IsNullOrWhiteSpace(data.Code))
            {
                // check code is active
                if (data.ExpiryDate <= DateTime.Now)
                {
                    var sqlQuery = GetUpdateUserCodeQuery(data.UserId, data.StoreId, data.CreatedDate);
                    _db.Query(sqlQuery);

                    userCodeModel.ErrorMessage = string.Format("Koden har löpt ut! Försök efter {0} dag, {1}:{2} timmar.", data.ExpiryDate.AddDays(1).Subtract(DateTime.Now).Days, data.ExpiryDate.AddDays(1).Subtract(DateTime.Now).Hours, data.ExpiryDate.AddDays(1).Subtract(DateTime.Now).Minutes);
                    return userCodeModel;
                }
                else
                {
                    userCodeModel.Code = data.Code;
                    userCodeModel.IsActive = data.IsActive;
                    return userCodeModel;
                }
            }
            else
            {
                // generate code..
                var sqlQuery = GetGenerateUserCodeQuery(userId, storeId);
                _db.Query(sqlQuery, new
                {
                    @userId = userId,
                    @storeId = storeId,
                    @code = CommonConstants.GenerateRandomCode(),
                    @createdDate = DateTime.Now,
                    @expiryDate = DateTime.Now.AddMinutes(CommonConstants.UserCodeExpireTime),
                    @isActive = true
                });

                var model = GetExistingUserCode(userId, storeId);
                if (model != null)
                {
                    userCodeModel.Code = model.Code;
                    userCodeModel.IsActive = model.IsActive;
                    return userCodeModel;
                }
                else
                {
                    userCodeModel.ErrorMessage = "Error vid försök att få ny kod. Vänligen prova igen.";
                    return userCodeModel;
                }
            }
        }

        private UserCodeModel GetExistingUserCode(long userId, long storeId)
        {
            var sqlQuery = GetUserCodeQuery(userId, storeId);
            UserCodeModel userCodeModel = _db.Query<UserCodeModel>(sqlQuery).FirstOrDefault();
            return userCodeModel;
        }

        private static string GetUserCodeQuery(long userId, long storeId)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"SELECT TOP 1 * FROM UserCode WHERE CAST(CreatedDate AS DATE)= '{0}' and UserId = {1} AND StoreId = {2} ORDER BY CreatedDate DESC", DateTime.Now.Date, userId, storeId));
            return sqlQuery.ToString();
        }

        private static string GetGenerateUserCodeQuery(long userId, long storeId)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO UserCode(UserId, StoreId, Code, CreatedDate, ExpiryDate, IsActive) VALUES(@userId,@storeId,@code,@createdDate,@expiryDate,@isActive)");
            return sqlQuery.ToString();
        }

        private static string GetUpdateUserCodeQuery(long userId, long storeId, DateTime createdDate)
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(string.Format(@"UPDATE UserCode SET IsActive='{0}' WHERE CreatedDate='{1}' and UserId={2} and StoreId={3} ", false, createdDate.ToString("MM/dd/yyyy HH:mm:ss.fff"), userId, storeId));
            return sqlQuery.ToString();
        }
    }
}
