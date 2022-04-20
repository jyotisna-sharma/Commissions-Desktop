#define DEBUG
using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using MyAgencyVault.BusinessLibrary;
using MyAgencyVault.FollowUpProcess;
using System.Diagnostics;
using System.Threading;
using MyAgencyVault.BusinessLibrary.Masters;

namespace MyAgencyVault.FollowUpProcess
{
    public class FollowUpStatus
    {
        public Guid PolicyId;
        public bool IsComplete = false;
        public Exception ex;
        public string Actions = "";
        public bool IsTracked = false;
    }

    public class ConstantTrm
    {
        public const string ModeNull = "--Mode is Null--";

    }

    public class FollowUpService
    {
        public static void FollowUpProc()
        {
            string strServiceValue = SystemConstant.GetKeyValue("FollowUpService");

            if (strServiceValue == "Stop")
            {
                return;
            }

            double DaysCnt = Convert.ToDouble(SystemConstant.GetKeyValue(MasterSystemConst.NextFollowUpRunDaysCount.ToString()));
            if (DaysCnt == 0)
            {
                return;
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            System.Console.WriteLine("Please wait ..fetching policy from database");

            List<PolicyDetailsData> PolicyLst = Policy.GetPolicyDataForWindowService(parameters);

            PolicyLst = new List<PolicyDetailsData>(PolicyLst.Where(p => p.IsDeleted == false)).ToList();

            PolicyLst = new List<PolicyDetailsData>(PolicyLst.Where(p => p.ClientName != null)).ToList();

            PolicyLst = new List<PolicyDetailsData>(PolicyLst.Where(p => p.ClientId != null)).ToList();

            PolicyLst = new List<PolicyDetailsData>(PolicyLst.Where(p => p.PolicyId != Guid.Empty)).ToList();

            DateTime dtNextdate = new DateTime();
            dtNextdate = System.DateTime.Now;

            System.Console.WriteLine("\nFind the policy where runs the follow up process..");

            PolicyLst = new List<PolicyDetailsData>(PolicyLst.Where(p => p.LastFollowUpRuns != null && Convert.ToDateTime(p.LastFollowUpRuns).AddDays(DaysCnt) <= dtNextdate)).ToList();

            PolicyLst = PolicyLst.Where(p => p.LastFollowUpRuns != null).ToList();

            PolicyLst = PolicyLst.OrderBy(p => p.PolicyNumber != null).ToList();
            PolicyLst = PolicyLst.OrderBy(p => p.PolicyNumber != string.Empty).ToList();

            PolicyLst = PolicyLst.OrderBy(p => p.LastFollowUpRuns).ToList();

            System.Console.WriteLine("Total policy where need to runs follow process :" + PolicyLst.Count());

            foreach (PolicyDetailsData _Policy in PolicyLst)
            {
                if (Policy.FollowUpRunsRequired(_Policy.PolicyId))
                {
                    try
                    {
                        System.Console.WriteLine("\nFollow up running  for Policy number: " + _Policy.PolicyNumber);

                        FollowUpStatus _FollowUpStatus = FollowUpProcedure(_Policy, _Policy.IsTrackPayment);
                        //System.Console.WriteLine("**Follow up running  for Policy Id : " + _Policy.PolicyId.ToString() + "\n Policy number " + _Policy.PolicyNumber + " , " + _FollowUpStatus.IsComplete.ToString());
                        decimal dbPmc = PostUtill.CalculatePMC(_Policy.PolicyId);
                        //System.Console.WriteLine("PMC Calculated for Policy Id : " + _Policy.PolicyId.ToString() + "\n Policy number " + _Policy.PolicyNumber + " , " + _FollowUpStatus.IsComplete.ToString());
                        decimal dbPac = PostUtill.CalculatePAC(_Policy.PolicyId);
                        //System.Console.WriteLine("PAC Calculated for Policy Id : " + _Policy.PolicyId.ToString() + "\n Policy number " + _Policy.PolicyNumber + " , " + _FollowUpStatus.IsComplete.ToString());

                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("\nIssue  for Policy number: " + _Policy.PolicyNumber);
                        System.Console.WriteLine(ex.StackTrace.ToString());
                    }
                    finally
                    {
                        PolicyLocking.UnlockPolicy(_Policy.PolicyId);
                    }
                }
                else
                {
                    //PolicyLocking.UnlockPolicy(_Policy.PolicyId);
                    //System.Console.WriteLine("FollowUp Runs not required for Policy Id : " + _Policy.PolicyId.ToString() + "\n Policy number " + _Policy.PolicyNumber);
                }

            }
            if (PolicyLst.Count == 0)
            {
                System.Console.WriteLine("\nNo. policy found to runs\n");
            }
            System.Console.WriteLine("Follow up process completed");
            System.Console.ReadLine();
        }

        private static FollowUpStatus FollowUpProcedure(PolicyDetailsData FollowPolicy, bool IsTrackPayment)
        {
            FollowUpStatus _FollowUpStatus = new FollowUpStatus();
            _FollowUpStatus.IsTracked = false;
            bool IsAutoTrmDateUpadte = true;
            Policy.UpdateLastFollowupRunsWithTodayDate(FollowPolicy.PolicyId);
            bool isPaymenyExist = false;
            bool ReturnFlag = false;
            DateTime? StoreFirstMissingMonth = null;
            int noOfMissingCount = 0;
            PolicyDetailsData policy = PostUtill.GetPolicy(FollowPolicy.PolicyId);

            MasterPolicyMode? _MasterPolicyMode;

            _MasterPolicyMode = PostUtill.ModeEntryFromDeu(policy, null, false);

            PaymentMode _PaymentMode = PostUtill.ConvertMode(_MasterPolicyMode.Value);

            FollowUpDate _FollowUpDate = FollowUpUtill.CalculateFollowUpDateRange(_PaymentMode, policy, isPaymenyExist);

            //Advance payment
            bool invoiceDateBelongsToRange = false;

            bool bAvailableIntoRange = false;
            List<DateTime> dtAdavaceDateRange = new List<DateTime>();
            int intMulitply = 0;
            if (_FollowUpDate != null)
            {
                DateTime? originalEffectiveDate = policy.OriginalEffectiveDate;
                //Need to find invoice date and serch range inserted invoice date   

                int? intAdvance = null;
                if (originalEffectiveDate != null)
                {
                    intAdvance = policy.Advance;
                    intMulitply = GetRangValue(_PaymentMode);

                    for (int j = 0; j < intAdvance * intMulitply; j++)
                    {
                        dtAdavaceDateRange.Add(originalEffectiveDate.Value.AddMonths(j));
                    }
                }
            }


            //Get All Payment
            List<PolicyPaymentEntriesPost> _AllPaymentEntriesOnPolicyFormissing = PolicyPaymentEntriesPost.GetPolicyPaymentEntryPolicyIDWise(policy.PolicyId);
            //Get All  resolved issue
            List<PolicyPaymentEntriesPost> _AllResolvedorClosedIssueId = PolicyPaymentEntriesPost.GetAllResolvedorClosedIssueId(policy.PolicyId);

            if (_AllPaymentEntriesOnPolicyFormissing.Count > 0)
            {
                isPaymenyExist = true;
                invoiceDateBelongsToRange = false;

                for (int j = 0; j < _AllPaymentEntriesOnPolicyFormissing.Count; j++)
                {

                    for (int k = 0; k < dtAdavaceDateRange.Count; k++)
                    {
                        if (_AllPaymentEntriesOnPolicyFormissing[j].InvoiceDate.Equals(dtAdavaceDateRange[k]))
                        {
                            invoiceDateBelongsToRange = true;
                            break;
                        }
                        else
                        {
                            invoiceDateBelongsToRange = false;
                        }
                    }
                    if (invoiceDateBelongsToRange)
                    {
                        invoiceDateBelongsToRange = true;
                        break;
                    }


                }
            }
            else
            {
                isPaymenyExist = false;
            }

            try
            {

                if (IsTrackPayment)
                {
                    _FollowUpStatus.IsTracked = true;

                    List<DisplayFollowupIssue> FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);

                    #region Random
                    if (_PaymentMode == PaymentMode.Random)
                    {
                        FollowupIssueLst = FollowupIssue.GetIssues(policy.PolicyId);
                        //InValided The Issue
                        foreach (DisplayFollowupIssue follw in FollowupIssueLst)
                        {
                            MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(follw.IssueId, null);
                            FollowupIssue.DeleteIssue(follw.IssueId);
                        }

                    }
                    #endregion


                    if (_PaymentMode == PaymentMode.OneTime)
                    {
                    }

                    foreach (DisplayFollowupIssue follw in FollowupIssueLst)
                    {
                        MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(follw.IssueId, null);
                        FollowupIssue.DeleteIssue(follw.IssueId);
                    }

                    //FollowUpDate _FollowUpDate = FollowUpUtill.CalculateFollowUpDateRange(_PaymentMode, policy, isPaymenyExist);


                    if (_FollowUpDate.FromDate == null && !ReturnFlag)
                    {
                        List<DisplayFollowupIssue> FollowupIssueLst1 = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                        foreach (DisplayFollowupIssue follw in FollowupIssueLst1)
                        {
                            FollowupIssue.Delete(follw.IssueId);
                        }
                        ReturnFlag = true;
                    }

                    if (_FollowUpDate.ToDate == null && !ReturnFlag)
                    {
                        List<DisplayFollowupIssue> FollowupIssueLst1 = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                        foreach (DisplayFollowupIssue follw in FollowupIssueLst1)
                        {
                            FollowupIssue.Delete(follw.IssueId);
                        }
                        ReturnFlag = true;

                    }

                    if (_FollowUpDate.FromDate > _FollowUpDate.ToDate && !ReturnFlag)
                    {
                        List<DisplayFollowupIssue> FollowupIssueLst1 = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                        foreach (DisplayFollowupIssue follw in FollowupIssueLst1)
                        {
                            FollowupIssue.Delete(follw.IssueId);
                        }
                        ReturnFlag = true;
                    }


                    List<DateRange> _DateRangeForMissingLst = null;

                    if (_DateRangeForMissingLst == null)
                    {

                        foreach (DisplayFollowupIssue follw in FollowupIssueLst)
                        {
                            MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(follw.IssueId, null);
                            FollowupIssue.DeleteIssue(follw.IssueId);
                        }

                    }

                    if (!ReturnFlag)
                    {
                        FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                        _DateRangeForMissingLst = FollowUpUtill.MakeFollowUpDateRangeForMissing(_FollowUpDate, _PaymentMode);
                        //Get the issue range and delete before and after issue of the range
                        List<DisplayFollowupIssue> _FollowupIssueDoInValid = FollowupIssueLst.Where(p => (p.FromDate < _FollowUpDate.FromDate || p.ToDate > _FollowUpDate.ToDate)).ToList();

                        foreach (DisplayFollowupIssue closedIssue in _FollowupIssueDoInValid)
                        {
                            if (closedIssue.IssueStatusId != (int)FollowUpIssueStatus.Closed)
                            {
                                MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(closedIssue.IssueId, null);
                                FollowupIssue.DeleteIssue(closedIssue.IssueId);
                            }
                        }

                        FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);

                        //GEt Issue which is not closed and not the varriance into the payment
                        foreach (DisplayFollowupIssue follw in FollowupIssueLst.Where(p => p.IssueStatusId != (int)FollowUpIssueStatus.Closed).Where(p => p.IssueCategoryID != (int)FollowUpIssueCategory.VarSchedule))
                        {
                            bool flag = false;

                            for (int idx = 0; idx < _DateRangeForMissingLst.Last().RANGE; idx++)
                            {
                                DateTime? FirstDate = _DateRangeForMissingLst.Where(p => p.RANGE == idx + 1).ToList()[0].STARTDATE;
                                DateTime? LastDate = _DateRangeForMissingLst.Where(p => p.RANGE == idx + 1).ToList()[(int)_PaymentMode - 1].ENDDATE;

                                if (follw.FromDate == FirstDate && follw.ToDate == LastDate)
                                {
                                    flag = true;
                                    break;
                                }
                                else
                                {
                                    flag = false;
                                }

                            }
                            if (!flag)
                            {
                                MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(follw.IssueId, null);
                                FollowupIssue.DeleteIssue(follw.IssueId);
                            }
                        }


                        FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                        for (int idx = 0; idx < _DateRangeForMissingLst.Last().RANGE; idx++)
                        {
                            DateTime? FirstDate = _DateRangeForMissingLst.Where(p => p.RANGE == idx + 1).ToList()[0].STARTDATE;
                            DateTime? LastDate = _DateRangeForMissingLst.Where(p => p.RANGE == idx + 1).ToList()[(int)_PaymentMode - 1].ENDDATE;

                            //List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesFormissing = PolicyPaymentEntriesPost.GetAllPaymentEntriesOfRange(FirstDate.Value, LastDate.Value, policy.PolicyId);
                            //Get the payment at given range
                            List<PolicyPaymentEntriesPost> _PolicyPaymentEntriesFormissing = _AllPaymentEntriesOnPolicyFormissing.Where(p => p.InvoiceDate >= FirstDate.Value).Where(p => p.InvoiceDate <= LastDate.Value).ToList<PolicyPaymentEntriesPost>();

                            bool Rflag = FollowUpUtill.ISExistsResolveIssuesForDateRange(FirstDate.Value, LastDate.Value, policy.PolicyId, FollowupIssueLst);
                            if (_PolicyPaymentEntriesFormissing.Count == 0)
                            {
                                StoreFirstMissingMonth = FirstDate;
                                noOfMissingCount++;
                                List<DisplayFollowupIssue> issfolllst = FollowupIssueLst.Where(p => (p.FromDate == FirstDate && p.ToDate == LastDate)).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();
                                if (issfolllst.Count() != 0)
                                {
                                    if (Rflag)
                                    {
                                        StoreFirstMissingMonth = null;
                                        noOfMissingCount = 0;
                                    }
                                    if (FirstDate <= policy.OriginalEffectiveDate && policy.OriginalEffectiveDate <= LastDate)
                                    {
                                        DisplayFollowupIssue tempfis = issfolllst.FirstOrDefault();
                                        if (tempfis.IssueCategoryID != (int)FollowUpIssueCategory.MissFirst)
                                        {
                                            tempfis.IssueCategoryID = (int)FollowUpIssueCategory.MissFirst;
                                            //FollowupIssue.AddUpdate(tempfis);
                                        }

                                        if (invoiceDateBelongsToRange)
                                        {
                                            foreach (var item in dtAdavaceDateRange)
                                            {
                                                if (item.Equals(FirstDate.Value))
                                                {
                                                    bAvailableIntoRange = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    bAvailableIntoRange = false;
                                                }

                                            }
                                        }

                                        if (bAvailableIntoRange)
                                        {
                                            tempfis.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                            tempfis.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                        }

                                        FollowupIssue.AddUpdate(tempfis);
                                    }
                                    else
                                    {
                                        DisplayFollowupIssue tempfis = issfolllst.FirstOrDefault();
                                        if (tempfis.IssueCategoryID != (int)FollowUpIssueCategory.MissInv)
                                        {
                                            tempfis.IssueCategoryID = (int)FollowUpIssueCategory.MissInv;
                                            //FollowupIssue.AddUpdate(tempfis);
                                        }
                                        if (invoiceDateBelongsToRange)
                                        {
                                            foreach (var item in dtAdavaceDateRange)
                                            {
                                                if (item.Equals(FirstDate.Value))
                                                {
                                                    bAvailableIntoRange = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    bAvailableIntoRange = false;
                                                }
                                            }
                                        }

                                        if (bAvailableIntoRange)
                                        {
                                            tempfis.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                            tempfis.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                        }
                                        FollowupIssue.AddUpdate(tempfis);
                                    }
                                }
                                else
                                {
                                    if (FirstDate <= policy.OriginalEffectiveDate && policy.OriginalEffectiveDate <= LastDate)
                                        FollowUpUtill.RegisterIssueAgainstMissingPayment(policy, FollowUpIssueCategory.MissFirst, FirstDate.Value, LastDate.Value);
                                    else
                                        FollowUpUtill.RegisterIssueAgainstMissingPayment(policy, FollowUpIssueCategory.MissInv, FirstDate.Value, LastDate.Value);

                                    if (invoiceDateBelongsToRange)
                                    {
                                        foreach (var item in dtAdavaceDateRange)
                                        {
                                            if (item.Equals(FirstDate.Value))
                                            {
                                                bAvailableIntoRange = true;
                                                break;
                                            }
                                            else
                                            {
                                                bAvailableIntoRange = false;
                                            }
                                        }
                                    }

                                    if (bAvailableIntoRange)
                                    {
                                        FollowupIssueLst = FollowupIssue.GetIssues(policy.PolicyId);
                                        List<DisplayFollowupIssue> issueFollowUp = FollowupIssueLst.Where(p => (p.FromDate == FirstDate && p.ToDate == LastDate)).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();
                                        DisplayFollowupIssue tempIssue = issueFollowUp.FirstOrDefault();

                                        tempIssue.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                        tempIssue.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                        FollowupIssue.AddUpdate(tempIssue);
                                    }

                                }
                                if (noOfMissingCount != 0 && StoreFirstMissingMonth != null && IsAutoTrmDateUpadte)
                                    FollowUpUtill.AutoPolicyTerminateProcess(_PaymentMode, StoreFirstMissingMonth, policy.PolicyId, noOfMissingCount, null);
                            }
                            else
                            {
                                StoreFirstMissingMonth = null;
                                noOfMissingCount = 0;
                                List<DisplayFollowupIssue> FollowupIssueMissingtoclose = FollowupIssueLst.Where(p => p.FromDate == FirstDate).Where(p => p.ToDate == LastDate).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();

                                if (FollowupIssueMissingtoclose.Count > 0)
                                {
                                    foreach (DisplayFollowupIssue _fossu in FollowupIssueMissingtoclose)
                                    {
                                        _fossu.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                        _fossu.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                        FollowupIssue.AddUpdate(_fossu);
                                    }
                                }
                                else
                                {
                                    if (FirstDate <= policy.OriginalEffectiveDate && policy.OriginalEffectiveDate <= LastDate)
                                        FollowUpUtill.RegisterIssueAgainstMissingPayment(policy, FollowUpIssueCategory.MissFirst, FirstDate.Value, LastDate.Value);

                                    else
                                    {
                                        FollowUpUtill.RegisterIssueAgainstMissingPayment(policy, FollowUpIssueCategory.MissInv, FirstDate.Value, LastDate.Value);
                                    }

                                    FollowupIssueLst = FollowupIssue.GetIssues(policy.PolicyId);

                                    List<DisplayFollowupIssue> FollowupIssueMissingtoclose1 = FollowupIssueLst.Where(p => p.FromDate == FirstDate).Where(p => p.ToDate == LastDate).Where(p => (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)).ToList();
                                    foreach (DisplayFollowupIssue _fossu in FollowupIssueMissingtoclose1)
                                    {
                                        _fossu.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                        _fossu.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                        FollowupIssue.AddUpdate(_fossu);
                                    }

                                }
                            }
                        }
                    }

                    FollowupIssueLst = FollowupIssue.GetIssues(FollowPolicy.PolicyId);

                    //Code for manually resolved or closed the issue for commision dashboard
                    foreach (PolicyPaymentEntriesPost ResolvedorClosedIssue in _AllResolvedorClosedIssueId)
                    {
                        DisplayFollowupIssue FollowupIssuetemp = FollowupIssueLst.Where(p => (p.PolicyId == ResolvedorClosedIssue.PolicyID) && (p.InvoiceDate == ResolvedorClosedIssue.InvoiceDate)).FirstOrDefault();
                        PolicyPaymentEntriesPost objPolicyPaymentEntriesPost = _AllResolvedorClosedIssueId.Where(p => (p.PolicyID == ResolvedorClosedIssue.PolicyID) && (p.InvoiceDate == ResolvedorClosedIssue.InvoiceDate)).Where(p => p.FollowUpIssueResolveOrClosed == 1).FirstOrDefault();

                        if (objPolicyPaymentEntriesPost != null)
                        {
                            if (objPolicyPaymentEntriesPost.PaymentEntryID != null)
                            {
                                FollowupIssuetemp.IssueResultId = (int)FollowUpResult.Resolved_Brk;
                                FollowupIssuetemp.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                FollowupIssue.AddUpdate(FollowupIssuetemp);
                                PolicyPaymentEntriesPost.UpdateVarPaymentIssueId(ResolvedorClosedIssue.PaymentEntryID, FollowupIssuetemp.IssueId);
                            }
                        }
                    }

                    foreach (PolicyPaymentEntriesPost ppepfv in _AllPaymentEntriesOnPolicyFormissing)
                    {
                        DisplayFollowupIssue FollowupIssuetemp = FollowupIssueLst.Where(p => (p.PolicyId == ppepfv.PolicyID) && (p.InvoiceDate == ppepfv.InvoiceDate)).Where(p => p.IssueCategoryID == 3).FirstOrDefault();

                        bool flagvarience = PostUtill.CheckForIncomingScheduleVariance(ppepfv, policy.ModeAvgPremium);
                        if (flagvarience)
                        {
                            if (FollowupIssuetemp == null)
                                FollowUpUtill.RegisterIssueAgainstScheduleVariance(ppepfv);

                            else
                                PolicyPaymentEntriesPost.UpdateVarPaymentIssueId(ppepfv.PaymentEntryID, FollowupIssuetemp.IssueId);
                        }
                        else
                        {
                            if (FollowupIssuetemp != null)
                            {

                                if (FollowupIssuetemp.IssueStatusId != (int)FollowUpIssueStatus.Closed)
                                {
                                    FollowupIssuetemp.IssueStatusId = (int)FollowUpIssueStatus.Closed;
                                    FollowupIssuetemp.IssueResultId = (int)FollowUpResult.Resolved_CD;
                                    FollowupIssue.AddUpdate(FollowupIssuetemp);
                                }
                            }
                        }
                    }

                    //If policy settings. Track incoming %=. F. 
                    //Delete issues with (status=open or result =Resolved_CD) and (category=VarSchedule or category=VarCompDue)                    
                    if (policy.IsTrackIncomingPercentage == false)
                    {
                        List<DisplayFollowupIssue> AllIssueList = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                        List<DisplayFollowupIssue> ForDeleteIssuelist = new List<DisplayFollowupIssue>(AllIssueList.Where(p => (p.IssueStatusId == (int)FollowUpIssueStatus.Open || p.IssueCategoryID == (int)FollowUpResult.Resolved_CD) && (p.IssueCategoryID == (int)FollowUpIssueCategory.VarCompDue || p.IssueCategoryID == (int)FollowUpIssueCategory.VarSchedule)));
                        foreach (var item in ForDeleteIssuelist)
                        {
                            MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(item.IssueId, null);
                            FollowupIssue.DeleteIssue(item.IssueId);
                        }
                    }
                    //If policy settings. Track missing month=. F. 
                    //Delete issues with (status=open or result =Resolved_CD) and (category=miss first or category=miss inv) 
                    if (policy.IsTrackMissingMonth == false)
                    {
                        List<DisplayFollowupIssue> AllIssueList = FollowupIssue.GetIssues(FollowPolicy.PolicyId);
                        List<DisplayFollowupIssue> ForDeleteIssuelist = new List<DisplayFollowupIssue>(AllIssueList.Where(p => (p.IssueStatusId == (int)FollowUpIssueStatus.Open || p.IssueCategoryID == (int)FollowUpResult.Resolved_CD) && (p.IssueCategoryID == (int)FollowUpIssueCategory.MissFirst || p.IssueCategoryID == (int)FollowUpIssueCategory.MissInv)));
                        foreach (var item in ForDeleteIssuelist)
                        {
                            MyAgencyVault.BusinessLibrary.FollowUpUtill.UpdateIssueIdOfPaymentsForIssueId(item.IssueId, null);
                            FollowupIssue.DeleteIssue(item.IssueId);
                        }
                    }
                    _FollowUpStatus.IsComplete = true;
                }
            }
            catch (Exception ex)
            {
                _FollowUpStatus.IsComplete = false;
                _FollowUpStatus.Actions += "--Error-";
                _FollowUpStatus.ex = ex;
                ActionLogger.Logger.WriteLog(ex.StackTrace.ToString(), true);
                //ActionLogger.Logger.WriteLog("\n", true);
#if DEBUG
               // throw new Exception();
#endif
            }
            //transaction.Complete();
            return _FollowUpStatus;
            //}
        }

        private static int GetRangValue(PaymentMode _PaymentType)
        {
            int intVAlue = 0;
            switch (_PaymentType.ToString())
            {
                case "Monthly":
                    intVAlue = 1;
                    break;
                case "Quarterly":
                    intVAlue = 3;
                    break;
                case "HalfYearly":
                    intVAlue = 6;
                    break;
                case "Yearly":
                    intVAlue = 12;
                    break;
                case "OneTime":
                    intVAlue = 1;
                    break;
                case "Random":
                    intVAlue = 0;
                    break;
            }

            return intVAlue;
        }

    }
}
