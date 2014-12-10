using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NowPlaying.XApplication.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public class RM_UNIQUE_PROCESS
    {
        public int dwProcessId;

        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }
}
