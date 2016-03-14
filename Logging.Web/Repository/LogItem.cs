namespace Quantumart.QP8.Logging.Web.Repository
{
	internal class LogItem
	{
		public string Text { get; private set; }
		public string Listener { get; private set; }

		public LogItem(string text, string listener)
		{
			Text = text;
			Listener = listener;
		}
	}
}
