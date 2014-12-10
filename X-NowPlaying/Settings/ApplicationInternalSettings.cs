using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NowPlaying.XApplication.Win32;

namespace NowPlaying.XApplication.Settings
{
    public class ApplicationInternalSettings : ApplicationSettingsBase
    {
        public ApplicationInternalSettings()
            : base("NowPlaying.XApplication.Internal.Settings")
        {

        }

        [UserScopedSetting]
        public WINDOWPLACEMENT? Placement
        {
            get { return this["Placement"] != null ? (WINDOWPLACEMENT?)(WINDOWPLACEMENT)this["Placement"] : null; }
            set { this["Placement"] = value; }
        }
    }
}
