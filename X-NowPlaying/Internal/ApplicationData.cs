using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;

namespace NowPlaying.XApplication.Internal
{
    public static class ApplicationData
    {
        public static List<XObject> Load()
        {
            string key = @"Software\Sony Corporation\Sony MediaPlayerX\Database";
            string dbpath = "";

            RegistryKey registryKey;
            if (Environment.Is64BitOperatingSystem)
            {
                registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(key);
            }
            else
            {
                registryKey = Registry.LocalMachine.OpenSubKey(key);
            }

            if (registryKey == null)
            {
                //旧式
                registryKey = Registry.LocalMachine.OpenSubKey(key);
            }
            dbpath = (string)registryKey.GetValue("MetallicData", "");
            registryKey.Close();

            if (String.IsNullOrEmpty(dbpath))
            {
                MessageBox.Show("X-APPLICATIONが正常にインストールされていません。");
                return null;
            }

            string accessConnection = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};", dbpath);
            string sql = "select * from t_object where ObjectSpecId=2;";

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
                    accessConnection = String.Format("Driver={Microsoft Access Driver (*.mdb)};DBQ={0};", dbpath);
                    connection = new OleDbConnection(accessConnection);
                }
                catch (Exception)
                {
                    MessageBox.Show("Microsoft Data Access ComponentsもしくはMicrosoft Access Driverがインストールされていません。");
                    return null;
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
                MessageBox.Show("X-APPLICATIONの楽曲データベースへのアクセスに失敗しました。");
                return null;
            }
            finally
            {
                connection.Close();
            }

            List<XObject> xobjects = new List<XObject>();
            DataRowCollection collection = ds.Tables["t_object"].Rows;
            foreach (DataRow row in collection)
            {
                var xobj = new XObject(row[2].ToString(), row[69].ToString(), row[70].ToString(), row[74].ToString(), row[111].ToString());
                xobj.Object100 = row[3].ToString();
                xobjects.Add(xobj);
            }
            return xobjects;
        }
    }
}
