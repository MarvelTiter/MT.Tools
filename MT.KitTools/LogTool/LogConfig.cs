using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.KitTools.LogTool
{
    public class LogConfig
    {
        public LogType EnabledLog { get; set; } = LogType.Console | LogType.Debug | LogType.File;
        public string LogDirectory { get; set; }
    }
}
