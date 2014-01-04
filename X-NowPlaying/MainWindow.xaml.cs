using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using X_NowPlaying.X_Application;

namespace X_NowPlaying
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private XObject o;

        public MainWindow()
        {
            InitializeComponent();

            this.NowSong.Text = "取得中...";
            this.NowAlbum.Text = "取得中...";
            this.NowEditor.Text = "取得中...";
        }

        public void Update(XObject o)
        {
            this.o = o;
            this.NowSong.Text = o.SongTitle;
            this.NowAlbum.Text = String.IsNullOrEmpty(o.AlbumName) ? "Unknown" : o.AlbumName;
            this.NowEditor.Text = String.IsNullOrEmpty(o.ArtistName) ? "Unknown" : o.ArtistName;

            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri(o.JacketFile);
                image.EndInit();
                Image img = (Image)this.Jacket;
                img.Source = null;
                img.Source = image;
            } catch (Exception e)
            {
                Image img = (Image)this.Jacket;
                img.Source = null;
            }
            if(o.JacketFile == "")
            {
                Image img = (Image)this.Jacket;
                img.Source = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://twitter.com/intent/tweet?original_referer={0}&text={1}";
            //君の知らない物語 / Today Is A Beautiful Day - supercell
            string format = "{0} / {1} - {2} #NowPlaying";
            if (o == null) return;

            url = String.Format(url, 
                HttpUtility.UrlEncode("http://tuyapin.net/xnowplaying.html"),
                HttpUtility.UrlEncode(String.Format(format, o.SongTitle, o.AlbumName, o.ArtistName))
            );
            System.Diagnostics.Process.Start(url);
        }
    }
}
