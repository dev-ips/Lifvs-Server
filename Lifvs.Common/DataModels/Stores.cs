using System;

namespace Lifvs.Common.DataModels
{
    public class Stores
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Email { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Address { get; set; }
        public virtual string City { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual string QRCode { get; set; }
        public virtual string Latitude { get; set; }
        public virtual string Longitude { get; set; }
        public virtual decimal? Rating { get; set; }
        //public virtual string InternalNumber { get; set; }
        public virtual string StoreNumber { get; set; }
        public virtual DateTime? CreatedDate { get; set; }
        public string SupervisorName { get; set; }
    }
}
