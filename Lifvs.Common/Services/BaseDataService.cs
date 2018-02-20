using Lifvs.Common.Services.Interfaces;
using log4net;
using System.Configuration;

namespace Lifvs.Common.Services
{
    public class BaseDataService : IBaseDataService
    {
        private readonly ILog _log;
        public BaseDataService(ILog log)
        {
            _log = log;
        }
        public string GetWelcomeMessage()
        {
            var welcomeMessage = ConfigurationManager.AppSettings["WelcomeMessage"];
            return welcomeMessage;
        }
    }
}
