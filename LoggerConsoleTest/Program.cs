// See https://aka.ms/new-console-template for more information

using MT.KitTools.LogTool;

Logger.Enable(LogType.Console | LogType.File);
for (int i = 0; i < 10; i++)
{
    Logger.Warn($"测试{i}");
    Task.Delay(1000).Wait();
}
Logger.Disable(LogType.File);
for (int i = 0; i < 10; i++)
{
    Logger.Warn($"测试{i}");
    Task.Delay(1000).Wait();
}
Console.Read();