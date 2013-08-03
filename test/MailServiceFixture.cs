namespace tinyBugs.test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using RobMensching.TinyBugs.Services;
    using Xunit;

    public class MailServiceFixture
    {
        [Fact]
        public void CanSendEmail()
        {
            MailService.SendVerifyUser("rob@robmensching.com", "tokengoeshere");
        }
    }
}
