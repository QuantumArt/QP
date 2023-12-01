using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.MultistepActions.Csv;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Export
{
    internal class ExportArticlesCommand : IMultistepActionStageCommand
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        private const int ItemsPerStep = 20;

        public int ContentId { get; }

        public int SiteId { get; }

        public int ItemCount { get; }

        public int[] Ids { get; }

        private int[] ExtensionContentIds { get; }

        public ExportArticlesCommand(MultistepActionStageCommandState state)
            : this(state.ParentId, state.Id, 0, state.Ids.ToArray(), state.ExtensionContentIds.ToArray())
        {
        }

        public ExportArticlesCommand(int siteId, int contentId, int itemCount, int[] ids, int[] extensionContentIds)
        {
            SiteId = siteId;
            ContentId = contentId;
            ItemCount = itemCount;
            Ids = ids;
            ExtensionContentIds = extensionContentIds;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            ParentId = SiteId,
            Id = ContentId,
            Ids = Ids.ToList(),
            ExtensionContentIds = ExtensionContentIds.ToList()
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = ItemCount,
            StepCount = MultistepActionHelper.GetStepCount(ItemCount, ItemsPerStep),
            Name = ContentStrings.ExportArticles
        };

        public MultistepActionStepResult Step(int step)
        {
            var content = ContentRepository.GetById(ContentId);
            var settings = HttpContext.Session.GetValue<ExportSettings>(HttpContextSession.ExportSettingsSessionKey);
            Ensure.NotNull(content, string.Format(ContentStrings.ContentNotFound, ContentId));
            Ensure.NotNull(settings);
            var extContents = ExtensionContentIds != null && ExtensionContentIds.Any() ? ContentRepository.GetList(ExtensionContentIds) : new Content[] { };

            var csv = new CsvWriter(SiteId, ContentId, Ids, extContents, settings);
            var result = new MultistepActionStepResult { ProcessedItemsCount = csv.Write(step, ItemsPerStep) };

            HttpContext.Session.SetValue(HttpContextSession.ExportSettingsSessionKey, settings);

            if (csv.CsvReady)
            {
                var info = new FileInfo(settings.UploadFilePath);
                result.AdditionalInfo = info.Exists ? info.Name : string.Empty;
            }

            return result;
        }
    }
}
