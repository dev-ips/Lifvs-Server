using Lifvs.Common.Repositories.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lifvs.Common.ApiModels;
using Dapper;
using Lifvs.Common.DataModels;
using Lifvs.Common.Utility.Interfaces;

namespace Lifvs.Common.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnection _db;
        private readonly ILog _log;
        private readonly ICryptoGraphy _cryptoGraphy;
        public EmployeeRepository(IDbConnection db, ILog log, ICryptoGraphy cryptoGraphy)
        {
            _db = db;
            _log = log;
            _cryptoGraphy = cryptoGraphy;
        }
        public List<EmployeeViewModel> GetEmployees(List<int> roleIds)
        {
            var sqlQuery = GetEmployeesQuery();
            var employees = _db.Query<EmployeeViewModel>(sqlQuery, new
            {
                @roleIds = roleIds.ToArray()
            }).ToList();
            return employees;
        }
        public WebUserInvitation GetWebUserInvitationByEmail(string email)
        {
            var sqlQuery = GetWebUserInvitationByEmailQuery();
            var webUserInvitation = _db.Query<WebUserInvitation>(sqlQuery, new
            {
                @email = email
            }).FirstOrDefault();
            return webUserInvitation;
        }
        public long InsertWebUserInvitation(WebUserInvitation model)
        {
            var sqlQuery = InsertWebUserInvitationQuery();
            var invitationId = _db.ExecuteScalar<long>(sqlQuery, new
            {
                @email = model.Email,
                @roleId = model.RoleId,
                @createdDate = model.CreatedDate,
                @createdBy = model.CreatedBy
            });
            return invitationId;
        }
        public WebUserInvitation GetWebUserInvitation(int id)
        {
            var sqlQuery = GetWebUserInvitationQuery();
            var webUserInvitation = _db.Query<WebUserInvitation>(sqlQuery, new
            {
                @id = id
            }).FirstOrDefault();
            return webUserInvitation;
        }
        public long InsertEmployee(WebUser model)
        {
            var sqlQuery = InsertEmployeeQuery();
            var webUserId = _db.ExecuteScalar<long>(sqlQuery, new
            {
                @email = model.Email,
                @password = string.IsNullOrEmpty(model.Password) ? null : _cryptoGraphy.EncryptString(model.Password),
                @name = model.Name,
                @additionalinformation = model.AdditionalInformation,
                @roleId = model.RoleId,
                @createdby = model.CreatedBy,
                @createddate = DateTime.Now,
                @modifiedby = model.ModifiedBy,
                @modifieddate = DateTime.Now
            });
            return webUserId;
        }
        public void UpdateWebUserInvitation(WebUserInvitation model)
        {
            var sqlQuery = UpdateWebUserInvitationQuery();
            _db.Execute(sqlQuery, new
            {
                @userId = model.UserId,
                @invitationApprovedDate = model.InvitationApprovedDate,
                @id = model.Id
            });
        }
        public bool UpdateEmployeeRole(WebUser model)
        {
            var sqlQuery = UpdateEmployeeRoleQuery();
            _db.Execute(sqlQuery, new
            {
                @roleId = model.RoleId,
                @modifiedBy = model.ModifiedBy,
                @modifiedDate = model.ModifiedDate,
                @id = model.Id
            });
            return true;
        }
        public void DeleteEmployee(long userId)
        {
            var sqlQuery = DeleteEmployeeQuery();
            _db.Execute(sqlQuery, new
            {
                @id = userId
            });
        }
        private static string GetEmployeesQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"WITH E AS
                              (
                                    SELECT ROW_NUMBER()OVER(ORDER BY Id) as RowNum,Id,Email,RoleId,CASE RoleId WHEN 1 THEN 'Super Admin' WHEN 2 THEN 'Admin' WHEN 3 THEN 'Employee' END as CurrentRole,
                                    COUNT(1)OVER() as ResultsCount 
                                    FROM WebUser
                              )
                              SELECT * FROM E
                              WHERE E.RoleId IN @roleIds;");
            return sqlQuery.ToString();
        }
        private static string GetWebUserInvitationByEmailQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM WebUserInvitation WHERE Email=@email;");
            return sqlQuery.ToString();
        }
        private static string InsertWebUserInvitationQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO WebUserInvitation(Email,RoleId,CreatedDate,CreatedBy)
                              VALUES(@email,@roleId,@createdDate,@createdBy); SELECT SCOPE_IDENTITY();");
            return sqlQuery.ToString();
        }
        private static string GetWebUserInvitationQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"SELECT * FROM WebUserInvitation WHERE Id=@id;");
            return sqlQuery.ToString();
        }
        private static string InsertEmployeeQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"INSERT INTO WebUser (Email,Password,Name,AdditionalInformation,RoleId,Active,Deleted,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate) 
                                VALUES(@email,@password,@name,@additionalinformation,@roleId,1,0,@createdby,@createddate,@modifiedby,@modifieddate); SELECT SCOPE_IDENTITY();");
            return sqlQuery.ToString();
        }
        private static string UpdateWebUserInvitationQuery()
        {
            var updateWebUserInvitationQuery = new StringBuilder();
            updateWebUserInvitationQuery.Append(@"UPDATE WebUserInvitation SET IsInvitationAccepted=1,UserId=@userId,InvitationApprovedDate=@invitationApprovedDate WHERE Id=@id;");
            return updateWebUserInvitationQuery.ToString();
        }
        private static string UpdateEmployeeRoleQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"UPDATE WebUser SET RoleId=@roleId,ModifiedBy=@modifiedBy,ModifiedDate=@modifiedDate WHERE Id=@id;");
            return sqlQuery.ToString();
        }
        private static string DeleteEmployeeQuery()
        {
            var sqlQuery = new StringBuilder();
            sqlQuery.Append(@"DELETE FROM WebUser WHERE Id=@id;");
            return sqlQuery.ToString();
        }
    }
}
