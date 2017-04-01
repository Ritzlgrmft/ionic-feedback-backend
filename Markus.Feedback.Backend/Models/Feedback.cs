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
		public string Name { get; set; }
		[RegularExpression(@"^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$")]
		public string Email { get; set; }
		public string Screenshot { get; set; }
		public DeviceInfo DeviceInfo { get; set; }
		public AppInfo AppInfo { get; set; }
		public LogMessage[] LogMessages { get; set; }

		public override string ToString()
		{
			return $"{Timestamp}\t{Category}\t{Message}\t{Name}\t{Email}" +
			$"\t{!string.IsNullOrEmpty(Screenshot)}\t{DeviceInfo != null}\t{AppInfo != null}" +
			$"\t{LogMessages != null}";
		}
	}
}