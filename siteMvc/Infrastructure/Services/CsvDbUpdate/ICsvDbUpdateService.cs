using System.Collections.Generic;
using Quantumart.QP8.BLL.Models.CsvDbUpdate;

namespace Quantumart.QP8.WebMvc.Infrastructure.Services.CsvDbUpdate
{
    /// <summary>
    /// Service for replaying csv data
    /// </summary>
    public interface ICsvDbUpdateService
    {
        /// <summary>
        /// Import csv data
        /// </summary>
        /// <param name="data">Csv data to import</param>
        void Process(IEnumerable<CsvDbUpdateModel> data);
    }
}
