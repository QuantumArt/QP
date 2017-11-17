using System.Collections.Generic;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class ContentForm : LockableEntityObject
    {
        public int ObjectId { get; set; }

        [LocalizedDisplayName("Content", NameResourceType = typeof(TemplateStrings))]
        public int? ContentId { get; set; } //nullable для работы формы new object

        [LocalizedDisplayName("GenerateCode", NameResourceType = typeof(TemplateStrings))]
        public bool GenerateUpdateScript { get; set; }

        [LocalizedDisplayName("NetLanguage", NameResourceType = typeof(TemplateStrings))]
        public int NetLanguageId { get; set; }

        [LocalizedDisplayName("SubmissionResponsePage", NameResourceType = typeof(TemplateStrings))]
        public int? ThankYouPageId { get; set; }

        public Content Content { get; set; }

        public object Object { get; set; }

        public Page Page { get; set; }

        public IEnumerable<NetLanguage> NetLanguages { get; set; }

        public override string LockedByAnyoneElseMessage => string.Format("Content Form is locked by user {0}", LockedByUser.Name);
    }
}
