using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Xml.Linq;
using System.Xml;
using System.Diagnostics;
using System.Net;
using System.Windows;
using MyAgencyVault.VM.CommonItems;
using MyAgencyVault.ViewModel.CommonItems;
using System.Threading;
using MyAgencyVault.VM.BaseVM;
using System.Management;

namespace MyAgencyVault.VM.VMLib
{
  public class VMVersionHelper : BaseViewModel
  {

    private string MSIFilePath = Path.Combine(Path.GetTempPath(), "MyAgencyVaultInstaller.msi");
    private string CmdFilePath = Path.Combine(Path.GetTempPath(), "Install.cmd");
    private string MsiUrl = String.Empty;

    public bool CheckForNewVersion()
    {
        MsiUrl = GetVersionURLFromService();// GetNewVersionUrl();
        return (!string.IsNullOrEmpty(MsiUrl) && MsiUrl.Length > 0);
    }

    public void DownloadNewVersion()
    {
      try
      {
        DownloadNewVersion(MsiUrl);
        CreateCmdFile();
        RunCmdFile();
        ExitApplication();
      }
      catch
      {
        MessageBox.Show("Unable to Download File", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
      /// <summary>
      /// Method to call web service to get URL details of the server
      /// To avoid using public access to versioninfo.xml
      /// </summary>
      /// <returns></returns>
    string GetVersionURLFromService()
    {
        string strVersionUrl = String.Empty;
        try
        {
            DateTime time = DateTime.Now;
            var currentVersion = ConfigurationManager.AppSettings["Version"];

            MyAgencyVaultSvc.NewVersionInfo obj = (new MyAgencyVaultSvc.MavServiceClient()).GetServerURL(currentVersion);
            if(obj != null)
            {
                strVersionUrl = obj.ServerURL;
            }
        }
        catch (Exception ex)
        {
        }
        return strVersionUrl;
    }

    private string GetNewVersionUrl()
    {
        string strVersionUrl = String.Empty;
        try
        {
            DateTime time = DateTime.Now;
            var currentVersion = Convert.ToInt32(ConfigurationManager.AppSettings["Version"]);
            //get xml from url.
           var url = ConfigurationManager.AppSettings["VersionUrl"].ToString();
        //    var url = (new MyAgencyVaultSvc.MavServiceClient())
            var builder = new StringBuilder();
            using (var stringWriter = new StringWriter(builder))
            {
                using (var xmlReader = new XmlTextReader(url))
                {
                    var doc = XDocument.Load(xmlReader);
                    //get versions.
                    var versions = from v in doc.Descendants("version")
                                   select new
                                   {
                                       Name = v.Element("name").Value,
                                       Number = Convert.ToInt32(v.Element("number").Value),
                                       URL = v.Element("url").Value,
                                       Date = Convert.ToDateTime(v.Element("date").Value)
                                   };

                    var version = versions.ToList()[0];
                    //check if latest version newer than current version.
                    string[] CredentialsArray = version.URL.Split(';');
                    if (CredentialsArray.Length < 4)
                    {
                        return strVersionUrl;
                    }
                    if (version.Number > currentVersion)
                    {
                        strVersionUrl = version.URL;
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        return strVersionUrl;
    }

    //private string GetNewVersionUrl()
    //{
    //    string strVersionUrl = String.Empty;
    //    try
    //    {
           

    //        DateTime time = DateTime.Now;
    //        var currentVersion = Convert.ToInt32(ConfigurationManager.AppSettings["Version"]);
    //        //get xml from url.
    //        var url = ConfigurationManager.AppSettings["VersionUrl"].ToString();
    //        ServiceClients obj = new ServiceClients();
    //        int VersionNumber = Convert.ToInt32(obj.MasterClient.GetSystemConstantKeyValue("version"));

    //        if (VersionNumber > currentVersion)
    //        {
    //            //strVersionUrl = version.URL;
    //            strVersionUrl = "http://199.66.132.155/FileManager/Installers;Administrator;Broad2012Broad2012;cdeptsql";
    //        }

    //        var builder = new StringBuilder();
    //        //using (var stringWriter = new StringWriter(builder))
    //        //{
    //        //    using (var xmlReader = new XmlTextReader(url))
    //        //    {
    //        //        var doc = XDocument.Load(xmlReader);
    //        //        //get versions.
    //        //        var versions = from v in doc.Descendants("version")
    //        //                       select new
    //        //                       {
    //        //                           Name = v.Element("name").Value,
    //        //                           Number = Convert.ToInt32(v.Element("number").Value),
    //        //                           URL = v.Element("url").Value,
    //        //                           Date = Convert.ToDateTime(v.Element("date").Value)
    //        //                       };

    //        //        var version = versions.ToList()[0];

    //        //        ServiceClients obj = new ServiceClients();
    //        //        int VersionNumber = Convert.ToInt32(obj.MasterClient.GetSystemConstantKeyValue("version"));

    //        //        //check if latest version newer than current version.
    //        //        string[] CredentialsArray = version.URL.Split(';');
    //        //        if (CredentialsArray.Length < 4)
    //        //        {
    //        //            return strVersionUrl;
    //        //        }
    //        //        if (version.Number > currentVersion)
    //        //        {
    //        //            strVersionUrl = version.URL;
    //        //        }
    //        //    }
    //        //}
    //    }
    //    catch (Exception)
    //    {
    //    }
    //    return strVersionUrl;
    //}

  

    private void DownloadNewVersion(string url)
    {
      AutoResetEvent autoResetEvent = new AutoResetEvent(false);
      //delete existing msi.
      if (File.Exists(MSIFilePath))
      {
        File.Delete(MSIFilePath);
      }
      //download new msi.
     
      string[] CredentialsArray = url.Split(';');
      FileUtility ObjDownload = FileUtility.CreateClient(CredentialsArray[0], CredentialsArray[1], CredentialsArray[2], CredentialsArray[3]);
      ObjDownload.DownloadComplete += (obj1, obj2) =>
      {
        autoResetEvent.Set();
      };
      string RemotePath = @"/MyAgencyVaultInstaller.msi";
      ObjDownload.DownloadFile(RemotePath, MSIFilePath, this);
      autoResetEvent.WaitOne();
    }

    private void CreateCmdFile()
    {
      //check if file exists.
      if (File.Exists(CmdFilePath))
        File.Delete(CmdFilePath);
      //create new file.
      var fi = new FileInfo(CmdFilePath);
      var fileStream = fi.Create();
      fileStream.Close();
      //write commands to file.
      using (TextWriter writer = new StreamWriter(CmdFilePath))
      {
        writer.WriteLine(@"msiexec /i MyAgencyVaultInstaller.msi /passive");
      }
    }

    private void RunCmdFile()
    {//run command file to reinstall app.
      System.Diagnostics.Process proc = new System.Diagnostics.Process(); // Declare New Process
      proc.StartInfo.FileName = Path.Combine(Path.GetTempPath(), "Install.cmd");
      proc.StartInfo.WorkingDirectory = Path.GetTempPath();
      proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
      proc.StartInfo.CreateNoWindow = true;
      proc.Start();
      proc.WaitForExit();
    }

    private void ExitApplication()
    {//exit the app.
      
    }

    private string _downloadValue;
    public String DownloadValue
    {
      get
      {
        return _downloadValue;

      }
      set
      {
        _downloadValue = value;
        OnPropertyChanged("DownloadValue");
      }

    }
  }
}
