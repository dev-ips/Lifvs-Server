using Lifvs.App_Start;
using StackExchange.Profiling;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Lifvs
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            IoCConfig.RegisterDependencies();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        private bool IsUserAllowedToSeeMiniProfilerUI(HttpRequest httpRequest)
        {
            // Implement your own logic for who 
            // should be able to access ~/mini-profiler-resources/results-index
            //var principal = httpRequest.RequestContext.HttpContext.User;
            //return principal.IsInRole("Developer");
            return httpRequest.QueryString.AllKeys.Contains("showProfiler");
        }

        protected void Application_BeginRequest()
        {
            MiniProfiler.Settings.Results_Authorize = IsUserAllowedToSeeMiniProfilerUI;
            MiniProfiler.Settings.Results_List_Authorize = IsUserAllowedToSeeMiniProfilerUI;
            MiniProfiler.Start();
        }

        protected void Application_EndRequest()
        {
            MiniProfiler.Stop();
        }
    }
}
