using Lifvs.Common.Services.Interfaces;
using System.Web.Mvc;

namespace Lifvs.Controllers
{
    public class EmailController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;
        public EmailController(IAccessTokenService accessTokenService)
        {
            _accessTokenService = accessTokenService;
        }
        // GET: Email
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ConfirmEmail(string id,string email)
        {
            var output = _accessTokenService.VerifyUser(id, email);
            return View(output);
        }
    }
}