using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Quantumart.QPublishing.Database;

namespace Quantumart.QPublishing.Info
{
    public class ContentDataQueryObject : IQueryObject
    {
        public DBConnector Cnn { get; set; }
        public bool GetCount { get; set; }
        public bool GetCountInTable => true;
        public int ContentId { get; set; }
        public string SiteName { get; set; }
        public string ContentName { get; set; }
        public string Fields { get; set; }
        public string Where { get; set; }
        public string OrderBy { get; set; }
        public string ExtraFrom { get; set; }
        public long StartRow { get; set; }
        public long PageSize { get; set; }
        public byte UseSchedule { get; set; }        
        public string StatusName { get; set; }
        public byte ShowSplittedArticle { get; set; }
        public byte IncludeArchive { get; set; }

        public long UserId { get; set; }
        public long GroupId { get; set; }
        public long StartLevel { get; set; }
        public long EndLevel { get; set; }
        public bool FilterRecords { get; set; }

        public bool UseClientSelection { get; set; }
        public bool UseSecurity { get; set; }

        public bool CacheResult { get; set; }
        public double CacheInterval { get; set; }

        public bool WithReset { get; set; }
        public List<SqlParameter> Parameters { get; set; }
        public string CountSql { get; private set; }

        private static readonly string InsertKey = "<$_security_insert_$>";
        private static readonly string DefaultStatusName = "Published";
        private static readonly string DefaultOrderBy = "c.content_item_id";
        private static readonly Regex DescRegex = new Regex(@"desc$", RegexOptions.IgnoreCase);
        private static readonly Regex AscRegex = new Regex(@"asc$", RegexOptions.IgnoreCase);
        private static readonly Regex CRegex = new Regex(@"^c\.", RegexOptions.IgnoreCase);

        public string WhereExpression => UseClientSelection ? String.Empty : Where;

        public string OrderByExpression => UseClientSelection ? "CONTENT_ITEM_ID ASC" : OrderBy;

        public long StartRowExpression => UseClientSelection ? 1 : StartRow;

        public long PageSizeExpression => UseClientSelection ? 0 : PageSize;

        public bool IsFirstPage => StartRowExpression <= 1;

        public ContentDataQueryObject(DBConnector cnn, int contentId, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive, bool cacheResult, double cacheInterval, bool useClientSelection, bool withReset)
            : this(cnn, String.Empty, String.Empty, fields, whereExpression, orderExpression, startRow, pageSize, useSchedule, statusName, showSplittedArticle, includeArchive, cacheResult, cacheInterval, useClientSelection, withReset)
        {
            ContentId = contentId;
        }


        public ContentDataQueryObject(DBConnector cnn, int contentId, string fields, string whereExpression, string orderExpression, long startRow, long pageSize)
            : this(cnn, String.Empty, String.Empty, fields, whereExpression, orderExpression, startRow, pageSize)
        {
            ContentId = contentId;
        }


        public ContentDataQueryObject(DBConnector cnn, string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize)
            : this (cnn, siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, 1, DefaultStatusName, 0, 0, false, 0, false, false)
        {
            
        }

        public ContentDataQueryObject(DBConnector cnn, string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive, bool cacheResult, double cacheInterval, bool useClientSelection, bool withReset)
        {
            Parameters = new List<SqlParameter>();
            SiteName = siteName; 
            ContentName = contentName; 
            Fields = fields; 
            Where = whereExpression; 
            OrderBy = orderExpression; 
            StartRow = startRow; 
            PageSize = pageSize; 
            UseSchedule = useSchedule; 
            StatusName = statusName; 
            ShowSplittedArticle = showSplittedArticle;
            IncludeArchive = includeArchive; 
            Cnn = cnn;
            UseClientSelection = useClientSelection; 
            CacheResult = cacheResult; 
            CacheInterval = cacheInterval; 
            WithReset = withReset;
            GetCount = true;
        }

