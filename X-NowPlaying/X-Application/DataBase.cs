using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;


namespace X_NowPlaying.X_Application
{
    public class DataBase
    {
        public static List<XObject> Sounds = new List<XObject>();

        public static void Parse()
        {
            //眠い

            string skey = @"Software\Sony Corporation\Sony MediaPlayerX\Database";
            string dbpath = "";

            //32bit は　死ね
            RegistryKey key = Registry.LocalMachine.OpenSubKey(skey);
            dbpath = (String)key.GetValue("MetallicData", "");
            key.Close();

            if(String.IsNullOrEmpty(dbpath))
            {
                //なかったし(ﾟ⊿ﾟ)ｼﾗﾈ
                System.Windows.MessageBox.Show("X-APPLICATIONのデータベースロケーションが取得できませんでした。");
                return;
            }

            string accessConnection = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dbpath + ";";
            string sql = "select * from t_object where ObjectSpecID=2;";

            DataSet ds = new DataSet();
            OleDbConnection connection = null;
            try
            {
                connection = new OleDbConnection(accessConnection);
            }
            catch (Exception)
            {
                try
                {
                    accessConnection = "Driver={Microsoft Access Driver (*.mdb)};DBQ=" + dbpath + ";";
                    connection = new OleDbConnection();
                } catch (Exception)
                {
                    //諦めて
                    return;
                }
            }

            try
            {
                OleDbCommand command = new OleDbCommand(sql, connection);
                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                connection.Open();
                adapter.Fill(ds, "t_object");
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                connection.Close();
            }

            /*
            int i = 0;
            DataColumnCollection dcc = ds.Tables["t_object"].Columns;
            foreach (DataColumn dc in dcc)
            {
                Console.WriteLine("Column name[{0}] is {1}, of type {2}", i++, dc.ColumnName, dc.DataType);
            }
             * */

            DataRowCollection drc = ds.Tables["t_object"].Rows;
            foreach(DataRow row in drc)
            {
                Sounds.Add(new XObject(row[2].ToString(), row[69].ToString(), row[74].ToString(), row[111].ToString(), row[70].ToString()));
            }
        }
    }
}
