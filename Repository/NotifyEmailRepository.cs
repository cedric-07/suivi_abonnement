using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using suivi_abonnement.Repository.Interface;
using System;
namespace suivi_abonnement.Repository
{
    public class NotifyEmailRepository : INotifyEmailRepository
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;

        public NotifyEmailRepository()
        {
            // Configuration pour Gmail ou Yahoo
            _smtpServer = "smtp.gmail.com"; // Pour Gmail
            _smtpPort = 587;
            _smtpUser = "cedricnomena60@gmail.com";  // Remplace avec ton email
            _smtpPassword = "egca cvzw csvs zwyh"; // Utilise un mot de passe d'application Gmail
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Suivi Abonnement", _smtpUser));
                email.To.Add(new MailboxAddress(toEmail, toEmail));
                email.Subject = subject;

                var body = new TextPart("html")
                {
                    Text = message
                };

                email.Body = body;

                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    await smtp.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(_smtpUser, _smtpPassword);
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }

                Console.WriteLine($"📧 Email envoyé à {toEmail} ✅");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur d'envoi de l'email : {ex.Message}");
            }
        }

    }
}