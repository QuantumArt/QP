using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services
{
	public interface IFingerprintService
	{
		byte[] GetFingerprint(XDocument xsettings);
		byte[] GetFingerprint(IEnumerable<FingerprintEntityTypeSettings> settings);
	}
	public class FingerprintService : IFingerprintService
	{
		private readonly IFingerprintRepository fingerprintRepository;

		public FingerprintService() : this(new FingerprintRepository())
		{
		}

		public FingerprintService(IFingerprintRepository fingerprintRepository)
		{
			if (fingerprintRepository == null)
				throw new ArgumentNullException("fingerprintRepository");

			this.fingerprintRepository = fingerprintRepository;
		}

		#region IFingerprintService Members
		public byte[] GetFingerprint(XDocument xsettings)
		{
			if (xsettings == null)
				throw new ArgumentNullException("xsettings");

			IEnumerable<FingerprintEntityTypeSettings> settings = ConvertToEntityTypeSettings(xsettings);
			return GetFingerprint(settings);
		}

		public byte[] GetFingerprint(IEnumerable<FingerprintEntityTypeSettings> settings)
		{
			using (HashAlgorithm hasher = MD5CryptoServiceProvider.Create())
			{
				hasher.Initialize();

				Action<IEnumerable<object>> rowEnumerator = (row) =>
				{
					string str = String.Join("", row.Select(c =>
						c is IFormattable ? (c as IFormattable).ToString(null, CultureInfo.InvariantCulture) : c.ToString()
					));
					byte[] buffer = Encoding.Unicode.GetBytes(str);
					hasher.TransformBlock(buffer, 0, buffer.Length, null, 0);
				};

				foreach (FingerprintEntityTypeSettings query in settings.OrderBy(q => q.EntityTypeCode, StringComparer.InvariantCultureIgnoreCase))
				{
					IterateQuery(query, rowEnumerator);
				}

				hasher.TransformFinalBlock(new byte[0], 0, 0);
				return hasher.Hash;
			}
		}
		#endregion

		private void IterateQuery(FingerprintEntityTypeSettings query, Action<IEnumerable<object>> rowEnumerator)
		{
			switch (query.EntityTypeCode)
			{
				case C.EntityTypeCode.Site:
					fingerprintRepository.IterateSite(query, rowEnumerator);
					break;
				case C.EntityTypeCode.Content:
					fingerprintRepository.IterateContent(query, rowEnumerator);
					break;
				case C.EntityTypeCode.ContentLink:
					fingerprintRepository.IterateContentLink(query, rowEnumerator);
					break;
				case C.EntityTypeCode.Field:
					fingerprintRepository.IterateField(query, rowEnumerator);
					break;
				case C.EntityTypeCode.Article:
					fingerprintRepository.IterateArticle(query, rowEnumerator);
					break;
				case C.EntityTypeCode.Notification:
					fingerprintRepository.IterateNotification(query, rowEnumerator);
					break;
				case C.EntityTypeCode.Workflow:
					fingerprintRepository.IterateWorkflow(query, rowEnumerator);
					break;
				case C.EntityTypeCode.StatusType:
					fingerprintRepository.IterateStatusType(query, rowEnumerator);
					break;
				case C.EntityTypeCode.CustomAction:
					fingerprintRepository.IterateCustomAction(query, rowEnumerator);
					break;
				case C.EntityTypeCode.VisualEditorPlugin:
					fingerprintRepository.IterateVePlugin(query, rowEnumerator);
					break;
				case C.EntityTypeCode.VisualEditorStyle:
					fingerprintRepository.IterateVeStyle(query, rowEnumerator);
					break;
				case C.EntityTypeCode.User:
					fingerprintRepository.IterateUser(query, rowEnumerator);
					break;
				case C.EntityTypeCode.UserGroup:
					fingerprintRepository.IterateUserGroup(query, rowEnumerator);
					break;
				case C.EntityTypeCode.SiteFolder:
					fingerprintRepository.IterateSiteFolder(query, rowEnumerator);
					break;
				case C.EntityTypeCode.ContentFolder:
					fingerprintRepository.IterateContentFolder(query, rowEnumerator);
					break;
				case C.EntityTypeCode.SitePermission:
					fingerprintRepository.IterateSitePermission(query, rowEnumerator);
					break;
				case C.EntityTypeCode.ContentPermission:
					fingerprintRepository.IterateContentPermission(query, rowEnumerator);
					break;
				case C.EntityTypeCode.ArticlePermission:
					fingerprintRepository.IterateArticlePermission(query, rowEnumerator);
					break;
				case C.EntityTypeCode.WorkflowPermission:
					fingerprintRepository.IterateWorkflowPermission(query, rowEnumerator);
					break;
				case C.EntityTypeCode.SiteFolderPermission:
					fingerprintRepository.IterateSiteFolderPermission(query, rowEnumerator);
					break;
				case C.EntityTypeCode.EntityTypePermission:
					fingerprintRepository.IterateEntityTypePermission(query, rowEnumerator);
					break;
				case C.EntityTypeCode.ActionPermission:
					fingerprintRepository.IterateActionPermission(query, rowEnumerator);
					break;
				case C.EntityTypeCode.PageTemplate:
					fingerprintRepository.IterateTemplate(query, rowEnumerator);
					break;
				case C.EntityTypeCode.TemplateObject:
					fingerprintRepository.IterateTemplateObject(query, rowEnumerator);
					break;
				case C.EntityTypeCode.TemplateObjectFormat:
					fingerprintRepository.IterateTemplateObjectFormat(query, rowEnumerator);
					break;
				case C.EntityTypeCode.Page:
					fingerprintRepository.IteratePage(query, rowEnumerator);
					break;
				case C.EntityTypeCode.PageObject:
					fingerprintRepository.IteratePageObject(query, rowEnumerator);
					break;
				case C.EntityTypeCode.PageObjectFormat:
					fingerprintRepository.IteratePageObjectFormat(query, rowEnumerator);
					break;
				default:
					break;
			}
		}

		private IEnumerable<FingerprintEntityTypeSettings> ConvertToEntityTypeSettings(XDocument xsettings)
		{
			// XDocument to IEnumerable<FingerprintQuerySetting>
			IEnumerable<FingerprintEntityTypeSettings> result = xsettings.Descendants("entityType")
				.Where(xe => xe.Attribute("code") != null && !String.IsNullOrWhiteSpace(xe.Attribute("code").Value))
				.Distinct(new LambdaEqualityComparer<XElement>((e1, e2) => StringComparer.InvariantCultureIgnoreCase.Equals(e1.Attribute("code"), e2.Attribute("code").Value)))
				.Select(xe =>
				{
					FingerprintEntityTypeSettings qsetting = new FingerprintEntityTypeSettings
					{
						EntityTypeCode = xe.Attribute("code").Value,
					};

					qsetting.ConsiderCurentIdentity = 
						(
							xe.Attribute("considerIdentity") 
							?? new XAttribute(
								"considerIdentity",
								!qsetting.EntityTypeCode.Equals(C.EntityTypeCode.Article, StringComparison.CurrentCultureIgnoreCase)
							)
						)
						.Value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
					

					XElement included = xe.Element("included");
					XElement excepted = xe.Element("excepted");
					if (included != null)
					{
						qsetting.IncludedIDs = included.Descendants("id").Select(xid => Converter.ToInt32(xid.Value)).ToArray();
						qsetting.IncludedParentIDs = included.Descendants("parentId").Select(xid => Converter.ToInt32(xid.Value)).ToArray();
					}
					if (excepted != null)
					{
						qsetting.ExceptedIDs = excepted.Descendants("id").Select(xid => Converter.ToInt32(xid.Value)).ToArray();
						qsetting.ExceptedParentIDs = excepted.Descendants("parentId").Select(xid => Converter.ToInt32(xid.Value)).ToArray();
					}

					if (qsetting.IncludedIDs.Any() && qsetting.ExceptedIDs.Any())
						qsetting.ExceptedIDs = new int[0];
					if (qsetting.IncludedParentIDs.Any() && qsetting.ExceptedParentIDs.Any())
						qsetting.ExceptedParentIDs = new int[0];

					return qsetting;
				});

			result = DeriveAncestorRestrictions(result);
			return result;
		}


		private delegate FingerprintAncestorRestrictionTree RestrictionTreeDelegate(string entityTypeCode, ref bool isRestrictedParent);
		/// <summary>
		/// Получить Иерархические ограничения
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		private IEnumerable<FingerprintEntityTypeSettings> DeriveAncestorRestrictions(IEnumerable<FingerprintEntityTypeSettings> settings)
		{
			Debug.Assert(settings != null);

			Dictionary<string, EntityType> entityTypeDictionary = fingerprintRepository.GetEntityTypesCodeKeyDictionaty();
			Dictionary<string, FingerprintEntityTypeSettings> settingDictionary = settings.ToDictionary(s => s.EntityTypeCode);

			RestrictionTreeDelegate getRestrictionTree = null;
			getRestrictionTree = delegate (string entityTypeCode, ref bool isRestrictedParent)
			 {
				 Debug.Assert(!String.IsNullOrWhiteSpace(entityTypeCode));
				 if (!entityTypeDictionary.ContainsKey(entityTypeCode))
					 throw new ConfigurationErrorsException("Undefined Entity Type Code: " + entityTypeCode);

				 EntityType entityType = entityTypeDictionary[entityTypeCode];
				 // если нет родителя - останавливаем рекурсию
				 if (!entityType.ParentId.HasValue || String.Equals(entityType.ParentCode, EntityTypeCode.CustomerCode, StringComparison.InvariantCultureIgnoreCase))
					 return null;

				 EntityType parentEntityType = entityTypeDictionary[entityType.ParentCode];
				 FingerprintEntityTypeSettings parentSettings = settingDictionary.ContainsKey(parentEntityType.Code) ? settingDictionary[parentEntityType.Code] : null;

				 // если у родителя есть ограничения - останавливаем рекурсию
				 if (parentSettings != null && parentSettings.HasDirectRestriction)
				 {
					 isRestrictedParent = true;
					 return new FingerprintAncestorRestrictionTree
					 {
						 EntityType = parentEntityType,
						 Settings = parentSettings,
						 Parent = null
					 };
				 }
				 else // иначе далее рекурсивно
				 {
					 return new FingerprintAncestorRestrictionTree
					 {
						 EntityType = parentEntityType,
						 // рекурсия
						 Parent = getRestrictionTree(parentEntityType.Code, ref isRestrictedParent),
						 Settings = null
					 };
				 }
			 };

			foreach (var ets in settings)
			{
				if(!ets.HasDirectRestriction)
				{
					bool isRP = false;
					FingerprintAncestorRestrictionTree restrictionTree = getRestrictionTree(ets.EntityTypeCode, ref isRP);
					if(restrictionTree != null && isRP)
						ets.AncestorRestrictionTree = restrictionTree;
				}
				yield return ets;
			}
		}

	}
}
