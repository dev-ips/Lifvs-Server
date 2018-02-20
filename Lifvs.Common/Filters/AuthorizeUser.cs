using Lifvs.Common.Helpers;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Lifvs.Common.Filters
{
    public class AuthorizeUser : AuthorizeAttribute
    {
        /// <summary>
        /// On Authorization
        /// </summary>
        /// <param name="actionContext">action Context</param>
        /// <returns></returns>
        public override void OnAuthorization(HttpActionContext actionContext)
        {

            string sid = null;
            string actor = null;
            var requestScope = actionContext.Request.GetDependencyScope();

            // Resolve the service you want to use.
            var exception = requestScope.GetService(typeof(IExceptionManager)) as IExceptionManager ?? new ExceptionManager();

            var identity = (ClaimsPrincipal)Thread.CurrentPrincipal;
            // Get the claims values
            if (identity != null)
            {
                sid = identity.Claims.Where(c => c.Type == ClaimTypes.Sid)
                    .Select(c => c.Value).SingleOrDefault();
                actor = identity.Claims.Where(c => c.Type == ClaimTypes.Actor)
                    .Select(c => c.Value).SingleOrDefault();
            }
            var dictRouteData = actionContext.Request.GetRouteData().Values;

            // This is code for when managerId is logged in user

            //-This is code for only users
            {
                if (dictRouteData.ContainsKey("userId"))
                {
                    var routeDataClientId = dictRouteData["userId"].ToString();
                    if (!string.Equals(sid, routeDataClientId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //return;
                        throw exception.ThrowException(HttpStatusCode.Unauthorized, "", "Authorization failed.");
                    }
                }
            }

            base.OnAuthorization(actionContext);
        }
    }
}
