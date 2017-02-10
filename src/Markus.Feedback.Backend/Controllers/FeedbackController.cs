using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeKit;
using Newtonsoft.Json;

namespace Markus.Feedback.Backend.Controllers
{
	[Route("api/[controller]")]
	public class FeedbackController : Controller
	{
		private ILogger<FeedbackController> logger;

		public FeedbackController(ILogger<FeedbackController> logger)
		{
			this.logger = logger;
		}

		[HttpPost]
		[ActionName("Create")]
		public async Task<IActionResult> CreateAsync([FromBody]Models.Feedback feedback)
		{
			logger.LogDebug($"Create\t{JsonConvert.SerializeObject(feedback, Formatting.None)}");
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

				await this.SendMailAsync();
			}
			catch (Exception e)
			{
				logger.LogError($"Create\t{e}");
				return BadRequest();
			}
			return Ok();
		}

		private async Task SendMailAsync()
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("Feedback Backend", "feedback@markus.onmicrosoft.com"));
			message.To.Add(new MailboxAddress("Markus", "me@markus.onmicrosoft.com"));
			message.Subject = "Hello World - A mail from ASPNET Core";
			var bodyBuilder = new BodyBuilder();
			bodyBuilder.HtmlBody = @"<b>This is bold and this is <i>italic</i></b>";
			message.Body = bodyBuilder.ToMessageBody();

			using (var client = new SmtpClient())
			{
				await client.ConnectAsync("smtp.office365.com", 587, false);
				// since we don't have an OAuth2 token, disable the XOAUTH2 authentication mechanism   
				client.AuthenticationMechanisms.Remove("XOAUTH2");
				await client.AuthenticateAsync("me@markus.onmicrosoft.com", "4SogineM");
				await client.SendAsync(message);
				await client.DisconnectAsync(true);
			}
		}
	}
}