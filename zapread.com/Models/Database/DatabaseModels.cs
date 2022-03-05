using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zapread.com.Models
{
    /// <summary>
    /// Describes an icon which can be used on the website.
    /// This should be deprecated in a future version.
    /// </summary>
    public class ZapIcon
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Index(IsUnique = true)]
        [StringLength(80)]
        public string Icon { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumUses { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Lib { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ImageSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] Image { get; set; }
    }

    /// <summary>
    /// Storage of user graphics in the database
    /// </summary>
    public class UserImage
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int ImageId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte[] Image { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int XSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int YSize { get; set; }

        /// <summary>
        /// // Version is used when image is updated
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserAppId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum VoteDirection
    {
        /// <summary>
        /// 
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// 
        /// </summary>
        Up = 1,
        /// <summary>
        /// 
        /// </summary>
        Down = 2,
    }

    /// <summary>
    /// 
    /// </summary>
    public class PendingPostVote
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public Int64 Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PostId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public VoteDirection Direction { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual LNTransaction Payment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsComplete { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PendingCommentVote
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public Int64 Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int CommentId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public VoteDirection Direction { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual LNTransaction Payment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsComplete { get; set; }
    }
}