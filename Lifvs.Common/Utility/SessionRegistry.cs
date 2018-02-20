using Lifvs.Common.ApiModels;
using Lifvs.Common.DataModels;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Security;

namespace Lifvs.Common.Utility
{
    public class SessionRegistry
    {
        public static void SetUserDetail(int intUserId, SessionUser inputParameters)
        {
            var userData = JsonConvert.SerializeObject(inputParameters);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, "Lifvs", DateTime.Now, DateTime.Now.AddDays(1), true, userData, FormsAuthentication.FormsCookiePath);
            string hash = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        public static SessionUser GetUserData()
        {
            if (HttpContext.Current.User != null)
            {
                if (HttpContext.Current.User.Identity.AuthenticationType == "Forms")
                {
                    FormsIdentity formsIdentity = null;
                    formsIdentity = (FormsIdentity)HttpContext.Current.User.Identity;
                    string str = formsIdentity.Ticket.UserData;
                    var userData = JsonConvert.DeserializeObject<SessionUser>(str);
                    return userData;
                }
            }
            return null;
        }

        public static void SetUserDetail(int intUserId, User inputParameters)
        {
            var userData = JsonConvert.SerializeObject(inputParameters);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, "Lifvs", DateTime.Now, DateTime.Now.AddDays(1), true, userData, FormsAuthentication.FormsCookiePath);
            string hash = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
    }
}
