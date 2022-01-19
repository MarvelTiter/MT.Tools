// See https://aka.ms/new-console-template for more information

using MT.KitTools.LogTool;
Logger.EnableAllDefault();
for (int i = 0; i < 10; i++)
{
    Logger.Info($"测试{i}");
    Task.Delay(1000).Wait();
}
Console.Read();