using System;

namespace Lifvs.Common.ApiModels
{
    public class AudienceResponse
    {
        public UserDetail data { get; set; }
    }
    public class UserDetail
    {
        public long UserId { get; set; }
        public string Email { get; set; }
        public Guid AccessToken { get; set; }
        public bool IsVerified { get; set; }
        public string Message { get; set; }
        public bool IsCardRegistered { get; set; }
        public string UserCode { get; set; }

    }
    public class ErrorResponseModel
    {
        public string Message { get; set; }
    }
}
