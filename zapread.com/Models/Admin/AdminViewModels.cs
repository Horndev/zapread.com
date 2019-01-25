using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models
{
    public class AuditUserViewModel
    {
        public string Username { get; set; }
    }

    public class AdminViewModel
    {
        public ZapReadGlobals Globals { get; set; }

        public double PendingGroupToDistribute { get; set; }
        public double LNTotalDeposited { get; set; }
        public double LNTotalWithdrawn { get; set; }
    }

    public class AddUserToGroupRoleModel
    {
        public string User { get; set; }

        public string GroupName { get; set; }

        public string Role { get; set; }

        public List<string> Roles { get; set; }
    }

    public class Stat
    {
        //public DateTime TimeStamp { get; set; }
        public Int64 TimeStampUtc { get; set; } 
        public int Count { get; set; }
    }

    public enum DateGroupType
    {
        Day,
        Week,
        Month,
        Quarter,
        Year
    }

    public class SiteAdminBarUserInfoViewModel
    {
        public int UserId { get; set; }
        public int Balance { get; set; }
        public int NumPosts { get; set; }
        public int TotalDeposited { get; set; }
        public int TotalWithdrawn { get; set; }
        public int TotalEarned { get; set; }
        public int TotalSpent { get; set; }
    }
}