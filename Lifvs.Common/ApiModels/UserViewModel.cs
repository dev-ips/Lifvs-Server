namespace Lifvs.Common.ApiModels
{
    public class UserViewModel
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string DeviceType { get; set; }
        public int RowNum { get; set; }
        public int ResultCount { get; set; }
    }
    public class UserProfileViewModel
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string StreetAddress { get; set; }
        public string PostalAddress { get; set; }
        public string AreaAddress { get; set; }
        public long? CountryId { get; set; }
    }
}
