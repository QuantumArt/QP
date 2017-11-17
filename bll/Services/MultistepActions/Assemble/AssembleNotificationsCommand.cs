using System.Linq;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
    internal class AssembleNotificationsCommand : IMultistepActionStageCommand
    {
        public bool SiteOrTemplate { get; }
        public int AssemblingEntityId { get; }
        public string AssemblingEntityName { get; }

        public AssembleNotificationsCommand(MultistepActionStageCommandState state, bool siteOrTemplate)
            : this(state.Id, null, siteOrTemplate)
        {
        }

        public AssembleNotificationsCommand(int assemblingEntityId, string assemblingEntityName, bool siteOrTemplate)
        {
            SiteOrTemplate = siteOrTemplate;
            AssemblingEntityId = assemblingEntityId;
            AssemblingEntityName = assemblingEntityName;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = BuildSiteStageCommandTypes.BuildNotifications,
            ParentId = 0,
            Id = AssemblingEntityId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = 1,
            StepCount = 1,
            Name = SiteOrTemplate ? string.Format(SiteStrings.AssembleNotificationStageName, AssemblingEntityName ?? "") : string.Format(TemplateStrings.AssembleNotificationsStageName, AssemblingEntityName ?? "")
        };

        #region IMultistepActionStageCommand Members

        public MultistepActionStepResult Step(int step)
        {
            var notificationIds = SiteOrTemplate ? AssembleRepository.GetSiteFormatId(AssemblingEntityId) : AssembleRepository.GetTemplateFormatId(AssemblingEntityId);
            if (notificationIds.Any())
            {
                foreach (var id in notificationIds)
                {
                    new AssembleFormatController(id, AssembleMode.Notification, QPContext.CurrentDbConnectionString).Assemble();
                }
            }

            return new MultistepActionStepResult { ProcessedItemsCount = 1 };
        }

        #endregion
    }
}
