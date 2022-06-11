using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using zapread.com.Models.Database.Financial;

namespace zapread.com.Models.Database
{
    /// <summary>
    /// Database record for a user account
    /// </summary>
    public class User
    {
        [Key]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// This links the zapread database user to the ASP user for login/OWIN
        /// </summary>
        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(37)]                  // should be 36, added 1 char for buffer
        [Index]                             // often queried on, so index added
        public string AppId { get; set; }

        public string AboutMe { get; set; }

        public DateTime? DateJoined { get; set; }

        public DateTime? DateLastActivity { get; set; }

        public Int64 Reputation { get; set; }

        /// <summary>
        /// This was supposed to be for encryption/signatures but is now used to track hangfire jobs [TODO: refactor]
        /// </summary>
        public string PGPPubKey { get; set; }

        public bool IsOnline { get; set; }

        /// <summary>
        /// Comma-separated list of language codes. e.g.: en,es,it,fr
        /// </summary>
        public string Languages { get; set; }
        //Earnings including direct, group, community
        public double TotalEarned { get; set; }

        public virtual ICollection<EarningEvent> EarningEvents { get; set; }
        public virtual ICollection<SpendingEvent> SpendingEvents { get; set; }
        
        public virtual UserSettings Settings { get; set; }
        
        public virtual UserFunds Funds { get; set; }
        public virtual UserImage ThumbImage { get; set; }
        public virtual UserImage ProfileImage { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<BannerAlert> BannerAlerts { get; set; }

        [InverseProperty("DismissedBy")]
        public virtual ICollection<BannerAlert> DismissedBannerAlerts { get; set; }

        [InverseProperty("FollowedByUsers")]
        public virtual ICollection<Post> FollowingPosts { get; set; }

        [InverseProperty("IgnoredByUsers")]
        public virtual ICollection<Post> IgnoringPosts { get; set; }

        [InverseProperty("IgnoredByUsers")]
        public virtual ICollection<User> IgnoringUsers { get; set; }

        [InverseProperty("IgnoringUsers")]
        public virtual ICollection<User> IgnoredByUsers { get; set; }

        /// <summary>
        /// Blocking users means that you will not get chats/messages from the user
        /// </summary>
        [InverseProperty("BlockedByUsers")]
        public virtual ICollection<User> BlockingUsers { get; set; }

        [InverseProperty("BlockingUsers")]
        public virtual ICollection<User> BlockedByUsers { get; set; }

        [InverseProperty("VotesUp")]
        public virtual ICollection<Post> PostVotesUp { get; set; }

        [InverseProperty("VotesDown")]
        public virtual ICollection<Post> PostVotesDown { get; set; }

        [InverseProperty("HiddenBy")]
        public virtual ICollection<Post> HiddenPosts { get; set; }

        [InverseProperty("VotesUp")]
        public virtual ICollection<Comment> CommentVotesUp { get; set; }

        [InverseProperty("VotesDown")]
        public virtual ICollection<Comment> CommentVotesDown { get; set; }

        [InverseProperty("Following")]
        public virtual ICollection<User> Followers { get; set; }

        [InverseProperty("Followers")]
        public virtual ICollection<User> Following { get; set; }

        [InverseProperty("Members")]
        public virtual ICollection<Group> Groups { get; set; }

        [InverseProperty("Ignoring")]
        public virtual ICollection<Group> IgnoredGroups { get; set; }

        [InverseProperty("Moderators")]
        public virtual ICollection<Group> GroupModeration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("Administrators")]
        public virtual ICollection<Group> GroupAdministration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("User")]
        public virtual ICollection<GroupBanished> GroupBanished { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<LNTransaction> LNTransactions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("User")]
        public ICollection<Financial.Withdraw> Withdraws { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<APIKey> APIKeys { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ICollection<UserProcess> UserProcesses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("UserId")]
        public virtual ICollection<Post> Posts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("UserId")]
        public virtual ICollection<Comment> Comments { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("To")]
        public virtual ICollection<UserMessage> Messages { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [InverseProperty("To")]
        public virtual ICollection<UserAlert> Alerts { get; set; }

        /// <summary>
        /// Code used for this user
        /// </summary>
        public string ReferralCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Referral ReferralInfo { get; set; }

        /// <summary>
        /// Achievements by this user
        /// </summary>
        [InverseProperty("AchievedBy")]
        public virtual ICollection<UserAchievement> Achievements { get; set; }

        /// <summary>
        /// Reactions unlocked by this user
        /// </summary>
        [InverseProperty("UnlockedBy")]
        public virtual ICollection<Reaction> AvailableReactions { get; set; }

        /// <summary>
        /// Navigation property
        /// </summary>
        public ICollection<PostReaction> PostReactions { get; set; }

        /// <summary>
        /// Navigation property
        /// </summary>
        public ICollection<CommentReaction> CommentReactions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        public static implicit operator User(string v)
        {
            // not sure why this exists...
            throw new NotImplementedException();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}