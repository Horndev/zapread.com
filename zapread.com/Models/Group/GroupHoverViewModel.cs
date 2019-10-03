using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.GroupViews
{
    public class GroupHoverViewModel
    {
        public int GroupId { get; set; }
        public int GroupLevel { get; set; }
        public int GroupPostCount { get; set; }
        public int GroupMemberCount { get; set; }
    }
}