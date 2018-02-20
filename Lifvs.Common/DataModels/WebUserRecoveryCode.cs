using System;

namespace Lifvs.Common.DataModels
{
    public class WebUserRecoveryCode
    {
        public virtual long Id { get; set; }
        public virtual long WebUserId { get; set; }
        public virtual string RecoveryCode { get; set; }
        public virtual bool Active { get; set; }
        public virtual DateTime ExpiredOn { get; set; }
    }
}
