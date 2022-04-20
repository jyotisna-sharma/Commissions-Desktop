using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace MyAgencyVault.VM.CommonItems
{
    class ApplicationAgencyVault
    {
        public static string ApplicationDirectory()
        {
            try
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            catch { return string.Empty; }
        }

        public static string ApplicationDataDirectory()
        {
            //try
            //{
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data");
            //}
            //catch { string.Empty; }
        }
    }
}
