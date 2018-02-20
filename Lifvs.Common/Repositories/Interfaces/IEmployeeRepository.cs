using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using System.Collections.Generic;

namespace Lifvs.Common.Repositories.Interfaces
{
    public interface IEmployeeRepository
    {
        List<EmployeeViewModel> GetEmployees(List<int> roleIds);
        WebUserInvitation GetWebUserInvitationByEmail(string email);
        long InsertWebUserInvitation(WebUserInvitation model);
        WebUserInvitation GetWebUserInvitation(int id);
        long InsertEmployee(WebUser model);
        void UpdateWebUserInvitation(WebUserInvitation model);
        bool UpdateEmployeeRole(WebUser model);
        void DeleteEmployee(long userId);
    }
}
