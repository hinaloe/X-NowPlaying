using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_NowPlaying.X_Application
{
    public class XObject
    {
        /// <summary>
        /// Song title
        /// </summary>
        public string SongTitle;

        /// <summary>
        /// Artist name
        /// </summary>
        public string ArtistName;

        /// <summary>
        /// Album name
        /// </summary>
        public string AlbumName;

        /// <summary>
        /// Sound file location path
        /// </summary>
        public string SoundFile;

        /// <summary>
        /// Jacket file location path
        /// </summary>
        public string JacketFile;

        public XObject(string song, string artist, string album, string sound, string jacket)
        {
            this.SongTitle = song;
            this.ArtistName = artist;
            this.AlbumName = album;
            this.SoundFile = sound;
            this.JacketFile = jacket;
        }
    }
}
