using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X_NowPlaying.Internal
{
    public class XObject
    {
        /// <summary>
        /// 楽曲名
        /// </summary>
        public string ObjectName { private set; get; }

        /// <summary>
        /// アーティスト名
        /// </summary>
        public string Object201 { private set; get; }

        /// <summary>
        /// ジャケットのパス
        /// </summary>
        public string Object202 { private set; get; }

        /// <summary>
        /// アルバムのパス
        /// </summary>
        public string Object206 { private set; get; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        public string Object500 { private set; get; }

        
        // =====================================================================================================
        // Extended
        // -----------------------------------------------------------------------------------------------------
        public string Object100 { set; get; }

        public XObject(string obj1, string obj201, string obj202, string obj206, string obj500)
        {
            this.ObjectName = obj1;
            this.Object201 = obj201;
            this.Object202 = obj202;
            this.Object206 = obj206;
            this.Object500 = obj500;
        }
    }
}
