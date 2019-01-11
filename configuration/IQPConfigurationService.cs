using Quantumart.QP8.Configuration.Models;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quantumart.QP8.Configuration
{
    public interface IQPConfigurationService
    {
        [Get("/api/v1/customers/{id}")]
        Task<QaConfigCustomer> GetCustomer([AliasAs("id")] string customerCode);

        [Get("/api/v1/customers/{id}")]
        Task<List<QaConfigCustomer>> GetCustomers();

        [Get("/api/v1/variables")]
        Task<List<QaConfigApplicationVariable>> GetVariables();
    }
}
