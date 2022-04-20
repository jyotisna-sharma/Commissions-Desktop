using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.VM
{
  public class ServiceClients:IDisposable
  {
    private MastersClient _masterClient;
    public MastersClient MasterClient
    {
      get
      {
        if (_masterClient == null || _masterClient.State == System.ServiceModel.CommunicationState.Closed)
          _masterClient = new MastersClient();
        return _masterClient;
      }
    }
    private AdvancedPayeeClient _advancedPayeeClient;
    public AdvancedPayeeClient AdvancedPayeeClient
    {
      get
      {
        if (_advancedPayeeClient == null || _advancedPayeeClient.State == System.ServiceModel.CommunicationState.Closed)
          _advancedPayeeClient = new AdvancedPayeeClient();
        return _advancedPayeeClient;
      }
    }
    private BatchClient _batchClient;
    public BatchClient BatchClient
    {
      get
      {
        if (_batchClient == null || _batchClient.State == System.ServiceModel.CommunicationState.Closed)
          _batchClient = new BatchClient();
        return _batchClient;
      }
    }
    private BillingLineDetailClient _billingLineDetailClient;
    public BillingLineDetailClient BillingLineDetailClient
    {
      get
      {
        if (_billingLineDetailClient == null || _billingLineDetailClient.State == System.ServiceModel.CommunicationState.Closed)
          _billingLineDetailClient = new BillingLineDetailClient();
        return _billingLineDetailClient;
      }
      set { }
    }
    private CarrierClient _carrierClient;
    public CarrierClient CarrierClient
    {
      get
      {
        if (_carrierClient == null || _carrierClient.State == System.ServiceModel.CommunicationState.Closed
            || _carrierClient.State == System.ServiceModel.CommunicationState.Faulted)
          _carrierClient = new CarrierClient();
        return _carrierClient;
      }
      set { }
    }
    private ClientClient _clientClient;
    public ClientClient ClientClient
    {
      get
      {
        if (_clientClient == null || _clientClient.State == System.ServiceModel.CommunicationState.Closed
            || _clientClient.State == System.ServiceModel.CommunicationState.Faulted)
          _clientClient = new ClientClient();
        return _clientClient;
      }
      set { }
    }
    private ClientAndPaymentClient _clientAndPaymentClient;
    public ClientAndPaymentClient ClientAndPaymentClient
    {
      get
      {
        if (_clientAndPaymentClient == null || _clientAndPaymentClient.State == System.ServiceModel.CommunicationState.Closed)
          _clientAndPaymentClient = new ClientAndPaymentClient();
        return _clientAndPaymentClient;
      }
      set { }
    }
    private ComDeptServiceClient _comDeptServiceClient;
    public ComDeptServiceClient ComDeptServiceClient
    {
      get
      {
        if (_comDeptServiceClient == null || _comDeptServiceClient.State == System.ServiceModel.CommunicationState.Closed)
          _comDeptServiceClient = new ComDeptServiceClient();
        return _comDeptServiceClient;
      }
      set { }
    }
    private LicenseeNoteClient _licenseeNoteClient;
    public LicenseeNoteClient LicenseeNoteClient
    {
      get
      {
        if (_licenseeNoteClient == null || _licenseeNoteClient.State == System.ServiceModel.CommunicationState.Closed)
          _licenseeNoteClient = new LicenseeNoteClient();
        return _licenseeNoteClient;
      }
    }

    private CommissionEntryClient _commissionEntryClient;
    public CommissionEntryClient CommissionEntryClient
    {
      get
      {
        if (_commissionEntryClient == null || _commissionEntryClient.State == System.ServiceModel.CommunicationState.Closed)
          _commissionEntryClient = new CommissionEntryClient();
        return _commissionEntryClient;
      }
      set { }
    }
    private CoverageClient _coverageClient;
    public CoverageClient CoverageClient
    {
      get
      {
        if (_coverageClient == null || _coverageClient.State == System.ServiceModel.CommunicationState.Closed)
          _coverageClient = new CoverageClient();
        return _coverageClient;
      }
      set { }
    }
    private FollowupIssueClient _followupIssueClient;
    public FollowupIssueClient FollowupIssueClient
    {
      get
      {
        if (_followupIssueClient == null || _followupIssueClient.State == System.ServiceModel.CommunicationState.Closed)
          _followupIssueClient = new FollowupIssueClient();
        return _followupIssueClient;
      }
      set { }
    }
    private FormulaClient _formulaClient;
    public FormulaClient FormulaClient
    {
      get
      {
        if (_formulaClient == null || _formulaClient.State == System.ServiceModel.CommunicationState.Closed)
          _formulaClient = new FormulaClient();
        return _formulaClient;
      }
      set { }
    }
    private GlobalPayorContactClient _globalPayorContactClient;
    public GlobalPayorContactClient GlobalPayorContactClient
    {
      get
      {
        if (_globalPayorContactClient == null || _globalPayorContactClient.State == System.ServiceModel.CommunicationState.Closed)
          _globalPayorContactClient = new GlobalPayorContactClient();
        return _globalPayorContactClient;
      }
      set { }
    }
    private IncomingPamentClient _incomingPamentClient;
    public IncomingPamentClient IncomingPamentClient
    {
      get
      {
        if (_incomingPamentClient == null || _incomingPamentClient.State == System.ServiceModel.CommunicationState.Closed)
          _incomingPamentClient = new IncomingPamentClient();
        return _incomingPamentClient;
      }
      set { }
    }

    private GlobalIncomingScheduleClient _globalIncomingScheduleClient;
    public GlobalIncomingScheduleClient GlobalIncomingScheduleClient
    {
      get
      {
        if (_globalIncomingScheduleClient == null || _globalIncomingScheduleClient.State == System.ServiceModel.CommunicationState.Closed)
          _globalIncomingScheduleClient = new GlobalIncomingScheduleClient();
        return _globalIncomingScheduleClient;
      }
      set { }
    }

    private IncomingScheduleClient _incomingScheduleClient;
    public IncomingScheduleClient IncomingScheduleClient
    {
      get
      {
        if (_incomingScheduleClient == null || _incomingScheduleClient.State == System.ServiceModel.CommunicationState.Closed)
          _incomingScheduleClient = new IncomingScheduleClient();
        return _incomingScheduleClient;
      }
      set { }
    }

    private OutgoingScheduleClient _OutgoingScheduleClient;
    public OutgoingScheduleClient OutgoingScheduleClient
    {
      get
      {
        if (_OutgoingScheduleClient == null || _OutgoingScheduleClient.State == System.ServiceModel.CommunicationState.Closed)
          _OutgoingScheduleClient = new OutgoingScheduleClient();
        return _OutgoingScheduleClient;
      }
      set { }
    }

    private OutgoingPaymentClient _OutGoingPaymentClient;
    public OutgoingPaymentClient OutGoingPaymentClient
    {
      get
      {
        if (_OutGoingPaymentClient == null || _OutGoingPaymentClient.State == System.ServiceModel.CommunicationState.Closed || _OutGoingPaymentClient.State == System.ServiceModel.CommunicationState.Faulted)
          _OutGoingPaymentClient = new OutgoingPaymentClient();
        return _OutGoingPaymentClient;
      }
      set { }
    }


    private InvoiceClient _invoiceClient;
    public InvoiceClient InvoiceClient
    {
      get
      {
        if (_invoiceClient == null || _invoiceClient.State == System.ServiceModel.CommunicationState.Closed)
          _invoiceClient = new InvoiceClient();
        return _invoiceClient;
      }
      set { }
    }
    private JournalClient _journalClient;
    public JournalClient JournalClient
    {
      get
      {
        if (_journalClient == null || _journalClient.State == System.ServiceModel.CommunicationState.Closed)
          _journalClient = new JournalClient();
        return _journalClient;
      }
      set { }
    }
    private LicenseeClient _licenseeClient;
    public LicenseeClient LicenseeClient
    {
      get
      {
        if (_licenseeClient == null || _licenseeClient.State == System.ServiceModel.CommunicationState.Closed)
          _licenseeClient = new LicenseeClient();
        return _licenseeClient;
      }
      set { }
    }
    private NewsClient _newsClient;
    public NewsClient NewsClient
    {
      get
      {
        if (_newsClient == null || _newsClient.State == System.ServiceModel.CommunicationState.Closed)
          _newsClient = new NewsClient();
        return _newsClient;
      }
      set { }
    }
    private NoteClient _noteClient;
    public NoteClient NoteClient
    {
      get
      {
        if (_noteClient == null || _noteClient.State == System.ServiceModel.CommunicationState.Closed || _noteClient.State == System.ServiceModel.CommunicationState.Faulted)
          _noteClient = new NoteClient();
        return _noteClient;
      }
      set { }
    }
    //private  OutgoingPaymentClient _outgoingPaymentClient;
    //public  OutgoingPaymentClient OutgoingPaymentClient
    //{
    //    get
    //    {
    //        if (_outgoingPaymentClient == null || _outgoingPaymentClient.State == System.ServiceModel.CommunicationState.Closed)
    //            _outgoingPaymentClient = new OutgoingPaymentClient();
    //        return _outgoingPaymentClient;
    //    }
    //    set { }
    //}
    private PayeeClient _payeeClient;
    public PayeeClient PayeeClient
    {
      get
      {
        if (_payeeClient == null || _payeeClient.State == System.ServiceModel.CommunicationState.Closed)
          _payeeClient = new PayeeClient();
        return _payeeClient;
      }
      set { }
    }
    private PayorClient _payorClient;
    public PayorClient PayorClient
    {
      get
      {
        if (_payorClient == null || _payorClient.State == System.ServiceModel.CommunicationState.Closed)
          _payorClient = new PayorClient();
        return _payorClient;
      }
      set { }
    }
    private PayorDefaultsClient _payorDefaultsClient;
    public PayorDefaultsClient PayorDefaultsClient
    {
      get
      {
        if (_payorDefaultsClient == null || _payorDefaultsClient.State == System.ServiceModel.CommunicationState.Closed)
          _payorDefaultsClient = new PayorDefaultsClient();
        return _payorDefaultsClient;
      }
    }
    private PayorToolClient _payorToolClient;
    public PayorToolClient PayorToolClient
    {
      get
      {
        if (_payorToolClient == null || _payorToolClient.State == System.ServiceModel.CommunicationState.Closed)
          _payorToolClient = new PayorToolClient();
        return _payorToolClient;
      }
      set { }
    }
    private PayorToolAvailablelFieldTypeClient _payorToolAvailablelFieldTypeClient;
    public PayorToolAvailablelFieldTypeClient PayorToolAvailablelFieldTypeClient
    {
      get
      {
        if (_payorToolAvailablelFieldTypeClient == null || _payorToolAvailablelFieldTypeClient.State == System.ServiceModel.CommunicationState.Closed)
          _payorToolAvailablelFieldTypeClient = new PayorToolAvailablelFieldTypeClient();
        return _payorToolAvailablelFieldTypeClient;
      }
    }
    private PayorToolFieldClient _payorToolFieldClient;
    public PayorToolFieldClient PayorToolFieldClient
    {
      get
      {
        if (_payorToolFieldClient == null || _payorToolFieldClient.State == System.ServiceModel.CommunicationState.Closed)
          _payorToolFieldClient = new PayorToolFieldClient();
        return _payorToolFieldClient;
      }
    }
    private PayorUserWebSiteClient _payorUserWebSiteClient;
    public PayorUserWebSiteClient PayorUserWebSiteClient
    {
      get
      {
        if (_payorUserWebSiteClient == null || _payorUserWebSiteClient.State == System.ServiceModel.CommunicationState.Closed)
          _payorUserWebSiteClient = new PayorUserWebSiteClient();
        return _payorUserWebSiteClient;
      }
    }
    private PolicyClient _policyClient;
    public PolicyClient PolicyClient
    {
      get
      {
        if (_policyClient == null || _policyClient.State == System.ServiceModel.CommunicationState.Closed || _policyClient.State == System.ServiceModel.CommunicationState.Faulted)
          _policyClient = new PolicyClient();
        return _policyClient;
      }
    }

    private PolicyToolIncommingSheduleClient _policyIncomingScheduleClient;
    public PolicyToolIncommingSheduleClient PolicyIncomingScheduleClient
    {
      get
      {
        if (_policyIncomingScheduleClient == null || _policyIncomingScheduleClient.State == System.ServiceModel.CommunicationState.Closed)
          _policyIncomingScheduleClient = new PolicyToolIncommingSheduleClient();
        return _policyIncomingScheduleClient;
      }
    }

    private PolicyDetailsClient _policyDetailsClient;
    public PolicyDetailsClient PolicyDetailsClient
    {
      get
      {
        if (_policyDetailsClient == null || _policyDetailsClient.State == System.ServiceModel.CommunicationState.Closed)
          _policyDetailsClient = new PolicyDetailsClient();
        return _policyDetailsClient;
      }
    }
    private PolicyLearnedFieldClient _policyLearnedFieldClient;
    public PolicyLearnedFieldClient PolicyLearnedFieldClient
    {
      get
      {
        if (_policyLearnedFieldClient == null || _policyLearnedFieldClient.State == System.ServiceModel.CommunicationState.Closed)
          _policyLearnedFieldClient = new PolicyLearnedFieldClient();
        return _policyLearnedFieldClient;
      }
    }
    private PolicySettingsClient _policySettingsClient;
    public PolicySettingsClient PolicySettingsClient
    {
      get
      {
        if (_policySettingsClient == null || _policySettingsClient.State == System.ServiceModel.CommunicationState.Closed)
          _policySettingsClient = new PolicySettingsClient();
        return _policySettingsClient;
      }
    }
    private StatementClient _statementClient;
    public StatementClient StatementClient
    {
      get
      {
        if (_statementClient == null || _statementClient.State == System.ServiceModel.CommunicationState.Closed)
          _statementClient = new StatementClient();
        return _statementClient;
      }
    }
    private UserClient _userClient;
    public UserClient UserClient
    {
      get
      {
        if (_userClient == null || _userClient.State == System.ServiceModel.CommunicationState.Closed
            || _userClient.State == System.ServiceModel.CommunicationState.Faulted)
          _userClient = new UserClient();
        return _userClient;
      }
    }
    private StatementDatesClient _statementDatesClient;
    public StatementDatesClient StatementDatesClient
    {
      get
      {
        if (_statementDatesClient == null || _statementDatesClient.State == System.ServiceModel.CommunicationState.Closed)
          _statementDatesClient = new StatementDatesClient();
        return _statementDatesClient;
      }
    }

    //ARG
    private LastViewPolicyClient _lastViewPolicyClient;
    public LastViewPolicyClient LastViewPolicyClient
    {

      get
      {
        try
        {
          if (_lastViewPolicyClient == null || _lastViewPolicyClient.State == System.ServiceModel.CommunicationState.Closed)
            _lastViewPolicyClient = new LastViewPolicyClient();
          return _lastViewPolicyClient;

        }
        catch //(System.ServiceModel.FaultException<ApplicationFault> app)
        {

          //   string str = app.Detail.Error;
          return null;
        }

      }

    }

    // ARG
    private PolicySearchedClient _policySearchedClient;
    public PolicySearchedClient PolicySearchedClient
    {
      get
      {
        if (_policySearchedClient == null || _policySearchedClient.State == System.ServiceModel.CommunicationState.Closed)
          _policySearchedClient = new PolicySearchedClient();
        return _policySearchedClient;
      }
    }


    private IssueCategoryClient _issueCategoryClient;
    public IssueCategoryClient IssueCategoryClient
    {

      get
      {
        if (_issueCategoryClient == null || _issueCategoryClient.State == System.ServiceModel.CommunicationState.Closed)
          _issueCategoryClient = new IssueCategoryClient();
        return _issueCategoryClient;
      }
    }
    ////
    private IssueReasonsClient _issueReasonClient;
    public IssueReasonsClient IssueReasonClient
    {

      get
      {
        if (_issueReasonClient == null || _issueReasonClient.State == System.ServiceModel.CommunicationState.Closed)
          _issueReasonClient = new IssueReasonsClient();
        return _issueReasonClient;
      }
    }


    private IssueResultsClient _issueResultClient;
    public IssueResultsClient IssueResultClient
    {

      get
      {
        if (_issueResultClient == null || _issueResultClient.State == System.ServiceModel.CommunicationState.Closed)
          _issueResultClient = new IssueResultsClient();
        return _issueResultClient;
      }
    }


    private IssueStatusClient _issueStatusClient;
    public IssueStatusClient IssueStatusClient
    {

      get
      {
        if (_issueStatusClient == null || _issueStatusClient.State == System.ServiceModel.CommunicationState.Closed)
          _issueStatusClient = new IssueStatusClient();
        return _issueStatusClient;
      }
    }

    private BatchFilesClient _batchFilesClient;
    public BatchFilesClient BatchFilesClient
    {

      get
      {
        if (_batchFilesClient == null || _batchFilesClient.State == System.ServiceModel.CommunicationState.Closed)
          _batchFilesClient = new BatchFilesClient();
        return _batchFilesClient;
      }
    }

    private ExportCardPayeeInfoClient _exportCardPayeeInfoClient;
    public ExportCardPayeeInfoClient ExportCardPayeeInfoClient
    {

      get
      {
        if (_exportCardPayeeInfoClient == null || _exportCardPayeeInfoClient.State == System.ServiceModel.CommunicationState.Closed)
          _exportCardPayeeInfoClient = new ExportCardPayeeInfoClient();
        return _exportCardPayeeInfoClient;
      }
    }

    private CalculateVariableServiceClient _calcVariableClient;
    public CalculateVariableServiceClient CalcVariableClient
    {

      get
      {
        if (_calcVariableClient == null || _calcVariableClient.State == System.ServiceModel.CommunicationState.Closed)
          _calcVariableClient = new CalculateVariableServiceClient();
        return _calcVariableClient;
      }
    }

    private ImportTransactionFileClient _importTransactionFileClient;
    public ImportTransactionFileClient ImportTSFileClient
    {

      get
      {
        if (_importTransactionFileClient == null || _importTransactionFileClient.State == System.ServiceModel.CommunicationState.Closed)
          _importTransactionFileClient = new ImportTransactionFileClient();
        return _importTransactionFileClient;
      }
    }

    private LicenseeInvoiceClient _licenseeInvoiceClient;
    public LicenseeInvoiceClient LicenseeInvoiceClient
    {

      get
      {
        if (_licenseeInvoiceClient == null || _licenseeInvoiceClient.State == System.ServiceModel.CommunicationState.Closed)
          _licenseeInvoiceClient = new LicenseeInvoiceClient();
        return _licenseeInvoiceClient;
      }
    }

    private ExportDateClient _exportDateClient;
    public ExportDateClient exportDateClient
    {

      get
      {
        if (_exportDateClient == null || _exportDateClient.State == System.ServiceModel.CommunicationState.Closed)
          _exportDateClient = new ExportDateClient();
        return _exportDateClient;
      }
    }

    private InvoiceLineClient _invocieLineClient;
    public InvoiceLineClient InvoiceLineClient
    {

      get
      {
        if (_invocieLineClient == null || _invocieLineClient.State == System.ServiceModel.CommunicationState.Closed)
          _invocieLineClient = new InvoiceLineClient();
        return _invocieLineClient;
      }
    }

    private SendMailClient _sendMailClient;
    public SendMailClient SendMailClient
    {

      get
      {
        if (_sendMailClient == null || _sendMailClient.State == System.ServiceModel.CommunicationState.Closed)
          _sendMailClient = new SendMailClient();
        return _sendMailClient;
      }
    }

    private PolicyToLearnPostClient _policytoLearnedPostClient;
    public PolicyToLearnPostClient PolicyToLearnPostClient
    {

      get
      {
        if (_policytoLearnedPostClient == null || _policytoLearnedPostClient.State == System.ServiceModel.CommunicationState.Closed)
          _policytoLearnedPostClient = new PolicyToLearnPostClient();
        return _policytoLearnedPostClient;
      }
    }

    private LearnedToPolicyPostClient _learnedToPolictPostClient;
    public LearnedToPolicyPostClient LearnedToPolicyPostClient
    {

      get
      {
        if (_learnedToPolictPostClient == null || _learnedToPolictPostClient.State == System.ServiceModel.CommunicationState.Closed)
          _learnedToPolictPostClient = new LearnedToPolicyPostClient();
        return _learnedToPolictPostClient;
      }
    }

    private DataEntryUnitClient _DeuClient;
    public DataEntryUnitClient DeuClient
    {

      get
      {
        if (_DeuClient == null || _DeuClient.State == System.ServiceModel.CommunicationState.Closed)
          _DeuClient = new DataEntryUnitClient();
        return _DeuClient;
      }
    }

    private PostUtilClient _PostUtilClient;
    public PostUtilClient PostUtilClient
    {

      get
      {
        if (_PostUtilClient == null || _PostUtilClient.State == System.ServiceModel.CommunicationState.Closed)
          _PostUtilClient = new PostUtilClient();
        return _PostUtilClient;
      }
    }
    //private  SystemConstantClient _sysConstantClient;
    //public  SystemConstantClient SystemConstClient
    //{

    //    get
    //    {
    //        if (_sysConstantClient == null || _sysConstantClient.State == System.ServiceModel.CommunicationState.Closed)
    //            _sysConstantClient = new SystemConstClient();
    //        return _sysConstantClient;LinkPaymentReciptRecordsClient
    //    }
    //}
    private LinkPaymentPoliciesClient _linkPaymentPoliciesClient;
    public LinkPaymentPoliciesClient LinkPaymentPoliciesClient
    {

      get
      {
        if (_linkPaymentPoliciesClient == null || _linkPaymentPoliciesClient.State == System.ServiceModel.CommunicationState.Closed)
          _linkPaymentPoliciesClient = new LinkPaymentPoliciesClient();
        return _linkPaymentPoliciesClient;
      }
    }

    private LinkPaymentReciptRecordsClient _linkPaymentReciptRecordsClient;
    public LinkPaymentReciptRecordsClient LinkPaymentReciptRecordsClient
    {

      get
      {
        if (_linkPaymentReciptRecordsClient == null || _linkPaymentReciptRecordsClient.State == System.ServiceModel.CommunicationState.Closed)
          _linkPaymentReciptRecordsClient = new LinkPaymentReciptRecordsClient();
        return _linkPaymentReciptRecordsClient;
      }
    }

    private BatchStatmentRecordsClient _batchStatmentRecordsClient;
    public BatchStatmentRecordsClient BatchStatmentRecordsClient
    {

      get
      {
        if (_batchStatmentRecordsClient == null || _batchStatmentRecordsClient.State == System.ServiceModel.CommunicationState.Closed)
          _batchStatmentRecordsClient = new BatchStatmentRecordsClient();
        return _batchStatmentRecordsClient;
      }
    }

    ////
    private BatchInsuredRecoredClient _batchInsuredRecordsClient;
    public BatchInsuredRecoredClient BatchInsuredRecordsClient
    {

      get
      {
        if (_batchInsuredRecordsClient == null || _batchInsuredRecordsClient.State == System.ServiceModel.CommunicationState.Closed)
          _batchInsuredRecordsClient = new BatchInsuredRecoredClient();
        return _batchInsuredRecordsClient;
      }
    }

    private DownloadBatchClient _downloadBatchClient;
    public DownloadBatchClient DownloadBatchClient
    {

      get
      {
        if (_downloadBatchClient == null || _downloadBatchClient.State == System.ServiceModel.CommunicationState.Closed)
          _downloadBatchClient = new DownloadBatchClient();
        return _downloadBatchClient;
      }
    }

    private PayorSourceClient _payorSourceClient;
    public PayorSourceClient PayorSourceClient
    {

      get
      {
        if (_payorSourceClient == null || _payorSourceClient.State == System.ServiceModel.CommunicationState.Closed)
          _payorSourceClient = new PayorSourceClient();
        return _payorSourceClient;
      }
    }

    private ComDeptSupportClient _comDeptClient;
    public ComDeptSupportClient ComDeptSupportClient
    {

      get
      {
        if (_comDeptClient == null || _comDeptClient.State == System.ServiceModel.CommunicationState.Closed)
          _comDeptClient = new ComDeptSupportClient();
        return _comDeptClient;
      }
    }

    private ReportClient _reportClient;
    public ReportClient ReportClient
    {

      get
      {
        if (_reportClient == null || _reportClient.State == System.ServiceModel.CommunicationState.Closed)
          _reportClient = new ReportClient();
        return _reportClient;
      }
    }

    private PayorTemplateClient _payorTemplateClient;
    public PayorTemplateClient PayorTemplateClient
    {

      get
      {
        if (_payorTemplateClient == null || _payorTemplateClient.State == System.ServiceModel.CommunicationState.Closed)
          _payorTemplateClient = new PayorTemplateClient();
        return _payorTemplateClient;
      }
    }

    private PolicyOutgoingDistributionClient _policyOutgoingDistributionClient;
    public PolicyOutgoingDistributionClient PolicyOutgoingDistributionClient
    {
      get
      {
        if (_policyOutgoingDistributionClient == null || _policyOutgoingDistributionClient.State == System.ServiceModel.CommunicationState.Closed)
          _policyOutgoingDistributionClient = new PolicyOutgoingDistributionClient();

        return _policyOutgoingDistributionClient;
      }
    }

    private DisplayPayorClient _DisplayedPayorClient;
    public DisplayPayorClient DisplayedPayorClient
    {
      get
      {
        if (_DisplayedPayorClient == null || _DisplayedPayorClient.State == System.ServiceModel.CommunicationState.Closed)
          _DisplayedPayorClient = new DisplayPayorClient();

        return _DisplayedPayorClient;
      }
    }

    private SettingDisplayPayorClient _SettingDisplayedPayorClient;
    public SettingDisplayPayorClient SettingDisplayedPayorClient
    {
      get
      {
        if (_SettingDisplayedPayorClient == null || _SettingDisplayedPayorClient.State == System.ServiceModel.CommunicationState.Closed)
          _SettingDisplayedPayorClient = new SettingDisplayPayorClient();

        return _SettingDisplayedPayorClient;
      }
    }

    private ConfigDisplayPayorClient _ConfigDisplayedPayorClient;
    public ConfigDisplayPayorClient ConfigDisplayedPayorClient
    {
      get
      {
        if (_ConfigDisplayedPayorClient == null || _ConfigDisplayedPayorClient.State == System.ServiceModel.CommunicationState.Closed)
          _ConfigDisplayedPayorClient = new ConfigDisplayPayorClient();

        return _ConfigDisplayedPayorClient;
      }
    }

    private BrokerCodeClient _BrokerCodeClient;
    public BrokerCodeClient BrokerCodeClient
    {
        get
        {
            if (_BrokerCodeClient == null || _BrokerCodeClient.State == System.ServiceModel.CommunicationState.Closed)
                _BrokerCodeClient = new BrokerCodeClient();

            return _BrokerCodeClient;
        }
    }

    private CompTypeClient _CompTypeClient;
    public CompTypeClient CompTypeClient
    {
        get
        {
            if (_CompTypeClient == null || _CompTypeClient.State == System.ServiceModel.CommunicationState.Closed)
            {
                _CompTypeClient = new CompTypeClient();
            }
            return _CompTypeClient;
        }
    }

    private SettingsClient _SettingsClient;
    public SettingsClient SettingsClient
    {
        get
        {
            if (_SettingsClient == null || _SettingsClient.State == System.ServiceModel.CommunicationState.Closed)
            {
                _SettingsClient = new SettingsClient();
            }
            return _SettingsClient;
        }
    }


        //-------
        //private  PolicyPaymentEntriesPostClient _policyPaymentEntriesPostClient;
        //public  PolicyPaymentEntriesPostClient PolicyPaymentEntriesPostClient
        //{
        //    get
        //    {
        //        if (_policyPaymentEntriesPostClient == null || _policyPaymentEntriesPostClient.State == System.ServiceModel.CommunicationState.Closed)
        //            _policyPaymentEntriesPostClient = new PolicyPaymentEntriesPost();
        //        return _policyPaymentEntriesPostClient;
        //    }


        //}


        public void Dispose()
    {
     
    }
  }
}
