using System.ComponentModel.DataAnnotations;

namespace Lifvs.Common.ApiModels
{
    public class LoginModel
    {
        [Required(ErrorMessage ="Email is required.")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage ="Password is required.")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
    public class RecoveryEmail
    {
        [Required(ErrorMessage = "Email is required.")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Enter valid email.")]
        public string Email { get; set; }
    }
    public class ChangePassword
    {
        [Required(ErrorMessage = "Email is required.")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Enter valid email.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Recovery code is required.")]
        public string RecoveryCode { get; set; }

    }
}
