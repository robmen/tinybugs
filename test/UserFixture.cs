namespace tinyBugs.test
{
    using System;
    using RobMensching.TinyBugs.Models;
    using RobMensching.TinyBugs.Services;
    using ServiceStack.OrmLite;
    using Xunit;

    public class UserFixture
    {
        [Fact]
        public void CanCreateNewUser()
        {
            var user = UserService.Create("tinybugs@robmensching.com", "My voice is my password.");
            Assert.NotEqual(Guid.Empty, user.Guid);
            Assert.Equal("tinybugs@robmensching.com", user.Email);
            Assert.Equal(24, user.Salt.Length);
            Assert.Equal(88, user.PasswordHash.Length);
            Assert.Equal(UserRole.Unverfied, user.Role);
        }

        [Fact]
        public void CanGenerateGravatarImageUrl()
        {
            var url = UserService.GetGravatarImageUrlForEmail("rob@robmensching.com");
            Assert.Equal("http://www.gravatar.com/avatar/179453ce28338e22260a03651e80ac44?r=pg&d=mm", url);
        }

        [Fact]
        public void CanGenerateValidToken()
        {
            string token = UserService.GenerateVerifyToken();

            DateTime issued;
            Assert.True(UserService.TryValidateVerificationToken(token, out issued));
            Assert.NotEqual(DateTime.MinValue, issued);
            Assert.True(issued <= DateTime.UtcNow);
        }
    }
}
