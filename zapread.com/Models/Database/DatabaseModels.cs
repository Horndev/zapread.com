using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace zapread.com.Models
{
    public class ZapIcon
    {
        public int Id { get; set; }

        [Index(IsUnique = true)]
        [StringLength(80)]
        public string Icon { get; set; }

        public int NumUses { get; set; }

        public string Lib { get; set; }

        public int ImageSize { get; set; }

        public byte[] Image { get; set; }
    }

    public class UserImage
    {
        [Key]
        public int ImageId { get; set; }
        public byte[] Image { get; set; }
        public string ContentType { get; set; }
        public int XSize { get; set; }
        public int YSize { get; set; }

        // Version is used when image is updated
        public int Version { get; set; }
        public string UserAppId { get; set; }
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

}