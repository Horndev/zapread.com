using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Manage
{
    public class UpdatedUserAliasView
    {
        public string NewUserName { get; set; }

        public string OldUserName { get; set; }

        public Database.User User { get; set; }
    }
}