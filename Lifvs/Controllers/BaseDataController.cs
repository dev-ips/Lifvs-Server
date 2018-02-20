using Lifvs.Common.Services.Interfaces;
using log4net;
using System;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Lifvs.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/basedata")]
    public class BaseDataController : ApiController
    {
        private readonly ILog _log;
        private readonly IBaseDataService _baseDataService;
        public BaseDataController(ILog log, IBaseDataService baseDataService)
        {
            _log = log;
            _baseDataService = baseDataService;
        }
        [HttpGet]
        [Route("welcomemessage")]
        public IHttpActionResult GetWelcomeMessage()
        {
            try
            {
                var welcomeMessage = _baseDataService.GetWelcomeMessage();
                return Ok(new { Message = welcomeMessage });
            }
            catch(Exception ex)
            {
                _log.ErrorFormat("Error in getting welcome message. Error : {0}", ex.Message);
                _log.Error(ex);
                throw;
            }
        }
    }
}
