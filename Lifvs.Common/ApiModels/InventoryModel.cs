using System;
using System.ComponentModel.DataAnnotations;

namespace Lifvs.Common.ApiModels
{
    public class InventoryModel
    {
    }
    public class InventoryViewModel
    {
        public long Id { get; set; }
        public string BrandName { get; set; }
        public string Name { get; set; }
        [Display(Name = "Price")]
        public decimal OutPriceIncVat { get; set; }
        public string Specification { get; set; }
        public string ImagePath { get; set; }
        public long CreatedBy { get; set; }
        public string InventoryCode { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string VolumeType { get; set; }
        public string Supplier { get; set; }
        public string Volume { get; set; }
        public decimal REA { get; set; }
    }
    public class InventoryDropDown
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
    public class InventoryAddModel
    {
        public long Id { get; set; }
        [Display(Name = "Brand Name")]
        [Required(ErrorMessage = "Brand Name is required.")]
        public string BrandName { get; set; }
        [Display(Name = "Inventory Name")]
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Display(Name = "Barcode")]
        [Required(ErrorMessage = "Scanned code is required.")]
        public string InventoryCode { get; set; }
        [Display(Name = "Photo")]
        public string ImagePath { get; set; }
        public long CreatedBy { get; set; }
        public string FileUrl { get; set; }

        public string VolumeType { get; set; }
        public string Supplier { get; set; }
        [Required(ErrorMessage = "InPrice is required.")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "In price is not valid.")]
        public decimal InPrice { get; set; }
        [Required(ErrorMessage = "OutPrice is required.")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Out price is not valid.")]
        public decimal OutPriceIncVat { get; set; }
        [Required(ErrorMessage = "REA is required.")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "REA is not valid.")]
        public decimal REA { get; set; }
        public string Volume { get; set; }
        [Required(ErrorMessage = "Specification is required.")]
        public string Specification { get; set; }
    }
    public class InventorySearchModel
    {
        public string SearchKeyWord { get; set; }
        public DateTime? CurrentDate { get; set; }
        public string InventoryCode { get; set; }
    }
    public class InventoryResponseModel
    {
        public long Id { get; set; }
        public string BrandName { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public long CreatedBy { get; set; }
        public string InventoryCode { get; set; }
        public decimal REA { get; set; }
    }
}
