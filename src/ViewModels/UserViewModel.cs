namespace RobMensching.TinyBugs.ViewModels
{
    using RobMensching.TinyBugs.Models;

    public class UserViewModel
    {
        public UserViewModel()
        {
        }

        public UserViewModel(User user)
        {
            this.Username = user.UserName;
            this.FullName = user.FullName;
            this.Role = user.Role;
        }

        public string Username { get; set; }

        public string FullName { get; set; }

        public UserRole Role { get; set; }
    }
}
