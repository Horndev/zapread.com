using System.Collections.Generic;

namespace zapread.com.Models.Admin
{
    /// <summary>
    /// 
    /// </summary>
    public class AddUserToGroupRoleViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Roles { get; set; }
    }
}