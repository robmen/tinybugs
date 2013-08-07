namespace tinyBugs.test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Xunit;
    using RobMensching.TinyBugs.ViewModels;

    public class RelativeDateFixture
    {
        [Fact]
        public void RelativeYears()
        {
            DateTime o = DateTime.UtcNow.Subtract(new TimeSpan(560, 0, 0, 0));
            RelativeDateViewModel vm = new RelativeDateViewModel(o);

            Assert.Equal<string>("a year and half ago", vm.ToString());
            Assert.Equal<string>(o.Year + "-" + o.Month.ToString("00") + "-" + o.Day.ToString("00"), vm.Date);
        }

        [Fact]
        public void RelativeToday()
        {
            DateTime o = DateTime.UtcNow.Subtract(new TimeSpan(3, 0, 0));
            RelativeDateViewModel vm = new RelativeDateViewModel(o);

            Assert.Equal<string>("earlier today", vm.ToString());
        }

        [Fact]
        public void RelativeYesterday()
        {
            DateTime o = DateTime.UtcNow.Subtract(new TimeSpan(24, 0, 0));
            RelativeDateViewModel vm = new RelativeDateViewModel(o);

            Assert.Equal<string>("yesterday", vm.ToString());
        }

        [Fact]
        public void RelativeDays()
        {
            DateTime o = DateTime.UtcNow.Subtract(new TimeSpan(3, 0, 0, 0));
            RelativeDateViewModel vm = new RelativeDateViewModel(o);

            Assert.Equal<string>("3 days ago", vm.ToString());
        }
    }
}
