using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Text;
using System.Diagnostics.Contracts;

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

        [ContractAbbreviator]
        private void EnsureStringNotNull(string str)
        {
            Contract.Requires(str != null);
        }


        [ContractVerification(true)]
        [HttpPost(Name = "CheckEmail")]
        public string Post(string email)
        {
            // check validity
            EnsureStringNotNull(email);
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
            Contract.Requires(!string.IsNullOrEmpty(pr_code));

            Contract.Ensures(Contract.Result<string>() != null);
            // send email
            if (!SendEmail(email, pr_code)) return "";

            // send response
            Contract.Assert(!string.IsNullOrEmpty(email));
            return pr_code;
        }


        [ContractVerification(true)]
        private string GenerateCode() 
        {
            Random _random = new Random();
            var builder = new StringBuilder(8);

            char offset = 'A';
            const int lettersOffset = 26;

            for (var i = 0; i < 8; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                Contract.Assert(char.IsLetter(@char));
                builder.Append(@char);
            }

            Contract.Ensures(Contract.Result<string>() != null);
            return builder.ToString();
        }

        [ContractVerification(true)]
        private bool SendEmail(string email, string code)
        {
            Contract.Requires(!string.IsNullOrEmpty(email));
            Contract.Requires(!string.IsNullOrEmpty(code));
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Helper App", "apphelper.f@ukr.net"));
            message.To.Add(new MailboxAddress("User", email));

            message.Subject = "Hepler Mail Verification Code";
            message.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = "Please, enter this code in Helper App to verify your email: " + code
            };

            Contract.Requires(message != null);
            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                string smtpName = Environment.GetEnvironmentVariable("smtpName");
                string smtpPort = Environment.GetEnvironmentVariable("smtpPort");
                string maillog = Environment.GetEnvironmentVariable("maillog");
                string mailpass = Environment.GetEnvironmentVariable("mailpass");

                EnsureStringNotNull(smtpName);
                EnsureStringNotNull(smtpPort);
                EnsureStringNotNull(maillog);
                EnsureStringNotNull(mailpass);

                smtp.Connect(smtpName, Convert.ToInt32(smtpPort));
                smtp.Authenticate(maillog, mailpass);

                smtp.Send(message);
                smtp.Disconnect(true);
            }
            return true;
        }
    }
}