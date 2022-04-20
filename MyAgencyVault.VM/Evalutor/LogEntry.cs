//LogEntry.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.VMLib.PayorForm.Evalutor
{
    public class LogEntry
    {
        #region Properties

        public string TokenAction { get; set; }
        public List<LogOperation> Operations { get; set; }

        #endregion

        #region Constructors

        public LogEntry()
        {
            TokenAction = string.Empty;
            Operations = new List<LogOperation>();
        }

        #endregion
    }
}
