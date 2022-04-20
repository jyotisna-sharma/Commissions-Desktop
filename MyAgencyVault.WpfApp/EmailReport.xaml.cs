using MyAgencyVault.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using MyAgencyVault.ViewModel.VMLib;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for EmailReport.xaml
    /// </summary>
    public partial class EmailReport : Window
    {
        public static bool Checkbutton;
        public static string EmailAddress;
        public static bool IsEmailInProgress = false;
        public EmailReport()
        {

            InitializeComponent();


        }
        public void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            IsEmailInProgress = true;
            VMInstances.RepManager.EmailAddress = EmailAddress;
            VMInstances.RepManager.Workerwork();
        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsEmailInProgress = false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<string> emailArray = new List<string>();

            if (textBoxEmail.Text.Trim().Length == 0)
            {
                errormessage.Text = "Email address cannot be blank.";
            }
            else
            {
                Regex rgx = new Regex(@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*(\+[a-z0-9-]+)?@[a-z0-9-]+(\.[a-z0-9-]+)*$");
                string toAddress = textBoxEmail.Text.Trim();
                if (toAddress.IndexOf(",") != -1)
                {
                    emailArray = toAddress.Replace(" ", "").Split(',').ToList();
                }
                else
                {
                    emailArray.Add(toAddress);
                }

                bool isValid = true;
                foreach (string email in emailArray)
                {
                    if (!rgx.IsMatch(email))
                    {
                        // SendEmail(email, subject, body);
                      
                        isValid = false;
                        break;
                    }
                }

                if (!isValid)
                {
                    errormessage.Text = "Email address is not valid.";
                    textBoxEmail.Select(0, textBoxEmail.Text.Length);
                    textBoxEmail.Focus();
                }
                else
                {
                    EmailAddress = textBoxEmail.Text;
                    BackgroundWorker worker = new BackgroundWorker();
                    worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                    worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                    worker.RunWorkerAsync();
                    this.Close();
                }

            }
           /* else if (!Regex.IsMatch(textBoxEmail.Text, @"^(\s?[^\s,]+@[^\s,]+\.[^\s,]+\s?,)*(\s?[^\s,]+@[^\s,]+\.[^\s,]+)$"))
            {
                errormessage.Text = "Email address is not valid.";
                textBoxEmail.Select(0, textBoxEmail.Text.Length);
                textBoxEmail.Focus();
            }
            else
            {

                //Checkbutton = true;
                // VMInstances.RepManager.Checktype = Checkbutton;
                EmailAddress = textBoxEmail.Text;
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += new System.ComponentModel.DoWorkEventHandler(worker_DoWork);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
                worker.RunWorkerAsync();
                //System.Threading.Tasks.Task.Factory.StartNew
                //    (
                //        () => VMInstances.RepManager.Workerwork()
                //    );

                //VMInstances.RepManager.InitMgtReportEmail();
                this.Close();

            }*/
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {

            // Checkbutton = false;
            this.Close();

        }
    }
}
