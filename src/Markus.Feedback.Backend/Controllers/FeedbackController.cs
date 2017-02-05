using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
		public IActionResult Create([FromBody]Models.Feedback feedback)
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
					return BadRequest();
				}

				// bool itemExists = _toDoRepository.DoesItemExist(item.ID);
				// if (itemExists)
				// {
				// 	return StatusCode(StatusCodes.Status409Conflict, ErrorCode.TodoItemIDInUse.ToString());
				// }
				// _toDoRepository.Insert(item);
			}
			catch (Exception e)
			{
				logger.LogError($"Create\t{e}");
				return BadRequest();
			}
			return Ok();
		}
	}
}