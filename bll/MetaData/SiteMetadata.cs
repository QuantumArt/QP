using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Quantumart.QP8;
using Quantumart.QP8.BLL.Resources;
using Quantumart.QP8.BLL.Validators;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL.Metadata
{
	public class SiteMetadata
	{
		[RequiredValidator(MessageTemplateResourceName = "DnsNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "DnsMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.DomainName, MessageTemplateResourceName = "DnsInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("DNS", NameResourceType = typeof(SiteStrings))]
		public string Dns
		{
			get;
			set;
		}

        [MaxLengthValidator(255, MessageTemplateResourceName = "StageDnsMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[CustomValidator(typeof(SiteValidators), "ValidateStageDnsInput", MessageTemplateResourceName = "StageDnsNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.DomainName, MessageTemplateResourceName = "StageDnsInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string StageDns
		{
			get;
			set;
		}

		[RequiredValidator(MessageTemplateResourceName = "UploadUrlNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "UploadUrlMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.RelativeWebFolderUrl, MessageTemplateResourceName = "UploadUrlInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string UploadUrl
		{
			get;
			set;
		}

		[MaxLengthValidator(255, MessageTemplateResourceName = "UploadUrlPrefixMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.AbsoluteWebFolderUrl, MessageTemplateResourceName = "UploadUrlPrefixInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string UploadUrlPrefix
		{
			get;
			set;
		}

		[RequiredValidator(MessageTemplateResourceName = "UploadDirNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
		[StringLengthValidator(255, MessageTemplateResourceName = "UploadDirMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "UploadDirInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string UploadDir
		{
			get;
			set;
		}

		[RequiredValidator(MessageTemplateResourceName = "LiveVirtualRootNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "LiveVirtualRootMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.RelativeWebFolderUrl, MessageTemplateResourceName = "LiveVirtualRootInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string LiveVirtualRoot
		{
			get;
			set;
		}

		[RequiredValidator(MessageTemplateResourceName = "LiveDirectoryNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "LiveDirectoryMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "LiveDirectoryInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string LiveDirectory
		{
			get;
			set;
		}

        [MaxLengthValidator(255, MessageTemplateResourceName = "TestDirectoryMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "TestDirectoryInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string TestDirectory
		{
			get;
			set;
		}

		[RequiredValidator(MessageTemplateResourceName = "StageVirtualRootNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "StageVirtualRootMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.RelativeWebFolderUrl, MessageTemplateResourceName = "StageVirtualRootInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string StageVirtualRoot
		{
			get;
			set;
		}

		[RequiredValidator(MessageTemplateResourceName = "StageDirectoryNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "StageDirectoryMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "StageDirectoryInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string StageDirectory
		{
			get;
			set;
		}

		[RequiredValidator(MessageTemplateResourceName = "AssemblyPathNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "AssemblyPathMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "AssemblyPathInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string AssemblyPath
		{
			get;
			set;
		}

		[RequiredValidator(MessageTemplateResourceName = "StageAssemblyPathNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "StageAssemblyPathMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
		[FormatValidator(Constants.RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "StageAssemblyPathInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
		public string StageAssemblyPath
		{
			get;
			set;
		}
	}
}
