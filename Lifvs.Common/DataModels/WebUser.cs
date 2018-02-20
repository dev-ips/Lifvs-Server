using System;

namespace Lifvs.Common.DataModels
{
    public class WebUser
    {
        public virtual long Id { get; set; }
        public virtual string Email { get; set; }
        public virtual string Password { get; set; }
        public virtual string Name { get; set; }
        public virtual string AdditionalInformation { get; set; }
        public virtual int? RoleId { get; set; }
        public virtual bool Active { get; set; }
        public virtual bool Deleted { get; set; }
        public virtual long? CreatedBy { get; set; }
        public virtual DateTime? CreatedDate { get; set; }
        public virtual long? ModifiedBy { get; set; }
        public virtual DateTime? ModifiedDate { get; set; }
    }
}