        public SqlCommand GetSqlCommand()
        {
            int contentId;
            int siteId = 0;
            if (ContentId != 0)
            {
                contentId = ContentId;
            }
            else
            {
                siteId = Cnn.GetSiteId(SiteName);
                if (siteId == 0)
                    throw new ApplicationException($"Site '{SiteName}' is not found");
                contentId = Cnn.GetContentId(Cnn.GetSiteId(SiteName), ContentName);
                if (contentId == 0)
                    throw new ApplicationException($"Content '{SiteName}.{ContentName}' is not found");
            }

            string select = GetSqlCommandSelect(contentId);
            string from = GetSqlCommandFrom(contentId);
            if (UseSecurity)
            {
                from = from.Replace(InsertKey, Cnn.GetSecuritySql(contentId, UserId, GroupId, StartLevel, EndLevel));
            }
            string where = GetSqlCommandWhere(siteId);
            string orderBy = GetSqlCommandOrderBy();
            long startRow = StartRowExpression <= 0 ? 1 : StartRowExpression;
            long endRow = new long[] { 0, Int32.MaxValue, Int32.MaxValue - 1 }.Contains(PageSizeExpression) ? 0 : startRow + PageSizeExpression - 1;

            CountSql = $"SELECT cast(COUNT(*) as bigint) FROM {@from} WHERE {@where}";

            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["Quantumart.dll_SQL2012Mode"]) || GetCount)
            {
                sb.AppendLine("WITH PAGED_DATA_CTE AS");
                sb.AppendLine("(");
                sb.AppendLine(
                    $"	SELECT c.*, ROW_NUMBER() OVER (ORDER BY {orderBy}) AS ROW_NUMBER, {(GetCount ? "COUNT(*) OVER()" : "0")} AS ROWS_COUNT");
                sb.AppendLine($"	FROM ( SELECT {@select} FROM {@from} WHERE {@where} ) AS c");
                sb.AppendLine(")");
                sb.AppendLine("SELECT * FROM PAGED_DATA_CTE");
                if (endRow > 0 || startRow > 1)
                {
                    sb.AppendLine(" WHERE 1 = 1");
                    if (startRow > 1)
                    {
                        sb.AppendLine(" AND ROW_NUMBER >= @startRow");
                        Parameters.Add(new SqlParameter("@startRow", SqlDbType.Int) { Value = startRow });
                    }
                    if (endRow > 0)
                    {
                        sb.AppendLine(" AND ROW_NUMBER <= @endRow");
                        Parameters.Add(new SqlParameter("@endRow", SqlDbType.Int) { Value = endRow });
                    }
                }
                sb.AppendLine("ORDER BY ROW_NUMBER ASC");
            }
            else
            {
                sb.AppendLine($"SELECT {@select} FROM {@from} WHERE {@where} ");
                sb.Append("ORDER BY ");
                sb.AppendLine(orderBy);
                if (endRow > 0 || startRow > 1)
                {
                    if (endRow != int.MaxValue)
                    {
                        Parameters.Add(new SqlParameter("@startRow", SqlDbType.Int) { Value = startRow - 1 });
                        Parameters.Add(new SqlParameter("@endRow", SqlDbType.Int) { Value = endRow });
                        sb.AppendLine(@"OFFSET @startRow ROWS FETCH NEXT @endRow - @startRow ROWS ONLY");
                    }
                    else
                    {
                        if (startRow > 1)
                        {
                            Parameters.Add(new SqlParameter("@startRow", SqlDbType.Int) { Value = startRow - 1 });
                            sb.AppendLine(@"OFFSET @startRow ROWS");
                        }
                    }
                }
            }

            SqlCommand cmd = new SqlCommand
            {
                CommandText = sb.ToString(),
                CommandType = CommandType.Text
            };

