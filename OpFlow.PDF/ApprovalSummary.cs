using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebSupergoo.ABCpdf11;

namespace OpFlow.PDF
{
    public class ApprovalSummary
    {
        public static byte[] GenerateSummaryPDF(TraySummary traySummary, string logoPath)
        {

            var memoryStream = new MemoryStream();

            using (Doc pdfDoc = new Doc())
            {
                pdfDoc.HtmlOptions.Engine = EngineType.Gecko;
                pdfDoc.FontSize = 12;

                var xImage = XImage.FromFile(logoPath, new XReadOptions());

                pdfDoc.Rect.Left = 200;
                pdfDoc.Rect.Bottom = 600;
                pdfDoc.Rect.Width = 250;
                pdfDoc.Rect.Height = 250;
                pdfDoc.AddImageObject(xImage, true);

                pdfDoc.Rect.Left = 40;
                pdfDoc.Rect.Bottom = 40;
                pdfDoc.Rect.Width = 550;
                pdfDoc.Rect.Height = 650;

                var imageHtml = $"<div>Tray Name: {traySummary.ProposedTray.TrayName} - Total Instruments: {traySummary.ProposedTray.InstrumentCount}</div>";
                imageHtml += "<hr />";
                imageHtml += $"<div>Source Trays:</div>";
                foreach (var tray in traySummary.SourceTrays)
                {
                    imageHtml += $"<div>{tray.TrayName} - Total Instruments: {tray.InstrumentCount} Decrease: {tray.CountChange} ({tray.PcntChange:#.00} %)</div>";
                }

                imageHtml += "<hr />";
                imageHtml += "<div>Source Tray Instruments</div>";

                imageHtml += "<table><thead><tr><th>Tray</th><th>Instrument</th><th>Quantity</th></tr></thead><tbody>";

                foreach (var tray in traySummary.SourceTrays)
                {
                    foreach (var instrument in tray.Instruments)
                    {
                        imageHtml += $"<tr><td>{instrument.TrayName}</td><td>{instrument.InstrumentName}</td><td>{instrument.Quantity}</td></tr>";
                    }
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

                imageHtml += "<table><thead><tr><th>Auditor</th><th>Surgeon</th><th>Procedure</th><th>Date</th><th>Used</th><th>Comments</th></tr></thead><tbody>";

                foreach (var audit in traySummary.Audits)
                {
                    imageHtml += $"<tr><td>{audit.AuditUser}</td><td>{audit.SurgeonName}</td><td>{audit.Procedure} {audit.CptCode1} {audit.CptCode2} {audit.CptCode3}</td><td>{audit.ScheduleTime?.ToString("M/d/yyyy")}</td><td>{audit.UsedInstruments}</td><td>{audit.AuditComments}</td></tr>";
                }

                imageHtml += "</tbody></table>";
                imageHtml += "<hr />";

                imageHtml += "<div>Configuration</div>";
                imageHtml += "<table><thead><tr><th>Seq</th><th>Instrument</th><th>Source Tray</th><th>Source Qty</th><th>Avg Qty Used</th><th>Reason Code</th><th>Proposed Quantity</th><th>Comments</th></tr></thead><tbody>";

                foreach (var instrument in traySummary.Instruments)
                {
                    string reason = string.Empty;
                    switch (instrument.Reason)
                    {
                        case "U":
                            reason = "Usage";
                            break;
                        case "S":
                            reason = "Safety";
                            break;
                        case "B":
                            reason = "Buffer";
                            break;
                    }

                    imageHtml += $"<tr><td>{instrument.Sequence}</td><td>{instrument.InstrumentName}</td><td>{instrument.TrayName}</td><td>{instrument.SourceQty}</td><td>{instrument.AvgUsed}</td><td>{reason}</td><td>{instrument.Quantity}</td><td>{instrument.Comments}</td></tr>";
                }

                imageHtml += "</tbody></table>";
                imageHtml += "<hr />";

                imageHtml += "<div>Cards</div>";
                imageHtml += "<table><thead><tr><th>Service Line</th><th>Card</th><th>Surgeon</th><th>Tray</th></tr></thead><tbody>";

                foreach (var card in traySummary.Cards)
                {
                    imageHtml += $"<tr><td>{card.SpecialtyName}</td><td>{card.CardDescription}</td><td>{card.SurgeonName}</td><td>{card.TrayName}</td></tr>";
                }

                imageHtml += "</tbody></table>";

                imageHtml += "<br />";
                imageHtml += "<br />";


                imageHtml += "<table style=\"width:100%\"><thead><tr><th style=\"width:33%\"><hr /></th><th style=\"width:33%\"><hr /></th><th style=\"width:33%\"><hr /></th></th></tr>";
                imageHtml += "<tr><th>Lead</th><th>Signature</th><th>Date</th></tr><tr><td>&nbsp;</td></tr><tr><td>&nbsp;</td></tr>";

                imageHtml += "<tr><th><hr /></th><th><hr /></th><th><hr /></th></th></tr>";
                imageHtml += "<tr><th>Surgeon</th><th>Signature</th><th>Date</th></tr><tr><td>&nbsp;</td></tr><tr><td>&nbsp;</td></tr>";

                imageHtml += "<tr><th><hr /></th><th><hr /></th><th><hr /></th></th></tr>";
                imageHtml += "<tr><th>Administration Delegate</th><th>Signature</th><th>Date</th></tr><tr></tr></thead></table>";

                var chainId = pdfDoc.AddImageHtml(imageHtml);
                //pdfDoc.AddImageHtml(imageHtml, true, 700, false);

                while (true)
                {
                    //pdfDoc.FrameRect();
                    if (!pdfDoc.Chainable(chainId))
                        break;

                    pdfDoc.Page = pdfDoc.AddPage();
                    chainId = pdfDoc.AddImageToChain(chainId);
                }

                pdfDoc.Save(memoryStream);
                pdfDoc.Clear();
            }

            return memoryStream.GetBuffer();
        }
    }
}