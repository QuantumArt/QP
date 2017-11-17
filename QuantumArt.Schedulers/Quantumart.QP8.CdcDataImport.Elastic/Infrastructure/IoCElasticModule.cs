using Autofac;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Factories;
using QP8.Infrastructure.Logging.PrtgMonitoring.NLogExtensions.Interfaces;
using Quantumart.QP8.BLL.Logging;
using Quantumart.QP8.BLL.Services.CdcImport;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure
{
    public class IoCElasticModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(GetType().Assembly).AsImplementedInterfaces().AsSelf().InstancePerLifetimeScope();
            builder.RegisterTypes(typeof(ExternalSystemNotificationService), typeof(DbService), typeof(CdcElasticImportService)).AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterTypes(typeof(PrtgErrorsHandler)).AsSelf();

            builder.RegisterInstance(new PrtgNLogFactory(
                LoggerData.DefaultPrtgServiceStateVariableName,
                LoggerData.DefaultPrtgServiceQueueVariableName,
                LoggerData.DefaultPrtgServiceStatusVariableName
            )).As<IPrtgNLogFactory>();
        }
    }
}
