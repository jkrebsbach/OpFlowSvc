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
        public static byte[] GetReport(string reportName, Dictionary<string, DataTable> datasets, ReportParameter[] parameters = null)
        {
            return GetReport(reportName, "IMAGE", datasets, parameters);
        }

        public static byte[] GetReport(string reportName, string format, Dictionary<string, DataTable> datasets, ReportParameter[] parameters = null)
        {
            var viewer = new ReportViewer();
            viewer.LocalReport.Refresh();
            viewer.LocalReport.ReportPath = $"Resources/{reportName}.rdlc"; //This is your rdlc name.

            if (parameters != null)
                viewer.LocalReport.SetParameters(parameters);

            foreach (var dataset in datasets.Keys)
            {
                viewer.LocalReport.DataSources.Add(new ReportDataSource(dataset, datasets[dataset])); 
            }

            return viewer.LocalReport.Render(format, null, out var mimeType, out var encoding, out var filenameExtension, out var streamids, out var warnings);
        }
    }
}