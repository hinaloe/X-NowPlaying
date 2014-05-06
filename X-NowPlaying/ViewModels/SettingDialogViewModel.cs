using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using X_NowPlaying.Models;
using X_NowPlaying.Views;

namespace X_NowPlaying.ViewModels
{
    public class SettingDialogViewModel : ViewModel
    {

        public SettingDialog Dialog;
        private MainWindowViewModel main;

        public SettingDialogViewModel(MainWindowViewModel main)
        {
            this.main = main;
        }

        public void Initialize()
        {
            this.TextFormat = Settings.TextFormat;
            if(String.IsNullOrEmpty(Settings.TwitterScreenName))
            {
                this.TwitterScreenName = "Not Connected";
            }
            else
            {
                this.TwitterScreenName = "@" + Settings.TwitterScreenName;
            }

            if(String.IsNullOrEmpty(Settings.CroudiaScreenName))
            {
                this.CroudiaScreenName = "Not Connected";
            }
            else
            {
                this.CroudiaScreenName = "@" + Settings.CroudiaScreenName;
            }
        }

        #region AuthorizeTwitterCommand
        private ViewModelCommand _AuthorizeTwitterCommand;

        public ViewModelCommand AuthorizeTwitterCommand
        {
            get
            {
                if (_AuthorizeTwitterCommand == null)
                {
                    _AuthorizeTwitterCommand = new ViewModelCommand(AuthorizeTwitter);
                }
                return _AuthorizeTwitterCommand;
            }
        }

        public void AuthorizeTwitter()
        {
            AuthorizeWindow window = new AuthorizeWindow();
            AuthorizeWindowViewModel viewmodel = new AuthorizeWindowViewModel(this.main, Internal.ServiceType.Twitter);
            window.DataContext = viewmodel;
            window.Owner = this.Dialog;
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }
        #endregion


        #region AuthorizeCroudiaCommand
        private ViewModelCommand _AuthorizeCroudiaCommand;

        public ViewModelCommand AuthorizeCroudiaCommand
        {
            get
            {
                if (_AuthorizeCroudiaCommand == null)
                {
                    _AuthorizeCroudiaCommand = new ViewModelCommand(AuthorizeCroudia);
                }
                return _AuthorizeCroudiaCommand;
            }
        }

        public void AuthorizeCroudia()
        {
            AuthorizeWindow window = new AuthorizeWindow();
            AuthorizeWindowViewModel viewmodel = new AuthorizeWindowViewModel(this.main, Internal.ServiceType.Croudia);
            window.DataContext = viewmodel;
            window.Owner = this.Dialog;
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            window.ShowDialog();
        }
        #endregion


        #region OKCommand
        private ViewModelCommand _OKCommand;

        public ViewModelCommand OKCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new ViewModelCommand(OK);
                }
                return _OKCommand;
            }
        }

        public void OK()
        {
            Settings.TextFormat = this.TextFormat;
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "WindowAction"));
        }
        #endregion


        #region TextFormat変更通知プロパティ
        private string _TextFormat;

        public string TextFormat
        {
            get
            { return _TextFormat; }
            set
            { 
                if (_TextFormat == value)
                    return;
                _TextFormat = value;
                this.TextCount = (140 - this._TextFormat.Length);
                RaisePropertyChanged();
            }
        }
        #endregion


        #region TextCount変更通知プロパティ
        private int _TextCount;

        public int TextCount
        {
            get
            { return _TextCount; }
            set
            { 
                if (_TextCount == value)
                    return;
                _TextCount = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region TwitterScreenName変更通知プロパティ
        private string _TwitterScreenName;

        public string TwitterScreenName
        {
            get
            { return _TwitterScreenName; }
            set
            { 
                if (_TwitterScreenName == value)
                    return;
                _TwitterScreenName = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        #region CroudiaScreenName変更通知プロパティ
        private string _CroudiaScreenName;

        public string CroudiaScreenName
        {
            get
            { return _CroudiaScreenName; }
            set
            { 
                if (_CroudiaScreenName == value)
                    return;
                _CroudiaScreenName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

    }
}
