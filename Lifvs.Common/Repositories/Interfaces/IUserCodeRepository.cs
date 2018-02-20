using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface IUserCodeRepository
    {
        GetUserCodeModel GenerateUserCode(long userId, long storeId);
    }
}
