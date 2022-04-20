using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ActionLogger
{
    public class Logger
    {
        public static void WriteLog(Exception ex, bool IsServer)
        {

            //File.Create("E:\\"+DateTime.Today.Year);
            //File.OpenWrite("E:\\" + DateTime.Today.Year);
            //File.Create("E:\\ErrorLog\\" + String.Format("{0:d_M_yyyy_HH_mm_ss}"+".txt", DateTime.Today)).Close();
            try
            {
                Environment.SpecialFolder special = Environment.SpecialFolder.MyDocuments;
                string folderLocation = Environment.GetFolderPath(special);
                folderLocation += "\\ErrorLog" + (IsServer ? "\\Server_\\" : "\\Client_\\");
                Directory.CreateDirectory(folderLocation);
                using (StreamWriter strw = new StreamWriter(folderLocation + String.Format("{0:d_M_yyyy}" + ".txt", DateTime.Today), true))
                {
                    strw.WriteLine("------InnerException----------");
                    try
                    {
                        strw.WriteLine(ex.InnerException.Message);
                    }
                    catch
                    {
                    }
                    strw.WriteLine("------StackTrace----------");

                    strw.WriteLine(ex.StackTrace);
                    strw.WriteLine("------Message----------");

                    strw.WriteLine(ex.Message);
                    strw.Close();
                }

            }
            catch
            {

            }

        }

        public static void WriteLog(string msg, bool IsServer)
        {
            try
            {
                Environment.SpecialFolder special = Environment.SpecialFolder.MyDocuments;
                string folderLocation = Environment.GetFolderPath(special);
                folderLocation += "\\ErrorLog" + (IsServer ? "\\Server_\\" : "\\Client_\\");
                Directory.CreateDirectory(folderLocation);
                using (StreamWriter strw = new StreamWriter(folderLocation + String.Format("{0:d_M_yyyy}" + ".txt", DateTime.Today), true))
                {
                    strw.WriteLine(DateTime.Now.ToString()+ ": " + msg);
                    strw.Close();
                }
            }
            catch
            {

            }
            
        }

        public static void WriteImportLog(string msg, bool IsServer)
        {
            try
            {
                Environment.SpecialFolder special = Environment.SpecialFolder.MyDocuments;
                string folderLocation = Environment.GetFolderPath(special);
                folderLocation += "\\ErrorLog" + (IsServer ? "\\ServerImportLog_\\" : "\\ClientImportLog_\\");
                Directory.CreateDirectory(folderLocation);
                using (StreamWriter strw = new StreamWriter(folderLocation + String.Format("{0:d_M_yyyy}" + ".txt", DateTime.Today), true))
                {
                    strw.WriteLine(msg);
                    strw.Close();
                }
            }
            catch
            {
                Environment.SpecialFolder special = Environment.SpecialFolder.MyDocuments;
                string folderLocation = Environment.GetFolderPath(special);
                folderLocation += "\\ErrorLog" + (IsServer ? "\\ServerCrarsh_\\" : "\\ClientCrarsh_\\");
                Directory.CreateDirectory(folderLocation);
                using (StreamWriter strw = new StreamWriter(folderLocation + String.Format("{0:d_M_yyyy}" + ".txt", DateTime.Today), true))
                {
                    strw.WriteLine(msg);
                    strw.Close();
                }
            }

        }

        /// <summary>
        /// New method to save import policy logs at different location
        /// to reduce original log file size and ease of use.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="IsServer"></param>
        public static void WriteImportPolicyLog(string msg, bool IsServer)
        {
            try
            {
                Environment.SpecialFolder special = Environment.SpecialFolder.MyDocuments;
                string folderLocation = Environment.GetFolderPath(special);
                folderLocation += "\\ErrorLog" + (IsServer ? "\\ServerImportPolicyLog_\\" : "\\ClientImportLog_\\");
                Directory.CreateDirectory(folderLocation);
                using (StreamWriter strw = new StreamWriter(folderLocation + String.Format("{0:d_M_yyyy}" + ".txt", DateTime.Today), true))
                {
                    strw.WriteLine(DateTime.Now.ToString() + ": " + msg);
                    strw.Close();
                }
            }
            catch
            {
                Environment.SpecialFolder special = Environment.SpecialFolder.MyDocuments;
                string folderLocation = Environment.GetFolderPath(special);
                folderLocation += "\\ErrorLog" + (IsServer ? "\\ServerCrarsh_\\" : "\\ClientCrarsh_\\");
                Directory.CreateDirectory(folderLocation);
                using (StreamWriter strw = new StreamWriter(folderLocation + String.Format("{0:d_M_yyyy}" + ".txt", DateTime.Today), true))
                {
                    strw.WriteLine(DateTime.Now.ToString() + ": " + msg);
                    strw.Close();
                }
            }

        }
    }
}
