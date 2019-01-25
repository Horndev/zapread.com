using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Admin
{
    public class AddUserToGroupRoleViewModel
    {
        public string User { get; set; }

        public string GroupName { get; set; }

        public string Role { get; set; }

        public List<string> Roles { get; set; }
    }
}