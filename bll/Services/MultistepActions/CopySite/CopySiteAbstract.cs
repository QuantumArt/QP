#if !NET_STANDARD
using System;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public abstract class CopySiteAbstract : MultistepActionServiceAbstract
    {
        protected override string ContextSessionKey => "CopySiteService.ProcessingContext";

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
            switch (state.Type)
            {
                case CopySiteStageCommandTypes.CopySiteContents:
                    return new CopySiteContentsCommand(state);
                case CopySiteStageCommandTypes.CopySiteVirtualContents:
                    return new CopySiteVirtualContentsCommand(state);
                case CopySiteStageCommandTypes.CopySiteContentLinks:
                    return new CopySiteContentLinksCommand(state);
                case CopySiteStageCommandTypes.CopySiteArticles:
                    return new CopySiteArticlesCommand(state);
                case CopySiteStageCommandTypes.CopySiteTemplates:
                    return new CopySiteTemlatesCommand(state);
                case CopySiteStageCommandTypes.CopySiteFiles:
                    return new CopySiteFilesCommand(state);
                case CopySiteStageCommandTypes.CopySiteSettings:
                    return new CopySiteSettingsCommand(state);
                case CopySiteStageCommandTypes.CopySiteItemLinks:
                    return new CopySiteItemLinksCommand(state);
                case CopySiteStageCommandTypes.CopySiteUpdateArticleIds:
                    return new CopySiteUpdateArticleIdsCommand(state);
                default:
                    throw new ApplicationException("Undefined Copy Site Stage Command Type: " + state.Type);
            }
        }
    }

    internal class CopySiteStageCommandTypes
    {
        public const int CopySiteParams = 1;
        public const int CopySiteContents = 2;
        public const int CopySiteVirtualContents = 3;
        public const int CopySiteContentLinks = 4;
        public const int CopySiteArticles = 5;
        public const int CopySiteTemplates = 6;
        public const int CopySiteFiles = 7;
        public const int CopySiteSettings = 8;
        public const int CopySiteItemLinks = 9;
        public const int CopySiteUpdateArticleIds = 10;
    }
}
#endif
