using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Admin
{
    public class SiteAdminBarUserInfoViewModel
    {
        public int UserId { get; set; }
        public int Balance { get; set; }
        public int NumPosts { get; set; }
        public int TotalDeposited { get; set; }
        public int TotalWithdrawn { get; set; }
        public int TotalEarned { get; set; }
        public int TotalSpent { get; set; }

        public string Email { get; set; }
    }
}