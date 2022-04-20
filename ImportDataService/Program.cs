using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace ImportDataService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ImportTool sr = new ImportTool();
            sr.StartFolderWatcher();

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new ImportTool() 
            //};
            //ServiceBase.Run(ServicesToRun);

        }
    }
}
