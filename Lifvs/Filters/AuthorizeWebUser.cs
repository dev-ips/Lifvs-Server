using Lifvs.Alerts;
using Lifvs.Common.Utility;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Lifvs.Filters
{
    public class AuthorizeWebUser : AuthorizeAttribute
    {
        private readonly int[] allowedRoles;
        public AuthorizeWebUser(params int[] roles)
        {
            this.allowedRoles = roles;
        }
        protected override bool AuthorizeCore(HttpContextBase filterContext)
        {
            bool authorize = false;
            foreach(var role in allowedRoles)
            {
                var user = SessionRegistry.GetUserData();
                if (user != null && user.RoleId == role)
                {
                    authorize = true;
                }
            }
            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var user = SessionRegistry.GetUserData();
            if (user == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    {"action","Index" },
                    {"controller","Login"}
                }).WithError("Please login to continue");
            }
            else
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "action","Index"},
                    {"controller","Dashboard" }
                }).WithError("You are not authorized to access this page.");
            }
        }
    }
}