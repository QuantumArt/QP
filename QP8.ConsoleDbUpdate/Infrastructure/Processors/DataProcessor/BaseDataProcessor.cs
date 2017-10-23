using QP8.Infrastructure.Helpers;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.DataProcessor
{
    internal abstract class BaseDataProcessor : IDataProcessor
    {
        protected BaseDataProcessor(BaseSettingsModel dataSettings)
        {
            SetupQpContext(dataSettings.CustomerCode);
        }

        private static void SetupQpContext(string connectionStringOrCustomerCode)
        {
            if (SqlHelpers.TryParseConnectionString(connectionStringOrCustomerCode, out var cnsBuilder))
            {
                QPContext.CurrentDbConnectionString = QPConfiguration.TuneConnectionString(cnsBuilder.ConnectionString, out var _);
            }
            else
            {
                QPContext.CurrentCustomerCode = connectionStringOrCustomerCode;
                QPContext.CurrentDbConnectionString = QPConfiguration.TuneConnectionString(cnsBuilder.ConnectionString, out var _);
            }
        }

        public abstract void Process();

        public abstract void Process(string inputData);

        protected static string GetConnectionString(string connectionStringOrCustomerCode) => SqlHelpers.TryParseConnectionString(connectionStringOrCustomerCode, out var cnsBuilder)
            ? cnsBuilder.ConnectionString
            : QPConfiguration.GetConnectionString(connectionStringOrCustomerCode);
    }
}
