using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using zapread.com.Models.Database;
using zapread.com.Models.Lightning;

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

    /// <summary>
    /// Describes an earning by user or entity
    /// </summary>
    public class EarningEvent
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 0=direct, 1=group, 2=community, 3=website
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 0=post, 1=comment, 3=tip
        /// </summary>
        public int OriginType { get; set; }

        /// <summary>
        /// This is the post or comment identifier
        /// </summary>
        public int OriginId { get; set; }

        /// <summary>
        /// Amount in Satoshi
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Timestamp of event
        /// </summary>
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

    /// <summary>
    /// Records a lightning network transaction
    /// </summary>
    public class LNTransaction
    {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Guid WithdrawId { get; set; }

        /// <summary>
        /// User which owns this transaction
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// The transaction use function
        /// </summary>
        public TransactionUse UsedFor { get; set; }

        /// <summary>
        /// Id (FK) for the item this was used for - depending on the function in UsedFor
        /// </summary>
        public int UsedForId { get; set; }

        /// <summary>
        /// An action associated with the use
        /// </summary>
        public TransactionUseAction UsedForAction { get; set; }

        /// <summary>
        /// This is the bech32 encoded payment request
        /// </summary>
        public string PaymentRequest { get; set; }

        /// <summary>
        /// Hash String for payment (this should be a 32 byte hex-encoded string)
        /// payment_hash or r_hash
        /// </summary>
        public string HashStr { get; set; }

        /// <summary>
        /// [invoice] The hash of the preimage
        /// r_hash
        /// </summary>
        public string PreimageHash { get; set; }

        /// <summary>
        /// [payments] payment_hash
        /// </summary>
        public string PaymentHash { get; set; }

        /// <summary>
        /// When the transaction was settled
        /// </summary>
        public DateTime? TimestampSettled { get; set; }

        /// <summary>
        /// When this transaction was created
        /// </summary>
        public DateTime? TimestampCreated { get; set; }

        /// <summary>
        /// When syncing the database with the node, this is the timestamp in UTC this payment row was last updated
        /// </summary>
        public DateTime? TimestampUpdated { get; set; }

        /// <summary>
        /// The Lightning node which handled this transaction
        /// </summary>
        public LNNode ZapreadNode { get; set; }

        /// <summary>
        /// The payment_index (if known) for this payment in the Lightning node database
        /// </summary>
        public int? PaymentIndex { get; set; }

        /// <summary>
        /// The "add" index of this invoice. Each newly created invoice will increment this index making it monotonically increasing. 
        /// Callers to the SubscribeInvoices call can use this to instantly get notified of all added invoices with an add_index greater than this one.
        /// </summary>
        public int? AddIndex { get; set; }

        /// <summary>
        /// The "settle" index of this invoice. Each newly settled invoice will increment this index making it monotonically increasing. 
        /// Callers to the SubscribeInvoices call can use this to instantly get notified of all settled invoices with an settle_index greater than this one.
        /// </summary>
        public int? SettleIndex { get; set; }

        /// <summary>
        /// FAILURE_REASON_NONE	0	
        /// FAILURE_REASON_TIMEOUT	1	
        /// FAILURE_REASON_NO_ROUTE	2	
        /// FAILURE_REASON_ERROR	3	
        /// FAILURE_REASON_INCORRECT_PAYMENT_DETAILS	4	
        /// FAILURE_REASON_INSUFFICIENT_BALANCE	5
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// UNKNOWN	0	
        /// IN_FLIGHT	1	
        /// SUCCEEDED	2	
        /// FAILED	3
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// OPEN	0	
        /// SETTLED	1	
        /// CANCELED	2	
        /// ACCEPTED	3
        /// </summary>
        public string InvoiceState { get; set; }

        /// <summary>
        /// Indicate if this was a keysend transaction
        /// </summary>
        public bool? IsKeysend { get; set; }

        /// <summary>
        /// The preimage is proof that a payment was completed
        /// </summary>
        public string PaymentPreimage { get; set; }

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