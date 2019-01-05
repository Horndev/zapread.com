using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace zapread.com.Models
{
    /// <summary>
    /// Database record for a user account
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string AppId { get; set; }
        public string AboutMe { get; set; }
        public DateTime? DateJoined { get; set; }

        public Int64 Reputation { get; set; }

        public string PGPPubKey { get; set; }

        // Comma-separated list of language codes. e.g.: en,es,it,fr
        public string Languages { get; set; }

        //Earnings including direct, group, community
        public double TotalEarned { get; set; }
        public virtual ICollection<EarningEvent> EarningEvents { get; set; }
        public virtual ICollection<SpendingEvent> SpendingEvents { get; set; }

        public virtual UserSettings Settings { get; set; }
        public virtual UserFunds Funds { get; set; }
        public virtual UserImage ThumbImage { get; set; }
        public virtual UserImage ProfileImage { get; set; }
        public virtual UserIgnoreUser UserIgnores { get; set; }
        public virtual ICollection<Post> PostVotesUp { get; set; }
        public virtual ICollection<Post> PostVotesDown { get; set; }
        public virtual ICollection<Comment> CommentVotesUp { get; set; }
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

        [InverseProperty("Administrators")]
        public virtual ICollection<Group> GroupAdministration { get; set; }

        public ICollection<LNTransaction> LNTransactions { get; set; }

        [InverseProperty("To")]
        public virtual ICollection<UserMessage> Messages { get; set; }

        [InverseProperty("To")]
        public virtual ICollection<UserAlert> Alerts { get; set; }

        public static implicit operator User(string v)
        {
            throw new NotImplementedException();
        }
    }

    public class UserIgnoreUser
    {
        public int Id { get; set; }

        public virtual ICollection<User> IgnoringUsers { get; set; }
    }

    public class UserAlert
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime? TimeStamp { get; set; }

        [InverseProperty("Alerts")]
        public User To { get; set; }

        public Post PostLink { get; set; }
        public Comment CommentLink { get; set; }

        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UserMessage
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public string Title { get; set; }

        [InverseProperty("Messages")]
        public User To { get; set; }

        public User From { get; set; }
        public Post PostLink { get; set; }
        public Comment CommentLink { get; set; }

        public DateTime? TimeStamp { get; set; }

        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class UserSettings
    {
        public int Id { get; set; }

        // Emails:
        public bool NotifyOnOwnPostCommented { get; set; }
        public bool NotifyOnOwnCommentReplied { get; set; }
        public bool NotifyOnNewPostSubscribedGroup { get; set; }
        public bool NotifyOnNewPostSubscribedUser { get; set; }

        public bool NotifyOnReceivedTip { get; set; }
        public bool NotifyOnPrivateMessage { get; set; }
        public bool NotifyOnMentioned { get; set; }

        // Alerts:
        public bool AlertOnOwnPostCommented { get; set; }

        public string ColorTheme { get; set; }
    }

    public class ZapIcon
    {
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [StringLength(80)]
        public string Icon { get; set; }

        public int NumUses { get; set; }

        public string Lib { get; set; }
    }

    public class UserFunds
    {
        [Key]
        public int Id { get; set; }
        public double TotalEarned { get; set; }
        public double Balance { get; set; }
    }

    public class UserImage
    {
        [Key]
        public int ImageId { get; set; }
        public byte[] Image { get; set; }
    }

    /// <summary>
    /// Database record for a post
    /// </summary>
    public class Post
    {
        [Key]
        public int PostId { get; set; }
        public int Score { get; set; }
        public string PostTitle { get; set; }
        public DateTime? TimeStamp { get; set; }
        public DateTime? TimeStampEdited { get; set; }                  // When the post was last edited, or null
        public string Content { get; set; }
        public double TotalEarned { get; set; }
        // User who made post
        public virtual User UserId { get; set; }

        // Language post is written in
        public string Language { get; set; }

        [InverseProperty("Posts")]
        public virtual Group Group { get; set; }
        public virtual ICollection<User> VotesUp { get; set; }
        public virtual ICollection<User> VotesDown { get; set; }

        [InverseProperty("Post")]
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<UserImage> Images { get; set; }

        // Post flags
        public bool IsDeleted { get; set; }
        public bool IsSticky { get; set; }
        public bool IsNSFW { get; set; }
        public bool IsDraft { get; set; }
        public bool IsPublished { get; set; }
    }

    public enum VoteDirection
    {
        Undefined = 0,
        Up = 1,
        Down = 2,
    }

    public class PendingPostVote
    {
        [Key]
        public Int64 Id { get; set; }

        public int PostId { get; set; }

        public VoteDirection Direction { get; set; }

        public virtual LNTransaction Payment { get; set; }

        public bool IsComplete { get; set; }
    }

    public class PendingCommentVote
    {
        [Key]
        public Int64 Id { get; set; }

        public int CommentId { get; set; }

        public VoteDirection Direction { get; set; }

        public virtual LNTransaction Payment { get; set; }

        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// Database record for a comment
    /// </summary>
    public class Comment
    {
        [Key]
        public Int64 CommentId { get; set; }

        [InverseProperty("Comments")]
        public Post Post { get; set; }

        [InverseProperty("Replies")]
        public Comment Parent { get; set; }

        [InverseProperty("Parent")]
        public virtual ICollection<Comment> Replies { get; set; }

        public virtual User UserId { get; set; }
        public DateTime? TimeStamp { get; set; }

        public string Text { get; set; }

        public int Score { get; set; }

        public virtual ICollection<User> VotesUp { get; set; }
        public virtual ICollection<User> VotesDown { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsReply { get; set; }

        public double TotalEarned { get; set; }
    }

    /// <summary>
    /// Database record for a community group
    /// </summary>
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        public string GroupName { get; set; }

        // When the group was created
        public DateTime? CreationDate { get; set; }

        [InverseProperty("Groups")]
        public virtual ICollection<User> Members { get; set; }

        [InverseProperty("IgnoredGroups")]
        public virtual ICollection<User> Ignoring { get; set; }

        [InverseProperty("GroupModeration")]
        public virtual ICollection<User> Moderators { get; set; }

        [InverseProperty("GroupAdministration")]
        public virtual ICollection<User> Administrators { get; set; }

        [InverseProperty("Group")]
        public virtual ICollection<Post> Posts { get; set; }

        // These earnings need to go out
        public double TotalEarnedToDistribute { get; set; }

        public double TotalEarned { get; set; }

        public int Tier { get; set; }

        public string Tags { get; set; }

        public string Icon { get; set; }
    }

    public class ZapReadGlobals
    {
        [Key]
        public int Id { get; set; }

        public double ZapReadEarnedBalance { get; set; }

        public double ZapReadTotalEarned { get; set; }

        public double ZapReadTotalWithdrawn { get; set; }

        public ICollection<LNTransaction> LNWithdraws { get; set; }

        // Funds waiting to be distributed to users
        public double CommunityEarnedToDistribute { get; set; }

        public double TotalEarnedCommunity { get; set; }

        public double TotalDepositedCommunity { get; set; }

        public double TotalWithdrawnCommunity { get; set; }
    }
}