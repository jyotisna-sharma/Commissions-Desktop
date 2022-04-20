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
using System.Windows.Navigation;
using System.Windows.Shapes;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
using System.IO;
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for ReportManager.xaml
    /// </summary>
    /// 
    
    
    public partial class RepManager : UserControl
    {
        EmailReport emreport = null;
        public RepManager()
        {
            InitializeComponent();
            VMInstances.RepManager.OpenEmailWindow += RepManager_OpenEmailWindow;
        }

        private void RepManager_OpenEmailWindow()
        {
            //emreport.ShowDialog();
            try
            {
                if (EmailReport.IsEmailInProgress)
                {
                    MessageBox.Show("A report is already being sent with email in the background. Please try after sometime.");
                    return;
                }
                //VMInstances.RepManager.EmailAddress = EmailReport.EmailAddress;

                if (emreport == null)
                {
                    EmailReport emreport = new EmailReport();
                    emreport.ShowDialog();
                }
                // VMInstances.RepManager.Checktype = EmailReport.Checkbutton;

            }
            catch (Exception e)
            {
                //ActionLogger.Logger.WriteLog("background email sending not working" + e.Message, true);
                
            }
        }

        private void tbReportManager_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string str = e.Source.ToString();
                if (str.Contains("TabControl"))
                {
                    if (VMInstances.RepManager != null)
                    {
                        string header = ((System.Windows.Controls.HeaderedContentControl)(((System.Windows.Controls.Primitives.Selector)(e.Source)).SelectedItem)).Header.ToString();
                        if (header == "Payee Statement Reports")
                        {
                            VMInstances.RepManager.SelectedTab = "Payee";
                        }
                        else if (header == "Management Reports")
                        {
                            VMInstances.RepManager.SelectedTab = "Management";
                        }
                        else
                        {
                            VMInstances.RepManager.SelectedTab = "Audit";
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
