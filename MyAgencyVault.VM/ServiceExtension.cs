using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ComponentModel;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using System.Runtime.Serialization;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using MyAgencyVault.ViewModel.Converters;

namespace MyAgencyVault.VM.MyAgencyVaultSvc
{


    public partial class News
    {
        private string _simpleNewsContent;
        public string SimpleNewsContent
        {
            get { return _simpleNewsContent; }
            set { _simpleNewsContent = value; RaisePropertyChanged("SimpleNewsContent"); }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            RTFToTextConverter converter = new RTFToTextConverter();
            SimpleNewsContent = converter.Convert(this.NewsContent);
        }
    }

    public partial class LicenseeNote
    {
        private string _simpleTextContent;
        public string SimpleTextContent
        {
            get { return _simpleTextContent; }
            set { _simpleTextContent = value; RaisePropertyChanged("SimpleTextContent"); }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            RTFToTextConverter converter = new RTFToTextConverter();
            SimpleTextContent = converter.Convert(this.Content);
        }
    }

    public partial class LicenseeDisplayData
    {
        public void Copy(LicenseeDisplayData s)
        {
            LicenseeStatus = s.LicenseeStatus;
            Company = s.Company;
            ContactFirst = s.ContactFirst;
            ContactLast = s.ContactLast;
            Address1 = s.Address1;
            Address2 = s.Address2;
            City = s.City;
            State = s.State;
            Phone = s.Phone;
            Fax = s.Fax;
            Email = s.Email;
            LicensePaymentModeId = s.LicensePaymentModeId;
            AccountCode = s.AccountCode;
            LicenseeSource = s.LicenseeSource;
            Commissionable = s.Commissionable;
            TrackDateDefault = s.TrackDateDefault;
            TaxRate = s.TaxRate;
            CutOffDay1 = s.CutOffDay1;
            CutOffDay2 = s.CutOffDay2;
            DueBalance = s.DueBalance;
        }
    }

    public partial class PolicyNotes : ICloneable
    {
        private string _simpleTextContent;
        public string SimpleTextContent
        {
            get { return _simpleTextContent; }
            set { _simpleTextContent = value; RaisePropertyChanged("SimpleTextContent"); }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            RTFToTextConverter converter = new RTFToTextConverter();
            SimpleTextContent = converter.Convert(this.Content);
        }

        #region ICloneable Members

        public object Clone()
        {
            PolicyNotes pn = new PolicyNotes();
            pn.Content = this.Content;
            pn.CreatedDate = this.CreatedDate;
            pn.LastModifiedDate = this.LastModifiedDate;
            pn.NoteID = this.NoteID;
            pn.PolicyID = this.PolicyID;
            return pn;
        }
        #endregion
    }
    public partial class PolicyOutgoingDistribution : ICloneable
    {
        private string payeeName;
        public string PayeeName
        {
            get
            {
                return payeeName;
            }
            set
            {
                payeeName = value;
                RaisePropertyChanged("PayeeName");
            }
        }

        private string nickName;
        public string NickName
        {
            get
            {
                return nickName;
            }
            set
            {
                nickName = value;
                RaisePropertyChanged("NickName");
            }
        }

        private string paidStatus;

        public string PaidStatus
        {
            get { return paidStatus; }
            set
            {
                paidStatus = value;
                RaisePropertyChanged("PaidStatus");
            }
        }
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            using (ServiceClients serviceClients = new ServiceClients())
            {
                User _User = serviceClients.UserClient.GetUserIdWise(RecipientUserCredentialId.Value);
                PayeeName = _User.FirstName;
                NickName = _User.NickName;
                if (IsPaid.Value)
                {
                    PaidStatus = "Paid";
                }
                else
                {
                    PaidStatus = "Un Paid";

                }
            }
        }
        #region ICloneable Members

        public object Clone()
        {
            PolicyOutgoingDistribution pod = new PolicyOutgoingDistribution();
            pod.OutgoingPaymentId = this.OutgoingPaymentId;
            pod.PaymentEntryId = this.PaymentEntryId;
            pod.RecipientUserCredentialId = this.RecipientUserCredentialId;
            pod.PaidAmount = this.PaidAmount;
            pod.CreatedOn = this.CreatedOn;
            // pod.ReferencedOutgoingScheduleId = this.ReferencedOutgoingScheduleId;
            //pod.ReferencedOutgoingAdvancedScheduleId = this.ReferencedOutgoingAdvancedScheduleId;
            pod.IsPaid = this.IsPaid;
            //18-Apr-2011
            pod.Premium = this.Premium;
            pod.OutGoingPerUnit = this.OutGoingPerUnit;
            pod.Payment = this.Payment;
            PayeeName = this.PayeeName;
            NickName = this.NickName;
            PaidStatus = this.PaidStatus;
            return pod;
        }

