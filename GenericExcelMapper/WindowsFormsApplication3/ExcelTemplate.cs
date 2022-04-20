using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Linq;
using System.Runtime.InteropServices;

namespace GenericExcelMapper
{
    public partial class ExcelTemplate : UserControl
    {
        Microsoft.Office.Interop.Excel.ApplicationClass _excelApplication;
        Microsoft.Office.Interop.Excel.Workbook _newWorkbook;
        Microsoft.Office.Interop.Excel.Worksheet _newWorksheet;

        private const int QuestionColumn = 1;
        private const int AnswerColumn = 2;
        
        int _dataPosTop = 4;

        private GenericExcelMapperVM ExcelMapper;
        private Payor CurrentPayor;

        public ExcelTemplate()
        {
            InitializeComponent();
            VMInstances.setVMObject += new VMInstances.ConfigVMSetDelegate(VMInstances_setVMObject);
        }

        void VMInstances_setVMObject(VMConfigrationManager vmObject)
        {
            if (VMInstances.ConfigurationManager != null)
            {
                ExcelMapper = VMInstances.ConfigurationManager.VmGenericDataMapper;
                ExcelMapper.ReloadData += new EventHandler(ExcelMapper_ReloadData);
                CurrentPayor = VMInstances.ConfigurationManager.CurrentPayor;
            }

            if (ExcelMapper != null)
            {
                lstIssueList.Items.Clear();

                if(ExcelMapper.IssuedFiles != null)
                    foreach (string file in ExcelMapper.IssuedFiles)
                        lstIssueList.Items.Add(file);

                LoadDeuFieldExcelGrid();
            }            
        }

        private void LoadDeuFieldExcelGrid()
        {
            int row = 1;

            axSpreadsheetMap.Cells[row, QuestionColumn] = "DB Column";
            axSpreadsheetMap.Cells[row, AnswerColumn] = "Column No.";
            axSpreadsheetMap.Cells[row, AnswerColumn + 1] = "Excel Column";

            row = row + 1;
            foreach (string column in ExcelMapper.DeuFields)
            {
                axSpreadsheetMap.Cells[row, QuestionColumn] = column;
                if (ExcelMapper.PayorTemplate != null && ExcelMapper.PayorTemplate.MappedFields != null)
                {
                    MappedField field = ExcelMapper.PayorTemplate.MappedFields.FirstOrDefault(s => s.DBField == column);
                    if (field != null && !string.IsNullOrEmpty(field.DBField))
                    {
                        axSpreadsheetMap.Cells[row, QuestionColumn + 1] = field.ExcelField;
                        axSpreadsheetMap.Cells[row, QuestionColumn + 2] = field.ExcelFieldName;
                    }
                    else
                    {
                        axSpreadsheetMap.Cells[row, QuestionColumn + 1] = string.Empty;
                        axSpreadsheetMap.Cells[row, QuestionColumn + 2] = string.Empty;
                    }
                }
                else
                {
                    axSpreadsheetMap.Cells[row, QuestionColumn + 1] = string.Empty;
                    axSpreadsheetMap.Cells[row, QuestionColumn + 2] = string.Empty;
                }
                row++;
            }
        }

        private void SaveToolStripButtonClick(object sender, EventArgs e)
        {
            
        }

        private string GetXlsFileColumnList(int HeaderRow)
        {
            string xlColumnList = string.Empty;
            int column = 1;
            while (axSpreadsheet.get_Range("A1:A1").get_Item(HeaderRow, column).Text.ToString().Length > 0)
            {
               xlColumnList += axSpreadsheet.get_Range("A1:A1").get_Item(HeaderRow, column++).Text.ToString().Trim() + ",";
            }
            xlColumnList = xlColumnList.Remove(xlColumnList.Length - 1, 1);
            return xlColumnList;
        }
        

        private string GetXLColumnNumber(string dbColumn,List<Map> listMap)
        {           
             foreach(Map map in listMap)
             {            
                 if( map.DbColumn==dbColumn)
                 {
                     return map.XlColumn;
                 }         
             }
             return "";
        }

