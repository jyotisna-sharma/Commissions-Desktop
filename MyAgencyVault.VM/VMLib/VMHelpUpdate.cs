using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyAgencyVault.VM.MyAgencyVaultSvc;
using MyAgencyVault.ViewModel.CommonItems;
using System.ComponentModel;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using MyAgencyVault.VM.BaseVM;
using System.Workflow.ComponentModel;
using System.IO;
using MyAgencyVault.VM;
using MyAgencyVault.VM.CommonItems;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows;
using MyAgencyVault.ViewModel.Converters;


namespace MyAgencyVault.ViewModel.VMLib
{
  public class VMHelpUpdate : BaseViewModel
  {

    #region "Delegate And Event"

    public delegate void OnPopUpHelpUpdate(bool isEditable);
    public event OnPopUpHelpUpdate OpenDialog;
    public delegate void OnClosePopUpHelpUpdate();
    public event OnClosePopUpHelpUpdate CloseDialog;

    #endregion

    #region Comm Dept Support

    private ComDeptSupport ComDeptSupportObject;

    private ObservableCollection<ComDeptSupportData> _SupportFiles;
    public ObservableCollection<ComDeptSupportData> SupportFiles
    {
      get { return _SupportFiles; }
      set
      {
        _SupportFiles = value;
        OnPropertyChanged("SupportFiles");
      }
    }

    private ComDeptSupportData _CurrentSupportFile;
    public ComDeptSupportData CurrentSupportFile
    {
      get { return _CurrentSupportFile; }
      set
      {
        _CurrentSupportFile = value;
        OnPropertyChanged("CurrentSupportFile");
      }
    }

    private ICommand _SupportFileClicked;
    public ICommand SupportFileClicked
    {
      get
      {
        if (_SupportFileClicked == null)
        {
          _SupportFileClicked = new BaseCommand(p => BeforeOnSupportFileClicked(), p => OnSupportFileClicked(p));
        }
        return _SupportFileClicked;
      }

    }


    private ICommand _ViewNewsClick;
    public ICommand ViewNewsClick
    {
      get
      {
        if (_ViewNewsClick == null)
        {
          _ViewNewsClick = new BaseCommand(p => BeforeOnViewNewsClicked(), p => OnViewNewsClicked(p));
        }
        return _ViewNewsClick;
      }

    }

    private bool BeforeOnViewNewsClicked()
    {

      if (RoleManager.UserAccessPermission(MasterModule.HelpUpdate) == ModuleAccessRight.Read)
        return false;
      else
        return true;
    }

    private void OnViewNewsClicked(object value)
    {
      News news = value as News;

      if (news != null)
      {
        DoOpen(false);
      }
    }

    private WebDevPath ObjWebDevPath;


    private bool BeforeOnSupportFileClicked()
    {
      if (CurrentSupportFile == null)
        return false;

      if (RoleManager.UserAccessPermission(MasterModule.HelpUpdate) == ModuleAccessRight.Read)
        return false;
      else
        return true;
    }

    private void OnSupportFileClicked(object value)
    {
      ComDeptSupportData data = value as ComDeptSupportData;
      FileUtility ObjDownload = FileUtility.CreateClient(ObjWebDevPath.URL, ObjWebDevPath.UserName, ObjWebDevPath.Password, ObjWebDevPath.DomainName);

      ObjDownload.DownloadComplete += new DownloadCompleteDel(ObjDownload_DownloadComplete);
      string RemotePath = "/Support/" + data.FileName + data.FileType;
      ObjDownload.Download(RemotePath, Path.Combine(Path.GetTempPath(), data.FileName + data.FileType));
    }

    void ObjDownload_DownloadComplete(int statusCode, string localFilePath)
    {
      string status = statusCode.ToString();
      if (status.StartsWith("20"))
      {
        //MessageBox.Show("File named " + localFilePath + " is downloaded successfully.");
        Process.Start(localFilePath);
      }
      else
      {
        System.Windows.MessageBox.Show("Problem with downloading file named " + localFilePath + ". Please try again.");
      }
    }

