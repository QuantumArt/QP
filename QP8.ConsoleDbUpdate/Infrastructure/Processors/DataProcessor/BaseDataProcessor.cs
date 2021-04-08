using Irony.Parsing;
using QP8.Infrastructure.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal abstract class BaseDataProcessor : IDataProcessor
    {
        protected BaseDataProcessor(BaseSettingsModel dataSettings)
        {
            SetupQpContext(dataSettings.CustomerCode, dataSettings.DbType);
        }

        private static void SetupQpContext(string connectionStringOrCustomerCode, DatabaseType dbType)
        {
            if (SqlHelpers.TryParseConnectionString(connectionStringOrCustomerCode, (QP.ConfigurationService.Models.DatabaseType)(int)dbType, out var cnsBuilder))
            {
                QPContext.CurrentDbConnectionInfo = new QpConnectionInfo(
                    QPConfiguration.TuneConnectionString(cnsBuilder.ConnectionString, dbType:dbType),
                    dbType
                );
            }
            else
            {
                QPContext.CurrentCustomerCode = connectionStringOrCustomerCode;
            }

            QPContext.CurrentUserId = SpecialIds.AdminUserId;
        }

        public abstract void Process();

        public abstract void Process(string inputData);

        protected static string GetConnectionString(string connectionStringOrCustomerCode, DatabaseType dbType) => SqlHelpers.TryParseConnectionString(connectionStringOrCustomerCode, (QP.ConfigurationService.Models.DatabaseType)(int)dbType,  out var cnsBuilder)
            ? cnsBuilder.ConnectionString
            : QPConfiguration.GetConnectionString(connectionStringOrCustomerCode);
    }
}
