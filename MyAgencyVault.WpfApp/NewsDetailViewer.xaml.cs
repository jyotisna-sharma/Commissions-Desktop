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
using MyAgencyVault.WinApp.Common;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for NewsDetailViewer.xaml
    /// </summary>
    public partial class NewsDetailViewer : Window
    {
        private Mode _mode = Mode.Preview;


        #region"Constructor"

        /// <summary>
        /// Default constructor
        /// </summary>
        public NewsDetailViewer(bool isEditable)
        {
            InitializeComponent();
            if (!isEditable)
            {
                txtUpdateTitle.IsReadOnly = true;
                txtContentTitle.IsReadOnly = true;

                btnSave.IsEnabled = false;
            }
            else
            {
                txtUpdateTitle.IsReadOnly = false;
                txtContentTitle.IsReadOnly = false;

                btnSave.IsEnabled = true;
            }
        }




        ///// <summary>
        ///// Paremetrized constructor
        ///// </summary>
        ///// <param name="mode"></param>
        ///// <param name="newsID"></param>
        //public NewsDetailViewer(Mode mode, Guid newsID)
        //{
        //    InitializeComponent();
        //    _mode = mode;
        //    objHelpUpdate = new HelpUpdateVM(newsID);
        //    this.DataContext = objHelpUpdate;
        //}

        #endregion

        /// <summary>
        /// Set the visibility of form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
