using Lifvs.Common.ApiModels;
using System.Security.Claims;
using System.Security.Principal;

namespace Lifvs.Common.Helpers
{
    public class SecurityHelper
    {
        public static IPrincipal CreateAndGetPrincipal(string name, Audience audience)
        {
            var identity = new GenericIdentity(name, "AccessToken");
            identity.AddClaim(new Claim(ClaimTypes.Sid, audience.AudienceId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Email, audience.Email ?? string.Empty));
            identity.AddClaim(new Claim(ClaimTypes.Actor, "User"));
            string[] roles = new string[1];
            roles[0] = "User";
            IPrincipal principal = new GenericPrincipal(identity, roles);

            return principal;
        }
    }
}
