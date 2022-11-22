using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class ExtendedArticle
    {
        public Article BaseArticle { get; }

        public Dictionary<Field, Article> Extensions { get; }

        public ExtendedArticle(Article baseArticle)
        {
            BaseArticle = baseArticle;
            Extensions = new Dictionary<Field, Article>();
        }
    }
}
