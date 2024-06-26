using System;
using System.Linq;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Base
{
    public abstract class MultistepActionServiceBase<TCommand> : MultistepActionServiceAbstract, IActionCode
        where TCommand : MultistepActionStageCommandBase
    {
        private TCommand Command { get; set; }

        protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
        {
            var result = new MultistepActionSettings() { ParentId = parentId };
            result.Stages.Add(Command.GetStageSettings());
            return result;
        }

        protected MultistepActionServiceContext CreateContext(int parentId, int id, int[] ids, bool? boundToExternal, S3Options options)
        {
            var action = BackendActionService.GetByCode(ActionCode);
            var itemsPerStep = action.EntityLimit ?? 1;
            var state = new MultistepActionStageCommandState
            {
                ParentId = parentId,
                Id = id,
                Ids = ids.ToList(),
                ExtensionContentIds = ContentRepository.GetReferencedAggregatedContentIds(parentId, ids).ToList(),
                BoundToExternal = boundToExternal,
                ItemsPerStep = itemsPerStep,
                S3Options = options
            };

            Command = (TCommand)CreateCommand(state);
            var result = new MultistepActionServiceContext();
            result.CommandStates.Add(Command.GetState());
            return result;
        }

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal, S3Options options)
        {
            return CreateContext(parentId, id, new int[0], boundToExternal, options);
        }

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
            var command = Activator.CreateInstance<TCommand>();
            command.Initialize(state);
            return command;
        }

        protected override string ContextSessionKey => $"{GetType().Name}.Settings";

        public override MultistepActionSettings Setup(int parentId, int id, int[] ids, bool? boundToExternal, S3Options options)
        {
            if (HasAlreadyRun())
            {
                throw new ApplicationException(MultistepActionStrings.ActionHasAlreadyRun);
            }

            var context = CreateContext(parentId, id, ids, boundToExternal, options);
            HttpContext.Session.SetValue(ContextSessionKey, context);

            return CreateActionSettings(parentId, id);
        }

        public override MultistepActionSettings Setup(int parentId, int id, bool? boundToExternal, S3Options options)
        {
            return Setup(parentId, id, new int[0], boundToExternal, options);
        }

        public override void TearDown()
        {
            MultistepActionStageCommandBase.TearDown();
            base.TearDown();
        }

        public abstract string ActionCode { get; }
    }
}
