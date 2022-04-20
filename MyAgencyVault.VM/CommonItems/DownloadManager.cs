using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyAgencyVault.VM.CommonItems
{
    public class DownloadManager
    {
        public Guid TrackingNo { get; set; }

        public string BatchEntryStatus { get; set; }

        public string payor { get; set; }

        public DateTime? Available { get; set; }

        public string FileType { get; set; }

        public string Batch { get; set; }

        public string Statement { get; set; }

        public string BatchStatus { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Url { get; set; }

        public string FileName { get; set; }

        public string NavigationInstructions { get; set; }

        public string Agency { get; set; }

    }
}
