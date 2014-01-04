using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.VisualBasic.FileIO;

namespace X_NowPlaying
{
    public class FileController
    {
        public static void CSVLoad(String file, params List<String>[] list)
        {
            TextReader tr = new StreamReader(file, Encoding.UTF8);
            try
            {
                TextFieldParser parser = new TextFieldParser(tr);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.TrimWhiteSpace = false;
                while (parser.EndOfData == false)
                {
                    String[] fields = parser.ReadFields();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (i < list.Length)
                        {
                            list[i].Add(fields[i]);
                        }
                    }
                }
            }
            catch (Exception) { }
            tr.Dispose();
        }

        /// <summary>
        /// ディレクトリを作成します。
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectory(String path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// ファイル、もしくはフォルダーを削除します。
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(String path)
        {
            if (path.EndsWith("/") || path.EndsWith("\\"))
            {
                return;
            }
            System.Diagnostics.Debug.WriteLine(path);
            if (File.Exists(path))
            {
                DeleteFile(path);
            }
            else if (Directory.Exists(path))
            {
                DeleteDirectory(path);
            }
        }

        /// <summary>
        /// ディレクトリを削除します。
        /// </summary>
        /// <param name="path">パス</param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        private static void DeleteDirectory(String path)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                foreach (FileInfo info in dirInfo.GetFiles())
                {
                    if ((info.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        info.Attributes = FileAttributes.Normal;
                    }
                }

                foreach (DirectoryInfo info in dirInfo.GetDirectories())
                {
                    DeleteDirectory(info.FullName);
                }

                if ((dirInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    dirInfo.Attributes = FileAttributes.Directory;
                }

                dirInfo.Delete(true);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Directory:" + path + "にアクセスできませんでした。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw ex;
            }
        }

        /// <summary>
        /// ファイルを削除します。
        /// </summary>
        /// <param name="path">削除するファイル</param>
        private static void DeleteFile(String path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// ファイル、フォルダーが存在するかを返します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Boolean Exists(String path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            else if (File.Exists(path))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ファイルもしくはフォルダーをコピーします。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        public static void Coly(String source, String dest)
        {
            //ファイルの場合
            if (File.Exists(source))
            {
                if (!Directory.Exists(Path.GetDirectoryName(dest)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                }
                File.Copy(source, dest);
                return;
            }

            //フォルダーの場合
            if (Directory.Exists(source))
            {
                if (!Directory.Exists(dest))
                {
                    Directory.CreateDirectory(dest);
                }
                dest += "\\";
                String[] files = Directory.GetFiles(source, "*.*", System.IO.SearchOption.AllDirectories);
                foreach (String file in files)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(file)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(file));
                    }
                    File.Copy(file, dest + file, true);
                }
                return;
            }
        }

        /// <summary>
        /// ファイル名を変更します。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        public static void Rename(String path, String name)
        {
            File.Move(path, name);
        }
    }
}
