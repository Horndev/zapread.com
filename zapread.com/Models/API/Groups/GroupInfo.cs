using System;
using System.Collections.Generic;

namespace zapread.com.Models.API.Groups
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedddMMMYYYY { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A short description of the group
        /// </summary>
        public string ShortDescription { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int IconId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumMembers { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int NumPosts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UserPosts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Progress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsMember { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsBanished { get; set; }

        /// <summary>
        /// UTC time banishment expires
        /// </summary>
        public DateTime? BanishExpires { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ulong Earned { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsMod { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsAdmin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsIgnoring { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DefaultLanguage { get; set; }
    }
}