using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GenericExcelDataMapper
{
    public class ExcelFileUtility
    {
        Microsoft.Office.Interop.Excel.Application _excelApplication;
        Microsoft.Office.Interop.Excel.Workbook _newWorkbook;
        Microsoft.Office.Interop.Excel.Worksheet _newWorksheet;

        public string GetXlsFileColumnList(int HeaderRow)
        {
            if (_newWorksheet == null)
                return string.Empty;

            string xlColumnList = string.Empty;
            int column = 1;
            while (_newWorksheet.Cells[HeaderRow, column].Value2 != null && _newWorksheet.Cells[HeaderRow, column].Value2.ToString().Length > 0)
            {
                xlColumnList += _newWorksheet.Cells[HeaderRow, column].Value2.ToString().Trim() + ",";
                column++;
            }
            xlColumnList = xlColumnList.Remove(xlColumnList.Length - 1, 1);
            return xlColumnList;
        }

        public void CloseFile()
        {
            _newWorkbook.Close(true, null, null);
            _excelApplication.Quit();

            System.Runtime.InteropServices.Marshal.ReleaseComObject(_newWorkbook);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(_newWorksheet);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(_excelApplication);
        }

        public ExcelFileUtility(string ExcelPath, AxMicrosoft.Office.Interop.Owc11.AxSpreadsheet axSpreadsheet)        
        {
             Object missing = System.Reflection.Missing.Value;
            _excelApplication = new Microsoft.Office.Interop.Excel.Application();
            _excelApplication.Visible = false;

            if (!string.IsNullOrEmpty(ExcelPath))
            {
                _newWorkbook = _excelApplication.Workbooks.Open(ExcelPath, 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, false, false);
                Thread.Sleep(500);
                _newWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)_newWorkbook.Worksheets[1];

                _newWorksheet.Cells.Select();
                _newWorksheet.Cells.Copy(missing);
                axSpreadsheet.Cells.Select();
                axSpreadsheet.Cells.Paste();
                axSpreadsheet.ViewOnlyMode = true;
            }
            else
            {
                MessageBox.Show("File is not downloaded properly.Try again.");
            }
        }
    }
}
