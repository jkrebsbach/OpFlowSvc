using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OpFlow.Service.DataAccess
{
    public abstract class EmailHelper
    {
        public static async Task<Response> SendEmail(string emailTarget, string message)
        {
            var targets = new List<string>()
            {
                emailTarget
            };

            return await SendEmail(targets, message);
        }

        public static async Task<Response> SendEmail(List<string> emailTargets, string message)
        {
            if (emailTargets == null || !emailTargets.Any())
            {
                return null;
            }

            var subject = "Test Subject";
            var plainTextContent = message;
            var htmlContent = message; // Add <b> tags as appropriate

            var apiKey = ConfigurationManager.AppSettings["OPFLOW_API_KEY"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("noreply@opflow.com", "Operative Flow");

            var tos = new List<EmailAddress>();

            var emailEnvironment = ConfigurationManager.AppSettings["EmailEnvironment"];

            foreach (var emailTarget in emailTargets)
            {
                var to = new EmailAddress(emailTarget, "OpFlow User");
                if (emailEnvironment != "Production")
                {
                    to = new EmailAddress("dave@opflowtech.com");
                    subject += $" ({emailTarget})";
                }

                tos.Add(to);
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, plainTextContent, htmlContent);

            msg.AddCc("info@opflow.com");
            var response = await client.SendEmailAsync(msg);

            return response;
        }
    }
}