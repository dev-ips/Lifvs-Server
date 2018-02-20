using Lifvs.Common.Helpers;
using System;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Lifvs.Common.Filters
{
    public abstract class SecurityAuthentication : Attribute, IAuthenticationFilter
    {
        public bool AllowMultiple
        {
            get { return false; }
        }
        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;
            var queryParams = request.RequestUri.ParseQueryString();

            if (authorization == null)
            {
                // No authentication was attempted (for this authentication method).
                // Do not set either Principal (which would indicate success) or ErrorResult (indicating an error).
                return;
            }

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Missing credentials.", request);
                return;
            }

            //This will be executed by the derived class.
            //We do this so that we can switch the authentication attributes for different controllers
            IPrincipal principal = AuthenticateAsync(authorization.Parameter, context, cancellationToken);

            if (principal == null)
            {
                // Authentication was attempted but failed. Set ErrorResult to indicate an error.
                context.ErrorResult = new AuthenticationFailureResult("Invalid token. Token is either expired or invalid.", request);
            }
            else
            {
                // Authentication was attempted and succeeded. Set Principal to the authenticated user.
                context.Principal = principal;
            }
        }

        protected abstract IPrincipal AuthenticateAsync(string authorizationParameter, HttpAuthenticationContext context, CancellationToken cancellationToken);

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
