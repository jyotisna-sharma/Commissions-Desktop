using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MyAgencyVault.VM.VMLib;
using MyAgencyVault.ViewModel;

namespace MyAgencyVault.WinApp
{
  /// <summary>
  /// Interaction logic for DownLoadDialog.xaml
  /// </summary>
  public partial class DownLoadDialog : Window, IViewDialog
  {
    public DownLoadDialog(VMVersionHelper versionHelper)
    {
      InitializeComponent();
      this.DataContext = versionHelper;

    }

    void IViewDialog.ShowCustomMsg(string strFomat)
    {
        this.ShowDialog();
    }
    void IViewDialog.Show(string strFomat)
    {
      this.ShowDialog();
    }

    void IViewDialog.Close()
    {
      this.Hide();
    }
  }
}
