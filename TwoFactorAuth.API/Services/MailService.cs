using System.Net;
using System.Net.Mail;

namespace TwoFactorAuth.API.Services
{
    public class MailService(IConfiguration configuration)
    {
        private readonly IConfiguration _config = configuration;

        public void SendEmail(string toEmail, string otp)
        {
            string smtpServer = _config["Smtp:Server"];
            int smtpPort = int.Parse(_config["Smtp:Port"]);
            string smtpUser = _config["Smtp:User"];
            string smtpPass = _config["Smtp:Pass"];

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                client.EnableSsl = true;
                client.Send(smtpUser, toEmail, "Your OTP Code", $"Your verification code is: {otp}");
            }
        }
    }
}
