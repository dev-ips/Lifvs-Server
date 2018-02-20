using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.ApiModels
{
    public class StoreInventoryModel
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public long InventoryId { get; set; }
        public long NumberOfItems { get; set; }

        public string Store { get; set; }
        public string Inventory { get; set; }
    }

    public class AddStoreInventoryModel
    {
        public long Id { get; set; }
        [Display(Name ="Store")]
        [Required(ErrorMessage ="Store is required.")]
        public long StoreId { get; set; }
        [Display(Name = "Inventory")]
        [Required(ErrorMessage ="Inventory is required.")]
        public long InventoryId { get; set; }
        [Display(Name = "Number of Items")]
        public long? NumberOfItems { get; set; }
    }
    public class StoreInventoryCSVModel
    {
        public long Id { get; set; }
        public long StoreId { get; set; }
        public long InventoryId { get; set; }
        public long NumberOfItems { get; set; }
    }
}

