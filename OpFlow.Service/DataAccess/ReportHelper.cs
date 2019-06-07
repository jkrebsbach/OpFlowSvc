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
        public static byte[] GetReport(string reportName, DataSet analytics, ReportParameter[] parameters = null)
        {
            Warning[] warnings;
            string[] streamids;
            string mimeType;
            string encoding;
            string filenameExtension;

            ReportDataSource rdsAct = new ReportDataSource(reportName, analytics.Tables[0]);
            ReportViewer viewer = new ReportViewer();
            viewer.LocalReport.Refresh();
            viewer.LocalReport.ReportPath = $"Resources/{reportName}.rdlc"; //This is your rdlc name.

            if (parameters != null)
                viewer.LocalReport.SetParameters(parameters);

            viewer.LocalReport.DataSources.Add(rdsAct); // Add  datasource here         
            byte[] bytes = viewer.LocalReport.Render("Image", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
            
            return bytes;
        }
    }
}