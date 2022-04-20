using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using MyAgencyVault.WinApp.UserControls;
using MyAgencyVault.ViewModel;
using MyAgencyVault.VM;
using MyAgencyVault.ViewModel.CommonItems;
using MyAgencyVault.WinApp.Common;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.VMLib;

namespace MyAgencyVault.WinApp
{
    public class VMUserControl
    {
      //Comment to test the Merging
        private static PayorToolManager m_UCPayorToolManager;
               
        public static PayorToolManager getPayorToolUserControl()
        {
            if (m_UCPayorToolManager == null)
                m_UCPayorToolManager = new PayorToolManager();
            return m_UCPayorToolManager;
        }

        private static DataEntryUnit m_UCDEUManager;
        public static DataEntryUnit getDEUUserControl()
        {
            if (m_UCDEUManager == null)
                m_UCDEUManager = new DataEntryUnit();
            return m_UCDEUManager;
        }

        private static FollowUpManager m_UCFollowUpManager;
        public static FollowUpManager getFollowUpUserControl()
        {
            if (m_UCFollowUpManager == null)
                m_UCFollowUpManager = new FollowUpManager();
            return m_UCFollowUpManager;
        }

        private static DownloadManager m_UCDownloadManager;
        public static DownloadManager getDownloadUserControl()
        {
            if (m_UCDownloadManager == null)
                m_UCDownloadManager = new DownloadManager();
            return m_UCDownloadManager;
        }

        private static ConfigurationManager m_UCConfigurationManager;
        public static ConfigurationManager getConfigurationManagerUserControl()
        {
            if (m_UCConfigurationManager == null)
                m_UCConfigurationManager = new ConfigurationManager();
            return m_UCConfigurationManager;
        }

        private static Settings m_UCSettingManager;
        public static Settings getSettingManagerUserControl()
        {
            if (m_UCSettingManager == null)
                m_UCSettingManager = new Settings();
            return m_UCSettingManager;
        }

        private static PolicyManager m_UCPolicyManager;
        public static PolicyManager getPolicyManagerUserControl()
        {
            if (m_UCPolicyManager == null)
                m_UCPolicyManager = new PolicyManager();
            return m_UCPolicyManager;
        }

        private static PolicyManagerOptimized m_OptimizedPolicyManager;
        public static PolicyManagerOptimized getOptimizedPolicyManagerUserControl()
        {
            if (m_OptimizedPolicyManager == null)
                m_OptimizedPolicyManager = new PolicyManagerOptimized();
            return m_OptimizedPolicyManager;
        }

        private static PeopleManager m_UCPeopleManager;
        public static PeopleManager getPeopleManagerUserControl()
        {
            if (m_UCPeopleManager == null)
                m_UCPeopleManager = new PeopleManager();
            return m_UCPeopleManager;
        }

        private static ucCompManager m_UCCompManager;
        public static ucCompManager getCompManagerUserControl()
        {
            if (m_UCCompManager == null)
                m_UCCompManager = new ucCompManager();
            return m_UCCompManager;
        }

        private static BillingManager m_UCBillingManager;
        public static BillingManager getBillingManagerUserControl()
        {
            if (m_UCBillingManager == null)
                m_UCBillingManager = new BillingManager();
            return m_UCBillingManager;
        }

        private static RepManager m_UCRepManager;
        public static RepManager getRepManagerUserControl()
        {
            if (m_UCRepManager == null)
                m_UCRepManager = new RepManager();
            return m_UCRepManager;
        }

        private static CreateClient m_CreateClient;
        public static CreateClient CreateClient
        {
            get { return m_CreateClient; }
            set { m_CreateClient = value; }
        }
     
    }
}
