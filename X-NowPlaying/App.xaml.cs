using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

using X_NowPlaying.X_Application;

namespace X_NowPlaying
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private Timer timer;
        private MainWindow main;

        public static string Format = "{0} / {1} - {2} #NowPlaying";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                XElement element = XElement.Load(Directory.GetCurrentDirectory() + "/assets/config.xml");
                var q = from p in element.Elements("Configuration")
                        select new
                        {
                            Format = (string)p.Element("format")
                        };
                foreach(var item in q)
                {
                    Format = item.Format;
                }
            } catch (Exception)
            {

            }

            DataBase.Parse();

            MainWindow main = new MainWindow();
            main.Show();

            this.main = main;

            this.timer = new Timer(Searh, null, 0, 1000 * 10);
        }

        private void Searh(object ob)
        {
            Console.WriteLine("Updating...");
            foreach (X_Application.XObject o in DataBase.Sounds)
            {
                try
                {
                    FileController.Rename(o.SoundFile, o.SoundFile);
                }
                catch (IOException)
                {
                    //now playing
                    main.Dispatcher.Invoke(new Action(() =>
                    {
                        main.Update(o);
                    }));
                    return;
                }
            }
            /*
            main.Dispatcher.Invoke(new Action(() =>
                {
                    main.Update(new X_NowPlaying.X_Application.XObject("取得中...", "取得中...", "取得中...", "", ""));
                }));
            */
        }
    }
}
