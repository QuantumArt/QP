namespace Quantumart.QP8.Logging.Loggers
{
	public interface ILogger<T>
		where T : class
	{
		void Log(T model);
	}
}
