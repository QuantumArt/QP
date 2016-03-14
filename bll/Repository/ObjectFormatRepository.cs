using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Utils;
using System.Data;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ObjectFormatRepository
    {
        internal static IEnumerable<ListItem> GetObjectFormats(int parentId, int? contentId, int[] selectedFormatIds)
        {
            using (var scope = new QPConnectionScope())
            {
				IEnumerable<ObjectFormatDAL> formats;
				if (contentId.HasValue)
				{
					formats = QPContext.EFContext.ObjectFormatSet.Include("Object").Include("Object.Container").Where(n => n.Object.Container.ContentId == contentId.Value);
				}
				else
				{
					formats = QPContext.EFContext.ObjectFormatSet.Where(n => n.ObjectId == parentId);
				}
				return formats.Select(row => new ListItem { 
					Text = row.Name.ToString(), 
					Value = row.Id.ToString(), 
					Selected = selectedFormatIds.Contains((int)row.Id)
				}).ToArray();
            }
        }

		internal static bool IsSiteDotNeByObjectFormatId(int objectFormatId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.IsSiteDotNeByObjectFormatId(scope.DbConnection, objectFormatId);
			}
		}

		internal static int CreateDefaultFormat(int contentId, string backendUrl, string currentCustomerCode)
		{
			string objectName;
			using (var scope = new QPConnectionScope())
			{
                objectName = Common.GetNewTemplateObjectName(scope.DbConnection, contentId);
			}

			int siteId = ContentRepository.GetById(contentId).SiteId;

			int pageTemplateId = Converter.ToInt32(QPContext.EFContext.PageTemplateSet
				.SingleOrDefault(t => t.SiteId == siteId && t.IsSystem == true).Id);			

			bool isDotNet = SiteRepository.GetById(siteId).IsDotNet;

			var obj = ObjectRepository.SaveObjectProperties(new BllObject
			{
				PageTemplateId = pageTemplateId,
				Name = objectName,
				Description = "Automatically Generated Object",
				TypeId = 2,
				AllowStageEdit = true,
				Global = false,
				LastModifiedBy = QPContext.CurrentUserId,
				Container = new Container
				{				
					ContentId = contentId,
					OrderStatic = "c.modified desc",
					SelectTotal = "1",
					ScheduleDependence = true,
					RotateContent = 0,
					ApplySecurity = false,
					ShowArchived = false,
					CursorType = "adOpenForwardOnly",
					CursorLocation = "adUseClient",					
					LockType = "adLockReadOnly",					
				}
			});

			obj.NetName = string.Format("default_{0}", obj.Id);
			ObjectRepository.UpdateObjectProperties(obj);			

			string formatBody = "";
			string codeBehind = "";
			if (isDotNet)
			{
				using (var scope = new QPConnectionScope())
				{
					formatBody = Common.FormatBodyNet(contentId, scope.DbConnection);
				}
				codeBehind = Common.CodeBehind(currentCustomerCode, backendUrl);
			}

			else
				using (var scope = new QPConnectionScope())
				{
					formatBody = Common.FormatBodyVBScript(contentId, currentCustomerCode, backendUrl, scope.DbConnection);
				}
			int objectFormatId = FormatRepository.SaveObjectFormatProperties(new ObjectFormat 
				{
					ObjectId = obj.Id,
					Name = "default",
					LastModifiedBy = QPContext.CurrentUserId,
					FormatBody = formatBody,
					NetLanguageId = isDotNet ? (int?)1 : null,
					NetFormatName = isDotNet ? "default" : null,
					CodeBehind = isDotNet ? codeBehind : null,
				}
			).Id;

			ObjectRepository.UpdateDefaultFormatId(obj.Id, objectFormatId);

			using (var scope = new QPConnectionScope())
			{
				Common.CreateContainerStatusBind(scope.DbConnection, obj.Id, contentId);
			}
			return objectFormatId;
		}

		internal static NotificationObjectFormat ReadNotificationTemplateFormat(int id)
		{
			return MappersRepository.NotificationTemplateFormatMapper.GetBizObject(
				QPContext.EFContext.ObjectFormatSet
				.SingleOrDefault(g => g.Id == id));
		}

		internal static ObjectFormat ReadObjectFormat(int id, bool pageOrTemplate)
		{
			return 
				MappersRepository.ObjectFormatMapper.GetBizObject(
				QPContext.EFContext.ObjectFormatSet.Include("LastModifiedByUser")
				.SingleOrDefault(g => g.Id == id));				
		}

		internal static NotificationObjectFormat UpdateNotificationTemplateFormat(NotificationObjectFormat item)
		{			
			return DefaultRepository.Update<NotificationObjectFormat, ObjectFormatDAL>(item);
		}

		/// <summary>
		/// Возвращает список по ids
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<NotificationObjectFormat> GetList(IEnumerable<int> IDs)
		{
			IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
			return MappersRepository.NotificationTemplateFormatMapper
				.GetBizList(QPContext.EFContext.ObjectFormatSet
					.Where(f => decIDs.Contains(f.Id))
					.ToList()
				);
		}

		internal static IEnumerable<ObjectFormat> GetFormatsByObjectId(int objectId)
		{
			return MappersRepository.ObjectFormatMapper
				.GetBizList(QPContext.EFContext.ObjectFormatSet
					.Where(f => f.ObjectId == objectId)
					.ToList()
				);
		}
        internal static void CopySiteTemplateObjectFormats(string relationsBetweenObjects, ref string result)
        {
            using (var scope = new QPConnectionScope())
            {
                IEnumerable<DataRow> rows = Common.CopySiteTemplateObjectFormats(scope.DbConnection, relationsBetweenObjects);
                result = MultistepActionHelper.GetXmlFromDataRows(rows, "object_format");
            }
        }
	}
}
