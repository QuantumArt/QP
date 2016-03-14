using QA_Merger;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
	public class ObjectFormatVersion : EntityObject
	{
		[LocalizedDisplayName("CodeBehind", NameResourceType = typeof(ObjectFormatStrings))]
		public string CodeBehind { get; set; }

		[LocalizedDisplayName("Presentation", NameResourceType = typeof(ObjectFormatStrings))]
		public string FormatBody { get; set; }

		[LocalizedDisplayName("NetClassName", NameResourceType = typeof(TemplateStrings))]
		public string NetFormatName { get; set; }

		public NetLanguage NetLanguage { get; set; }

		[LocalizedDisplayName("NetLanguage", NameResourceType = typeof(TemplateStrings))]
		public string NetLanguageName { get { return NetLanguage.Name; } }

		public ObjectFormat ObjectFormat { get; set; }

		[LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
		public string NameToMerge { get; set; }

		[LocalizedDisplayName("NetClassName", NameResourceType = typeof(TemplateStrings))]
		public string NetFormatNameToMerge { get; set; }

		[LocalizedDisplayName("Description", NameResourceType = typeof(EntityObjectStrings))]
		public string DescriptionToMerge { get; set; }

		[LocalizedDisplayName("NetLanguage", NameResourceType = typeof(TemplateStrings))]
		public string NetLanguageNameToMerge { get; set; }

		[LocalizedDisplayName("Presentation", NameResourceType = typeof(ObjectFormatStrings))]
		public string PresentationToMerge { get; set; }

		[LocalizedDisplayName("CodeBehind", NameResourceType = typeof(ObjectFormatStrings))]
		public string CodeBehindToMerge { get; set; }

		public int? NetLanguageId { get; set; }
		/// <summary>
		/// Фальшивый идентификатор для текущей версии
		/// </summary>
		public static readonly int CurrentVersionId = 1;

		/// <summary>
		/// Версия, с которой происходит сравнение
		/// </summary>
		public ObjectFormatVersion VersionToMerge { get; set; }
				
		/// <summary>
		/// ID версии, с которой происходит сравнение
		/// </summary>
		public int MergedId
		{
			get
			{
				return (VersionToMerge == null) ? 0 : VersionToMerge.Id;
			}
		}

		/// <summary>
		/// Осуществляет слияние с указанной версией для реализации сравнения
		/// </summary>
		/// <param name="versionToMerge">версия для слияния</param>
		internal void MergeToVersion(ObjectFormatVersion versionToMerge)
		{			
			NameToMerge = ObjectFormatVersion.Merge(Formatter.ProtectHtml(Name), Formatter.ProtectHtml(versionToMerge.Name));
			NetFormatNameToMerge = ObjectFormatVersion.Merge(Formatter.ProtectHtml(NetFormatName), Formatter.ProtectHtml(versionToMerge.NetFormatName));
			DescriptionToMerge = ObjectFormatVersion.Merge(Formatter.ProtectHtml(Description), Formatter.ProtectHtml(versionToMerge.Description));
			NetLanguageNameToMerge = ObjectFormatVersion.Merge(Formatter.ProtectHtml(NetLanguage.Name), Formatter.ProtectHtml(versionToMerge.NetLanguage.Name));
			PresentationToMerge = ObjectFormatVersion.Merge(Formatter.ProtectHtml(FormatBody), Formatter.ProtectHtml(versionToMerge.FormatBody));
			CodeBehindToMerge = ObjectFormatVersion.Merge(Formatter.ProtectHtml(CodeBehind), Formatter.ProtectHtml(versionToMerge.CodeBehind));
			VersionToMerge = versionToMerge;
		}

		/// <summary>
		/// Осуществляет слияние 2 строк с помощью QA_Merger
		/// </summary>
		/// <param name="s1">строка 1</param>
		/// <param name="s2">строка 2</param>
		/// <returns>результат слияния</returns>
		public static string Merge(string s1, string s2)
		{
			string prefix = "<html><body>";
			string suffix = "</body></html>";
			string mergeFormat = "{0}{1}{2}";
			Merger merger = new Merger(String.Format(mergeFormat, prefix, s1, suffix), String.Format(mergeFormat, prefix, s2, suffix));
			string result = merger.merge();
			return result.Replace(prefix, "").Replace(suffix, "");
		}

		[LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
		public string ExpandedName
		{
			get
			{
				return (Id == CurrentVersionId || Id == 0) ? ArticleStrings.CurrentVersion : String.Format(ArticleStrings.VersionN, Id);
			}
		}
	}
}
