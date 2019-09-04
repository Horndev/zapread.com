namespace zapread.com.Models.Manage
{
    public class ManageSettingViewModel
    {
        public ManageSettingViewModel(string name, bool isActive)
        {
            Name = name;
            IsActive = isActive;
        }

        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}