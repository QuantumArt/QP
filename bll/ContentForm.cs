using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class ContentForm : LockableEntityObject
    {
        public int ObjectId { get; set; }

        [Display(Name = "Content", ResourceType = typeof(TemplateStrings))]
        public int? ContentId { get; set; } //nullable для работы формы new object

        [Display(Name = "GenerateCode", ResourceType = typeof(TemplateStrings))]
        public bool GenerateUpdateScript { get; set; }

        [Display(Name = "NetLanguage", ResourceType = typeof(TemplateStrings))]
        public int NetLanguageId { get; set; }

        [Display(Name = "SubmissionResponsePage", ResourceType = typeof(TemplateStrings))]
        public int? ThankYouPageId { get; set; }

        public Content Content { get; set; }

        public object Object { get; set; }

        public Page Page { get; set; }

        public IEnumerable<NetLanguage> NetLanguages { get; set; }

        public override string LockedByAnyoneElseMessage => string.Format("Content Form is locked by user {0}", LockedByUser.Name);
    }
}
