using System;

namespace Lifvs.Common.DataModels
{
    public class UserCardDetails
    {
        public virtual long Id { get; set; }
        public virtual long? UserId { get; set; }
        public virtual string CardNumber { get; set; }
        public virtual string CardType { get; set; }
        public virtual int ExpiredMonth { get; set; }
        public virtual int ExpiredYear { get; set; }
        public virtual int CVC { get; set; }
        public virtual string CreditCardId { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual DateTime? CreatedDate { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
    }
}
