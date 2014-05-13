using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace X_NowPlaying.Win32
{
    public class WinApi
    {
        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        static extern int RmRegisterResources(uint pSessionHandle, UInt32 nFiles, string[] rgsFileNames, UInt32 nApplications, [In] RM_UNIQUE_PROCESS[] rgApplications, UInt32 nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo, [In, Out] RM_PROCESS_INFO[] rgAffectedApps, ref uint lpdwRebootReasons);

        private const int RmRebootReasonNone = 0;
        private const int CCH_RM_MAX_APP_NAME = 255;
        private const int CCH_RM_MAX_SVC_NAME = 63;

        [StructLayout(LayoutKind.Sequential)]
        struct RM_UNIQUE_PROCESS
        {
            public int dwPeocessId;
            public FILETIME ProcessStartTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
            public string strAppName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
            public string strServiceShortName;

            public RM_APP_TYPE ApplicationType;

            public uint AppStatus;

            public uint TSSessionId;

            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        enum RM_APP_TYPE
        {
            RmUnkownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        // http://msdn.microsoft.com/ja-jp/magazine/cc163450.aspx
        // ↑日本語訳がｱﾚ

        public static bool GetUsingFiles(IList<string> filePaths)
        {
            uint sessionHandle;
            bool flag = false;

            int rv = RmStartSession(out sessionHandle, 0, Guid.NewGuid().ToString("N"));
            if(rv != 0)
            {
                throw new Win32Exception();
            }

            try
            {
                string[] pathStrings = new string[filePaths.Count];
                filePaths.CopyTo(pathStrings, 0);
                rv = RmRegisterResources(sessionHandle, (uint)pathStrings.Length, pathStrings, 0, null, 0, null);
                if(rv != 0)
                {
                    throw new Win32Exception();
                }

                const int ERROR_MORE_DATA = 234;
                uint pnProcInfoNeeded = 0, pnProcInfo = 0, lpdwRebootReasons = RmRebootReasonNone;
                rv = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);
                if(rv == ERROR_MORE_DATA)
                {
                    RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = (uint)processInfo.Length;

                    rv = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                    if(rv != 0)
                    {
                        throw new Win32Exception();
                    }

                    for(int i = 0; i < pnProcInfo; i++)
                    {
                        try
                        {
                            string name = Process.GetProcessById(processInfo[i].Process.dwPeocessId).ProcessName;
                            if (name.Equals("x-APPLICATION") || name.Equals("x-APPLISMO"))
                            {
                                flag = true;
                                break;
                            }
                        }
                        catch (Exception) { }
                    }
                }
                else if(rv != 0)
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                RmEndSession(sessionHandle);
            }
            return flag;
        }
    }
}
