using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Markus.Feedback.Backend.Configuration;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Markus.Feedback.Backend.Controllers
{
	[Route("api/[controller]")]
	public class FeedbackController : Controller
	{
		private ILogger<FeedbackController> logger;
		private MailConfiguration mailConfiguration;
		private RegistrationConfiguration registrationConfiguration;

		public FeedbackController(
			ILogger<FeedbackController> logger,
			IOptions<MailConfiguration> mailConfigurationAccessor,
			IOptions<RegistrationConfiguration> registrationConfigurationAccessor)
		{
			this.logger = logger;
			this.mailConfiguration = mailConfigurationAccessor.Value;
			this.registrationConfiguration = registrationConfigurationAccessor.Value;
		}

		[HttpPost]
		[ActionName("Create")]
		[EnableCors("CorsPolicy")]
		public async Task<IActionResult> CreateAsync([FromBody]Models.Feedback feedback)
		{
			logger.LogDebug($"Create\t{feedback.ToString()}");
			try
			{
				if (feedback == null)
				{
					logger.LogError("Create\tfeedback missing");
					return BadRequest();
				}
				else if (!ModelState.IsValid)
				{
					logger.LogError("Create\tinvalid ModelState");
					foreach (var entry in ModelState)
					{
						foreach (var error in entry.Value.Errors)
						{
							logger.LogInformation($"Create\t{entry.Key}\t{error.ErrorMessage}");
						}
					}
					return BadRequest();
				}

				var app = CheckAuthentication();
				if (app == null)
				{
					logger.LogError("Create\tAuthentication missing");
					return Unauthorized();
				}

				await this.SendMailAsync(app, feedback);
			}
			catch (Exception e)
			{
				logger.LogError($"Create\t{e}");
				return BadRequest();
			}
			return Ok();
		}

		private AppConfiguration CheckAuthentication()
		{
			AppConfiguration app = null;

			string authorization = Request.Headers["Authorization"];
			if (authorization != null && authorization.StartsWith("Basic"))
			{
				var encodedAppCredentials = authorization.Substring("Basic ".Length).Trim();
				var encoding = Encoding.GetEncoding("iso-8859-1");
				var appCredentials = encoding.GetString(Convert.FromBase64String(encodedAppCredentials)).Split(':');

				app = this.registrationConfiguration.Apps.FirstOrDefault(
					a => a.AppKey == appCredentials[0] && a.AppSecret == appCredentials[1]
				);
			}

			return app;
		}

		private async Task SendMailAsync(AppConfiguration app, Models.Feedback feedback)
		{
			logger.LogDebug($"{feedback.Screenshot?.Substring(0, 40)}");
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(mailConfiguration.SenderName, mailConfiguration.SenderMail));
			message.To.Add(new MailboxAddress(app.RecipientName, app.RecipientMail));
			message.Subject = $"Feedback from {app.AppName}";

			var bodyBuilder = new BodyBuilder();
			bodyBuilder.HtmlBody = $"<p><b>Timestamp:</b> {feedback.Timestamp.ToString("o")}</p>";
			if (!string.IsNullOrEmpty(feedback.Category))
			{
				message.Subject += $" - {feedback.Category}";
				bodyBuilder.HtmlBody += $"<p><b>Category:</b> {feedback.Category}</p>";
			}
			if (!string.IsNullOrEmpty(feedback.Email))
			{
				bodyBuilder.HtmlBody += $"<p><b>Email:</b> {feedback.Email}</p>";
			}
			bodyBuilder.HtmlBody += $"<p>{feedback.Message}</p>";
			if (feedback.DeviceInfo != null)
			{
				bodyBuilder.HtmlBody += "<table><tr><th colspan=\"2\">Device Info</th></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>Manufacturer</th><td>{feedback.DeviceInfo.Manufacturer}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>Model</th><td>{feedback.DeviceInfo.Model}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>Uuid</th><td>{feedback.DeviceInfo.Uuid}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>Serial</th><td>{feedback.DeviceInfo.Serial}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>Platform</th><td>{feedback.DeviceInfo.Platform}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>Version</th><td>{feedback.DeviceInfo.Version}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>Cordova</th><td>{feedback.DeviceInfo.Cordova}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>IsVirtual</th><td>{feedback.DeviceInfo.IsVirtual}</td></tr>";
				bodyBuilder.HtmlBody += "</table>";
			}
			if (feedback.AppInfo != null)
			{
				bodyBuilder.HtmlBody += "<table><tr><th colspan=\"2\">App Info</th></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>AppName</th><td>{feedback.AppInfo.AppName}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>PackageName</th><td>{feedback.AppInfo.PackageName}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>VersionCode</th><td>{feedback.AppInfo.VersionCode}</td></tr>";
				bodyBuilder.HtmlBody += $"<tr><th>VersionNumber</th><td>{feedback.AppInfo.VersionNumber}</td></tr>";
				bodyBuilder.HtmlBody += "</table>";
			}
			if (feedback.LogMessages != null)
			{
				bodyBuilder.HtmlBody += "<table><tr><th colspan=\"2\">Log Messages</th></tr>";
				bodyBuilder.HtmlBody += "<tr><th>Timestamp</th><th>Level</th><th>Logger</th><th>Method Name</th><th>Message</th></tr>";
				var blank = " ";
				foreach (var logMessage in feedback.LogMessages)
				{
					bodyBuilder.HtmlBody += $"<tr><td>{logMessage.TimeStamp}</td><td>{logMessage.Level}</td><td>{logMessage.Logger}</td><td>{logMessage.MethodName}</td><td>{string.Join(blank, logMessage.Message)}</td></tr>";
				}
				bodyBuilder.HtmlBody += "</table>";
			}
			if (!string.IsNullOrEmpty(feedback.Screenshot))
			{
				bodyBuilder.HtmlBody += $"<p><img src=\"{feedback.Screenshot}\"></p>";
			}
			message.Body = bodyBuilder.ToMessageBody();

			using (var client = new SmtpClient())
			{
				await client.ConnectAsync(mailConfiguration.Server, mailConfiguration.Port, false);
				// since we don't have an OAuth2 token, disable the XOAUTH2 authentication mechanism   
				client.AuthenticationMechanisms.Remove("XOAUTH2");
				await client.AuthenticateAsync(mailConfiguration.User, mailConfiguration.Password);
				await client.SendAsync(message);
				await client.DisconnectAsync(true);
			}
		}
	}
}