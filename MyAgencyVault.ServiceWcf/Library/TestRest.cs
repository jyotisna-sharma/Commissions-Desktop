﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary;
using System.ServiceModel;
using DLinq = DataAccessLayer.LinqtoEntity;
using System.ServiceModel.Web;
using System.ServiceModel.Dispatcher;
using System.Runtime.Serialization;
using System.Data;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data.EntityClient;

namespace MyAgencyVault.WcfService
{
    [ServiceContract]
    interface ITestRest
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json,  BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string GetDataRest(string s);

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json,  BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        JSONResponse ImportPolicyService(string PolicyTable, Guid LicenseeID);

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ListResponse GetPayorCarrierList(string StartDate, string EndDate);
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        AddUserResponse SaveBenefitUserInfo(string UserDetails, Guid LicenseeID);

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        PayorResponse GetPayorsList(string StartDate, string EndDate, List<string> PayorData=null);
    }
    public enum ResponseCodes
    {
        RecordNotFound = 404,
        Created = 201,
        Accepted = 202,
        Unauthorized = 401,
        Success = 200,
        Fail = 210,
        Forbidden = 403,
        InternalError = 500,
        Notimplemented = 501

    };

    public partial class MavService : ITestRest, IErrorHandler
    {
        public string GetDataRest(string s)
        {
            return string.Format("You entered: {0}", s);
        }

        public PayorResponse GetPayorsList(string StartDate, string EndDate, List<string> PayorData = null)
        {
            PayorResponse res = null;
            try
            {
                if (WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"] != null)
                {
                    string val = Convert.ToString(WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"]);
                    ActionLogger.Logger.WriteLog("GetPayorsList - header key:  " + val, true);
                    if (val != "CommDept1973")
                    {
                        res = new PayorResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                        ActionLogger.Logger.WriteLog("GetPayorsList - Authentication failure ", true);
                        return res;
                    }
                }
                else
                {
                    res = new PayorResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                    ActionLogger.Logger.WriteLog("GetPayorsList - Authentication failure ", true);
                    return res;
                }
            }
            catch (Exception ex)
            {
                res = new PayorResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                ActionLogger.Logger.WriteLog("GetPayorsList - Authentication failure , no key in header", true);
                return res;
            }

            try
            {
                //if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
                //{
                DateTime dtStart = DateTime.MinValue;
                DateTime dtEnd = DateTime.MinValue;
                DateTime.TryParse(StartDate, out dtStart);
                ActionLogger.Logger.WriteLog("GetPayorsList - Start date: " + dtStart, true);
                DateTime.TryParse(EndDate, out dtEnd);
                ActionLogger.Logger.WriteLog("GetPayorsList - EndDate:  " + dtEnd, true);
                ActionLogger.Logger.WriteLog("GetPayorsList - PayorData:  " + PayorData, true);

                if ((dtStart != DateTime.MinValue && dtEnd != DateTime.MinValue) ||
                    (dtStart == DateTime.MinValue && dtEnd == DateTime.MinValue))
                {
                   
                    //if (string.IsNullOrEmpty(PayorData))
                    if(PayorData == null)
                    {
                        List<PayorObject> objPayr = BusinessLibrary.Payor.GetPayorsOnDate(dtStart, dtEnd);
                        res = new PayorResponse("Request processed successfully!", Convert.ToInt16(200), "");
                        res.PayorList = objPayr;
                    }
                    else
                    {
                        List<string> invalidPayorsIfAny = new List<string>();
                        List<PayorCarrierObject> listCarriers = BusinessLibrary.Carrier.GetCarriersOnPayor(dtStart, dtEnd, PayorData, out invalidPayorsIfAny);
                        res = new PayorResponse("Request processed successfully!", Convert.ToInt16(200), "");
                        res.CarrierList = listCarriers;
                        res.InvalidPayors = invalidPayorsIfAny;
                    }

                    return res;
                }
                else
                {
                    res = new PayorResponse("Exception in returning list: Please check incoming request", Convert.ToInt16(210), "Invalid request!");
                    ActionLogger.Logger.WriteLog("GetPayorCarrierList - invalid request: ", true);
                    return res;
                }
                
            }
            catch (Exception ex)
            {
                res = new PayorResponse("Exception in returning list: " + ex.Message, Convert.ToInt16(210), "Exception getting data!");
                ActionLogger.Logger.WriteLog("GetPayorCarrierList - exception: " + ex.Message, true);
                return res;
            }

        }
        public ListResponse GetPayorCarrierList(string StartDate, string EndDate)
        {
            ListResponse res = null;
            try
            {
                if (WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"] != null)
                {
                    string val = Convert.ToString(WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"]);
                    ActionLogger.Logger.WriteLog("GetPayorCarrierList - header key:  " + val, true);
                    if (val != "CommDept1973")
                    {
                        res = new ListResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                        ActionLogger.Logger.WriteLog("GetPayorCarrierList - Authentication failure ", true);
                        return res;
                    }
                }
                else
                {
                    res = new ListResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                    ActionLogger.Logger.WriteLog("GetPayorCarrierList - Authentication failure ", true);
                    return res;
                }
            }
            catch (Exception ex)
            {
                res = new ListResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                ActionLogger.Logger.WriteLog("GetPayorCarrierList - Authentication failure , no key in header", true);
                return res;
            }

            try
            {
                //if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
                //{
                DateTime dtStart = DateTime.MinValue;
                DateTime dtEnd = DateTime.MinValue;
                DateTime.TryParse(StartDate, out dtStart);
                ActionLogger.Logger.WriteLog("GetPayorCarrierList - Start date: " + dtStart, true);
                DateTime.TryParse(EndDate, out dtEnd);
                ActionLogger.Logger.WriteLog("GetPayorCarrierList - EndDate:  " + dtEnd, true);

                if ((dtStart != DateTime.MinValue && dtEnd != DateTime.MinValue) ||
                    (dtStart == DateTime.MinValue && dtEnd == DateTime.MinValue))
                {
                    List<CarrierObject> objCarr = BusinessLibrary.Carrier.GetCarriersOnDate(dtStart, dtEnd);
                    List<PayorObject> objPayr = BusinessLibrary.Payor.GetPayorsOnDate(dtStart, dtEnd);
                    res = new ListResponse("Request processed successfully!", Convert.ToInt16(200), "");
                    res.CarrierList = objCarr;
                    res.PayorList = objPayr;
                    return res;
                }
                else
                {
                    res = new ListResponse("Exception in returning list: Please check incoming request", Convert.ToInt16(210), "Invalid request!");
                    ActionLogger.Logger.WriteLog("GetPayorCarrierList - invalid request: ", true);
                    return res;
                }
                //}
            }
            catch (Exception ex)
            {
                res = new ListResponse("Exception in returning list: " + ex.Message, Convert.ToInt16(210), "Exception getting data!");
                ActionLogger.Logger.WriteLog("GetPayorCarrierList - exception: " + ex.Message, true);
                return res;
            }
        }

        public JSONResponse ImportPolicyService(string strExcel, Guid LicenseeID/*, string uniqueKey*/)
        {
           
            JSONResponse jres = null;
            //Read header and return if not present
            try
            {
                if (WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"] != null)
                {
                    string val = Convert.ToString(WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"]);
                    ActionLogger.Logger.WriteImportPolicyLog("Import policy - header key:  " + val, true);
                    if (val != "CommDept1973")
                    {
                        jres = new JSONResponse("Import process cannot be started as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                        ActionLogger.Logger.WriteImportPolicyLog("Import policy - Authentication failure ", true);
                        return jres;
                    }
                }
                else
                {
                    jres = new JSONResponse("Import process cannot be started as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                    ActionLogger.Logger.WriteImportPolicyLog("Import policy - Authentication failure ", true);
                    return jres;
                }
            }
            catch (Exception ex)
            {
                jres = new JSONResponse("Import process cannot be started as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                ActionLogger.Logger.WriteImportPolicyLog("Import policy - Authentication failure , no key in header", true);
                return jres;
            }

            try
            {
                string inRequest = "Incoming table: " + strExcel + ", LicenseeID: " + LicenseeID;
                ActionLogger.Logger.WriteImportPolicyLog(inRequest, true);
                strExcel = strExcel.Replace("'", "");

                //Following added to handle blank values in Outgoing schedule array
                //As found in R&D that blank array in json cannot be deserialized as data table
                strExcel = strExcel.Replace("[]", "[{}]");


                //if (!string.IsNullOrEmpty(uniqueKey) && uniqueKey.Trim() == "CommDept1973")
                {
                    List<CompType> lstComp = (new CompType()).GetAllComptype();
                    ObservableCollection<CompType> compList = new ObservableCollection<CompType>(lstComp);

                    DataTable tbExcel = (DataTable)Newtonsoft.Json.JsonConvert.DeserializeObject(strExcel, (typeof(DataTable)));
                    //   MailServerDetail.sendMail("jyotisna@acmeminds.com", "Import process started by benefits at " + DateTime.Now.ToString(), inRequest);

                    if (Convert.ToString(LicenseeID).ToUpper() == "A3E3FCEE-05B5-4D3A-9D62-E113AED72946")
                    {
                        ActionLogger.Logger.WriteImportPolicyLog("Caravus request ", true);
                        Benefits_PolicyImportStatus status = Policy.ImportPolicy_Benefits_Caravus(tbExcel, LicenseeID, compList);
                        jres = new JSONResponse(string.Format("Import process execution completed"), Convert.ToInt16(200), "");
                        jres.ImportStatus = status;
                    }
                    else
                    {
                        ActionLogger.Logger.WriteImportPolicyLog("Not a Caravus request ", true);
                        Benefits_PolicyImportStatus status = Policy.ImportPolicy_Benefits(tbExcel, LicenseeID, compList);
                        jres = new JSONResponse(string.Format("Import process execution completed"), Convert.ToInt16(200), "");
                        jres.ImportStatus = status;
                    }
                    ActionLogger.Logger.WriteImportPolicyLog("Import Process Execution completed ", true);
                }
               
             
            }
            catch (Exception ex)
            {
                jres = new JSONResponse("Import process execution failed", Convert.ToInt16(210), ex.Message);
                ActionLogger.Logger.WriteImportPolicyLog("Import process execution  failure: " + ex.Message,true);
            }
            return jres;
         
        }

        /// <summary>
        /// Author:Ankit khandelwal
        /// CreateOn:Sept 05,2018
        /// Purpose:Save user for Benefit Pro. with BGUserID
        /// </summary>
        /// <param name="userDetails"></param>
        /// <param name="LicenseeID"></param>
        /// <returns></returns>
        public AddUserResponse SaveBenefitUserInfo(string userDetails, Guid LicenseeID)
        {
            PolicyImportStatus objStatus = new PolicyImportStatus();
            int addCount = 0;
            int updateCount = 0;
            int errorCount = 0;
            List<Benefits_UserMsg> errorlist = new List<Benefits_UserMsg>();

            AddUserResponse jres = null;
            try
            {
                string inRequest = "Incoming  save user Request: " + userDetails + ", LicenseeID: " + LicenseeID;
                ActionLogger.Logger.WriteLog(inRequest, true);
                //strExcel = strExcel.Replace("'", "");
                if (WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"] != null)
                {
                    string val = Convert.ToString(WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"]);
                    ActionLogger.Logger.WriteLog("Save Agent- header key:  " + val, true);
                    if (val != "CommDept1973")
                    {
                        jres = new AddUserResponse("Agent cannot be added because incoming request is not valid", ResponseCodes.Fail, "Unauthorized request");
                        ActionLogger.Logger.WriteLog("Save Agent - Authentication failure ", true);
                        return jres;
                    }
                }
                else
                {
                    jres = new AddUserResponse("Agent cannot be added because incoming request is not valid", ResponseCodes.Fail, "Unauthorized request");
                    ActionLogger.Logger.WriteLog("Save Agent - Authentication failure ", true);
                    return jres;
                }
            }
            catch (Exception ex)
            {
                jres = new AddUserResponse("Agent cannot be added because incoming request is not valid", ResponseCodes.Fail, ex.Message);
                ActionLogger.Logger.WriteLog(ex.Message, true);
                return jres;
            }
            try
            {
                DataTable tbExcel = (DataTable)Newtonsoft.Json.JsonConvert.DeserializeObject(userDetails, (typeof(DataTable)));
                //int update = 1;
                //int new = 1;
                for (int i = 0; i < tbExcel.Rows.Count; i++)
                {
                    User GetUserInfo = new User();
                    try
                    {
                        GetUserInfo.FirstName = Convert.ToString(tbExcel.Rows[i]["FirstName"]);
                        GetUserInfo.LastName = Convert.ToString(tbExcel.Rows[i]["LastName"]);
                        GetUserInfo.BGUserId = Convert.ToString(tbExcel.Rows[i]["BGUserId"]);
                        GetUserInfo.CellPhone = Convert.ToString(tbExcel.Rows[i]["CellPhone"]);
                        GetUserInfo.UserName = GetUserInfo.FirstName + GetUserInfo.LastName.Substring(0, 1);
                        GetUserInfo.Password = "test123";
                        GetUserInfo.PasswordHintQ = "First school";
                        GetUserInfo.PasswordHintA = "school";
                        GetUserInfo.Email = Convert.ToString(tbExcel.Rows[i]["Email"]);
                        GetUserInfo.Permissions = new List<UserPermissions>(7);
                        GetUserInfo.LicenseeId = LicenseeID;
                        GetUserInfo.CreateOn = DateTime.Now;
                        GetUserInfo.Role = (UserRole)3;
                        DLinq.CommissionDepartmentEntities ctx = new DLinq.CommissionDepartmentEntities(); //create your entity object here
                        EntityConnection ec = (EntityConnection)ctx.Connection;
                        SqlConnection sc = (SqlConnection)ec.StoreConnection; //get the SQLConnection that your entity object would use
                        string adoConnStr = sc.ConnectionString;

                        using (SqlConnection con = new SqlConnection(adoConnStr))
                        {

                            using (SqlCommand cmd = new SqlCommand("spCheckUsername", con))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@FirstName", GetUserInfo.FirstName);
                                cmd.Parameters.AddWithValue("@LastName", GetUserInfo.LastName);
                                cmd.Parameters.Add("@IsExists", SqlDbType.VarChar, 30);
                                cmd.Parameters["@IsExists"].Direction = ParameterDirection.Output;
                                cmd.Parameters.Add("@IsUsername", SqlDbType.VarChar, 500);
                                cmd.Parameters["@IsUsername"].Direction = ParameterDirection.Output;
                                cmd.Parameters.Add("@IsUserNameValid", SqlDbType.VarChar, 500);
                                cmd.Parameters["@IsUserNameValid"].Direction = ParameterDirection.Output;
                                con.Open();
                                cmd.ExecuteNonQuery();
                                var abc = cmd.Parameters["@IsExists"].Value.ToString();
                                GetUserInfo.UserName = cmd.Parameters["@IsUsername"].Value.ToString();

                            }

                        }
                        GetUserInfo.NickName = GetUserInfo.UserName;
                        var IsCheckUserNameExist = IsUserNameExist(GetUserInfo.UserCredentialID, GetUserInfo.UserName);

                        if (IsCheckUserNameExist == true)
                        {
                            ActionLogger.Logger.WriteLog(" Save Agent - User already exist", true);
                            GetUserInfo.UserName = GetUserInfo.FirstName + GetUserInfo.LastName.Substring(0, 1) + "_1";
                            jres = new AddUserResponse("Agent Cant be added because User already exist", ResponseCodes.Fail, "Unauthorized request");
                            ActionLogger.Logger.WriteLog(" Save Agent - Agent add successfully  with different username like 'XYZ123', User already exist", true);
                            GetUserInfo.NickName = GetUserInfo.UserName;
                        }
                        if (!string.IsNullOrEmpty(GetUserInfo.LastName) && !string.IsNullOrEmpty(GetUserInfo.BGUserId))
                        {

                            ActionLogger.Logger.WriteLog("save user with BGUserId: " + GetUserInfo.BGUserId + ", LicenseeID: " + LicenseeID + ",FirstName:" + GetUserInfo.FirstName, true);
                            GetUserInfo.AddUpdateBGUser(out string type, out string message);

                            if (type == "0")
                            {
                                updateCount++;
                            }
                            if (type == "1")
                            {
                                addCount++;
                            }
                            if (type == "2")
                            {
                                errorCount++;
                                Benefits_UserMsg test = new Benefits_UserMsg();
                                test.BGUserId = GetUserInfo.BGUserId;
                                test.Message = message;
                                errorlist.Add(test);
                            }

                            ActionLogger.Logger.WriteLog("Save Agent - Agent added successfully", true);
                        }
                        else
                        {
                            string message = "";
                            if (string.IsNullOrEmpty(GetUserInfo.LastName))
                            {
                                message = "LastName can't be blank";
                                ActionLogger.Logger.WriteLog("user can't save  with Blank lastName: " + GetUserInfo.BGUserId + ", LicenseeID: " + LicenseeID + ",FirstName:" + GetUserInfo.FirstName, true);
                            }
                            if (string.IsNullOrEmpty(GetUserInfo.BGUserId))
                            {
                                message = "BGUserId can't be blank";
                                ActionLogger.Logger.WriteLog("user can't save  with Blank BGUSerId: " + GetUserInfo.BGUserId + ", LicenseeID: " + LicenseeID + ",FirstName:" + GetUserInfo.FirstName, true);
                            }

                            errorCount++;
                            jres = new AddUserResponse(string.Format("Agent can't be added in cd"), ResponseCodes.Fail, "");
                            jres.ImportUsersStatus = new Benefits_UserResponseStatus();
                            Benefits_UserMsg test = new Benefits_UserMsg();
                            test.BGUserId = GetUserInfo.BGUserId;
                            test.Message = message;
                            errorlist.Add(test);

                        }
                    }
                    catch (Exception ex)
                    {
                        ActionLogger.Logger.WriteLog(ex.Message, true);
                        // throw ex;
                        errorCount++;
                        jres = new AddUserResponse(string.Format("Agent can't be added in cd"), ResponseCodes.Fail, "");
                        jres.ImportUsersStatus = new Benefits_UserResponseStatus();
                        Benefits_UserMsg test = new Benefits_UserMsg();
                        test.BGUserId = GetUserInfo.BGUserId;
                        test.Message = ex.Message;
                        errorlist.Add(test);
                        continue;
                    }

                }
                jres = new AddUserResponse(string.Format("Import process execution completed"), ResponseCodes.Success, "");
                ActionLogger.Logger.WriteLog("Import process execution completed: ", true);
                jres.ImportUsersStatus = new Benefits_UserResponseStatus();
                jres.ImportUsersStatus.NewCount = addCount;
                jres.ImportUsersStatus.ErrorCount = errorCount;
                jres.ImportUsersStatus.UpdateCount = updateCount;
                jres.ImportUsersStatus.ErrorUserList = errorlist;
                ActionLogger.Logger.WriteLog("Import process execution completed: " + jres.ToStringDump(), true);
                return jres;
            }
            catch (Exception ex)
            {

                jres = new AddUserResponse("Agent Cant be added", ResponseCodes.Fail, ex.Message);
                ActionLogger.Logger.WriteLog(" Save Benefit User request fails: " + ex.Message, true);
                errorCount++;
                ActionLogger.Logger.WriteLog(ex.Message, true);
                jres.ImportUsersStatus = new Benefits_UserResponseStatus();
                jres.ImportUsersStatus.ErrorCount = errorCount;
                Benefits_UserMsg test = new Benefits_UserMsg();
                test.Message = ex.Message;
                errorlist.Add(test);
                jres.ImportUsersStatus.ErrorUserList = errorlist;
                return jres;
            }

        }

    }

}
    [DataContract]
    public class JSONResponse
    {
        private string _message;
        private int _errorCode;
        private string _exceptionMessage;
        Benefits_PolicyImportStatus _importStatus;

        public JSONResponse()
        {
            //Empty parameter constructor;
        }
        public JSONResponse(string message, int errorCode, string exceptionMessage)
        {
            this.Message = message;
            this.ResponseCode = errorCode;
            this.ExceptionMessage = exceptionMessage;
            // this.SecondResult = secondResult;
        }
        [DataMember]
        public Benefits_PolicyImportStatus ImportStatus
        {
            get { return _importStatus; }
            set { _importStatus = value; }
        }
        [DataMember]
        public int ResponseCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        [DataMember]
        public string ExceptionMessage
        {
            get { return _exceptionMessage; }
            set { _exceptionMessage = value; }
        }
    }

    [DataContract]
    public class ListResponse
    {
        private string _message;
        private int _errorCode;
        private string _exceptionMessage;
        List<PayorObject> _listPayors;
        List<CarrierObject> _listCarriers;

        [DataMember]
        public int ResponseCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        [DataMember]
        public string ExceptionMessage
        {
            get { return _exceptionMessage; }
            set { _exceptionMessage = value; }
        }

        [DataMember]
        public List<PayorObject> PayorList
        {
            get { return _listPayors; }
            set { _listPayors = value; }
        }

        [DataMember]
        public List<CarrierObject> CarrierList
        {
            get { return _listCarriers; }
            set { _listCarriers = value; }
        }


        public ListResponse()
        {
            //Empty parameter constructor;
        }
        public ListResponse(string message, int errorCode, string exceptionMessage)
        {
            this.Message = message;
            this.ResponseCode = errorCode;
            this.ExceptionMessage = exceptionMessage;
            // this.SecondResult = secondResult;
        }
    }
    [DataContract]
    public class AddUserResponse
    {
        private string _message;
        private int _errorCode;
        private string _exceptionMessage;
        Benefits_UserResponseStatus _AddingUsersStatus;
        public AddUserResponse()
        {
            //Empty parameter constructor;
        }
        public AddUserResponse(string message, Enum errorCode, string exceptionMessage)
        {
            this.Message = message;
            this.ResponseCode = Convert.ToInt32(errorCode);
            this.ExceptionMessage = exceptionMessage;
            // this.SecondResult = secondResult;
        }
        [DataMember]
        public int ResponseCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        [DataMember]
        public string ExceptionMessage
        {
            get { return _exceptionMessage; }
            set { _exceptionMessage = value; }
        }

        [DataMember]
        public Benefits_UserResponseStatus ImportUsersStatus
        {
            get { return _AddingUsersStatus; }
            set { _AddingUsersStatus = value; }
        }
    }

[DataContract]
public class PayorResponse
{
    private string _message;
    private int _errorCode;
    private string _exceptionMessage;
    List<PayorObject> _listPayors;
    List<PayorCarrierObject> _listCarriers;
    List<string> _invalidPayors; 

    [DataMember]
    public int ResponseCode
    {
        get { return _errorCode; }
        set { _errorCode = value; }
    }

    [DataMember]
    public string Message
    {
        get { return _message; }
        set { _message = value; }
    }
    [DataMember]
    public string ExceptionMessage
    {
        get { return _exceptionMessage; }
        set { _exceptionMessage = value; }
    }

    [DataMember]
    public List<PayorObject> PayorList
    {
        get { return _listPayors; }
        set { _listPayors = value; }
    }

    [DataMember]
    public List<PayorCarrierObject> CarrierList
    {
        get { return _listCarriers; }
        set { _listCarriers = value; }
    }

    [DataMember]
    public List<string> InvalidPayors
    {
        get { return _invalidPayors; }
        set { _invalidPayors = value; }
    }

    public PayorResponse()
    {
        //Empty parameter constructor;
    }
    public PayorResponse(string message, int errorCode, string exceptionMessage)
    {
        this.Message = message;
        this.ResponseCode = errorCode;
        this.ExceptionMessage = exceptionMessage;
        // this.SecondResult = secondResult;
    }
}

