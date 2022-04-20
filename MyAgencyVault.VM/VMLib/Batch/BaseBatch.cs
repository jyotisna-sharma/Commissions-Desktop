using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using iTextSharp.text.pdf;
using MyAgencyVault.VM.CommonItems;

namespace MyAgencyVault.ViewModel.CommonBatch
{
    class BaseBatch
    {
        private void AddWaterMark(String sourceFile, String outputFile, String watermarkText)
        {
            try
            {
                PdfReader reader = new PdfReader(sourceFile);
                int pageCount = reader.NumberOfPages;
                PdfStamper stamper = new PdfStamper(reader, new System.IO.FileStream(outputFile, FileMode.Create));
                PdfContentByte underContent = null;

                iTextSharp.text.Rectangle rect = reader.GetPageSizeWithRotation(1);

                Single watermarkFontSize = 24;
                Single watermarkFontOpacity = 0.1f;
                Single watermarkRotation = 0f;
                iTextSharp.text.pdf.BaseFont watermarkFont;
                iTextSharp.text.Color watermarkFontColor;
                
                watermarkFont = iTextSharp.text.pdf.BaseFont.CreateFont(iTextSharp.text.pdf.BaseFont.HELVETICA,
                                                             iTextSharp.text.pdf.BaseFont.CP1252,
                                                            iTextSharp.text.pdf.BaseFont.NOT_EMBEDDED);
                watermarkFontColor = iTextSharp.text.Color.BLACK;
                iTextSharp.text.pdf.PdfGState gstate = new iTextSharp.text.pdf.PdfGState();
                
                gstate.FillOpacity = watermarkFontOpacity;
                gstate.StrokeOpacity = watermarkFontOpacity;

                for (int intCount = 1; intCount <= pageCount; intCount++)
                {
                    underContent = stamper.GetUnderContent(intCount);
                    underContent.SaveState();
                    underContent.SetGState(gstate);
                    underContent.SetColorFill(watermarkFontColor);
                    underContent.BeginText();
                    underContent.SetFontAndSize(watermarkFont, watermarkFontSize);
                    underContent.SetTextMatrix(10, 10);
                    underContent.ShowTextAligned(iTextSharp.text.Element.ALIGN_CENTER, watermarkText, rect.Width / 2, 10, watermarkRotation);
                    underContent.EndText();
                    underContent.RestoreState();

                }

                stamper.Close();
                reader.Close();
            }
            catch
            {

            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName">
        /// This is local batch file that needs to be updated to the server.
        /// </param>
        /// <param name="AgencyName">
        /// This is the Agency Name.
        /// </param>
        public string CreateWaterMarkFile(string fileName, string AgencyName)
        {

            try
            {
                FileStream fs;
                string RemotefileName = Path.Combine(ApplicationAgencyVault.ApplicationDataDirectory(), Path.GetFileName(fileName));
                fs = new FileStream(RemotefileName, FileMode.Create);
                fs.Close();

                AddWaterMark(fileName, RemotefileName, AgencyName);
                return RemotefileName;
            }
            catch 
            {
                return string.Empty;
            }
        }
    }
}
