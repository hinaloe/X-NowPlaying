using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace X_NowPlaying
{
    public class Settings
    {
        public static string TextFormat { set; get; }

        public static string TwitterAccessToken { set; get; }

        public static string TwitterAccessTokenSecet { set; get; }

        public static string TwitterScreenName { set; get; }

        public static string CroudiaAccessToken { set; get; }

        public static string _CroudiaRefreshToken;
        public static string CroudiaRefreshToken
        {
            set
            {
                if (Equals(_CroudiaRefreshToken, value) || String.IsNullOrEmpty(value))
                    return;

                _CroudiaRefreshToken = value;
                Settings.Save();
            }
            get
            {
                return _CroudiaRefreshToken;
            }
        }

        public static string CroudiaScreenName { set; get; }

        public static bool IsTopLevel { set; get; }

        public static bool AutoTweet { set; get; }

        public static string TwitterConsumerKey
        {
            get { return "lHf2QI9xSFXKcmShszqfmtg85"; }
        }

        public static string TwitterConsumerSecret
        {
            get { return "HaDtZs1l5p0PS3o7yDKEp6EUERImSb8hK67Fftan7vcs6m77l1"; }
        }

        public static string CroudiaConsumerKey
        {
            get { return "1326cc850376c7de3d9aaf2b569ba9df3b916a0a6f5fc568601b1c6c46910203"; }
        }

        public static string CroudiaConsumerSecret
        {
            get { return "be1912ee3abefc02e459d69f76a1a0951b831eed14508d94980a77dcf2676a6c"; }
        }

        public static void Load()
        {
            try
            {
                var element = XElement.Load(System.IO.Directory.GetCurrentDirectory() + "\\settings.xml");
                var q = from p in element.Elements("Configuration")
                        select new
                        {
                            CroudiaAccessToken = (string)p.Element("CroudiaAccessToken"),
                            CroudiaRefreshToken = (string)p.Element("CroudiaRefreshToken"),
                            CroudiaScreenName = (string)p.Element("CroudiaScreenName"),
                            TwitterAceessToken = (string)p.Element("TwitterAccessToken"),
                            TwitterAccessTokenSecet = (string)p.Element("TwitterAccessTokenSecret"),
                            TwitterScreenName = (string)p.Element("TwitterScreenName"),
                            TextFormat = (string)p.Element("TextFormat"),
                            IsTopLevel = Boolean.Parse((string)p.Element("IsTopLevel")),
                            AutoTweet = p.Element("AutoTweet") == null ? false : Boolean.Parse((string)p.Element("AutoTweet"))
                        };
                foreach (var item in q)
                {
                    Settings.TextFormat = item.TextFormat;
                    Settings.TwitterAccessToken = item.TwitterAceessToken;
                    Settings.TwitterAccessTokenSecet = item.TwitterAccessTokenSecet;
                    Settings.TwitterScreenName = item.TwitterScreenName;
                    Settings.CroudiaAccessToken = item.CroudiaAccessToken;
                    Settings.CroudiaRefreshToken = item.CroudiaRefreshToken;
                    Settings.CroudiaScreenName = item.CroudiaScreenName;
                    Settings.IsTopLevel = item.IsTopLevel;
                    Settings.AutoTweet = item.AutoTweet;
                }
            }
            catch (Exception)
            {
                Settings.TextFormat = "%{song} - %{artist} / %{album} #NowPlaying";
                Settings.TwitterAccessToken = "";
                Settings.TwitterAccessTokenSecet = "";
                Settings.TwitterScreenName = "";
                Settings.CroudiaAccessToken = "";
                Settings.CroudiaRefreshToken = "";
                Settings.CroudiaScreenName = "";
                Settings.IsTopLevel = false;
                Settings.AutoTweet = false;
            }
        }

        public static void Save()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";

            XmlWriter xw = XmlWriter.Create(System.IO.Directory.GetCurrentDirectory() + "//settings.xml", settings);
            xw.WriteStartElement("X-NowPlaying");
            xw.WriteStartElement("Configuration");
            xw.WriteElementString("TwitterAccessToken", Settings.TwitterAccessToken);
            xw.WriteElementString("TwitterAccessTokenSecret", Settings.TwitterAccessTokenSecet);
            xw.WriteElementString("TwitterScreenName", Settings.TwitterScreenName);
            xw.WriteElementString("CroudiaAccessToken", Settings.CroudiaAccessToken);
            xw.WriteElementString("CroudiaRefreshToken", Settings.CroudiaRefreshToken);
            xw.WriteElementString("CroudiaScreenName", Settings.CroudiaScreenName);
            xw.WriteElementString("TextFormat", Settings.TextFormat);
            xw.WriteElementString("IsTopLevel", Settings.IsTopLevel.ToString());
            xw.WriteElementString("AutoTweet", Settings.AutoTweet.ToString());
            xw.WriteEndElement();
            xw.WriteEndElement();
            xw.Close();
        }
    }
}
