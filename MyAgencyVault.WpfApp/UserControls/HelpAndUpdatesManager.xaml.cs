using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyAgencyVault.ViewModel;
using MyAgencyVault.WinApp.Common;

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for HelpAndUpdatesManager.xaml
    /// </summary>
    public partial class HelpAndUpdatesManager : UserControl
    {
        //HelpUpdateVM objHelpUpdateVM;
        public HelpAndUpdatesManager()
        {
            InitializeComponent();
           // objHelpUpdateVM = objHelpUpdate;

        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            //objHelpUpdateVM.EditHelpUpdate();

        }

        private void HyperlinkFile_Click(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start(objHelpUpdateVM.CurrHelpUpdateViedo.FilePath);

        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            

        }

        #region "Commented code"

        private DataTable _dtVideos;
        //private DataTable getVideos()
        //
        //    DataTable dt = new DataTable();
        //    dt.Columns.Add("VideoFileName");            
        //    dt.Columns.Add("Context");
        //    dt.Columns.Add("Added");
        //    DataRow dr;

        //    dr = dt.NewRow();
        //    dr["VideoFileName"] = "how do i video";
        //    dr["Context"] = "It shows, various acheivable functionalities in the application";
        //    dr["Added"] = "12/12/2009";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["VideoFileName"] = "how to manage people Video";
        //    dr["Context"] = "It shows how to manage people Video";
        //    dr["Added"] = "12/12/2009";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["VideoFileName"] = "how to make entry for downloaded statements";
        //    dr["Context"] = "It shows how to make entry for downloaded statements";
        //    dr["Added"] = "12/1/2010";
        //    dt.Rows.Add(dr);

        //    dr = dt.NewRow();
        //    dr["VideoFileName"] = "how to create new payor and manage its settings";
        //    dr["Context"] = "It shows how to create new payor and manage its settings.";
        //    dr["Added"] = "2/2/2010";
        //    dt.Rows.Add(dr);

        //    return dt;
        //}
        //private DataTable DtVideos
        //{
        //    get
        //    {
        //        if (_dtVideos == null)
        //            _dtVideos = getVideos();
        //        return _dtVideos;
        //    }
        //}
        private DataTable _dtNews;
        private DataTable getNews()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("NewsId");
            dt.Columns.Add("NewsTitle");
            dt.Columns.Add("LastUpdated");
            dt.Columns.Add("Content");
            DataRow dr;

            dr = dt.NewRow();
            dr["NewsId"] = "12345";
            dr["NewsTitle"] = "Download Manager now contains help texts";
            dr["LastUpdated"] = "12/12/2009";
            dr["Content"] = "Download Manager now contains help texts. You...";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["NewsId"] = "12346";
            dr["NewsTitle"] = "Make your statements to be downloaded automatically on its schedule";
            dr["LastUpdated"] = "12/12/2009";
            dr["Content"] = "Now statements / batch download can be automated. you need to do only some ....";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["NewsId"] = "12347";
            dr["NewsTitle"] = "Download Manager now contains help texts";
            dr["LastUpdated"] = "1/1/2010";
            dr["Content"] = "Download Manager now contains help texts. You...";
            dt.Rows.Add(dr);

            dr = dt.NewRow();
            dr["NewsId"] = "12348";
            dr["NewsTitle"] = "Make your statements to be downloaded automatically on its schedule";
            dr["LastUpdated"] = "2/2/2010";
            dr["Content"] = "Now statements / batch download can be automated. you need to do only some ....";
            dt.Rows.Add(dr);
            return dt;
        }
        private DataTable DtNews
        {
            get
            {
                if (_dtNews == null)
                    _dtNews = getNews();
                return _dtNews;
            }
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //fill the video tutorial grid
            //this.dgVideos.ItemsSource = objHelpUpdate.HelpUpdateDatails;

            //fill the news /updates grid
            //   this.dgNewsOrUpdates.ItemsSource = objHelpUpdate.HelpUpdateDatails.GetAllHelpUpdateData;

        }

        private void dgNewsOrUpdates_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Window w = new NewsDetailViewer(MyAgencyVault.WinApp.Common.Mode.Edit);
            //w.ShowDialog();
        }
        #endregion
    }
}
