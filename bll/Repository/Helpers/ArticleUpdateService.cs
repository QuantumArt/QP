using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    public class ArticleUpdateService
    {
        private int Counter { get; set; }

        private Article Article { get; set; }

        private List<SqlParameter> Parameters { get; }

        private string FieldParamName => $"@field{Counter}";

        private static string ContentItemId => "@ItemId";

        private static string SplittedParamName => "@splitted";

        public ArticleUpdateService()
        {
            Counter = 0;
            Parameters = new List<SqlParameter>();
        }

        public ArticleUpdateService(Article item)
            : this()
        {
            Article = item;
        }

        private string GetSqlQuery()
        {
            var sqlResult = new StringBuilder();
            sqlResult.AppendLine("CREATE TABLE #resultIds (id numeric, attribute_id numeric not null, to_remove bit not null default 0);");
            sqlResult = CollectSqlForItem(sqlResult);

            foreach (var item in Article.FieldValues)
            {
                Parameters.Add(new SqlParameter(FieldParamName, SqlDbType.Int) { Value = item.Field.Id });
                if (item.Field.Type.DatabaseType == "NTEXT")
                {
                    sqlResult = CollectSqlForBlobData(sqlResult, item.Value);
                }
                else
                {
                    sqlResult = CollectSqlForData(sqlResult, item.Field, item.Value);
                    switch (item.Field.RelationType)
                    {
                        case RelationType.ManyToMany:
                            sqlResult = СollectSqlForManyToMany(sqlResult, item.Field, item.Value);
                            break;
                        case RelationType.ManyToOne:
                            sqlResult = СollectSqlForManyToOne(sqlResult, item.Field, item.Value);
                            break;
                    }
                }

                Counter++;
            }

            sqlResult.AppendLine($"EXEC qp_replicate {ContentItemId}");
            sqlResult.AppendLine($"EXEC qp_update_m2o_final {ContentItemId};");
            sqlResult.AppendLine("DROP TABLE #resultIds;");
            return sqlResult.ToString();
        }

        private StringBuilder CollectSqlForBlobData(StringBuilder sqlResult, string value)
        {
            var blobDataParamName = $"@qp_blob_data{Counter}";
            Parameters.Add(new SqlParameter(blobDataParamName, SqlDbType.NText) { Value = GetParameterValue(value) });
            sqlResult.AppendLine($"EXEC qp_update_blob_data {ContentItemId}, {FieldParamName}, {blobDataParamName}, 1;");
            return sqlResult;
        }

        private StringBuilder CollectSqlForData(StringBuilder sqlResult, Field field, string value)
        {
            var dataParamName = $"@qp_data{Counter}";
            var data = field.TypeInfo.FormatFieldValue(value);
            if (field.Type.Name == FieldTypeName.DynamicImage)
            {
                data = GetDynamicImageData(field);
            }

            Parameters.Add(new SqlParameter(dataParamName, SqlDbType.NVarChar, 3500) { Value = GetParameterValue(data) });
            sqlResult.AppendLine($"EXEC qp_update_data {ContentItemId}, {FieldParamName}, {dataParamName}, 1;");
            return sqlResult;
        }

        private StringBuilder СollectSqlForManyToMany(StringBuilder sqlResult, Field field, string value)
        {
            var linkParamName = $"@link{Counter}";
            var linkValueParamName = $"@linkValue{Counter}";
            Parameters.Add(new SqlParameter(linkParamName, SqlDbType.Int) { Value = field.LinkId.Value });
            Parameters.Add(new SqlParameter(linkValueParamName, SqlDbType.NVarChar, -1) { Value = !string.IsNullOrEmpty(value) ? (object)value : DBNull.Value });
            sqlResult.AppendLine($"EXEC qp_update_m2m {ContentItemId}, {linkParamName}, {linkValueParamName}, {SplittedParamName}, 0;");
            return sqlResult;
        }

        private StringBuilder СollectSqlForManyToOne(StringBuilder sqlResult, Field field, string value)
        {
            var backFieldParamName = $"@backField{Counter}";
            var backFieldValueParamName = $"@backFieldValue{Counter}";
            Parameters.Add(new SqlParameter(backFieldParamName, SqlDbType.Int) { Value = field.BackRelationId.Value });
            Parameters.Add(new SqlParameter(backFieldValueParamName, SqlDbType.NVarChar, -1) { Value = !string.IsNullOrEmpty(value) ? (object)value : DBNull.Value });
            sqlResult.AppendLine($"EXEC qp_update_m2o {ContentItemId}, {backFieldParamName}, {backFieldValueParamName}, 0;");
            return sqlResult;
        }

        private StringBuilder CollectSqlForItem(StringBuilder sqlResult)
        {
            const string contentParamName = "@contentId";
            const string statusParamName = "@statusId";
            const string visibleParamName = "@visible";
            const string archiveParamName = "@archive";
            const string lastModifiedParamName = "@lastModifiedBy";
            const string delayedParamName = "@scheduleNewVersion";
            const string permanentLockParamName = "@permanentLock";
            const string uniqueIdParamName = "@uniqueId";
            const string cancelSplitParamName = "@cancelSplit";

            Parameters.Add(new SqlParameter(ContentItemId, SqlDbType.Int) { Value = Article.Id, Direction = ParameterDirection.InputOutput });
            Parameters.Add(new SqlParameter(SplittedParamName, SqlDbType.Bit) { Direction = ParameterDirection.Output });
            if (Article.IsNew)
            {
                Parameters.Add(new SqlParameter(contentParamName, SqlDbType.Int) { Value = Article.ContentId });
                sqlResult.AppendLine($"INSERT INTO content_item (CONTENT_ID, STATUS_TYPE_ID, VISIBLE, ARCHIVE, NOT_FOR_REPLICATION, LAST_MODIFIED_BY, SCHEDULE_NEW_VERSION_PUBLICATION, PERMANENT_LOCK, UNIQUE_ID) Values({contentParamName}, {statusParamName}, {visibleParamName}, {archiveParamName}, 1, {lastModifiedParamName}, {delayedParamName}, {permanentLockParamName}, {uniqueIdParamName});SELECT {ContentItemId} = SCOPE_IDENTITY();SELECT {SplittedParamName} = splitted FROM content_item WHERE content_item_id = {ContentItemId};");
            }
            else
            {
                sqlResult.AppendLine($"UPDATE content_item SET modified = getdate(), last_modified_by = {lastModifiedParamName}, STATUS_TYPE_ID = {statusParamName}, VISIBLE = {visibleParamName}, ARCHIVE = {archiveParamName}, SCHEDULE_NEW_VERSION_PUBLICATION = {delayedParamName}, PERMANENT_LOCK = {permanentLockParamName}, UNIQUE_ID = {uniqueIdParamName}, CANCEL_SPLIT = {cancelSplitParamName} WHERE content_item_id = {ContentItemId};SELECT {SplittedParamName} = splitted FROM content_item WHERE content_item_id = {ContentItemId};");
            }

            Parameters.Add(new SqlParameter(statusParamName, SqlDbType.Int) { Value = Article.StatusTypeId });
            Parameters.Add(new SqlParameter(visibleParamName, SqlDbType.Bit) { Value = Article.Visible });
            Parameters.Add(new SqlParameter(archiveParamName, SqlDbType.Bit) { Value = Article.Archived });
            Parameters.Add(new SqlParameter(lastModifiedParamName, SqlDbType.Int) { Value = QPContext.CurrentUserId });
            Parameters.Add(new SqlParameter(delayedParamName, SqlDbType.Bit) { Value = Article.Delayed });
            Parameters.Add(new SqlParameter(permanentLockParamName, SqlDbType.Bit) { Value = Article.PermanentLock });
            Parameters.Add(new SqlParameter(uniqueIdParamName, SqlDbType.UniqueIdentifier) { Value = Article.UniqueId ?? Guid.NewGuid() });
            Parameters.Add(new SqlParameter(cancelSplitParamName, SqlDbType.Bit) { Value = Article.CancelSplit });
            return sqlResult;
        }

        private static object GetParameterValue(string value)
        {
            return string.IsNullOrEmpty(value) ? DBNull.Value : (object)value;
        }

        private string GetDynamicImageData(Field field)
        {
            if (field.DynamicImage == null)
            {
                return string.Empty;
            }

            var baseFieldValue = Article.FieldValues.Single(n => n.Field.Id == field.BaseImageId);
            return field.DynamicImage.GetValue(baseFieldValue.Value);
        }

        public Article Update()
        {
            int id;
            using (new QPConnectionScope())
            {
                Common.ExecuteSql(QPConnectionScope.Current.DbConnection, GetSqlQuery(), Parameters, ContentItemId, out id);
            }

            Article = ArticleRepository.GetById(id);
            return Article;
        }
    }
}
