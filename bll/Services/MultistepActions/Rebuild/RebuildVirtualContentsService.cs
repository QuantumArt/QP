using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Rebuild
{
	public sealed class RebuildVirtualContentsService : MultistepActionServiceAbstract
	{
		RebuildVirtualContentViewsCommand rebuildViewsCommand;
		RebuildUserQueryCommand rebuildUserQueryCommand;

		public override MultistepActionSettings Setup(int siteId, int contentId, bool? boundToExternal)
		{
			Content content = ContentRepository.GetById(contentId);
			if (content == null)
				throw new ApplicationException(String.Format(SiteStrings.SiteNotFound, siteId));

			List<Content.TreeItem> rebuildedViewSubContents;
			var helper = new VirtualContentHelper();
			using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(contentId))
			{
				rebuildedViewSubContents = helper.TraverseForUpdateVirtualSubContents(content);
			}

			rebuildViewsCommand = new RebuildVirtualContentViewsCommand(contentId, content.Name, rebuildedViewSubContents);
			rebuildViewsCommand.Setup();

			rebuildUserQueryCommand = new RebuildUserQueryCommand(contentId, content.Name, rebuildedViewSubContents);
			rebuildUserQueryCommand.Setup();

			return base.Setup(siteId, contentId, boundToExternal);
		}


		protected override MultistepActionSettings CreateActionSettings(int siteId, int contentId)
		{
			return new MultistepActionSettings
			{
				Stages = new[]
				{
					rebuildViewsCommand.GetStageSettings(),
					rebuildUserQueryCommand.GetStageSettings()
				}
			};
		}

		protected override MultistepActionServiceContext CreateContext(int siteId, int contentId, bool? boundToExternal)
		{
			return new MultistepActionServiceContext
			{
				CommandStates = new[]
				{
					rebuildViewsCommand.GetState(),
					rebuildUserQueryCommand.GetState()
				}
			};
		}

		protected override string ContextSessionKey
		{
			get { return "RebuildUserQuery.ProcessingContext"; }
		}

		protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
		{
			switch (state.Type)
			{
				case RebuildVirtualContentsCommandTypes.RebuildViews:
					return new RebuildVirtualContentViewsCommand(state);
				case RebuildVirtualContentsCommandTypes.RebuildUserQueries:
					return new RebuildUserQueryCommand(state);
				default:
					throw new ApplicationException("Undefined Rebuilding Virtual Contents Stage Command Type: " + state.Type);
			}
		}

		public override void TearDown()
		{
			RebuildVirtualContentViewsCommand.TearDown();
			RebuildUserQueryCommand.TearDown();
			base.TearDown();
		}
	}

	internal class RebuildVirtualContentsCommandTypes
	{
		public const int RebuildViews = 1;
		public const int RebuildUserQueries = 2;

	}
}
