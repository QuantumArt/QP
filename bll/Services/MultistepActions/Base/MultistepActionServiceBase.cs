#if !NET_STANDARD
using System;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Base
{
    public abstract class MultistepActionServiceBase<TCommand> : MultistepActionServiceAbstract, IActionCode
        where TCommand : MultistepActionStageCommandBase
    {
        private TCommand Command { get; set; }

        protected override MultistepActionSettings CreateActionSettings(int parentId, int id) => new MultistepActionSettings
        {
            Stages = new[]
            {
                Command.GetStageSettings()
            },

            ParentId = parentId
        };

        protected MultistepActionServiceContext CreateContext(int parentId, int id, int[] ids, bool? boundToExternal)
        {
            var action = BackendActionService.GetByCode(ActionCode);
            var itemsPerStep = action.EntityLimit ?? 1;
            var state = new MultistepActionStageCommandState
            {
                ParentId = parentId,
                Id = id,
                Ids = ids,
                ExtensionContents = ContentRepository.GetList(
                    ContentRepository.GetReferencedAggregatedContentIds(parentId, ids ?? new int[0])
                ).ToArray(),
                BoundToExternal = boundToExternal,
                ItemsPerStep = itemsPerStep
            };

            Command = (TCommand)CreateCommand(state);
            return new MultistepActionServiceContext { CommandStates = new[] { Command.GetState() } };
        }

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal) => CreateContext(parentId, id, new int[0], boundToExternal);

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
            var command = Activator.CreateInstance<TCommand>();
            command.Initialize(state);
            return command;
        }

        protected override string ContextSessionKey => $"{GetType().Name}.Settings";

        public override MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal)
        {
            if (HasAlreadyRun())
            {
                throw new ApplicationException(MultistepActionStrings.ActionHasAlreadyRun);
            }

            var context = CreateContext(parentId, id, ids, boundToExternal);
            HttpContext.Current.Session[ContextSessionKey] = context;

            return CreateActionSettings(parentId, id);
        }

        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal) => Setup(parentId, id, new int[0], boundToExternal);

        public override void TearDown()
        {
            MultistepActionStageCommandBase.TearDown();
            base.TearDown();
        }

        public abstract string ActionCode { get; }
    }
}
#endif
