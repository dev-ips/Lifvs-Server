using Dapper;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Utility.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _db;
        private readonly ILog _log;
        private readonly ICryptoGraphy _cryptoGraphy;
        public UserRepository(IDbConnection db, ILog log, ICryptoGraphy cryptoGraphy)
        {
            _db = db;
            _log = log;
            _cryptoGraphy = cryptoGraphy;
        }
        public bool isEmailExist(string email)
        {
            var sqlQuery = isEmailExistQuery();
            _log.DebugFormat("Executing following validate user email query: {0}", sqlQuery);
            var userCount = _db.ExecuteScalar(sqlQuery, new
            {
                @email = email
            }).ToString();
            return userCount == "0";
        }
        public long AddUserCardDetails(UserCardDetails model)
        {
            var sqlQuery = AddUserCardDetailsQuery();
            var userCardDetailId = _db.ExecuteScalar<long>(sqlQuery, new
            {
                @userId = model.UserId,
                @cardNumber = model.CardNumber,
                @expiredMonth = model.ExpiredMonth,
                @expiredYear = model.ExpiredYear,
                @cvc = model.CVC,
                @creditCardId = model.CreditCardId,
                @phoneNumber = model.PhoneNumber,
                @createdDate = model.CreatedDate,
                @modifiedDate = model.ModifiedDate
            });
            return userCardDetailId;
        }
        public UserCardDetails GetUserCardDetailsByUserId(long userId)
        {
            var sqlQuery = GetUserCardDetailsByUserIdQuery();
            var userCardDetails = _db.Query<UserCardDetails>(sqlQuery, new
            {
                @userId = userId
            }).FirstOrDefault();

            return userCardDetails;
        }
        public UserCardDetails GetUserCardDetailByCardNumber(string cardNumber, long userId)
        {
            var sqlQuery = GetUserCardDetailByCardNumberQuery();
            var userCardDetails = _db.Query<UserCardDetails>(sqlQuery, new
            {
                @cardNumber = _cryptoGraphy.EncryptString(cardNumber),
                @userId = userId
            }).FirstOrDefault();
            return userCardDetails;
        }
        public UserCardDetails GetCradDetailsById(long cardDetailId)
        {
            var sqlQuery = GetCradDetailsByIdQuery();
            var userCardDetail = _db.Query<UserCardDetails>(sqlQuery, new
            {
                @cardDetailId = cardDetailId
            }).FirstOrDefault();

            return userCardDetail;
        }
        public bool UpdateUserCardDetails(UserCardDetails model)
        {
            var sqlQuery = UpdateUserCardDetailsQuery();
            _db.Execute(sqlQuery, new
            {
                @userId = model.UserId,
                @cardNumber = model.CardNumber,
                @cardType = model.CardType,
                @expiredMonth = model.ExpiredMonth,
                @expiredYear = model.ExpiredYear,
                @cvc = model.CVC,
                @creditCardId = model.CreditCardId,
                @phoneNumber = model.PhoneNumber,
                @modifiedDate = model.ModifiedDate,
                @id = model.Id
            });
            return true;
        }
        public bool UpdateUserCardData(UserCardDetails model)
        {
            var sqlQuery = UpdateUserCardDataQuery();
            _db.Execute(sqlQuery, new
            {
                @userId = model.UserId,
                @expiredMonth = model.ExpiredMonth,
                @expiredYear = model.ExpiredYear,
                @cvc = model.CVC,
                @creditCardId = model.CreditCardId,
                @phoneNumber = model.PhoneNumber,
                @modifiedDate = model.ModifiedDate,
                @id = model.Id
            });
            return true;
        }
        public List<UserViewModel> GetUsers()
        {
            var sqlQuery = GetUsersQuery();
            var users = _db.Query<UserViewModel>(sqlQuery).ToList();
            return users;
        }
        public bool ChangeUserProfile(User model)
        {
            var sqlQuery = ChangeUserProfileQuery();
            _db.Execute(sqlQuery, new
            {
                @streetAddress = model.StreetAddress,
                @postalAddress = model.PostalAddress,
                @areaAddress = model.AreaAddress,
                @countryId = model.CountryId,
                @userId = model.Id
            });
            return true;
        }
        private static string ChangeUserProfileQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE [User] SET StreetAddress=@streetAddress,PostalAddress=@postalAddress,AreaAddress=@areaAddress,CountryId=@countryId WHERE Id=@userId;");
            return sqlQuery.ToString();
        }
        private static string isEmailExistQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT COUNT(Id) FROM [User] WHERE Email=@email;");
            return sqlQuery.ToString();
        }
        private static string AddUserCardDetailsQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO UserCardDetails(UserId,CardNumber,ExpiredMonth,ExpiredYear,CVC,CreditCardId,PhoneNumber,CreatedDate,ModifiedDate)
                              VALUES(@userId,@cardNumber,@expiredMonth,@expiredYear,@cvc,@creditCardId,@phoneNumber,@createdDate,@modifiedDate); SELECT SCOPE_IDENTITY();");
            return sqlQuery.ToString();
        }
        private static string GetCradDetailsByIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM UserCardDetails WHERE Id=@cardDetailId;");
            return sqlQuery.ToString();
        }
        private static string GetUserCardDetailsByUserIdQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM UserCardDetails WHERE UserId=@userId;");
            return sqlQuery.ToString();
        }
        private static string GetUserCardDetailByCardNumberQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM UserCardDetails WHERE CardNumber=@cardNumber AND UserId!=@userId;");
            return sqlQuery.ToString();
        }
        private static string UpdateUserCardDetailsQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE UserCardDetails SET UserId=@userId,CardNumber=@cardNumber,CardType=@cardType,ExpiredMonth=@expiredMonth,ExpiredYear=@expiredYear,CVC=@cvc,CreditCardId=@creditCardId,PhoneNumber=@phoneNumber,ModifiedDate=@modifiedDate WHERE Id=@id;");
            return sqlQuery.ToString();
        }
        private static string UpdateUserCardDataQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE UserCardDetails SET UserId=@userId,ExpiredMonth=@expiredMonth,ExpiredYear=@expiredYear,CVC=@cvc,CreditCardId=@creditCardId,PhoneNumber=@phoneNumber,ModifiedDate=@modifiedDate WHERE Id=@id;");
            return sqlQuery.ToString();
        }
        private static string GetUsersQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"WITH U AS
                              (SELECT ROW_NUMBER()OVER(ORDER BY Id) as RowNum,Id,Email,DeviceType,
                               COUNT(1)OVER() as ResultCount
                               FROM [User])
                            SELECT * FROM U;");
            return sqlQuery.ToString();
        }
    }
}
