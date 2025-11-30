using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Reporting.NETCore;
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
            var localReport = new LocalReport();
            localReport.Refresh();
            localReport.ReportPath = $"Resources/{reportName}.rdlc"; //This is your rdlc name.

            if (parameters != null)
                localReport.SetParameters(parameters);

            foreach (var dataset in datasets.Keys)
            {
                localReport.DataSources.Add(new ReportDataSource(dataset, datasets[dataset])); 
            }

            try
            {
                return localReport.Render(format, null, out _, out _, out _, out _, out _);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}