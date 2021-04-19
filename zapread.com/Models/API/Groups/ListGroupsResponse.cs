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

    public class GroupInfo
    {
        public int Id { get; set; }
        public string CreatedddMMMYYYY { get; set; }
        public string Name { get; set; }
        public List<string> Tags { get; set; }
        public string Icon { get; set; }
        public int Level { get; set; }
        public int NumMembers { get; set; }
        public int NumPosts { get; set; }
        public int UserPosts { get; set; }
        public int Progress { get; set; }
        public bool IsMember { get; set; }
        public bool IsLoggedIn { get; set; }
        //public List<User> Members { get; set; }
        public bool IsMod { get; set; }
        public bool IsAdmin { get; set; }
    }
}