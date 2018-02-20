using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;

namespace Lifvs.Common.Utility.Interfaces
{
    public interface ISetupUser
    {
        void SetupUserDetail(int UserId, SessionUser user);
        SessionUser GetUserData();
        void SetupUserDetail(int intUserId, User inputParameters);
    }
}
