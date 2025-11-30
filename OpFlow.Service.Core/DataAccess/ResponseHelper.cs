using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using OpFlow.Data;

namespace OpFlow.Service.DataAccess
{
    public abstract class ResponseHelper
    {
        public static HttpResponseMessage PdfResponse(string json)
        {
            var opflowPdf = "http://opflowpdftest.cloudapp.net/default.aspx";
            //var opflowPdf = ConfigurationManager.AppSettings["OpFlowPDF"];
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
            try
            {
                var httpResponse = (HttpWebResponse) request.GetResponse();
                using (var input = httpResponse.GetResponseStream())
                {
                    long size = input.Read(buffer, 0, buffer.Length);
                    while (size > 0)
                    {
                        memStream.Write(buffer, 0, (int) size);
                        received += size;

                        size = input.Read(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (WebException webEx)
            {
                WebResponse errResp = webEx.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                var tmpInt = 0;
            }

            return PdfResponse(memStream);
        }

        public static CompositeSummary CompositeImageResponse(object summary, List<byte[]> pngResult)
        {
            return new CompositeSummary()
            {
                    Summary = summary,
                    Images = pngResult?.Select(img => new SecureImage()
                    {
                        DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(img)
                    }).ToList()
                };
        }

        public static List<SecureImage> ImageResponse(List<byte[]> pngResult)
        {
            return pngResult.Select(img => new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(img)
                }).ToList();
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
        public static HttpResponseMessage ExcelResponse(byte[] excelData)
        {
            var memStream = new MemoryStream(excelData);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                { FileName = "CardListExport.xlsx", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }
    }
}