//LogOperation.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class LogOperation
    {
        #region Properties

        public string OperationText { get; set; }
        public string OutputContents { get; set; }
        public string StackContents { get; set; }

        #endregion

        #region Constructors

        public LogOperation()
        {
            OperationText = string.Empty;
            OutputContents = string.Empty;
            StackContents = string.Empty;
        }

        #endregion
    }
}
