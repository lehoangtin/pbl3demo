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
    
    // Bỏ qua kiểm tra chứng chỉ SSL (Quan trọng khi chạy ở Localhost)
    smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

    try
    {
        await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
        await smtp.SendAsync(message);
    }
    catch (Exception ex)
    {
        // Thay vì Console.WriteLine, hãy ném lỗi để biết chính xác lỗi gì (Auth fail, Connection refused...)
        throw new Exception($"Lỗi gửi mail: {ex.Message}");
    }
    finally
    {
        await smtp.DisconnectAsync(true);
    }
}}
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