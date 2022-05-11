using MT.KitTools.EnumExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitTools.Test
{
    public class EnumTest
    {
        [SetUp]
        public void Setup()
        {

        }

        enum TestEnum
        {
            [Display(Name = "字符串")]
            String,
            [Display(Name = "数字")]
            Number,
            Function
        }

        [Test]
        public void EnumDisplayNameTest()
        {
            var str = TestEnum.String.GetDisplayName();
            Assert.IsTrue(str == "字符串");
            var str2 = TestEnum.Function.GetDisplayName();
            Assert.IsTrue(str2 == "Function");
        }

        class State
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        [Test]
        public void TestGenerate()
        {
            var eType = EnumHelper.GenerateEnumType(TestStateEnum(), s => s.Name, i => i.Value);

        }

        IEnumerable<State> TestStateEnum()
        {
            yield return new State() { Name = "待提交", Value = 11 };
            yield return new State() { Name = "待审核", Value = 22 };
            yield return new State() { Name = "待复核", Value = 33 };
            yield return new State() { Name = "已复核", Value = 44 };
            yield return new State() { Name = "已归档", Value = 55 };
        }
    }
}
