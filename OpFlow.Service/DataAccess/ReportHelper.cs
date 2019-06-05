using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Reporting.WebForms;
using OpFlow.Data;

namespace OpFlow.Service.DataAccess
{
    public class ReportHelper
    {
        public static byte[] GetTrayRationalization(string reportName, DataSet analytics)
        {
            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string filenameExtension;

            ReportDataSource rdsAct = new ReportDataSource("TrayRationalization", analytics.Tables[0]);
            ReportViewer viewer = new ReportViewer();
            viewer.LocalReport.Refresh();
            viewer.LocalReport.ReportPath = $"Resources/{reportName}.rdlc"; //This is your rdlc name.
            //viewer.LocalReport.SetParameters(param);
            viewer.LocalReport.DataSources.Add(rdsAct); // Add  datasource here         
            byte[] bytes = viewer.LocalReport.Render("Image", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
            
            return bytes;
        }

        public static string GetTrayRationalizationBase64(string reportName, DataSet analytics)
        {
            var bytes = GetTrayRationalization(reportName, analytics);

            return Convert.ToBase64String(bytes);
        }
    }
}