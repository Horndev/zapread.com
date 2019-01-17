using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models
{

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

        public bool IsPrivateMessage { get; set; }
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

        // Customization
        public string ColorTheme { get; set; }

        // Languages
        public bool ViewAllLanguages { get; set; }
        public bool ViewTranslatedLanguages { get; set; }
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

        // Unlockable description of group - set by admin
        public string ShortDescription { get; set; }

        // Posts in this group are in this language by default
        public string DefaultLanguage { get; set; }
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