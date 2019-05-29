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
                XSettings.InstallLicense(Licensing.ABCPDF);

                string content;

                using (var reader = new StreamReader(Request.InputStream))
                    content = reader.ReadToEnd();

                var traySummary = JsonConvert.DeserializeObject<TraySummary>(content);

                var logoPath = HttpContext.Current.Server.MapPath("~/Resources/opflow_logo.png");
                byte[] imageBytes = ApprovalSummary.GenerateSummaryPDF(traySummary, logoPath);

                Response.ClearHeaders();
                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=TraySummary.PDF");
                Response.AddHeader("content-length", imageBytes.Length.ToString());

                Response.BinaryWrite(imageBytes);
                Response.End();
            }
        }


    }
}