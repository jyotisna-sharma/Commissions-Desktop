using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace MyAgencyVault.VM
{
    public class UsernameClientBehavior: BehaviorExtensionElement, IEndpointBehavior, IClientMessageInspector
    {
        /// <summary>
        /// Gets the current username
        /// </summary>
        private string UserName
        {
            get
            {
                // TODO: Get the username from the thread
                // For testing purposes, we fill it with a test value
                return "TestUser";
            }
        }

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>
        /// The behavior extension.
        /// </returns>
        protected override object CreateBehavior()
        {
            return this;
        }

        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Type"/>.
        /// </returns>
        public override Type BehaviorType
        {
            get { return GetType(); }
        }

        /// <summary>
        /// Implement to confirm that the endpoint meets some intended criteria.
        /// </summary>
        /// <param name="endpoint">The endpoint to validate.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        /// Implement to pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="endpoint">The endpoint to modify.</param><param name="bindingParameters">The objects that binding elements require to support the behavior.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the service across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that exposes the contract.</param><param name="endpointDispatcher">The endpoint dispatcher to be modified or extended.</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        /// <summary>
        /// Implements a modification or extension of the client across an endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint that is to be customized.</param><param name="clientRuntime">The client runtime to be customized.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(this);
        }

        /// <summary>
        /// Enables inspection or modification of a message before a request message is sent to a service.
        /// </summary>
        /// <returns>
        /// The object that is returned as the <paramref name="correlationState "/>argument of the <see cref="M:System.ServiceModel.Dispatcher.IClientMessageInspector.AfterReceiveReply(System.ServiceModel.Channels.Message@,System.Object)"/> method. This is null if no correlation state is used.The best practice is to make this a <see cref="T:System.Guid"/> to ensure that no two <paramref name="correlationState"/> objects are the same.
        /// </returns>
        /// <param name="request">The message to be sent to the service.</param><param name="channel">The WCF client object channel.</param>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            string action = (request.Headers.Action != null) ? System.IO.Path.GetFileName(request.Headers.Action).ToLower() : "";
          //  if (request.Headers != null && request.Headers.Action != null && (request.Headers.Action.ToLower().Contains("add") || request.Headers.Action.ToLower().Contains("update") || request.Headers.Action.ToLower().Contains("delete") || request.Headers.Action.ToLower().Contains("remove") || request.Headers.Action.ToLower().Contains("unlink") || request.Headers.Action.ToLower().Contains("post")))
            if (!string.IsNullOrEmpty(action) && action != "addlog" && (action.Contains("add") || action.Contains("update") || action.Contains("delete") || action.Contains("remove") || action.Contains("link") || action.Contains("post") || action.Contains("save"))) //(!action.Contains("get") && (!action.Contains("load")))) //
            {
                if (!string.IsNullOrEmpty(MyAgencyVault.ViewModel.CommonItems.RoleManager.LoggedInUser))
                {
                    var header = new UsernameHeader(Convert.ToString(MyAgencyVault.ViewModel.CommonItems.RoleManager.userCredentialID));//, MyAgencyVault.ViewModel.CommonItems.RoleManager.LoggedInUser, MyAgencyVault.ViewModel.CommonItems.RoleManager.Role.ToString(), System.Configuration.ConfigurationSettings.AppSettings["DisplayVersion"].ToString(), Convert.ToString(MyAgencyVault.ViewModel.CommonItems.RoleManager.LicenseeId));
                    request.Headers.Add(header);
                }
            }
            return null;
        }

        /// <summary>
        /// Enables inspection or modification of a message after a reply message is received but prior to passing it back to the client application.
        /// </summary>
        /// <param name="reply">The message to be transformed into types and handed back to the client application.</param><param name="correlationState">Correlation state data.</param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }
    }
}