using MT.KitTools.StringExtension;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MT.KitTools.LogTool
{
    public class FileLogger : ILogger
    {
        public LogConfig LogConfig { get; set; }
        static readonly ConcurrentQueue<(string, string)> logQueue = new ConcurrentQueue<(string, string)>();
        private AutoResetEvent Pause => new AutoResetEvent(false);
        private string separator = "----------------------------------------------------------------------------------------------------------------------";
        CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        public FileLogger()
        {
            var writeTask = new Task(obj =>
            {
                while (true)
                {
                    Pause.WaitOne(1000, true);
                    List<string[]> temp = new List<string[]>();
                    foreach (var logItem in logQueue)
                    {
                        string logPath = logItem.Item1;
                        string logMergeContent = $"{logItem.Item2}{Environment.NewLine}{separator}{Environment.NewLine}";
                        string[] logArr = temp.FirstOrDefault(d => d[0].Equals(logPath));
                        if (logArr != null)
                        {
                            logArr[1] = string.Concat(logArr[1], logMergeContent);
                        }
                        else
                        {
                            logArr = new[]
                            {
                                logPath,
                                logMergeContent
                            };
                            temp.Add(logArr);
                        }

                        logQueue.TryDequeue(out (string, string) _);
                    }

                    foreach (var item in temp)
                    {
                        WriteText(item[0], item[1]);
                    }
                }
            }, null, CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            writeTask.Start();
        }

        public void WriteLog(LogInfo logInfo)
        {
            logQueue.Enqueue((GetLogPath(), logInfo.FormatLogMessage()));
        }
        private string GetLogPath()
        {
            string newFilePath;
            var logDir = string.IsNullOrEmpty(LogConfig?.LogDirectory) ? Path.Combine(Environment.CurrentDirectory, "logs") : LogConfig?.LogDirectory;
            Directory.CreateDirectory(logDir);
            string extension = ".log";
            string fileNameNotExt = $"{DateTime.Now:yyyy-MM-dd}_Part";
            string fileNamePattern = string.Concat(fileNameNotExt, "*", extension);
            List<string> filePaths = Directory.GetFiles(logDir, fileNamePattern, SearchOption.TopDirectoryOnly).ToList();

            if (filePaths.Count > 0)
            {
                int fileMaxLen = filePaths.Max(d => d.Length);
                string lastFilePath = filePaths.Where(d => d.Length == fileMaxLen).OrderByDescending(d => d).FirstOrDefault();
                if (new FileInfo(lastFilePath).Length > 1 * 1024 * 1024)
                {
                    var no = new Regex(@"(?<=Part)(\d+)").Match(Path.GetFileName(lastFilePath)).Value;
                    var parse = int.TryParse(no, out int tempno);
                    var formatno = $"{(parse ? tempno + 1 : tempno)}";
                    var newFileName = string.Concat(fileNameNotExt, formatno, extension);
                    newFilePath = Path.Combine(logDir, newFileName);
                }
                else
                {
                    newFilePath = lastFilePath;
                }
            }
            else
            {
                var newFileName = string.Concat(fileNameNotExt, $"{0}", extension);
                newFilePath = Path.Combine(logDir, newFileName);
            }

            return newFilePath;
        }

        private static void WriteText(string logPath, string logContent)
        {
            try
            {
                if (!File.Exists(logPath))
                {
                    File.CreateText(logPath).Close();
                }
#if (NET5_0_OR_GREATER)
                using var sw = File.AppendText(logPath);
                sw.Write(logContent);
#else
                using (var sw = File.AppendText(logPath))
                {
                    sw.Write(logContent);
                }
#endif
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Dispose()
        {
            CancellationTokenSource.Cancel();
        }
    }
}
