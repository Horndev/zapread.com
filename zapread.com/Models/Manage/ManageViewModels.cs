using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using zapread.com.Models.Database;

namespace zapread.com.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ManageUserViewModel : UserViewModel
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public bool HasPassword { get; set; }

        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool BrowserRemembered { get; set; }
        public bool IsGoogleAuthenticatorEnabled { get; set; }
        public bool IsEmailAuthenticatorEnabled { get; set; }
        public FinancialViewModel Financial { get; set; }
        public UserAchievementsViewModel AchievementsViewModel { get; set; }
        public IList<string> Languages { get; set; }
        public IList<string> KnownLanguages { get; set; }
        public UserSettings Settings { get; set; }

        /// <summary>
        /// If the user was not referred - then this flag determines if the user can gift their referral to another user
        /// </summary>
        public bool CanGiftReferral { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ManageUserGroupsViewModel
    {
        public IEnumerable<GroupInfo> Groups { get; set; }
    }

    public class UpdateImageViewModel
    {

    }

    /// <summary>
    /// 
    /// </summary>
    public class FinancialViewModel
    {
        public IList<LNTxViewModel> Transactions { get; set; }
        public IList<EarningsViewModel> Earnings { get; set; }
        public IList<SpendingsViewModel> Spendings { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SpendingsViewModel
    {
        public DateTime TimeStamp { get; set; }
        public string Value { get; set; }
        public string Link { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EarningsViewModel
    {
        public DateTime TimeStamp { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public int ItemId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class LNTxViewModel
    {
        public DateTime Timestamp { get; set; }
        public Int64 Value { get; set; }
        public string Type { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AboutMeViewModel
    {
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "About Me")]
        public string AboutMe { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}