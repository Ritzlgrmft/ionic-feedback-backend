using System;
using Microsoft.AspNetCore.Mvc;

namespace Markus.Feedback.Backend.Controllers
{
	[Route("api/[controller]")]
	public class FeedbackController : Controller
	{
		[HttpPost]
		public IActionResult Create([FromBody]Models.Feedback feedback)
		{
			try
			{
				if (feedback == null || !ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				// bool itemExists = _toDoRepository.DoesItemExist(item.ID);
				// if (itemExists)
				// {
				// 	return StatusCode(StatusCodes.Status409Conflict, ErrorCode.TodoItemIDInUse.ToString());
				// }
				// _toDoRepository.Insert(item);
			}
			catch (Exception)
			{
				// return BadRequest(ErrorCode.CouldNotCreateItem.ToString());
			}
			return Ok();
		}
	}
}