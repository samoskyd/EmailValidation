using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Text;

namespace EmailValidation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;

        public EmailController(ILogger<EmailController> logger)
        {
            _logger = logger;
        }

        [HttpPost(Name = "CheckEmail")]
        public string Post(string email)
        {
            // check validity
            try
            {
                var eAddress = new MailAddress(email);
            }
            catch
            {
                return "";
            }

            // generate code
            string pr_code = GenerateCode();

            // send email
            if (!SendEmail(email, pr_code)) return "";

            // send response
            return pr_code;
        }

        private string GenerateCode() 
        {
            Random _random = new Random();
            var builder = new StringBuilder(8);

            char offset = 'A';
            const int lettersOffset = 26;

            for (var i = 0; i < 8; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return builder.ToString();
        }

        private bool SendEmail(string email, string code)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Helper App", "apphelper.f@ukr.net"));
            message.To.Add(new MailboxAddress("User", email));

            message.Subject = "Hepler Mail Verification Code";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = "Please, enter this code in Helper App to verify your email: " + code
            };

            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                string smtpName = System.Environment.GetEnvironmentVariable("smtpName");
                string smtpPort = System.Environment.GetEnvironmentVariable("smtpPort");
                string maillog = System.Environment.GetEnvironmentVariable("maillog");
                string mailpass = System.Environment.GetEnvironmentVariable("mailpass");

                smtp.Connect(smtpName, Convert.ToInt32(smtpPort));
                smtp.Authenticate(maillog, mailpass);

                smtp.Send(message);
                smtp.Disconnect(true);
            }
            return true;
        }
    }
}