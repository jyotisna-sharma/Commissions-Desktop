using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.BaseVM;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.ViewModel.CommonItems;
using System.Windows.Input;
using System.ComponentModel;
using MyAgencyVault.EmailFax;
using System.Windows.Controls;
using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.Behaviour;
using System.Threading;
using System.Windows;
using Microsoft.Office;
using MyAgencyVault.VM.CommonItems;
using System.Linq.Expressions;
using System.IO;


namespace MyAgencyVault.VM.VMLib
{
  public enum FollowUpSearchFilterStatus
  {
    Open = 1,
    PaymentPending = 6,
    Closed = 2,
    All = 0,

  }
  public class VMFollowUpManager : BaseViewModel, IDataRefresh
  {

      #region variables
      // FollowupIssue _followUpIssue = null;
      private bool _RefreshRequired = false;
      public bool RefreshRequired
      {
          get { return _RefreshRequired; }
          set { _RefreshRequired = value; }
      }
      #endregion
      private ServiceClients _serviceclients;
      private ServiceClients serviceClients
      {
          get
          {
              if (_serviceclients == null)
              {
                  _serviceclients = new ServiceClients();
              }
              return _serviceclients;
          }
      }

      public delegate void ClosetermWindows();
      public event ClosetermWindows CloseEvent;

      #region ICommand
      private ICommand clickStatusChanged;

      public ICommand ClickStatusChanged
      {
          get
          {
              if (clickStatusChanged == null)
              {
                  clickStatusChanged = new BaseCommand(param => OnStatusChanged(param));
              }
              return clickStatusChanged;
          }

      }

      private ICommand policyComboChanged;

      public ICommand PolicyComboChanged
      {
          get
          {
              if (policyComboChanged == null)
              {
                  policyComboChanged = new BaseCommand(param => onComboStatusChanged(param));
              }

              return policyComboChanged;
          }

      }

      private ICommand policyNotesChanged;

      public ICommand PolicyNotesChanged
      {
          get
          {
              if (policyNotesChanged == null)
              {
                  policyNotesChanged = new BaseCommand(param => BeforeonPolicyNotesCahnged(), param => onPolicyNotesCahnged(param));
              }
              return policyNotesChanged;
          }

      }

      private bool BeforeonPolicyNotesCahnged()
      {
          if (RoleManager.UserAccessPermission(MasterModule.FollowUpManger) == ModuleAccessRight.Read)
              return false;
          else
              return true;
      }



      private ICommand sendMailCommand;
      public ICommand SendMailCommand
      {
          get
          {
              if (sendMailCommand == null)
              {
                  sendMailCommand = new BaseCommand(param => BeforeOnSendMailClick(param), param => OnSendMailClick(param));
              }
              return sendMailCommand;
          }

      }

      private bool BeforeOnSendMailClick(object param)
      {
          if (RoleManager.UserAccessPermission(MasterModule.FollowUpManger) == ModuleAccessRight.Read)
              return false;
          else
              return true;
      }


      #endregion

      #region CommandMethod
      private void onComboStatusChanged(object param)
      {
          SaveViewPoliciesIssueData();
      }
      private void onPolicyNotesCahnged(object param)
      {
          SavePolicyNotesdata();
      }

      public string Subject { get; set; }
      public string EmailID { get; set; }
      public string Body { get; set; }
      private WebDevPath ObjWebDevPath;
      private void OnSendMailClick(object param)
      {
          string clickedLink = param as string;
          MailData _MailData = new MailData();

          Subject = "<HTML>Subject";
          try
          {
              switch (clickedLink)
              {

                  case "FaxAgency":
                      _MailData.ToMail = "FaxAgency";
                      break;
                  case "FaxPayor":
                      _MailData.ToMail = "FaxPayor";

                      break;
                  case "EmailAgency":
                      Guid UserID = serviceClients.PostUtilClient.GetPolicyHouseOwner(SelectFollowUpIssue.LicenseeId);

                      if (UserID != null)
                          _MailData.ToMail = serviceClients.UserClient.getUserEmail(UserID);


                      if (_MailData.ToMail == null)
                      {

                      }
                      break;
                  default:
                      //casse to send E mail to payor
                      if (SelectedPayorContact != null)
                      {
                          EmailID = SelectedPayorContact.Email ?? string.Empty;
                          _MailData.ToMail = EmailID;
                      }
                      if (_MailData.ToMail == null)
                      {

                      }
                      break;
              }
              _MailData.AgencyName = LicenseeLst.Where(p => p.LicenseeId == SelectFollowUpIssue.LicenseeId).FirstOrDefault().Company;
              _MailData.CarrierName = SelectedFollowupIssueDetail.Carrier;
              _MailData.Category = SelectFollowUpIssue.Category.CategoryName;
              _MailData.ClientName = SelectedFollowupIssueDetail.Client;
              _MailData.Created = SelectedFollowupIssueDetail.Created;
              _MailData.InvoiceDate = SelectFollowUpIssue.InvoiceDate.ToString();
              _MailData.PolicyNumber = SelectFollowUpIssue.PolicyNumber;
              _MailData.Product = SelectedFollowupIssueDetail.Product;
              _MailData.ReceiverName = "";
              _MailData.TrackNumber = SelectedFollowupIssueDetail.TrackingNumber.ToString();
              //serviceClients.FollowupIssueClient.EmailToAgencyPayor(_MailData);
              Microsoft.Office.Interop.Outlook.Application oApp = new Microsoft.Office.Interop.Outlook.Application();
              Microsoft.Office.Interop.Outlook.MailItem message = (Microsoft.Office.Interop.Outlook.MailItem)oApp.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
              //Microsoft.Office.Interop.Outlook.Recipient oRecip;
              message.Subject = "CommissionDept Follow-Up Service: " + _MailData.ClientName + " Tracking # " + _MailData.TrackNumber;

              //get Mail body and set to HTML Body
              message.HTMLBody = MailBody(_MailData);
              message.To = _MailData.ToMail;
              //oRecip = (Microsoft.Office.Interop.Outlook.Recipient)message.Recipients.Add(_MailData.ToMail);
              //oRecip.Resolve();
              message.Display(true);
              //oRecip = null;
              message = null;
              oApp = null;


          }
          catch
          {
              MessageBoxResult _MessageBoxResult = MessageBox.Show("Requested Email is not registered with the System", "Email", MessageBoxButton.OK, MessageBoxImage.Warning);
          }
      }

      private string MailBody(MailData EmailContentdata)
      {

          string KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
          ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);
          string MailBody = string.Empty;
          //get logo path from deve server
          try
          {
              string strLogoPath = ObjWebDevPath.URL + "/Images/Logo.png";

              MailBody = "<table style='font-family: Tahoma; font-size: 12px; width: 100%; height: 100%' " +
                         "cellpadding='0'cellspacing='0' baorder='1' bordercolor='red'><tr><td colspan='2'>Dear " +
                     EmailContentdata.ReceiverName +
                         "</td></tr><tr><td colspan='2'>" +
                         "&nbsp;</td></tr><tr><td colspan='2'>Please advise on the " +
                     EmailContentdata.CarrierName +
                         " policy below if there is an error in payment." +
                     "<tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>We appreciate your help in this manner.</td></tr><tr><td colspan='2'>" +
                     "&nbsp;</td></tr><tr><td colspan='2'>Regards,</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>CommissionsDept Follow-Up Team on behalf of " +
                     EmailContentdata.AgencyName +
                     "</td></tr><tr><td colspan='2'>Email: " +
                     EmailContentdata.CommDeptMail +
                         "</td></tr><tr><td colspan='2'>Fax: " +
                     EmailContentdata.CommDeptFaxNumber +
                         "</td></tr><tr><td colspan='2'>Phone: " +
                     EmailContentdata.CommDeptPhoneNumber +
                     "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'><img src=" + strLogoPath + "  alt='CommissionsDept' /></td></tr><tr><td colspan='2'>&nbsp;</td></tr>"
                     + "</td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td colspan='2'>Client: <span style='padding-left: 50px'><b>" +
                     EmailContentdata.ClientName +
                         "</b></span></td></tr><tr><td colspan='2'>Policy: <span style='padding-left: 50px'><b>" +
                     EmailContentdata.PolicyNumber +
                         "</b></span></td></tr><tr>" +
                     "<td colspan='2'>Product: <span style='padding-left: 50px'><b>" +
                     EmailContentdata.Product +
                         "</b></span></td></tr><tr><td colspan='2'>Invoice: " +
                     "<span style='padding-left: 50px'><b>" +
                   EmailContentdata.InvoiceDate +
                         "</b></span></td></tr><tr><td colspan='2'>Issue: <span style='padding-left: 50px'><b>" +
                     EmailContentdata.Category + "<br/><tr><td colspan='2'>Follow-Up Tracking # " +
                     EmailContentdata.TrackNumber +
                         "</td></tr><tr><td colspan='2'>Created: " +
                     EmailContentdata.Created +
                         "</td></tr></table>";
          }
          catch
          {
          }

          return MailBody;

      }

      //bool isAllOnlyView = false;
      //bool isAllFollowUp = false;
      private void OnStatusChanged(object param)
      {
          string clickedLink = param as string;
          switch (clickedLink)
          {
              case "Open":
                  Status = FollowUpSearchFilterStatus.Open;
                  break;
              case "PaymentPending":
                  Status = FollowUpSearchFilterStatus.PaymentPending;
                  break;
              case "Closed":
                  Status = FollowUpSearchFilterStatus.Closed;
                  break;
              default:
                  Status = FollowUpSearchFilterStatus.All;
                  break;
          }
      }
      #endregion

      #region AddUpdate
      private void SaveViewPoliciesIssueData()
      {
          try
          {
              if (SelectFollowUpIssue == null) return;
              AddViewedIssueDataInFollowUp();
              if (SelectFollowUpIssue != null && SelectFollowUpIssue.IssueId != Guid.Empty)
                  serviceClients.FollowupIssueClient.UpdateIssuesSrcAsync(SelectFollowUpIssue, SelectFollowUpIssue.IssueStatusId.Value, RoleManager.userCredentialID);
          }
          catch
          {
          }
      }
      private void AddViewedIssueDataInFollowUp()
      {
          //SelectFollowUpIssue.PreviousStatusId = SelectFollowUpIssue.IssueStatusId;
          //SelectFollowUpIssue.ModifiedBy = RoleManager.userCredentialID;
      }
      private DisplayFollowupIssue InsrtPolicyissues(DisplayFollowupIssue SelectFollowUpIssue)
      {
          return new DisplayFollowupIssue
          {
              IssueId = SelectFollowUpIssue.IssueId,

              IssueStatusId = SelectFollowUpIssue.IssueStatusId,
              IssueCategoryID = SelectFollowUpIssue.IssueCategoryID,
              IssueResultId = SelectFollowUpIssue.IssueResultId,
              IssueReasonId = SelectFollowUpIssue.IssueReasonId,
              InvoiceDate = SelectFollowUpIssue.InvoiceDate,
              NextFollowupDate = SelectFollowUpIssue.NextFollowupDate,
              PolicyIssueNote = SelectFollowUpIssue.PolicyIssueNote,
              // Payment = SelectFollowUpIssue.Payment,
              Payor = SelectFollowUpIssue.Payor,
              Agency = SelectFollowUpIssue.Agency,
              Insured = SelectFollowUpIssue.Insured,
              PolicyNumber = SelectFollowUpIssue.PolicyNumber,
              PolicyId = SelectFollowUpIssue.PolicyId,
              // PreviousStatusId = SelectFollowUpIssue.PreviousStatusId,
          };
      }

