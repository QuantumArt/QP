using System;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using System.Text.RegularExpressions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Repository;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL
{

	public class ObjectFormat: LockableEntityObject
	{
		[LocalizedDisplayName("CodeBehind", NameResourceType = typeof(ObjectFormatStrings))]
		public string CodeBehind { get; set; }

		[LocalizedDisplayName("Presentation", NameResourceType = typeof(ObjectFormatStrings))]
		public string FormatBody { get; set; }

		public int ObjectId { get; set; }

		public DateTime? Assembled { get; set; }

		public bool AssembleInLive { get; set; }

		public bool AssembleInStage { get; set; }

		public bool AssembleNotificationInLive { get; set; }

		public bool AssembleNotificationInStage { get; set; }

		public bool AssemblePreviewInLive { get; set; }

		public bool AssemblePreviewInStage { get; set; }

		[LocalizedDisplayName("NetClassName", NameResourceType = typeof(TemplateStrings))]
		public string NetFormatName { get; set; }

		[LocalizedDisplayName("NetLanguage", NameResourceType = typeof(TemplateStrings))]
		public int? NetLanguageId { get; set; }

		public IEnumerable<Notification> Notifications { get; set; }

		public bool PageOrTemplate { get; set; }

		public bool IsSiteDotNet { get; set; }

		public override string EntityTypeCode
		{
			get
			{
				return PageOrTemplate ? Constants.EntityTypeCode.PageObjectFormat : Constants.EntityTypeCode.TemplateObjectFormat;
			}
		}		

		public override int ParentEntityId
		{
			get
			{
				return ObjectId;
			}
		}

		internal static ObjectFormat Create(int parentId, bool pageOrTemplate, bool isSiteDotNet)//true-page false-template
		{
			var format = new ObjectFormat();
			format.ObjectId = parentId;
			format.PageOrTemplate = pageOrTemplate;
			format.IsSiteDotNet = isSiteDotNet;
			if (isSiteDotNet)
				format.NetLanguageId = NetLanguage.GetcSharp().Id;
			return format;
		}

		public override void Validate()
		{
			var errors = new RulesException<ObjectFormat>();

			base.Validate(errors);

			if (!string.IsNullOrWhiteSpace(NetFormatName) && IsSiteDotNet)
			{
				if (!Regex.IsMatch(NetFormatName, RegularExpressions.NetName))
					errors.ErrorFor(x => x.NetFormatName, TemplateStrings.NetNameInvalidFormat);
				if (!PageTemplateRepository.ObjectFormatNetNameUnique(NetFormatName, ObjectId, Id))
					errors.ErrorFor(x => x.NetFormatName, TemplateStrings.NetNameNotUnique);
				if (NetFormatName.Length > 255)
					errors.ErrorFor(x => x.NetFormatName, TemplateStrings.NetNameMaxLengthExceeded);
			}

			if (NetLanguageId != null)
			{
				if (string.IsNullOrWhiteSpace(CodeBehind))
					errors.ErrorFor(n => n.CodeBehind, ObjectFormatStrings.CodeBehindRequired);
			}

			if (!errors.IsEmpty)
				throw errors;
		}

		public override string LockedByAnyoneElseMessage
		{
			get { return String.Format(TemplateStrings.FormatLockedByAnyoneElse, LockedBy); }
		}

		private Dictionary<string, string> _siteUrls = null;

		private Dictionary<string, string> siteUrls
		{
			get
			{
				if (_siteUrls == null)
					_siteUrls = SiteUrlHelper.GetSiteUrlsByObjectId(ObjectId);
				return _siteUrls;
			}
		}

		public void ReplacePlaceHoldersToUrls()
		{
			if (!String.IsNullOrEmpty(FormatBody))
			{
				FormatBody
					.Replace(UploadUrlPlaceHolder, siteUrls["ImagesLongUploadUrl"])
					.Replace(SiteUrlPlaceHolder, siteUrls["CurrentUrl"]);
			}
		}

		public void ReplaceUrlsToPlaceHolders()
		{
			if (!String.IsNullOrEmpty(FormatBody))
			{
				FormatBody
					.Replace(siteUrls["ImagesLongUploadUrl"], UploadUrlPlaceHolder)
					.Replace(siteUrls["StageUrl"], SiteUrlPlaceHolder)
					.Replace(siteUrls["LiveUrl"], SiteUrlPlaceHolder);
			}
		}

		private string UploadUrlPlaceHolder
		{
			get { return "<%=upload_url%>"; }
		}

		private string SiteUrlPlaceHolder
		{
			get { return "<%=site_url%>"; }
		}
	}

	public class NotificationObjectFormat : ObjectFormat
	{
		public new string EntityTypeCode
		{
			get
			{
				return Constants.EntityTypeCode.TemplateObjectFormat;
			}
		}
	}
}
