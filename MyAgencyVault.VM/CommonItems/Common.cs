using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.VMLib
{
    public static class Common
    {

        public static string ValidateIncomingSchedule(ObservableCollection<Graded> GradedList, ObservableCollection<NonGraded> NonGradedList, bool IsGraded, string SelectedIncomingMode, int ScheduleTypeId)
        {
            if (SelectedIncomingMode == "Custom")
            {
                var scheduleName = ScheduleTypeId == 1 ? "% of Premium" : "Per Head";
                if (IsGraded)
                {
                    if (GradedList == null || (GradedList != null && GradedList.Count == 0))
                    {
                        return "Please enter a range of values in schedule.";
                    }
                    else
                    {
                        GradedList = new ObservableCollection<Graded>(GradedList.OrderBy(x => x.From));

                        for (int i = 0; i < GradedList.Count; i++)
                        {
                            
                            if (GradedList[i].From == 0 || GradedList[i].To == 0 || GradedList[i].Percent == 0)
                                return "From, To or"+ " " +scheduleName + " "+ "field cannot be blank or '0'.";

                            if (GradedList[i].To <= GradedList[i].From)
                                return "To value must be greater than From value."; // + GradedList[i].From;

                            if (i > 0)
                            {
                                if (GradedList[i].From != GradedList[i - 1].To + 1)
                                {
                                    return "'From' value in the range must be the next number of 'To' value in previous range.";
                                }
                            }

                        }
                        if (GradedList.OrderBy(x => x.From).FirstOrDefault().From != 1)
                        {
                            return "Range is missing with 'From' value starting from '1'.";
                        }
                    }
                }
                else
                {
                    if (NonGradedList == null || (NonGradedList != null && NonGradedList.Count == 0))
                    {
                        return "Please enter values in schedule.";
                    }
                    else
                    {
                        NonGradedList = new ObservableCollection<NonGraded>(NonGradedList.OrderBy(x => x.Year));

                        for (int i = 0; i < NonGradedList.Count; i++)
                        {
                            if (NonGradedList[i].Year == 0 || NonGradedList[i].Percent == 0)
                                return "Year and "+ scheduleName+" "+"cannot be blank or 0 in schedule";

                            if (i > 0)
                            {
                                if (NonGradedList[i].Year != NonGradedList[i - 1].Year + 1)
                                {
                                    return "Year numbers must be consecutive without missing any value in between.";
                                }
                            }

                        }
                        if (NonGradedList.FirstOrDefault().Year != 1)
                        {
                            return "Year 1 schedule is missing.";
                        }
                    }
                }
            }
            return "";
        }
    }
}