      private void SavePolicyNotesdata()
      {
          try
          {
              if (SelectFollowUpIssue == null) return;
              AddPolicyNotes();
              serviceClients.FollowupIssueClient.AddUpdatePolicyIssueNotesScr(RoleManager.userCredentialID, SelectFollowUpIssue.IssueId, SelectFollowUpIssue.PolicyIssueNote);
          }
          catch
          { }
      }

      private void AddPolicyNotes()
      {

          SelectFollowUpIssue.PolicyIssueNote = FollowUpIssueNote;
          //SelectFollowUpIssue.ModifiedBy = RoleManager.userCredentialID;
          //_followUpIssue = InsrtPolicyissues(SelectFollowUpIssue);
      }
      #endregion

      public static List<StaticMasterCategory> staticmastercategory { get; set; }

      public VMFollowUpManager()
      {
          try
          {
              PropertyChanged += new PropertyChangedEventHandler(VMFollowUpManager_PropertyChanged);
              if (RoleManager.Role == UserRole.SuperAdmin)
              {
                  LicenseeLst = new ObservableCollection<LicenseeDisplayData>();
                  LicenseeLst.Add(new LicenseeDisplayData { Company = "--All--", LicenseeId = Guid.Empty, });

                  foreach (LicenseeDisplayData Li in FillLicenseeLst)
                  {
                      LicenseeLst.Add(Li);
                  }
              }
              else if (RoleManager.Role == UserRole.Agent)
              {
                  LicenseeLst = new ObservableCollection<LicenseeDisplayData>(FillLicenseeLst.Where(p => p.LicenseeId == RoleManager.LicenseeId).ToList());
              }

              Thread th = new Thread(() =>
                  {
                      SelectedLicensee = LicenseeLst == null ? new LicenseeDisplayData() : LicenseeLst.FirstOrDefault();
                  }
              );

              FUCategory = FillCategoryCombo;
              FUStatus = FillIssuesStatus;
              FUResult = FillIssueResult;
              FUReason = FillIssueReason;
              FollowUpScreenControl();

          }
          catch
          {
          }

      }

      Guid PrevSelectedLicID = new Guid();
      Guid PrevSelectedPayorID = new Guid();

