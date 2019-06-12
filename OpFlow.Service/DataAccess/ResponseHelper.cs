using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace OpFlow.Service.DataAccess
{
    public abstract class ResponseHelper
    {
        public static HttpResponseMessage PdfResponse(string json)
        {

            var opflowPdf = ConfigurationManager.AppSettings["OpFlowPDF"];
            var request = (HttpWebRequest)WebRequest.Create(opflowPdf);
            request.ContentType = "application/json";
            request.Method = HttpMethod.Post.Method;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            byte[] buffer = new byte[1024];
            long received = 0;
            var memStream = new MemoryStream();
            var httpResponse = (HttpWebResponse)request.GetResponse();
            using (var input = httpResponse.GetResponseStream())
            {
                long size = input.Read(buffer, 0, buffer.Length);
                while (size > 0)
                {
                    memStream.Write(buffer, 0, (int)size);
                    received += size;

                    size = input.Read(buffer, 0, buffer.Length);
                }
            }

            return PdfResponse(memStream);
        }

        public static HttpResponseMessage PdfResponse(byte[] pdfBytes)
        {
            var memStream = new MemoryStream(pdfBytes);

            return PdfResponse(memStream);
        }


        private static HttpResponseMessage PdfResponse(Stream memStream)
        {
            memStream.Position = 0;

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                    { FileName = "TrayRationalization.pdf", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }
    }
}