        private void UploadtoolStripButtonClick(object sender, EventArgs e)
        {
            Dal dal = new Dal();
            Template template = GeTemplateId();
            if (template.TemplateId == 0)
            {
                MessageBox.Show("Template not found ;");
                return;
            }
            var listMap = dal.GetMapListDb(template.TemplateId);
            var cl = dal.GetDbColumns();
            int firstRow = GeFirstRow(template);
            int lastRow = GeLastRow(firstRow);
            for (int i = firstRow; i < lastRow; i++)
            {
                var row = new List<ColData>();
                foreach (var c in cl)
                {
                    var coldata = new ColData();
                    if (GetXLColumnNumber(c.CloumnName, listMap).Length > 0)
                    {
                        coldata.XlValue = axSpreadsheet.get_Range("A1:A1").get_Item(i, Convert.ToInt32(GetXLColumnNumber(c.CloumnName, listMap))).Text.ToString();
                        coldata.DbColumn = c.CloumnName;
                        coldata.IsNumeric = IsNumeric(coldata.XlValue);
                        if (coldata.XlValue.ToString().Length > 0 && coldata.IsNumeric.ToString().Length > 0)
                        {
                            row.Add(coldata);
                        }
                    }

                }
                dal.UpdateDatabase(row);
            }
            MessageBox.Show("Upload Done");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IsNumeric(string val)
        {
            Double result;
            return Double.TryParse(val, out result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private int GeFirstRow(Template t)
        {
            var i = 1;
            while (i < 200)
            {
                if (t.ColumnList == GetXlsFileColumnList(i) && t.ColumnList.Length > 0)
                {
                    return i + 1;
                }
                i++;
            }

            return 0;
        }

        private int GeLastRow(int firstRow)
        {
            var i = firstRow;
            while (GetXlsFileColumnList(i).Length>0)
            {      
                i++;
            }
            return i;
        }

        private Template GeTemplateId()
        {
            var dal = new Dal();
            var t1 = new Template();
            List<Template> templateList = dal.GetTemplates();
            foreach (var t in templateList)
            {
                var i = 1;
                while(i<100)
                {
                    if (t.ColumnList == GetXlsFileColumnList(i) && t.ColumnList.Length>0)
                    {
                        return t;
                    }
                    i++;
                }
            }
            return t1;
        }

        private void CmbPayersSelectedIndexChanged(object sender, EventArgs e)
        {
            var dal = new Dal();
            List<Issue> issue = dal.GetIssueList(CurrentPayor.PayorName);
            lstIssueList.DataSource = issue;
            lstIssueList.DisplayMember = "IssueName";
            lstIssueList.ValueMember = "IssueName";
        }
                
        private void lstIssueList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            string ExcelPath = listBox.Text;
            LoadExcelFile(ExcelPath);
        }

        private void LoadExcelFile(string ExcelPath)
        {
            Object missing = System.Reflection.Missing.Value;
             _excelApplication = new Microsoft.Office.Interop.Excel.ApplicationClass();
            _excelApplication.Visible = false;

            ExcelPath = ExcelMapper.DownloadExcelFile(ExcelPath);

            if (!string.IsNullOrEmpty(ExcelPath))
            {
                _newWorkbook = _excelApplication.Workbooks.Open(ExcelPath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, false, false);
                _newWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)_newWorkbook.Worksheets[1];

                _newWorksheet.Cells.Select();
                _newWorksheet.Cells.Copy(missing);
                axSpreadsheet.Cells.Select();
                axSpreadsheet.Cells.Paste();
                axSpreadsheet.ViewOnlyMode = true;
                axSpreadsheetMap.ViewOnlyMode = true;

                _newWorkbook.Close(true, null, null);
                _excelApplication.Quit();

                Marshal.ReleaseComObject(_newWorkbook);
                Marshal.ReleaseComObject(_newWorksheet);
                Marshal.ReleaseComObject(_excelApplication);

                //releaseObject(_newWorkbook);
                //releaseObject(_newWorksheet);
                //releaseObject(_excelApplication);
            }
            else
            {
                MessageBox.Show("File is not downloaded properly.Try again.");
            }
        }

        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Unable to release the Object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        } 

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ExcelMapper.PayorTemplate != null)
                ExcelMapper.PayorTemplate = new PayorTemplate();
            else
                ExcelMapper.PayorTemplate.MappedFields.Clear();