    /// <summary>
    /// Adding News Panel is visible or not.
    /// </summary>
    private Visibility _VisibleAddNews;
    public Visibility VisibleAddNews
    {
      get { return _VisibleAddNews; }
      set { _VisibleAddNews = value; OnPropertyChanged("VisibleAddNews"); }
    }

    /// <summary>
    /// View News Panel is visible or not.
    /// </summary>
    private Visibility _VisibleViewNews;
    public Visibility VisibleViewNews
    {
      get { return _VisibleViewNews; }
      set { _VisibleViewNews = value; OnPropertyChanged("VisibleViewNews"); }
    }

    #endregion

    #region "Local Variable And Command"

    public ICommand _SaveClick = null;
    public ICommand _CancelClick = null;
    public ICommand _DeleteClick = null;
    public ICommand _closeCommand = null;
    public ICommand _OpenCommand = null;


    ObservableCollection<News> _newsDetails = null;
    News _currentNews = null;

    #endregion

    public VMHelpUpdate()
    {
      using (ServiceClients serviceClients = new ServiceClients())
      {
        string KeyValue = serviceClients.MasterClient.GetSystemConstantKeyValue("WebDevPath");
        ObjWebDevPath = WebDevPath.GetWebDevPath(KeyValue);

        this.PropertyChanged += new PropertyChangedEventHandler(VMHelpUpdate_PropertyChanged);
        HelpUpdateScreenControl();

        ComDeptSupportObject = new ComDeptSupport();
        SupportFiles = serviceClients.ComDeptSupportClient.getSupportFiles(ComDeptSupportObject);

        if (SupportFiles != null && SupportFiles.Count != 0)
          CurrentSupportFile = SupportFiles[0];
      }
    }
    #region ControlLevelProerty
    #region SettingScreen
    private void HelpUpdateScreenControl()
    {
      if (RoleManager.Role == UserRole.Agent)
      {
        NewButtonEnable = false;
        DeleteButtonEnable = false;
        VisibleAddNews = Visibility.Hidden;
        VisibleViewNews = Visibility.Visible;
        try
        {
          if (RoleManager.UserPermissions[(int)MasterModule.Settings - 1].Permission == ModuleAccessRight.Read)
          {
            NewButtonEnable = false;
            DeleteButtonEnable = false;
          }
        }
        catch
        {

        }
      }
      else if (RoleManager.Role == UserRole.Administrator)
      {
        VisibleAddNews = Visibility.Hidden;
        VisibleViewNews = Visibility.Visible;

        NewButtonEnable = false;
        DeleteButtonEnable = false;
      }
      else if (RoleManager.Role == UserRole.DEP)
      {
        VisibleAddNews = Visibility.Hidden;
        VisibleViewNews = Visibility.Visible;

        NewButtonEnable = false;
        DeleteButtonEnable = false;
      }
      else if (RoleManager.Role == UserRole.HO)
      {
        VisibleAddNews = Visibility.Hidden;
        VisibleViewNews = Visibility.Visible;

        NewButtonEnable = false;
        DeleteButtonEnable = false;
      }
      else if (RoleManager.Role == UserRole.SuperAdmin)
      {
        VisibleAddNews = Visibility.Visible;
        VisibleViewNews = Visibility.Hidden;

        NewButtonEnable = true;
        DeleteButtonEnable = true;
      }

    }

    private bool newbuttonenable = false;
    public bool NewButtonEnable
    {
      get
      {
        return newbuttonenable;
      }
      set
      {
        newbuttonenable = value;
        OnPropertyChanged("NewButtonEnable");
      }

    }
    private bool deletebuttonenable = false;
    public bool DeleteButtonEnable
    {
      get
      {
        return deletebuttonenable;
      }
      set
      {
        deletebuttonenable = value;
        OnPropertyChanged("DeleteButtonEnable");
      }
    }


