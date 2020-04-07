using System;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services
{
    [Serializable]
    public class RecreateDynamicImagesContext
    {
        public RecreateDynamicImagesContext()
        {
            ArticleData = new List<Tuple<int, string>>();
            ProcessedImages = new List<string>();
        }
        public int ContentId { get; set; }

        public int FieldId { get; set; }

        public List<Tuple<int, string>> ArticleData { get; set; }

        public List<string> ProcessedImages { get; set; }
    }
}
