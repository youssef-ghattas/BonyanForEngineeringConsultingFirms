using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace BonyanForEngineeringConsultingFirms.Services
{
	public class EmailService
	{
		private readonly IConfiguration _config;

		public EmailService(IConfiguration config)
		{
			_config = config;
		}

		public void SendEmail(string toEmail, string toName, string subject, string htmlBody)
		{
			var settings = _config.GetSection("EmailSettings");

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(settings["FromName"], settings["FromEmail"]));
			message.To.Add(new MailboxAddress(toName, toEmail));
			message.Subject = subject;

			message.Body = new TextPart("html") { Text = htmlBody };

			using var client = new SmtpClient();
			client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
			client.Authenticate(settings["FromEmail"], settings["Password"]);
			client.Send(message);
			client.Disconnect(true);
		}
	}
}