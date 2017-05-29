using Quantumart.QP8.Constants.Cdc.Enums;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Data
{
    internal class CdcChangeDto
    {
        public string Action { get; set; }

        public CdcActionType ChangeType { get; set; }

        public int OrderNumber { get; set; }

        public CdcEntityDto Entity { get; set; }
    }
}
