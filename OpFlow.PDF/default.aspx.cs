using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using WebSupergoo.ABCpdf11;

namespace OpFlow.PDF
{
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.HttpMethod == HttpMethod.Post.Method)
            {
                string content;

                using (var reader = new StreamReader(Request.InputStream))
                    content = reader.ReadToEnd();

                var traySummary = JsonConvert.DeserializeObject<TraySummary>(content);

                byte[] imageBytes = GenerateSummaryPDF(traySummary);

                Response.ClearHeaders();
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=TraySummary.PDF");
                Response.AddHeader("content-length", imageBytes.Length.ToString());

                Response.BinaryWrite(imageBytes);
                Response.End();
            }
        }

        private static byte[] GenerateSummaryPDF(TraySummary traySummary)
        {

            var memoryStream = new MemoryStream();

            using (Doc pdfDoc = new Doc())
            {
                pdfDoc.HtmlOptions.Engine = EngineType.Gecko;
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

                var imageHtml = $"<div>Tray Name: {traySummary.ProposedTray.TrayName}</div>";
                imageHtml += "<hr />";
                imageHtml += $"<div>Source Trays:</div>";
                foreach (var tray in traySummary.SourceTrays)
                {
                    imageHtml += $"<div>{tray.TrayName}</div>";
                }
                imageHtml += "<hr />";
                imageHtml += "<div>Instruments</div>";
                imageHtml += "<table><thead><tr><th>Instrument</th><th>Quantity</th></tr></thead><tbody>";

                foreach (var instrument in traySummary.Instruments)
                {
                    imageHtml += $"<tr><td>{instrument.InstrumentName}</td><td>{instrument.Quantity}</td></tr>";
                }

                imageHtml += "</tbody></table>";
                imageHtml += "<hr />";
                imageHtml += "<div>Counts</div>";

                imageHtml += "<table><thead><tr><th>Surgeon</th><th>Procedure</th><th>Date</th><th>Used</th></tr></thead><tbody>";

                foreach (var count in traySummary.Counts)
                {
                    imageHtml += $"<tr><td>{count.SurgeonName}</td><td>{count.Procedure} {count.CptCode1} {count.CptCode2} {count.CptCode3}</td><td>{count.ScheduleTime?.ToString("M/d/yyyy")}</td><td>{count.UsedInstruments}</td></tr>";
                }

                imageHtml += "</tbody></table>";
                imageHtml += "<hr />";
                imageHtml += "<div>Audits</div>";

                imageHtml += "<table><thead><tr><th>Auditor</th><th>Surgeon</th><th>Procedure</th><th>Date</th><th>Used</th></tr></thead><tbody>";

                foreach (var audit in traySummary.Audits)
                {
                    imageHtml += $"<tr><td>{audit.AuditUser}</td><td>{audit.SurgeonName}</td><td>{audit.Procedure} {audit.CptCode1} {audit.CptCode2} {audit.CptCode3}</td><td>{audit.ScheduleTime?.ToString("M/d/yyyy")}</td><td>{audit.UsedInstruments}</td></tr>";
                }

                imageHtml += "</tbody></table>";
                imageHtml += "<hr />";

                pdfDoc.AddImageHtml(imageHtml);

                /*pdfDoc.Rect.Left = 40;
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
                auditsTable.NextCell();*/

                pdfDoc.Save(memoryStream);
                pdfDoc.Clear();
            }

            return memoryStream.GetBuffer();
        }

    }
}