#if !NET_STANDARD
using System;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
    public abstract class RemovingServiceAbstract : MultistepActionServiceAbstract
    {
        protected override string ContextSessionKey => "RemovingService.ProcessingContext";

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
            switch (state.Type)
            {
                case RemovingStageCommandTypes.ClearContent:
                    return new ClearContentCommand(state);
                case RemovingStageCommandTypes.RemoveContent:
                    return new RemoveContentCommand(state);
                case RemovingStageCommandTypes.RemoveSite:
                    return new RemoveSiteCommand(state);
                case RemovingStageCommandTypes.RemoveSiteArticles:
                    return new RemoveSiteArticlesCommand(state);
                case RemovingStageCommandTypes.RemoveSiteContents:
                    return new RemoveSiteContentsCommand(state);
                default:
                    throw new ApplicationException("Undefined Removing Stage Command Type: " + state.Type);
            }
        }
    }

    internal class RemovingStageCommandTypes
    {
        public const int ClearContent = 1;
        public const int RemoveContent = 2;
        public const int RemoveSiteArticles = 3;
        public const int RemoveSiteContents = 4;
        public const int RemoveSite = 5;
    }
}
#endif
