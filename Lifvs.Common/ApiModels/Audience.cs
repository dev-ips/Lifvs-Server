namespace Lifvs.Common.ApiModels
{
    public class Audience
    {
        public string AudienceId { get; set; }
        public string Email { get; set; }
        public string AudienceType { get; set; }
    }
    public class RegisterResponse
    {
        public long UserId { get; set; }
        public string Message { get; set; }
    }
    public class LoginUnVerifiedEmailResponse
    {

        public EmailUnverifiedResponse data { get; set; }
    }
    public class EmailVerificationResponse
    {
        public string MessageType { get; set; }
        public string Message { get; set; }
    }
    public class EmailUnverifiedResponse
    {
        public long UserId { get; set; }
        public bool IsVerified { get; set; }
        public string Message { get; set; }
    }
    public class FacebookLoginErrorResponse
    {
        public string Message { get; set; }
    }
}
