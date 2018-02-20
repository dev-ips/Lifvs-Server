using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Utility.Interfaces;

namespace Lifvs.Common.Utility
{
    public class SetupUser : ISetupUser
    {
        public SetupUser()
        {

        }
        public void SetupUserDetail(int intUserId, SessionUser inputParameters)
        {
            SessionRegistry.SetUserDetail(intUserId, inputParameters);
        }

        public SessionUser GetUserData()
        {
            return SessionRegistry.GetUserData();
        }

        public void SetupUserDetail(int intUserId, User inputParameters)
        {
            SessionRegistry.SetUserDetail(intUserId, inputParameters);
        }
    }
}
