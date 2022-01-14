using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MT.KitTools.LogTool
{
    public class LogException : Exception
    {
        public LogException(Exception ex) : base(ex.Message, ex)
        {

        }
        public bool Handled { get; set; }
    }
}
