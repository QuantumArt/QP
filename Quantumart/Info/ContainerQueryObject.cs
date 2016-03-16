using System;
using System.Data;
using System.Data.SqlClient;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Pages;

namespace Quantumart.QPublishing.Info
{
    public class ContainerQueryObject : IQueryObject
    {
        public string Select { get; set; }
        public string From { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string StartRow { get; set; }
        public string PageSize { get; set; }
        public bool GetCount { get; set; }
        public bool GetCountInTable => false;
        public bool UseSecurity { get; set; }
        public int UserId { get; set; }
        public int GroupId { get; set; }
        public string StartLevel { get; set; }
        public string EndLevel { get; set; }
        public int ContentId { get; set; }
        public DBConnector Cnn { get; set; }
        public string CachePrefix { get; set; }
        public bool CacheResult { get; set; }
        public double CacheInterval { get; set; }
        public bool WithReset { get; set; }

        private const string InsertKey = "<$_security_insert_$>";

        public bool IsFirstPage => Int32.Parse(StartRow) <= 1;

        public ContainerQueryObject(DBConnector cnn, string select, string from, string where, string orderBy, string startRow, string pageSize)
        {
            Cnn = cnn;
            Select = select; 
            From = from; 
            Where = where; 
            OrderBy = orderBy; 
            StartRow = startRow;
            PageSize = pageSize;
        }


        public ContainerQueryObject(DBConnector cnn, string select, string from, string where, string orderBy, string startRow, string pageSize, bool getCount, bool useSecurity, double duration, string startLevel, string endLevel, int contentId, int uid, int gid)
        : this (cnn, select, from, where, orderBy, startRow, pageSize)
        {
            GetCount = getCount;
            UseSecurity = useSecurity;
            CacheInterval = duration / 60;
            CacheResult = CacheInterval > 0;
            if (UseSecurity)
            {
                UserId = uid;
                GroupId = gid;
                ContentId = contentId;
                StartLevel = startLevel;
                EndLevel = endLevel;
            }
        }

        public SqlCommand GetSqlCommand()
        {
            var cmd = new SqlCommand
            {
                CommandText = "qp_GetContentPage",
                CommandType = CommandType.StoredProcedure
            };

            if (UseSecurity)
            {
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@use_security", DbType = DbType.Int32, Value = "1" });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@user_id", DbType = DbType.Decimal, Value = UserId });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@group_id", DbType = DbType.Decimal, Value = GroupId });

                cmd.Parameters.Add(new SqlParameter { ParameterName = "@start_level", DbType = DbType.Int32, Value = StartLevel });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@end_level", DbType = DbType.Int32, Value = EndLevel });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@entity_name", DbType = DbType.String, Value = "content_item" });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@parent_entity_name", DbType = DbType.String, Value = "content" });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@parent_entity_id", DbType = DbType.Decimal, Value = ContentId.ToString() });
                cmd.Parameters.Add(new SqlParameter { ParameterName = "@insert_key", DbType = DbType.String, Value = InsertKey });
            }


            cmd.Parameters.Add(new SqlParameter { ParameterName = "@Select", DbType = DbType.String, Value = Select });
            cmd.Parameters.Add(new SqlParameter { ParameterName = "@From", DbType = DbType.String, Value = From });
            cmd.Parameters.Add(new SqlParameter { ParameterName = "@Where", DbType = DbType.String, Value = QPageEssential.CleanSql(Where) });
            cmd.Parameters.Add(new SqlParameter { ParameterName = "@OrderBy", DbType = DbType.String, Value = QPageEssential.CleanSql(OrderBy) });
            cmd.Parameters.Add(new SqlParameter { ParameterName = "@StartRow", DbType = DbType.Int32, Value = StartRow });
            cmd.Parameters.Add(new SqlParameter { ParameterName = "@PageSize", DbType = DbType.Int32, Value = PageSize });

            cmd.Parameters.Add(new SqlParameter { ParameterName = "@GetCount", DbType = DbType.Int32, Value = GetCount });
            cmd.Parameters.Add(new SqlParameter { ParameterName = "@TotalRecords", DbType = DbType.Int32, Direction = ParameterDirection.Output });

            return cmd;
        }

        public string GetKey(string prefix)
        {
            return
                $"{prefix}{"GetContainer."}::{Select}::{From}::{Where}::{OrderBy}::{StartRow}::{PageSize}::{UserId}::{GroupId}::{StartLevel}::{EndLevel}";
        }

        public string OutputParamName => "@TotalRecords";

        public string CountSql
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
