using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

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

        [Display(Name = "Symmetric", ResourceType = typeof(FieldStrings))]
        public bool Symmetric { get; set; }

        [Display(Name = "MapLinkAsClass", ResourceType = typeof(FieldStrings))]
        public bool MapAsClass { get; set; }

        [Display(Name = "NetLinkName", ResourceType = typeof(FieldStrings))]
        [RegularExpression(RegularExpressions.NetName, ErrorMessageResourceName = "NetLinkNameInvalidFormat", ErrorMessageResourceType = typeof(FieldStrings))]
        [StringLength(255, ErrorMessageResourceName = "NetLinkNameMaxLengthExceeded", ErrorMessageResourceType = typeof(FieldStrings))]
        public string NetLinkName { get; set; }

        [Display(Name = "NetPluralLinkName", ResourceType = typeof(FieldStrings))]
        [RegularExpression(RegularExpressions.NetName, ErrorMessageResourceName = "NetPluralLinkNameInvalidFormat", ErrorMessageResourceType = typeof(FieldStrings))]
        [StringLength(255, ErrorMessageResourceName = "NetPluralLinkNameMaxLengthExceeded", ErrorMessageResourceType = typeof(FieldStrings))]
        public string NetPluralLinkName { get; set; }

        private Content _content;

        public Content Content
        {
            get => _content ?? (_content = ContentRepository.GetById(LContentId));
            set => _content = value;
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
                } while (((IFieldRepository)new FieldRepository()).NetNameExists(this));
            }

            if (!string.IsNullOrEmpty(NetPluralLinkName))
            {
                var name = NetPluralLinkName;
                var index = 0;
                do
                {
                    index++;
                    NetPluralLinkName = MutateHelper.MutateNetName(name, index);
                } while (((IFieldRepository)new FieldRepository()).NetPluralNameExists(this));
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
