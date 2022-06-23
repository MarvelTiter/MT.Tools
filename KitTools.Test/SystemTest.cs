using MT.KitTools.Machine;
using NUnit.Framework;
using System.Threading.Tasks;

namespace KitTools.Test
{
    public class SystemTest
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void SysInfo()
        {
            System.Console.WriteLine(SysHelper.OsPlatform());
            System.Console.WriteLine(SysHelper.OSArchitecture());
            System.Console.WriteLine(SysHelper.ProcessArchitecture());
            System.Console.WriteLine(SysHelper.OSVersion());
            System.Console.WriteLine(SysHelper.OSDescription());
            System.Console.WriteLine(SysHelper.HostName());
            System.Console.WriteLine(SysHelper.RunningTime());
        }
        [Test]
        public async Task CpuInfo()
        {
            System.Console.WriteLine(CpuHelper.ProcessorCount());
            System.Console.WriteLine(CpuHelper.CpuTotalUsage());
            System.Console.WriteLine(await CpuHelper.CpuProcessUsageAsync());
        }

        [Test]
        public void MemoryInfo()
        {
            var b = MemoryHelper.ProcessMemoryUsage();
            var b2 = MemoryHelper.TotalUsage();
            System.Console.WriteLine($"b:{b}, kb:{b / 1024}, mb:{b / 1024 / 1024}");
            System.Console.WriteLine($"b:{b2}, kb:{b2 / 1024}, mb:{b2 / 1024 / 1024}, gb:{b2 / 1024 / 1024 / 1024}");

        }
    }
}
