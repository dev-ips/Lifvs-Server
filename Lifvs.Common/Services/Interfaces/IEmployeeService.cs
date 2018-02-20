using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Services.Interfaces
{
    public interface IEmployeeService
    {
        List<EmployeeViewModel> GetEmployees();
        void SendInvitation(InvitationModel model, long userId);
        WebUserInvitation GetWebUserInvitation(int id);
        long CreateEmployee(WebUser model);
        void UpdateWebUserInvitation(SignUpModel model, long userId);
        bool UpdateEmployeeRole(EmployeeViewModel model, long userId);
        void DeleteEmployee(long userId);
    }
}
