using System;
using System.Linq;
using System.Xml.Linq;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Providers.ConfigurationProvider
{
    internal class FileConfigurationProvider : IConfigurationProvider
    {
        private readonly string _qpConfigurationFile;

        public FileConfigurationProvider(string qpConfigurationFile)
        {
            _qpConfigurationFile = qpConfigurationFile;
        }

        public ApplicationSettings UpdateSettings(ApplicationSettings settings)
        {
            XDocument xmlConfig = XDocument.Load(_qpConfigurationFile);

            XElement customers = xmlConfig.Root.Elements()
                .Where(e => e.Name == "customers")
                .FirstOrDefault();

            if (customers is null)
            {
                throw new OptionException("There is no customers section in qp configuration file", nameof(customers));
            }

            XElement customer = customers.Elements()
                .Where(e => e.Attributes()
                    .Any(a => a.Name == "customer_name" && a.Value == settings.CustomerCode)
                )
                .FirstOrDefault();

            if (customer is null)
            {
                throw new OptionException("Can't find customer in qp configuration file", settings.CustomerCode);
            }

            string dbType = customer.Attributes()
                .Where(a => a.Name == "db_type")
                .Select(a => a.Value)
                .FirstOrDefault();

            if (!Enum.TryParse(dbType, out DatabaseType databaseType))
            {
                throw new OptionException("Unknown database type in qp configuration file", dbType);
            }

            string connectionString = customer.Elements()
                .Where(e => e.Name == "db")
                .Select(x => x.Value)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new OptionException("Can't find connection string in qp configuration file", nameof(connectionString));
            }

            settings.DbType = databaseType;
            settings.ConnectionString = connectionString;

            return settings;
        }
    }
}
