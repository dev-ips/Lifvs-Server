using System;

namespace Lifvs.Common.DataModels
{
    public class WebUserInvitation
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public bool IsInvitationAccepted { get; set; }
        public long? UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? InvitationApprovedDate { get; set; }
    }
}
