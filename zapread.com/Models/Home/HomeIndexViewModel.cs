using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.Home
{
    public class HomeIndexViewModel
    {
        public string Sort { get; set; }
        public List<GroupInfo> SubscribedGroups { get; set; }
    }
}