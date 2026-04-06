using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace StudyShare.Services
{
    public class EmailSender
    {
        private readonly MailSettings _mailSettings;

        public EmailSender(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
            message.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                // Kết nối tới Gmail SMTP
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                // Đăng nhập bằng App Password
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                // Gửi mail
                await smtp.SendAsync(message);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine("Lỗi gửi mail: " + ex.Message);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }

    // Lớp Model để map dữ liệu từ appsettings.json
    public class MailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}