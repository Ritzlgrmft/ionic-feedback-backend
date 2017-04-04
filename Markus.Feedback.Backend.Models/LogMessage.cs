using System;

namespace Markus.Feedback.Backend.Models
{
	public class LogMessage
	{
		public DateTime TimeStamp;
		public string Level;
		public string Logger;
		public string MethodName;
		public string[] Message;
	}
}