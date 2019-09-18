using System;
using System.ComponentModel.DataAnnotations;
using zapread.com.Models.Database;

namespace zapread.com.Models
{
    public class SpendingEvent
    {
        [Key]
        public int Id { get; set; }

        public DateTime? TimeStamp { get; set; }

        public double Amount { get; set; }

        public Post Post { get; set; }

        public Comment Comment { get; set; }

        public Group Group { get; set; }

        // type?
    }

    public class EarningEvent
    {
        [Key]
        public int Id { get; set; }

        // 0=direct, 1=group, 2=community
        public int Type { get; set; }

        // 0=post, 1=comment, 3=tip
        public int OriginType { get; set; }

        // This is the post or comment identifier
        public int OriginId { get; set; }

        public double Amount { get; set; }

        public DateTime? TimeStamp { get; set; }
    }

    public enum TransactionUse
    {
        Undefined = 0,
        VotePost = 1,
        VoteComment = 2,
        Tip = 3,
        UserDeposit = 4,
    }

    public enum TransactionUseAction
    {
        Undefined = -1,
        VoteDown = 0,
        VoteUp = 1,
    }

    public class LNTransaction
    {
        [Key]
        public int Id { get; set; }

        public User User { get; set; }

        public TransactionUse UsedFor { get; set; }

        public int UsedForId { get; set; }

        public TransactionUseAction UsedForAction { get; set; }

        public string PaymentRequest { get; set; }

        public string HashStr { get; set; }

        /// <summary>
        /// When the transaction was executed
        /// </summary>
        public DateTime? TimestampSettled { get; set; }

        public DateTime? TimestampCreated { get; set; }

        public Int64 Amount { get; set; }
        public string Memo { get; set; }

        public bool IsDeposit { get; set; }         // True if transaction is a deposit
        public bool IsSettled { get; set; }
        public bool IsIgnored { get; set; }         // Don't check this invoice for payment status

        /// <summary>
        /// The fee which was paid (in Satoshi)
        /// </summary>
        public Int64? FeePaid_Satoshi { get; set; }

        public string NodePubKey { get; set; }      // If known - the public key of the other node

        /// <summary>
        /// If the invoice was applied in use
        /// </summary>
        public bool IsSpent { get; set; }

        /// <summary>
        /// If the invoice was applied in use
        /// </summary>
        public bool IsLimbo { get; set; }

        /// <summary>
        /// Record error messages
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Record of payment error
        /// </summary>
        public bool IsError { get; set; }
    }

}