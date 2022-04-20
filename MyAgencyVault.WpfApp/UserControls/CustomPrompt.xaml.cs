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

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for CustomPrompt.xaml
    /// </summary>
    public partial class CustomPrompt : Window
    {
        //private string _imagePath;
        //public string imagePath
        //{
        //    get
        //    {
        //        return _imagePath;
        //    }
        //    set
        //    {
        //        _imagePath = value;
        //        OnPropertyChanged("imagePath");
        //    }
        //}
        public CustomPrompt()
        {
            InitializeComponent();
            //string strLocalPath = AppDomain.CurrentDomain.BaseDirectory;
            //string strSavePath = @"\Images\Icons\floppy_disk_blue.png";

            //imagePath = strLocalPath + strSavePath;
        }

        
        public string SelectedOption
        {
            get;
            set;
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveAll_Checked(object sender, RoutedEventArgs e)
        {
            if(e.Source is RadioButton)
            {
                SelectedOption = string.IsNullOrEmpty(Convert.ToString((e.Source as RadioButton).Tag)) ? "OnlyNew" : Convert.ToString((e.Source as RadioButton).Tag); //handles the blank case, which is on default selection
            }
        }
    }
}
