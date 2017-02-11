namespace Markus.Feedback.Backend.Configuration
{
	public class MailConfiguration
	{
		public string Server { get; set; }
		public int Port { get; set; }
		public string User { get; set; }
		public string Password { get; set; }
		public string SenderMail { get; set; }
		public string SenderName { get; set; }
	}
}