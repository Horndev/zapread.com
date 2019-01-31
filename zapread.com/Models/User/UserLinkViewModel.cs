using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models.UserViews
{
    public class UserLinkViewModel
    {
        public User User { get; set; }

        public bool IsIgnored { get; set; }
    }
}