using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace GenericExcelDataMapper
{
    public partial class ExcelTemplate : UserControl
    {

        private GenericExcelMapperVM ExcelMapper;
        private ExcelFileUtility ExcelFileUtil;

        public ExcelTemplate()
        {
            InitializeComponent();
            VMInstances.setVMObject += new VMInstances.ConfigVMSetDelegate(VMInstances_setVMObject);
            this.Disposed += new EventHandler(ExcelTemplate_Disposed);
        }

        void ExcelTemplate_Disposed(object sender, EventArgs e)
        {
            if (ExcelFileUtil != null)
                ExcelFileUtil.CloseFile();
            ExcelFileUtil = null;
        }

        void VMInstances_setVMObject(VMConfigrationManager vmObject)
        {
            if (VMInstances.ConfigurationManager != null)
            {
                ExcelMapper = VMInstances.ConfigurationManager.VmGenericDataMapper;
                ExcelMapper.ReloadData += new EventHandler(ExcelMapper_ReloadData);
                ExcelMapper.FileDownloaded += new EventHandler(ExcelMapper_FileDownloaded);
            }         
        }


        public string GetXlsFileColumnList(int HeaderRow)
        {
            if (ExcelFileUtil != null)
                return ExcelFileUtil.GetXlsFileColumnList(HeaderRow);
            else
                return string.Empty;
        }

        public void ExcelMapper_FileDownloaded(object sender, EventArgs e)
        {
            string ExcelPath = sender as string;
            ExcelFileUtil = new ExcelFileUtility(ExcelPath,axSpreadsheet);
        }

        void ExcelMapper_ReloadData(object sender, EventArgs e)
        {
            if (ExcelFileUtil != null)
                ExcelFileUtil.CloseFile();
            ExcelFileUtil = null;

            ReloadTemplate();
        }

        private void ReloadTemplate()
        {
            if (ExcelMapper != null)
            {
                axSpreadsheet.Cells.Clear();
            }     
        }

        public string getColumnNumber()
        {
            return axSpreadsheet.ActiveCell.Column.ToString();
        }

        public string getColumnName()
        {
            return axSpreadsheet.ActiveCell.Value2.ToString();
        }
    }
}
