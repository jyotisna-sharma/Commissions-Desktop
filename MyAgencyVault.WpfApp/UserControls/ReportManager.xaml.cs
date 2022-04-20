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
using System.Windows.Navigation;
using System.Windows.Shapes;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
using System.IO;

namespace MyAgencyVault.WinApp.UserControls
{
    /// <summary>
    /// Interaction logic for ReportManager.xaml
    /// </summary>
    /// 
    
    
    public partial class ReportManager : UserControl
    {
        //Document pdf = new Document(); 
        public ReportManager()
        {
            InitializeComponent();
        }

        ////private void btnPayeePrint_Click(object sender, RoutedEventArgs e)
        ////{
        ////    //create the demo report pdf file and open it for the user. 
        ////    //
        ////    Random Rndom = new Random();
        ////    int number = Rndom.Next();
        ////    PdfWriter.GetInstance(pdf, new FileStream("..\\..\\Common" + "\\" + number +  ".pdf", FileMode.CreateNew));
        ////    pdf.Open();
        ////    StringBuilder strTmpChat=new StringBuilder();

            
       
            
        ////    PdfdataFillig();
            
        ////    pdf.Close();
        ////    StreamReader StrRdr = new StreamReader("..\\..\\Common" + "\\" + "\\" + number + ".pdf");
        ////    Stream strm = StrRdr.BaseStream;
            
        ////    byte[] b = new byte[(int)strm.Length];
        ////    strm.Read(b, 0, (int)strm.Length);
        ////    strm.Flush(); 
        ////    strm.Close();
            
             
        ////    strm.Dispose(); 
        ////    //File.OpenRead ("..\\..\\Common" + "\\" + "\\" + number + ".pdf");
        ////   System.Diagnostics.Process.Start("..\\..\\Common" + "\\" + "\\" + number + ".pdf");
        ////    ////File.Delete("..\\..\\Common" + "\\" + "\\" + number + ".pdf");
          
        ////}
        ////private void PdfdataFillig()
        ////{

        ////    iTextSharp.text.Paragraph prd = new iTextSharp.text.Paragraph();
        ////    prd.Font.Size = 20;
        ////    prd.Font.Color = iTextSharp.text.Color.RED; 
              
        ////    prd.Add("Check Register                                        ABC License");

        ////    pdf.Add(new iTextSharp.text.Paragraph(prd));
        ////    ////pdf.Add(new iTextSharp.text.Paragraph(")); 
        ////    //This is used for declaring the table
        ////        iTextSharp.text.Table DtUserDetail=new iTextSharp.text.Table(5);         
        ////        //Setting the table width
        ////        DtUserDetail.Width = 110;
        ////        //Setting the Border width of the table
        ////        DtUserDetail.BorderWidth = 0;
        ////        //Binding a cell value inside a table
        ////        iTextSharp.text.Cell cellTable = new Cell("Payor");                
        ////        //This is used for alignment setting of the table
                   
        ////        cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////        cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////        cellTable.BorderWidth = 0;
        ////        cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
            
        ////        //This is used for adding the cell inside the table
        ////        DtUserDetail.AddCell(cellTable);
        ////        //Binding a cell value inside a table
        ////        cellTable = new Cell("Batch #"); 
        ////        //This is used for alignment setting of the table
        ////        cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////        cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////        //set the border of the table
        ////        cellTable.BorderWidth = 0;
        ////        cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
        ////        //This is used for adding the cell inside the table
        ////        DtUserDetail.AddCell(cellTable);
        ////        //Binding a cell value inside a table
        ////        cellTable = new Cell("Statement #");
        ////        cellTable.BorderWidth = 0;
        ////        //This is used for alignment setting of the table
        ////        cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////        cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////        cellTable.SetWidth("60");
        ////        cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
        ////        //This is used for adding the cell inside the table
        ////        DtUserDetail.AddCell(cellTable);
        ////        //Binding a cell value inside a table
        ////        cellTable = new Cell("Date Entered");
        ////        cellTable.BorderWidth = 0;
        ////        //This is used for alignment setting of the table
        ////        cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////        cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////        cellTable.Width = 20;
        ////        cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
        ////        //This is used for adding the cell inside the table
        ////        DtUserDetail.AddCell(cellTable);
        ////       //Binding a cell value inside a table
        ////        cellTable = new Cell("Check Amount");
        ////        cellTable.BorderWidth = 0;
        ////        //This is used for alignment setting of the table
        ////        cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////        cellTable.VerticalAlignment = Element.ALIGN_MIDDLE ;
        ////        cellTable.Width = 20;
        ////        cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;
        ////        //This is used for adding the cell inside the table
        ////        DtUserDetail.AddCell(cellTable);
        ////        //This loop is used for running up to Gridex row count







        ////        cellTable = new Cell("Aetna");
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            cellTable.BorderWidth = 0;
             
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("12345");
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            //set the border of the table
        ////            cellTable.BorderWidth = 0;
                   
        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("2344");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            cellTable.SetWidth("60");
                   
        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("1/1/2010");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////            cellTable.Width = 20;
                 
        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("$   1245.00");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////            cellTable.Width = 20;
               
        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);


        ////            cellTable = new Cell("Oxford");
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            cellTable.BorderWidth = 0;
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("12345");
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            //set the border of the table
        ////            cellTable.BorderWidth = 0;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("34345");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            cellTable.SetWidth("60");

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("1/1/2010");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////            cellTable.Width = 20;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("$   1345.00");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////            cellTable.Width = 20;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);

        ////            cellTable = new Cell("Zsurance");
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            cellTable.BorderWidth = 0;
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("12345");
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            //set the border of the table
        ////            cellTable.BorderWidth = 0;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("34345");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            cellTable.SetWidth("60");

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("1/18/2010");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////            cellTable.Width = 20;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("$   1645.00");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////            cellTable.Width = 20;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);

        ////            cellTable = new Cell("Totals: 6 Checks");
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            cellTable.BorderWidth = 0;
        ////            cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY ;
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("");
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            //set the border of the table
        ////            cellTable.BorderWidth = 0;
        ////            cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_TOP;
        ////            cellTable.SetWidth("60");
        ////            cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////            cellTable.Width = 20;
        ////            cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY ;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);
        ////            //Binding a cell value inside a table
        ////            cellTable = new Cell("$   9345.00");
        ////            cellTable.BorderWidth = 0;
        ////            //This is used for alignment setting of the table
        ////            cellTable.HorizontalAlignment = Element.ALIGN_CENTER;
        ////            cellTable.VerticalAlignment = Element.ALIGN_MIDDLE;
        ////            cellTable.Width = 20;
        ////            cellTable.BackgroundColor = iTextSharp.text.Color.LIGHT_GRAY;

        ////            //This is used for adding the cell inside the table
        ////            DtUserDetail.AddCell(cellTable);

        ////         pdf.Add(DtUserDetail);

        ////}

        

    }
}
