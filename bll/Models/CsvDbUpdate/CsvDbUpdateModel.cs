using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Models.CsvDbUpdate
{
    public class CsvDbUpdateModel
    {
        public int ContentId { get; set; }

        public IDictionary<int, IList<CsvDbUpdateFieldModel>> Fields { get; set; }
    }
}
