using System;
using Quantumart.QP8.BLL.Enums.Csv;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesLogData
    {
        public Guid Id { get; set; }

        public int[] InsertedArticleIds { get; set; }

        public int[] UpdatedArticleIds { get; set; }

        public CsvImportMode ImportAction { get; set; }
    }
}
