namespace zapread.com.Models.Manage
{
    /// <summary>
    /// 
    /// </summary>
    public class ManageSettingViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isActive"></param>
        public ManageSettingViewModel(string name, bool isActive)
        {
            Name = name;
            IsActive = isActive;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsActive { get; set; }
    }
}