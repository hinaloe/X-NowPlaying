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

using X_NowPlaying.Models;
using X_NowPlaying.Internal;

using CoreTweet;
using CoreTweet.Rest;

using Dolphin.Croudia;
using Dolphin.Croudia.Rest;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace X_NowPlaying.ViewModels
{
    public class AuthorizeWindowViewModel : ViewModel
    {
        private ServiceType ServiceType;
        private MainWindowViewModel MainWindowViewModel;
        private CoreTweet.OAuth.OAuthSession OAuthSession;

        public AuthorizeWindowViewModel(MainWindowViewModel main, ServiceType type)
        {
            this.MainWindowViewModel = main;
            this.ServiceType = type;
            this.PinCode = "";
        }

        public void Initialize()
        {
            if(this.ServiceType == Internal.ServiceType.Twitter)
            {
                this.OAuthSession = CoreTweet.OAuth.Authorize(Settings.TwitterConsumerKey, Settings.TwitterConsumerSecret);
                Process.Start(this.OAuthSession.AuthorizeUri.ToString());
            }
            else
            {
                Process.Start(this.MainWindowViewModel.CroudiaAccountProvider.GetAuthorizeUrl("nx3nh6fzjcns"));
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
            if(this.PinCode.Length == 7)
            {
                return true;
            }
            return false;
        }

        public async void Auth()
        {
            await Task.Run(() =>
                {
                    if(this.ServiceType == Internal.ServiceType.Twitter)
                    {
                        this.MainWindowViewModel.TwitterTokens = CoreTweet.OAuth.GetTokens(this.OAuthSession, this.PinCode);
                        Settings.TwitterAccessToken = this.MainWindowViewModel.TwitterTokens.AccessToken;
                        Settings.TwitterAccessTokenSecet = this.MainWindowViewModel.TwitterTokens.AccessTokenSecret;
                        Settings.TwitterScreenName = this.MainWindowViewModel.TwitterTokens.Account.VerifyCredentials().ScreenName;
                    }
                    else
                    {
                        HttpWebRequest request = null;
                        HttpWebResponse response = null;
                        try
                        {
                            request = (HttpWebRequest)WebRequest.Create("http://api.tuyapin.net/starfish/accesstoken.php?code=" + this.PinCode);
                            request.Method = "GET";
                            request.ContentType = "application/x-www-form-urlencoded";

                            response = (HttpWebResponse)request.GetResponse();
                            System.IO.Stream stream = response.GetResponseStream();
                            System.IO.StreamReader sr = new System.IO.StreamReader(stream);
                            string json = sr.ReadToEnd();
                            sr.Close();
                            stream.Close();

                            JArray o = JArray.Parse(json);

                            this.MainWindowViewModel.CroudiaAccountProvider.GetAccessToken((string)o[0]["message"]);
                            Settings.CroudiaAccessToken = this.MainWindowViewModel.CroudiaAccountProvider.AccessToken;
                            Settings.CroudiaRefreshToken = this.MainWindowViewModel.CroudiaAccountProvider.RefreshToken;
                            Settings.CroudiaScreenName = this.MainWindowViewModel.CroudiaAccountProvider.VerifyCredentials().ScreenName;
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
