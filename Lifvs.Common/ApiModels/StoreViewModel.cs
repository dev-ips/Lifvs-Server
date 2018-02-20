using System.ComponentModel.DataAnnotations;

namespace Lifvs.Common.ApiModels
{
    public class StoreViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string QRCode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        //public string InternalNumber { get; set; }
        public string StoreNumber { get; set; }
        public decimal? Rating { get; set; }
        public double? Distance { get; set; }
        public int TotalRatings { get; set; }
        public string SupervisorName { get; set; }
    }
    public class StoreInputModel
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string SearchFied { get; set; }
    }

    public class AddStoreModel
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Email is invalid.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone is required.")]
        //[RegularExpression(@"^\+(?:[0-9]?){6,14}[0-9]$", ErrorMessage = "Invalid phone number.")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Enter only 0-9 digit.")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }
        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }
        [Required(ErrorMessage = "PostalCode is required.")]
        public string PostalCode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public decimal? Rating { get; set; }
        //public string InternalNumber { get; set; }
        public string StoreNumber { get; set; }
        [Required(ErrorMessage = "Supervisor name is required.")]
        public string SupervisorName { get; set; }
        //[Required(ErrorMessage = "QRCode is required.")]
        public string QRCode { get; set; }
    }
    public class ScanStoreModel
    {
        public long UserId { get; set; }
        public long StoreId { get; set; }
        public string QRCode { get; set; }
    }
    public class ScanStoreResponseModel
    {
        public bool IsValid { get; set; }
        public string Massage { get; set; }
        public long CartId { get; set; }
    }
    public class StoreDropDownModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class StoresResponseModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string QRCode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string InternalNumber { get; set; }
        public string StoreNumber { get; set; }
        public decimal? Rating { get; set; }
        public double? Distance { get; set; }
        public int TotalRatings { get; set; }
        //public int[] InventoryIds { get; set; }
    }
    public class StoreRateModel
    {
        public long UserId { get; set; }
        public long StoreId { get; set; }
        public decimal Rating { get; set; }
    }
}