            string xlColumnList = GetXlsFileColumnList(_dataPosTop);
            ExcelMapper.PayorTemplate.XlsColumnList = xlColumnList;

            ExcelMapper.PayorTemplate.SheetName = txtBoxSheetName.Text;
            int HeaderRow = 0;

            if (int.TryParse(txtBoxDataStartIndex.Text, out HeaderRow))
                ExcelMapper.PayorTemplate.DataStartIndex = HeaderRow;
            else
                ExcelMapper.PayorTemplate.DataStartIndex = 0;

            if (ExcelMapper.PayorTemplate.MappedFields == null)
                ExcelMapper.PayorTemplate.MappedFields = new ObservableCollection<MappedField>();
            else
                ExcelMapper.PayorTemplate.MappedFields.Clear();

            foreach (string deuField in ExcelMapper.DeuFields)
            {
                string mappedXlsFieldNo = GetXlsColumnNumber(deuField);
                
                if (!string.IsNullOrEmpty(mappedXlsFieldNo))
                {
                    MappedField mapField = new MappedField();
                    mapField.ExcelField = mappedXlsFieldNo;
                    mapField.DBField = deuField;
                    mapField.ExcelFieldName = GetXlsColumnName(deuField);
                    ExcelMapper.PayorTemplate.MappedFields.Add(mapField);
                }
            }
           
            ExcelMapper.SaveTemplate();
        }

        private string GetXlsColumnNumber(string dbColumn)
        {
            var row = 2;
            while (axSpreadsheetMap.get_Range("A1:A1").get_Item(row, QuestionColumn).Text.ToString().Length != 0)
            {
                if (axSpreadsheetMap.get_Range("A1:A1").get_Item(row, QuestionColumn).Text.ToString() == dbColumn)
                {
                    return axSpreadsheetMap.get_Range("A1:A1").get_Item(row, AnswerColumn).Text.ToString();
                };
                row++;
            }
            return string.Empty ;
        }

         private string GetXlsColumnName(string dbColumn)
        {
            var row = 2;
            while (axSpreadsheetMap.get_Range("A1:A1").get_Item(row, QuestionColumn).Text.ToString().Length != 0)
            {
                if (axSpreadsheetMap.get_Range("A1:A1").get_Item(row, QuestionColumn).Text.ToString() == dbColumn)
                {
                    return axSpreadsheetMap.get_Range("A1:A1").get_Item(row, AnswerColumn + 1).Text.ToString();
                };
                row++;
            }
            return string.Empty ;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ExcelMapper.CancelTemplate();
            ReloadTemplate();
        }

        void ExcelMapper_ReloadData(object sender, EventArgs e)
        {
            ReloadTemplate();
        }

        private void ReloadTemplate()
        {
            if (ExcelMapper != null)
            {
                lstIssueList.Items.Clear();
                axSpreadsheet.Cells.Clear();

                if (ExcelMapper.IssuedFiles != null)
                    foreach (string file in ExcelMapper.IssuedFiles)
                        lstIssueList.Items.Add(file);

                LoadDeuFieldExcelGrid();
            }     
        }

        private void btnPaste_Click(object sender, EventArgs e)
        {
            if (axSpreadsheetMap.ActiveCell.Column == AnswerColumn)
            {
                axSpreadsheetMap.ActiveCell.Value2 = axSpreadsheet.ActiveCell.Column.ToString();
                axSpreadsheetMap.ActiveCell.Offset[0, 1].Value2 = axSpreadsheet.ActiveCell.Value2;
                _dataPosTop = axSpreadsheet.ActiveCell.Row;
            }
        }
    }
}
