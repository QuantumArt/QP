using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Rebuild
{
    internal class RebuildUserQueryCommand : IMultistepActionStageCommand
    {
        private const int ItemsPerStep = 10;

        public int ContentId { get; }

        public string ContentName { get; }

        public List<Content.TreeItem> RebuildedSubViewContents { get; }

        private int _itemCount;

        public RebuildUserQueryCommand(MultistepActionStageCommandState state)
            : this(state.Id, null)
        {
        }

        public RebuildUserQueryCommand(int contentId, string contentName)
        {
            ContentId = contentId;
            ContentName = contentName;
        }

        public RebuildUserQueryCommand(int contentId, string contentName, List<Content.TreeItem> rebuildedSubViewContents)
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
            HttpContext.Current.Session[HttpContextSession.RebuildUserQueryCommandProcessingContext] = new RebuildUserQueryCommandContext
            {
                ContentIdsToRebuild = contentsToRebuild.ToArray()
            };
        }

        internal static void TearDown()
        {
            HttpContext.Current.Session[HttpContextSession.RebuildUserQueryCommandProcessingContext] = null;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = RebuildVirtualContentsCommandTypes.RebuildUserQueries,
            ParentId = 0,
            Id = ContentId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = _itemCount,
            StepCount = MultistepActionHelper.GetStepCount(_itemCount, ItemsPerStep),
            Name = string.Format(ContentStrings.RebuildUserQueryStageName, ContentName ?? string.Empty)
        };

        public MultistepActionStepResult Step(int step)
        {
            var context = HttpContext.Current.Session[HttpContextSession.RebuildUserQueryCommandProcessingContext] as RebuildUserQueryCommandContext;
            var ids = context.ContentIdsToRebuild
                .Skip(step * ItemsPerStep)
                .Take(ItemsPerStep)
                .ToArray();

            var helper = new VirtualContentHelper();
            var contents = ContentRepository.GetList(ids).ToDictionary(n => n.Id);
            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(ContentId))
            {
                foreach (var content in ids.Select(n => contents[n]).Where(n => n.VirtualType == VirtualType.UserQuery))
                {
                    helper.UpdateUserQueryAsSubContent(content);
                }
            }

            return new MultistepActionStepResult { ProcessedItemsCount = ItemsPerStep };
        }
    }

    [Serializable]
    public class RebuildUserQueryCommandContext
    {
        public int[] ContentIdsToRebuild { get; set; }
    }
}
