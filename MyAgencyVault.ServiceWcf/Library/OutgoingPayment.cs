using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary;
using System.ServiceModel;

namespace MyAgencyVault.WcfService
{
    [ServiceContract]
    interface IOutgoingPayment
    {
        [OperationContract]
        void AddUpdateOutgoingPayment(List<OutGoingPayment> GisgoingSchedule, bool IsCustomSchedule = false, bool IsTieredSchedule = false);
        
        [OperationContract]
        void DeleteOutgoingPayment(OutGoingPayment OutGoingPymnt);
        
        [OperationContract]
        List<OutGoingPayment> GetOutgoingPayments();

        [OperationContract]
        List<OutGoingPayment> GetOutgoingSheduleForPolicy(Guid PolicyId);

        [OperationContract]
        bool CheckIsPaymentFromDEUForOutgoingPaymentID(Guid OutgoingPaymentid);

        [OperationContract]
        PolicyOutgoingDistribution GetOutgoingPaymentByID(Guid OutgoingPaymentid);
    }

    public partial class MavService : IOutgoingPayment
    {
        public void AddUpdateOutgoingPayment(List<OutGoingPayment> GisgoingSchedule, bool IsCustomSchedule=false, bool IsTieredSchedule = false)
        {
            try
            {
                if (GisgoingSchedule != null)
                {
                    ActionLogger.Logger.WriteLog("AddUpdateOutgoingPayment, new schedule: " + GisgoingSchedule.ToStringDump(), true);
                }
            }
            catch(Exception ex)
            {
                ActionLogger.Logger.WriteLog("AddUpdateOutgoingPayment request log not written: " + ex.Message, true);
            }
           
            OutGoingPayment.AddUpdate(GisgoingSchedule, IsCustomSchedule,IsTieredSchedule);
        }

        public void DeleteOutgoingPayment(OutGoingPayment OutGoingPymnt)
        {
            try
            {
                if (OutGoingPymnt != null)
                {
                    ActionLogger.Logger.WriteLog("DeleteOutgoingPayment schedule: " + OutGoingPymnt.ToStringDump(), true);
                }
            }
            catch (Exception ex)
            {
                ActionLogger.Logger.WriteLog("DeleteOutgoingPayment request log not written: " + ex.Message, true);
            }
            OutGoingPymnt.Delete();
        }

        public List<OutGoingPayment> GetOutgoingPayments()
        {
            return OutGoingPayment.GetOutgoingShedule();
        }

        public List<OutGoingPayment> GetOutgoingSheduleForPolicy(Guid PolicyId)
        {
            List <OutGoingPayment> lstPayments = OutGoingPayment.GetOutgoingSheduleForPolicy(PolicyId);
            if(lstPayments != null)
            {
                try
                {
                    ActionLogger.Logger.WriteLog("GetOutgoingSheduleForPolicy PolicyID: " + PolicyId + ", Schedule: " + lstPayments.ToStringDump(), true);
                }
                catch(Exception ex)
                {
                    ActionLogger.Logger.WriteLog("GetOutgoingSheduleForPolicynot error for  PolicyID: " + PolicyId + ", Exception: " + ex.Message, true);
                }
            }
            return lstPayments;
        }

        public bool CheckIsPaymentFromDEUForOutgoingPaymentID(Guid OutgoingPaymentid)
        {
            return PolicyOutgoingDistribution.CheckIsPaymentFromDEU(OutgoingPaymentid);
        }

        public PolicyOutgoingDistribution GetOutgoingPaymentByID(Guid OutgoingPaymentid)
        {
            return PolicyOutgoingDistribution.GetOutgoingPaymentById(OutgoingPaymentid);
        }
    }
}
