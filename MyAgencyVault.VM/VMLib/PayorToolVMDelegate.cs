using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.ViewModel;
using MyAgencyVault.ViewModel.VMLib;
using MyAgencyVault.VM.MyAgencyVaultSvc;

namespace MyAgencyVault.VM.VMLib
{
    public class PayorToolVMDelegate
    {
        PayorToolVM m_PayorToolVM;

        public PayorToolVMDelegate()
        {
            m_PayorToolVM = VMInstances.PayorToolVM;
        }

        internal bool OnDropValidation(PayorToolAvailablelFieldType payorToolField)
        {
            return m_PayorToolVM.OnDropValidation(payorToolField);
        }

        internal PayorToolField OnDropCompleted(PayorToolAvailablelFieldType payorToolField, double p, double p_2, double p_3, double p_4)
        {
            return m_PayorToolVM.OnDropCompleted(payorToolField);
        }

        internal void OnFieldDeleted(PayorToolAvailablelFieldType payorToolAvailablelFieldType)
        {
            m_PayorToolVM.OnFieldDeleted(payorToolAvailablelFieldType);
        }

        internal void OnFieldSelectionChanged(PayorToolAvailablelFieldType selectedField)
        {
            m_PayorToolVM.OnFieldSelectionChanged(selectedField);
        }

        internal void SetCanvas(PayorForm.DesignerCanvas designerCanvas)
        {
            m_PayorToolVM.SetCanvas(designerCanvas);
        }

        internal Guid getPayorId()
        {
            return m_PayorToolVM.getPayorId();
        }
    }
}
