using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web;
using System.Web.Http;

namespace Lifvs.Common.Helpers
{
    public class ExceptionManager : IExceptionManager
    {
        private readonly ILog _logger;


        public ExceptionManager(ILog logger)
        {
            _logger = logger;
        }


        public ExceptionManager()
        {
        }


        public HttpResponseException ThrowException(HttpStatusCode status, string phrase = "", dynamic content = null)
        {
            Log(status, HttpContext.Current, string.Empty);

            throw new HttpResponseException(
                    new HttpResponseMessage
                    {
                        StatusCode = status,
                        ReasonPhrase = phrase,
                        Content = new ObjectContent<dynamic>(content, new JsonMediaTypeFormatter())
                    });
        }


        public HttpResponseException ThrowException(HttpStatusCode status, string phrase, List<KeyValuePair<string, string>> errorMessages)
        {
            Log(status, HttpContext.Current, string.Join("\n", errorMessages.Select(e => e.Value).ToList()));

            var errors = errorMessages.Select(CreateErrorElement).ToList();

            throw new HttpResponseException(
                    new HttpResponseMessage
                    {
                        StatusCode = status,
                        ReasonPhrase = phrase,
                        Content = new ObjectContent<List<ErrorElement>>(errors, new JsonMediaTypeFormatter())
                    });
        }


        public HttpResponseException ThrowException(HttpStatusCode status, string phrase, List<string> errorMessages)
        {
            var errors = errorMessages.Select(CreateErrorElement).ToList();

            Log(status, HttpContext.Current, string.Join("\n", errorMessages));

            throw new HttpResponseException(
                    new HttpResponseMessage
                    {
                        StatusCode = status,
                        ReasonPhrase = phrase,
                        Content = new ObjectContent<List<ErrorElement>>(errors, new JsonMediaTypeFormatter())
                    });
        }


        private void Log(HttpStatusCode status, HttpContext httpContext, string errorMessages)
        {
            LogLog4Net(status, httpContext, errorMessages);
        }

        private void LogLog4Net(HttpStatusCode status, HttpContext httpContext, string errorMessages)
        {
            var url = httpContext.Request.RawUrl;
            var message = string.Format("Response Code:{0}({2}); Errors:{3}{1}", status, errorMessages, (int)status, Environment.NewLine);
            var appLogger = new Thread(() => _logger.ErrorFormat("Message : {0}{1} Url : {2}", message, Environment.NewLine, url)) { IsBackground = true };
            appLogger.Start();
        }


        public HttpResponseException ThrowException(HttpStatusCode status, string phrase, string errorMessage)
        {
            var errors = new List<ErrorElement> { new ErrorElement { ErrorMessage = errorMessage } };

            Log(status, HttpContext.Current, errorMessage);

            return new HttpResponseException(
                    new HttpResponseMessage
                    {
                        StatusCode = status,
                        ReasonPhrase = phrase,
                        Content = new ObjectContent<List<ErrorElement>>(errors, new JsonMediaTypeFormatter())
                    });
        }


        public ErrorElement CreateErrorElement(string errormessage)
        {
            return new ErrorElement { ErrorMessage = errormessage };
        }


        public ErrorElement CreateErrorElement(KeyValuePair<string, string> errormessage)
        {
            return new ErrorElement { ErrorMessage = errormessage.Value, ErrorCode = errormessage.Key };
        }


        public HttpResponseException ThrowException(HttpStatusCode status, string phrase = "", List<ErrorElement> errorElements = null)
        {

            throw new HttpResponseException(
                    new HttpResponseMessage
                    {
                        StatusCode = status,
                        ReasonPhrase = phrase,
                        Content = new ObjectContent<List<ErrorElement>>(errorElements, new JsonMediaTypeFormatter())
                    });
        }
    }
}
