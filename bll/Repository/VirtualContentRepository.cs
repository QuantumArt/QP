using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Transactions;
using Npgsql;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Utils;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Quantumart.QP8.BLL.Repository
{
    internal class VirtualContentRepository
    {
        /// <summary>
        /// Добавляет новый виртуальный контент
        /// </summary>
        internal static Content Save(Content content)
        {
            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Content, content);
            var newContent = DefaultRepository.Save<Content, ContentDAL>(content);

            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Content);
            return newContent;
        }

        /// <summary>
        /// Обновляет информацию о контенте
        /// </summary>
        internal static Content Update(Content content) => DefaultRepository.Update<Content, ContentDAL>(content);

        /// <summary>
        /// Возвращает данные о виртуальных полях JOIN-контента
        /// </summary>
        internal static IEnumerable<VirtualFieldData> GetJoinFieldData(int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                var dt = Common.GetVirtualFieldData(scope.DbConnection, contentId).AsEnumerable().ToList();
                return MapperFacade.VirtualFieldDataMapper.GetBizList(dt);
            }
        }

        /// <summary>
        /// Возвращает список join-контентов которые содержат виртуальные поля построенные на основе полей родительского контента
        /// </summary>
        internal static IEnumerable<Content> GetJoinRelatedContents(Content parentContent)
        {
            var baseFieldIds = Converter.ToDecimalCollection(parentContent.Fields.Select(f => f.Id)).Distinct().ToArray();
            var joinRelatedContentIDs = Converter.ToInt32Collection(QPContext.EFContext.FieldSet
                .Where(f => f.PersistentId != null && baseFieldIds.Contains(f.PersistentId.Value))
                .Select(f => f.ContentId)
                .Distinct()
                .ToArray()
            );

            return ContentRepository.GetList(joinRelatedContentIDs).Where(c => c.VirtualType == VirtualType.Join).ToArray();
        }

        internal static void RunCreateViewDdl(string viewCreateDdl)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ExecuteSql(scope.DbConnection, viewCreateDdl);
            }
        }

        internal static void CreateUnitedView(int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CreateUnitedView(scope.DbConnection, contentId);
            }
        }

        internal static void CreateFrontendViews(int contentId, bool useNative)
        {
            using (var scope = new QPConnectionScope())
            {
                var dbType = DatabaseTypeHelper.ResolveDatabaseType(scope.DbConnection);
                if (dbType == DatabaseType.Postgres)
                {
                    Common.CreateContentViews(scope.DbConnection, contentId, false, useNative);
                }
                else
                {
                    Common.CreateFrontendViews(scope.DbConnection, contentId);
                }
            }
        }

        internal static void DropView(string viewName)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.DropView(scope.DbConnection, viewName);
            }
        }

        internal static void RefreshView(string viewName)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RefreshView(scope.DbConnection, viewName);
            }
        }

        /// <summary>
        /// Возвращает список контентов на основе которых можно строить виртуальные контетны типа join
        /// </summary>
        internal static IEnumerable<ListItem> GetAcceptableContentForVirtualJoin(int siteId)
        {
            return QPContext.EFContext.ContentSet.Where(c => c.VirtualType == VirtualType.None && (c.SiteId == siteId || c.SiteId != siteId && c.IsShared))
                .Select(c => new { c.Id, Text = c.Name })
                .ToArray()
                .OrderBy(c => c.Text, StringComparer.InvariantCultureIgnoreCase)
                .Select(c => new ListItem { Value = c.Id.ToString(CultureInfo.InvariantCulture), Text = c.Text })
                .ToArray();
        }

        /// <summary>
        /// Возвращает поля всех контентов id которых указаны
        /// </summary>
        internal static IEnumerable<Field> GetFieldsOfContents(IEnumerable<int> contentIds)
        {
            var dContentIDs = Converter.ToDecimalCollection(contentIds);
            return MapperFacade.FieldMapper.GetBizList(FieldRepository.DefaultFieldQuery
                .Where(f => dContentIDs.Contains(f.ContentId))
                .OrderBy(f => f.ContentId)
                .ToList()
            );
        }

        /// <summary>
        /// Возвращает контенты-источники для UNION
        /// </summary>
        internal static IEnumerable<int> GetUnionSourceContents(int contentId)
        {
            return Converter.ToInt32Collection(QPContext.EFContext.UnionContentsSet
                .Where(r => r.VirtualContentId == contentId)
                .Select(r => r.UnionContentId)
                .Distinct()
                .ToArray());
        }

        internal static void ChangeUnionContentTriggerState(bool enable)
        {
            if (QPContext.DatabaseType != DatabaseType.SqlServer)
            {
                return;
            }
            using (var scope = new QPConnectionScope())
            {
                Common.ChangeTriggerState(scope.DbConnection, "ti_union_contents_auto_map_attrs", enable);
            }
        }

        /// <summary>
        /// Сохранить записи в таблице union_contents
        /// </summary>
        internal static void RecreateUnionSourcesInfo(Content virtualContent, IEnumerable<int> unionSourceContentIDs)
        {
            try
            {
                ChangeUnionContentTriggerState(false);
                var virtualContentId = virtualContent.Id;
                var context = QPContext.EFContext;
                var recToRemove = context.UnionContentsSet.Where(u => u.VirtualContentId == virtualContentId).ToArray();

                DefaultRepository.SimpleDeleteBulk(recToRemove, context);
                DefaultRepository.SimpleSaveBulk(unionSourceContentIDs.Select(id => new UnionContentsDAL { VirtualContentId = virtualContentId, UnionContentId = id }));
            }
            finally
            {
                ChangeUnionContentTriggerState(true);
            }
        }

        internal static void RemoveUnionSourcesInfo(Content virtualContent)
        {
            var virtualContentId = virtualContent.Id;
            var context = QPContext.EFContext;
            var recToRemove = context.UnionContentsSet.Where(u => u.VirtualContentId == virtualContentId).ToArray();
            DefaultRepository.SimpleDeleteBulk(recToRemove, context);
        }

        /// <summary>
        /// Проверяет текст запроса на корректность
        /// </summary>
        internal static bool IsQueryQueryCorrect(string userQuery, out string errorMessage)
        {
            errorMessage = null;
            using (QPConfiguration.OutOfTransaction())
            {
                try
                {
                    var viewName = $"uq_v_test_{DateTime.Now.Ticks}";
                    var schema = SqlQuerySyntaxHelper.DbSchemaName(QPContext.DatabaseType);
                    var createTestViewSql = $"CREATE VIEW {schema}{viewName} AS {userQuery}";
                    using (var connect = QPContext.CreateDbConnection())
                    {
                        connect.Open();
                        Common.ExecuteSql(connect, createTestViewSql);
                        Common.DropView(connect, viewName);
                    }

                    return true;
                }
                catch (SqlException ex)
                {
                    errorMessage = ex.ErrorsToString();
                    return false;
                }
                catch (NpgsqlException ex)
                {
                    errorMessage = ex.Message;
                    return false;
                }
            }
        }

        /// <summary>
        /// Возвращает информацию о столбцах запроса
        /// </summary>
        internal static IEnumerable<UserQueryColumn> GetQuerySchema(string userQuery)
        {
            using (var scope = new QPConnectionScope())
            {
                var viewName = $"uq_v_test_{DateTime.Now.Ticks}";
                var schema = SqlQuerySyntaxHelper.DbSchemaName(QPContext.DatabaseType);
                var createTestViewSql = $"CREATE VIEW {schema}{viewName} AS {userQuery}";
                RunCreateViewDdl(createTestViewSql);

                var dttU = Common.GetViewColumnUsage(scope.DbConnection, viewName);
                DropView(viewName);

                return DataTableToUserQueryColumns(dttU);
            }
        }

        /// <summary>
        /// Возвращает информацию о столбцах View
        /// </summary>
        internal static IEnumerable<UserQueryColumn> GetViewSchema(string viewName)
        {
            using (var scope = new QPConnectionScope())
            {
                return DataTableToUserQueryColumns(Common.GetViewColumnUsage(scope.DbConnection, viewName));
            }
        }

        private static IEnumerable<UserQueryColumn> DataTableToUserQueryColumns(DataTable dt)
        {
            return dt.AsEnumerable().Select(r => new UserQueryColumn
            {
                ColumnName = r.Field<string>("ColumnName"),
                DbType = r.Field<string>("DbType"),
                TableName = r.Field<string>("TableName"),
                TableDbType = r.Field<string>("TableDbType"),
                NumericScale = r.Field<int?>("NumericScale"),
                CharMaxLength = r.Field<int?>("CharMaxLength")
            }).ToArray();
        }

        /// <summary>
        /// Сохранить записи в таблице union_contents
        /// </summary>
        internal static void RecreateUserQuerySourcesInfo(Content uqVirtualContent)
        {
            var virtualContentId = uqVirtualContent.Id;
            RemoveUserQuerySourcesInfo(uqVirtualContent);
            DefaultRepository.SimpleSaveBulk(uqVirtualContent.UserQueryContentViewSchema.SelectUniqContentIDs().Select(id => new UserQueryContentsDAL
            {
                IsIdSource = false,
                RealContentId = id,
                VirtualContentId = virtualContentId
            }));
        }

        internal static void RemoveUserQuerySourcesInfo(Content uqVirtualContent)
        {
            var virtualContentId = uqVirtualContent.Id;
            var context = QPContext.EFContext;
            var recToRemove = context.UserQueryContentsSet.Where(u => u.VirtualContentId == virtualContentId).ToArray();
            DefaultRepository.SimpleDeleteBulk(recToRemove, context);
        }

        /// <summary>
        /// Возвращает граф связей контентов
        /// </summary>
        internal static Dictionary<int, int[]> GetContentRelationGraph()
        {
            using (var scope = new QPConnectionScope())
            {
                var relationView = Common.GetVirtualContentRelations(scope.DbConnection);
                var graph = relationView.AsEnumerable().GroupBy(r => r.Field<decimal>("BASE_CONTENT_ID")).Select(g => new
                {
                    BaseContentID = g.Key,
                    ParentContentIDs = g.Select(vr => vr.Field<decimal>("VIRTUAL_CONTENT_ID")).ToArray()
                });

                return graph.ToDictionary(p => Converter.ToInt32(p.BaseContentID), p => Converter.ToInt32Collection(p.ParentContentIDs).ToArray());
            }
        }

        internal static Field GetAcceptableBaseFieldForCloning(string fieldName, string contentIds, int virtualContentId, bool forNew)
        {
            using (var scope = new QPConnectionScope())
            {
                var ids = Common.GetAcceptableBaseFieldIdsForCloning(scope.DbConnection, fieldName, contentIds, virtualContentId, forNew).ToList();
                if (ids.Any())
                {
                    var id = ids.Select(vr => (int)vr.Field<decimal>("id")).First();
                    return FieldRepository.GetById(id);
                }

                return null;
            }
        }

        internal static IEnumerable<EntityObject> GetList(IEnumerable<int> ids)
        {
            var decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return MapperFacade.ContentMapper.GetBizList(QPContext.EFContext.ContentSet.Where(f => decIDs.Contains(f.Id)).ToList());
        }
    }
}
