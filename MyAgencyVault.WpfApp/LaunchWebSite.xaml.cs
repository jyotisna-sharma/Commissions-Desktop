using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Windows.Navigation;

using MyAgencyVault.VMLib;
using MyAgencyVault.VM.VMLib;
using System.Net;
using System.Windows.Forms;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for LaunchWebSite.xaml
    /// </summary>
    public partial class LaunchWebSite : Window
    {

        DownloadBatchVM _downloadBatch;
        VMStatementManager _vmStatementManaegr;
        int intProg = 0;
        System.Windows.Forms.WebBrowser webBrowser;

        public LaunchWebSite(VMStatementManager statementManager)
        {
            InitializeComponent();
            _vmStatementManaegr = statementManager;
            _downloadBatch = statementManager.CurrentDownloadBatch;

            intProg = statementManager.CurrentProgress;
            

            webBrowser = (System.Windows.Forms.WebBrowser)browserHost.Child;

            if (_downloadBatch.Url.StartsWith("http"))
            {
                webBrowser.Navigate(_downloadBatch.Url);
            }
            else
            {
                webBrowser.Navigate("http://" + _downloadBatch.Url);
            }

            //SetUserNamePassword(webBrowser, "http://" + _downloadBatch.Url, _downloadBatch.UserNameControl, _downloadBatch.UserName, _downloadBatch.PasswordControl, _downloadBatch.Password);

            
           //mshtml.HTMLDocument dom = (mshtml.HTMLDocument)myBrowser.Document;
            //dom.co
           // webBrowser.DocumentText = HtmlString;
             //if(_downloadBatch.Url.StartsWith("http"))
            //    myBrowser.Navigate(_downloadBatch.Url);
            //else
            //    myBrowser.Navigate("http://" + _downloadBatch.Url);
        }

        public void SetUserNamePassword(string sURL, string sUserNameID, string sUserName, string sPasswordID, string sPassword)
        {
            try
            {
                HtmlElement ele = null;
                if (!string.IsNullOrEmpty(sUserNameID))
                    ele = webBrowser.Document.GetElementById(sUserNameID);

                if (ele == null)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(webBrowser.Document)))
                    {
                        HtmlElementCollection htmlElements = webBrowser.Document.GetElementsByTagName("input");
                        foreach (HtmlElement element in htmlElements)
                        {
                            if (element.Name == sUserNameID)
                            {
                                ele = element;
                                break;
                            }
                        }
                    }

                }

                if (ele != null)
                    ele.InnerText = sUserName;

                if (!string.IsNullOrEmpty(Convert.ToString(webBrowser.Document)))
                {
                    ele = webBrowser.Document.GetElementById(sPasswordID);
                }
                if (ele == null)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(webBrowser.Document)))
                    {
                        HtmlElementCollection htmlElements = webBrowser.Document.GetElementsByTagName("input");
                        foreach (HtmlElement element in htmlElements)
                        {
                            if (element.Name == sPasswordID)
                            {
                                ele = element;
                                break;
                            }
                        }
                    }
                }

                if (ele != null)
                    ele.InnerText = sPassword;

                //ele = webBrowser.Document.GetElementById("Login");
                //if (ele != null)
                //    ele.InvokeMember("click");
            }
            catch
            {
            }
        }

        //public string GetHTMLPageString(string sURL, string sUserNameID, string sUserName, string sPasswordID, string sPassword)
        //{
        //    string sTemp = "";
        //    string sTempPre = "";
        //    string sTempPost = "";
        //    string responseText = "";
        //    int iPos = 0;
        //    int iPos1 = 0;
        //    //SEND REQUEST
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sURL);
        //    request.Method = "GET";
        //    request.ContentType = "text/http";
        //    request.ContentLength = 0;
        //    //RETRIEVE RESPONSE

        //    using (StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream()))
        //    {
        //        responseText = sr.ReadToEnd();
        //    }
        //    //FillUserName
        //    iPos = responseText.LastIndexOf("id=\"" + sUserNameID + "\"");
        //    if (iPos != -1)
        //    {
        //        sTempPre = responseText.Substring(0, iPos);
        //        iPos = sTempPre.LastIndexOf("<input");
        //        sTempPre = responseText.Substring(0, iPos);
        //        sTempPost = responseText.Substring(iPos);
        //        iPos1 = sTempPost.IndexOf(">");
        //        sTemp = sTempPost.Substring(0, iPos1);
        //        sTempPost = sTempPost.Substring(iPos1);

        //        if (sTemp.Contains("value=\"\""))
        //            sTemp = sTemp.Replace("value=\"\"", "value=" + "\"" + sUserName + "\"");
        //        else
        //            sTemp += " value=\"" + sUserName + "\"";

        //        responseText = sTempPre + sTemp + sTempPost;
        //    }
        //    //FillPassword
        //    iPos = responseText.LastIndexOf("id=\"" + sPasswordID + "\"");
        //    if (iPos > 0)
        //    {
        //        sTempPre = responseText.Substring(0, iPos);
        //        iPos = sTempPre.LastIndexOf("<input");
        //        sTempPre = responseText.Substring(0, iPos);
        //        sTempPost = responseText.Substring(iPos);
        //        iPos1 = sTempPost.IndexOf(">");
        //        sTemp = sTempPost.Substring(0, iPos1);
        //        sTempPost = sTempPost.Substring(iPos1);
                
        //        if (sTemp.Contains("value=\"\""))
        //            sTemp = sTemp.Replace("value=\"\"", "value=\"" + sPassword + "\"");
        //        else
        //            sTemp += " value=\"" + sPassword + "\"";

        //        responseText = sTempPre + sTemp + sTempPost;
        //    }
        //    return responseText;
        //}

        private void WebBrowser_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            if (e.Url != null)
            {
                if (e.Url.ToString().EndsWith(".txt") || e.Url.ToString().EndsWith(".pdf") || e.Url.ToString().EndsWith(".csv") || e.Url.ToString().EndsWith(".xls") || e.Url.ToString().EndsWith(".xlsm"))
                {
                    _vmStatementManaegr.DownloadBatchFile(e.Url.ToString(), _downloadBatch);
                    _vmStatementManaegr.CurrentDownloadBatch.DownloadBatch.FileType = System.IO.Path.GetExtension(e.Url.ToString()).Replace(".", "");
                    this.Close();
                }
            }
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_downloadBatch.Url.StartsWith("http"))
            {
                SetUserNamePassword(_downloadBatch.Url, _downloadBatch.UserNameControl, _downloadBatch.UserName, _downloadBatch.PasswordControl, _downloadBatch.Password);
               
            }
            else
            {
                SetUserNamePassword("http://" + _downloadBatch.Url, _downloadBatch.UserNameControl, _downloadBatch.UserName, _downloadBatch.PasswordControl, _downloadBatch.Password);
            }
        }

        //private void myBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        //{
        //    if (e.Uri != null)
        //    {
        //        if (e.Uri.ToString().EndsWith(".txt") || e.Uri.ToString().EndsWith(".pdf") || e.Uri.ToString().EndsWith(".csv") || e.Uri.ToString().EndsWith(".xls") || e.Uri.ToString().EndsWith(".xlsm"))
        //        {
        //            _vmStatementManaegr.DownloadBatchFile(e.Uri.ToString(), _downloadBatch);
        //            _vmStatementManaegr.CurrentDownloadBatch.DownloadBatch.FileType = System.IO.Path.GetExtension(e.Uri.ToString()).Replace(".", "");
        //            this.Close();
        //        }
        //    }
        //}

        //private void myBrowser_Navigated(object sender, NavigationEventArgs e)
        //{
            
        //}
    }
}

