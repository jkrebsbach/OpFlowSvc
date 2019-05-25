using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using OpFlow.Data;
using WebSupergoo.ABCpdf11;

namespace OpFlow.Service.DataAccess
{
    public class ImageHelper
    {
        public static byte[] GenerateSummaryPDF(TrayRationalization proposedTray, 
            List<TrayRationalizationItem> instruments, List<TraySurgeryAudit> audits, List<TraySurgeryAudit> counts, List<SourceTraySummary> sourceTrays)
        {
            
            var memoryStream = new MemoryStream();

            using (Doc pdfDoc = new Doc())
            {
                pdfDoc.FontSize = 12;

                var logoPath = HttpContext.Current.Server.MapPath("~/Resources/opflow_logo.png");
                var xImage = XImage.FromFile(logoPath, new XReadOptions());

                pdfDoc.Rect.Left = 200;
                pdfDoc.Rect.Bottom = 700;
                pdfDoc.Rect.Width = 250;
                pdfDoc.Rect.Height = 50;
                pdfDoc.AddImageObject(xImage, true);

                pdfDoc.Rect.Left = 40;
                pdfDoc.Rect.Bottom = 40;
                pdfDoc.Rect.Width = 500;
                pdfDoc.Rect.Height = 650;

                AddLine(pdfDoc, $"Tray Name: {proposedTray.TrayName}");
                AddLine(pdfDoc, "");
                AddLine(pdfDoc, "Source Trays:");

                foreach (var tray in sourceTrays)
                {
                    AddLine(pdfDoc, $"{tray.TrayName}");
                }

                AddLine(pdfDoc, "");
                AddLine(pdfDoc, "Instruments:");

                pdfDoc.Rect.Bottom = 10;
                pdfDoc.Rect.Height = 500;

                var instrumentTable = new PDFTable(pdfDoc, 2);
                instrumentTable.CellPadding = 5;
                instrumentTable.HorizontalAlignment = 1;
                
                var index = 0;
                instrumentTable.NextRow();
                string[] colData = { "Instruments", "Quantity" };
                colData[0] = $"<stylerun hpos=0>{colData[0]}</stylerun>";
                instrumentTable.AddHtml(colData);

                foreach (var instrument in instruments)
                {
                    instrumentTable.NextRow();
                    colData = new [] { instrument.InstrumentName, instrument.Quantity.ToString()  };
                    colData[0] = $"<stylerun hpos=0>{colData[0]}</stylerun>";

                    instrumentTable.AddHtml(colData);

                    if ((index % 2) == 1)
                        instrumentTable.FillRow("220 220 220", index);

                    index++;
                }
                instrumentTable.NextCell();

                // Reset to left align
                pdfDoc.TextStyle.HPos = 0;

                AddLine(pdfDoc, "Counts:");

                var countsTable = new PDFTable(pdfDoc, 4);
                instrumentTable.CellPadding = 5;
                instrumentTable.HorizontalAlignment = 1;

                index = 0;
                countsTable.NextRow();
                colData = new [] { "Surgeon", "Procedure", "Date", "Used" };
                colData[0] = $"<stylerun hpos=0>{colData[0]}</stylerun>";
                countsTable.AddHtml(colData);

                foreach (var count in counts)
                {
                    countsTable.NextRow();
                    colData = new[] { count.SurgeonName, count.CptCode1, count.ScheduleTime?.ToString("M/d/yyyy") ?? "", count.UsedInstruments?.ToString() ?? "" };
                    colData[0] = $"<stylerun hpos=0>{colData[0]}</stylerun>";

                    countsTable.AddHtml(colData);

                    if ((index % 2) == 1)
                        countsTable.FillRow("220 220 220", index);

                    index++;
                }
                countsTable.NextCell();

                // Reset to left align
                pdfDoc.TextStyle.HPos = 0;

                AddLine(pdfDoc, "Audits:");

                var auditsTable = new PDFTable(pdfDoc, 5);
                auditsTable.CellPadding = 5;
                auditsTable.HorizontalAlignment = 1;

                index = 0;
                auditsTable.NextRow();
                colData = new[] { "Auditor", "Surgeon", "Procedure", "Date", "Used" };
                colData[0] = $"<stylerun hpos=0>{colData[0]}</stylerun>";
                auditsTable.AddHtml(colData);

                foreach (var audit in audits)
                {
                    auditsTable.NextRow();
                    colData = new[] { audit.AuditUser, audit.SurgeonName, audit.CptCode1, audit.ScheduleTime?.ToString("M/d/yyyy") ?? "", audit.UsedInstruments?.ToString() ?? "" };
                    colData[0] = $"<stylerun hpos=0>{colData[0]}</stylerun>";

                    auditsTable.AddHtml(colData);

                    if ((index % 2) == 1)
                        auditsTable.FillRow("220 220 220", index);

                    index++;
                }
                auditsTable.NextCell();

                pdfDoc.Save(memoryStream);
                pdfDoc.Clear();
            }

            return memoryStream.GetBuffer();
        }

        private static void AddLine(Doc pdfDoc, string line)
        {
            pdfDoc.AddText($"{line}\r\n");

            if (!(pdfDoc.Pos.Y < 100)) return;
            pdfDoc.AddPage();
            pdfDoc.PageNumber = pdfDoc.PageNumber + 1;

            pdfDoc.Rect.Left = 40;
            pdfDoc.Rect.Bottom = 40;
            pdfDoc.Rect.Width = 500;
            pdfDoc.Rect.Height = 650;
        }
    }
}