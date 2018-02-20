using System;

namespace Lifvs.Common.ApiModels
{
    public class RecoveryCode
    {
        public long? UserId { get; set; }
        public string RecoverCode { get; set; }
        public DateTime ExpiredOn { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
    public class ChangePasswordModel
    {
        public long? UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
