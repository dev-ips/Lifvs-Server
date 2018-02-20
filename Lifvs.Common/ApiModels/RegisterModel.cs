namespace Lifvs.Common.ApiModels
{
    public class RegisterModel
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PhotoUrl { get; set; }
        public string StreetAddress { get; set; }
        public string PostalAddress { get; set; }
        public string AreaAddress { get; set; }
        public long? CountryId { get; set; }
        public string AuthType { get; set; }
        public string AuthId { get; set; }
        public string DeviceType { get; set; }
        public string DeviceId { get; set; }
    }
}
