using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;

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

		public bool IsNew { get { return LinkId == default(int); } }

		[LocalizedDisplayName("Symmetric", NameResourceType = typeof(FieldStrings))]	
		public bool Symmetric { get; set; }

		[LocalizedDisplayName("MapLinkAsClass", NameResourceType = typeof(FieldStrings))]		
		public bool MapAsClass { get; set; }

		[LocalizedDisplayName("NetLinkName", NameResourceType = typeof(FieldStrings))]
		[FormatValidator(Constants.RegularExpressions.NetName, MessageTemplateResourceName = "NetLinkNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "NetLinkNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]				
		public string NetLinkName { get; set; }
		
		[LocalizedDisplayName("NetPluralLinkName", NameResourceType = typeof(FieldStrings))]
		[FormatValidator(Constants.RegularExpressions.NetName, MessageTemplateResourceName = "NetPluralLinkNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "NetPluralLinkNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]				
		public string NetPluralLinkName { get; set; }

		private Content _Content;
		public Content Content
		{
			get
			{
				if (_Content == null)
					_Content = ContentRepository.GetById(LContentId);
				return _Content;
			}
			set
			{
				_Content = value;
			}
		}

		internal void MutateNames()
		{
			if (!String.IsNullOrEmpty(NetLinkName))
			{
				string name = NetLinkName;
				int index = 0;
				do
				{
					index++;
					NetLinkName = MutateHelper.MutateNetName(name, index);
				}
				while (FieldRepository.NetNameExists(this));
			}

			if (!String.IsNullOrEmpty(NetPluralLinkName))
			{
				string name = NetPluralLinkName;
				int index = 0;
				do
				{
					index++;
					NetPluralLinkName = MutateHelper.MutateNetName(name, index);
				}
				while (FieldRepository.NetPluralNameExists(this));
			}
		}

		internal ContentLink GetBackwardLink()
		{
			ContentLink result = (ContentLink)this.MemberwiseClone();
			int temp = result.LContentId;
			result.LContentId = result.RContentId;
			result.RContentId = temp;
			return result;
		}

		public ContentLink Clone(int sourceId, int destinationId)
		{
			ContentLink link = (ContentLink)this.MemberwiseClone();
			link.LinkId = 0;
			link.MutateNames();
			if (link.LContentId == sourceId)
				link.LContentId = destinationId;
			if (link.RContentId == sourceId)
				link.RContentId = destinationId;
			return link;
		}
	}
}
