using Microsoft.AspNetCore.Mvc;

namespace Markus.Feedback.Backend.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}