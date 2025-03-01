using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MyECommerce.Services
{
    public class SendGridService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _templateId; // ✅ Store Template ID

        public SendGridService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException("SendGrid API Key is missing in appsettings.json");
            _templateId = _configuration["SendGrid:TemplateId"] ?? throw new ArgumentNullException("SendGrid Template ID is missing in appsettings.json");
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string otp)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress("joinyfy@gmail.com", "Joinyfy Placement");
                var to = new EmailAddress(toEmail);

                // ✅ SendGrid Dynamic Template Variables (Matches Old Project)
                var dynamicTemplateData = new
                {
                    FirstName = toEmail, // Use Email as Username
                    otp = otp,
                    expiry = "5 minutes"
                };

                var msg = MailHelper.CreateSingleTemplateEmail(from, to, _templateId, dynamicTemplateData);
                var response = await client.SendEmailAsync(msg);

                Console.WriteLine($"✅ SendGrid Response Code: {response.StatusCode}");

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    return true; // ✅ Email sent successfully
                }
                else
                {
                    string responseBody = await response.Body.ReadAsStringAsync();
                    Console.WriteLine($"❌ SendGrid Error Response: {responseBody}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                return false;
            }
        }
    }
}
