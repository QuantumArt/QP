using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Interfaces.Db;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class ContentLink : BizObject
    {
        public ContentLink()
        {
            Symmetric = true;
            WasNew = false;
        }

        public int LinkId { get; set; }

        public int ForceLinkId { get; set; }

        public int LContentId { get; set; }

        public int RContentId { get; set; }

        public bool WasNew { get; set; }

        public bool IsNew => LinkId == default(int);

        [LocalizedDisplayName("Symmetric", NameResourceType = typeof(FieldStrings))]
        public bool Symmetric { get; set; }

        [LocalizedDisplayName("MapLinkAsClass", NameResourceType = typeof(FieldStrings))]
        public bool MapAsClass { get; set; }

        [LocalizedDisplayName("NetLinkName", NameResourceType = typeof(FieldStrings))]
        [FormatValidator(RegularExpressions.NetName, MessageTemplateResourceName = "NetLinkNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NetLinkNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string NetLinkName { get; set; }

        [LocalizedDisplayName("NetPluralLinkName", NameResourceType = typeof(FieldStrings))]
        [FormatValidator(RegularExpressions.NetName, MessageTemplateResourceName = "NetPluralLinkNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NetPluralLinkNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string NetPluralLinkName { get; set; }

        private Content _content;

        public Content Content
        {
            get { return _content ?? (_content = ContentRepository.GetById(LContentId)); }
            set
            {
                _content = value;
            }
        }

        internal void MutateNames()
        {
            if (!string.IsNullOrEmpty(NetLinkName))
            {
                var name = NetLinkName;
                var index = 0;
                do
                {
                    index++;
                    NetLinkName = MutateHelper.MutateNetName(name, index);
                }
                while (((IFieldRepository)new FieldRepository()).NetNameExists(this));
            }

            if (!string.IsNullOrEmpty(NetPluralLinkName))
            {
                var name = NetPluralLinkName;
                var index = 0;
                do
                {
                    index++;
                    NetPluralLinkName = MutateHelper.MutateNetName(name, index);
                }
                while (((IFieldRepository)new FieldRepository()).NetPluralNameExists(this));
            }
        }

        internal ContentLink GetBackwardLink()
        {
            var result = (ContentLink)MemberwiseClone();
            var temp = result.LContentId;
            result.LContentId = result.RContentId;
            result.RContentId = temp;
            return result;
        }

        public ContentLink Clone(int sourceId, int destinationId)
        {
            var link = (ContentLink)MemberwiseClone();
            link.LinkId = 0;
            link.MutateNames();

            if (link.LContentId == sourceId)
            {
                link.LContentId = destinationId;
            }

            if (link.RContentId == sourceId)
            {
                link.RContentId = destinationId;
            }

            return link;
        }
    }
}