    #endregion
    #endregion

    void VMHelpUpdate_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case "CurrentNews":
          break;
        default:
          break;
      }
    }

    #region "Public Properties"

    public ObservableCollection<News> NewsDetails
    {
      get
      {
        if (_newsDetails == null)
        {
          using (ServiceClients serviceClients = new ServiceClients())
          {
            //new change by vinod 
            //Sorting by Last update date
            _newsDetails = new ObservableCollection<News>(serviceClients.NewsClient.GetNewsList().OrderByDescending(n => n.LastModifiedOn).ToList());
            //select first new by default
            if (_newsDetails.Count > 0)
            {
              CurrentNews = _newsDetails.FirstOrDefault();
            }
          }
        }
        return _newsDetails;
      }
      set
      {
        _newsDetails = value;
        OnPropertyChanged("NewsDetails");
      }
    }



    public News CurrentNews
    {
      get
      {
        return _currentNews;
      }
      set
      {
        _currentNews = value;
        OnPropertyChanged("CurrentNews");
      }
    }

    /// <summary>
    /// command property for cancel
    /// </summary>
    public ICommand CancelClick
    {
      get
      {
        if (_CancelClick == null)
        {
          _CancelClick = new BaseCommand(param => CanCancel(), param => DoCancel());
        }
        return _CancelClick;
      }

    }

    /// <summary>
    /// command for save click
    /// </summary>
    public ICommand SaveClick
    {
      get
      {
        if (_SaveClick == null)
        {
          _SaveClick = new BaseCommand(param => CanSave(), param => DoSave());
        }
        return _SaveClick;
      }

    }

    public event Action RequestOpen;
    public ICommand OpenCommand
    {
      get
      {
        if (_closeCommand == null)
        {
          _OpenCommand = new BaseCommand(
               param => CanOpen(),
             param => DoOpen(true)
        );
        }
        return _OpenCommand;
      }
    }


    /// <summary>
    /// check for Open Main Form
    /// </summary>
    /// <returns></returns>
    private bool CanOpen()
    {
      if (RoleManager.UserAccessPermission(MasterModule.HelpUpdate) == ModuleAccessRight.Read)
        return false;
      else
        return true;
    }
    /// <summary>
    /// Do processes for Open Main Form
    /// </summary>
    /// <returns></returns>
    public void DoOpen(bool isEditable)
    {
      //means that super user is adding the content...
      if (isEditable)
        InitilizeData();

      if (OpenDialog != null)
      {
        OpenDialog(isEditable);
      }
    }

    /// <summary>
    /// close the form manualy
    /// </summary>
    public event Action RequestClose;
    public ICommand CloseCommand
    {
      get
      {
        if (_closeCommand == null)
        {
          _closeCommand = new BaseCommand(param => CanClose(),
             param => Close()
        );
        }
        return _closeCommand;
      }
    }


    public void EditHelpUpdate()
    {
      if (OpenDialog != null)
      {
        OpenDialog(true);
      }
    }


    /// <summary>
    /// command property for delete news
    /// </summary>
    public ICommand DeleteClick
    {
      get
      {
        if (_DeleteClick == null)
        {
          _DeleteClick = new BaseCommand(param => CanDelete(), param => DoDelete());
        }
        return _DeleteClick;
      }
    }

    #endregion

    #region "Method"

    /// <summary>
    /// check for Save details
    /// </summary>
    /// <returns></returns>
    public bool CanSave()
    {
      try
      {
        if (CurrentNews.NewsTitle == null)
        {
          return false;
        }
        else
        {
          if (CurrentNews.NewsTitle.Trim() == string.Empty)
            return false;
          else
            return true;
        }
      }
      catch
      {
        return false;
      }
    }


    /// <summary>
    /// Do processes for Save details
    /// </summary>
    public void DoSave()
    {
      if (CurrentNews.NewsTitle == null) return;

      try
      {
        using (ServiceClients serviceClients = new ServiceClients())
        {
          if (string.IsNullOrEmpty(CurrentNews.NewsTitle.Trim())) return;

          serviceClients.NewsClient.AddUpdateNews(CurrentNews);

          RTFToTextConverter converter = new RTFToTextConverter();
          CurrentNews.SimpleNewsContent = converter.Convert(CurrentNews.NewsContent);

          TurnOnNewsToFlashBit();

          if (!NewsDetails.Contains(CurrentNews))
            NewsDetails.Add(CurrentNews);

          //Sorting by Added date
          NewsDetails = new ObservableCollection<News>(NewsDetails.OrderByDescending(o => o.LastModifiedOn).ToList());
          //select if new is availble here
          if (NewsDetails.Count > 0)
            CurrentNews = NewsDetails.LastOrDefault();

          if (CloseDialog != null)
            CloseDialog();
        }
      }
      catch (Exception)
      {
      }


    }

    private void TurnOnNewsToFlashBit()
    {
      using (ServiceClients serviceClients = new ServiceClients())
      {
        serviceClients.UserClient.TurnOnNewsToFlashBit();
      }
    }

    void InitilizeData()
    {
      CurrentNews = new News();
      CurrentNews.NewsID = Guid.NewGuid();
      CurrentNews.CreatedOn = DateTime.Now.Date;
      CurrentNews.LastModifiedOn = DateTime.Now.Date;
    }


    /// <summary>
    /// check for Save details
    /// </summary>
    /// <returns></returns>
    public bool CanCancel()
    {
      return true;
    }


    private bool CanClose()
    {
      return true;
    }

    /// <summary>
    /// Do processes for close login Form
    /// </summary>
    /// <returns></returns>
    public void Close()
    {
      if (RequestClose != null)
      {
        RequestClose();
      }
    }

    /// <summary>
    /// Do processes for Save details
    /// </summary>
    public void DoCancel()
    {
      //if (CloseDialog != null)
      //    CloseDialog();           
      CurrentNews = null;
      if (CloseDialog != null)
      {   //select if new is availble here
        if (NewsDetails.Count > 0)
        {
          CurrentNews = NewsDetails.LastOrDefault();
        }
        CloseDialog();
      }
    }

    /// <summary>
    /// check for delete details
    /// </summary>
    /// <returns></returns>
    public bool CanDelete()
    {
      #region"Enable and disable delete button"
      if (CurrentNews == null)
        DeleteButtonEnable = false;
      else
        DeleteButtonEnable = true;

      #endregion

      if (CurrentNews != null)
      {
        //if (CurrentNews.NewsContent == null) return false;
        if (CurrentNews.NewsTitle == null) return false;
      }

      if (RoleManager.UserAccessPermission(MasterModule.HelpUpdate) == ModuleAccessRight.Read)
        return false;
      else
        return true;
    }


    /// <summary>
    /// Do processes for Save details
    /// </summary>
    public void DoDelete()
    {
      try
      {
        using (ServiceClients serviceClients = new ServiceClients())
        {
          //check all the null before go to delete
          //delete Selected New if new title is available
          if (CurrentNews == null || string.IsNullOrEmpty(CurrentNews.NewsTitle)) return;

          MessageBoxResult _MessageBoxResult = System.Windows.MessageBox.Show("Are you sure.", "Delete Warning", MessageBoxButton.YesNo);
          if (_MessageBoxResult == MessageBoxResult.No)
          {
            return;
          }
          serviceClients.NewsClient.DeleteNews(CurrentNews);
          NewsDetails.Remove(CurrentNews);

          if (NewsDetails.Count > 0)
          {
            CurrentNews = NewsDetails.FirstOrDefault();
          }
        }
      }
      catch
      { }
    }

    #endregion
  }
}
