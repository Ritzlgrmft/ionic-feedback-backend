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
				var encoding = Encoding.UTF8;
				var appCredentials = encoding.GetString(Convert.FromBase64String(encodedAppCredentials)).Split(':');

				app = this.registrationConfiguration.Apps.FirstOrDefault(
					a => a.AppKey == appCredentials[0] && a.AppSecret == appCredentials[1]
				);
			}

			return app;
		}

		private async Task SendMailAsync(AppConfiguration app, Models.Feedback feedback)
		{
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
				var deviceInfoAttachment =
					$"Manufacturer\t{feedback.DeviceInfo.Manufacturer}\n" +
					$"Model\t{feedback.DeviceInfo.Model}\n" +
					$"Uuid\t{feedback.DeviceInfo.Uuid}\n" +
					$"Serial\t{feedback.DeviceInfo.Serial}\n" +
					$"Platform\t{feedback.DeviceInfo.Platform}\n" +
					$"Version\t{feedback.DeviceInfo.Version}\n" +
					$"Cordova\t{feedback.DeviceInfo.Cordova}\n" +
					$"IsVirtual\t{feedback.DeviceInfo.IsVirtual}\n";
				bodyBuilder.Attachments.Add("deviceInfo.txt", Encoding.UTF8.GetBytes(deviceInfoAttachment), new ContentType("text", "plain"));
			}
			if (feedback.AppInfo != null)
			{
				var appInfoAttachment =
					$"AppName\t{feedback.AppInfo.AppName}\n" +
					$"PackageName\t{feedback.AppInfo.PackageName}\n" +
					$"VersionCode\t{feedback.AppInfo.VersionCode}\n" +
					$"VersionNumber\t{feedback.AppInfo.VersionNumber}\n";
				bodyBuilder.Attachments.Add("appInfo.txt", Encoding.UTF8.GetBytes(appInfoAttachment), new ContentType("text", "plain"));
			}
			if (feedback.LogMessages != null)
			{
				var logMessagesAttachment = "Timestamp\tLevel\tLogger\tMethod Name\tMessage\n";
				foreach (var logMessage in feedback.LogMessages)
				{
					logMessagesAttachment += $"{logMessage.TimeStamp}\t{logMessage.Level}\t{logMessage.Logger}\t{logMessage.MethodName}\t{string.Join("\t", logMessage.Message)}\n";
				}
				bodyBuilder.Attachments.Add("logMessages.txt", Encoding.UTF8.GetBytes(logMessagesAttachment), new ContentType("text", "plain"));
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