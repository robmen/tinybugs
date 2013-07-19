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
            Assert.Equal(22, user.Salt.Length);
            Assert.Equal(86, user.PasswordHash.Length);
            Assert.False(user.Verified);
        }
    }
}
