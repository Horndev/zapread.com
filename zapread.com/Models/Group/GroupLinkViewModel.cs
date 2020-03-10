using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using zapread.com.Models.Database;

namespace zapread.com.Models.GroupViews
{
    public class GroupLinkViewModel
    {
        [Obsolete]
        public Group Group { get; set; }

        public int GroupId { get; set; }

        public string GroupName { get; set; }

        public bool IsFirstPost { get; set; }
    }
}