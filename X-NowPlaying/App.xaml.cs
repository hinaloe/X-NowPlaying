using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using Livet;

using NowPlaying.XApplication.Settings;

namespace NowPlaying.XApplication
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            Settings.Settings.Load();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //TODO:ロギング処理など
            MessageBox.Show(
                ((Exception)e.ExceptionObject).ToString(),
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Settings.Settings.Load();
            Environment.Exit(1);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Settings.Settings.Load();
            base.OnExit(e);
        }
    }
}