      void VMFollowUpManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
          try
          {
              switch (e.PropertyName)
              {
                  case "SelectedPayor":

                      if (Convert.ToString(SelectedFollowOnlyView) == "Follow up")
                      {
                          try
                          {
                              if (SharedVMData.TestMasterFollowupIssueList.Count == 0)
                              {
                                  SharedVMData.TestMasterFollowupIssueList = FillFollowUpIssueLst();
                              }
                              ObservableCollection<DisplayFollowupIssue> TempCollection = new ObservableCollection<DisplayFollowupIssue>(SelectedLicensee.LicenseeId != Guid.Empty ? SharedVMData.TestMasterFollowupIssueList.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId) : SharedVMData.TestMasterFollowupIssueList);
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SelectedPayor.PayorID != Guid.Empty ? TempCollection.Where(p => p.PayorId == SelectedPayor.PayorID) : TempCollection);
                              int intStatus = Getstatus(Convert.ToString(SelectedIssueStatus));

                              if (intStatus == 0)//all             
                                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.ToList());
                              else
                              {
                                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IssueStatusId == intStatus).ToList());
                              }
                              Sorting();
                              ManageGridRecord();
                          }
                          catch
                          {
                          }
                      }
                      else
                      {
                          try
                          {
                              if (SharedVMData.ReadonlyMasterFollowUpList.Count == 0)
                              {
                                  SharedVMData.ReadonlyMasterFollowUpList = FillFollowUpIssueLst();
                              }
                              ObservableCollection<DisplayFollowupIssue> TempCollection = new ObservableCollection<DisplayFollowupIssue>(SelectedLicensee.LicenseeId != Guid.Empty ? SharedVMData.ReadonlyMasterFollowUpList.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId) : SharedVMData.ReadonlyMasterFollowUpList);
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SelectedPayor.PayorID != Guid.Empty ? TempCollection.Where(p => p.PayorId == SelectedPayor.PayorID) : TempCollection);
                              int intStatus = Getstatus(Convert.ToString(SelectedIssueStatus));

                              if (intStatus == 0)//all             
                                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.ToList());
                              else
                              {
                                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IssueStatusId == intStatus).ToList());
                              }
                              ManageGridRecord();
                          }
                          catch
                          {
                          }
                      }

                      break;
                  case "SelectedLicensee":
                      if (SelectedLicensee.LicenseeId != Guid.Empty)
                          SharedVMData.SelectedLicensee = SharedVMData.Licensees.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId).FirstOrDefault();
                      SelectedPayor = PayorLst.Where(p => p.PayorID == Guid.Empty).FirstOrDefault();
                      SelectedLicenseeChanged();

                      break;
                  case "SelectFollowUpIssue":


                      if (SelectFollowUpIssue.IssueId == null || SelectFollowUpIssue.IssueId == Guid.Empty)
                      {
                          Thread th2 = new Thread(() =>
                    {
                        FollowupIssuedetailLst = new ObservableCollection<IssuePolicyDetail>();
                        FollowIncomingPaymentLst = new ObservableCollection<FollowupIncomingPament>();
                        FollowUpIssueNote = "";
                        FollowUpPayorContact = new ObservableCollection<FollowUPPayorContacts>();
                        FollowUpIssueNote = "";
                        return;
                    }
                         );
                          th2.IsBackground = true;
                          th2.Start();
                      }
                      Thread th1 = new Thread(() =>
                     {
                         try
                         {
                             FollowupIssuedetailLst = FillIssueDetailInformation();
                             if (FollowupIssuedetailLst != null)
                                 SelectedFollowupIssueDetail = FollowupIssuedetailLst.FirstOrDefault();

                             payment = (SelectedFollowupIssueDetail.Payment == null ? 0 : SelectedFollowupIssueDetail.Payment.Value);

                             FollowIncomingPaymentLst = FillIncomingPayment();
                             if (FollowIncomingPaymentLst != null)
                                 SelectedFollowUpIncomingPayment = FollowIncomingPaymentLst.FirstOrDefault();

                             FollowUpPayorContact = FillFollowUpPayorContact();
                             if (FollowUpPayorContact != null)
                                 SelectedPayorContact = FollowUpPayorContact.FirstOrDefault();
                         }
                         catch
                         {
                         }

                     }
                    );
                      th1.IsBackground = true;
                      th1.Start();

                      #region"show policy issue notes"
                      try
                      {
                          //set  isssue notes if available                                                                                                                                                                                                                                                                                        
                          FollowUpIssueNote = SelectFollowUpIssue == null ? "" : SelectFollowUpIssue.PolicyIssueNote;
                          //if not found then serch into the all issue
                          if (FollowUpIssueNote == null)
                          {
                              FollowupIssueNoteList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.PolicyId == SelectFollowUpIssue.PolicyId && p.PolicyIssueNote != null));

                              foreach (var item in FollowupIssueNoteList)
                              {
                                  if (item.PolicyIssueNote != null)
                                  {
                                      FollowUpIssueNote = item.PolicyIssueNote;
                                  }
                              }
                          }

                          if (SelectFollowUpIssue.LicenseeId != PrevSelectedPayorID)
                          {
                              //load Agency payor notes 
                              if ((SelectFollowUpIssue.LicenseeId != null))
                              {
                                  if ((SelectFollowUpIssue.LicenseeId == Guid.Empty))
                                  {
                                      if (SelectFollowUpIssue.PayorId != null)
                                      {
                                          LoadPayorNotes((Guid)(SelectFollowUpIssue.LicenseeId), ((Guid)SelectFollowUpIssue.PayorId));
                                          PrevSelectedPayorID = (Guid)SelectFollowUpIssue.PayorId;
                                      }
                                  }

                              }
                          }

                          if (SelectFollowUpIssue.LicenseeId != PrevSelectedLicID)
                          {
                              if ((SelectFollowUpIssue.LicenseeId != null) )
                              {
                                  if ((SelectFollowUpIssue.LicenseeId != Guid.Empty))
                                  {
                                      //Call to load Agency notes
                                      LoadAgencyNotes((Guid)(SelectFollowUpIssue.LicenseeId));
                                      //Load Batch            
                                      LoadAgencyBathes((Guid)(SelectFollowUpIssue.LicenseeId));
                                      PrevSelectedLicID = (Guid)SelectFollowUpIssue.LicenseeId;
                                  }
                              }
                          }
                      }
                      catch
                      {
                      }

                      #endregion
                      break;

                  case "payment":
                      SavePaymentData();
                      break;
                  case "Status":
                      if (Status == FollowUpSearchFilterStatus.All)
                      {
                          if (isFollowUpChecked)
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.TestMasterFollowupIssueList);
                          }
                          else
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.ReadonlyMasterFollowUpList);
                          }
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackPayment == true).ToList());
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackMissingMonth == true).ToList());
                          int intStatus = Getstatus(Convert.ToString(SelectedIssueStatus));

                          if (intStatus == 0)//all             
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.ToList());
                          else
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IssueStatusId == intStatus).ToList());
                          }
                          ManageGridRecord();
                      }
                      else
                      {
                          if (isFollowUpChecked)
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.TestMasterFollowupIssueList.Where(p => p.IssueStatusId == (int)Status).ToList());
                          }
                          else
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.ReadonlyMasterFollowUpList.Where(p => p.IssueStatusId == (int)Status).ToList());
                          }
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackPayment == true).ToList());
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackMissingMonth == true).ToList());
                          int intStatus = Getstatus(Convert.ToString(SelectedIssueStatus));

                          if (intStatus == 0)//all             
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.ToList());
                          else
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IssueStatusId == intStatus).ToList());
                          }
                          ManageGridRecord();
                      }
                      if (SelectedLicensee.LicenseeId != Guid.Empty)
                      {
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(f => f.LicenseeId == SelectedLicensee.LicenseeId));
                      }
                      if (SelectedPayor.PayorID != Guid.Empty)
                      {
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(f => f.PayorId == SelectedPayor.PayorID));
                      }

                      Sorting();

                      break;
                  case "SelectedFUCategory":
                      SaveViewPoliciesIssueData();

                      break;
                  case "SelectedFUStatus":
                      SaveViewPoliciesIssueData();
                      break;
                  case "SelectedFUResult":
                      SaveViewPoliciesIssueData();
                      break;
                  case "SelectedFUReason":
                      SaveViewPoliciesIssueData();
                      break;

                  case "SelectedFollowUpIncomingPayment":
                      break;

                  case "SelectedObject":
                      Sorting();
                      break;
                  case "PopulatingRecords":
                      //Load batch date
                      //FillBatchDate();

                      break;

                  case "SelectedIssueStatus":

                      int intStatus1 = Getstatus(Convert.ToString(SelectedIssueStatus));

                      if (intStatus1 == 0)
                      {
                          if (Convert.ToString(SelectedFollowOnlyView) == "Follow up")
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.TestMasterFollowupIssueList);
                          }
                          else
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.ReadonlyMasterFollowUpList);
                          }
                          int intStatus = Getstatus(Convert.ToString(SelectedIssueStatus));

                          if (intStatus == 0)//all             
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.ToList());
                          else
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IssueStatusId == intStatus).ToList());
                          }
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackPayment == true).ToList());
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackMissingMonth == true).ToList());

                          ManageGridRecord();
                      }
                      else
                      {
                          if (Convert.ToString(SelectedFollowOnlyView) == "Follow up")
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.TestMasterFollowupIssueList.Where(p => p.IssueStatusId == intStatus1).ToList());
                              //FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.TestMasterFollowupIssueList.Where(p => p.IssueStatusId == (int)Status).ToList());
                          }
                          else
                          {
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SharedVMData.ReadonlyMasterFollowUpList.Where(p => p.IssueStatusId == intStatus1).ToList());
                          }
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackPayment == true).ToList());
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackMissingMonth == true).ToList());
                          ManageGridRecord();
                      }
                      if (SelectedLicensee.LicenseeId != Guid.Empty)
                      {
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(f => f.LicenseeId == SelectedLicensee.LicenseeId));
                      }
                      if (SelectedPayor.PayorID != Guid.Empty)
                      {
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(f => f.PayorId == SelectedPayor.PayorID));
                      }
                      break;

                  case "SelectedFollowOnlyView":
                      SelectedLicenseeChanged();
                      break;
              }
          }
          catch (Exception)
          {
          }
      }

      private void ManageGridRecord()
      {
          try
          {
              foreach (DisplayFollowupIssue fiss in FollowupIssueList)
              {
                  fiss.GenericBool = FollowupIssueToolGridEnable;
                  fiss.Status = FUStatus.Where(p => p.StatusID == fiss.IssueStatusId).FirstOrDefault();
                  fiss.Category = FUCategory.Where(p => p.CategoryID == fiss.IssueCategoryID).FirstOrDefault();
                  fiss.Results = FUResult.Where(p => p.ResultsID == fiss.IssueResultId).FirstOrDefault();
                  fiss.Reason = FUReason.Where(p => p.ReasonsID == fiss.IssueReasonId).FirstOrDefault();
                  fiss.PropertyChanged += new PropertyChangedEventHandler(SelectFollowUpIssue_PropertyChanged);
              }
          }
          catch (Exception)
          {
          }
      }

      #region"sorting function"

      private void Sorting()
      {
          try
          {
              if (FollowupIssueList == null) return;

              if (FollowupIssueList.Count > 0)
              {
                  switch (SelectedObject.ToString())
                  {
                      case "1-Order by payor,agency,client,policy,invoice date":
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.OrderBy(p => p.Payor).ThenBy(p => p.Agency).ThenBy(p => p.Insured).ThenBy(p => p.PolicyNumber).ThenBy(p => p.InvoiceDate));
                          break;
                      case "2-Order by agency,payor,client,policy,invoice date":
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.OrderBy(p => p.Agency).ThenBy(p => p.Payor).ThenBy(p => p.Insured).ThenBy(p => p.PolicyNumber).ThenBy(p => p.InvoiceDate));
                          break;
                      case "3-Order by client,payor,agency,policy,invoice date":
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.OrderBy(p => p.Insured).ThenBy(p => p.Agency).ThenBy(p => p.Payor).ThenBy(p => p.PolicyNumber).ThenBy(p => p.InvoiceDate));
                          break;
                      case "4-Order by policy,payor,agency,client,invoice date":
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.OrderBy(p => p.PolicyNumber).ThenBy(p => p.Payor).ThenBy(p => p.Agency).ThenBy(p => p.Insured).ThenBy(p => p.InvoiceDate));
                          break;
                      default:
                          FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.OrderBy(p => p.Payor).ThenBy(p => p.Agency).ThenBy(p => p.Insured).ThenBy(p => p.PolicyNumber).ThenBy(p => p.InvoiceDate));
                          break;
                  }
              }
          }
          catch
          {
          }

      }

      private void SetDefaultSorting()
      {
          try
          {
              if (SortingObject.Count > 0)
              {
                  SelectedObject = SortingObject.FirstOrDefault();
              }
          }
          catch
          {
          }
      }

      private string _SelectedObject;
      public string SelectedObject
      {
          get
          {
              return _SelectedObject;
          }
          set
          {
              _SelectedObject = value;
              OnPropertyChanged("SelectedObject");
          }
      }

      private ObservableCollection<string> _SortingObject;
      public ObservableCollection<string> SortingObject
      {
          get
          {
              return _SortingObject;
          }
          set
          {
              _SortingObject = value;
              OnPropertyChanged("SortingObject");
          }
      }

      private void LoadFilter()
      {
          try
          {
              SortingObject = new ObservableCollection<string>();             
              SortingObject.Clear();

              SortingObject.Add("1-Order by payor,agency,client,policy,invoice date");
              SortingObject.Add("2-Order by agency,payor,client,policy,invoice date");
              SortingObject.Add("3-Order by client,payor,agency,policy,invoice date");
              SortingObject.Add("4-Order by policy,payor,agency,client,invoice date");

              if (SortingObject.Count > 0)
              {
                  SelectedObject = SortingObject.FirstOrDefault();
              }
          }
          catch
          {
          }
      }

      #endregion

      void SelectFollowUpIssue_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
          DisplayFollowupIssue _FollowupIssue = sender as DisplayFollowupIssue;

          try
          {
              switch (e.PropertyName)
              {
                  case "Category":
                      SelectFollowUpIssue.IssueCategoryID = _FollowupIssue.Category.CategoryID;
                      SaveViewPoliciesIssueData();
                      break;
                  case "Reason":
                      SelectFollowUpIssue.IssueReasonId = _FollowupIssue.Reason.ReasonsID;
                      SaveViewPoliciesIssueData();

                      break;
                  case "Results":
                      SelectFollowUpIssue.IssueResultId = _FollowupIssue.Results.ResultsID;
                      SaveViewPoliciesIssueData();

                      break;
                  case "Status":
                      if (SelectFollowUpIssue.IssueStatusId == _FollowupIssue.Status.StatusID) return;
                      SelectFollowUpIssue.IssueStatusId = _FollowupIssue.Status.StatusID;
                      UpadteResult();
                      SaveViewPoliciesIssueData();
                      if (SelectFollowUpIssue.PolicyId != null)
                      {
                          bool isTrackPaymentChecked = serviceClients.PolicyClient.IsTrackPaymentChecked(SelectFollowUpIssue.PolicyId.Value);
                          serviceClients.PostUtilClient.FollowUpProcedure(FollowUpRunModules.ResolveIssue, null, SelectFollowUpIssue.PolicyId.Value, isTrackPaymentChecked, false, RoleManager.Role, null);
                      }
                      break;
                  case "NextFollowupDate":
                      SaveViewPoliciesIssueData();
                      break;
              }

          }
          catch (Exception)
          {
          }
      }

      private void UpadteResult()
      {
          try
          {
              if (SelectFollowUpIssue.IssueStatusId == 1)
              {
                  SelectFollowUpIssue.Results = FUResult.Where(p => p.ResultsID == 4).FirstOrDefault();
                  SelectFollowUpIssue.IssueResultId = 4;
              }
              else
              {
                  SelectFollowUpIssue.Results = FUResult.Where(p => p.ResultsID == 2).FirstOrDefault();
                  SelectFollowUpIssue.IssueResultId = 2;
              }

          }
          catch (Exception)
          {
          }
      }

      private void SavePaymentData()
      {
          try
          {
              if (payment != null)
              {
                  decimal dc = Convert.ToDecimal(payment);
                  serviceClients.FollowupIssueClient.AddPaymentData(SelectFollowUpIssue.IssueId, payment);
              }
          }
          catch
          {

          }

      }

      private ObservableCollection<string> _followOnlyView;
      public ObservableCollection<string> FollowOnlyView
      {
          get
          {
              return _followOnlyView;
          }
          set
          {
              _followOnlyView = value;
              OnPropertyChanged("FollowOnlyView");
          }
      }

      private string _selectedFollowOnlyView;
      public string SelectedFollowOnlyView
      {
          get
          {
              return _selectedFollowOnlyView == null ? null : _selectedFollowOnlyView;
          }
          set
          {
              _selectedFollowOnlyView = value;
              OnPropertyChanged("SelectedFollowOnlyView");
          }
      }

      private ObservableCollection<string> _IssueStatus;
      public ObservableCollection<string> IssueStatus
      {
          get
          {
              return _IssueStatus;
          }
          set
          {
              _IssueStatus = value;
              OnPropertyChanged("IssueStatus");
          }
      }

      private string _selectedIssueStatus;
      public string SelectedIssueStatus
      {
          get
          {
              return _selectedIssueStatus == null ? null : _selectedIssueStatus;
          }
          set
          {
              _selectedIssueStatus = value;
              OnPropertyChanged("SelectedIssueStatus");
          }
      }

      private int _intFilterValue;
      public int intFilterValue
      {
          get
          {
              return _intFilterValue;
          }
          set
          {
              _intFilterValue = value;
              OnPropertyChanged("intFilterValue");
          }
      }

      private ICommand _GetAllIssueByFilter;
      public ICommand GetAllIssueByFilter
      {
          get
          {
              if (_GetAllIssueByFilter == null)
              {
                  _GetAllIssueByFilter = new BaseCommand(param => GetAllData());
              }
              return _GetAllIssueByFilter;
          }

      }

      private void GetAllData()
      {
          Thread th = new Thread(() => {LoadAllData();});
          th.Start();
      }

      private void LoadAllData()
      {
          try
          {
              IsBusy = true;

              if (Convert.ToString(SelectedFollowOnlyView) == "Follow up")
              {
                  if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                  {
                      SharedVMData.TestMasterFollowupIssueList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, Guid.Empty, FollowUp, intFilterValue);
                  }
                  else
                  {
                      //if logged in by agency
                      Guid guidID = (Guid)RoleManager.LicenseeId;
                      SharedVMData.TestMasterFollowupIssueList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, guidID, FollowUp, intFilterValue);
                  }
                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SelectedLicensee.LicenseeId != Guid.Empty ? SharedVMData.TestMasterFollowupIssueList.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId) : SharedVMData.TestMasterFollowupIssueList);
              }
              else if (Convert.ToString(SelectedFollowOnlyView) == "Only view")
              {
                  if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                  {
                      SharedVMData.ReadonlyMasterFollowUpList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, Guid.Empty, FollowUp, intFilterValue);
                  }
                  else
                  {
                      //if logged in by agency
                      Guid guidID = (Guid)RoleManager.LicenseeId;
                      SharedVMData.ReadonlyMasterFollowUpList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, guidID, FollowUp, intFilterValue);
                  }
                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SelectedLicensee.LicenseeId != Guid.Empty ? SharedVMData.ReadonlyMasterFollowUpList.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId) : SharedVMData.ReadonlyMasterFollowUpList);
              }

              int intStatus = Getstatus(SelectedIssueStatus);

              if (intStatus == 0)
              {
                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.ToList());
              }
              else
              {
                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IssueStatusId == intStatus).ToList());
              }

              if (SelectedPayor.PayorID != Guid.Empty)
              {
                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.PayorId == SelectedPayor.PayorID).ToList());
              }             

              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackPayment == true).ToList());
              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackMissingMonth == true).ToList());
              Sorting();

              ManageGridRecord();
              IsBusy = false;
          }
          catch
          {
          }
      }

      private int Getstatus(string strStatus)
      {
          int intValue = 0;
          try
          {
              switch (strStatus)
              {
                  case "Open":
                      //intValue = 1;
                      intValue = (int)FollowUpSearchFilterStatus.Open;
                      break;

                  case "Pending":
                      //intValue = 6;
                      intValue = (int)FollowUpSearchFilterStatus.PaymentPending;
                      break;

                  case "Closed":
                      //intValue = 2;
                      intValue = (int)FollowUpSearchFilterStatus.Closed;
                      break;

                  case "All":
                      //intValue = 0;
                      intValue = (int)FollowUpSearchFilterStatus.All;
                      break;
              }
          }
          catch
          {
          }
          return intValue;
      }

      public VMFollowUpManager(StaticMasterCategory staticmastercategory, StaticMasterStatus staticmasterstatus, StaticMasterResult staticmasterresult, StaticMasterReason staticmasterreason)
      {
          try
          {
              //ActionLogger.Logger.WriteLog("1 " + DateTime.Now.ToLongTimeString(), false);
              MasterIssuesOption _MasterIssuesOption = FillMasterIssueOptions();
              FUCategory = _MasterIssuesOption.IssueCategories;
              FUStatus = _MasterIssuesOption.IssueStatus;
              FUResult = _MasterIssuesOption.IssueResults;
              FUReason = _MasterIssuesOption.IssueReasons;
              // ActionLogger.Logger.WriteLog("2 " + DateTime.Now.ToLongTimeString(), false);
              for (int i = 0; i < FUCategory.Count; i++)
              {
                  staticmastercategory.Add(FUCategory[i]);
              }

              for (int i = 0; i < FUStatus.Count; i++)
              {
                  staticmasterstatus.Add(FUStatus[i]);
              }
              for (int i = 0; i < FUResult.Count; i++)
              {
                  staticmasterresult.Add(FUResult[i]);
              }
              for (int i = 0; i < FUReason.Count; i++)
              {
                  staticmasterreason.Add(FUReason[i]);
              }

              //Status = FollowUpSearchFilterStatus.All;
              //FollowUp = true;
              //FillPayorLst();

              //Set Default to open by default
              Status = FollowUpSearchFilterStatus.Open;
              FollowUp = true;
              FillPayorLst();


              FollowOnlyView = new ObservableCollection<string>();
              FollowOnlyView.Add("Follow up");
              FollowOnlyView.Add("Only view");

              SelectedFollowOnlyView = FollowOnlyView.FirstOrDefault();

              IssueStatus = new ObservableCollection<string>();
              IssueStatus.Add("Open");
              IssueStatus.Add("Pending");
              IssueStatus.Add("Closed");
              IssueStatus.Add("All");

              //Load batch date
              FillBatchDate();

              SelectedIssueStatus = IssueStatus.FirstOrDefault();
              intFilterValue = 180;

              if (RoleManager.Role == UserRole.SuperAdmin)
              {
                  LicenseeLst = new ObservableCollection<LicenseeDisplayData>();
                  ObservableCollection<LicenseeDisplayData> _tempLicenseLst = FillLicenseeLst;
                  LicenseeLst = new ObservableCollection<LicenseeDisplayData>(_tempLicenseLst.OrderBy(p => p.Company));
                  LicenseeLst.Insert(0, new LicenseeDisplayData
                  {
                      Company = "--All--",
                      LicenseeId = Guid.Empty,
                  }
                  );
              }
              else if (RoleManager.Role == UserRole.Agent || RoleManager.Role == UserRole.Administrator || RoleManager.Role == UserRole.HO)
              {
                  LicenseeLst = new ObservableCollection<LicenseeDisplayData>(FillLicenseeLst.Where(p => p.LicenseeId == RoleManager.LicenseeId).ToList().OrderBy(p => p.Company));
              }

              SelectedPayor = PayorLst.Where(p => p.PayorID == Guid.Empty).FirstOrDefault();
              SelectedLicensee = LicenseeLst == null ? new LicenseeDisplayData() : LicenseeLst.FirstOrDefault();
              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>();

              FollowUpScreenControl();

              Thread th1 = new Thread(() =>
              {
                  FollowUpEnable = true;
                  OnlyViewEnable = false;
                  //Load data for filter
                  LoadFilter();
                  ChangeInFieldStatus("followup");
                  PropertyChanged += new PropertyChangedEventHandler(VMFollowUpManager_PropertyChanged);
              }
              );
              th1.Start();

              //FollowUpEnable = true;
              //OnlyViewEnable = false;
              ////Load data for filter
              //LoadFilter();
              //Thread th = new Thread(() =>
              //    {
              //        ChangeInFieldStatus("followup");
                      
              //    }
              //    );
              
              //th.Start();

              //PropertyChanged += new PropertyChangedEventHandler(VMFollowUpManager_PropertyChanged);

          }
          catch (Exception)
          {
          }
      }

      private MasterIssuesOption FillMasterIssueOptions()
      {
          return serviceClients.FollowupIssueClient.FillMasterIssueOptions();
      }

      #region ControlLevelProerty
      #region SettingScreen
      private void FollowUpScreenControl()
      {
          try
          {
              if (RoleManager.Role == UserRole.Agent)
              {
                  FollowupIssueToolGridEnable = false;
                  FollowupIssueToolGridReadOnly = true;

                  if (RoleManager.UserPermissions[(int)MasterModule.Settings - 1].Permission == ModuleAccessRight.Read)
                  {
                      FollowupIssueToolGridEnable = false;
                      FollowupIssueToolGridReadOnly = true;
                  }
                  //FollowupIssueToolGridEnable = true;
                  //FollowupIssueToolGridReadOnly = false;
              }
              else if (RoleManager.Role == UserRole.Administrator)
              {
                  FollowupIssueToolGridEnable = false;
                  FollowupIssueToolGridReadOnly = true;

              }
              else if (RoleManager.Role == UserRole.DEP)
              {
                  FollowupIssueToolGridEnable = false;
                  FollowupIssueToolGridReadOnly = true;
              }
              else if (RoleManager.Role == UserRole.HO)
              {
                  FollowupIssueToolGridEnable = false;
                  FollowupIssueToolGridReadOnly = true;
              }
              else if (RoleManager.Role == UserRole.SuperAdmin)
              {
                  FollowupIssueToolGridEnable = true;
                  FollowupIssueToolGridReadOnly = false;
              }
          }
          catch (Exception)
          {
          }
      }
      private bool followupissuetoolgridenable = false;
      public bool FollowupIssueToolGridEnable
      {
          get
          {
              return followupissuetoolgridenable;
          }
          set
          {
              followupissuetoolgridenable = value;
              OnPropertyChanged("FollowupIssueToolGridEnable");
          }
      }

      private bool followupissuetoolgridreadonly = false;
      public bool FollowupIssueToolGridReadOnly
      {
          get
          {
              return followupissuetoolgridreadonly;
          }
          set
          {
              followupissuetoolgridreadonly = value;
              OnPropertyChanged("FollowupIssueToolGridReadOnly");
          }
      }
      #endregion
      #endregion
      #region FollowUpwindowCombo

      private LicenseeDisplayData _selectedLicensee;
      public LicenseeDisplayData SelectedLicensee
      {
          get
          {
              return _selectedLicensee == null ? new LicenseeDisplayData() : _selectedLicensee;
          }
          set
          {
              _selectedLicensee = value;
              OnPropertyChanged("SelectedLicensee");
          }
      }

      private ObservableCollection<LicenseeDisplayData> _licenseeLst;
      public ObservableCollection<LicenseeDisplayData> LicenseeLst
      {
          get
          {
              return _licenseeLst;
          }
          set
          {
              _licenseeLst = value;
              OnPropertyChanged("LicenseeLst");
          }
      }
      private ObservableCollection<LicenseeDisplayData> _licenseeList;
      private ObservableCollection<LicenseeDisplayData> FillLicenseeLst
      {
          get
          {
              if (_licenseeList == null || _licenseeList.Count == 0)
              {
                  _licenseeList = serviceClients.LicenseeClient.GetLicenseeList(LicenseeStatusEnum.ActiveAndPending, RoleManager.LicenseeId.Value);
              }
              return _licenseeList;
          }
      }

      private DisplayedPayor _selectedPayor;
      public DisplayedPayor SelectedPayor
      {
          get
          {
              return _selectedPayor == null ? new DisplayedPayor() : _selectedPayor;
          }
          set
          {
              _selectedPayor = value;
              OnPropertyChanged("SelectedPayor");

          }
      }

      private ObservableCollection<DisplayedPayor> _payorLst;
      public ObservableCollection<DisplayedPayor> PayorLst
      {
          get
          {
              return _payorLst;
          }
          set
          {
              _payorLst = value;
              OnPropertyChanged("PayorLst");
          }
      }
      // private ObservableCollection<Payor> FillPayorLst()
      private void FillPayorLst()
      {
          PayorLst = new ObservableCollection<DisplayedPayor>();
          try
          {
              serviceClients.DisplayedPayorClient.GetDisplayPayorsCompleted += new EventHandler<GetDisplayPayorsCompletedEventArgs>(PayorClient_GetPayorsOnlyCompleted);

              ObservableCollection<DisplayedPayor> PayorLst1 = new ObservableCollection<DisplayedPayor>();

              ObservableCollection<DisplayedPayor> _tempPayorLst = new ObservableCollection<DisplayedPayor>();
              PayorFillInfo fillInfo = new PayorFillInfo { PayorStatus = PayorStatus.Active };
              if (SelectedLicensee.Company == "--All--")
              {
                  serviceClients.DisplayedPayorClient.GetDisplayPayorsAsync(Guid.Empty, fillInfo);
              }
              else
              {
                  serviceClients.DisplayedPayorClient.GetDisplayPayorsAsync(Guid.Empty, fillInfo);
              }


          }
          catch (Exception)
          {
          }
          //foreach (Payor pay in _tempPayorLst)
          //{
          //    PayorLst1.Add(pay);
          //}
          //if (PayorLst1 != null)
          //    PayorLst1 = new ObservableCollection<Payor>(PayorLst1.OrderBy(p => p.PayorName));

          //PayorLst1.Insert(0,
          //    new Payor
          //    {
          //        PayorName = "--All--",
          //        PayorID = Guid.Empty,
          //        ISGlobal = true,

          //    }
          //    );
          //return PayorLst1;


      }

      void PayorClient_GetPayorsOnlyCompleted(object sender, GetDisplayPayorsCompletedEventArgs e)
      {
          try
          {
              ObservableCollection<DisplayedPayor> _payorLst = e.Result;
              ObservableCollection<DisplayedPayor> PayorLst1 = new ObservableCollection<DisplayedPayor>(_payorLst.OrderBy(p => p.PayorName));
              PayorLst = new ObservableCollection<DisplayedPayor>(PayorLst1);
              PayorLst.Add(new DisplayedPayor { PayorName = "--All--", PayorID = Guid.Empty, });
              SelectedPayor = PayorLst.Where(p => p.PayorID == Guid.Empty).FirstOrDefault();
          }
          catch
          {
          }
      }
      #region FollowUpCombo
      //-- Result
      private IssueResults _selectedFUResult;

      public IssueResults SelectedFUResult
      {
          get
          {
              //SaveViewPoliciesIssueData();
              return _selectedFUResult;
          }
          set
          {
              _selectedFUResult = value;
              OnPropertyChanged("SelectedFUResult");
          }
      }

      private ObservableCollection<IssueResults> _FUResult;

      public ObservableCollection<IssueResults> FUResult
      {
          get
          {
              return _FUResult;
          }
          set
          {
              _FUResult = value;
              OnPropertyChanged("FUResult");
          }
      }
      private ObservableCollection<IssueResults> _masterIssueResults;
      private ObservableCollection<IssueResults> FillIssueResult
      {
          get
          {
              if (_masterIssueResults == null)
              {
                  _masterIssueResults = new ObservableCollection<IssueResults>(ConvertResultStatus(serviceClients.IssueResultClient.GetAllResults()));
              }
              return _masterIssueResults;
          }
      }
      private List<IssueResults> ConvertResultStatus(ObservableCollection<IssueResults> resultstatus)
      {
          return (from re in resultstatus
                  select new IssueResults
                  {
                      ResultsID = re.ResultsID,
                      ResultsName = re.ResultsName,

                  }
                  ).ToList();
      }
      //---///

      //-- Reason
      private IssueReasons _selectedFUReason;

      public IssueReasons SelectedFUReason
      {
          get
          {
              //SaveViewPoliciesIssueData();
              return _selectedFUReason;
          }
          set
          {
              _selectedFUReason = value;
              OnPropertyChanged("SelectedFUReason");
          }
      }

      private ObservableCollection<IssueReasons> _FUReason;
      public ObservableCollection<IssueReasons> FUReason
      {
          get
          {
              return _FUReason;
          }
          set
          {
              _FUReason = value;
              OnPropertyChanged("FUReason");
          }
      }
      private ObservableCollection<IssueReasons> _masterIssueReasons;
      private ObservableCollection<IssueReasons> FillIssueReason
      {
          get
          {
              try
              {
                  if (_masterIssueReasons == null)
                  {
                      _masterIssueReasons = new ObservableCollection<IssueReasons>(ConvertReasonStatus(serviceClients.IssueReasonClient.GetAllReason()));
                  }
              }
              catch
              {
              }
              return _masterIssueReasons;
          }
      }
      private List<IssueReasons> ConvertReasonStatus(ObservableCollection<IssueReasons> reasonStatus)
      {
          return (from re in reasonStatus
                  select new IssueReasons
                  {
                      ReasonsID = re.ReasonsID,
                      ReasonsName = re.ReasonsName,

                  }
                     ).ToList();
      }
      //----///
      //--Status
      private IssueStatus _SelectedFUStatus;

      public IssueStatus SelectedFUStatus
      {
          get
          {
              //SaveViewPoliciesIssueData();
              return _SelectedFUStatus;
          }
          set
          {
              _SelectedFUStatus = value;
              OnPropertyChanged("SelectedFUStatus");
          }
      }

      private ObservableCollection<IssueStatus> _FUStatus;
      public ObservableCollection<IssueStatus> FUStatus
      {
          get
          {
              return _FUStatus;
          }
          set
          {
              _FUStatus = value;
              OnPropertyChanged("FUStatus");
          }

      }
      private ObservableCollection<IssueStatus> _masterIssueStatus;
      private ObservableCollection<IssueStatus> FillIssuesStatus
      {
          get
          {
              try
              {
                  if (_masterIssueStatus == null)
                  {
                      _masterIssueStatus = new ObservableCollection<IssueStatus>(ConvertFollowUpStatus(serviceClients.IssueStatusClient.GetAllStatus()));
                  }
              }
              catch
              {
              }
              return _masterIssueStatus;
          }
      }

      private List<IssueStatus> ConvertFollowUpStatus(ObservableCollection<IssueStatus> status)
      {
          return (from st in status
                  select new IssueStatus
                  {
                      StatusID = st.StatusID,
                      StatusName = st.StatusName,

                  }
                     ).ToList();
      }
      //--Status//

      private IssueCategory _SelectedFUCategory;
      public IssueCategory SelectedFUCategory
      //    private StaticMasterCategory _SelectedFUCategory;
      //public StaticMasterCategory SelectedFUCategory
      {
          get
          {
              //  SaveViewPoliciesIssueData();
              return _SelectedFUCategory;
          }
          set
          {
              _SelectedFUCategory = value;
              OnPropertyChanged("SelectedFUCategory");
          }
      }

      private ObservableCollection<IssueCategory> _FUCategory;
      public ObservableCollection<IssueCategory> FUCategory
      {
          get
          {
              return _FUCategory;
          }
          set
          {
              _FUCategory = value;
              OnPropertyChanged("FUCategory");
          }

      }

      private ObservableCollection<IssueCategory> _masterIssueCategories;
      private ObservableCollection<IssueCategory> FillCategoryCombo
      {
          get
          {
              try
              {
                  if (_masterIssueCategories == null)
                  {
                      _masterIssueCategories = new ObservableCollection<IssueCategory>(ConvertFollowUpCategory(serviceClients.IssueCategoryClient.GetAllCategory()));
                  }
              }
              catch
              {
              }
              return _masterIssueCategories;
          }
      }
      private List<IssueCategory> ConvertFollowUpCategory(ObservableCollection<IssueCategory> CategoryMain)
      {
          return (from cm in CategoryMain
                  select new IssueCategory
                  {
                      CategoryID = cm.CategoryID,
                      CategoryName = cm.CategoryName,

                  }
                  ).ToList();
      }
      #endregion
      #region FollowupIssue

      private FollowupIncomingPament _selectedFollowUpIncomingPayment;

      public FollowupIncomingPament SelectedFollowUpIncomingPayment
      {
          get
          {
              return _selectedFollowUpIncomingPayment == null ? new FollowupIncomingPament() : _selectedFollowUpIncomingPayment;
          }
          set
          {
              _selectedFollowUpIncomingPayment = value;
              OnPropertyChanged("SelectedFollowUpIncomingPayment");

          }
      }


      private ObservableCollection<FollowupIncomingPament> _followIncomingPaymentLst;
      public ObservableCollection<FollowupIncomingPament> FollowIncomingPaymentLst
      {
          get
          {
              return _followIncomingPaymentLst;
          }
          set
          {
              _followIncomingPaymentLst = value;
              OnPropertyChanged("FollowIncomingPaymentLst");
          }
      }

      private ObservableCollection<FollowupIncomingPament> FillIncomingPayment()
      {
          #region "commented code"
          //Guid tempPolicyId = SelectFollowUpIssue.PolicyId ?? Guid.Empty;
          //if (tempPolicyId != Guid.Empty)
          //{
          //    return new ObservableCollection<FollowupIncomingPament>(serviceClients.FollowupIssueClient.GetIncomingPayment(tempPolicyId).OrderByDescending(p => p.InvoiceDate));
          //}
          //else
          //{
          //    return new ObservableCollection<FollowupIncomingPament>();
          //}
          #endregion

          ObservableCollection<FollowupIncomingPament> objFollowupIncomingPament = new ObservableCollection<FollowupIncomingPament>();
          try
          {
              Guid tempPolicyId = SelectFollowUpIssue.PolicyId ?? Guid.Empty;
              if (tempPolicyId != Guid.Empty)
              {
                  objFollowupIncomingPament = serviceClients.FollowupIssueClient.GetIncomingPayment(tempPolicyId);
                  if (objFollowupIncomingPament != null)
                  {
                      if (objFollowupIncomingPament.Count > 0)
                      {
                          objFollowupIncomingPament = new ObservableCollection<FollowupIncomingPament>(objFollowupIncomingPament.OrderByDescending(p => p.InvoiceDate).ToList());
                      }
                  }
              }
              else
              {
                  objFollowupIncomingPament = new ObservableCollection<FollowupIncomingPament>();
              }
          }
          catch
          {
          }

          return objFollowupIncomingPament;
      }

      private FollowUPPayorContacts _selectedPayorContact;

      public FollowUPPayorContacts SelectedPayorContact
      {
          get
          {
              return _selectedPayorContact == null ? new FollowUPPayorContacts() : _selectedPayorContact;
          }
          set
          {
              _selectedPayorContact = value;
              OnPropertyChanged("SelectedPayorContact");
          }
      }
      private ObservableCollection<FollowUPPayorContacts> _followUpPayorContact;

      public ObservableCollection<FollowUPPayorContacts> FollowUpPayorContact
      {
          get
          {
              //if (SelectFollowUpIssue.PolicyNumber == null)
              //    return null;
              return _followUpPayorContact;
          }
          set
          {
              _followUpPayorContact = value;
              OnPropertyChanged("FollowUpPayorContact");

          }
      }

      private ObservableCollection<FollowUPPayorContacts> FillFollowUpPayorContact()
      {
          Guid tempPolicyId = SelectFollowUpIssue.PolicyId ?? Guid.Empty;
          if (tempPolicyId != Guid.Empty)
          {
              return new ObservableCollection<FollowUPPayorContacts>(ConvertPayorContact(serviceClients.FollowupIssueClient.GetPayorContact(tempPolicyId)));
          }
          else
          {
              return new ObservableCollection<FollowUPPayorContacts>();
          }
      }

      private List<FollowUPPayorContacts> ConvertPayorContact(ObservableCollection<FollowUPPayorContacts> payorContact)
      {
          if (payorContact == null)
              payorContact = new ObservableCollection<FollowUPPayorContacts>();
          return (from pc in payorContact
                  select new FollowUPPayorContacts
                  {
                      FirstName = pc.FirstName,
                      LastName = pc.LastName,
                      ConatcPerf = pc.ConatcPerf,
                      Phone = pc.Phone,
                      Email = pc.Email,
                      Fax = pc.Fax,
                      Priority = pc.Priority,
                      City = pc.City,
                      State = pc.State,
                      zip = pc.zip,

                  }).ToList();
      }

      private string _followUpIssueNote;

      public string FollowUpIssueNote
      {
          get
          {
              //    if (_SelectFollowUpIssue == null)
              //    {
              //        _followUpIssueNote = "";
              //    }
              //    _followUpIssueNote = _SelectFollowUpIssue.PolicyIssueNote;
              return _followUpIssueNote;
          }
          set
          {
              _followUpIssueNote = value;
              OnPropertyChanged("FollowUpIssueNote");

          }
      }

      private DisplayFollowupIssue _SelectFollowUpIssue;
      public DisplayFollowupIssue SelectFollowUpIssue
      {
          get
          {
              return _SelectFollowUpIssue == null ? new DisplayFollowupIssue() : _SelectFollowUpIssue;

          }

          set
          {
              _SelectFollowUpIssue = value;
              OnPropertyChanged("SelectFollowUpIssue");
          }

      }
      private ObservableCollection<DisplayFollowupIssue> _followupIssueList;
      public ObservableCollection<DisplayFollowupIssue> FollowupIssueList
      {
          get
          {
              try
              {
                  return _followupIssueList;
              }
              catch
              {
                  return null;
              }
          }

          set
          {
              _followupIssueList = value;
              OnPropertyChanged("FollowupIssueList");
          }
      }

      private ObservableCollection<DisplayFollowupIssue> _followupIssueNoteList;
      public ObservableCollection<DisplayFollowupIssue> FollowupIssueNoteList
      {
          get
          {
              try
              {
                  return _followupIssueNoteList;
              }
              catch
              {
                  return null;
              }
          }

          set
          {
              _followupIssueNoteList = value;
              OnPropertyChanged("FollowupIssueNoteList");
          }
      }

      private ObservableCollection<DisplayFollowupIssue> FillFollowUpIssueLst()
      {
          ObservableCollection<DisplayFollowupIssue> _teFollowupIssuelst = new ObservableCollection<DisplayFollowupIssue>();
          try
          {
              IsBusy = true;
              //ObservableCollection<DisplayFollowupIssue> _teFollowupIssuelst = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr((int)Status, SelectedPayor.PayorID, SelectedLicensee.LicenseeId, FollowUp);
              //Load All status issue
              _teFollowupIssuelst = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, SelectedPayor.PayorID, SelectedLicensee.LicenseeId, FollowUp,intFilterValue);

              if (_teFollowupIssuelst != null && _teFollowupIssuelst.Count != 0)
              {
                  _teFollowupIssuelst.ToList().ForEach(p => p.GenericBool = FollowupIssueToolGridEnable);
              }
              IsBusy = false;
          }
          catch
          {
              IsBusy = false;
          }
          return _teFollowupIssuelst;
      }
      private void UpdateList(DisplayFollowupIssue f)
      {
          f.GenericBool = FollowupIssueToolGridEnable;
      }
      private ObservableCollection<IssuePolicyDetail> _followupIssuedetailLst;

      public ObservableCollection<IssuePolicyDetail> FollowupIssuedetailLst
      {
          get
          {
              return _followupIssuedetailLst;
          }
          set
          {
              _followupIssuedetailLst = value;
              OnPropertyChanged("FollowupIssuedetailLst");
          }
      }
      private ObservableCollection<IssuePolicyDetail> FillIssueDetailInformation()
      {
          Guid tempPolicyID = SelectFollowUpIssue.PolicyId ?? Guid.Empty;
          if (tempPolicyID != Guid.Empty)
          {
              return serviceClients.FollowupIssueClient.GetIssueDetail(tempPolicyID, SelectFollowUpIssue.IssueId);
          }
          else
          {
              return new ObservableCollection<IssuePolicyDetail>();
          }
      }

      private IssuePolicyDetail _selectedFollowupIssueDetail;

      public IssuePolicyDetail SelectedFollowupIssueDetail
      {
          get
          {
              return _selectedFollowupIssueDetail == null ? new IssuePolicyDetail() : _selectedFollowupIssueDetail;
          }
          set
          {
              _selectedFollowupIssueDetail = value;
              OnPropertyChanged("SelectedFollowupIssueDetail");
          }
      }

      #endregion

      /// <summary>
      /// It is updated when ClickStatusChanged command Fire
      /// </summary>
      private FollowUpSearchFilterStatus _Status;
      public FollowUpSearchFilterStatus Status
      {
          get
          {
              return _Status;
          }
          set
          {
              _Status = value;
              OnPropertyChanged("Status");
          }
      }

      private bool _FollowUp;
      public bool FollowUp
      {
          get
          {
              return _FollowUp;
          }
          set
          {
              _FollowUp = value;
              OnPropertyChanged("FollowUp");

          }
      }
      private bool _IsBusy;
      public bool IsBusy
      {
          get
          {
              return _IsBusy;
          }
          set
          {
              _IsBusy = value;
              PopulatingRecords = !value;
              OnPropertyChanged("IsBusy");
          }
      }

      #region "Load and Save Payor Notes"

      private string _SettingPayorNote;
      public string SettingPayorNote
      {
          get
          {
              return _SettingPayorNote;
          }
          set
          {
              _SettingPayorNote = value;
              OnPropertyChanged("SettingPayorNote");
          }
      }

      private string _PayorNotes;
      public string PayorNotes
      {
          get
          {
              return _PayorNotes;
          }
          set
          {
              _PayorNotes = value;
              OnPropertyChanged("PayorNotes");
          }
      }

      private ICommand _FollowUpAgencyPayorNotesUpdate;
      public ICommand FollowUpAgencyPayorNotesUpdate
      {
          get
          {
              if (_FollowUpAgencyPayorNotesUpdate == null)
                  _FollowUpAgencyPayorNotesUpdate = new BaseCommand(param => BeforeFollowUpAgencyPayorNotesUpdate(), param => OnFollowUpAgencyPayorNotesUpdate());
              return _FollowUpAgencyPayorNotesUpdate;
          }
      }

      private ICommand _FollowUpPayorNotesUpdate;
      public ICommand FollowUpPayorNotesUpdate
      {
          get
          {
              if (_FollowUpPayorNotesUpdate == null)
                  _FollowUpPayorNotesUpdate = new BaseCommand(param => BeforeFollowUpPayorNotesUpdate(), param => OnFollowUpPayorNotesUpdate());
              return _FollowUpPayorNotesUpdate;
          }
      }

      private bool BeforeFollowUpAgencyPayorNotesUpdate()
      {
          bool bValue = true;
          try
          {
              if (SelectFollowUpIssue == null)
                  return bValue = false;
              if (SelectFollowUpIssue.LicenseeId == null)
                  return bValue = false;
              if (SelectFollowUpIssue.PayorId == null)
                  return bValue = false;
          }
          catch
          {
          }


          return bValue;
      }

      private bool BeforeFollowUpPayorNotesUpdate()
      {
          bool bValue = true;
          try
          {
              if (SelectFollowUpIssue == null)
                  return bValue = false;
              //Crash (reported by Eric) Change -10 may 2012
              if (SharedVMData.SelectedLicensee == null)
                  return bValue = false;
              if (SharedVMData.SelectedLicensee.LicenseeId == null)
                  return bValue = false;
              if (SelectFollowUpIssue.PayorId == null)
                  return bValue = false;
          }
          catch
          {
          }

          return bValue;
      }

      #region"Term Date Click"

      private ICommand _cmdUpdateAutotermDate;
      public ICommand cmdUpdateAutotermDate
      {
          get
          {
              if (_cmdUpdateAutotermDate == null)
                  _cmdUpdateAutotermDate = new BaseCommand(param => BeforeCmdUpdateAutotermDate(), param => OnCmdUpdateAutotermDate());
              return _cmdUpdateAutotermDate;
          }
      }

      private bool BeforeCmdUpdateAutotermDate()
      {
          bool bValue = true;
          try
          {
          }
          catch
          {
          }

          return bValue;
      }

      public delegate void openTermDate();
      public event openTermDate openTermDateEvent;      
      private void OnCmdUpdateAutotermDate()
      {
          try
          {
              if (SelectedFollowupIssueDetail.Created != null)
              {
                  if (openTermDateEvent != null)
                  {
                      openTermDateEvent();
                      SelectedFollowupIssueDetail.PolicyTermDate = SharedVMData.TermanationDate.ToString();
                      if (SharedVMData.isClosedWindow)
                      {
                          CloseEvent();
                      }
                      
                  }
              }
              else
              {
                  MessageBox.Show("Please select follow up issue");
              }
          }
          catch
          {
          }
      }
      #endregion

      private void OnFollowUpAgencyPayorNotesUpdate()
      {
          try
          {
              if (SelectFollowUpIssue.LicenseeId != null && SelectFollowUpIssue.PayorId != null && SelectFollowUpIssue.LicenseeId != new Guid() && (Guid)SelectFollowUpIssue.PayorId != new Guid())
              {
                  using (ServiceClients serviceClients = new ServiceClients())
                  {
                      PayorSource _PayorSource = new PayorSource { LicenseeId = (Guid)SelectFollowUpIssue.LicenseeId, PayorId = (Guid)SelectFollowUpIssue.PayorId };
                      _PayorSource.Notes = SettingPayorNote;
                      serviceClients.PayorSourceClient.AddUpdatePayorSource(_PayorSource);
                  }
              }
              else
              {
                  MessageBox.Show("Please Select Agency and Payor", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
              }
          }
          catch
          {
          }
      }

      private void OnFollowUpPayorNotesUpdate()
      {
          try
          {
              if (SharedVMData.SelectedLicensee.LicenseeId != null && SelectFollowUpIssue.PayorId != null && SharedVMData.SelectedLicensee.LicenseeId != new Guid() && (Guid)SelectFollowUpIssue.PayorId != new Guid())
              {
                  using (ServiceClients serviceClients = new ServiceClients())
                  {

                      PayorSource _PayorSource = new PayorSource { LicenseeId = SharedVMData.SelectedLicensee.LicenseeId, PayorId = (Guid)SelectFollowUpIssue.PayorId };
                      _PayorSource.ConfigNotes = PayorNotes;
                      serviceClients.PayorSourceClient.AddPayorConfigSource(_PayorSource);
                  }
              }
              else
              {
                  MessageBox.Show("Please select Payor", "MyAgencyVault", MessageBoxButton.OK, MessageBoxImage.Information);
              }
          }
          catch
          {
          }
      }

      private void LoadPayorNotes(Guid LicId, Guid PayrID)
      {
          try
          {
              if (LicId != null && PayrID != null && LicId != new Guid() && PayrID != new Guid())
              {
                  using (ServiceClients serviceClients = new ServiceClients())
                  {

                      if (LicId == SharedVMData.SelectedLicensee.LicenseeId)
                      {
                          PayorSource _PayorSource = serviceClients.PayorSourceClient.GetPayorSource(new PayorSource { LicenseeId = SharedVMData.SelectedLicensee.LicenseeId, PayorId = PayrID });
                          SettingPayorNote = _PayorSource.Notes;
                          PayorNotes = _PayorSource.ConfigNotes;
                      }
                      else
                      {
                          //Call to get setting Payor Notes on the basis of linense     
                          PayorSource _PayorSource = serviceClients.PayorSourceClient.GetPayorSource(new PayorSource { LicenseeId = LicId, PayorId = PayrID });
                          SettingPayorNote = _PayorSource.Notes;

                          //Call to get Configuration Payor Notes                
                          PayorSource _PayorSourceForConfig = serviceClients.PayorSourceClient.GetPayorSource(new PayorSource { LicenseeId = SharedVMData.SelectedLicensee.LicenseeId, PayorId = PayrID });
                          PayorNotes = _PayorSourceForConfig.ConfigNotes;
                      }

                  }
              }
          }
          catch
          {
          }
      }
      #endregion

      private bool _PopulatingRecords;
      public bool PopulatingRecords
      {
          get
          {
              return _PopulatingRecords;
          }
          set
          {
              _PopulatingRecords = value;
              OnPropertyChanged("PopulatingRecords");
          }
      }

      private decimal _payment;
      public decimal payment
      {
          get
          {
              return _payment;
          }
          set
          {
              _payment = value;
              OnPropertyChanged("payment");
          }
      }
      private ICommand _PaymentLost;
      private ICommand PaymentLost
      {
          get
          {
              if (_PaymentLost == null)
              {
                  _PaymentLost = new BaseCommand(param => ChangeInPayment(param));
              }
              return _PaymentLost;
          }

      }

      private object ChangeInPayment(object param)
      {
          throw new NotImplementedException();
      }

      private ICommand _FollowUp_OnlyView;
      public ICommand FollowUp_OnlyView
      {
          get
          {
              if (_FollowUp_OnlyView == null)
              {
                  _FollowUp_OnlyView = new BaseCommand(param => ThreadChangeInFieldStatus(param));
              }
              return _FollowUp_OnlyView;
          }
      }

      private void ThreadChangeInFieldStatus(object param)
      {
          Thread th = new Thread(() =>
          {
              ChangeInFieldStatus(param);
          }
               );
          th.Start();

      }

      private bool isFollowUpChecked;

      public bool IsFollowUpChecked
      {
          get { return isFollowUpChecked; }
          set { isFollowUpChecked = value; OnPropertyChanged("IsFollowUpChecked"); }
      }

      private bool _followUpEnable;
      public bool FollowUpEnable
      {
          get { return _followUpEnable; }
          set { _followUpEnable = value; OnPropertyChanged("FollowUpEnable"); }
      }

      private bool _onlyViewEnable;
      public bool OnlyViewEnable
      {
          get { return _onlyViewEnable; }
          set { _onlyViewEnable = value; OnPropertyChanged("OnlyViewEnable"); }
      }

      private bool isOnlyViewChecked;

      public bool IsOnlyViewChecked
      {
          get { return isOnlyViewChecked; }
          set { isOnlyViewChecked = value; OnPropertyChanged("IsOnlyViewChecked"); }
      }

      //private void ChangeInFieldStatus(object param)
      //{
      //    try
      //    {
      //        //Call to unchecked radio button for filter
      //        //SetDefaultSorting();
      //        string str = param as string;
      //        if (str == "followup")
      //        {
      //            FollowUp = true;
      //            IsFollowUpChecked = true;
      //            IsOnlyViewChecked = false;
      //            if (SharedVMData.MasterFollowupIssueList.Count() == 0)
      //            {
      //                IsBusy = true;

      //                if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
      //                {
      //                    SharedVMData.MasterFollowupIssueList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, Guid.Empty, FollowUp);
      //                }
      //                else
      //                {
      //                    Guid guidID = (Guid)RoleManager.LicenseeId;

      //                    SharedVMData.MasterFollowupIssueList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, guidID, FollowUp);
      //                }

      //            }

      //            FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SelectedLicensee.LicenseeId != Guid.Empty ? SharedVMData.MasterFollowupIssueList.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId) : SharedVMData.MasterFollowupIssueList);

      //        }
      //        else
      //        {
      //            FollowUp = false;
      //            IsFollowUpChecked = false;
      //            IsOnlyViewChecked = true;
      //            if (SharedVMData.ReadonlyMasterFollowUpList.Count() == 0)
      //            {
      //                IsBusy = true;
      //                if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
      //                {
      //                    SharedVMData.ReadonlyMasterFollowUpList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, Guid.Empty, FollowUp);
      //                }
      //                else
      //                {
      //                    Guid guidID = (Guid)RoleManager.LicenseeId;

      //                    SharedVMData.ReadonlyMasterFollowUpList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, guidID, FollowUp);
      //                }

      //            }
      //            FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SelectedLicensee.LicenseeId != Guid.Empty ? SharedVMData.ReadonlyMasterFollowUpList.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId) : SharedVMData.ReadonlyMasterFollowUpList);

      //        }

      //        //Show only the open recards
      //        FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IssueStatusId == 1).ToList());

      //        //Show only the open recards
             

      //        //Default sorting
      //        //FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.OrderBy(p => p.Payor).ThenBy(p => p.Agency).ThenBy(p => p.Insured).ThenBy(p => p.PolicyNumber).ThenBy(p => p.InvoiceDate));

      //        ManageGridRecord();
      //        isAllFollowUp = true;
      //        //SelectFollowUpIssue = FollowupIssueList == null ? new DisplayFollowupIssue() : FollowupIssueList.FirstOrDefault();

      //        IsBusy = false;
      //    }
      //    catch (Exception ex)
      //    {
      //    }
      //}

      private void ChangeInFieldStatus(object param)
      {
          try
          {
              //Call to unchecked radio button for filter
              //SetDefaultSorting();
              string str = param as string;
              if (str == "followup")
              {
                  FollowUp = true;
                  if (SharedVMData.TestMasterFollowupIssueList.Count() == 0)
                  {
                      IsBusy = true;
                      if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                      {
                          SharedVMData.TestMasterFollowupIssueList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, Guid.Empty, FollowUp, intFilterValue);
                      }
                      else
                      {
                          Guid guidID = (Guid)RoleManager.LicenseeId;
                          SharedVMData.TestMasterFollowupIssueList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, guidID, FollowUp, intFilterValue);
                      }
                  }

                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SelectedLicensee.LicenseeId != Guid.Empty ? SharedVMData.TestMasterFollowupIssueList.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId) : SharedVMData.TestMasterFollowupIssueList);

              }
              else
              {
                  FollowUp = false;
                  if (SharedVMData.ReadonlyMasterFollowUpList.Count() == 0)
                  {
                      IsBusy = true;
                      if (RoleManager.LoggedInUser.ToString().ToUpper() == "SUPER")
                      {
                          SharedVMData.ReadonlyMasterFollowUpList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, Guid.Empty, FollowUp, intFilterValue);
                      }
                      else
                      {
                          Guid guidID = (Guid)RoleManager.LicenseeId;
                          SharedVMData.ReadonlyMasterFollowUpList = serviceClients.FollowupIssueClient.GetFewIssueAccordingtoModeScr(0, Guid.Empty, guidID, FollowUp, intFilterValue);
                      }
                  }
                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(SelectedLicensee.LicenseeId != Guid.Empty ? SharedVMData.ReadonlyMasterFollowUpList.Where(p => p.LicenseeId == SelectedLicensee.LicenseeId) : SharedVMData.ReadonlyMasterFollowUpList);

              }   
              
              int intStatus = Getstatus(Convert.ToString(SelectedIssueStatus));

              if (intStatus==0)//all             
                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.ToList());
              else
                  FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IssueStatusId == intStatus).ToList());
              

              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackPayment == true).ToList());
              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>(FollowupIssueList.Where(p => p.IsTrackMissingMonth == true).ToList());
              ////Default sorting
              Sorting();
              ManageGridRecord();              
              //isAllFollowUp = true;
              //SelectFollowUpIssue = FollowupIssueList == null ? new DisplayFollowupIssue() : FollowupIssueList.FirstOrDefault();
              IsBusy = false;
          }
          catch (Exception)
          {
          }
      }

      public VMSharedData _SharedVMData;
      public VMSharedData SharedVMData
      {
          get
          {
              if (_SharedVMData == null)
                  _SharedVMData = VMSharedData.getInstance();

              return _SharedVMData;
          }
      }

      private ObservableCollection<DateTime> _BatchDate;
      public ObservableCollection<DateTime> BatchDate
      {

          get
          {
              return _BatchDate;
          }

          set
          {
              _BatchDate = value;
              OnPropertyChanged("BatchDate");

          }

      }

      private DateTime _SelectedBatchDate;
      public DateTime SelectedBatchDate
      {
          get
          {

              return _SelectedBatchDate;

          }

          set
          {
              _SelectedBatchDate = value;
              OnPropertyChanged("SelectedBatchDate");

          }
      }

      private ObservableCollection<LicenseeNote> _lincenseNotes;
      public ObservableCollection<LicenseeNote> lincenseNotes
      {
          get
          {
              return _lincenseNotes;
          }
          set
          {
              _lincenseNotes = value;
              OnPropertyChanged("lincenseNotes");
          }
      }
      private bool isLodaedFillBatchDate = true;
      private void FillBatchDate()
      {
          try
          {
              if (isLodaedFillBatchDate)
              {
                  DateTime _TodayDate = DateTime.Today;
                  BatchDate = new ObservableCollection<DateTime>();
                  for (int idx = 0; idx < 5; idx++)
                  {
                      BatchDate.Add(_TodayDate.AddYears(-idx));
                  }
                  SelectedBatchDate = BatchDate.FirstOrDefault();
                  isLodaedFillBatchDate = false;
              }

          }
          catch (Exception)
          {
          }
      }

      private ObservableCollection<Batch> _BatchLst;
      public ObservableCollection<Batch> BatchLst
      {
          get
          {
              return _BatchLst;
          }
          set
          {
              _BatchLst = value;
              OnPropertyChanged("BatchLst");
          }
      }

      private Batch _SelectedBatch;
      public Batch SelectedBatch
      {
          get
          {
              return _SelectedBatch;
          }
          set
          {
              _SelectedBatch = value;
              OnPropertyChanged("SelectedBatch");
          }
      }

      private ICommand _RefeshAgencyNotes;
      public ICommand RefeshAgencyNotes
      {
          get
          {
              if (_RefeshAgencyNotes == null)
              {
                  _RefeshAgencyNotes = new BaseCommand(param => OnRefeshAgencyNotes());
              }
              return _RefeshAgencyNotes;
          }

      }

      private ICommand _RefeshBatch;
      public ICommand RefeshBatch
      {
          get
          {
              if (_RefeshBatch == null)
              {
                  _RefeshBatch = new BaseCommand(param => OnRefeshBatch());
              }
              return _RefeshBatch;
          }

      }

      private ICommand _ViewFiles;
      public ICommand ViewFiles
      {
          get
          {
              if (_ViewFiles == null)
              {
                  _ViewFiles = new BaseCommand(param => BeforeOnViewFiles(), param => OnViewFiles());
              }
              return _ViewFiles;
          }

      }

      private ICommand _ViewBatchFile;
      public ICommand ViewBatchFile
      {
          get
          {
              if (_ViewBatchFile == null)
              {
                  _ViewBatchFile = new BaseCommand(param => BeforeInComingTabViewFiles(), param => OnInComingTabViewFiles());
              }
              return _ViewBatchFile;
          }

      }

      private void OnRefeshAgencyNotes()
      {
          LoadAgencyNotes((Guid)(SelectFollowUpIssue.LicenseeId));
      }

      private void OnRefeshBatch()
      {
          LoadAgencyBathes((Guid)(SelectFollowUpIssue.LicenseeId));
      }

      private bool BeforeOnViewFiles()
      {
          bool bValue = true;
          if (SelectedBatch != null)
              bValue = true;
          else
              bValue = false;

          return bValue;
      }

      private bool BeforeInComingTabViewFiles()
      {
          bool bValue = true;
          if (SelectedFollowUpIncomingPayment != null)
          {
              if (SelectedFollowUpIncomingPayment.Batch != null)
              {
                  bValue = true;
              }
              else
                  bValue = false;
          }
          else
              bValue = false;

          return bValue;
      }
      private AutoResetEvent autoResetEvent;
      private void OnViewFiles()
      {
          string RemotePath = string.Empty;
          try
          {
              WebDevPath webDevPath = null;
              string key = "WebDevPath";
              string KeyValue = string.Empty;
              if (SharedVMData.MasterSystemConstants.ContainsKey(key))
              {
                  KeyValue = SharedVMData.MasterSystemConstants[key];
              }
              {
                  KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
                  //SharedVMData.MasterSystemConstants.Add(key, KeyValue);
              }
              webDevPath = WebDevPath.GetWebDevPath(KeyValue);

              Mouse.OverrideCursor = Cursors.Wait;
              autoResetEvent = new AutoResetEvent(false);

              FileUtility ObjDownload = FileUtility.CreateClient(webDevPath.URL, webDevPath.UserName, webDevPath.Password, webDevPath.DomainName);

              string localPath = Path.Combine(System.IO.Path.GetTempPath(), Path.GetFileName(SelectedBatch.FileName));

              string strFileExtension = System.IO.Path.GetExtension(SelectedBatch.FileName);

              if (strFileExtension.ToLower().Contains("pdf"))
              {
                  RemotePath = "/UploadBatch/" + SelectedBatch.FileName;
              }
              else
              {
                  RemotePath = "/UploadBatch/Import/Success/" + SelectedBatch.FileName;
              }
              //string RemotePath = "/UploadBatch/" + SelectedBatch.FileName;

              ObjDownload.DownloadComplete += new DownloadCompleteDel(ObjDownload_DownloadComplete);
              ObjDownload.ErrorOccured += new ErrorOccuredDel(ObjDownload_ErrorOccured);
              ObjDownload.Download(RemotePath, localPath);

              Mouse.OverrideCursor = Cursors.Arrow;
          }
          catch (Exception)
          {
          }
      }

      #region"Show view file on incoming tab in follw up manager"
      private void OnInComingTabViewFiles()
      {
          try
          {
              WebDevPath webDevPath = null;
              string key = "WebDevPath";
              string KeyValue = string.Empty;
              string strFileName = string.Empty;
              bool bvalue = true;

              //get uploaded files name
              if (SelectedFollowUpIncomingPayment != null)
              {
                  strFileName = serviceClients.BatchClient.BatchName(SelectedFollowUpIncomingPayment.Batch);
              }
              else
              {
                  MessageBox.Show("Please select incoming payment.", "MyAgencyVault", MessageBoxButton.OK);
                  return;
              }

              if (!string.IsNullOrEmpty(strFileName))
              {
                  //if (!strFileName.ToLower().Contains(".pdf"))
                  //{
                  //    MessageBox.Show("No file available for a manual commission dashboard entry.", "MyAgencyVault", MessageBoxButton.OK);
                  //    return;
                  //}
                  if (strFileName.ToLower().Contains(".pdf"))
                  {
                      //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                      //return;
                      bvalue = false;
                  }
                  else if (strFileName.ToLower().Contains(".xlsx"))
                  {
                      //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                      //return; 
                      bvalue = false;

                  }
                  else if (strFileName.ToLower().Contains(".xls"))
                  {
                      //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                      //return;
                      bvalue = false;
                  }
                  else if (strFileName.ToLower().Contains(".csv"))
                  {
                      //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                      //return;
                      bvalue = false;
                  }
                  else if (strFileName.ToLower().Contains(".txt"))
                  {
                      //MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                      //return;
                      bvalue = false;
                  }
                  if (bvalue)
                  {
                      MessageBox.Show("No file available for a manual commission dashboard entry.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                      return;
                  }
              }
              else
              {
                  MessageBox.Show("Please select payment.", "MyAgencyVault", MessageBoxButton.OK);
                  return;
              }

              if (SharedVMData.MasterSystemConstants.ContainsKey(key))
              {
                  KeyValue = SharedVMData.MasterSystemConstants[key];
              }
              {
                  KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
                  //SharedVMData.MasterSystemConstants.Add(key, KeyValue);
              }
              webDevPath = WebDevPath.GetWebDevPath(KeyValue);

              Mouse.OverrideCursor = Cursors.Wait;
              autoResetEvent = new AutoResetEvent(false);

              FileUtility ObjDownload = FileUtility.CreateClient(webDevPath.URL, webDevPath.UserName, webDevPath.Password, webDevPath.DomainName);

              string localPath = Path.Combine(System.IO.Path.GetTempPath(), Path.GetFileName(strFileName));
              string RemotePath = "/UploadBatch/" + strFileName;

              ObjDownload.DownloadComplete += new DownloadCompleteDel(ObjDownload_DownloadComplete);
              ObjDownload.ErrorOccured += new ErrorOccuredDel(ObjDownload_ErrorOccured);
              ObjDownload.Download(RemotePath, localPath);
              Mouse.OverrideCursor = Cursors.Arrow;
          }
          catch (Exception)
          {
          }
      }
      #endregion

      void ObjDownload_ErrorOccured(Exception error)
      {
          MessageBox.Show("There is some problem in viewing the file.Please try again", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }

      void ObjDownload_DownloadComplete(int statusCode, string localFilePath)
      {
          try
          {
              if (statusCode.ToString().StartsWith("20"))
              {
                  System.Diagnostics.Process.Start(localFilePath);
              }
              else
              {
                  MessageBox.Show("There is some problem in viewing the file.Please try again", "View Error", MessageBoxButton.OK, MessageBoxImage.Error);
              }
          }
          catch
          {
          }
      }


      #region "load agency Notes"
      private void LoadAgencyNotes(Guid LicenseeId)
      {
          try
          {
              //Get Agency notes which is entered from billing manager

              if ((LicenseeId == null) || (LicenseeId == new Guid()))
              {
                  if ((SelectedLicensee != null) || (SelectedLicensee.LicenseeId == new Guid()))
                  {
                      //get batch when followup issue is not selected
                      if (SelectedLicensee != null && SelectedLicensee.LicenseeId != new Guid())
                          lincenseNotes = new ObservableCollection<LicenseeNote>(serviceClients.LicenseeNoteClient.GetLicenseeNotes(SelectedLicensee.LicenseeId).OrderBy(s => s.CreatedDate));
                      else
                          lincenseNotes = null;
                  }
              }
              else
              {
                  lincenseNotes = new ObservableCollection<LicenseeNote>(serviceClients.LicenseeNoteClient.GetLicenseeNotes(LicenseeId).OrderBy(s => s.CreatedDate));
              }
          }
          catch
          {
          }
      }
      #endregion

      #region "load agency Notes"
      private void LoadAgencyBathes(Guid LicenseeId)
      {
          try
          {
              //Get Agency bathes which is same as followup manager
              //Batch load on the basis of current year
              if ((LicenseeId == null) || (LicenseeId == new Guid()))
              {
                  //get batch when followup issue is not selected
                  if (SelectedLicensee != null && SelectedLicensee.LicenseeId != new Guid())
                      BatchLst = new ObservableCollection<Batch>(serviceClients.BatchClient.GetCurrentBatch(SelectedLicensee.LicenseeId, SelectedBatchDate).OrderByDescending(s => s.BatchNumber));

                  else
                      BatchLst = null;
              }
              else
                  BatchLst = BatchLst = new ObservableCollection<Batch>(serviceClients.BatchClient.GetCurrentBatch(LicenseeId, SelectedBatchDate).OrderByDescending(s => s.BatchNumber));

              if (BatchLst != null && BatchLst.Count != 0)
              {
                  SelectedBatch = BatchLst.FirstOrDefault();

                  foreach (var item in BatchLst)
                  {
                      if (item != null)
                      {
                          decimal batchTotal = (serviceClients.BatchStatmentRecordsClient.GetBatchTotal(item.BatchId));

                          if (batchTotal > 0)
                              item.TotalStatementAmount = "$" + String.Format("{0:0,0.00}", batchTotal);
                          else
                              item.TotalStatementAmount = "$" + String.Format("{0:0.00}", batchTotal);
                      }
                  }
              }
          }
          catch
          {
          }
      }
      #endregion

      #region IDataRefresh Members

      public void Refresh()
      {
          if (SharedVMData.TestMasterFollowupIssueList.Count != 0 && SharedVMData.ReadonlyMasterFollowUpList.Count != 0)
          {
              Thread th1 = new Thread(() =>
              {
                  ChangeInFieldStatus("followup");
              }
              );
              th1.Start();

          }
      }

      //public void SelectedLicenseeChanged()
      //{
      //    try
      //    {
      //        FollowUpEnable = true;
      //        OnlyViewEnable = true;

      //        if (SelectedLicensee.Company == "--All--")
      //        {
      //            if (isFollowUpChecked)
      //            {
      //                Thread th1 = new Thread(() =>
      //                    {
      //                        ChangeInFieldStatus("followup");
      //                    }
      //                );
      //                th1.Start();


      //            }
      //            else
      //            {
      //                Thread th1 = new Thread(() =>
      //                {
      //                    ChangeInFieldStatus("onlyview");
      //                }
      //               );
      //                th1.Start();

      //            }

      //        }
      //        else
      //        {
      //            //FollowUpEnable = false;
      //            //OnlyViewEnable = false;

      //            if (isFollowUpChecked)
      //            {
      //                if (serviceClients.BillingLineDetailClient.IsFollowUpLicensee(SelectedLicensee.LicenseeId))
      //                {
      //                    Thread th1 = new Thread(() =>
      //                    {
      //                        ChangeInFieldStatus("followup");
      //                    }
      //               );
      //                    th1.Start();


      //                }
      //                else
      //                {
      //                    Thread th1 = new Thread(() =>
      //                    {
      //                        ChangeInFieldStatus("onlyview");
      //                    }
      //               );
      //                    th1.Start();
      //                }
      //            }
      //            else
      //            {
      //                Thread th1 = new Thread(() =>
      //                {
      //                    ChangeInFieldStatus("onlyview");
      //                }
      //               );
      //                th1.Start();
      //            }
      //        }
      //    }
      //    catch
      //    {
      //    }
      //}

      public void SelectedLicenseeChanged()
      {
          try
          {
              if (SelectedLicensee.Company == "--All--")
              {
                  if (Convert.ToString(SelectedFollowOnlyView) == "Follow up")
                  {
                      Thread th1 = new Thread(() =>
                      {
                          ChangeInFieldStatus("followup");
                      }
                      );
                      th1.Start();


                  }
                  else//Only view
                  {
                      Thread th1 = new Thread(() =>
                      {
                          ChangeInFieldStatus("onlyview");
                      }
                     );
                      th1.Start();

                  }

              }
              else
              {
                  //FollowUpEnable = false;
                  //OnlyViewEnable = false;

                  if (Convert.ToString(SelectedFollowOnlyView) == "Follow up")
                  {
                      if (serviceClients.BillingLineDetailClient.IsFollowUpLicensee(SelectedLicensee.LicenseeId))
                      {
                          Thread th1 = new Thread(() =>
                          {
                              ChangeInFieldStatus("followup");
                          }
                     );
                          th1.Start();


                      }
                      else
                      {
                          Thread th1 = new Thread(() =>
                          {
                              //SelectedFollowOnlyView = FollowOnlyView.LastOrDefault();
                              //ChangeInFieldStatus("onlyview");
                              FollowupIssueList = new ObservableCollection<DisplayFollowupIssue>();
                          }
                     );
                          th1.Start();
                      }
                  }
                  else
                  {
                      Thread th1 = new Thread(() =>
                      {
                          ChangeInFieldStatus("onlyview");
                      }
                     );
                      th1.Start();
                  }
              }
          }
          catch
          {
          }
      }
      #endregion
  }

  public class StaticMasterCategory : List<IssueCategory>, INotifyPropertyChanged
  {

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged(PropertyChangedEventArgs e)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, e);
      }
    }
  }

  public class StaticMasterStatus : List<IssueStatus>
  {
  }
  public class StaticMasterResult : List<IssueResults>
  {

  }
  public class StaticMasterReason : List<IssueReasons>
  {

  }
    #endregion
}
