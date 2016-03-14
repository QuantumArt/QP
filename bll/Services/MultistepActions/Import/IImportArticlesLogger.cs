namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
	public interface IImportArticlesLogger
	{
		void LogStartImport(ImportSettings settings);
		void LogStep(int step, ImportSettings settings);
		void LogEndImport(ImportSettings settings);
	}
}