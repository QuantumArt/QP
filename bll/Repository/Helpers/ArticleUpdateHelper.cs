using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Repository.Helpers
{
    public class ArticleUpdateHelper
    {
        #region private

        private int Counter { get; set; }

        private StringBuilder Result { get; set; }

        private Article Article { get; set; }

        /// <summary>
        /// результирующий SQL-запрос
        /// </summary>
        private string SqlString
        {
            get
            {
                return Result.ToString();
            }
        }

        /// <summary>
        /// накопленные SQL-параметры
        /// </summary>
        private List<SqlParameter> Parameters { get; set; }


        #region parameter names

        private string DataParamName
        {
            get
            {
                return String.Format("@qp_data{0}", Counter);
            }
        }

        private string BlobDataParamName
        {
            get
            {
                return String.Format("@qp_blob_data{0}", Counter);
            }
        }

        private string FieldParamName
        {
            get
            {
                return String.Format("@field{0}", Counter);
            }
        }

        private string LinkParamName
        {
            get
            {
                return String.Format("@link{0}", Counter);
            }
        }

        private string LinkValueParamName
        {
            get
            {
                return String.Format("@linkValue{0}", Counter);
            }
        }

        private string BackFieldParamName
        {
            get
            {
                return String.Format("@backField{0}", Counter);
            }
        }

        private string BackFieldValueParamName
        {
            get
            {
                return String.Format("@backFieldValue{0}", Counter);
            }
        }


        private string ResultIdTableName
        {
            get
            {
                return "#resultIds";
            }
        }

        private string ItemParamName
        {
            get
            {
                return "@ItemId";
            }
        }

        private string ContentParamName
        {
            get
            {
                return "@contentId";
            }
        }

        private string StatusParamName
        {
            get
            {
                return "@statusId";
            }
        }

        private string VisibleParamName
        {
            get
            {
                return "@visible";
            }
        }

        private string ArchiveParamName
        {
            get
            {
                return "@archive";
            }
        }

        private string LastModifiedParamName
        {
            get
            {
                return "@lastModifiedBy";
            }
        }

        private string DelayedParamName
        {
            get
            {
                return "@scheduleNewVersion";
            }
        }

        private string SplittedParamName
        {
            get
            {
                return "@splitted";
            }
        }

        private string PermanentLockParamName
        {
            get
            {
                return "@permanentLock";
            }
        }

        private string CancelSplitParamName
        {
            get
            {
                return "@cancelSplit";
            }
        }


        #endregion

        #region sql statements


        private string InsertItemSql
        {
            get
            {
                return String.Format("insert into content_item (CONTENT_ID, STATUS_TYPE_ID, VISIBLE, ARCHIVE, NOT_FOR_REPLICATION, LAST_MODIFIED_BY, SCHEDULE_NEW_VERSION_PUBLICATION, PERMANENT_LOCK) Values({0}, {1}, {2}, {3}, 1, {4}, {5}, {8});SELECT {6} = SCOPE_IDENTITY();SELECT {7} = splitted from content_item where content_item_id = {6};", ContentParamName, StatusParamName, VisibleParamName, ArchiveParamName, LastModifiedParamName, DelayedParamName, ItemParamName, SplittedParamName, PermanentLockParamName);
            }
        }

        private string UpdateItemSql
        {
            get
            {
                return String.Format("update content_item set modified = getdate(), last_modified_by = {0}, STATUS_TYPE_ID = {1}, VISIBLE = {2}, ARCHIVE = {3}, SCHEDULE_NEW_VERSION_PUBLICATION = {4}, PERMANENT_LOCK = {7}, CANCEL_SPLIT = {8} where content_item_id = {5};SELECT {6} = splitted from content_item where content_item_id = {5};", LastModifiedParamName, StatusParamName, VisibleParamName, ArchiveParamName, DelayedParamName, ItemParamName, SplittedParamName, PermanentLockParamName, CancelSplitParamName);
            }
        }

        private string UpdateDataSql
        {
            get
            {
                return String.Format("exec qp_update_data {0}, {1}, {2}, 1;", ItemParamName, FieldParamName, DataParamName);
            }
        }

        private string UpdateBlobDataSql
        {
            get
            {
                return String.Format("exec qp_update_blob_data {0}, {1}, {2}, 1;", ItemParamName, FieldParamName, BlobDataParamName);
            }
        }

        private string AfterUpdateSql
        {
            get
            {
                return String.Format("exec qp_update_m2o_final {0};", ItemParamName);
            }
        }

        private string ReplicateSql
        {
            get
            {
                return String.Format("exec qp_replicate {0}", ItemParamName);
            }
        }


        private string CreateTempTableSql
        {
            get
            {
                return String.Format("create table {0} (id numeric, attribute_id numeric not null, to_remove bit not null default 0);", ResultIdTableName);
            }
        }

        private string DropTempTableSql
        {
            get
            {
                return String.Format("drop table {0};", ResultIdTableName);
            }
        }

        private string UpdateM2MSql
        {
            get
            {
                return String.Format("exec qp_update_m2m {0}, {1}, {2}, {3}, 0;", ItemParamName, LinkParamName, LinkValueParamName, SplittedParamName);
            }
        }

        private string UpdateM2OSql
        {
            get
            {
                return String.Format("exec qp_update_m2o {0}, {1}, {2}, 0;", ItemParamName, BackFieldParamName, BackFieldValueParamName);
            }
        }

        #endregion

        #region collect sql
        /// <summary>
        /// Собирает SQL-строки и параметры для статьи
        /// </summary>
        private void CollectSql()
        {
            Result.AppendLine(CreateTempTableSql);
            CollectSqlForItem();

            foreach (FieldValue item in Article.FieldValues)
            {
                Parameters.Add(new SqlParameter(FieldParamName, SqlDbType.Int) { Value = item.Field.Id });
                if (item.Field.Type.DatabaseType == "NTEXT")
                {
                    CollectSqlForBlobData(item.Value);
                }
                else
                {
                    CollectSqlForData(item.Field, item.Value);

                    if (item.Field.RelationType == RelationType.ManyToMany)
                    {
                        СollectSqlForManyToMany(item.Field, item.Value);
                    }

                    else if (item.Field.RelationType == RelationType.ManyToOne)
                    {
                        СollectSqlForManyToOne(item.Field, item.Value);
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
            string data = field.TypeInfo.FormatFieldValue(value);
            if (field.Type.Name == FieldTypeName.DynamicImage)
            {
                data = GetDynamicImageData(field);
            }
            Parameters.Add(new SqlParameter(DataParamName, SqlDbType.NVarChar, 3500) { Value = GetParameterValue(data) });
            Result.AppendLine(UpdateDataSql);
        }


        private void СollectSqlForManyToMany(Field field, string value)
        {
            Parameters.Add(new SqlParameter(LinkParamName, SqlDbType.Int) { Value = field.LinkId.Value });
            Parameters.Add(new SqlParameter(LinkValueParamName, SqlDbType.NVarChar, -1) { Value = !String.IsNullOrEmpty(value) ? (object)value : DBNull.Value });
            Result.AppendLine(UpdateM2MSql);
        }

        private void СollectSqlForManyToOne(Field field, string value)
        {
            Parameters.Add(new SqlParameter(BackFieldParamName, SqlDbType.Int) { Value = field.BackRelationId.Value });
            Parameters.Add(new SqlParameter(BackFieldValueParamName, SqlDbType.NVarChar, -1) { Value = !String.IsNullOrEmpty(value) ? (object)value : DBNull.Value });
            Result.AppendLine(UpdateM2OSql);
        }

        private void CollectSqlForItem()
        {

            Parameters.Add(new SqlParameter(ItemParamName, SqlDbType.Int) { Value = Article.Id, Direction = ParameterDirection.InputOutput });
            Parameters.Add(new SqlParameter(SplittedParamName, SqlDbType.Bit) { Direction = ParameterDirection.Output }); // internal use only
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


        #endregion

        private static object GetParameterValue(string value)
        {
            return (String.IsNullOrEmpty(value)) ? DBNull.Value : (object)value;
        }

        private string GetDynamicImageData(Field field)
        {
            if (field.DynamicImage == null)
                return String.Empty;
            else
            {
                FieldValue baseFieldValue = Article.FieldValues.Where(n => n.Field.Id == field.BaseImageId).Single();
                return field.DynamicImage.GetValue(baseFieldValue.Value);
            }
        }

        #endregion

        #region constructors
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
        #endregion

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
