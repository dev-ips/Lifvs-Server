using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Utility.Interfaces
{
    public interface IEmailNotifier
    {
        void SendEmail(string to, string body, string subject);
    }
}
