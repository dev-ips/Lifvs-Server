using System.Web.Mvc;

namespace Lifvs.Areas.CustomerArea
{
    public class CustomerAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Customer";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
               "Customer_default",
               "Customer/{controller}/{action}/{id}",
               defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional },
               namespaces: new[] { "Lifvs.Areas.Customer.Controllers" }
            );
        }
    }
}