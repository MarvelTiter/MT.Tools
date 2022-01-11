// See https://aka.ms/new-console-template for more information

using MT.KitTools.Log;

Logger.Enable(LogType.Console | LogType.File);
Logger.Info("info");
Logger.Error("error");
Logger.Fatal("fatal");
Logger.Debug("debug");
Logger.Info("info");
Logger.Info("info");
Logger.Info("info");
Logger.Warn("警告");
Console.Read();