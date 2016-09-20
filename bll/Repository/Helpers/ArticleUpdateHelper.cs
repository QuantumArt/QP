using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    public class ArticleUpdateHelper
    {
        private int Counter { get; set; }

        private StringBuilder Result { get; }

        private Article Article { get; set; }

        private string SqlString => Result.ToString();

        private List<SqlParameter> Parameters { get; }

        private string DataParamName => $"@qp_data{Counter}";

        private string BlobDataParamName => $"@qp_blob_data{Counter}";

        private string FieldParamName => $"@field{Counter}";

        private string LinkParamName => $"@link{Counter}";

        private string LinkValueParamName => $"@linkValue{Counter}";

        private string BackFieldParamName => $"@backField{Counter}";

        private string BackFieldValueParamName => $"@backFieldValue{Counter}";

        private static string ResultIdTableName => "#resultIds";

        private static string ItemParamName => "@ItemId";

        private static string ContentParamName => "@contentId";

        private static string StatusParamName => "@statusId";

        private static string VisibleParamName => "@visible";

        private static string ArchiveParamName => "@archive";

        private static string LastModifiedParamName => "@lastModifiedBy";

        private static string DelayedParamName => "@scheduleNewVersion";

        private static string SplittedParamName => "@splitted";

        private static string PermanentLockParamName => "@permanentLock";

        private static string CancelSplitParamName => "@cancelSplit";

        private static string InsertItemSql => string.Format("insert into content_item (CONTENT_ID, STATUS_TYPE_ID, VISIBLE, ARCHIVE, NOT_FOR_REPLICATION, LAST_MODIFIED_BY, SCHEDULE_NEW_VERSION_PUBLICATION, PERMANENT_LOCK) Values({0}, {1}, {2}, {3}, 1, {4}, {5}, {8});SELECT {6} = SCOPE_IDENTITY();SELECT {7} = splitted from content_item where content_item_id = {6};", ContentParamName, StatusParamName, VisibleParamName, ArchiveParamName, LastModifiedParamName, DelayedParamName, ItemParamName, SplittedParamName, PermanentLockParamName);

        private static string UpdateItemSql => string.Format("update content_item set modified = getdate(), last_modified_by = {0}, STATUS_TYPE_ID = {1}, VISIBLE = {2}, ARCHIVE = {3}, SCHEDULE_NEW_VERSION_PUBLICATION = {4}, PERMANENT_LOCK = {7}, CANCEL_SPLIT = {8} where content_item_id = {5};SELECT {6} = splitted from content_item where content_item_id = {5};", LastModifiedParamName, StatusParamName, VisibleParamName, ArchiveParamName, DelayedParamName, ItemParamName, SplittedParamName, PermanentLockParamName, CancelSplitParamName);

        private string UpdateDataSql => $"exec qp_update_data {ItemParamName}, {FieldParamName}, {DataParamName}, 1;";

        private string UpdateBlobDataSql => $"exec qp_update_blob_data {ItemParamName}, {FieldParamName}, {BlobDataParamName}, 1;";

        private static string AfterUpdateSql => $"exec qp_update_m2o_final {ItemParamName};";

        private static string ReplicateSql => $"exec qp_replicate {ItemParamName}";

        private static string CreateTempTableSql => $"create table {ResultIdTableName} (id numeric, attribute_id numeric not null, to_remove bit not null default 0);";

        private static string DropTempTableSql => $"drop table {ResultIdTableName};";

        private string UpdateM2MSql => $"exec qp_update_m2m {ItemParamName}, {LinkParamName}, {LinkValueParamName}, {SplittedParamName}, 0;";

        private string UpdateM2OSql => $"exec qp_update_m2o {ItemParamName}, {BackFieldParamName}, {BackFieldValueParamName}, 0;";

        public ArticleUpdateHelper()
        {
            Counter = 0;
            Result = new StringBuilder();
            Parameters = new List<SqlParameter>();
        }

        public ArticleUpdateHelper(Article item)
            : this()
        {
            Article = item;
        }

        private void CollectSql()
        {
            Result.AppendLine(CreateTempTableSql);
            CollectSqlForItem();

            foreach (var item in Article.FieldValues)
            {
                Parameters.Add(new SqlParameter(FieldParamName, SqlDbType.Int) { Value = item.Field.Id });
                if (item.Field.Type.DatabaseType == "NTEXT")
                {
                    CollectSqlForBlobData(item.Value);
                }
                else
                {
                    CollectSqlForData(item.Field, item.Value);
                    switch (item.Field.RelationType)
                    {
                        case RelationType.ManyToMany:
                            СollectSqlForManyToMany(item.Field, item.Value);
                            break;
                        case RelationType.ManyToOne:
                            СollectSqlForManyToOne(item.Field, item.Value);
                            break;
                    }
                }

                Counter++;
            }

            Result.AppendLine(ReplicateSql);
            Result.AppendLine(AfterUpdateSql);
            Result.AppendLine(DropTempTableSql);
        }

        private void CollectSqlForBlobData(string value)
        {
            Parameters.Add(new SqlParameter(BlobDataParamName, SqlDbType.NText) { Value = GetParameterValue(value) });
            Result.AppendLine(UpdateBlobDataSql);
        }

        private void CollectSqlForData(Field field, string value)
        {
            var data = field.TypeInfo.FormatFieldValue(value);
            if (field.Type.Name == FieldTypeName.DynamicImage)
            {
                data = GetDynamicImageData(field);
            }
            Parameters.Add(new SqlParameter(DataParamName, SqlDbType.NVarChar, 3500) { Value = GetParameterValue(data) });
            Result.AppendLine(UpdateDataSql);
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private void СollectSqlForManyToMany(Field field, string value)
        {
            Parameters.Add(new SqlParameter(LinkParamName, SqlDbType.Int) { Value = field.LinkId.Value });
            Parameters.Add(new SqlParameter(LinkValueParamName, SqlDbType.NVarChar, -1) { Value = !string.IsNullOrEmpty(value) ? (object)value : DBNull.Value });
            Result.AppendLine(UpdateM2MSql);
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private void СollectSqlForManyToOne(Field field, string value)
        {
            Parameters.Add(new SqlParameter(BackFieldParamName, SqlDbType.Int) { Value = field.BackRelationId.Value });
            Parameters.Add(new SqlParameter(BackFieldValueParamName, SqlDbType.NVarChar, -1) { Value = !string.IsNullOrEmpty(value) ? (object)value : DBNull.Value });
            Result.AppendLine(UpdateM2OSql);
        }

        private void CollectSqlForItem()
        {
            Parameters.Add(new SqlParameter(ItemParamName, SqlDbType.Int) { Value = Article.Id, Direction = ParameterDirection.InputOutput });
            Parameters.Add(new SqlParameter(SplittedParamName, SqlDbType.Bit) { Direction = ParameterDirection.Output });
            if (Article.IsNew)
            {
                Parameters.Add(new SqlParameter(ContentParamName, SqlDbType.Int) { Value = Article.ContentId });
                Result.AppendLine(InsertItemSql);
            }
            else
            {
                Result.AppendLine(UpdateItemSql);
            }

            Parameters.Add(new SqlParameter(StatusParamName, SqlDbType.Int) { Value = Article.StatusTypeId });
            Parameters.Add(new SqlParameter(LastModifiedParamName, SqlDbType.Int) { Value = QPContext.CurrentUserId });
            Parameters.Add(new SqlParameter(VisibleParamName, SqlDbType.Bit) { Value = Article.Visible });
            Parameters.Add(new SqlParameter(ArchiveParamName, SqlDbType.Bit) { Value = Article.Archived });
            Parameters.Add(new SqlParameter(DelayedParamName, SqlDbType.Bit) { Value = Article.Delayed });
            Parameters.Add(new SqlParameter(PermanentLockParamName, SqlDbType.Bit) { Value = Article.PermanentLock });
            Parameters.Add(new SqlParameter(CancelSplitParamName, SqlDbType.Bit) { Value = Article.CancelSplit });
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
            CollectSql();
            int id;
            using (new QPConnectionScope())
            {
                Common.ExecuteSql(QPConnectionScope.Current.DbConnection, SqlString, Parameters, ItemParamName, out id);
            }

            Article = ArticleRepository.GetById(id);
            return Article;
        }
    }
}
