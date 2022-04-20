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
using System.Data;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for CommissionDashEdit.xaml
    /// </summary>
    public partial class CommissionDashEdit : Window
    {
        public CommissionDashEdit()
        {
            InitializeComponent();
            ////DgEditCommission.ItemsSource = this.getOutCommission.DefaultView;  
        }
        DataTable _getOutGoingCommissions;
        DataTable getOutCommission
        {
            get
            {
                if (_getOutGoingCommissions == null)
                {
                    _getOutGoingCommissions = getOutGoingCommissions();

                }
                return _getOutGoingCommissions;

            }
        }
        DataTable getOutGoingCommissions()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Payee");
            dt.Columns.Add("Premium");
            dt.Columns.Add("Payment");

            dt.Columns.Add("OutGoing Per Unit");
            dt.Columns.Add("Total Due To Payee");
           
            DataRow dr;

            dr = dt.NewRow();
            dr["Payee"] = "Smith John";
            dr["Premium"] = "2.00 %";
            dr["Payment"] = "5%";
            dr["OutGoing Per Unit"] = "30%";
            dr["Total Due To Payee"] = "$ 85.00";
           
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["Payee"] = "Wood Eric";
            dr["Premium"] = "1.00 %";
            dr["Payment"] = "4%";
            dr["OutGoing Per Unit"] = "6%";
            dr["Total Due To Payee"] = "$ 85.00";
           
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["Payee"] = "House LTD";
            dr["Premium"] = "4.00 %";
            dr["Payment"] = "2%";
            dr["OutGoing Per Unit"] = "3%";
            dr["Total Due To Payee"] = "$ 385.00";
          
            dt.Rows.Add(dr);


            return dt;
        }
    }
}
