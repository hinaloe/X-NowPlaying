using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NowPlaying.XApplication.Win32
{
    public enum RM_APP_TYPE
    {
        RmUnkownApp = 0,

        RmMainWindow = 1,

        RmOtherWindow = 2,

        RmService = 3,

        RmExplorer = 4,

        RmConsole = 5,

        RmCritical = 1000
    }
}
