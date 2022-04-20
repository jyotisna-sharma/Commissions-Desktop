using System.Windows.Input;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.VM;
using MyAgencyVault.VM.BaseVM;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System;
using System.Transactions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows;
using MyAgencyVault.VM.VMLib.DEU;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.VMLib
{
    public class CommissionDashBoardVM : BaseViewModel
    {
        public delegate void OpenCommissionDashBoard();
        public event OpenCommissionDashBoard OpenCommissionDashBoardEvent;

        public delegate void CloseCommissionDashBoard();
        public event CloseCommissionDashBoard CloseCommissionDashBoardEvent;
        private ServiceClients _ServiceClients;
        private ServiceClients serviceClients
        {
          get
          {
            if (_ServiceClients == null)
            {
              _ServiceClients = new ServiceClients();
            }
            return _ServiceClients;
          }
        }

        private VMSharedData _SharedVMData;
        public VMSharedData SharedVMData
        {
          get
          {
            if (_SharedVMData == null)
            {
              _SharedVMData = VMSharedData.getInstance();
            }

            return _SharedVMData;
          }
        }
        public PolicyDetailsData SelectedPolicy = null;

        #region CommissionDashBoard

        bool isEditEnable = true;
        public bool IsEditEnable
        {
            get { return isEditEnable; }
            set
            {
                isEditEnable = value;
            }
        }

        private PolicyPaymentEntriesPost policySelectedIncomingPaymentCommissionDashBoard;
        public PolicyPaymentEntriesPost PolicySelectedIncomingPaymentCommissionDashBoard
        {
            get
            {
                if (policySelectedIncomingPaymentCommissionDashBoard != null && (policySelectedIncomingPaymentCommissionDashBoard.SplitPer == null||policySelectedIncomingPaymentCommissionDashBoard.SplitPer==0))
                {
                    policySelectedIncomingPaymentCommissionDashBoard.SplitPer = 100;
                }
                return policySelectedIncomingPaymentCommissionDashBoard == null ? new PolicyPaymentEntriesPost() { SplitPer = 100 } : policySelectedIncomingPaymentCommissionDashBoard;

            }
            set
            {
                policySelectedIncomingPaymentCommissionDashBoard = value;
                OnPropertyChanged("PolicySelectedIncomingPaymentCommissionDashBoard");
            }
        }
        private Batch commissionDashSelectedBatch;
        public Batch CommissionDashSelectedBatch
        {
            get { return commissionDashSelectedBatch; }
            set { commissionDashSelectedBatch = value; OnPropertyChanged("CommissionDashSelectedBatch"); }
        }

        private Statement commissionDashSelectedStatement;
        public Statement CommissionDashSelectedStatement
        {
            get { return commissionDashSelectedStatement; }
            set { commissionDashSelectedStatement = value; OnPropertyChanged("CommissionDashSelectedStatement"); }
        }

        private Batch GenerateBatch()
        {
            try
            {
                Batch batch = serviceClients.PolicyClient.GenerateBatch(SelectedPolicy);
                return batch;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private Statement GenerateStatment()
        {
            try
            {
                Statement statement = serviceClients.PolicyClient.GenerateStatment
                    (CommissionDashSelectedBatch.BatchId, SelectedPolicy.PayorId ?? Guid.Empty,
                     PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived,
                     RoleManager.HouseOwnerId ?? Guid.Empty);
                return statement;
            }
            catch
            {
                return null;
            }
        }

        #region LostFocus
        private ICommand paymentRecivedLostFocus;
        public ICommand PaymentRecivedLostFocus
        {
            get
            {
                //if (paymentRecivedLostFocus == null)
                //{
                //    paymentRecivedLostFocus = new BaseCommand(x => CalCulateFormula());
                //}
                return paymentRecivedLostFocus;
            }
        }

        private ICommand commPerLostFocus;
        public ICommand CommPerLostFocus
        {
            get
            {
                //if (commPerLostFocus == null)
                //{
                //    commPerLostFocus = new BaseCommand(x => CalCulateFormula());
                //}
                return commPerLostFocus;
            }
        }

        private ICommand numOfunitsLostFocus;
        public ICommand NumOfunitsLostFocus
        {
            get
            {
                //if (numOfunitsLostFocus == null)
                //{
                //    numOfunitsLostFocus = new BaseCommand(x => CalCulateFormula());
                //}
                return numOfunitsLostFocus;
            }
        }

        private ICommand dollerPerUnitsLostFocus;
        public ICommand DollerPerUnitsLostFocus
        {
            get
            {
                //if (dollerPerUnitsLostFocus == null)
                //{
                //    dollerPerUnitsLostFocus = new BaseCommand(x => CalCulateFormula());
                //}
                return dollerPerUnitsLostFocus;
            }
        }

        private ICommand feeLostFocus;
        public ICommand FeeLostFocus
        {
            get
            {
                //if (feeLostFocus == null)
                //{
                //    feeLostFocus = new BaseCommand(x => CalCulateFormula());
                //}
                return feeLostFocus;
            }
        }

        private ICommand bonusLostFocus;
        public ICommand BonusLostFocus
        {
            get
            {
                //if (bonusLostFocus == null)
                //{
                //    bonusLostFocus = new BaseCommand(x => CalCulateFormula());
                //}
                return bonusLostFocus;
            }
        }

        private ICommand splitPerLostFocus;
        public ICommand SplitPerLostFocus
        {
            get
            {
                //if (splitPerLostFocus == null)
                //{
                //    splitPerLostFocus = new BaseCommand(x => CalCulateFormula());
                //}
                return splitPerLostFocus;
            }
        }

        private ICommand totalPaymentLostFocus;
        public ICommand TotalPaymentLostFocus
        {
            get
            {
                //if (totalPaymentLostFocus == null)
                //{
                //    totalPaymentLostFocus = new BaseCommand(x => CalCulateFormula());
                //}
                return totalPaymentLostFocus;
            }
        }
        #endregion

        private void CalCulateFormula()
        {
            try
            {
                /*if (PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived == null)
                {
                    PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived = 0;
                }
                if (PolicySelectedIncomingPaymentCommissionDashBoard.CommissionPercentage == null)
                {
                    PolicySelectedIncomingPaymentCommissionDashBoard.CommissionPercentage = 0;
                }
                if (PolicySelectedIncomingPaymentCommissionDashBoard.NumberOfUnits == null)
                {
                    PolicySelectedIncomingPaymentCommissionDashBoard.NumberOfUnits = 0;
                }
                if (PolicySelectedIncomingPaymentCommissionDashBoard.DollerPerUnit == null)
                {
                    PolicySelectedIncomingPaymentCommissionDashBoard.DollerPerUnit = 0;
                }
                if (PolicySelectedIncomingPaymentCommissionDashBoard.Fee == null)
                {
                    PolicySelectedIncomingPaymentCommissionDashBoard.Fee = 0;
                }
                if (PolicySelectedIncomingPaymentCommissionDashBoard.SplitPer == null)
                {
                    PolicySelectedIncomingPaymentCommissionDashBoard.SplitPer = 0;
                }
                decimal val = 0;
                val = (
                          (PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived*
                           (decimal) PolicySelectedIncomingPaymentCommissionDashBoard.CommissionPercentage/100)
                          +
                          (PolicySelectedIncomingPaymentCommissionDashBoard.NumberOfUnits*
                           PolicySelectedIncomingPaymentCommissionDashBoard.DollerPerUnit)
                          +
                          (PolicySelectedIncomingPaymentCommissionDashBoard.Fee +
                           PolicySelectedIncomingPaymentCommissionDashBoard.Bonus)
                      )*(decimal) PolicySelectedIncomingPaymentCommissionDashBoard.SplitPer/100;


                PolicySelectedIncomingPaymentCommissionDashBoard.TotalPayment = val;*/
            }
            catch (Exception) 
            {
            }
        }
        private ICommand clickpost;
        public ICommand ClickPost
        {
            get
            {
                if (clickpost == null)
                {
                    clickpost = new BaseCommand(x => BeforePostStart(),x => PostStart());
                }
                return clickpost;
            }
        }

        private bool BeforePostStart()
        {
            bool boolFlag = true;
            if (PolicySelectedIncomingPaymentCommissionDashBoard != null)
            {
                if (PolicySelectedIncomingPaymentCommissionDashBoard.InvoiceDate == null)
                {
                    boolFlag = false;
                }
                else
                    if (PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived == 0)
                    { 
                        //boolFlag = false;
                    }
                    else if (PolicySelectedIncomingPaymentCommissionDashBoard.CommissionPercentage < 0)
                    {
                        boolFlag = false;
                    }
                //else
                //if (PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived <= 0)
                //{ boolFlag = false; }
                //else if (PolicySelectedIncomingPaymentCommissionDashBoard.CommissionPercentage <= 0)
                //{
                //    boolFlag = false;
                //}

            }
            return boolFlag;

        }
        private ICommand _closeCommand;

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new BaseCommand(x => CloseDialog());
                }
                return _closeCommand;
            }

        }

        private void CloseDialog()
        {
            try
            {
                if (CloseCommissionDashBoardEvent != null)
                {
                    CloseCommissionDashBoardEvent();
                }
            }
            catch (Exception) 
            {}
        }
        string Msg = "Payment is not able to post properly.  At least one of the following is not " +
                        "allowing the entry:\n" +
                        "1.There is no outgoing schedule.\n" +
                        "2.There is no effective date.\n" +
                        "3.Invoice date is prior to effective date.\n" +
                        "4.The total outgoing payment(s) do not equal the incoming payment entered.\n";



        private void PostStart()
        {
            try
            {
                PostProcessReturnStatus _PostProcessReturnStatus;
                if (SelectedPolicy.PayorId == null || SelectedPolicy.PayorId == Guid.Empty || SelectedPolicy.CarrierID == null || SelectedPolicy.CarrierID == Guid.Empty)
                {
                    System.Windows.Forms.MessageBox.Show("Payor or Carrier is not assigned to seleted policy");
                    return;
                }
                if (SharedVMData.UpdateMode == UpdateMode.Add)
                {
                    try
                    {
                        #region NewPostCodeForCommissionDash

                        PolicySelectedIncomingPaymentCommissionDashBoard = UpdatePolicyPaymentEntry();
                        //_PostProcessReturnStatus=    erviceClients.PostUtilClient.CommissionDashBoardPostStart(CommissionDashSelectedBatch.BatchId, PolicySelectedIncomingPaymentCommissionDashBoard,
                        //        PostEntryProcess.FirstPost,RoleManager.Role);
                        _PostProcessReturnStatus = serviceClients.PostUtilClient.CommissionDashBoardPostStartClienVMWrapper(SelectedPolicy, PolicySelectedIncomingPaymentCommissionDashBoard,PostEntryProcess.FirstPost,RoleManager.Role);
                        SelectedPolicy.policyPaymentEntries = serviceClients.PostUtilClient.GetPolicyPaymentEntryForCommissionDashboard(SelectedPolicy.PolicyId);

                        if (!_PostProcessReturnStatus.IsComplete &&  _PostProcessReturnStatus.ErrorMessage == MessageConst.LockErrorMessage)
                        {
                            System.Windows.MessageBox.Show( _PostProcessReturnStatus.ErrorMessage + ", " + "Please try after some time", "Information !", MessageBoxButton.OK);
                            return;
                        }

                        if (!_PostProcessReturnStatus.IsComplete)
                        {
                            System.Windows.Forms.MessageBox.Show(Msg, "Information", MessageBoxButtons.OK,
                                                                 MessageBoxIcon.Information);
                            return;
                        }
                        #endregion
                    }

                    catch (Exception ex)
                    {
                    }
                }

                else if (SharedVMData.UpdateMode == UpdateMode.Edit)
                {
                    #region NewPostCodeForCommissionDash

                    _PostProcessReturnStatus = serviceClients.PostUtilClient.CommissionDashBoardPostStart(Guid.Empty, PolicySelectedIncomingPaymentCommissionDashBoard, PostEntryProcess.RePost, RoleManager.Role, !isEditEnable, RoleManager.userCredentialID); //If invoice date is updated, then IsEditEnable is always false
                    if (!_PostProcessReturnStatus.IsComplete)
                    {
                        System.Windows.Forms.MessageBox.Show(Msg, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    #endregion
                }
                if (CloseCommissionDashBoardEvent != null)
                {
                    CloseCommissionDashBoardEvent();
                }
            }
            catch (Exception ex)       
            {
                ActionLogger.Logger.WriteLog("exception posting from comm dashboard: " + ex.Message, true);
            }

        }

        private bool CheckToPaymentHODistributed(Guid _PaymentEntryID)
        {
            try
            {
                bool IsDistributeToHo = false;
                ObservableCollection<PolicyOutgoingDistribution> _PolicyOutgoingDistribution =
                    serviceClients.PostUtilClient.GetPolicyOutgoingPaymentForCommissionDashboard(_PaymentEntryID);
                if (_PolicyOutgoingDistribution.Count > 1)
                {
                    IsDistributeToHo = false;
                }
                else
                {
                    if (_PolicyOutgoingDistribution.Count != 0)
                    {


                        if (_PolicyOutgoingDistribution.FirstOrDefault().RecipientUserCredentialId ==
                            serviceClients.PostUtilClient.GetPolicyHouseOwner(SelectedPolicy.PolicyLicenseeId ??
                                                                              Guid.Empty))
                        {
                            IsDistributeToHo = true;

                        }
                        else
                        {
                            IsDistributeToHo = false;
                        }
                    }
                }
                return IsDistributeToHo;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private DEU GetDeuCollection(PolicyPaymentEntriesPost PolicySelectedIncomingPaymentCommissionDashBoard)
        {
            
            DEU _DEU = new DEU();
            try
            {
                _DEU.DEUENtryID = Guid.NewGuid();
                _DEU.OriginalEffectiveDate = SelectedPolicy.OriginalEffectiveDate;
                _DEU.PaymentRecived = PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived;
                _DEU.CommissionPercentage = PolicySelectedIncomingPaymentCommissionDashBoard.CommissionPercentage;
                _DEU.Insured = SelectedPolicy.Insured;
                _DEU.PolicyNumber = SelectedPolicy.PolicyNumber;
                _DEU.Enrolled = SelectedPolicy.Enrolled;
                _DEU.Eligible = SelectedPolicy.Eligible;
                _DEU.SplitPer = PolicySelectedIncomingPaymentCommissionDashBoard.SplitPer;
                _DEU.PolicyMode = SelectedPolicy.PolicyModeId;
                _DEU.TrackFromDate = SelectedPolicy.TrackFromDate.Value;
                _DEU.CompTypeID = SelectedPolicy.IncomingPaymentTypeId;
                _DEU.ClientID = SelectedPolicy.ClientId ?? Guid.Empty;
                _DEU.StmtID = PolicySelectedIncomingPaymentCommissionDashBoard.StmtID;
                _DEU.PolicyId = SelectedPolicy.PolicyId;
                _DEU.InvoiceDate = PolicySelectedIncomingPaymentCommissionDashBoard.InvoiceDate;
                _DEU.PayorId = SelectedPolicy.PayorId;
                _DEU.NoOfUnits = PolicySelectedIncomingPaymentCommissionDashBoard.NumberOfUnits;
                _DEU.DollerPerUnit = PolicySelectedIncomingPaymentCommissionDashBoard.DollerPerUnit;
                _DEU.Fee = PolicySelectedIncomingPaymentCommissionDashBoard.Fee;
                _DEU.Bonus = PolicySelectedIncomingPaymentCommissionDashBoard.Bonus;
                _DEU.CommissionTotal = PolicySelectedIncomingPaymentCommissionDashBoard.TotalPayment;
                _DEU.CarrierID = SelectedPolicy.CarrierID;
                _DEU.CoverageID = SelectedPolicy.CoverageId;
                _DEU.IsEntrybyCommissiondashBoard = (isEditEnable); // true;
                _DEU.CreatedBy = RoleManager.userCredentialID;
                _DEU.PostCompleteStatus = 0;
            }
            catch(Exception){}
            return _DEU;
        }

        public PolicyPaymentEntriesPost Show()
        {
            try
            {


                if (OpenCommissionDashBoardEvent != null)
                {
                    OpenCommissionDashBoardEvent();
                }
                
            }
            catch(Exception)
            {}
            return PolicySelectedIncomingPaymentCommissionDashBoard;
        }

        public PolicyPaymentEntriesPost Show(PolicyPaymentEntriesPost _PolicyPaymentEntriesPost)
        {
            try
            {
                if (_PolicyPaymentEntriesPost != null || _PolicyPaymentEntriesPost.PaymentEntryID != Guid.Empty)
                {
                    PolicySelectedIncomingPaymentCommissionDashBoard = _PolicyPaymentEntriesPost;
                }
                if (OpenCommissionDashBoardEvent != null)
                {
                    OpenCommissionDashBoardEvent();
                }
            }
            catch(Exception)
            {
                
            }
            return PolicySelectedIncomingPaymentCommissionDashBoard;
        }

        private PolicyPaymentEntriesPost UpdatePolicyPaymentEntry()
        {
            PolicyPaymentEntriesPost temp = new PolicyPaymentEntriesPost();
            try
            {
                if (PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID == Guid.Empty)
                {
                    temp.PaymentEntryID = Guid.NewGuid();
                }
                else
                {
                    temp.PaymentEntryID = PolicySelectedIncomingPaymentCommissionDashBoard.PaymentEntryID;
                }

                temp.PolicyID = SelectedPolicy.PolicyId; // PolicySelectedIncomingPaymentCommissionDashBoard.PolicyID;
                temp.InvoiceDate = PolicySelectedIncomingPaymentCommissionDashBoard.InvoiceDate;
                temp.PaymentRecived = PolicySelectedIncomingPaymentCommissionDashBoard.PaymentRecived;
                temp.CommissionPercentage = PolicySelectedIncomingPaymentCommissionDashBoard.CommissionPercentage;
                temp.NumberOfUnits = PolicySelectedIncomingPaymentCommissionDashBoard.NumberOfUnits;
                temp.DollerPerUnit = PolicySelectedIncomingPaymentCommissionDashBoard.DollerPerUnit;
                temp.Fee = PolicySelectedIncomingPaymentCommissionDashBoard.Fee;
                temp.SplitPer = PolicySelectedIncomingPaymentCommissionDashBoard.SplitPer;
                temp.TotalPayment = PolicySelectedIncomingPaymentCommissionDashBoard.TotalPayment;
                temp.CreatedOn = DateTime.Today;
                temp.CreatedBy = RoleManager.userCredentialID;
                temp.PostStatusID = PolicySelectedIncomingPaymentCommissionDashBoard.PostStatusID;
                temp.ClientId = SelectedPolicy.ClientId ?? Guid.Empty;
                    // PolicySelectedIncomingPaymentCommissionDashBoard.ClientId;
                temp.Bonus = PolicySelectedIncomingPaymentCommissionDashBoard.Bonus;
                temp.DEUEntryId = Guid.Empty;
            }
            catch(Exception)
            {
                
            }
            return temp;
        }
        #endregion
    }
}
