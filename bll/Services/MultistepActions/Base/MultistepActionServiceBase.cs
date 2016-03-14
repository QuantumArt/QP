using Quantumart.QP8.BLL.Services.MultistepActions.Export;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Base
{
	public abstract class MultistepActionServiceBase<TCommand> : MultistepActionServiceAbstract, IActionCode
		where TCommand : MultistepActionStageCommandBase
	{
		private TCommand Command { get; set; }

		protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
		{
			return new MultistepActionSettings
			{
				Stages = new[] 
				{ 
                    Command.GetStageSettings()
				},

				ParentId = parentId
			};
		}

		protected MultistepActionServiceContext CreateContext(int parentId, int id, int[] ids, bool? boundToExternal)
		{
			var action = BackendActionService.GetByCode(ActionCode);
			int itemsPerStep = action.EntityLimit ?? 1;
			var state = new MultistepActionStageCommandState() { ParentId = parentId, Id = id, Ids = ids, BoundToExternal = boundToExternal, ItemsPerStep = itemsPerStep };
						
			Command = (TCommand)CreateCommand(state);
			return new MultistepActionServiceContext { CommandStates = new[] { Command.GetState() } };
		}

		protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
		{
			return CreateContext(parentId, id, new int[0], boundToExternal);
		}

		protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
		{
			var command = Activator.CreateInstance<TCommand>();
			command.Initialize(state);
			return command;
		}

		protected override string ContextSessionKey
		{
			get { return GetType().Name + ".Settings"; }
		}

		public override MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal)
		{
			if (HasAlreadyRun())
				throw new ApplicationException(MultistepActionStrings.ActionHasAlreadyRun);

			MultistepActionServiceContext context = CreateContext(parentId, id, ids, boundToExternal);
			HttpContext.Current.Session[ContextSessionKey] = context;

			MultistepActionSettings settings = CreateActionSettings(parentId, id);
			return settings;
		}

		public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal)
		{
			return Setup(parentId, id, new int[0], boundToExternal);
		}

		public override void TearDown()
		{
			MultistepActionStageCommandBase.TearDown();
			base.TearDown();
		}		

		public abstract string ActionCode { get; }
	}
}
