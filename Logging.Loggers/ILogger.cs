namespace Quantumart.QP8.Logging.Loggers
{
	public interface ILogger<in T>
		where T : class
	{
		void Log(T model);
	}
}
