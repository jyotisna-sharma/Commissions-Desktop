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
    /// Interaction logic for PolicList.xaml
    /// </summary>
    public partial class PolicList : Window
    {
        public PolicList()
        {
            InitializeComponent();
            //bindPolcies(0);
        }
        //private void bindPolcies(int iStatus)
        //{
        //   PolicyManagerClient objService = new PolicyManagerClient();
        //    ConfigurationsClient objServiceConfig = new ConfigurationsClient();
        //    try
        //    {
        //        List<PolicyDetail> lstPolicies = new List<PolicyDetail>();

        //        if (iStatus != 0)
        //        {
        //            lstPolicies = objService.GetPolicies(159).Where(c => c.Status == iStatus).ToList<PolicyDetail>();
        //        }
        //        else
        //        {
        //            lstPolicies = objService.GetPolicies(159).ToList<PolicyDetail>();
        //        }
        //        ClientDetail[] lstClient = objService.GetClientList();

        //        AgentPayor[] lstAgentPayor = objServiceConfig.GetAgentPayorList(159);

        //       AgentCarrier[] lstAgentCarrier = objServiceConfig.GetPayorCarrier(0);

        //        Product[] lstProduct = objServiceConfig.GetAgentProductList(159);

        //        var vQuery = from p in lstPolicies
        //                     join cp in lstClient on p.Client equals cp.ClientId
        //                     join pa in lstAgentPayor on p.Payor equals pa.PayorId into tempPayor
        //                     join ac in lstAgentCarrier on p.Carrier equals ac.CarrierId into tempCarrier
        //                     join pr in lstProduct on p.Coverage equals pr.ProductId into tempProduct
        //                     from t in tempPayor.DefaultIfEmpty()
        //                     from c in tempCarrier.DefaultIfEmpty()
        //                     from prod in tempProduct.DefaultIfEmpty()
        //                     orderby cp.ClientName, p.Status, p.Coverage, p.Carrier, p.OrigEffectiveDate
        //                     select new
        //                     {
        //                         p.PolicyId,
        //                         PolicyNumber = p.PolicyNumber,
        //                         cp.ClientId,
        //                         //Client = cp.ClientName,
        //                         InsuredDiv = p.InsuredDiv,
        //                         Carrier = (c == null ? null : c.NickName),
        //                         //cp.City,
        //                         EffectiveDate = p.OrigEffectiveDate.Value.ToShortDateString(),
        //                         Coverage = (prod == null ? null : prod.NickName),
        //                         Status =
        //                            p.Status == 1 ? "Active" :
        //                            p.Status == 2 ? "Terminated" :
        //                            p.Status == 3 ? "Pending" :
        //                            p.Status == 4 ? "Deleted" :
        //                            "None",
        //                         CompType =
        //                            p.CompType == 1 ? "Commission" :
        //                            p.CompType == 2 ? "Override" :
        //                            p.CompType == 3 ? "Bonus" :
        //                            p.CompType == 4 ? "Fee" :
        //                            "None"
        //                     };



        //        //dgPolicies.ItemsSource = null;
        //        //dgPolicies.ItemsSource = vQuery.ToArray();
        //    }
        //    catch (Exception ex)
        //    {
        //        //Common.MyAgencyvaultException.ErrorHandle(ex, System.Reflection.MethodInfo.GetCurrentMethod().ReflectedType.FullName.ToString(), System.Reflection.MethodInfo.GetCurrentMethod().Name.ToString());
        //    }
        //    finally
        //    {
        //        objService.Close();
        //        objService = null;
        //    }

        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // dgPolicies.ItemsSource = this.DtPolicies.DefaultView;  
        }
        DataTable dtPolicies;
        DataTable getPolicies()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Policy#");
            dt.Columns.Add("Insured");
            dt.Columns.Add("Carrier");
            dt.Columns.Add("Effective");
            dt.Columns.Add("Product");

            dt.Columns.Add("Status");
            dt.Columns.Add("Comp Type");
            //dt.Columns.Add("PolicyId");
            //dt.Columns.Add("ClientId");          



            DataRow dr;

            dr = dt.NewRow();
            //dr["PolicyId"] = "123";
            dr["Policy#"] = "12345";
            //dr["ClientId"] = "26";
            dr["Insured"] = "Rajesh";
            dr["Carrier"] = "ICICI";
            dr["Effective"] = "12/12/2010";
            dr["Product"] = "ICICI prudential";
            dr["Status"] = "Active";
            dr["Comp Type"] = "Commission";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            ////dr["PolicyId"] = "124";
            dr["Policy#"] = "12346";
            //dr["ClientId"] = "24";
            dr["Insured"] = "Rajesh Singh";
            dr["Carrier"] = "Oxford";
            dr["Effective"] = "12/1/2011";
            dr["Product"] = "Oxford Health Plan";
            dr["Status"] = "Active";
            dr["Comp Type"] = "Commission";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            ////dr["PolicyId"] = "125";
            dr["Policy#"] = "12347";
            //dr["ClientId"] = "23";
            dr["Insured"] = "Pankaj";
            dr["Carrier"] = "Kodak";
            dr["Effective"] = "12/11/2010";
            dr["Product"] = "Kodak Child insurance";
            dr["Status"] = "Active";
            dr["Comp Type"] = "Commission";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            ////dr["PolicyId"] = "126";
            dr["Policy#"] = "12348";
            //dr["ClientId"] = "23";
            dr["Insured"] = "Jeorge";
            dr["Carrier"] = "Kodak";
            dr["Effective"] = "12/17/2011";
            dr["Product"] = "Kodak life insurance";
            dr["Status"] = "Terminated";
            dr["Comp Type"] = "Commission";
            dt.Rows.Add(dr);

            return dt;
        }
        DataTable DtPolicies
        {
            get
            {
                if (dtPolicies == null)
                    dtPolicies = getPolicies();
                return dtPolicies;
            }
        }
        void getPayors()
        {
            DataTable dtPolicyList = new DataTable();
            dtPolicyList.Columns.Add("Policy");
            dtPolicyList.Columns.Add("Client");
            dtPolicyList.Columns.Add("Adress");
            dtPolicyList.Columns.Add("City");
            dtPolicyList.Columns.Add("Date");
            dtPolicyList.Columns.Add("Status");
            dtPolicyList.Columns.Add("Comp Type");

            DataRow dr;

            dr = dtPolicyList.NewRow();
            dr[0] = "123";
            dr[1] = "Hira lal";
            dr[2] = "Gurgaon main";
            dr[3] = "Gurgaon";
            dr[4] = "12/2/2010";
            dr[5] = "1";
            dr[6] = "None";
            dtPolicyList.Rows.Add(dr);

            dr = dtPolicyList.NewRow();
            dr = dtPolicyList.NewRow();
            dr[0] = "124";
            dr[1] = "Mukesh Kumar";
            dr[2] = "Gurgaon main";
            dr[3] = "Gurgaon";
            dr[4] = "18/2/2010";
            dr[5] = "2";
            dr[6] = "None";
            dtPolicyList.Rows.Add(dr);
            

            dr = dtPolicyList.NewRow();
            dr[0] = "125";
            dr[1] = "Rajesh Singh";
            dr[2] = "Gurgaon Rajiv nagar";
            dr[3] = "Gurgaon";
            dr[4] = "21/2/2010";
            dr[5] = "3";
            dr[6] = "Commission";
            dtPolicyList.Rows.Add(dr);

            dr = dtPolicyList.NewRow();
            dr[0] = "126";
            dr[1] = "Pankaj Sharma";
            dr[2] = "Gandhi nagar Delhi";
            dr[3] = "Delhi";
            dr[4] = "26/2/2010";
            dr[5] = "1";
            dr[6] = "None";
            dtPolicyList.Rows.Add(dr);

            dgPolicies.ItemsSource = null;
            dgPolicies.ItemsSource = dtPolicyList.DefaultView ;
        }

        private void dgPolicies_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //ReplacePolicy rplPolicy = new ReplacePolicy();
                //rplPolicy.ShowDialog();  
            }
            catch(Exception)
            {

            }
        }

        private void dgPolicies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
