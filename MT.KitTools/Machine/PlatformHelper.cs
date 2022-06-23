using MT.KitTools.Machine.HelperImpl;
using MT.KitTools.Machine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MT.KitTools.Machine
{
    internal class PlatformHelper
    {
        internal static ICpu GetCpuHelper()
        {
#if NET48
            return new CpuImplForWindow();
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new CpuImplForWindow();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new PlatformNotSupportedException();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                throw new PlatformNotSupportedException();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                throw new PlatformNotSupportedException();
            }
            else
                throw new Exception("Unknow OsPlatform");
#endif
        }


    }
}
