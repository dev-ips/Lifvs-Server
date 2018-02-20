using Lifvs.Common.ApiModels;
using Lifvs.Common.Helpers;
using Lifvs.Common.Services.Interfaces;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Filters;

namespace Lifvs.Common.Filters
{
    public class AccessTokenAuthenticationAttribute : SecurityAuthentication
    {
        //In IOCConfig we use.PropertiesAutoWired to populate this.
        public IAccessTokenService AccessTokenService { get; set; }

        protected override IPrincipal AuthenticateAsync(string authorizationParameter, HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var requestScope = context.Request.GetDependencyScope();

            // Resolve the service you want to use.
            var service = requestScope.GetService(typeof(IAccessTokenService)) as IAccessTokenService;

            Audience audience;
            if (!service.TryValidateToken(authorizationParameter, out audience)) return null;

            cancellationToken.ThrowIfCancellationRequested();

            IPrincipal principal = SecurityHelper.CreateAndGetPrincipal(authorizationParameter, audience);

            return principal;
        }
    }
}
