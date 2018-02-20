using System;

namespace Lifvs.Common.DataModels
{

    public class AccessToken
    {
        public virtual long Id { get; set; }
        public virtual Guid AccessTokenVal { get; set; }
        public virtual string AudienceId { get; set; }
        public virtual string RoleType { get; set; }
        public virtual bool Expired { get; set; }
        public virtual DateTime? ExpiredOn { get; set; }
        public virtual string TokenType { get; set; }
    }
}
