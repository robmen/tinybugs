namespace RobMensching.TinyBugs.ViewModels
{
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;

    public class UserViewModel
    {
        public UserViewModel()
        {
        }

        public UserViewModel(User user)
        {
            this.Id = user.Id;
            this.Username = user.UserName;
            this.FullName = user.FullName;
            this.GravatarImageUrl = UserService.GetGravatarImageUrl(user.GravatarId);
            this.Role = user.Role;
            this.Url = ConfigService.AppPath + "user/" + user.Id;
        }

        public long Id { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public string GravatarImageUrl { get; set; }

        public UserRole Role { get; set; }

        public string Url { get; set; }

        public bool Unverified { get { return this.Role == UserRole.Unverfied; } }
    }
}
