using Lifvs.Common.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Services.Interfaces
{
    public interface IUserCodeService
    {
        GetUserCodeModel GenerateUserCode(long userId, long storeId);
    }
}
