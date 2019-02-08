﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

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