using System.Collections.Generic;

namespace zapread.com.Models.API.Groups
{
    public class GroupInfo
    {
        public int Id { get; set; }
        public string CreatedddMMMYYYY { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// A short description of the group
        /// </summary>
        public string ShortDescription { get; set; }
        public List<string> Tags { get; set; }
        public string Icon { get; set; }

        public int IconId { get; set; }

        public int Level { get; set; }
        public int NumMembers { get; set; }
        public int NumPosts { get; set; }
        public int UserPosts { get; set; }
        public int Progress { get; set; }
        public bool IsMember { get; set; }
        public bool IsLoggedIn { get; set; }
        public ulong Earned { get; set; }
        public bool IsMod { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsIgnoring { get; set; }

        public string DefaultLanguage { get; set; }
    }
}