using Servidor_PI.Repositories.Interfaces;
using System.Net.Mail;
using System.Net;

namespace Servidor_PI.Repositories // Ajuste o namespace se necessário
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "andre3000ferreira@gmail.com";
        private readonly string _smtpPass = "lslqmdgncsqpxzrd";

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort))
            {
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);

                var mail = new MailMessage
                {
                    From = new MailAddress(_smtpUser, "Ouvidoria da Organização"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                mail.To.Add(toEmail);

                await client.SendMailAsync(mail);
            }
        }
    }
}