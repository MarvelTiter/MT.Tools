using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MT.KitTools.LogTool;

namespace KitTools.Test
{

    internal class LoggerTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void LogTest()
        {
            Logger.Enable(LogType.Console | LogType.File);
            Logger.Info("测试");
            Console.Read();
        }
    }
}
