using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Lifvs.Common.Helpers
{
    public interface IExceptionManager
    {
        HttpResponseException ThrowException(HttpStatusCode status, string phrase = "", dynamic content = null);
        HttpResponseException ThrowException(HttpStatusCode status, string phrase, List<string> errorMessages);
        HttpResponseException ThrowException(HttpStatusCode status, string phrase, List<KeyValuePair<string, string>> errorMessages);
        HttpResponseException ThrowException(HttpStatusCode status, string phrase, string errorMessage);
        HttpResponseException ThrowException(HttpStatusCode status, string phrase, List<ErrorElement> errorElements);
    }

    public class ErrorElement
    {
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
    }
}
