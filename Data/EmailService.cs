using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace UserRegistrationApp.Data
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirm Your Email Address - Note Taking App";
            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Email Confirmation</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
                        .content {{ background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
                        .button {{ display: inline-block; padding: 12px 30px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                        .footer {{ margin-top: 20px; font-size: 12px; color: #666; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Welcome to Note Taking App!</h1>
                        </div>
                        <div class='content'>
                            <h2>Confirm Your Email Address</h2>
                            <p>Thank you for registering with our Note Taking App! To complete your registration and start creating notes, please confirm your email address by clicking the button below:</p>
                            
                            <p style='text-align: center; margin: 30px 0;'>
                                <a href='{confirmationLink}' class='button'>Confirm Email Address</a>
                            </p>
                            
                            <p>If the button doesn't work, you can also copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; background-color: #e9ecef; padding: 10px; border-radius: 4px;'>{confirmationLink}</p>
                            
                            <p>This link will expire in 24 hours for security reasons.</p>
                            
                            <p>If you didn't create an account with us, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 Note Taking App. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                // Development mode: Just log the email instead of sending
                if (_emailSettings.UseDevelopmentMode)
                {
                    _logger.LogInformation("=== DEVELOPMENT MODE EMAIL ===");
                    _logger.LogInformation("To: {Email}", toEmail);
                    _logger.LogInformation("Subject: {Subject}", subject);
                    _logger.LogInformation("Message: {Message}", message);
                    _logger.LogInformation("===============================");
                    return;
                }

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                emailMessage.To.Add(new MailboxAddress("", toEmail));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = message
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // Use SSL for port 465, STARTTLS for port 587
                var sslOptions = _emailSettings.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, sslOptions);

                if (!string.IsNullOrEmpty(_emailSettings.SmtpUser))
                {
                    await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
                }

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", toEmail);
                throw;
            }
        }
    }
}
