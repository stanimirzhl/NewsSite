namespace NewsSite.Models.ViewModels
{
    public class SettingsVM
    {
        public ProfileSettingsVM ProfileSettings { get; set; }
        public ChangePasswordVM ChangePassword { get; set; }
        public List<News> SavedNews { get; set; } = new List<News>();
        public string Section { get; set; }
    }
}
