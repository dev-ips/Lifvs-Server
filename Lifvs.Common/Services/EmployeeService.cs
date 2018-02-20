using Lifvs.Common.Helpers;
using Lifvs.Common.Repositories.Interfaces;
using Lifvs.Common.Services.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Lifvs.Common.Utility.Interfaces;
using System.Collections.Specialized;
using System.Configuration;
using Lifvs.Common.Utility;

namespace Lifvs.Common.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly ILog _log;
        private readonly IExceptionManager _exception;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAccessTokenRepository _accessTokenRepository;
        private readonly IFileManager _fm;
        private readonly IPathMapper _path;
        private readonly IEmailNotifier _emailNotifier;
        public EmployeeService(ILog log, IExceptionManager exception, IEmployeeRepository employeeRepository, IAccessTokenRepository accessTokenRepository, IFileManager fm, IPathMapper path, IEmailNotifier emailNotifier)
        {
            _log = log;
            _exception = exception;
            _employeeRepository = employeeRepository;
            _accessTokenRepository = accessTokenRepository;
            _fm = fm;
            _path = path;
            _emailNotifier = emailNotifier;
        }

        public List<EmployeeViewModel> GetEmployees()
        {
            var roleIds = new List<int> { (int)Enums.Enums.Roles.Admin, (int)Enums.Enums.Roles.Employee };
            var employees = _employeeRepository.GetEmployees(roleIds);
            return employees;
        }
        public void SendInvitation(InvitationModel model, long userId)
        {
            var checkEmail = _accessTokenRepository.GetWebUserByEmail(model.Email);
            if (checkEmail != null)
                throw new Exception("L'identificador de correu electrònic ja existeix.");

            var invitationId = InsertWebUserInvitation(model, userId);
            var webUrl = ConfigurationManager.AppSettings["WebUrl"];
            var fields = new StringDictionary
            {
                {"signUpUrl",string.Format("{0}{1}{2}",webUrl,"/Employee/Invitation/",invitationId ) }
            };
            var htmlBody = _fm.ReadFileContents(GetMailerTemplatePath("html", "CreateEmployeePage")).ReplaceMatch(fields);
            _emailNotifier.SendEmail(model.Email, htmlBody, "Invitation");
        }
        public long InsertWebUserInvitation(InvitationModel model, long userId)
        {
            var checkExistInvite = _employeeRepository.GetWebUserInvitationByEmail(model.Email);
            if (checkExistInvite != null)
                throw new Exception("Kontot finns redan.");
            var webUserInvitation = WebUserInvitationMapper(model, userId);
            var invitationId = _employeeRepository.InsertWebUserInvitation(webUserInvitation);
            return invitationId;
        }
        public WebUserInvitation GetWebUserInvitation(int id)
        {
            var webUserInvitation = _employeeRepository.GetWebUserInvitation(id);
            if (webUserInvitation == null)
                throw new Exception("Ogiltig förfrågan.");
            return webUserInvitation;
        }
        public long CreateEmployee(WebUser model)
        {
            var employeeId = _employeeRepository.InsertEmployee(model);
            return employeeId;
        }
        public void UpdateWebUserInvitation(SignUpModel model, long userId)
        {
            var webUserInvitation = WebUserInvitationUpdateModelMapper(model, userId);
            _employeeRepository.UpdateWebUserInvitation(webUserInvitation);
        }
        public bool UpdateEmployeeRole(EmployeeViewModel model, long userId)
        {
            var webUserModel = UpdateEmployeeRoleModelMapper(model, userId);
            return _employeeRepository.UpdateEmployeeRole(webUserModel);
        }
        public void DeleteEmployee(long userId)
        {
            _employeeRepository.DeleteEmployee(userId);
        }
        private string GetMailerTemplatePath(string text, string templateName)
        {
            var path = _path.MapPath(string.Format("mailtemplate/{0}/{1}.{2}", "default", templateName, text));

            return path;
        }
        private WebUserInvitation WebUserInvitationMapper(InvitationModel model, long userId)
        {
            return new WebUserInvitation
            {
                Email = model.Email,
                RoleId = model.RoleId.Value,
                UserId = null,
                InvitationApprovedDate = null,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };
        }
        private WebUserInvitation WebUserInvitationUpdateModelMapper(SignUpModel model, long userId)
        {
            return new WebUserInvitation
            {
                UserId = userId,
                InvitationApprovedDate = DateTime.Now,
                Id = model.InvitationId
            };
        }
        private WebUser UpdateEmployeeRoleModelMapper(EmployeeViewModel model, long userId)
        {
            return new WebUser
            {
                Id = model.Id,
                RoleId = model.RoleId,
                ModifiedBy = userId,
                ModifiedDate = DateTime.Now
            };
        }
    }
}
