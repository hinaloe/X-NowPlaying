using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using NowPlaying.XApplication.Models;
using NowPlaying.XApplication.Internal;

using CoreTweet;
using CoreTweet.Rest;

using Dolphin.Croudia;
using Dolphin.Croudia.Rest;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NowPlaying.XApplication.ViewModels
{
    public class AuthorizeWindowViewModel : ViewModel
    {
        private readonly ServiceType _serviceType;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private CoreTweet.OAuth.OAuthSession _oAuthSession;

        public AuthorizeWindowViewModel(MainWindowViewModel main, ServiceType type)
        {
            this._mainWindowViewModel = main;
            this._serviceType = type;
            this.PinCode = "";
        }

        public void Initialize()
        {
            if (this._serviceType == ServiceType.Twitter)
            {
                this._oAuthSession = CoreTweet.OAuth.Authorize(Settings.Settings.TwitterConsumerKey, Settings.Settings.TwitterConsumerSecret);
                Process.Start(this._oAuthSession.AuthorizeUri.ToString());
            }
            else
            {
                Process.Start(this._mainWindowViewModel.CroudiaAccountProvider.GetAuthorizeUrl("XPQJsHFGgFdh"));
            }
        }


        #region AuthCommand
        private ViewModelCommand _AuthCommand;

        public ViewModelCommand AuthCommand
        {
            get
            {
                if (_AuthCommand == null)
                {
                    _AuthCommand = new ViewModelCommand(Auth, CanAuth);
                }
                return _AuthCommand;
            }
        }

        public bool CanAuth()
        {
            if (this.PinCode.Length == 7)
            {
                return true;
            }
            return false;
        }

        public async void Auth()
        {
            await Task.Run(() =>
                {
                    if (this._serviceType == Internal.ServiceType.Twitter)
                    {
                        this._mainWindowViewModel.TwitterTokens = CoreTweet.OAuth.GetTokens(this._oAuthSession, this.PinCode);
                        Settings.Settings.TwitterAccessToken = this._mainWindowViewModel.TwitterTokens.AccessToken;
                        Settings.Settings.TwitterAccessTokenSecet = this._mainWindowViewModel.TwitterTokens.AccessTokenSecret;
                        Settings.Settings.TwitterScreenName = this._mainWindowViewModel.TwitterTokens.Account.VerifyCredentials().ScreenName;
                    }
                    else
                    {
                        HttpWebRequest request = null;
                        HttpWebResponse response = null;
                        try
                        {
                            // Deprecated
                            // Use "https://api.tuyapin.net".
                            //request = (HttpWebRequest)WebRequest.Create("http://api.tuyapin.net/starfish/accesstoken.php?code=" + this.PinCode);
                            request = (HttpWebRequest)WebRequest.Create("https://api.tuyapin.net/oauth/accesstoken.php?code=" + this.PinCode);
                            request.Method = "GET";
                            request.ContentType = "application/x-www-form-urlencoded";

                            response = (HttpWebResponse)request.GetResponse();
                            System.IO.Stream stream = response.GetResponseStream();
                            System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                            string json = sr.ReadToEnd();
                            sr.Close();
                            stream.Close();

                            var o = JArray.Parse(json);

                            this._mainWindowViewModel.CroudiaAccountProvider.GetAccessToken((string)o[0]["message"]);
                            Settings.Settings.CroudiaAccessToken = this._mainWindowViewModel.CroudiaAccountProvider.AccessToken;
                            Settings.Settings.CroudiaRefreshToken = this._mainWindowViewModel.CroudiaAccountProvider.RefreshToken;
                            Settings.Settings.CroudiaScreenName = this._mainWindowViewModel.CroudiaAccountProvider.VerifyCredentials().ScreenName;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                });
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "WindowAction"));
        }
        #endregion


        #region PinCode変更通知プロパティ
        private string _PinCode;

        public string PinCode
        {
            get
            { return _PinCode; }
            set
            {
                if (_PinCode == value)
                    return;
                _PinCode = value;
                RaisePropertyChanged();
                this.AuthCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

    }
}
