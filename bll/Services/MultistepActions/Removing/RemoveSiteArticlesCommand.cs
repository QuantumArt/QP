using System;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository.Articles;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
	/// <summary>
	/// Команда этапа удаления контента
	/// </summary>
	internal class RemoveSiteArticlesCommand : IMultistepActionStageCommand
	{
		private static readonly int ITEMS_PER_STEP = 100;

		public int SiteId { get; private set; }
		public string SiteName { get; private set; }
		public int ItemCount { get; private set; }

		public RemoveSiteArticlesCommand(MultistepActionStageCommandState state) : this(state.Id, null, 0) { }

		public RemoveSiteArticlesCommand(int siteId, string sitetName, int itemCount)
		{
			SiteId = siteId;
			SiteName = sitetName;
			ItemCount = itemCount;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = RemovingStageCommandTypes.RemoveSiteArticles,
				ParentId = 0,
				Id = SiteId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = ItemCount,
				StepCount = MultistepActionHelper.GetStepCount(ItemCount, ITEMS_PER_STEP),
				Name = String.Format(SiteStrings.RemoveSiteArticlesStageName, (SiteName ?? ""))
			};
		}

		#region IRemovingStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			ArticleRepository.RemoveSiteArticles(SiteId, ITEMS_PER_STEP);
			return new MultistepActionStepResult { ProcessedItemsCount = ITEMS_PER_STEP };
		}
		#endregion

		
	}
}
