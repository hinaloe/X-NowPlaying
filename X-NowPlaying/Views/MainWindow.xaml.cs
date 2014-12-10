using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NowPlaying.XApplication.Settings;
using NowPlaying.XApplication.Views;
using NowPlaying.XApplication.Win32;

namespace NowPlaying.XApplication.Views
{
    /* 
     * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                ((ViewModels.MainWindowViewModel)this.DataContext).Window = this;
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (Settings.Settings.ApplicationInternalSettings.Placement.HasValue)
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var placement = Settings.Settings.ApplicationInternalSettings.Placement.Value;
                placement.Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                placement.Flags = 0;
                placement.ShowCmd = placement.ShowCmd == (int)SW.SHOWMINIMIZED ? (int)SW.SHOWNORMAL : placement.ShowCmd;

                NativeMethods.SetWindowPlacement(hwnd, ref placement);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                WINDOWPLACEMENT placement;
                var hwnd = new WindowInteropHelper(this).Handle;
                NativeMethods.GetWindowPlacement(hwnd, out placement);

                Settings.Settings.ApplicationInternalSettings.Placement = placement;
            }
            Settings.Settings.Save();
        }
    }
}
