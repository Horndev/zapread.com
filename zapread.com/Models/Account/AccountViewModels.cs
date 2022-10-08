using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ExternalLoginConfirmationViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "I would like to receive notification emails")]
        public bool AcceptEmailsNotify { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ExternalLoginListViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ReturnUrl { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SendCodeViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string SelectedProvider { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReturnUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class VerifyCodeViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Provider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ForgotViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Refferal code (if any)
        /// </summary>
        [Display(Name = "Referral Code (optional)")]
        public string RefCode { get; set; }

        /// <summary>
        /// Base 64 encoded captcha image
        /// </summary>
        public string CaptchaSrcB64 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [Display(Name = "Captcha")]
        public string Captcha { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "I would like to receive notification emails")]
        public bool AcceptEmailsNotify { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "I would like to receive general newsletter emails")]
        public bool AcceptEmailsNews { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ResetPasswordViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Code { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
