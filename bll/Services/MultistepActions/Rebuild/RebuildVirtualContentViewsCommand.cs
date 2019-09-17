using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Rebuild
{
    internal class RebuildVirtualContentViewsCommand : IMultistepActionStageCommand
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        private const int ItemsPerStep = 1;

        public int ContentId { get; }

        public string ContentName { get; }

        public List<Content.TreeItem> RebuildedSubViewContents { get; }

        private int _itemCount;

        public RebuildVirtualContentViewsCommand(MultistepActionStageCommandState state)
            : this(state.Id, null)
        {
        }

        public RebuildVirtualContentViewsCommand(int contentId, string contentName)
        {
            ContentId = contentId;
            ContentName = contentName;
        }

        public RebuildVirtualContentViewsCommand(int contentId, string contentName, List<Content.TreeItem> rebuildedSubViewContents)
            : this(contentId, contentName)
        {
            RebuildedSubViewContents = rebuildedSubViewContents;
        }

        internal void Setup()
        {
            var contentsToRebuild =
                RebuildedSubViewContents
                    .OrderBy(c => c.Level)
                    .Distinct(new LambdaEqualityComparer<Content.TreeItem>((x, y) => x.ContentId.Equals(y.ContentId), x => x.ContentId))
                    .Select(c => c.ContentId)
                    .ToArray();

            _itemCount = contentsToRebuild.Length;

            HttpContext.Session.SetValue(
                HttpContextSession.RebuildVirtualContentProcessingContext,
                new RebuildVirtualContentViewsCommandContext
                {
                    ContentIdsToRebuild = contentsToRebuild
                });
        }

        internal static void TearDown()
        {
            HttpContext.Session.Remove(HttpContextSession.RebuildVirtualContentProcessingContext);
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = RebuildVirtualContentsCommandTypes.RebuildViews,
            ParentId = 0,
            Id = ContentId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = _itemCount,
            StepCount = MultistepActionHelper.GetStepCount(_itemCount, ItemsPerStep),
            Name = string.Format(ContentStrings.RebuildVirtualContentViewsStageName, ContentName ?? string.Empty)
        };

        public MultistepActionStepResult Step(int step)
        {
            var context = HttpContext.Session.GetValue<RebuildVirtualContentViewsCommandContext>(HttpContextSession.RebuildVirtualContentProcessingContext);

            var ids = context.ContentIdsToRebuild
                .Skip(step * ItemsPerStep)
                .Take(ItemsPerStep)
                .ToArray();

            var helper = new VirtualContentHelper();
            var contents = ContentRepository.GetList(ids).ToDictionary(n => n.Id);
            foreach (var content in ids.Select(n => contents[n]))
            {
                helper.RebuildSubContentView(content);
            }

            return new MultistepActionStepResult { ProcessedItemsCount = ItemsPerStep };
        }
    }

    [Serializable]
    public class RebuildVirtualContentViewsCommandContext
    {
        public int[] ContentIdsToRebuild { get; set; }
    }
}
