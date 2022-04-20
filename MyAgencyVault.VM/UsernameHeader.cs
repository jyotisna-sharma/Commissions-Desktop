using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Xml;

namespace MyAgencyVault.VM
{
    public class UsernameHeader : MessageHeader
    {
        #region Constants

        /// <summary>
        /// The name of the attribute to which the value of UserName is written
        /// </summary>
        public const string UserAttribute = "userName";
        //public const string UserRoleAttribute = "userRole";
        //public const string AppVersionAttribute = "appVersion";
        /// <summary>
        /// The name of the custom message header.
        /// </summary>
        public const string MessageHeaderName = "UserName";
        //public const string MessageHeaderBody = "UserRole";
        //public const string AppVersion = "AppVersion";

        /// <summary>
        /// The namespace of the custom message header.
        /// </summary>
        public const string MessageHeaderNamespace = "http://wcfheaderbehavior.com/services/username";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logon name of the user from which the request originated
        /// </summary>
        public string UserName { get; private set; } 
        public string UserRole { get; private set; }
        public string AppVersion { get; private set; }



        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="userName">The logon name of the user from which the request originated</param>
        public UsernameHeader(string userName)
        {
            //if (String.IsNullOrEmpty(userName))
            //    throw new ArgumentNullException("userName");
            UserName = userName;
        }
        
        public UsernameHeader(string userID, string userName, string role, string version, string licID)
        {
            //if (String.IsNullOrEmpty(userName))
            //    throw new ArgumentNullException("userName");
          //  UserName = "UserID: " + userID + ", User: " +  userName + ", Role: " + role  + ", Agency: " + licID + ", App Version: " + version;
            //UserRole = role;
            //AppVersion = version;
        }
        #endregion

        /// <summary>
        /// Gets the name of the message header.
        /// </summary>
        public override string Name
        {
            get { return MessageHeaderName; }
        }

        /// <summary>
        /// Gets the namespace of the message header.
        /// </summary>
        public override string Namespace
        {
            get { return MessageHeaderNamespace; }
        }

        /// <summary>
        /// Called when the header content is serialized using the specified XML writer.
        /// </summary>
        /// <param name="writer">An XmlDictionaryWriter.</param>
        /// <param name="messageVersion">Contains information related to the version of SOAP associated with a message and its exchange.</param>
        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteAttributeString(UserAttribute, UserName);
            //writer.WriteAttributeString(UserRoleAttribute, UserRole);
            //writer.WriteAttributeString(AppVersionAttribute, AppVersion);
        }
    }
}
