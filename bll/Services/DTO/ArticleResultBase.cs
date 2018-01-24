﻿namespace Quantumart.QP8.BLL.Services.DTO
{
    public abstract class ArticleResultBase : InitListResultBase
    {
        public string ContentName { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsUpdatable { get; set; }
        public bool IsArticleChangingActionsAllowed { get; set; }
        public bool ContentDisableChangingActions { get; set; }
    }
}
