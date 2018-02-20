using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Services
{
    public class UserCodeService : IUserCodeService
    {
        private readonly ILog _log;
        private readonly IUserCodeRepository _userCodeRepository;
        private readonly IExceptionManager _exception;

        public UserCodeService(ILog log, IUserCodeRepository userCodeRepository,IExceptionManager exception)
        {
            _log = log;
            _userCodeRepository = userCodeRepository;
            _exception = exception;
        }

        public GetUserCodeModel GenerateUserCode(long userId, long storeId)
        {
            return _userCodeRepository.GenerateUserCode(userId, storeId);
        }
    }
}
