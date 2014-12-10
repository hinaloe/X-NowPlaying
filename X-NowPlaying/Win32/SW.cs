using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NowPlaying.XApplication.Win32
{
    public enum SW : int
    {
        HIDE = 0,

        SHOWNORMAL = 1,

        SHOWMINIMIZED = 2,

        SHOWMAXIMIZED = 3,

        SHOWNOACTIVATE = 4,

        SHOW = 5,

        MINIMIZE = 6,

        SHOWMINNOACTIVE = 7,

        SHOWNA = 8,

        RESTORE = 9,

        SHOWDEFAULT = 10,

        FORCEMINIMIZE = 11,

        NORMAL = SHOWNORMAL,

        MAXIMIZE = SHOWMAXIMIZED,

        MAX = SHOWNOACTIVATE
    }
}
