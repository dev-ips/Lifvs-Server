using Lifvs.Common.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Services.Interfaces
{
    public interface IUserService
    {
        bool AddUserCardDetails(UserCardDetailModel model);
        bool UpdateUserCardDetails(UserCardDetailModel model);
        UserCardDetailModel GetUserCardDetails(long userCardDetailId);
        List<UserViewModel> GetUsers();
        UserValidCardResponse IsValidCard(long userId);
        UserProfileViewModel GetUserProfile(long userId);
        bool ChangeProfile(long userId, UserProfileViewModel model);
    }
}