        #endregion
    }

    public partial class DisplayFollowupIssue : ICloneable
    {
        private bool _GenericBool;
        public bool GenericBool
        {
            get
            {
                return _GenericBool;
            }
            set
            {
                _GenericBool = value;
                RaisePropertyChanged("GenericBool");
            }
        }
        private IssueCategory _category;
        public IssueCategory Category
        {
            get { return _category; }
            set
            {
                _category = value;

                RaisePropertyChanged("Category");
            }
        }
        private IssueReasons _reason;
        public IssueReasons Reason
        {
            get { return _reason; }
            set { _reason = value; RaisePropertyChanged("Reason"); }
        }

        private IssueResults _results;
        public IssueResults Results
        {
            get { return _results; }
            set { _results = value; RaisePropertyChanged("Results"); }
        }

        private IssueStatus _status;
        public IssueStatus Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged("Status"); }
        }

        #region ICloneable Members

        public object Clone()
        {
            DisplayFollowupIssue followupissue = new DisplayFollowupIssue();
            IssueId = this.IssueId;

            IssueStatusId = this.IssueStatusId;

            IssueCategoryID = this.IssueCategoryID;

            IssueResultId = this.IssueResultId;

            IssueReasonId = this.IssueReasonId;


            InvoiceDate = this.InvoiceDate;

            NextFollowupDate = this.NextFollowupDate;

            PolicyIssueNote = this.PolicyIssueNote;

            Payment = this.Payment;

            PolicyId = this.PolicyId;

            PreviousStatusId = this.PreviousStatusId;

            Payor = this.Payor;

            Agency = this.Agency;

            Insured = this.Insured;

            PolicyNumber = this.PolicyNumber;

            GenericBool = this.GenericBool;
            //Category = this.Category;
            //Reason = this.Reason;
            //     Results =this.Results;
            //     Status = this.Status;
            return followupissue;
        }

        #endregion
    }

    public partial class GlobalPayorContact
    {
        private bool _IsNew;
        public bool IsNew
        {
            get { return _IsNew; }
            set { _IsNew = value; RaisePropertyChanged("IsNew"); }
        }
    }

    public partial class IncomingScheduleEntry : ICloneable
    {
        public object Clone()
        {
            IncomingScheduleEntry newScheduleEntry = new IncomingScheduleEntry();
            newScheduleEntry.CoveragesScheduleId = this.CoveragesScheduleId;
            newScheduleEntry.EffectiveFromDate = this.EffectiveFromDate;
            newScheduleEntry.EffectiveToDate = this.EffectiveToDate;
            newScheduleEntry.FromRange = this.FromRange;
            newScheduleEntry.IsDeleted = this.IsDeleted;
            newScheduleEntry.Rate = this.Rate;
            newScheduleEntry.ToRange = this.ToRange;
            return newScheduleEntry;
        }
    }
    public partial class Graded : ICloneable
    {
        public object Clone()
        {
            Graded GradedScheduleEntry = new Graded();
            GradedScheduleEntry.From = this.From;
            GradedScheduleEntry.To = this.To;
            GradedScheduleEntry.Percent = this.Percent;
            GradedScheduleEntry.PercentField = this.PercentField;
            GradedScheduleEntry.FromField = this.FromField;
            GradedScheduleEntry.ToField = this.ToField;
            return GradedScheduleEntry;
        }
    }
    public partial class NonGraded : ICloneable
    {
        public object Clone()
        {
            NonGraded NonGradedScheduleEntry = new NonGraded();
            NonGradedScheduleEntry.Year = this.Year;
            NonGradedScheduleEntry.YearField = this.YearField;
            NonGradedScheduleEntry.Percent = this.Percent;
            NonGradedScheduleEntry.PercentField = this.PercentField;
          
            return NonGradedScheduleEntry;
        }
    }

    public partial class PolicyDetailsData : ICloneable
    {
        private DisplayedPayor _payor;
        public DisplayedPayor Payor
        {
            get
            {
                return _payor;
            }
            set
            {
                _payor = value;
                RaisePropertyChanged("Payor");
            }
        }


        private Carrier _carrier;
        public Carrier Carrier
        {
            get
            {
                return _carrier;
            }
            set
            {
                _carrier = value;
                RaisePropertyChanged("Carrier");
            }
        }
        private DisplayedCoverage _coverage;
        public DisplayedCoverage Coverage
        {
            get
            {
                return _coverage;

            }
            set
            {
                _coverage = value;
                RaisePropertyChanged("Coverage");
            }
        }

        private bool _isSelectedPolicyChangeAttach = false;
        public bool IsSelectedPolicyChangeAttach
        {
            get
            {
                return _isSelectedPolicyChangeAttach;
            }
            set
            {
                _isSelectedPolicyChangeAttach = value;
                RaisePropertyChanged("IsSelectedPolicyChangeAttach");
            }
        }


        private string _compTypeName;
        public string CompTypeName
        {
            get { return _compTypeName; }
            set
            {
                _compTypeName = value;
                RaisePropertyChanged("CompTypeName");
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            PolicyDetailsData policy = new PolicyDetailsData();
            policy.PolicyId = this.PolicyId;
            policy.PolicyNumber = this.PolicyNumber;
            policy.PolicyStatusId = this.PolicyStatusId;
            policy.PolicyStatusName = this.PolicyStatusName;
            policy.PolicyType = this.PolicyType;
            policy.PolicyLicenseeId = this.PolicyLicenseeId;
            policy.Insured = this.Insured;
            policy.OriginalEffectiveDate = this.OriginalEffectiveDate;
            policy.TrackFromDate = this.TrackFromDate;
            policy.PolicyModeId = this.PolicyModeId;
            policy.ModeAvgPremium = this.ModeAvgPremium;
            policy.SubmittedThrough = this.SubmittedThrough;
            policy.Enrolled = this.Enrolled;
            policy.Eligible = this.Eligible;
            policy.PolicyTerminationDate = this.PolicyTerminationDate;
            policy.TerminationReasonId = this.TerminationReasonId;
            policy.IsTrackMissingMonth = this.IsTrackMissingMonth;
            policy.IsTrackIncomingPercentage = this.IsTrackIncomingPercentage;
            policy.IsTrackPayment = this.IsTrackPayment;
            policy.IsDeleted = this.IsDeleted;
            policy.OldPolicyId = this.OldPolicyId;
            policy.CarrierID = this.CarrierID;
            policy.CarrierName = this.CarrierName;
            policy.CoverageId = this.CoverageId;
            policy.CoverageName = this.CoverageName;
            policy.ClientId = this.ClientId;
            policy.ClientName = this.ClientName;
            policy.ReplacedBy = this.ReplacedBy;
            policy.DuplicateFrom = this.DuplicateFrom;
            policy.IsIncomingBasicSchedule = this.IsIncomingBasicSchedule;
            policy.IsOutGoingBasicSchedule = this.IsOutGoingBasicSchedule;
            policy.PayorId = this.PayorId;
            policy.PayorName = this.PayorName;
            policy.SplitPercentage = this.SplitPercentage;
            policy.IncomingPaymentTypeId = this.IncomingPaymentTypeId;
            policy.CreatedOn = this.CreatedOn;
            policy.CreatedBy = this.CreatedBy;
            policy.CompTypeName = this.CompTypeName;
            policy.IsSavedPolicy = this.IsSavedPolicy;
            policy.PolicyPreviousData = this.PolicyPreviousData;
            return policy;
        }
        #endregion
    }
    public partial class Payor : ICloneable, IDataErrorInfo
    {
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }

        private bool _IsInvoiceDateTracked = true;
        public bool IsInvoiceDateTracked
        {
            get { return _IsInvoiceDateTracked; }
            set { _IsInvoiceDateTracked = value; RaisePropertyChanged("IsInvoiceDateTracked"); }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            IsInvoiceDateTracked = true;
        }

        public object Clone()
        {
            Payor payor = new Payor();

            payor.PayorID = this.PayorID;
            payor.ISGlobal = this.ISGlobal;
            payor.NickName = this.NickName;
            payor.StatusID = this.StatusID;
            payor.PayorName = this.PayorName;
            payor.Region = this.Region;
            payor.PayorRegionID = this.PayorRegionID;
            payor.PayorTypeID = this.PayorTypeID;
            payor.SourceType = this.SourceType;
            payor.Carriers = this.Carriers;
            payor.UserID = this.UserID;

            return payor;
        }

        public void Copy(Payor payor)
        {
            this.NickName = payor.NickName;
            this.StatusID = payor.StatusID;
            this.PayorName = payor.PayorName;
            this.Region = payor.Region;
            this.PayorRegionID = payor.PayorRegionID;
            this.PayorTypeID = payor.PayorTypeID;
            this.SourceType = payor.SourceType;
        }

        public SettingDisplayedPayor CreateSettingDisplayPayor()
        {
            SettingDisplayedPayor settingPayor = new SettingDisplayedPayor();
            settingPayor.IsGlobal = this.ISGlobal;
            settingPayor.NickName = this.NickName;
            settingPayor.PayorID = this.PayorID;
            settingPayor.PayorName = this.PayorName;
            settingPayor.RegionID = this.PayorRegionID;
            settingPayor.Region = this.Region;

            return settingPayor;
        }

        public ConfigDisplayedPayor CreateConfigDisplayPayor()
        {
            ConfigDisplayedPayor settingPayor = new ConfigDisplayedPayor();
            settingPayor.NickName = this.NickName;
            settingPayor.PayorID = this.PayorID;
            settingPayor.PayorName = this.PayorName;
            settingPayor.RegionID = this.PayorRegionID;
            settingPayor.Region = this.Region;

            return settingPayor;
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;
                switch (columnName)
                {
                    case "PayorName":
                        if (string.IsNullOrEmpty(PayorName))
                            result = "PayorName is mandatory field.";
                        break;
                    case "NickName":
                        if (string.IsNullOrEmpty(NickName))
                            result = "NickName is mandatory field.";
                        break;
                    default:
                        break;
                }
                return result;
            }
        }
    }

    public partial class Carrier : ICloneable
    {
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }

        private bool _IsNew;
        public bool IsNew
        {
            get { return _IsNew; }
            set { _IsNew = value; RaisePropertyChanged("IsNew"); }
        }

        public override string ToString()
        {
            return this.CarrierName;
        }

        public void Copy(Carrier carrier)
        {
            this.CarrierName = carrier.CarrierName;
            this.NickName = carrier.NickName;
            this.IsTrackIncomingPercentage = carrier.IsTrackIncomingPercentage;
            this.IsTrackMissingMonth = carrier.IsTrackMissingMonth;
        }

        public object Clone()
        {
            Carrier carrier = new Carrier();

            carrier.CarrierId = this.CarrierId;
            carrier.PayerId = this.PayerId;
            carrier.LicenseeId = this.LicenseeId;
            carrier.CarrierName = this.CarrierName;
            carrier.NickName = this.NickName;
            carrier.IsGlobal = this.IsGlobal;
            carrier.IsTrackIncomingPercentage = this.IsTrackIncomingPercentage;
            carrier.IsTrackMissingMonth = this.IsTrackMissingMonth;
            carrier.Coverages = this.Coverages;
            carrier.UserID = this.UserID;

            return carrier;
        }
    }

    public partial class DisplayedCarrier
    {
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }

        public override string ToString()
        {
            return this.CarrierName;
        }
    }

    public partial class DisplayedCoverage
    {
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public partial class Coverage : ICloneable
    {
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }

        private bool _IsNew;
        public bool IsNew
        {
            get { return _IsNew; }
            set { _IsNew = value; RaisePropertyChanged("IsNew"); }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public void Copy(Coverage coverage)
        {
            this.Name = coverage.Name;
            this.NickName = coverage.NickName;
        }

        public static ObservableCollection<Coverage> Copy(ObservableCollection<Coverage> covergaes)
        {
            ObservableCollection<Coverage> coverages = null;
            if (covergaes != null && covergaes.Count != 0)
            {
                coverages = new ObservableCollection<Coverage>();
                foreach (Coverage cov in covergaes)
                {
                    Coverage temp = new Coverage();
                    temp.CoverageID = cov.CoverageID;
                    temp.Name = cov.Name;
                    temp.NickName = cov.NickName;

                    coverages.Add(temp);
                }
            }

            return coverages;
        }

        public object Clone()
        {
            Coverage coverage = new Coverage();

            coverage.CoverageID = this.CoverageID;
            coverage.CarrierID = this.CarrierID;
            coverage.PayorID = this.PayorID;
            coverage.Name = this.Name;
            coverage.NickName = this.NickName;
            coverage.LicenseeId = this.LicenseeId;
            coverage.UserID = this.UserID;

            return coverage;
        }
    }

    public partial class GlobalIncomingSchedule : ICloneable
    {
        public object Clone()
        {
            GlobalIncomingSchedule newSchedule = new GlobalIncomingSchedule();
            newSchedule.CarrierId = this.CarrierId;
            newSchedule.CarrierName = this.CarrierName;
            newSchedule.CoverageId = this.CoverageId;
            newSchedule.ProductName = this.ProductName;
            newSchedule.ScheduleTypeId = this.ScheduleTypeId;
            newSchedule.ScheduleTypeName = this.ScheduleTypeName;
            newSchedule.IsModified = this.IsModified;

            if (this.IncomingScheduleList != null && this.IncomingScheduleList.Count != 0)
            {
                newSchedule.IncomingScheduleList = new System.Collections.ObjectModel.ObservableCollection<IncomingScheduleEntry>();
                foreach (IncomingScheduleEntry schEntry in this.IncomingScheduleList)
                {
                    IncomingScheduleEntry entry = schEntry.Clone() as IncomingScheduleEntry;
                    newSchedule.IncomingScheduleList.Add(entry);
                }
            }

            return newSchedule;
        }
    }

    public partial class PolicyIncomingSchedule : ICloneable
    {
        public object Clone()
        {
            PolicyIncomingSchedule newSchedule = new PolicyIncomingSchedule();
            newSchedule.PolicyId = this.PolicyId;
            newSchedule.ScheduleTypeId = this.ScheduleTypeId;
            newSchedule.ScheduleTypeName = this.ScheduleTypeName;
            newSchedule.IsModified = this.IsModified;

            if (this.IncomingScheduleList != null && this.IncomingScheduleList.Count != 0)
            {
                newSchedule.IncomingScheduleList = new System.Collections.ObjectModel.ObservableCollection<IncomingScheduleEntry>();
                foreach (IncomingScheduleEntry schEntry in this.IncomingScheduleList)
                {
                    IncomingScheduleEntry entry = schEntry.Clone() as IncomingScheduleEntry;
                    newSchedule.IncomingScheduleList.Add(entry);
                }
            }

            return newSchedule;
        }
    }

    public partial class PolicyOutgoingSchedule : ICloneable
    {
        public object Clone()
        {
            PolicyOutgoingSchedule newSchedule = new PolicyOutgoingSchedule();
            newSchedule.PolicyId = this.PolicyId;
            newSchedule.ScheduleTypeId = this.ScheduleTypeId;
            newSchedule.ScheduleTypeName = this.ScheduleTypeName;

            if (this.OutgoingScheduleList != null && this.OutgoingScheduleList.Count != 0)
            {
                newSchedule.OutgoingScheduleList = new System.Collections.ObjectModel.ObservableCollection<OutgoingScheduleEntry>();
                foreach (OutgoingScheduleEntry schEntry in this.OutgoingScheduleList)
                {
                    OutgoingScheduleEntry entry = schEntry.Clone() as OutgoingScheduleEntry;
                    newSchedule.OutgoingScheduleList.Add(entry);
                }
            }

            return newSchedule;
        }
    }

    public partial class OutgoingScheduleEntry : ICloneable
    {
        public object Clone()
        {
            OutgoingScheduleEntry newScheduleEntry = new OutgoingScheduleEntry();
            newScheduleEntry.CoveragesScheduleId = this.CoveragesScheduleId;
            newScheduleEntry.EffectiveFromDate = this.EffectiveFromDate;
            newScheduleEntry.EffectiveToDate = this.EffectiveToDate;
            newScheduleEntry.FromRange = this.FromRange;
            newScheduleEntry.Rate = this.Rate;
            newScheduleEntry.ToRange = this.ToRange;
            newScheduleEntry.PayeeName = this.PayeeName;
            newScheduleEntry.PayeeUserCredentialId = this.PayeeUserCredentialId;
            return newScheduleEntry;
        }

    }

    public partial class User : ICloneable
    {
        private string _FullName;
        public string FullName
        {
            get { return _FullName; }
            set
            {
                _FullName = value;
                RaisePropertyChanged("FullName");
            }
        }


        private string _ConfirmPassword;
        public string ConfirmPassword
        {
            get { return _ConfirmPassword; }
            set
            {
                _ConfirmPassword = value;
                RaisePropertyChanged("ConfirmPassword");
            }
        }

        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }

        private bool _IsSaved;
        public bool IsSaved
        {
            get { return _IsSaved; }
            set { _IsSaved = value; RaisePropertyChanged("IsSaved"); }
        }

        public object Clone()
        {
            User user = new User();

            user.Address = this.Address;
            user.AttachedToLicensee = this.AttachedToLicensee;
            user.CellPhone = this.CellPhone;
            user.City = this.City;
            user.Company = this.Company;
            user.CreateOn = this.CreateOn;
            user.Email = this.Email;
            user.Fax = this.Fax;
            user.FirstName = this.FirstName;
            user.FirstYearDefault = this.FirstYearDefault;
            user.IsDeleted = this.IsDeleted;
            user.IsHouseAccount = this.IsHouseAccount;
            user.IsLicenseDeleted = this.IsLicenseDeleted;
            user.LastName = this.LastName;
            user.LicenseeId = this.LicenseeId;
            user.LicenseeName = this.LicenseeName;
            user.LinkedUsers = this.LinkedUsers;
            user.NickName = this.NickName;
            user.OfficePhone = this.OfficePhone;
            user.Password = this.Password;
            user.PasswordHintA = this.PasswordHintA;
            user.PasswordHintQ = this.PasswordHintQ;
            user.Permissions = this.Permissions;
            user.RembStatus = this.RembStatus;
            user.RenewalDefault = this.RenewalDefault;
            user.Role = this.Role;
            user.SStatus = this.SStatus;
            user.State = this.State;
            user.UserClients = this.UserClients;
            user.UserCredentialID = this.UserCredentialID;
            user.UserName = this.UserName;
            user.ZipCode = this.ZipCode;

            return user;
        }

        public void Copy(User user)
        {
            this.Address = user.Address;
            //this.AttachedToLicensee = user.AttachedToLicensee;
            this.CellPhone = user.CellPhone;
            this.City = user.City;
            //this.Commissions = user.Commissions;
            this.Company = user.Company;
            //this.CreateOn = user.CreateOn;
            this.Email = user.Email;
            this.Fax = user.Fax;
            this.FirstName = user.FirstName;
            this.FirstYearDefault = user.FirstYearDefault;
            //this.IsDeleted = user.IsDeleted;
            this.IsHouseAccount = user.IsHouseAccount;
            this.IsLicenseDeleted = user.IsLicenseDeleted;
            this.LastName = user.LastName;
            this.LicenseeId = user.LicenseeId;
            this.LicenseeName = user.LicenseeName;
            this.LinkedUsers = user.LinkedUsers;
            this.NickName = user.NickName;
            this.OfficePhone = user.OfficePhone;
            this.Password = user.Password;
            this.PasswordHintA = user.PasswordHintA;
            this.PasswordHintQ = user.PasswordHintQ;
            //this.Permissions = user.Permissions;
            this.RembStatus = user.RembStatus;
            this.RenewalDefault = user.RenewalDefault;
            this.Role = user.Role;
            this.SStatus = user.SStatus;
            this.State = user.State;
            //this.UserClients = user.UserClients;
            this.UserCredentialID = user.UserCredentialID;
            this.UserName = user.UserName;
            this.ZipCode = user.ZipCode;
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            FullName = LastName + " " + FirstName;
            ConfirmPassword = Password;
            IsSaved = true;
        }

        public override string ToString()
        {
            return this.NickName;
        }
    }

    public partial class LinkedUser
    {
        public void Copy(User user)
        {
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.NickName = user.NickName;
            this.UserName = user.UserName;
            this.UserId = user.UserCredentialID;
            this.IsConnected = false;
        }
    }

    public partial class ServiceChargeType
    {
        public override string ToString()
        {
            return this.ServiceChargeName;
        }
    }

    public partial class ServiceProduct
    {
        public override string ToString()
        {
            return this.ServiceName;
        }
    }

    public partial class PayorToolAvailablelFieldType
    {
        public override string ToString()
        {
            return this.FieldName;
        }
    }

    public partial class Report
    {
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }
    }

    public partial class Region
    {
        public override string ToString()
        {
            return this.RegionName;
        }
    }

    public partial class Batch
    {
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }
    }

    public partial class ExposedDEU
    {
        private int _ReferenceNo;
        public int ReferenceNo
        {
            get { return _ReferenceNo; }
            set
            {
                _ReferenceNo = value;
                RaisePropertyChanged("ReferenceNo");
            }
        }

        private bool _EnableEditDeleteOperation;
        public bool EnableEditDeleteOperation
        {
            get
            {
                return _EnableEditDeleteOperation;
            }
            set
            {
                _EnableEditDeleteOperation = value;
                RaisePropertyChanged("EnableEditDeleteOperation");
            }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            EnableEditDeleteOperation = true;
        }
    }

    public partial class DisplayedPayor
    {
        private bool _IsChecked;
        public bool IsChecked
        {
            get { return _IsChecked; }
            set { _IsChecked = value; RaisePropertyChanged("IsChecked"); }
        }
    }

    public partial class ConfigDisplayedPayor : ICloneable
    {
        private Region _Region;
        public Region Region
        {
            get { return _Region; }
            set
            {
                _Region = value;
                RaisePropertyChanged("Region");
            }
        }

        public object Clone()
        {
            SettingDisplayedPayor payor = new SettingDisplayedPayor();

            payor.PayorID = this.PayorID;
            payor.NickName = this.NickName;
            payor.PayorName = this.PayorName;
            payor.Region = this.Region;
            payor.RegionID = this.RegionID;

            return payor;
        }

        public void Copy(Payor payor)
        {
            this.NickName = payor.NickName;
            this.PayorName = payor.PayorName;
            this.Region = payor.Region;
            this.RegionID = payor.PayorRegionID;
        }

        public Payor CreatePayor()
        {
            Payor payor = new Payor();
            payor.PayorID = this.PayorID;
            payor.NickName = this.NickName;
            payor.PayorName = this.PayorName;
            payor.Region = this.Region;
            payor.PayorRegionID = this.RegionID;

            return payor;
        }
    }

    public partial class SettingDisplayedPayor : ICloneable
    {
        private Region _Region;
        public Region Region
        {
            get { return _Region; }
            set
            {
                _Region = value;
                RaisePropertyChanged("Region");
            }
        }

        public object Clone()
        {
            SettingDisplayedPayor payor = new SettingDisplayedPayor();

            payor.PayorID = this.PayorID;
            payor.IsGlobal = this.IsGlobal;
            payor.NickName = this.NickName;
            payor.PayorName = this.PayorName;
            payor.Region = this.Region;
            payor.RegionID = this.RegionID;
            payor.SourceType = this.SourceType;

            return payor;
        }

        public void Copy(Payor payor)
        {
            this.NickName = payor.NickName;
            this.PayorName = payor.PayorName;
            this.Region = payor.Region;
            this.RegionID = payor.PayorRegionID;
            this.SourceType = payor.SourceType ?? 0;
        }

        public Payor CreatePayor()
        {
            Payor payor = new Payor();
            payor.PayorID = this.PayorID;
            payor.NickName = this.NickName;
            payor.PayorName = this.PayorName;
            payor.Region = this.Region;
            payor.PayorRegionID = this.RegionID;
            payor.SourceType = this.SourceType;

            return payor;
        }
    }

    public partial class PayorIncomingSchedule : ICloneable
    {
        public object Clone()
        {
            PayorIncomingSchedule newSchedule = new PayorIncomingSchedule();
            newSchedule.IncomingScheduleID = this.IncomingScheduleID;
            newSchedule.LicenseeID = this.LicenseeID;
            newSchedule.PayorID = this.PayorID;
            newSchedule.ScheduleTypeId = this.ScheduleTypeId;
            newSchedule.CarrierID = this.CarrierID;
            newSchedule.CoverageID = this.CoverageID;
            newSchedule.ProductType = this.ProductType;
            newSchedule.FirstYearPercentage = this.FirstYearPercentage;
            newSchedule.RenewalPercentage = this.RenewalPercentage;
            newSchedule.SplitPercentage = this.SplitPercentage;
            newSchedule.Advance = this.Advance;
            newSchedule.CreatedBy = this.CreatedBy;
            newSchedule.CreatedOn = this.CreatedOn;
            newSchedule.ModifiedBy = this.ModifiedBy;
            newSchedule.ModifiedOn = this.ModifiedOn;
            if (this.GradedSchedule != null && this.GradedSchedule.Count != 0)
            {
                newSchedule.GradedSchedule = new System.Collections.ObjectModel.ObservableCollection<Graded>();
                foreach (Graded data in this.GradedSchedule)
                {
                    Graded entry = data.Clone() as Graded;
                    newSchedule.GradedSchedule.Add(entry);
                }
            }

            if (this.NonGradedSchedule != null && this.NonGradedSchedule.Count != 0)
            {
                newSchedule.NonGradedSchedule = new System.Collections.ObjectModel.ObservableCollection<NonGraded>();
                foreach (NonGraded data in this.NonGradedSchedule)
                {
                    NonGraded entry = data.Clone() as NonGraded;
                    newSchedule.NonGradedSchedule.Add(entry);
                }
            }

            newSchedule.Mode = this.Mode;
            newSchedule.CustomType = this.CustomType;
            return newSchedule;
        }

        public void Copy(PayorIncomingSchedule schedule)
        {
            this.IncomingScheduleID = schedule.IncomingScheduleID;
            this.LicenseeID = schedule.LicenseeID;
            this.PayorID = schedule.PayorID;
            this.ScheduleTypeId = schedule.ScheduleTypeId;
            this.CarrierID = schedule.CarrierID;
            this.CoverageID = schedule.CoverageID;
            this.ProductType = schedule.ProductType;
            this.FirstYearPercentage = schedule.FirstYearPercentage;
            this.RenewalPercentage = schedule.RenewalPercentage;
            this.SplitPercentage = schedule.SplitPercentage;
            this.Advance = schedule.Advance;
            this.CreatedBy = schedule.CreatedBy;
            this.CreatedOn = schedule.CreatedOn;
            this.ModifiedBy = schedule.ModifiedBy;
            this.ModifiedOn = schedule.ModifiedOn;
            this.Mode = schedule.Mode;
            this.CustomType = schedule.CustomType;
            if (schedule.GradedSchedule != null)
            {
                this.GradedSchedule = new System.Collections.ObjectModel.ObservableCollection<Graded>();
                foreach (Graded data in schedule.GradedSchedule)
                {
                    Graded entry = data.Clone() as Graded;
                    this.GradedSchedule.Add(entry);
                }
            }

            if (schedule.NonGradedSchedule != null)
            {
                this.NonGradedSchedule = new System.Collections.ObjectModel.ObservableCollection<NonGraded>();
                foreach (NonGraded data in schedule.NonGradedSchedule)
                {
                    NonGraded entry = data.Clone() as NonGraded;
                    this.NonGradedSchedule.Add(entry);
                }
            }
        }
    }
    public partial class PolicyToolIncommingShedule : ICloneable
    {
        public PolicyToolIncommingShedule()
        {
            this.CustomType = CustomMode.Graded;
        }
        
        public object Clone()
        {
            PolicyToolIncommingShedule newSchedule = new PolicyToolIncommingShedule();
            newSchedule.IncomingScheduleID = this.IncomingScheduleID;
            newSchedule.LicenseeID = this.LicenseeID;
            newSchedule.PayorID = this.PayorID;
            newSchedule.ScheduleTypeId = this.ScheduleTypeId;
            newSchedule.CarrierID = this.CarrierID;
            newSchedule.CoverageID = this.CoverageID;
            newSchedule.ProductType = this.ProductType;
            newSchedule.FirstYearPercentage = this.FirstYearPercentage;
            newSchedule.RenewalPercentage = this.RenewalPercentage;
            newSchedule.SplitPercentage = this.SplitPercentage;
            newSchedule.Advance = this.Advance;
            newSchedule.CreatedBy = this.CreatedBy;
            newSchedule.CreatedOn = this.CreatedOn;
            newSchedule.ModifiedBy = this.ModifiedBy;
            newSchedule.ModifiedOn = this.ModifiedOn;
            if (this.GradedSchedule != null && this.GradedSchedule.Count != 0)
            {
                newSchedule.GradedSchedule = new System.Collections.ObjectModel.ObservableCollection<Graded>();
                foreach (Graded data in this.GradedSchedule)
                {
                    Graded entry = data.Clone() as Graded;
                    newSchedule.GradedSchedule.Add(entry);
                }
            }
            
                 if (this.NonGradedSchedule != null && this.NonGradedSchedule.Count != 0)
            {
                newSchedule.NonGradedSchedule = new System.Collections.ObjectModel.ObservableCollection<NonGraded>();
                foreach (NonGraded data in this.NonGradedSchedule)
                {
                    NonGraded entry = data.Clone() as NonGraded;
                    newSchedule.NonGradedSchedule.Add(entry);
                }
            }
         
            newSchedule.Mode = this.Mode;
            newSchedule.CustomType = this.CustomType;

            return newSchedule;
        }

        public void Copy(PolicyToolIncommingShedule schedule)
        {
            this.IncomingScheduleID = schedule.IncomingScheduleID;
            this.LicenseeID = schedule.LicenseeID;
            this.PayorID = schedule.PayorID;
            this.ScheduleTypeId = schedule.ScheduleTypeId;
            this.CarrierID = schedule.CarrierID;
            this.CoverageID = schedule.CoverageID;
            this.ProductType = schedule.ProductType;
            this.FirstYearPercentage = schedule.FirstYearPercentage;
            this.RenewalPercentage = schedule.RenewalPercentage;
            this.SplitPercentage = schedule.SplitPercentage;
            this.Advance = schedule.Advance;
            this.CreatedBy = schedule.CreatedBy;
            this.CreatedOn = schedule.CreatedOn;
            this.ModifiedBy = schedule.ModifiedBy;
            this.ModifiedOn = schedule.ModifiedOn;
            this.Mode = schedule.Mode;
            this.CustomType = schedule.CustomType;
            if (schedule.GradedSchedule != null)
            {
                this.GradedSchedule = new System.Collections.ObjectModel.ObservableCollection<Graded>();
                foreach (Graded data in schedule.GradedSchedule)
                {
                    Graded entry = data.Clone() as Graded;
                    this.GradedSchedule.Add(entry);
                }
            }

            if (schedule.NonGradedSchedule != null)
            {
                this.NonGradedSchedule = new System.Collections.ObjectModel.ObservableCollection<NonGraded>();
                foreach (NonGraded data in schedule.NonGradedSchedule)
                {
                    NonGraded entry = data.Clone() as NonGraded;
                    this.NonGradedSchedule.Add(entry);
                }
            }

        }
        public void CopyFromPayor(PayorIncomingSchedule schedule)
        {
            this.IncomingScheduleID = schedule.IncomingScheduleID;
            this.LicenseeID = schedule.LicenseeID;
            this.PayorID = schedule.PayorID;
            this.ScheduleTypeId = schedule.ScheduleTypeId;
            this.CarrierID = schedule.CarrierID;
            this.CoverageID = schedule.CoverageID;
            this.ProductType = schedule.ProductType;
            this.FirstYearPercentage = schedule.FirstYearPercentage;
            this.RenewalPercentage = schedule.RenewalPercentage;
            this.SplitPercentage = schedule.SplitPercentage;
            this.Advance = schedule.Advance;
            this.CreatedBy = schedule.CreatedBy;
            this.CreatedOn = schedule.CreatedOn;
            this.ModifiedBy = schedule.ModifiedBy;
            this.ModifiedOn = schedule.ModifiedOn;
            this.Mode = schedule.Mode;
            this.CustomType = schedule.CustomType;
            if (schedule.GradedSchedule != null )
            {
                this.GradedSchedule = new System.Collections.ObjectModel.ObservableCollection<Graded>();
                foreach (Graded data in schedule.GradedSchedule)
                {
                    Graded entry = data.Clone() as Graded;
                    this.GradedSchedule.Add(entry);
                }
            }

            if (schedule.NonGradedSchedule != null )
            {
                this.NonGradedSchedule = new System.Collections.ObjectModel.ObservableCollection<NonGraded>();
                foreach (NonGraded data in schedule.NonGradedSchedule)
                {
                    NonGraded entry = data.Clone() as NonGraded;
                    this.NonGradedSchedule.Add(entry);
                }
            }
        }
    }
}
