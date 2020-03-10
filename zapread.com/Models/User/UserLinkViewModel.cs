using zapread.com.Models.Database;

namespace zapread.com.Models.UserViews
{
    public class UserLinkViewModel
    {
        public User User { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public bool IsIgnored { get; set; }

        public bool IsFirstPost { get; set; }
    }
}