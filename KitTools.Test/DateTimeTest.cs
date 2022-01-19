using MT.KitTools.DateTimeExtension;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitTools.Test
{
    public class DateTimeTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test()
        {
            var d = DateTime.Now;
            Assert.IsTrue($"{DateTime.Now:yyyy-MM-dd} 00:00:00" == d.DayStartStr());
            Assert.IsTrue($"{DateTime.Now:yyyy-MM-dd} 23:59:59" == d.DayEndStr());
        }
    }
}
