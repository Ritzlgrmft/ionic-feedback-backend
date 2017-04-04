using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Markus.Feedback.Backend.Models;
using Newtonsoft.Json;

namespace Markus.Feedback.Backend.ConsoleTest
{
	class Program
	{
		static void Main(string[] args)
		{
			MainAsync(args).Wait();
		}

		static async Task MainAsync(string[] args)
		{
			using (var client = new HttpClient())
			{
				var encoding = Encoding.UTF8;
				var appCredentials = encoding.GetBytes("94f4e317-a8ef-4ece-92ff-9e0d9398b5eb:307726c0-f677-4918-beb5-01ca6fce80ea");
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(appCredentials));

				var logMessages = new List<LogMessage>();
				for (var i = 0; i < 10; i++)
				{
					logMessages.Add(new LogMessage
					{
						TimeStamp = DateTime.Now,
						Level = "INFO",
						Logger = "myLogger",
						MethodName = "myMethod",
						Message = new[] { "myMessage " + i }
					});
				}
				var feedback = new Feedback.Backend.Models.Feedback
				{
					Timestamp = DateTime.Now,
					Category = "c",
					Message = "myMessage",
					Name = "myName",
					Email = "my@email.de",
					LogMessages = logMessages.ToArray()
				};

				var content = new StringContent(JsonConvert.SerializeObject(feedback), Encoding.UTF8, "application/json");
				var response = await client.PostAsync("http://localhost:5000/api/Feedback", content);
				Console.WriteLine($"Request sent: {response.StatusCode}");
			}
		}
	}
}
