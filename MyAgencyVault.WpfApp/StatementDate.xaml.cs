using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.VMLib;
using System.Collections.ObjectModel;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.WinApp
{
    /// <summary>
    /// Interaction logic for StatementDate.xaml
    /// </summary>
    public partial class StatementDate : Window
    {
        VMConfigrationManager objConf;
        List<StatementDates> statementDates;
        int selectedYear;
        
        public StatementDate()
        {
            InitializeComponent();         
        }

       
        public StatementDate(VMConfigrationManager vmc)
        {
            objConf = vmc;
            InitializeComponent();
        }

        private void UpdateStatementDates_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection < StatementDates > addedDeletedDates = new ObservableCollection<StatementDates>(statementDates.Where(s => s.IsNew == true || s.IsDeleted == true).ToList());
            objConf.PayorDefaultVM.OnCloseCalender(addedDeletedDates);

            if (addedDeletedDates != null && addedDeletedDates.Count != 0)
            {
                statementDates.Where(s => s.IsNew == true).ToList().ForEach(s => s.IsNew = false);
                statementDates.RemoveAll(s => s.IsDeleted == true);

                List<StatementDates> dates = objConf.PayorDefaultVM.AllPayorStatementDates.Where(s => s.PayorID == objConf.SelectedDisplayPayor.PayorID && s.StatementDate.Year == selectedYear).ToList();
                foreach (StatementDates date in dates)
                    objConf.PayorDefaultVM.AllPayorStatementDates.Remove(date);

                objConf.PayorDefaultVM.AllPayorStatementDates.AddRange(statementDates);
            }

            this.Close();
        }

        private void cmbYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int month = 1;
            selectedYear = int.Parse(((System.Windows.Controls.ContentControl)(cmbYear.SelectedItem)).Content.ToString());

            List<DateTime> dates = null;
            List<DateTime> tempDates = null;

            if (objConf.PayorDefaultVM.AllPayorStatementDates != null)
            {
                statementDates = objConf.PayorDefaultVM.AllPayorStatementDates.Where(s => s.PayorID == objConf.SelectedDisplayPayor.PayorID && s.StatementDate.Year == selectedYear).ToList();
                dates = statementDates.Select(s => s.StatementDate).ToList();
            }

            foreach (UIElement element in gridCalanders.Children)
            {
                Calendar calender = element as Calendar;
                if (calender != null)
                {
                    DateTime StartDateTime = new DateTime(int.Parse(((System.Windows.Controls.ContentControl)(cmbYear.SelectedItem)).Content.ToString()), month, 1);
                    DateTime EndDateTime = StartDateTime.AddMonths(1).AddDays(-1);

                    calender.SelectedDates.Clear();
                    calender.DisplayDateStart = StartDateTime;
                    calender.DisplayDateEnd = EndDateTime;

                    if (dates != null)
                    {
                        tempDates = dates.Where(s => s >= StartDateTime && s <= EndDateTime).ToList();
                        if (tempDates != null)
                        {
                            foreach (DateTime date in tempDates)
                                calender.SelectedDates.Add(date);
                        }
                    }
                }
                month++;
            }
        }

        private void CloseUpdateStatements_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cal_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (DateTime date in e.AddedItems)
            {
                if (statementDates != null)
                {
                    StatementDates stmtDate = statementDates.FirstOrDefault(s => s.StatementDate == date);
                    if (stmtDate != null)
                    {
                        stmtDate.IsDeleted = false;
                    }
                    else
                    {
                        stmtDate = new StatementDates { IsNew = true, PayorID = objConf.SelectedDisplayPayor.PayorID, PayorStatementDateID = Guid.NewGuid(), StatementDate = date };
                        statementDates.Add(stmtDate);
                    }
                }
            }

            foreach (DateTime date in e.RemovedItems)
            {
                if (statementDates != null)
                {
                    StatementDates stmtDate = statementDates.FirstOrDefault(s => s.StatementDate == date);
                    if (stmtDate !=null)
                    {
                        stmtDate.IsDeleted = true;
                        if (stmtDate.IsNew == true)
                            statementDates.Remove(stmtDate);
                    }
                }
            }
        }
    }
}
