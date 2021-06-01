using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.DataTableUtils.Core;
using Shared.Mapper.Core;
using Shared.ReflectionUtils.Core;
using Shared.StringExtension;
using System;
using System.Data;
using System.Linq;

namespace UnitTestFx {
    [TestClass]
    public class UnitTest1 {
        enum Gender {
            Male,
            Female
        }

        class User {
            public string Name { get; set; }
            public int Age { get; set; }
            public bool IsSafe { get; set; }
            public Gender Gender { get; set; }

            public void Update() {
                Age += 1;
            }
        }

        class UserDTO {
            public string Name { get; set; }
            public int Age { get; set; }
            public bool IsSafe { get; set; }
            public Gender Gender { get; set; }

            public void Update() {
                Age += 1;
            }
        }

        DataTable GetTable() {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Age", typeof(int));
            dt.Columns.Add("IsSafe", typeof(bool));
            dt.Columns.Add("Gender", typeof(Gender));
            var row1 = dt.NewRow();
            row1[0] = "管理员";
            row1[1] = 22;
            row1[2] = true;
            row1[3] = Gender.Male;
            dt.Rows.Add(row1);

            var row2 = dt.NewRow();
            row2[0] = "测试员";
            row2[1] = 21;
            row2[2] = false;
            row2[3] = Gender.Female;
            dt.Rows.Add(row2);
            return dt;
        }

        [TestMethod]
        public void TestToEnumerable() {
            var dt = GetTable();
            var user = dt.ToEnumerable<User>().ToArray()[0];
            Assert.AreEqual(user.Name, "管理员");
            Assert.AreEqual(user.Age, 22);
            Assert.AreEqual(user.IsSafe, true);
            Assert.AreEqual(user.Gender, Gender.Male);
        }

        [TestMethod]
        public void TestSelect() {
            var dt = GetTable();
            User[] user = dt.Select<User>(row => row.Value<int>("Age") > 22).ToArray();
            Assert.AreEqual(user.Length, 0);
        }

        [TestMethod]
        public void TestSetProperty() {
            var user = new User();
            user.Set("Name", "hello");
            user.Set("Age", 18);
            Assert.AreEqual(user.Name, "hello");
            Assert.AreEqual(user.Age, 18);
            user.Invoke("Update");
            Assert.AreEqual(user.Age, 19);
        }

        [TestMethod]
        public void TestString() {
            if ("123".IsNumeric<int>(out var v)) {
                Assert.AreEqual(123, v);
            } else
                Assert.Fail();

            if ("123.12".IsNumeric<double>(out var v2)) {
                Assert.AreEqual(123.12, v2);
            } else
                Assert.Fail();

            if ("-123.12".IsNumeric<double>(out var v3)) {
                Assert.AreEqual(-123.12, v3);
            } else
                Assert.Fail();
        }

        [TestMethod]
        public void TestMapper() {
            var user = new User {
                Name = "测试",
                Age = 22,
                IsSafe = true,
                Gender = Gender.Male
            };
            UserDTO userDTO = Mapper.Map<User, UserDTO>(user);
            Mapper.CreateMap<User, UserDTO>(ctx => {
                
            });
        }
    }
}
