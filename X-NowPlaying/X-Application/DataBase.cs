using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualBasic.FileIO;


namespace X_NowPlaying.X_Application
{
    public class DataBase
    {
        public static List<XObject> Sounds = new List<XObject>();

        public static void Parse()
        {
            //var engine = new ScriptEngine();
            //engine.ExecuteFile("assets/application.js");

            var proc = System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/assets/application.js");
            proc.WaitForExit();

            //生成されたらパース
            StreamReader sr = new StreamReader("xapplication.db", Encoding.UTF8);
            try
            {
                TextFieldParser parser = new TextFieldParser(sr);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.TrimWhiteSpace = false;
                while (parser.LineNumber != -1)
                {
                    String[] fields = parser.ReadFields();
                    Sounds.Add(new XObject(fields[0].Replace("%22", "\""), fields[1].Replace("%22", "\""), fields[2].Replace("%22", "\""), fields[3], fields[4]));
                }
            }
            catch (Exception e) {
            }
            sr.Dispose();

            //FileController.Delete("xapplication.db");
        }
    }
}
