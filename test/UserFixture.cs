namespace tinyBugs.test
{
    using System;
    using RobMensching.TinyBugs.Services;
    using Xunit;

    public class UserFixture
    {
        [Fact]
        public void CanCreateNewUser()
        {
            var user = UserService.Create("tinybugs@robmensching.com", "My voice is my password.");
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.Equal("tinybugs@robmensching.com", user.Email);
            Assert.Equal(24, user.Salt.Length);
            Assert.Equal(88, user.PasswordHash.Length);
            Assert.False(user.Verified);
        }

        [Fact]
        public void CanGenerateGravatarImageUrl()
        {
            var url = UserService.GetGravatarImageUrlForEmail("rob@robmensching.com");
            Assert.Equal("http://www.gravatar.com/avatar/179453ce28338e22260a03651e80ac44?r=g&d=mm", url);
        }
    }
}
