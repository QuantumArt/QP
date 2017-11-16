using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
    /// <summary>
    /// Команда этапа удаления контента
    /// </summary>
    internal class RemoveSiteArticlesCommand : IMultistepActionStageCommand
    {
        private static readonly int ITEMS_PER_STEP = 100;

        public int SiteId { get; }
        public string SiteName { get; }
        public int ItemCount { get; }

        public RemoveSiteArticlesCommand(MultistepActionStageCommandState state)
            : this(state.Id, null, 0)
        {
        }

        public RemoveSiteArticlesCommand(int siteId, string sitetName, int itemCount)
        {
            SiteId = siteId;
            SiteName = sitetName;
            ItemCount = itemCount;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = RemovingStageCommandTypes.RemoveSiteArticles,
            ParentId = 0,
            Id = SiteId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = ItemCount,
            StepCount = MultistepActionHelper.GetStepCount(ItemCount, ITEMS_PER_STEP),
            Name = string.Format(SiteStrings.RemoveSiteArticlesStageName, SiteName ?? "")
        };

        #region IRemovingStageCommand Members

        public MultistepActionStepResult Step(int step)
        {
            ArticleRepository.RemoveSiteArticles(SiteId, ITEMS_PER_STEP);
            return new MultistepActionStepResult { ProcessedItemsCount = ITEMS_PER_STEP };
        }

        #endregion
    }
}
