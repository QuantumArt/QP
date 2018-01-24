using System;

namespace Quantumart.QP8.BLL.Services
{
    [Serializable]
    public class FieldDefaultValueContext
    {
        public int[] ProcessedContentItemIds { get; set; }

        public int ContentId { get; set; }

        public int FieldId { get; set; }

        public bool IsBlob { get; set; }

        public int[] DefaultArticles { get; set; }

        public bool IsM2M { get; set; }

        public bool Symmetric { get; set; }
    }
}
