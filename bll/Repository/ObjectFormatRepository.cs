using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository
{
    internal class ObjectFormatRepository
    {
        internal static IEnumerable<ListItem> GetObjectFormats(int parentId, int? contentId, int[] selectedFormatIds)
        {
            using (new QPConnectionScope())
            {
                IEnumerable<ObjectFormatDAL> formats = contentId.HasValue
                    ? QPContext.EFContext.ObjectFormatSet.Include("Object").Include("Object.Container").Where(n => n.Object.Container.ContentId == contentId.Value)
                    : QPContext.EFContext.ObjectFormatSet.Where(n => n.ObjectId == parentId);

                return formats.Select(row => new ListItem
                {
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

            var siteId = ContentRepository.GetById(contentId).SiteId;
            var pageTemplateId = Converter.ToInt32(QPContext.EFContext.PageTemplateSet.SingleOrDefault(t => t.SiteId == siteId && t.IsSystem).Id);
            var isDotNet = SiteRepository.GetById(siteId).IsDotNet;
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
                    LockType = "adLockReadOnly"
                }
            });

            obj.NetName = $"default_{obj.Id}";
            ObjectRepository.UpdateObjectProperties(obj);

            string formatBody;
            var codeBehind = string.Empty;
            if (isDotNet)
            {
                using (var scope = new QPConnectionScope())
                {
                    formatBody = Common.FormatBodyNet(contentId, scope.DbConnection);
                }

                codeBehind = Common.CodeBehind(currentCustomerCode, backendUrl);
            }
            else
            {
                using (var scope = new QPConnectionScope())
                {
                    formatBody = Common.FormatBodyVbScript(contentId, currentCustomerCode, backendUrl, scope.DbConnection);
                }
            }

            var objectFormatId = FormatRepository.SaveObjectFormatProperties(new ObjectFormat
            {
                ObjectId = obj.Id,
                Name = "default",
                LastModifiedBy = QPContext.CurrentUserId,
                FormatBody = formatBody,
                NetLanguageId = isDotNet ? (int?)1 : null,
                NetFormatName = isDotNet ? "default" : null,
                CodeBehind = isDotNet ? codeBehind : null
            }).Id;

            ObjectRepository.UpdateDefaultFormatId(obj.Id, objectFormatId);
            using (var scope = new QPConnectionScope())
            {
                Common.CreateContainerStatusBind(scope.DbConnection, obj.Id, contentId);
            }

            return objectFormatId;
        }

        internal static NotificationObjectFormat ReadNotificationTemplateFormat(int id)
        {
            return MapperFacade.NotificationTemplateFormatMapper.GetBizObject(QPContext.EFContext.ObjectFormatSet.SingleOrDefault(g => g.Id == id));
        }

        internal static ObjectFormat ReadObjectFormat(int id, bool pageOrTemplate)
        {
            return MapperFacade.ObjectFormatMapper.GetBizObject(QPContext.EFContext.ObjectFormatSet.Include("LastModifiedByUser").SingleOrDefault(g => g.Id == id));
        }

        internal static NotificationObjectFormat UpdateNotificationTemplateFormat(NotificationObjectFormat item) => DefaultRepository.Update<NotificationObjectFormat, ObjectFormatDAL>(item);

        internal static IEnumerable<ObjectFormat> GetFormatsByObjectId(int objectId)
        {
            return MapperFacade.ObjectFormatMapper.GetBizList(QPContext.EFContext.ObjectFormatSet.Where(f => f.ObjectId == objectId).ToList());
        }

        internal static void CopySiteTemplateObjectFormats(string relationsBetweenObjects, out string result)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.CopySiteTemplateObjectFormats(scope.DbConnection, relationsBetweenObjects);
                result = MultistepActionHelper.GetXmlFromDataRows(rows, "object_format");
            }
        }
    }
}
