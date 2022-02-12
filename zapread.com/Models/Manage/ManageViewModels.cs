﻿using Microsoft.AspNet.Identity;
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
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool BrowserRemembered { get; set; }
        public FinancialViewModel Financial { get; set; }
        public UserAchievementsViewModel AchievementsViewModel { get; set; }
        public IList<string> Languages { get; set; }
        public IList<string> KnownLanguages { get; set; }
        public UserSettings Settings { get; set; }
    }

    public class ManageUserGroupsViewModel
    {
        public IEnumerable<GroupInfo> Groups { get; set; }
    }

    public class UpdateImageViewModel
    {

    }

    public class FinancialViewModel
    {
        public IList<LNTxViewModel> Transactions { get; set; }
        public IList<EarningsViewModel> Earnings { get; set; }
        public IList<SpendingsViewModel> Spendings { get; set; }
    }

    public class SpendingsViewModel
    {
        public DateTime TimeStamp { get; set; }
        public string Value { get; set; }
        public string Link { get; set; }
    }

    public class EarningsViewModel
    {
        public DateTime TimeStamp { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public int ItemId { get; set; }
    }

    public class LNTxViewModel
    {
        public DateTime Timestamp { get; set; }
        public Int64 Value { get; set; }
        public string Type { get; set; }
    }

    public class AboutMeViewModel
    {
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "About Me")]
        public string AboutMe { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

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

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

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

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}