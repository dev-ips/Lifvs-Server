using System.ComponentModel.DataAnnotations;

namespace Lifvs.Common.ApiModels
{
    public class EmployeeViewModel
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public string CurrentRole { get; set; }
        public int RowNum { get; set; }
        public int ResultsCount { get; set; }
    }
    public class InvitationModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Role is required.")]
        public int? RoleId { get; set; }
    }
    public class SignUpModel
    {
        public long InvitationId { get; set; }
        public string Email { get; set; }
        public int? RoleId { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirm password do not match.")]
        public string ConfirmPassword { get; set; }
        public long? CreatedBy { get; set; }
    }
}
