using System;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services
{
    [Serializable]
    public class FieldDefaultValueContext
    {
        public List<int> ProcessedContentItemIds { get; set; }

        public int ContentId { get; set; }

        public int FieldId { get; set; }

        public bool IsBlob { get; set; }

        public List<int> DefaultArticles { get; set; }

        public bool IsM2M { get; set; }

        public bool Symmetric { get; set; }
    }
}
