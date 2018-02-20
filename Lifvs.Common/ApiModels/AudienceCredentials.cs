namespace Lifvs.Common.ApiModels
{
    public class AudienceCredentials
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string OAuthId { get; set; }
        public string OAuthType { get; set; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
    }
}
