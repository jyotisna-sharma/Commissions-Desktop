using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using MyAgencyVault.ViewModel.VMLib;
using System.Diagnostics;
using System.Reflection;
using MyAgencyVault.VMLib;
using System.Globalization;
using System.Threading;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.ViewModel;
using System.Windows.Threading;

namespace MyAgencyVault.WinApp
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public delegate void DownLoadNewverionOfmyagencyVault();
  public partial class App : Application
  {

    Login objLogin;
    MainWindow objMainWindow;
    ForgotPassword objForgot;
    VMLoginUser ViewModel;
    IViewDialog viewDialog;
    public bool IsLogOut;
    readonly Dispatcher dispatcher;
    /// <summary>
    /// Application start up event
    /// </summary>
    /// <param name="e"></param>
    /// 
    public App()
    {
      dispatcher = Dispatcher.CurrentDispatcher;
    }
    protected override void OnStartup(StartupEventArgs e)
    {
      // new VMOptimizePolicyManager(null).GetDataTableFromXls("D:\\Test.xls");
      //ActionLogger.Logger.WriteLog("------------------------Application Start--------------------------------------", false);
      AppDomain.CurrentDomain.UnhandledException +=
                    new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
      base.OnStartup(e);

#if DEBUG
      //if (!System.Diagnostics.Debugger.IsAttached)
      //{
      //    System.Diagnostics.Debugger.Launch();
      //}
#endif

      CultureInfo cinfo = new CultureInfo(Thread.CurrentThread.CurrentCulture.Name);
      cinfo.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
      cinfo.DateTimeFormat.DateSeparator = "/";
      Thread.CurrentThread.CurrentCulture = cinfo;

      ViewModel = new VMLoginUser();
      VMInstances.Login = ViewModel;

      objLogin = new Login(ViewModel);

      ViewModel.onOpenWindowClick += new VMLoginUser.OnOpenWindow(ViewModel_onOpenWindowClick);

      ViewModel.RequestClose += objLogin.Close;
      ViewModel.OpenLoginForm += objLogin.Show;
      // add event hamdler for Application  form
      this.Exit += this.Application_Exit;
      
      objLogin.DataContext = ViewModel;
      objLogin.Show();
      CheckAndDownloadNewVersion();
    }
    private void CheckAndDownloadNewVersion()
    {
      
      VMVersionHelper NewVersionChecker = new VMVersionHelper();
      viewDialog = new DownLoadDialog(NewVersionChecker);

      bool newVersionAvaialable = NewVersionChecker.CheckForNewVersion();
      if (newVersionAvaialable)
      {
        MessageBoxResult newVersionAlert = MessageBox.Show("New Version of MyAgencyVault is available. Click OK to Download", "MyAgencyVault", MessageBoxButton.OK);
        if (newVersionAlert == MessageBoxResult.OK)
        {
          DownLoadNewverionOfmyagencyVault downloader = new DownLoadNewverionOfmyagencyVault(NewVersionChecker.DownloadNewVersion);
          IAsyncResult result = downloader.BeginInvoke(new AsyncCallback(CallbackMethod), null);
          viewDialog.Show("PDF");
        }
      }
    }

    void CallbackMethod(IAsyncResult result)
    {
      dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
    {
      viewDialog.Close();
      Application.Current.Shutdown();
    }));
    }

    void ViewModel_onOpenWindowClick()
    {
      //if (objMainWindow != null) return; 
      ViewModel.CloseAplication += new Action(AppLogOut);

      objMainWindow = new MainWindow();
      this.MainWindow = objMainWindow;
      this.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;

      // add property for Login form
      objMainWindow.SetLoginVM = ViewModel;
      ViewModel.RequestOpen += new Action(ViewModel_RequestOpen);
    }

    void ViewModel_RequestOpen()
    {

      objMainWindow.Show();
    }

    void objMainWindow_LogOut()
    {


    }
    /// <summary>
    /// Application exit event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Application_Exit(object sender, EventArgs e)
    {
      if (objLogin != null)
      {
        objLogin.Close();

        if (IsLogOut)
        {
          Process p = new Process();
          p.StartInfo.FileName = Assembly.GetExecutingAssembly().Location;
          p.Start();
        }
      }
    }

    public void AppLogOut()
    {
      Application.Current.Shutdown();
    }

    private void Application_DispatcherUnhandledException(object sender,
                           System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
      if (e.Exception is System.Net.WebException)
      {
        //Handling the exception within the UnhandledException handler.
        MessageBox.Show("Please check the web service is accessible.\n" + e.Exception.Message, "Exception Caught",
                                MessageBoxButton.OK, MessageBoxImage.Error);
      }
      else
      {
        //Handling the exception within the UnhandledException handler.
        MessageBox.Show(e.Exception.Message, "Exception Caught",
                                MessageBoxButton.OK, MessageBoxImage.Error);
      }
      e.Handled = true;

    }

    void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      var ex = e.ExceptionObject as Exception;
      if (ex != null)
        MessageBox.Show(ex.Message, "Uncaught Thread Exception",
                        MessageBoxButton.OK, MessageBoxImage.Error);
    }

  }
}
