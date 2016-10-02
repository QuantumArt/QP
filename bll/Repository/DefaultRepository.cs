using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using D = System.Data.Objects.DataClasses;

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
            var result = type.Name.Replace(EF.EntitySuffix, EF.SetSuffix);
            if (quailfied)
            {
                result = EF.ContainerName + "." + result;
            }

            return result;
        }

        internal static TBiz Save<TBiz, TDal>(TBiz item)
            where TDal : D.EntityObject, IQPEntityObject
            where TBiz : EntityObject
        {
            return SaveAsUser<TBiz, TDal>(item, QPContext.CurrentUserId);
        }

        internal static TBiz SaveAsAdmin<TBiz, TDal>(TBiz item)
            where TDal : D.EntityObject, IQPEntityObject
            where TBiz : EntityObject
        {
            return SaveAsUser<TBiz, TDal>(item, SpecialIds.AdminUserId);
        }

        internal static TBiz SaveAsUser<TBiz, TDal>(TBiz item, int userId)
            where TDal : D.EntityObject, IQPEntityObject
            where TBiz : EntityObject
        {
            var entities = QPContext.EFContext;
            var dalItem = DefaultMapper.GetDalObject<TDal, TBiz>(item);

            if (item.ForceId != 0)
            {
                dalItem.Id = item.ForceId;
            }

            DateTime current;
            using (new QPConnectionScope())
            {
                current = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }

            dalItem.Created = current;
            dalItem.Modified = current;
            dalItem.LastModifiedBy = userId;
            entities.AddObject(GetSetNameByType(typeof(TDal)), dalItem);
            entities.SaveChanges();

            return DefaultMapper.GetBizObject<TBiz, TDal>(dalItem);
        }

        internal static TBiz Update<TBiz, TDal>(TBiz item)
            where TDal : D.EntityObject, IQPEntityObject
            where TBiz : EntityObject
        {
            var dalItem = DefaultMapper.GetDalObject<TDal, TBiz>(item);
            var entities = QPContext.EFContext;
            dalItem.LastModifiedBy = QPContext.CurrentUserId;

            using (new QPConnectionScope())
            {
                dalItem.Modified = Common.GetSqlDate(QPConnectionScope.Current.DbConnection);
            }

            entities.AttachTo(GetSetNameByType(typeof(TDal)), dalItem);
            entities.ObjectStateManager.ChangeObjectState(dalItem, EntityState.Modified);
            entities.SaveChanges();
            return DefaultMapper.GetBizObject<TBiz, TDal>(dalItem);
        }

        internal static void Delete<TDal>(int id)
            where TDal : D.EntityObject
        {
            SimpleDelete(new EntityKey(GetSetNameByType(typeof(TDal), true), "Id", (decimal)id));
        }

        internal static void Delete<TDal>(int[] id)
        where TDal : D.EntityObject
        {
            var entities = QPContext.EFContext;
            var sql = $"SELECT VALUE entity FROM {GetSetNameByType(typeof(TDal), true)} AS entity WHERE entity.Id IN {{{String.Join(",", id)}}}";
            var list = entities.CreateQuery<TDal>(sql).ToList();
            foreach (var result in list)
            {
                entities.DeleteObject(result);
            }

            entities.SaveChanges();
        }

        internal static TDal GetById<TDal>(int id)
        where TDal : D.EntityObject
        {
            var entities = QPContext.EFContext;
            var key = new EntityKey(GetSetNameByType(typeof(TDal), true), "Id", (decimal)id);
            object result;
            if (entities.TryGetObjectByKey(key, out result))
            {
                return (TDal)result;
            }

            return null;
        }

        internal static TDal SimpleSave<TDal>(TDal dalItem)
            where TDal : D.EntityObject
        {
            var entities = QPContext.EFContext;
            entities.AddObject(GetSetNameByType(typeof(TDal)), dalItem);
            entities.SaveChanges();
            return dalItem;
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        internal static IEnumerable<TDal> SimpleSave<TDal>(IEnumerable<TDal> dalItemList)
            where TDal : D.EntityObject
        {
            var entities = QPContext.EFContext;
            var setName = GetSetNameByType(typeof(TDal));
            foreach (var dalItem in dalItemList)
            {
                entities.AddObject(setName, dalItem);
            }

            entities.SaveChanges();
            return dalItemList;
        }

        internal static TDal SimpleUpdate<TDal>(TDal dalItem)
        where TDal : D.EntityObject
        {
            var entities = QPContext.EFContext;
            entities.AttachTo(GetSetNameByType(typeof(TDal)), dalItem);
            entities.ObjectStateManager.ChangeObjectState(dalItem, EntityState.Modified);
            entities.SaveChanges();
            return dalItem;
        }

        internal static void SimpleDelete(EntityKey key)
        {
            var entities = QPContext.EFContext;
            object result;
            if (entities.TryGetObjectByKey(key, out result))
            {
                entities.DeleteObject(result);
            }

            entities.SaveChanges();
        }

        internal static void SimpleDelete<TDal>(IEnumerable<TDal> dalItems)
            where TDal : D.EntityObject
        {
            var entities = QPContext.EFContext;
            foreach (var dalItem in dalItems)
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
                {
                    Common.ChangeInsertIdentityState(QPConnectionScope.Current.DbConnection, table, state);
                }
            }
        }

        internal static void TurnIdentityInsertOn(string entityTypeCode, EntityObject objectForCheck)
        {
            if (objectForCheck != null && QPConnectionScope.Current.IdentityInsertOptions.Contains(entityTypeCode))
            {
                objectForCheck.VerifyIdentityInserting(entityTypeCode);
            }

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
