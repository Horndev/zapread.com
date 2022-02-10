using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models.API.Groups
{
    public class LoadGroupResponse : ZapReadResponse
    {
        public int groupId { get; set; }

        public GroupInfo group { get; set; }

        public bool IsLoggedIn { get; set; }

        public string UserName { get; set; }
    }
}