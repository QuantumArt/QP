using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QP8.Infrastructure.Extensions;
using QP8.Plugins.Contract;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
using EntityState = Microsoft.EntityFrameworkCore.EntityState;

namespace Quantumart.QP8.BLL.Repository
{
    internal class QpPluginRepository
    {
        internal static IEnumerable<QpPluginListItem> List(ListCommand cmd, out int totalRecords)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetQpPluginsPage(scope.DbConnection, cmd.SortExpression, out totalRecords, cmd.StartRecord, cmd.PageSize);
                return MapperFacade.QpPluginListItemRowMapper.GetBizList(rows.ToList());
            }
        }

        internal static Dictionary<int, QpPlugin> GetQpFieldPluginDict()
        {
            var pluginDict = new Dictionary<int, QpPlugin>();
            var dataList = QPContext.EFContext.PluginSet.Include(n => n.Fields).ToList();
            var qpPlugins = MapperFacade.QpPluginMapper.GetBizList(dataList);
            foreach (var plugin in qpPlugins)
            {
                pluginDict.AddRange(plugin.Fields.ToDictionary(n => n.Id, m => plugin));
            }
            return pluginDict;
        }



        internal static QpPlugin GetById(int id)
        {
            return MapperFacade.QpPluginMapper.GetBizObject(QPContext.EFContext.PluginSet
                .Include(n => n.Fields)
                .Include(n => n.LastModifiedByUser)
                .SingleOrDefault(g => g.Id == id)
            );
        }

        internal static QpPlugin UpdateProperties(QpPlugin plugin)
        {
            var entities = QPContext.EFContext;
            DateTime timeStamp;
            using (var scope = new QPConnectionScope())
            {
                timeStamp = Common.GetSqlDate(scope.DbConnection);

                var dal = MapperFacade.QpPluginMapper.GetDalObject(plugin);
                dal.LastModifiedBy = QPContext.CurrentUserId;
                dal.Modified = timeStamp;
                entities.Entry(dal).State = EntityState.Modified;
                DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.QpPlugin, plugin);
                entities.SaveChanges();
                DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.QpPlugin);

                var forceIds = plugin.ForceFieldIds == null ? null : new Queue<int>(plugin.ForceFieldIds);
                var fields = new List<PluginFieldDAL>();
                foreach (var field in plugin.Fields.Where(n => n.Id == 0))
                {
                    var dalField = MapperFacade.QpPluginFieldMapper.GetDalObject(field);
                    dalField.PluginId = dal.Id;
                    if (forceIds != null)
                    {
                        dalField.Id = forceIds.Dequeue();
                    }

                    entities.Entry(dalField).State = EntityState.Added;
                    fields.Add(dalField);
                }

                DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.QpPluginField);
                entities.SaveChanges();
                DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.QpPluginField);

                foreach (var field in fields)
                {
                    Common.AddPluginColumn(scope.DbConnection, field);
                }

                return GetById(plugin.Id);
            }
        }

        internal static void Delete(int id)
        {
            using (var scope = new QPConnectionScope())
            {
                ChangeDeleteValuesTriggerState(scope.DbConnection, false);
                DefaultRepository.Delete<PluginDAL>(id);
                Common.DropPluginTables(scope.DbConnection, id);
                ChangeDeleteValuesTriggerState(scope.DbConnection, true);
            }
        }

        internal static void ChangeDeleteValuesTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "td_plugin_field_value", enable);
        }

        internal static bool CodeExists(QpPlugin plugin)
        {
            return QPContext.EFContext.PluginSet.Any(
                n => n.Code == plugin.Code && n.Id != plugin.Id
                && (!plugin.AllowMultipleInstances || !n.AllowMultipleInstances || n.InstanceKey == plugin.InstanceKey)
            );
        }

        internal static QpPlugin SaveProperties(QpPlugin plugin)
        {
            var entities = QPContext.EFContext;
            DateTime timeStamp;
            using (var scope = new QPConnectionScope())
            {
                timeStamp = Common.GetSqlDate(scope.DbConnection);

                var dal = MapperFacade.QpPluginMapper.GetDalObject(plugin);
                dal.LastModifiedBy = QPContext.CurrentUserId;
                dal.Modified = timeStamp;
                dal.Created = timeStamp;

                entities.Entry(dal).State = EntityState.Added;

                DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.QpPlugin, plugin);
                if (plugin.ForceId != 0)
                {
                    dal.Id = plugin.ForceId;
                }

                entities.SaveChanges();
                DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.QpPlugin);

                Common.CreatePluginTables(scope.DbConnection, (int)dal.Id);

                var forceIds = plugin.ForceFieldIds == null ? null : new Queue<int>(plugin.ForceFieldIds);
                var fields = new List<PluginFieldDAL>();
                foreach (var field in plugin.Fields)
                {
                    var dalField = MapperFacade.QpPluginFieldMapper.GetDalObject(field);
                    dalField.PluginId = dal.Id;
                    if (forceIds != null)
                    {
                        dalField.Id = forceIds.Dequeue();
                    }

                    entities.Entry(dalField).State = EntityState.Added;
                    fields.Add(dalField);
                }

                DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.QpPluginField);
                entities.SaveChanges();
                DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.QpPluginField);

                foreach (var field in fields)
                {
                    Common.AddPluginColumn(scope.DbConnection, field);
                }

                return MapperFacade.QpPluginMapper.GetBizObject(dal);
            }
        }

        internal static int GetPluginMaxOrder()
        {
            var plugins = QPContext.EFContext.PluginSet;
            return plugins.Any() ? plugins.Max(n => n.Order) : 0;
        }

        public static void CreateVersion(QpPlugin plugin)
        {
            var entities = QPContext.EFContext;
            var version = new QpPluginVersion
            {
                Contract = plugin.OldContract,
                Modified = plugin.OldModified,
                LastModifiedBy = plugin.OldLastModifiedBy,
                Created = DateTime.Now,
                Plugin = plugin,
                PluginId = plugin.Id
            };
            var dal = MapperFacade.QpPluginVersionMapper.GetDalObject(version);
            entities.Entry(dal).State = EntityState.Added;
            entities.SaveChanges();
        }

        internal static IEnumerable<QpPluginField> GetPluginFields(QpPluginRelationType relationType)
        {
            var relationTypeDal = MapperFacade.QpPluginFieldMapper.Write(relationType);
            var pluginFields = QPContext.EFContext.PluginFieldSet.Where(n => n.RelationType == relationTypeDal).ToList();
            return MapperFacade.QpPluginFieldMapper.GetBizList(pluginFields).ToList();
        }
    }
}
