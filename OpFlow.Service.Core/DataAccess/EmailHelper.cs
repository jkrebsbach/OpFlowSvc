using OpFlow.Data;
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
    public class EmailHelper
    {
        private string _apiKey;
        private string _emailEnvironment;

        public EmailHelper(IConfiguration configuration)
        {
            _apiKey = configuration.GetValue<string>("OPFLOW_API_KEY") ?? "";
            _emailEnvironment = configuration.GetValue<string>("EmailEnvironment") ?? "";
        }

        public async Task<Response> SendEmail(User emailTarget, string subject, string message, List<MessageAttachment> attachments = null)
        {
            var targets = new List<User>()
            {
                emailTarget
            };

            return await SendEmail(targets, subject, message, attachments);
        }

        public async Task<Response> SendEmail(IEnumerable<User> emailTargets, string subject, string message, List<MessageAttachment> attachments = null)
        {
            if (emailTargets == null || !emailTargets.Any())
            {
                return null;
            }

            var plainTextContent = message;
            var htmlContent = message; // Add <b> tags as appropriate

            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress("noreply@opflow.com", "Operative Flow");

            var tos = new List<EmailAddress>();


            foreach (var emailTarget in emailTargets)
            {
                if (string.IsNullOrEmpty(emailTarget.Email))
                    continue;

                var to = new EmailAddress(emailTarget.Email, $"{emailTarget.FirstName} {emailTarget.LastName}");
                if (_emailEnvironment != "Production")
                {
                    to = new EmailAddress("dave@opflowtech.com");
                    subject += $" ({emailTarget.Email})";
                }

                tos.Add(to);
            }

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, plainTextContent, htmlContent);

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var base64Content = Convert.ToBase64String(attachment.FileContent);
                    msg.AddAttachment(attachment.Filename, base64Content);
                }
            }

            msg.AddCc("info@opflow.com");
            var response = await client.SendEmailAsync(msg);

            return response;
        }

        public class MessageAttachment
        {
            public string Filename { get; set; }
            public byte[] FileContent { get; set; }
        }
    }
}