using MT.KitTools.DataTableExtension;
using MT.KitTools.LogTool;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitTools.Test
{
    public class DataTableTest
    {
        [SetUp]
        public void Setup()
        {

        }

        class TestModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public DateTime Birthdate { get; set; }
        }

        [Test]
        public void CustomTest()
        {
            var idTable = new DataTable();
            idTable.Columns.Add("Id", typeof(int));
            idTable.Columns.Add("Birthdate", typeof(DateTime));
            var r1 = idTable.NewRow();
            r1["Id"] = 10;
            r1["Birthdate"] = DateTime.Now.AddDays(-10);
            idTable.Rows.Add(r1);

            var nameTable = new DataTable();
            nameTable.Columns.Add("Name", typeof(string));
            var r2 = nameTable.NewRow();
            r2["Name"] = "测试";
            nameTable.Rows.Add(r2);

            var tm = new TestModel();
            tm.MapFromTable(idTable);
            tm.MapFromTable(nameTable);
            Assert.True(tm.Id == "10");
            Assert.True(tm.Name == "测试");
        }
    }
}
