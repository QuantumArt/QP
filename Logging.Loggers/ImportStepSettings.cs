using Quantumart.QP8.BLL.Services.MultistepActions.Import;

namespace Quantumart.QP8.Logging.Loggers
{
	public class ImportStepSettings
	{
		public int Step { get; private set; }
		public ImportSettings Settings { get; private set; }

		public ImportStepSettings(int step, ImportSettings settings)
		{
			Step = step;
			Settings = settings;
		}
	}
}