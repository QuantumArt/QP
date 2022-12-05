using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Mappers;

namespace Quantumart.QP8.BLL.Services.API
{
    public class ExactMapper : IConditionMapper<ConditionBase>
    {
        public ConditionBase Map(ConditionBase source) => source;
    }
}
