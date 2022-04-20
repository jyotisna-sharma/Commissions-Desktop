using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for CustomSchedule.xaml
    /// </summary>
    public partial class CustomSchedule : Window
    {

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        private object dataGridView1;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        string gridHeader = "% of Premium";
        public CustomSchedule(string headerText)
        {
            InitializeComponent();
            gridHeader = headerText;
        }
       
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {

        }
        private void RdbCustomMode2_Checked(object sender, RoutedEventArgs e)
        {
            grdGraded.Visibility = Visibility.Collapsed;
            grdNonGraded.Visibility = Visibility.Visible;
        }

        private void RdbCustomMode1_Checked(object sender, RoutedEventArgs e)
        {
            grdGraded.Visibility = Visibility.Visible;
            grdNonGraded.Visibility = Visibility.Collapsed;
        }
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // add Loaded handler through designer
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            int dwOldLong = GetWindowLong(hWnd, GWL_STYLE);
            SetWindowLong(hWnd, GWL_STYLE, dwOldLong & ~WS_SYSMENU);
            grdGraded.Columns[2].Header = gridHeader;
            grdNonGraded.Columns[1].Header = gridHeader;
        }
        private void MakeZeroForEmptyCells(object sender,DataGridViewCellValidatingEventArgs e)
        {
        //    //foreach (DataGrid row in grdGraded.Rows)
        //    //{
        //    //    foreach (DataGridViewCell cell in row.Cells)
        //    //    {
        //    //        string value = Convert.ToString(cell.Value);
        //    //        cell.Value = string.IsNullOrWhiteSpace(value) ? 0 : cell.Value;
        //    //    }

        //    //}
        }
    

        private void GrdGraded_CellEditEnding(object sender, Microsoft.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var value= (e.EditingElement as System.Windows.Controls.TextBox).Text;
            if (value == "" ||value == null )
            {
                (e.EditingElement as System.Windows.Controls.TextBox).Text = "0";
            }
        }

        private void GrdNonGraded_CellEditEnding(object sender, Microsoft.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            var value = (e.EditingElement as System.Windows.Controls.TextBox).Text;
            if (value == "" || value == null)
            {
                (e.EditingElement as System.Windows.Controls.TextBox).Text = "0";
            }
        }
    }
}
