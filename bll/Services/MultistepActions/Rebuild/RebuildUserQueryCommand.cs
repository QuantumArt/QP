using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Rebuild
{
	internal class RebuildUserQueryCommand : IMultistepActionStageCommand
	{
		private static readonly int ITEMS_PER_STEP = 10;
		private static readonly string SESSION_KEY = "RebuildUserQueryCommand.ProcessingContext";

		public int ContentId { get; private set; }
		public string ContentName { get; private set; }
		public List<Content.TreeItem> RebuildedSubViewContents { get; private set; }

		private int itemCount = 0;

		public RebuildUserQueryCommand(MultistepActionStageCommandState state) : this(state.Id, null) { }

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
				.ToArray()
			;

			itemCount = contentsToRebuild.Count();

			HttpContext.Current.Session[SESSION_KEY] = new RebuildUserQueryCommandContext
			{
				ContentIdsToRebuild = contentsToRebuild.ToArray()
			};
		}

		internal static void TearDown()
		{
			HttpContext.Current.Session[SESSION_KEY] = null;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = RebuildVirtualContentsCommandTypes.RebuildUserQueries,
				ParentId = 0,
				Id = ContentId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = itemCount,
				StepCount = MultistepActionHelper.GetStepCount(itemCount, ITEMS_PER_STEP),
				Name = String.Format(ContentStrings.RebuildUserQueryStageName, (ContentName ?? ""))
			};
		}

		#region IMultistepActionStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			RebuildUserQueryCommandContext context = HttpContext.Current.Session[SESSION_KEY] as RebuildUserQueryCommandContext;


			int[] ids = context.ContentIdsToRebuild
				.Skip(step * ITEMS_PER_STEP)
				.Take(ITEMS_PER_STEP)
				.ToArray();

			Dictionary<int, Content> contents = ContentRepository.GetList(ids)
				.ToDictionary(n => n.Id);

			var helper = new VirtualContentHelper();
			using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(ContentId))
			{
				foreach (Content content in ids.Select(n => contents[n]).Where(n => n.VirtualType == VirtualType.UserQuery))
				{
					helper.UpdateUserQueryAsSubContent(content);
				}
			}
			return new MultistepActionStepResult { ProcessedItemsCount = ITEMS_PER_STEP };
		}

		#endregion


	}

	[Serializable]
	public class RebuildUserQueryCommandContext
	{
		public int[] ContentIdsToRebuild { get; set; }
	}
}