            if (Parameters != null)
                foreach (var param in Parameters)
                    cmd.Parameters.Add(param);
            return cmd;
        }

        private string GetSqlCommandOrderBy()
        {
            return !String.IsNullOrEmpty(OrderByExpression) ? OrderByExpression : DefaultOrderBy;
        }


        private string GetSqlCommandFrom(int contentId)
        {
            string tableSuffix = ShowSplittedArticle == 0 ? "" : "_united";
            string from = "content_" + contentId + tableSuffix + " as c WITH(NOLOCK) ";
            if (UseSecurity)
            {
                if (FilterRecords)
                    from += $" INNER JOIN ({InsertKey}) as pi on c.content_item_id = pi.content_item_id ";
                else
                    from += $" LEFT OUTER JOIN ({InsertKey}) as pi on c.content_item_id = pi.content_item_id ";
            }
            if (!String.IsNullOrEmpty(ExtraFrom))
            {
                from += " " + ExtraFrom;
            }
            return from;
        }

        private string GetSqlCommandWhere(int siteId)
        {
            StringBuilder whereBuilder = new StringBuilder(!String.IsNullOrEmpty(WhereExpression) ? WhereExpression : "1 = 1");
            if (UseSchedule == 1)
                whereBuilder.Append(" and c.visible = 1");

            if (IncludeArchive == 0)
                whereBuilder.Append(" and c.archive = 0");
            whereBuilder.AppendFormat(" and c.status_type_id in ({0})", GetSqlCommandStatusString(siteId));
            return whereBuilder.ToString();
        }

        private string GetSqlCommandStatusString(int siteId)
        {
            string statusString;
            if (String.IsNullOrEmpty(StatusName) && siteId != 0)
            {
                statusString = Cnn.GetMaximumWeightStatusTypeId(siteId).ToString();
            }
            else
            {
                string statusName = !String.IsNullOrEmpty(StatusName) ? StatusName : DefaultStatusName;
                bool filterStatuses = siteId != 0 && statusName.ToLowerInvariant() != DefaultStatusName.ToLowerInvariant();

                HashSet<string> statuses = !filterStatuses ? null : new HashSet<string>(Cnn.GetStatuses("")
                        .OfType<DataRowView>()
                        .Select(n => n.Row.Field<string>("STATUS_TYPE_NAME").ToLowerInvariant())
                    );

                Func<string, bool> lambda = n => (statuses == null) || statuses.Contains(n.ToLowerInvariant());

                var resultStatuses = statusName.Split(',').Select(n => n.Trim()).Where(lambda).ToArray();

                if (!resultStatuses.Any())
                    throw new ApplicationException($"None of the given statuses ({statusName}) has been found");

                string[] statusParams = new string[resultStatuses.Length];
                for (var i = 0; i < resultStatuses.Length; i++)
                {
                    string paramName = "@status" + i;
                    Parameters.Add(new SqlParameter(paramName, SqlDbType.NVarChar, 255) { Value = resultStatuses[i]});
                    statusParams[i] = paramName;
                }

                statusString = "select status_type_id from status_type where status_type_name in (" + String.Join(", ", statusParams) + ")";
            }
            return statusString;
        }

        private string GetSqlCommandSelect(int contentId)
        {
            string select = null;

            if (!String.IsNullOrEmpty(Fields))
            {
                string orderBy = GetSqlCommandOrderBy();
                string[] orderByAttrs = String.IsNullOrEmpty(orderBy) ? new string[] { } : orderBy
                    .Split(',')
                    .Select(n => n.Trim())
                    .Select(n => CRegex.Replace(AscRegex.Replace(DescRegex.Replace(n, ""), ""), ""))
                    .Select(n => n.Trim().Replace("[", "").Replace("]", ""))
                    .ToArray();
                
                HashSet<string> attrs = new HashSet<string>(
                    Cnn.GetContentAttributeObjects(contentId)
                        .Select(n => n.Name.ToLowerInvariant())
                        .Union(new[] { "content_item_id", "archive", "visible", "created", "modified", "last_modified_by" })
                );

                select = String.Join(", ", Fields
                    .Split(',')
                    .Select(n => n.Trim().Replace("[", "").Replace("]", ""))
                    .Union(orderByAttrs, StringComparer.InvariantCultureIgnoreCase)
                    .Where(n => attrs.Contains(n.ToLowerInvariant()))
                    .Select(n => "[" + n + "]")
                    .ToArray()
                );
            }

            if (String.IsNullOrEmpty(select))
                select = "c.*";

            if (UseSecurity && !FilterRecords)
                select += ", IsNull(pi.permission_level, 0) as current_permission_level ";

            return select;
        }


        public string GetKey(string prefix)
        {
            return
                $"{prefix}{"GetContentData."}::{SiteName}::{ContentName}::{Fields}::{WhereExpression}::{OrderByExpression}::{StartRowExpression}::{PageSizeExpression}::{UseSchedule}::{StatusName}::{ShowSplittedArticle}::{IncludeArchive}::{UserId}::{GroupId}::{StartLevel}::{EndLevel}::{FilterRecords}";
        }

        public string OutputParamName => "@total_records";
    }
}
