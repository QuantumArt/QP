using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using System.Transactions;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Logging.Services;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportArticlesCommand : IMultistepActionStageCommand
    {
        private static readonly int ITEMS_PER_STEP = 20;

		private readonly ILogReader _logReader;
		private readonly IImportArticlesLogger _logger;

        public int SiteId { get; set; }
        public int ContentId { get; set; }
        public int ItemCount { get; set; }

		public ImportArticlesCommand(MultistepActionStageCommandState state, ILogReader logReader, IImportArticlesLogger logger)
			: this(state.ParentId, state.Id, 0, logReader, logger)
		{
		}

		public ImportArticlesCommand(int siteId, int contentId, int itemCount, ILogReader logReader, IImportArticlesLogger logger)
        {
            this.SiteId = siteId;
            this.ContentId = contentId;
            this.ItemCount = itemCount;
			_logReader = logReader;
			_logger = logger;
        }

        public MultistepActionStageCommandState GetState()
        {
            return new MultistepActionStageCommandState
            {
                ParentId = SiteId,
                Id = ContentId
            };
        }

        public MultistepActionStepResult Step(int step)
        {
            if (HttpContext.Current.Session["ImportArticlesService.Settings"] == null) {
                throw new ArgumentException("There is no specified settings");
            }
            ImportSettings setts = HttpContext.Current.Session["ImportArticlesService.Settings"] as ImportSettings;
            CsvReader reader = new CsvReader(SiteId, ContentId, setts);
            int processedItemsCount;
			MultistepActionStepResult result = new MultistepActionStepResult();

			using (var tscope = new TransactionScope())
			{
				using (var scope = new QPConnectionScope())
				{
					try
					{
						reader.Process(step, ITEMS_PER_STEP, out processedItemsCount);

						if (step * ITEMS_PER_STEP >= reader.ArticleCount - ITEMS_PER_STEP)
						{
							reader.PostUpdateM2MRelationAndO2MRelationFields();
						}
						_logger.LogStep(step, setts);
						result.ProcessedItemsCount = processedItemsCount;
						result.AdditionalInfo = _logReader.Read();
					}
					catch (Exception ex)
					{
						throw new ImportException(String.Format(ImportStrings.ImportInterrupted, ex.Message, reader.LastProcessed), ex, setts);
					}
				}
				tscope.Complete();
			}

            return result;
        }

        public MultistepStageSettings GetStageSettings()
        {
            return new MultistepStageSettings
            {
                ItemCount = ItemCount,
                StepCount = MultistepActionHelper.GetStepCount(ItemCount, ITEMS_PER_STEP),
                Name = ContentStrings.ImportArticles
            };
        }
    }
}
