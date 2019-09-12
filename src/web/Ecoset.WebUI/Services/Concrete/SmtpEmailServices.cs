using Ecoset.WebUI.Services.Abstract;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ecoset.WebUI.Options;

namespace Ecoset.WebUI.Services.Concrete
{
    public class AuthMessageSender : IEmailSender
    {
        private IWebHostEnvironment _env;
        private ILogger<AuthMessageSender> _logger;
        private EmailOptions _options;
        public AuthMessageSender(IWebHostEnvironment env, ILogger<AuthMessageSender> logger, IOptions<EmailOptions> options) {
            _env = env;
            _logger = logger;
            _options = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            mimeMessage.To.Add(new MailboxAddress(email, email));
            mimeMessage.Subject = subject;

            var template = LoadTemplate();
            var fullMessageHtml = template.Replace("$CONTENT$", message);

            var builder = new BodyBuilder();
            builder.TextBody = message;
            builder.HtmlBody = fullMessageHtml;
            mimeMessage.Body = builder.ToMessageBody();

            //TODO remove try-catch - replace with settings for dev environment
            try {
                using (var client = new SmtpClient())
                {
                    client.Connect(_options.Host, 587, false);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.SendAsync(mimeMessage);
                    await client.DisconnectAsync(true);
                }
            }
            catch (Exception e) {
                _logger.LogError("There was a problem sending an email: " + e.Message);
            }
        }

        public string LoadTemplate()
        {
            string emailPath =  _env.WebRootPath + "//email-template.txt"; //ConfigurationManager.AppSettings["EmailTemplate"].ToString();
            StreamReader rdr = File.OpenText(emailPath);
            try
            {
                string content = rdr.ReadToEnd();
                return content;
                }
            catch
            {
                return "$CONTENT$";
            }
            finally
            {
                rdr.Dispose();
            }
        }
    }
}
