using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.DataTableUtils.Core;
using System;
using System.Data;
using System.Linq;

namespace UnitTestFx {
    [TestClass]
    public class UnitTest1 {
        enum Gender {
            Boy,
            Girl
        }

        class User {
            public string Name { get; set; }
            public int Age { get; set; }
            public bool IsSafe { get; set; }
            public Gender Gender { get; set; }
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
            row1[3] = Gender.Boy;
            dt.Rows.Add(row1);

            var row2 = dt.NewRow();
            row2[0] = "测试员";
            row2[1] = 21;
            row2[2] = false;
            row2[3] = Gender.Girl;
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
            Assert.AreEqual(user.Gender, Gender.Boy);
        }

        [TestMethod]
        public void TestSelect() {
            var dt = GetTable();
            User[] user = dt.Select<User>(row => row.Value<int>("Age") > 22).ToArray();
            Assert.AreEqual(user.Length,0);
        }
    }
}
