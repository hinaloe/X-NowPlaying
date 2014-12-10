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

using NowPlaying.XApplication.Models;
using NowPlaying.XApplication.Views;
using NowPlaying.XApplication.Win32;
using NowPlaying.XApplication.Internal;

using Dolphin.Croudia;
using Dolphin.Croudia.Object;
using Dolphin.Croudia.Rest;

using CoreTweet;
using CoreTweet.Rest;

namespace NowPlaying.XApplication.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private Timer _timer;
        private List<XObject> _objects;

        public MainWindow Window;

        public readonly AccountProvider CroudiaAccountProvider;
        public Tokens TwitterTokens;

        private readonly List<List<XObject>> _parallel = new List<List<XObject>>();
        private XObject _currentObject;
        private int _curCount;

        public MainWindowViewModel()
        {
            this.IsTopmost = Settings.Settings.IsTopLevel;
            this.Title = "X-NowPlaying";
            this.Music = "Loading...";
            this.Album = "Loading...";
            this.Artist = "Loading...";

            //for Croudia
            this.CroudiaAccountProvider = new AccountProvider();
            this.CroudiaAccountProvider.ConsumerKey = Settings.Settings.CroudiaConsumerKey;
            this.CroudiaAccountProvider.ConsumerSecret = Settings.Settings.CroudiaConsumerSecret;
            if (!String.IsNullOrEmpty(Settings.Settings.CroudiaAccessToken) && !String.IsNullOrEmpty(Settings.Settings.CroudiaRefreshToken))
            {
                this.CroudiaAccountProvider.AccessToken = Settings.Settings.CroudiaAccessToken;
                this.CroudiaAccountProvider.RefreshToken = Settings.Settings.CroudiaRefreshToken;
            }

            //for Twitter
            if (!String.IsNullOrEmpty(Settings.Settings.TwitterAccessToken) && !String.IsNullOrEmpty(Settings.Settings.TwitterAccessTokenSecet))
            {
                this.TwitterTokens = Tokens.Create(Settings.Settings.TwitterConsumerKey, Settings.Settings.TwitterConsumerSecret, Settings.Settings.TwitterAccessToken, Settings.Settings.TwitterAccessTokenSecet);
            }
        }

        public void Initialize()
        {
            this.Title = "X-NowPlaying - Loading...";
            this.JacketImage = new BitmapImage(new Uri("/Resources/insert2.png", UriKind.Relative));
            _objects = ApplicationData.Load();
            if (_objects == null)
            {
                Environment.Exit(0);
            }

            //25個に分割
            int i = 0;
            List<XObject> list = null;
            foreach (XObject obj in _objects)
            {
                if (i == 25)
                {
                    _parallel.Add(list);
                    i = 0;
                }
                if (i == 0)
                {
                    list = new List<XObject>();
                }
                list.Add(obj);
                i++;
            }
            //5 sec interval
            this._timer = new Timer(Update, null, 0, 1000 * 5);
        }

        public void Update(object _)
        {
            bool found = false;
            foreach (List<XObject> list in this._parallel)
            {
                var list1 = list;
                Task.Run(() =>
                    {
                        List<string> buf = list1.Select(obj => obj.Object500).ToList();

                        if (GetUsingFiles(buf.ToArray<string>()))
                        {
                            foreach (XObject o in list1)
                            {
                                if (GetUsingFiles(new[] { o.Object500 }))
                                {
                                    var o1 = o;
                                    DispatcherHelper.UIDispatcher.Invoke(() =>
                                    {
                                        found = true;
                                        if (this._currentObject == o1)
                                        {
                                            //曲が切り替わってから最低10秒経過した(曲の早送りでのツイートを拒否するため)
                                            if (_curCount >= 0 && _curCount++ >= 2)
                                            {
                                                //自動投稿
                                                if (Settings.Settings.AutoTweet)
                                                {
                                                    if (this.CanTweet())
                                                    {
                                                        this.Tweet();
                                                    }
                                                    if (this.CanWhisper())
                                                    {
                                                        this.Whisper();
                                                    }
                                                    _curCount = -1;
                                                }
                                            }
                                            return;
                                        }
                                        _curCount = 0;
                                        this.Title = "X-NowPlaying - " + o1.ObjectName;
                                        this.Music = o1.ObjectName;
                                        this.Album = o1.Object206;
                                        this.Artist = o1.Object201;
                                        if (!String.IsNullOrEmpty(o1.Object202))
                                        {
                                            this.JacketImage = new BitmapImage(new Uri(o1.Object202));
                                            this.JacketImageStr = o1.Object202;
                                        }
                                        else
                                        {
                                            this.JacketImage = new BitmapImage(new Uri("/Resources/insert2.png", UriKind.Relative));
                                            this.JacketImageStr = "";
                                        }
                                        this._currentObject = o1;
                                    });
                                }
                            }
                        }
                    });
                if (found)
                {
                    break;
                }
            }
        }

        public string GenerateText()
        {
            string tweet = Settings.Settings.TextFormat;
            tweet = tweet.Replace("%{song}", this.Music);
            tweet = tweet.Replace("%{album}", this.Album);
            tweet = tweet.Replace("%{artist}", this.Artist);
            return tweet;
        }

        private bool GetUsingFiles(IList<string> filePaths)
        {
            uint sessionHandle;
            bool flag = false;

            int rv = NativeMethods.RmStartSession(out sessionHandle, 0, Guid.NewGuid().ToString("N"));
            if (rv != 0)
            {
                throw new Win32Exception();
            }

            try
            {
                string[] pathStrings = new string[filePaths.Count];
                filePaths.CopyTo(pathStrings, 0);
                rv = NativeMethods.RmRegisterResources(sessionHandle, (uint)pathStrings.Length, pathStrings, 0, null, 0, null);
                if (rv != 0)
                {
                    throw new Win32Exception();
                }

                const int ERROR_MORE_DATA = 234;
                const uint RmRebootReasonNone = 0;
                uint pnProcInfoNeeded = 0, pnProcInfo = 0, lpdwRebootReasons = RmRebootReasonNone;
                rv = NativeMethods.RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);
                if (rv == ERROR_MORE_DATA)
                {
                    RM_PROCESS_INFO[] processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = (uint)processInfo.Length;

                    rv = NativeMethods.RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                    if (rv != 0)
                    {
                        throw new Win32Exception();
                    }

                    for (int i = 0; i < pnProcInfo; i++)
                    {
                        try
                        {
                            string name = Process.GetProcessById(processInfo[i].Process.dwProcessId).ProcessName;
                            if (name.Equals("x-APPLICATION") || name.Equals("x-APPLISMO"))
                            {
                                flag = true;
                                break;
                            }
                        }
                        catch (Exception) { }
                    }
                }
                else if (rv != 0)
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                NativeMethods.RmEndSession(sessionHandle);
            }
            return flag;
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
            if (!String.IsNullOrEmpty(Settings.Settings.TwitterAccessToken) && !String.IsNullOrEmpty(Settings.Settings.TwitterAccessTokenSecet))
            {
                return true;
            }
            return false;
        }

        public async void Tweet()
        {
            if (this._currentObject == null)
            {
                return;
            }
            await Task.Run(() =>
                {
                    try
                    {
                        string tweet = this.GenerateText();
                        if (tweet.Contains("%{image}") && !String.IsNullOrEmpty(this.JacketImageStr) && System.IO.File.Exists(this.JacketImageStr))
                        {
                            tweet = tweet.Replace("%{image}", "");
                            this.TwitterTokens.Statuses.UpdateWithMedia(status => tweet, media => System.IO.File.OpenRead(this.JacketImageStr));
                        }
                        else
                        {
                            tweet = tweet.Replace("%{image}", "");
                            this.TwitterTokens.Statuses.Update(status => tweet);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
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
            if (!String.IsNullOrEmpty(Settings.Settings.CroudiaAccessToken) && !String.IsNullOrEmpty(Settings.Settings.CroudiaRefreshToken))
            {
                return true;
            }
            return false;
        }


        public async void Whisper()
        {
            if (this._currentObject == null)
            {
                return;
            }
            await Task.Run(() =>
                {
                    try
                    {
                        string tweet = this.GenerateText();
                        if (tweet.Contains("%{image}") && !String.IsNullOrEmpty(this.JacketImageStr) && System.IO.File.Exists(this.JacketImageStr))
                        {
                            tweet = tweet.Replace("%{image}", "");
                            this.CroudiaAccountProvider.UpdateStatusWithMedia(tweet, this.JacketImageStr);
                        }
                        else
                        {
                            tweet = tweet.Replace("%{image}", "");
                            this.CroudiaAccountProvider.UpdateStatus(tweet);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
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
