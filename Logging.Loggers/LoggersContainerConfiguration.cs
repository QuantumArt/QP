using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.Unity;
using Quantumart.QP8.BLL.Services.MultistepActions.Import;

namespace Quantumart.QP8.Logging.Loggers
{
	public class LoggersContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterType<LogWriter>(new InjectionFactory(c => EnterpriseLibraryContainer.Current.GetInstance<LogWriter>()));
			Container.RegisterType<ExceptionManager>(new InjectionFactory(c => EnterpriseLibraryContainer.Current.GetInstance<ExceptionManager>()));
			Container.RegisterType<IImportArticlesLogger, ImportArticlesLogger>();
		}
	}
}