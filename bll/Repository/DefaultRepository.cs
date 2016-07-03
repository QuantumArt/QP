using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using D = System.Data.Objects.DataClasses;
using B = Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Repository
{
    public interface ISavable
    {
        decimal Id { get; set; }
        DateTime Created { get; set; }
        DateTime Modified { get; set; }
    }
    
    internal class DefaultRepository
    {        
        internal static string GetSetNameByType(Type type, bool quailfied = false)
        {
            string result = type.Name.Replace(EF.EntitySuffix, EF.SetSuffix);
            if (quailfied)
                result = EF.ContainerName + "." + result;
            return result;

        }

        internal static Biz Save<Biz, Dal>(Biz item)
			where Dal : D.EntityObject, IQPEntityObject
            where Biz : B.EntityObject
        {
            return SaveAsUser<Biz, Dal>(item, QPContext.CurrentUserId);
        }

        internal static Biz SaveAsAdmin<Biz, Dal>(Biz item)
			where Dal : D.EntityObject, IQPEntityObject
			where Biz : B.EntityObject
        {
            return SaveAsUser<Biz, Dal>(item, SpecialIds.AdminUserId);
        }



        internal static Biz SaveAsUser<Biz, Dal>(Biz item, int userId)
			where Dal : D.EntityObject, IQPEntityObject
			where Biz : B.EntityObject
        {
            QP8Entities entities = QPContext.EFContext;

            Dal dalItem = DefaultMapper.GetDalObject<Dal, Biz>(item);

			if (item.ForceId != 0)
				dalItem.Id = item.ForceId;

            DateTime current;
            using(new QPConnectionScope())
            {
                current = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }
            ((IQPEntityObject)dalItem).Created = current;
            ((IQPEntityObject)dalItem).Modified = current;
            ((IQPEntityObject)dalItem).LastModifiedBy = userId;
            entities.AddObject(GetSetNameByType(typeof(Dal)), dalItem);
            entities.SaveChanges();
            Biz addedItem = DefaultMapper.GetBizObject<Biz, Dal>(dalItem);
            return addedItem;
        }

        internal static Biz Update<Biz, Dal>(Biz item)
			where Dal : D.EntityObject, IQPEntityObject
			where Biz : B.EntityObject
        {

            Dal dalItem = DefaultMapper.GetDalObject<Dal, Biz>(item);
            QP8Entities entities = QPContext.EFContext;
            ((IQPEntityObject)dalItem).LastModifiedBy = QPContext.CurrentUserId;
            using (new QPConnectionScope())
            {
                ((IQPEntityObject)dalItem).Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }
            entities.AttachTo(GetSetNameByType(typeof(Dal)), dalItem);
            entities.ObjectStateManager.ChangeObjectState(dalItem, EntityState.Modified);
            entities.SaveChanges();
            Biz updatedItem = DefaultMapper.GetBizObject<Biz, Dal>(dalItem);
            return updatedItem;
        }

        internal static void Delete<Dal>(int id)
			where Dal : D.EntityObject
        {
            SimpleDelete<Dal>(new EntityKey(GetSetNameByType(typeof(Dal), true), "Id", (decimal)id));
        }

        internal static void Delete<Dal>(int[] id)
		where Dal : D.EntityObject
        {
            QP8Entities entities = QPContext.EFContext;
            string SQL = String.Format("SELECT VALUE entity FROM {0} AS entity WHERE entity.Id IN {{{1}}}", GetSetNameByType(typeof(Dal), true), String.Join(",", id));
            List<Dal> list = entities.CreateQuery<Dal>(SQL).ToList();
            foreach(Dal result in list)
            {
                entities.DeleteObject(result);
            }
            entities.SaveChanges();
        }

        internal static Dal GetById<Dal>(int id)
		where Dal : D.EntityObject
        {
            QP8Entities entities = QPContext.EFContext;
            EntityKey key = new EntityKey(GetSetNameByType(typeof(Dal), true), "Id", (decimal)id);
            object result;
            if (entities.TryGetObjectByKey(key, out result))
            {
                return (Dal)result;
            }
            else
                return null;
        }

        internal static Dal SimpleSave<Dal>(Dal dalItem)
			where Dal : D.EntityObject
        {
            QP8Entities entities = QPContext.EFContext;
            entities.AddObject(GetSetNameByType(typeof(Dal)), dalItem);
            entities.SaveChanges();
            return dalItem;
        }

		internal static IEnumerable<Dal> SimpleSave<Dal>(IEnumerable<Dal> dalItemList)
			where Dal : D.EntityObject
		{
			QP8Entities entities = QPContext.EFContext;
			var setName = GetSetNameByType(typeof(Dal));
			foreach (var dalItem in dalItemList)
			{
				entities.AddObject(setName, dalItem);
			}
			entities.SaveChanges();
			return dalItemList;
		}

        internal static Dal SimpleUpdate<Dal>(Dal dalItem)
		where Dal : D.EntityObject
        {
            QP8Entities entities = QPContext.EFContext;
            entities.AttachTo(GetSetNameByType(typeof(Dal)), dalItem);
            entities.ObjectStateManager.ChangeObjectState(dalItem, EntityState.Modified);
            entities.SaveChanges();
            return dalItem;
        }

        internal static void SimpleDelete<Dal>(EntityKey key)
		where Dal : D.EntityObject
        {
            QP8Entities entities = QPContext.EFContext;
			object result;
			if (entities.TryGetObjectByKey(key, out result))
			{
				entities.DeleteObject(result);
			}
			entities.SaveChanges();
        }

        internal static void SimpleDelete<Dal>(IEnumerable<Dal> dalItems)
		where Dal : D.EntityObject
        {
            QP8Entities entities = QPContext.EFContext;
            foreach (Dal dalItem in dalItems)
            {
				object result;
				if (entities.TryGetObjectByKey(dalItem.EntityKey, out result))
				{
					entities.DeleteObject(result);
				}
            }
            entities.SaveChanges();
        }

		public static void ChangeIdentityInsertState(string entityTypeCode, bool state)
		{
			
			if (QPConnectionScope.Current.IdentityInsertOptions.Contains(entityTypeCode))
			{
				string table = null;
				switch (entityTypeCode)
				{
					case EntityTypeCode.ContentLink:
						table = "dbo.CONTENT_TO_CONTENT";
						break;
					case EntityTypeCode.Content:
						table = "dbo.CONTENT";
						break;
                    case EntityTypeCode.ContentGroup:
                        table = "dbo.CONTENT_GROUP";
                        break;
                    case EntityTypeCode.Field:
						table = "dbo.CONTENT_ATTRIBUTE";
						break;
					case EntityTypeCode.VisualEditorCommand:
						table = "dbo.VE_COMMAND";
						break;
					case EntityTypeCode.VisualEditorPlugin:
						table = "dbo.VE_PLUGIN";
						break;
					case EntityTypeCode.VisualEditorStyle:
						table = "dbo.VE_STYLE";
						break;
					case EntityTypeCode.CustomAction:
						table = "dbo.CUSTOM_ACTION";
						break;
					case EntityTypeCode.BackendAction:
						table = "dbo.BACKEND_ACTION";
						break;
					case EntityTypeCode.Site:
						table = "dbo.SITE";
						break;
					case EntityTypeCode.StatusType:
						table = "dbo.STATUS_TYPE";
						break;
					case EntityTypeCode.Workflow:
						table = "dbo.WORKFLOW";
						break;
					case EntityTypeCode.Notification:
						table = "dbo.NOTIFICATIONS";
						break;

				}
			
				if (table != null)
					Common.ChangeInsertIdentityState(QPConnectionScope.Current.DbConnection, table, state);
			}

		}


		internal static void TurnIdentityInsertOn(string entityTypeCode, EntityObject objectForCheck)
		{
			if (objectForCheck != null && QPConnectionScope.Current.IdentityInsertOptions.Contains(entityTypeCode))
				objectForCheck.VerifyIdentityInserting(entityTypeCode);
			ChangeIdentityInsertState(entityTypeCode, true);
		}

		internal static void TurnIdentityInsertOn(string entityTypeCode)
		{
			TurnIdentityInsertOn(entityTypeCode, null);
		}

		internal static void TurnIdentityInsertOff(string entityTypeCode)
		{
			ChangeIdentityInsertState(entityTypeCode, false);
		}
	}
}
