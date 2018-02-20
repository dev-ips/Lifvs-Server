using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using System.Collections.Generic;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface IUserRepository
    {
        bool isEmailExist(string email);
        long AddUserCardDetails(UserCardDetails model);
        UserCardDetails GetUserCardDetailsByUserId(long userId);
        bool UpdateUserCardDetails(UserCardDetails model);
        bool UpdateUserCardData(UserCardDetails model);
        UserCardDetails GetCradDetailsById(long cardDetailId);
        UserCardDetails GetUserCardDetailByCardNumber(string cardNumber, long userId);
        List<UserViewModel> GetUsers();
        bool ChangeUserProfile(User model);
    }
}
