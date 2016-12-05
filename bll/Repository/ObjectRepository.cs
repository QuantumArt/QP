using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository
{
	class ObjectRepository
	{
		internal static IEnumerable<ObjectListItem> ListTemplateObjects(ListCommand cmd, int templateId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetTemplateObjectsByTemplateId(scope.DbConnection, templateId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
				return MapperFacade.ObjectRowMapper.GetBizList(rows.ToList());
			}
		}

		internal static IEnumerable<ObjectListItem> ListPageObjects(ListCommand cmd, int parentId, out int totalRecords)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.GetPageObjectsByPageId(scope.DbConnection, parentId, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
				return MapperFacade.ObjectRowMapper.GetBizList(rows.ToList());
			}
		}

		internal static BllObject SaveObjectProperties(BllObject bllObject)
		{			
				BllObject result;
				result = DefaultRepository.Save<BllObject, ObjectDAL>(bllObject);
				if (bllObject.IsObjectContainerType)
				{
					bllObject.Container.ObjectId = result.Id;
					result.Container = SaveContainer(bllObject.Container);
				}

				else if (bllObject.IsObjectFormType)
				{
					bllObject.ContentForm.ObjectId = result.Id;
					result.ContentForm = SaveContentForm(bllObject.ContentForm);
				}
				if (bllObject.DefaultValues != null && bllObject.DefaultValues.Count() > 0)
				{
					SetDefaultValues(bllObject.DefaultValues, result.Id);
				}
				return result;
		}

		internal static void SetDefaultValues(IEnumerable<DefaultValue> dvals, int objectId)
		{
			QP8Entities entities = QPContext.EFContext;
			foreach (var obj in dvals)
			{
				ObjectValuesDAL dal = ObjectValuesDAL.CreateObjectValuesDAL(objectId, obj.VariableName, obj.VariableValue);
				entities.ObjectValuesSet.AddObject(dal);
			}
			entities.SaveChanges();
		}

		internal static void DeleteDefaultValues(int objectId)
		{
			QP8Entities entities = QPContext.EFContext;
			foreach (var dal in entities.ObjectValuesSet.Where(x => x.ObjectId == objectId))
				entities.ObjectValuesSet.DeleteObject(dal);
			entities.SaveChanges();
		}

		internal static IEnumerable<ObjectValue> GetDefaultValuesByObjectId(int objectId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MapperFacade.ObjectValueMapper.GetBizList(entities.ObjectValuesSet.Where(x => x.ObjectId == objectId).ToList());
		}

		internal static BllObject UpdateObjectProperties(BllObject bllObject)
		{
			var oldObject = GetObjectPropertiesById(bllObject.Id);
			var result = DefaultRepository.Update<BllObject, ObjectDAL>(bllObject);
			ManageObjectType(bllObject, oldObject, result);

			QP8Entities entities = QPContext.EFContext;
			DeleteDefaultValues(result.Id);
			SetDefaultValues(bllObject.DefaultValues, result.Id);			
			return result;
		}

		internal static void UpdateDefaultFormatId(int objectId, int formatId)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.UpdateDefaultFormatId(scope.DbConnection, objectId, formatId);
			}
		}

		private static void ManageObjectType(BllObject newObject, BllObject oldObject, BllObject result)
		{
			if (newObject.TypeId != oldObject.TypeId)//object type was changed
			{
				if (oldObject.IsObjectContainerType)
					DeleteContainer(oldObject.Id);
				else if (oldObject.IsObjectFormType)
					DeleteForm(oldObject.Id);

				if (newObject.IsObjectContainerType)
				{
					newObject.Container.ObjectId = result.Id;
					result.Container = SaveContainer(newObject.Container);
					result.Container.Content = ContentRepository.GetById(result.Container.ContentId.Value);
				}

				else if (newObject.IsObjectFormType)
				{
					newObject.ContentForm.ObjectId = result.Id;
					result.ContentForm = SaveContentForm(newObject.ContentForm);
					result.ContentForm.Content = ContentRepository.GetById(result.ContentForm.ContentId.Value);
				}
			}

			else // just update form or container
			{
				if (newObject.IsObjectFormType)
					result.ContentForm = (oldObject.ContentForm.ContentId == null) ? SaveContentForm(newObject.ContentForm) : UpdateForm(newObject.ContentForm);
				if (newObject.IsObjectContainerType)
					result.Container = (oldObject.Container.ContentId == null) ? SaveContainer(newObject.Container) : UpdateContainer(newObject.Container);
			}
		}

		internal static BllObject GetObjectPropertiesById(int id)
		{
			var result = MapperFacade.ObjectMapper.GetBizObject(QPContext.EFContext.ObjectSet.Include("ChildObjectFormats.Notifications").Include("InheritedObjects")
				.Include("PageTemplate.Site").Include("ObjectType").Include("LastModifiedByUser")
				.SingleOrDefault(g => g.Id == id));
			result.DefaultValues = GetDefaultValuesByObjectId(id).Select(x => new DefaultValue { VariableName = x.VariableName, VariableValue = x.VariableValue });
			return result;
		}

		internal static void DeleteObject(int id)
		{
			DefaultRepository.Delete<ObjectDAL>(id);
		}

		internal static void SetPageObjectEnableViewState(int pageId, bool enableViewState)
		{
			QP8Entities entities = QPContext.EFContext;

			var objects = entities.ObjectSet.Where(x => x.PageId == pageId);
			foreach (var obj in objects)
			{
				obj.EnableViewstate = enableViewState;
				/*using (new QPConnectionScope())
				{
					obj.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
				}*/
			}
			entities.SaveChanges();
		}

		internal static bool ObjectNetNameUnique(string netName, int? pageId, int pageTemplateId, bool PageOrTemplate, int id)
		{
			QP8Entities entities = QPContext.EFContext;

			return PageOrTemplate ?
				MapperFacade.ObjectMapper.GetBizList(entities.ObjectSet.Where(x => x.PageId == pageId && x.NetName == netName && x.Id != id).ToList()).Count() == 0 :
				MapperFacade.ObjectMapper.GetBizList(entities.ObjectSet.Where(x => x.PageId == null && x.PageTemplateId == pageTemplateId && x.NetName == netName && x.Id != id).ToList()).Count == 0;
		}

		internal static Container SaveContainer(Container container)
		{
			QP8Entities entities = QPContext.EFContext;
			container.CursorType = "adOpenForwardOnly";
			container.CursorLocation = "adUseClient";
			ContainerDAL dal = MapperFacade.ContainerMapper.GetDalObject(container);
			dal.CursorType = "adOpenForwardOnly";
			dal.CursorLocation = "adUseClient";
			dal.LockType = "adLockReadOnly";
			dal.Locked = null;
			entities.ContainerSet.AddObject(dal);
			entities.SaveChanges();
			return MapperFacade.ContainerMapper.GetBizObject(dal);
		}


		internal static Container UpdateContainer(Container container)
		{
			var dal = MapperFacade.ContainerMapper.GetDalObject(container);
			dal = DefaultRepository.SimpleUpdate(dal);
			return MapperFacade.ContainerMapper.GetBizObject(dal);
		}

		internal static ContentForm UpdateForm(ContentForm form)
		{
			var dal = MapperFacade.ContentFormMapper.GetDalObject(form);
			dal = DefaultRepository.SimpleUpdate(dal);
			return MapperFacade.ContentFormMapper.GetBizObject(dal);
		}

		internal static ContentForm SaveContentForm(ContentForm contentForm)
		{
			QP8Entities entities = QPContext.EFContext;
			ContentFormDAL dal = MapperFacade.ContentFormMapper.GetDalObject(contentForm);
			entities.ContentFormSet.AddObject(dal);
			entities.SaveChanges();
			return MapperFacade.ContentFormMapper.GetBizObject(dal);
		}

		internal static void DeleteContainer(int objectId)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.DeleteObjectContainer(scope.DbConnection, objectId);
			}
		}

		internal static void DeleteForm(int objectId)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.DeleteObjectForm(scope.DbConnection, objectId);
			}
		}

		internal static void SetObjectActiveStatuses(int objectId, IEnumerable<int> activeStatuses, bool isContainer)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.CleanObjectActiveStatuses(scope.DbConnection, objectId);
				if (isContainer)
					Common.SetObjectActiveStatuses(scope.DbConnection, objectId, activeStatuses);
			}
		}

		internal static IEnumerable<int> GetObjectActiveStatusIds(int objectId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.GetObjectActiveStatusesIds(scope.DbConnection, objectId);
			}
		}

		internal static IEnumerable<BllObject> GetTemplateObjects(int templateId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MapperFacade.ObjectMapper.GetBizList(entities.ObjectSet.Include("ChildObjectFormats").Include("PageTemplate").Where(x => x.PageTemplateId == templateId && x.PageId == null).ToList());
		}

		internal static IEnumerable<BllObject> GetPageObjects(int pageId)
		{
			QP8Entities entities = QPContext.EFContext;
			return MapperFacade.ObjectMapper.GetBizList(entities.ObjectSet.Include("ChildObjectFormats").Include("PageTemplate").Where(x => x.PageId == pageId).ToList());
		}

		internal static IEnumerable<TemplateObjectFormatDto> GetRestTemplateObjects(int templateId)
		{
			QP8Entities entities = QPContext.EFContext;
			var siteId = PageTemplateRepository.GetPageTemplatePropertiesById(templateId).SiteId;
			using (var scope = new QPConnectionScope())
			{
				return MapperFacade.TemplateObjectFormatDtoRowMapper.GetBizList(Common.GetRestTemplateObjects(scope.DbConnection, templateId, siteId).ToList());
			}
		}

		internal static IEnumerable<EntityObject> GetList(IEnumerable<int> IDs)
		{
			IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(IDs).Distinct().ToArray();
			return MapperFacade.ObjectMapper
				.GetBizList(QPContext.EFContext.ObjectSet
					.Where(f => decIDs.Contains(f.Id))
					.ToList()
				);					
		}

		internal static int GetTemplatesElementsCountOnSite(int siteId)
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.GetTemplatesElementsCountOnSite(scope.DbConnection, siteId);
			}
		}
		internal static void CopySiteTemplateObjects(string relationsBetweenTemplates, string relationsBetweenPages, ref string result)
		{
			using (var scope = new QPConnectionScope())
			{
				IEnumerable<DataRow> rows = Common.CopySiteTemplateObjects(scope.DbConnection, relationsBetweenTemplates, relationsBetweenPages);
				result = MultistepActionHelper.GetXmlFromDataRows(rows, "object");
			}
		}
		internal static void CopySiteUpdateObjects(string relationsBetweenObjectFormats, string relationsBetweenObjects)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.CopySiteUpdateObjects(scope.DbConnection, relationsBetweenObjectFormats, relationsBetweenObjects);
			}
		}
		internal static void CopySiteObjectValues(string relationsBetweenObjects)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.CopySiteObjectValues(scope.DbConnection, relationsBetweenObjects);
			}
		}
		internal static void CopySiteContainers(string relationsBetweenObjects, string relationsBetweenContents)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.CopySiteContainers(scope.DbConnection, relationsBetweenObjects, relationsBetweenContents);
			}
		}

		internal static void CopyContainerStatuses(string relBetweenStatuses, string relBetweenObjects)
		{
			using (var scope = new QPConnectionScope())
			{
				Common.CopyContainerStatuses(scope.DbConnection, relBetweenStatuses, relBetweenObjects);
			}
		}
	}
}
