using System;
using System.ComponentModel.DataAnnotations;

namespace Markus.Feedback.Backend.Models
{
	public class Feedback
	{
		[Required]
		public DateTime Timestamp { get; set; }
		public string Category { get; set; }
		[Required]
		public string Message { get; set; }
		[RegularExpression(@"^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$")]
		public string Email { get; set; }
		public string Screenshot { get; set; }
	}
}