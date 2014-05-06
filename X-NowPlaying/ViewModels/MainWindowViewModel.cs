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
using X_NowPlaying.Views;
using X_NowPlaying.Win32;
using X_NowPlaying.Internal;

using Dolphin.Croudia;
using Dolphin.Croudia.Object;
using Dolphin.Croudia.Rest;

using CoreTweet;
using CoreTweet.Rest;

namespace X_NowPlaying.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private Timer timer;
        private List<XObject> objects;

        public MainWindow Window;

        public AccountProvider CroudiaAccountProvider;
        public Tokens TwitterTokens;

        private List<List<XObject>> parallel = new List<List<XObject>>();
        private XObject CurrentObject = null;

        public MainWindowViewModel()
        {
            this.IsTopmost = Settings.IsTopLevel;
            this.Title = "X-NowPlaying";
            this.Music = "Loading...";
            this.Album = "Loading...";
            this.Artist = "Loading...";

            //for Croudia
            this.CroudiaAccountProvider = new AccountProvider();
            this.CroudiaAccountProvider.ConsumerKey = Settings.CroudiaConsumerKey;
            this.CroudiaAccountProvider.ConsumerSecret = Settings.CroudiaConsumerSecret;
            if(!String.IsNullOrEmpty(Settings.CroudiaAccessToken) && !String.IsNullOrEmpty(Settings.CroudiaRefreshToken))
            {
                this.CroudiaAccountProvider.AccessToken = Settings.CroudiaAccessToken;
                this.CroudiaAccountProvider.RefreshToken = Settings.CroudiaRefreshToken;
            }

            //for Twitter
            if(!String.IsNullOrEmpty(Settings.TwitterAccessToken) && !String.IsNullOrEmpty(Settings.TwitterAccessTokenSecet))
            {
                this.TwitterTokens = Tokens.Create(Settings.TwitterConsumerKey, Settings.TwitterConsumerSecret, Settings.TwitterAccessToken, Settings.TwitterAccessTokenSecet);
            }
        }

        public void Initialize()
        {
            this.Title = "X-NowPlaying - Loading...";
            this.JacketImage = new BitmapImage(new Uri("/Resources/insert2.png", UriKind.Relative));
            objects = ApplicationData.Load();
            if(objects == null)
            {
                Environment.Exit(0);
            }

            //25個に分割
            int i = 0;
            List<XObject> list = null;
            foreach(XObject obj in objects)
            {
                if(i == 25)
                {
                    parallel.Add(list);
                    i = 0;
                }
                if(i == 0)
                {
                    list = new List<XObject>();
                }
                list.Add(obj);
                i++;
            }

            //5sec interval
            this.timer = new Timer(Update, null, 0, 1000 * 5);
        }

        public void Update(object _)
        {
            bool found = false;
            foreach(List<XObject> list in this.parallel)
            {
                Task.Run(() =>
                    {
                        List<string> buf = new List<string>();
                        foreach(XObject obj in list)
                        {
                            buf.Add(obj.Object500);
                        }
                        if(WinApi.GetUsingFiles(buf.ToArray<string>()))
                        {
                            foreach(XObject o in list)
                            {
                                if (WinApi.GetUsingFiles(new string[] { o.Object500 }))
                                {
                                    DispatcherHelper.UIDispatcher.Invoke(new Action(() =>
                                    {
                                        found = true;
                                        if(this.CurrentObject == o)
                                        {
                                            return;
                                        }
                                        this.Title = "X-NowPlaying - " + o.ObjectName;
                                        this.Music = o.ObjectName;
                                        this.Album = o.Object206;
                                        this.Artist = o.Object201;
                                        if (!String.IsNullOrEmpty(o.Object202))
                                        {
                                            this.JacketImage = new BitmapImage(new Uri(o.Object202));
                                            this.JacketImageStr = o.Object202;
                                        }
                                        else
                                        {
                                            this.JacketImage = new BitmapImage(new Uri("/Resources/insert2.png", UriKind.Relative));
                                            this.JacketImageStr = "";
                                        }
                                        this.CurrentObject = o;
                                        Console.WriteLine("Update View.");
                                    }));
                                }
                            }
                        }
                    });
                if(found)
                {
                    break;
                }
            }
        }

        public string GenerateText()
        {
            string tweet = Settings.TextFormat;
            tweet = tweet.Replace("%{song}", this.Music);
            tweet = tweet.Replace("%{album}", this.Album);
            tweet = tweet.Replace("%{artist}", this.Artist);
            return tweet;
        }


        #region OpenSettingCommand
        private ViewModelCommand _OpenSettingCommand;

        public ViewModelCommand OpenSettingCommand
        {
            get
            {
                if (_OpenSettingCommand == null)
                {
                    _OpenSettingCommand = new ViewModelCommand(OpenSetting);
                }
                return _OpenSettingCommand;
            }
        }

        public void OpenSetting()
        {
            SettingDialog dialog = new SettingDialog();
            SettingDialogViewModel viewmodel = new SettingDialogViewModel(this);
            dialog.DataContext = viewmodel;
            dialog.Owner = this.Window;
            dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            dialog.ShowDialog();
            this.TweetCommand.RaiseCanExecuteChanged();
            this.WhisperCommand.RaiseCanExecuteChanged();
        }
        #endregion


        #region TweetCommand
        private ViewModelCommand _TweetCommand;

        public ViewModelCommand TweetCommand
        {
            get
            {
                if (_TweetCommand == null)
                {
                    _TweetCommand = new ViewModelCommand(Tweet, CanTweet);
                }
                return _TweetCommand;
            }
        }

        public bool CanTweet()
        {
            if(!String.IsNullOrEmpty(Settings.TwitterAccessToken) && !String.IsNullOrEmpty(Settings.TwitterAccessTokenSecet))
            {
                return true;
            }
            return false;
        }

        public async void Tweet()
        {
            await Task.Run(() =>
                {
                    string tweet = this.GenerateText();
                    if(tweet.Contains("%{image}") && !String.IsNullOrEmpty(this.JacketImageStr))
                    {
                        tweet = tweet.Replace("%{image}", "");
                        this.TwitterTokens.Statuses.UpdateWithMedia(status => tweet, media => System.IO.File.OpenRead(this.JacketImageStr));
                    }
                    else
                    {
                        tweet = tweet.Replace("%{image}", "");
                        this.TwitterTokens.Statuses.Update(status => tweet);
                    }
                });
        }
        #endregion


        #region WhisperCommand
        private ViewModelCommand _WhisperCommand;

        public ViewModelCommand WhisperCommand
        {
            get
            {
                if (_WhisperCommand == null)
                {
                    _WhisperCommand = new ViewModelCommand(Whisper, CanWhisper);
                }
                return _WhisperCommand;
            }
        }

        public bool CanWhisper()
        {
            if (!String.IsNullOrEmpty(Settings.CroudiaAccessToken) && !String.IsNullOrEmpty(Settings.CroudiaRefreshToken))
            {
                return true;
            }
            return false;
        }


        public async void Whisper()
        {
            await Task.Run(() =>
                {
                    string tweet = this.GenerateText();
                    if (tweet.Contains("%{image}") && !String.IsNullOrEmpty(this.JacketImageStr))
                    {
                        System.Windows.MessageBox.Show(this.JacketImageStr);
                        tweet = tweet.Replace("%{image}", "");
                        this.CroudiaAccountProvider.UpdateStatusWithMedia(tweet, this.JacketImageStr);
                    }
                    else
                    {
                        tweet = tweet.Replace("%{image}", "");
                        this.CroudiaAccountProvider.UpdateStatus(tweet);
                    }
                });
        }
        #endregion


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


        #region IsTopmost変更通知プロパティ
        private Boolean _IsTopmost;

        public Boolean IsTopmost
        {
            get
            { return _IsTopmost; }
            set
            { 
                if (_IsTopmost == value)
                    return;
                _IsTopmost = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region JacketImage変更通知プロパティ
        private ImageSource _JacketImage;

        public ImageSource JacketImage
        {
            get
            { return _JacketImage; }
            set
            {
                if (_JacketImage == value)
                    return;
                _JacketImage = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region JacketImageStr変更通知プロパティ
        private string _JacketImageStr;

        public string JacketImageStr
        {
            get
            { return _JacketImageStr; }
            set
            { 
                if (_JacketImageStr == value)
                    return;
                _JacketImageStr = value;
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
