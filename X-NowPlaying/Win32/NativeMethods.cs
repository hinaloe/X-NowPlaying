using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NowPlaying.XApplication.Win32
{
    public static class NativeMethods
    {
        [DllImport("user32")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        [DllImport("rstrtmgr", CharSet = CharSet.Unicode)]
        public static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr", CharSet = CharSet.Unicode)]
        public static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr", CharSet = CharSet.Unicode)]
        public static extern int RmRegisterResources(uint pSessionHandle, UInt32 nFiles, string[] rgsFileNames, UInt32 nApplications, [In] RM_UNIQUE_PROCESS[] rgApplications, UInt32 nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr", CharSet = CharSet.Unicode)]
        public static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo, [In, Out] RM_PROCESS_INFO[] rgAffectedApps, ref uint lpdwRebootReasons);
    }
}
