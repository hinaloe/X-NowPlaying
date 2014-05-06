using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using X_NowPlaying.Models;
using X_NowPlaying.Win32;
using X_NowPlaying.Internal;

namespace X_NowPlaying.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private Timer timer;
        private List<XObject> objects;

        public MainWindowViewModel()
        {
            this.Title = "X-NowPlaying";
        }

        public void Initialize()
        {
            this.Title = "X-NowPlaying - Loading...";
            this.JucketImage = new BitmapImage(new Uri("/Resources/insert2.png", UriKind.Relative));
            objects = ApplicationData.Load();
            if(objects == null)
            {
                Environment.Exit(0);
            }
            //5sec interval
            this.timer = new Timer(Update, null, 0, 1000 * 5);
        }

        public void Update(object _)
        {
            bool found = false;
            foreach(XObject o in this.objects)
            {
                Task.Run(() =>
                    {
                        if(WinApi.GetUsingFiles(new string[] { o.Object500 }))
                        {
                            DispatcherHelper.UIDispatcher.Invoke(new Action(() =>
                                {
                                    found = true;
                                    this.Title = "X-NowPlaying - ♪" + o.ObjectName;
                                    this.Music = o.ObjectName;
                                    this.Album = o.Object206;
                                    this.Artist = o.Object201;
                                    if (!String.IsNullOrEmpty(o.Object202))
                                    {
                                        this.JucketImage = new BitmapImage(new Uri(o.Object202));
                                    }
                                    else
                                    {
                                        this.JucketImage = new BitmapImage(new Uri("/Resources/insert2.png", UriKind.Relative));
                                    }
                                }));
                        }
                    });
                if(found)
                {
                    break;
                }
            }
        }


        #region Title変更通知プロパティ
        private string _Title;

        public string Title
        {
            get
            { return _Title; }
            set
            { 
                if (_Title == value)
                    return;
                _Title = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region JucketImage変更通知プロパティ
        private ImageSource _JucketImage;

        public ImageSource JucketImage
        {
            get
            { return _JucketImage; }
            set
            { 
                if (_JucketImage == value)
                    return;
                _JucketImage = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Music変更通知プロパティ
        private string _Music;

        public string Music
        {
            get
            { return _Music; }
            set
            { 
                if (_Music == value)
                    return;
                _Music = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Album変更通知プロパティ
        private string _Album;

        public string Album
        {
            get
            { return _Album; }
            set
            { 
                if (_Album == value)
                    return;
                _Album = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region Atrist変更通知プロパティ
        private string _Artist;

        public string Artist
        {
            get
            { return _Artist; }
            set
            { 
                if (_Artist == value)
                    return;
                _Artist = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
