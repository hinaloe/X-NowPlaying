using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NowPlaying.XApplication.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    public class RM_PROCESS_INFO
    {
        public RM_UNIQUE_PROCESS Process;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH.RM_MAX_APP_NAME + 1)]
        public string strAppName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH.RM_MAX_SVC_NAME + 1)]
        public string strServiceShortName;

        public RM_APP_TYPE ApplicationType;

        public uint AppStatus;

        public uint TSSessiond;

        [MarshalAs(UnmanagedType.Bool)]
        public bool bRestartable;
    }
}
