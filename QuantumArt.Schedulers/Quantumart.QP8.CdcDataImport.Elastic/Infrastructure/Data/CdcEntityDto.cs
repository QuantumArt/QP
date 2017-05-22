using System.Collections.Generic;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Data
{
    internal class CdcEntityDto
    {
        public string EntityType { get; set; }

        public string InvariantName { get; set; }

        public IDictionary<string, object> Columns { get; set; }
    }
}
