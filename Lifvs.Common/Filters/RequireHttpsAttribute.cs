using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Lifvs.Common.Filters
{
    public class RequireHttpsAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            {
                base.OnAuthorization(actionContext);
            }
        }
    }
}
