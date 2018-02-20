using System.Collections.Generic;
using System.Web.Mvc;

namespace Lifvs.Alerts
{
    public static class AlertExtensions
    {
        const string Alerts = "_Alerts";
        public static List<Alert> GetAlerts(this TempDataDictionary tempData)
        {
            if (!tempData.ContainsKey(Alerts))
            {
                tempData[Alerts] = new List<Alert>();
            }
            return (List<Alert>)tempData[Alerts];
        }

        public static ActionResult WithSuccess(this ActionResult action, string message)
        {
            return new AlertDecoratorResult(action, "alert-success", message);
        }

        public static ActionResult WithInfo(this ActionResult action,string message)
        {
            return new AlertDecoratorResult(action, "alert-info", message);
        }

        public static ActionResult WithWarning(this ActionResult action,string message)
        {
            return new AlertDecoratorResult(action, "alert-warning", message);
        }

        public static ActionResult WithError(this ActionResult action,string message)
        {
            return new AlertDecoratorResult(action, "alert-danger", message);
        }
    }
}