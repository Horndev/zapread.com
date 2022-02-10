using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace zapread.com.Models.API.Groups
{
    public class ListGroupsResponse : ZapReadResponse
    {
        public int draw { get; set; }
        public int recordsTotal {get; set;}
        public int recordsFiltered { get; set; }
        public List<GroupInfo> data { get; set; }
    }
}