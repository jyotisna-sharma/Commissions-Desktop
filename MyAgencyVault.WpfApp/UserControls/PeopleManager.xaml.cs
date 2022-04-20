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
using System.Data;
using System.Collections ;
using System.Xml.Linq ;
using MyAgencyVault.WinApp.Validation; 


namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for PeopleManager.xaml
    /// </summary>
    public partial class PeopleManager : UserControl
    {
        static DataTable dtAgentsCollection = null;
        public PeopleManager()
        {
            InitializeComponent();
          
            //bindUsers();
        }
        private void bindUsers()
        {
            try
            {
                XDocument XmlDoc = XDocument.Load(System.IO.Path.GetFullPath(@"Common\UserDetail.xml"));
                var tolllst = from t in XmlDoc.Descendants("UserInfo")
                                select new
                                {
                                    UserName = t.Element("UserName").Value,
                                  
                                };

                //UserClient obj = new UserClient();
                //var lstUsers = (from u in obj.getAgents(1).ToList<UserDetail>()    
                //                select new { u.FirstName, u.LastName, u.NickName, u.Company });
                //grdAgents.ItemsSource = lstUsers.ToArray();
                //var fortree = from u in lstUsers
                //              select new { u.FirstName };


                ArrayList Arrlst1 = new ArrayList();

                foreach (var s in tolllst)
                {
                    Arrlst1.Add(s.UserName.ToString());
                    //tv1.Content = s.FirstName.ToString();
                    //listUsers.Items.Add(tv1);
                }
                ////listUsers.ItemsSource = Arrlst1;
            }
            catch(Exception)
            {
            }

        }     

        private void PeopleManager_Loaded(object sender, RoutedEventArgs e)
        {
            //BindProductsCombo123();
            //cmbSelectedUsers.SelectedIndex = 0;
            //listUsers.SelectedIndex = 0;
           // BindingValues();
            
            

        }
        //private void BindProductsCombo()
        //{
        //    UserClient objUserClient = new UserClient();
        //    List<LicenseInformation> objProductInfo = new List<LicenseInformation>();
        //    try
        //    {
        //        objProductInfo = objUserClient.GetProductInformation().ToList<LicenseInformation>();

        //        LicenseInformation prdInfo = new LicenseInformation();
        //        prdInfo.LicenseId = 0;
        //        prdInfo.LicenseName = "Please Select";
        //        objProductInfo.Insert(0, prdInfo);

        //        var objProdInfo = from c in objProductInfo
        //                          select new { c.LicenseId, c.LicenseName };

        //        cmbProductId.ItemsSource = objProdInfo.ToArray();
        //       cmbProductId.SelectedIndex = 0;
              
        //    }
        //    catch (Exception ex)
        //    {
        //        //Common.MyAgencyvaultException.ErrorHandle(ex, "PeopleManager", "BindProductsCombo");
        //    }
        //    finally
        //    {
        //        objUserClient = null;
        //    }
        //}
        //private void BindProductsCombo123()
        //{
           
        //    try
        //    {

        //        XDocument XmlDoc = XDocument.Load(System.IO.Path.GetFullPath(@"Common\UserDetail.xml"));
        //        var objProductInfo1 = from t in XmlDoc.Descendants("UserInfo").Distinct()   
        //                      select new
        //                      {
        //                          LicenseName = t.Element("LicenseName").Value ,
                                 
        //                      };
        //        cmbProductId.ItemsSource = objProductInfo1.Distinct().ToArray(); 
        //        cmbProductId.SelectedIndex = 0;
        //        var objProductInfoq = from t in XmlDoc.Descendants("UserInfo").Distinct()
        //                              select new
        //                              {
        //                                  Question = t.Element("Question").Value
        //                              };

        //        cmboQuestion.ItemsSource = objProductInfoq.Distinct().ToArray();
        //        cmboQuestion.SelectedIndex = 0;
        //        var objProductInfoGrd = from t in XmlDoc.Descendants("UserInfo").Distinct()
        //                              select new
        //                              {
        //                                  UserName = t.Element("UserName").Value,
        //                                  FirstName = t.Element("Fname").Value,
        //                                  LastName = t.Element("Lname").Value,
        //                                  Company = t.Element("Company").Value,
        //                                  NickName = t.Element("Nick Name").Value 
        //                              };
        //        DataTable dtUsers = new DataTable();
        //        dtUsers.Columns.Add("Last Name");
        //        dtUsers.Columns.Add("First Name");                
        //        dtUsers.Columns.Add("Nick Name");  
        //        dtUsers.Columns.Add("User Name");
        //        foreach (var s in objProductInfoGrd)
        //        {
        //            DataRow DtRow=dtUsers.NewRow(); 
                    
        //            DtRow[0] = s.FirstName;
        //            DtRow[1] = s.LastName ;
        //            DtRow[2] = s.NickName;
        //            DtRow[3]=s.UserName ;
        //            dtUsers.Rows.Add(DtRow);   
                        
        //            //tv1.Content = s.FirstName.ToString();
        //            //listUsers.Items.Add(tv1);
        //        }


        //        grdAgents.ItemsSource  = dtUsers.DefaultView ; 



        //        ////LicenseInformation prdInfo = new LicenseInformation();
        //        ////prdInfo.LicenseId = 0;
        //        ////prdInfo.LicenseName = "Please Select";
        //        ////ArrayList Arrlst1 = new ArrayList();
        //        ////Arrlst1.Add("Please Select");
        //        ////foreach (var s in objProductInfo1)
        //        ////{

        //        ////    if (Arrlst1.IndexOf(s.LicenseName.ToString()) <= 0)
        //        ////    {
        //        ////        Arrlst1.Add(s.LicenseName.ToString());
        //        ////    }
        //        ////    //tv1.Content = s.FirstName.ToString();
        //        ////    //listUsers.Items.Add(tv1);
        //        ////}
                


              
              

        //    }
        //    catch (Exception ex)
        //    {
        //        //Common.MyAgencyvaultException.ErrorHandle(ex, "PeopleManager", "BindProductsCombo");
        //    }
        //    finally
        //    {
               
        //    }
        //}
        private void bindTreeView()
        {
             
        }

        private void listUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ////UserClient obj = new UserClient();
            ////var lstUsers = (from u in obj.getAgents(1).ToList<UserDetail>()
            ////                where u.FirstName.Equals(listUsers.Items[listUsers.SelectedIndex].ToString() )
            ////                select new { u.FirstName, u.LastName, u.NickName, u.Company,u.Address1,u.City,u.Email,u.Phone,u.CellPhone,u.State, 
            ////                u.Fax });
            ////BindingValues();
           
        }
        //////private void BindingValues()
        //////{
        //////    XDocument XmlDoc = XDocument.Load(System.IO.Path.GetFullPath(@"Common\UserDetail.xml"));
        //////    var lstUsers = from t in XmlDoc.Descendants("UserInfo")
        //////                   select new
        //////                   {
        //////                       UserName = t.Element("UserName").Value,
        //////                       UserId = t.Element("UserId").Value,
        //////                       FirstName = t.Element("Fname").Value,
        //////                       LastName = t.Element("Lname").Value,
        //////                       Company = t.Element("Company").Value,
        //////                       NickName = t.Element("NickName").Value,
        //////                       Address1 = t.Element("Address1").Value,
        //////                       Zip = t.Element("Zip").Value,
        //////                       City = t.Element("City").Value,
        //////                       State = t.Element("State").Value,
        //////                       Email = t.Element("Email").Value,
        //////                       Phone = t.Element("Phone").Value,
        //////                       CellPhone = t.Element("CellPhone").Value,
        //////                       Fax = t.Element("Fax").Value,
        //////                       IsEnable = t.Element("IsEnable").Value,
        //////                       Password = t.Element("Password").Value,
        //////                       UserType = t.Element("UserType").Value,
        //////                       Question = t.Element("Question").Value,
        //////                       LicenseName = t.Element("LicenseName").Value,
        //////                       Answer = t.Element("Answer").Value,
        //////                       QuestionNo = t.Element("QuestionNo").Value,
        //////                       HouseAccount = t.Element("HouseAccount").Value,
        //////                       PolicyManager = t.Element("PolicyManager").Value,
        //////                       PeopleManager = t.Element("PeopleManager").Value,
        //////                       Settings = t.Element("Settings").Value,
        //////                       FollowUpManager = t.Element("FollowUpManager").Value,
        //////                       HelpUpdate = t.Element("HelpUpdate").Value,
        //////                       CompManager = t.Element("CompManager").Value,
        //////                       ReportManager = t.Element("ReportManager").Value,

        //////                   };

        //////    var fortree = from u in lstUsers
        //////                  where u.UserName.Equals(listUsers.Items[listUsers.SelectedIndex].ToString())
        //////                  select new
        //////                  {
        //////                      u.FirstName,
        //////                      u.LastName,
        //////                      u.NickName,
        //////                      u.Company,
        //////                      u.Address1,
        //////                      u.City,
        //////                      u.Email,
        //////                      u.Phone,
        //////                      u.CellPhone,
        //////                      u.State,
        //////                      u.Fax,
        //////                      u.UserName,
        //////                      u.Password,
        //////                      u.Zip,
        //////                      u.LicenseName,
        //////                      u.Question,
        //////                      u.Answer,
        //////                      u.QuestionNo,
        //////                      u.HouseAccount,
        //////                      u.PeopleManager,
        //////                      u.PolicyManager,
        //////                      u.Settings,
        //////                      u.FollowUpManager,
        //////                      u.HelpUpdate,
        //////                      u.CompManager,
        //////                      u.ReportManager
        //////                  };
        //////    foreach (var s in fortree)
        //////    {
        //////        //txtUserName.Text = s.n;
        //////        txtAddress.Text = s.Address1;
        //////        txtCellPhone.Text = s.CellPhone;
        //////        txtOfficePhone.Text = s.Phone;
        //////        txtState.Text = s.State;
        //////        txtFirstName.Text = s.FirstName;
        //////        txtLastName.Text = s.LastName;
        //////        txtFax.Text = s.Fax;
        //////        txtCompany.Text = s.Company;
        //////        txtNickName.Text = s.NickName;
        //////        txtCity.Text = s.City;
        //////        txtEmail.Text = s.Email;
        //////        txtUserName.Text = s.UserName;
        //////        txtPassword.Password = s.Password;
        //////        txtPwdHint.Password = s.Password;
        //////        txtZip.Text = s.Zip;

        //////        cmbProductId.SelectedValue.Equals(s.LicenseName);
        //////        cmboQuestion.SelectedIndex = Int32.Parse(s.QuestionNo);
        //////        //cmboQuestion.SelectedValuePath.IndexOf(s.LicenseName );  
        //////        txtAnswer.Text = s.Answer;
        //////        lblSettingAgentName.Content = s.UserName;
        //////        if (s.HouseAccount == "True")
        //////            chkHouseAccount.IsChecked = true;
        //////        else
        //////            chkHouseAccount.IsChecked = false;
        //////        bool Bol = (s.CompManager == "1" ? true : false);
        //////        rdNoAccessConfgrManager.IsChecked = (s.CompManager == "1" ? true : false);
        //////        rdReadConfgrManager.IsChecked = (s.CompManager == "1" ? true : false);
        //////        rdNoAccessFollowUpManager.IsChecked = (s.FollowUpManager == "1" ? true : false);
        //////        rdNoAccessHelpUpdate.IsChecked = (s.HelpUpdate == "1" ? true : false);
        //////        rdNoAccessPeopleManager.IsChecked = (s.PeopleManager == "1" ? true : false);
        //////        rdWritePolicyManager.IsChecked = (s.PolicyManager == "1" ? true : false);
        //////    }
            
        //////}

        private void cmbSelectedUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Element_LostFocus(object sender, RoutedEventArgs e)
        {
            Control control = sender as Control;
            if (control is TextBox || control is PasswordBox)
                ControlValidation.OnValidation(control);
        }

        private void txtFirstYear_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtFirstYear.Text == "0.00 %")
            {
                txtFirstYear.SelectAll();
            }
        }

        private void txtRenewal_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtRenewal.Text == "0.00 %")
            {
                txtRenewal.SelectAll();
            }
        }

        ////private void bindProductAgenct()
        ////{
        ////    UserClient objUserClient = new UserClient();
        ////    try
        ////    {
        ////        //if (userId != 0)
        ////        //{
        ////            ////ddlAgents.DisplayMember = "Name";
        ////            ////ddlAgents.ValueMember = "UserId";

        ////            var myAgentsCollection = from agentscol in objUserClient.getAgentsByUser(Common.UserDetail.objUserdetail.ProductId, userId)
        ////                                     select new
        ////                                     {
        ////                                         Select = "",
        ////                                         agentscol.UserId,
        ////                                         agentscol.FirstName,
        ////                                         agentscol.LastName,
        ////                                         agentscol.NickName,
        ////                                         Name = string.Format("{0}, {1}", agentscol.LastName, agentscol.FirstName)
        ////                                     };

        ////            dtAgentsCollection = myAgentsCollection.Copy2DataTable();

        ////            grdAgents.ItemsSource = dtAgentsCollection.DefaultView;
        ////            grdAgents.Model.HideCols[0] = true;
        ////            grdAgents.Model.HideCols[2] = true;
        ////            ddlAgents.DataSource = myAgentsCollection.ToArray();

        ////            grdAgents.Model.ColStyles[1].CellType = GridCellTypeName.CheckBox;
        ////            grdAgents.Model.ColStyles[1].HorizontalAlignment = GridHorizontalAlignment.Center;

        ////            grdAgents.Model.ColWidths[1] = 70;
        ////            grdAgents.Model.ColWidths[2] = 110;
        ////            grdAgents.Model.ColWidths[3] = 110;
        ////            grdAgents.Model.ColWidths[4] = 110;
        ////            grdAgents.Model.ColWidths[5] = 110;
        ////            grdAgents.Model.ColWidths[6] = 110;

        ////            if (tvUsers.SelectedNode != null)
        ////            {
        ////                if (tvUsers.SelectedNode.Name.ToLower() != "agent" &&
        ////                    tvUsers.SelectedNode.Name.ToLower() != "management" &&
        ////                    tvUsers.SelectedNode.Name.ToLower() != "Administration")
        ////                {
        ////                    List<UserAgentLinking> lstAgentLinking = objUserClient.getUserAgentLinking(Convert.ToInt64(tvUsers.SelectedNode.Name)).ToList<UserAgentLinking>();
        ////                    if (dtAgentsCollection != null)
        ////                    {
        ////                        for (int i = 0; i < dtAgentsCollection.Rows.Count; i++)
        ////                        {
        ////                            foreach (UserAgentLinking uALinking in lstAgentLinking)
        ////                            {
        ////                                if (dtAgentsCollection.Rows[i][1].Equals(uALinking.AgentId))
        ////                                {
        ////                                    dtAgentsCollection.Rows[i][0] = true;
        ////                                }
        ////                            }
        ////                        }
        ////                    }
        ////                }
        ////                else
        ////                {
        ////                    ClearFields();
        ////                }
        ////            }
        ////            grdAgents.CheckBoxClick += new GridCellClickEventHandler(grdAgents_CheckBoxClick);
        ////        }
        ////    //}
        ////    catch (Exception ex)
        ////    {
        ////        throw ex;
        ////    }
        ////    finally
        ////    {
        ////        objUserClient.Close();
        ////        objUserClient = null;
        ////    }
        ////}
    }
}
