using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using MyAgencyVault.ViewModel;
using MyAgencyVault.VM;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VMLib;
using System.Windows.Controls;
using System.ComponentModel;
using MyAgencyVault.VMLib.PolicyManager;

namespace MyAgencyVault.ViewModel.VMLib
{
    public class VMInstances
    {
        private static string m_CurrentScreenName;
        public static string CurrentScreenName
        {
            get { return m_CurrentScreenName; }
            set { m_CurrentScreenName = value; }
        }

        private static PayorToolVM m_VMPayorTool;
        public static PayorToolVM PayorToolVM
        {
            get{return m_VMPayorTool;}
            set{m_VMPayorTool = value;}
        }

        private static VMDeu m_DeuVM;
        public static VMDeu DeuVM
        {
            get { return m_DeuVM; }
            set { m_DeuVM = value; }
        }

        private static VMFollowUpManager m_FollowUpVM;
        public static VMFollowUpManager FollowUpVM
        {
            get { return m_FollowUpVM; }
            set { m_FollowUpVM = value; }
        }

        private static VMStatementManager m_DownloadManager;
        public static VMStatementManager DownloadManagerVM
        {
            get { return m_DownloadManager; }
            set { m_DownloadManager = value; }
        }

        private static VMConfigrationManager m_ConfigurationManager;
        public static VMConfigrationManager ConfigurationManager
        {
            get { return m_ConfigurationManager; }
            set 
            { 
                m_ConfigurationManager = value;
                if (setVMObject != null)
                    setVMObject(m_ConfigurationManager);
            }
        }

        private static SettingsManagerVM m_SettingManager;
        public static SettingsManagerVM SettingManager
        {
            get { return m_SettingManager; }
            set { m_SettingManager = value; }
        }

        private static PolicyManagerVm m_PolicyManager;
        public static PolicyManagerVm PolicyManager
        {
            get { return m_PolicyManager; }
            set { m_PolicyManager = value; }
        }

        private static VMOptimizePolicyManager m_OptimizedPolicyManager;
        public static VMOptimizePolicyManager OptimizedPolicyManager
        {
            get { return m_OptimizedPolicyManager; }
            set { m_OptimizedPolicyManager = value; }
        }

        private static VMPolicyCommission m_PolicyCommissionVM;
        public static VMPolicyCommission PolicyCommissionVM
        {
            get { return m_PolicyCommissionVM; }
            set { m_PolicyCommissionVM = value; }
        }

        private static VMPolicySchedule m_PolicyScheduleVM;
        public static VMPolicySchedule PolicyScheduleVM
        {
            get { return m_PolicyScheduleVM; }
            set { m_PolicyScheduleVM = value; }
        }

        private static VMPolicySmartField m_PolicySmartFieldVM;
        public static VMPolicySmartField PolicySmartFieldVM
        {
            get { return m_PolicySmartFieldVM; }
            set { m_PolicySmartFieldVM = value; }
        }

        private static VMPolicyNote m_PolicyNoteVM;
        public static VMPolicyNote PolicyNoteVM
        {
            get { return m_PolicyNoteVM; }
            set { m_PolicyNoteVM = value; }
        }

        private static PeopleManagerVM m_PeopleManager;
        public static PeopleManagerVM PeopleManager
        {
            get { return m_PeopleManager; }
            set { m_PeopleManager = value; }
        }

        private static VMCompManager m_CompManager;
        public static VMCompManager CompManager
        {
            get { return m_CompManager; }
            set { m_CompManager = value; }
        }

        private static VmBillingManager m_BillingManager;
        public static VmBillingManager BillingManager
        {
            get { return m_BillingManager; }
            set { m_BillingManager = value; }
        }

        private static VMRepManager m_RepManager;
        public static VMRepManager RepManager
        {
            get { return m_RepManager; }
            set { m_RepManager = value; }
        }

        private static VMLoginUser m_Login;
        public static VMLoginUser Login
        {
            get { return m_Login; }
            set { m_Login = value; }
        }

        private static MainWindowVM m_vmMainWindow;
        public static MainWindowVM vmMainWindow
        {
            get
            {
                return m_vmMainWindow;
            }
            set
            {
                m_vmMainWindow = value;
            }
        }
        public delegate void ConfigVMSetDelegate(VMConfigrationManager vmObject);
        public static event ConfigVMSetDelegate setVMObject;
    }
}
