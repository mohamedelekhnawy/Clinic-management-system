using ClinicManagementSystem.api.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ClinicManagementSystem.api.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailOptions> emailOptions, ILogger<EmailService> logger)
        {
            _emailOptions = emailOptions.Value;
            _logger = logger;
        }

        public async Task SendVerificationCodeAsync(string email, string firstName, string code, CancellationToken cancellationToken = default)
        {
            // Check if email service is configured
            if (string.IsNullOrEmpty(_emailOptions.Host) || string.IsNullOrEmpty(_emailOptions.Username))
            {
                _logger.LogWarning("Email service is not configured. Skipping email send to {Email}. Verification code: {Code}", email, code);
                _logger.LogWarning("To enable email sending, configure Email settings in appsettings.json");
                // In development/testing, log the code so you can verify manually
                return;
            }

            try
            {
                var subject = "Verify Your Email - Clinic Management System";
                var body = GenerateVerificationEmailBody(firstName, code);

                await SendEmailAsync(email, subject, body, cancellationToken);

                _logger.LogInformation("Verification email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to {Email}", email);
                throw;
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            using var smtpClient = new SmtpClient(_emailOptions.Host, _emailOptions.Port)
            {
                EnableSsl = _emailOptions.EnableSsl,
                Credentials = new NetworkCredential(_emailOptions.Username, _emailOptions.Password),
                Timeout = 30000 // 30 seconds timeout
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailOptions.FromEmail, _emailOptions.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
        }

        private static string GenerateVerificationEmailBody(string firstName, string code)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Email Verification</title>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 40px auto;
                            background-color: #ffffff;
                            border-radius: 8px;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                            overflow: hidden;
                        }}
                        .header {{
                            background-color: #4CAF50;
                            color: #ffffff;
                            padding: 30px;
                            text-align: center;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 24px;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .content h2 {{
                            color: #333333;
                            margin-top: 0;
                        }}
                        .content p {{
                            color: #666666;
                            line-height: 1.6;
                            margin: 16px 0;
                        }}
                        .verification-code {{
                            background-color: #f8f9fa;
                            border: 2px dashed #4CAF50;
                            border-radius: 8px;
                            padding: 20px;
                            text-align: center;
                            margin: 30px 0;
                        }}
                        .verification-code .code {{
                            font-size: 32px;
                            font-weight: bold;
                            color: #4CAF50;
                            letter-spacing: 8px;
                            font-family: 'Courier New', monospace;
                        }}
                        .expiry-notice {{
                            background-color: #fff3cd;
                            border-left: 4px solid #ffc107;
                            padding: 12px 16px;
                            margin: 20px 0;
                            color: #856404;
                        }}
                        .footer {{
                            background-color: #f8f9fa;
                            padding: 20px 30px;
                            text-align: center;
                            color: #999999;
                            font-size: 12px;
                        }}
                        .footer p {{
                            margin: 5px 0;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Clinic Management System</h1>
                        </div>
                        <div class='content'>
                            <h2>Hi {firstName},</h2>
                            <p>Thank you for registering with Clinic Management System! To complete your registration, please verify your email address by entering the verification code below:</p>
                            
                            <div class='verification-code'>
                                <div class='code'>{code}</div>
                            </div>
                            
                            <div class='expiry-notice'>
                                <strong>Important:</strong> This verification code will expire in 15 minutes.
                            </div>
                            
                            <p>If you didn't create an account with Clinic Management System, please ignore this email.</p>
                            
                            <p>Best regards,<br>
                            The Clinic Management System Team</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message, please do not reply to this email.</p>
                            <p>&copy; 2026 Clinic Management System. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>";
        }
    }
}
