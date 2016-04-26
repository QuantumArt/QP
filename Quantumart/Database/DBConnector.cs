using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Reflection;
using System.Xml;
using System.Linq;
using Microsoft.Win32;
using Microsoft.VisualBasic;
using Quantumart.QPublishing.FileSystem;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.Info;
using Quantumart.QPublishing.Resizer;


namespace Quantumart.QPublishing.Database
{

    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {

        #region fields and properties

        private static readonly string IdentityParamString = "@itemId";

        public static readonly string LastModifiedByKey = "QP_LAST_MODIFIED_BY_KEY";

        internal static readonly int LegacyNotFound = -1;

        public DbCacheManager CacheManager { get; internal set; }

        public bool ForceLocalCache { get; set; }

        private int? _lastModifiedBy;
        public int LastModifiedBy
        {
            get
            {
                var result = 1;
                if (_lastModifiedBy.HasValue)
                {
                    result = _lastModifiedBy.Value;
                }
                else if (HttpContext.Current != null && HttpContext.Current.Items.Contains(LastModifiedByKey))
                {
                    result = (int)HttpContext.Current.Items[LastModifiedByKey];
                }
                return result;
            }
            set
            {
                _lastModifiedBy = value;
            }
        }

        public bool UseLocalCache => HttpRuntime.Cache == null || ForceLocalCache;

        public bool CacheData { get; set; }

        public bool UpdateManyToMany { get; set; }

        public bool UpdateManyToOne { get; set; }

        public bool ThrowNotificationExceptions { get; set; }

        public static string ConnectionString { get; set; }

        public string CustomConnectionString { get; set; }

        public string InstanceConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(CustomConnectionString))
                {
                    return CustomConnectionString;
                }
                else
                {
                    return ConnectionString;
                }
            }
        }

        public IDbConnection ExternalConnection { get; set; }

        public IDbTransaction ExternalTransaction { get; set; }

        private IDbConnection InternalConnection { get; set; }

        private IDbTransaction InternalTransaction { get; set; }

        private void CreateInternalConnection(bool withTransaction)
        {
            InternalConnection = GetActualSqlConnection();
            if (InternalConnection.State == ConnectionState.Closed)
                InternalConnection.Open();
            if (withTransaction)
            {
                var extTr = GetActualSqlTransaction();
                InternalTransaction = extTr ?? InternalConnection.BeginTransaction();
            }
        }

        private void CommitInternalTransaction()
        {
            if (ExternalTransaction == null)
                InternalTransaction.Commit();
        }

        private void DisposeInternalConnection()
        {
            if (ExternalConnection == null)
            {
                InternalConnection.Dispose();
                InternalConnection = null;
                InternalTransaction = null;
            }
        }


        private bool NeedToDisposeActualSqlConnection => ExternalConnection == null && InternalConnection == null;

        private string _instanceCachePrefix;
        public string InstanceCachePrefix => _instanceCachePrefix ?? (_instanceCachePrefix = ExtractCachePrefix(InstanceConnectionString));

        public static string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public static NameValueCollection AppSettings => ConfigurationManager.AppSettings;

        public IDynamicImage DynamicImageCreator { get; set; }

        private bool? _isStage;
        public bool IsStage
        {
            get
            {
                if (_isStage.HasValue)
                {
                    return _isStage.Value;
                }
                else if (CacheManager.Page != null)
                {
                    return CacheManager.Page.IsStage;
                }
                else
                {
                    return !CheckIsLive();
                }
            }
            set
            {
                _isStage = value;
            }

        }

        public IFileSystem FileSystem { get; set; }

        internal string UploadPlaceHolder => "<%=upload_url%>";

        internal string SitePlaceHolder => "<%=site_url%>";

        internal string UploadBindingPlaceHolder => "<%#upload_url%>";

        internal string SiteBindingPlaceHolder => "<%#site_url%>";

        #endregion

        #region constructors

        static DBConnector()
        {
            if (ConfigurationManager.ConnectionStrings["qp_database"] != null)
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString;
                ConfigurationManager.AppSettings["ConnectionString"] = ConnectionString;
            }
        }

        public DBConnector()
            : this(ConnectionString)
        {
        }

        public DBConnector(string strConnectionString)
        {

            ForceLocalCache = false;
            CacheData = true;
            UpdateManyToMany = true;
            UpdateManyToOne = true;
            ThrowNotificationExceptions = true;

            CustomConnectionString = strConnectionString;
            CacheManager = new DbCacheManager(this);
            FileSystem = new RealFileSystem();
            DynamicImageCreator = new DynamicImage();
        }

        public DBConnector(IDbConnection connection)
            : this(connection.ConnectionString)
        {
            ExternalConnection = connection;
        }

        public DBConnector(IDbConnection connection, IDbTransaction transaction)
            : this(connection)
        {
            ExternalTransaction = transaction;
        }

        #endregion

        #region static methods

        public static string GetConnectionString(string customerCode)
        {
            var doc = GetQpConfig();
            var node = doc.SelectSingleNode("configuration/customers/customer[@customer_name='" + customerCode + "']/db/text()");
            if (node != null)
            {
                return node.Value.Replace("Provider=SQLOLEDB;", "");
            }
            else
            {
                throw new InvalidOperationException("Cannot load connection string for Asp.NET in QP7 configuration file");
            }
        }

        public static XmlDocument GetQpConfig()
        {
            var qKey = Registry.LocalMachine.OpenSubKey("Software\\Quantum Art\\Q-Publishing");
            if (qKey != null)
            {
                var regValue = qKey.GetValue("Configuration File");
                if (regValue != null)
                {
                    var doc = new XmlDocument();
                    doc.Load(regValue.ToString());
                    return doc;
                }
                else
                {
                    throw new InvalidOperationException("QP7 records in the registry are inconsistent or damaged");
                }
            }
            else
            {
                throw new InvalidOperationException("QP7 is not installed");
            }
        }

        public static string GetQpTempDirectory()
        {
            var doc = GetQpConfig();
            var node = doc.SelectSingleNode("configuration/app_vars/app_var[@app_var_name='TempDirectory']/text()");
            if (node != null)
            {
                return node.Value;
            }
            else
            {
                throw new InvalidOperationException("Cannot load TempDirectory parameter from QP7 configuration file");
            }
        }

        public static string GetString(object obj, string defaultValue)
        {
            var result = Convert.ToString(obj);
            return String.IsNullOrEmpty(result) ? defaultValue : result;
        }

        public static bool GetNumBool(object obj)
        {
            return Convert.ToBoolean((decimal)obj);
        }

        public static int GetNumInt(object obj)
        {
            return (int)(decimal)obj;
        }

        private static string ExtractCachePrefix(string cnnString)
        {
            var result = String.Empty;
            if (cnnString != null)
            {
                var cnnParams = cnnString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable().Select(s => new { Key = s.Split('=')[0], Value = s.Split('=')[1] }).ToArray();
                var dbName = cnnParams
                    .Where(n => string.Equals(n.Key, "Initial Catalog", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(n.Key, "Database", StringComparison.InvariantCultureIgnoreCase))
                    .Select(n => n.Value)
                    .FirstOrDefault();

                if (dbName == null)
                    throw new ArgumentException("The connection supplied string should contain at least 'Initial Catalog' or 'Database' keyword.");


                var serverName = cnnParams
                    .Where(n =>  string.Equals(n.Key, "Data Source", StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(n.Key, "Server", StringComparison.InvariantCultureIgnoreCase))
                    .Select(n => n.Value)
                    .FirstOrDefault();

                if (serverName == null)
                    throw new ArgumentException("The connection string supplied should contain at least 'Data Source' or 'Server' keyword.");


                result = $"{dbName}.{serverName}.";
            }
            return result;
        }
        #endregion

        #region Utility methods

        public string FormatField(string input, int siteId, bool isLive)
        {
            var result = input;
            var uploadUrl = GetImagesUploadUrl(siteId, true);
            result = result.Replace(UploadPlaceHolder, uploadUrl);
            result = result.Replace(UploadBindingPlaceHolder, uploadUrl);
            var siteUrl = AppSettings["UseAbsoluteSiteUrl"] == "1" ? GetSiteUrl(siteId, isLive) : GetSiteUrlRel(siteId, isLive);
            result = result.Replace(SitePlaceHolder, siteUrl);
            result = result.Replace(SiteBindingPlaceHolder, siteUrl);
            return result;
        }

        public string FormatField(string input, int siteId)
        {
            return FormatField(input, siteId, !IsStage);
        }


        public int InsertDataWithIdentity(string queryString)
        {
            var command = new SqlCommand(queryString + ";SELECT @Identity = SCOPE_IDENTITY();");
            var idParam = command.Parameters.Add("@Identity", SqlDbType.Decimal);
            idParam.Direction = ParameterDirection.Output;
            ProcessData(command);
            return GetNumInt(idParam.Value);
        }

        public int GetIdentityId(SqlCommand command)
        {
            if (command.Parameters.Contains(IdentityParamString))
            {
                return GetNumInt(command.Parameters[IdentityParamString].Value);
            }
            else
            {
                return 0;
            }
        }

        public string ReplaceCommas(string str)
        {
            var sb = new StringBuilder(str);
            sb.Replace(",", ".");
            return sb.ToString();
        }

        public string GetCachedFileContents(string path)
        {
            var key = CacheManager.FileContentsCacheKeyPrefix + path.ToLowerInvariant();
            return GetCachedEntity(key, LoadFileContents);
        }

        private string LoadFileContents(string key)
        {
            var path = key.Replace(CacheManager.FileContentsCacheKeyPrefix, String.Empty);
            return File.ReadAllText(path);
        }

        private SqlConnection GetActualSqlConnection(string internalConnectionString)
        {
            return (InternalConnection ?? ExternalConnection) as SqlConnection ?? new SqlConnection(internalConnectionString);
        }

        private SqlConnection GetActualSqlConnection()
        {
            return GetActualSqlConnection(InstanceConnectionString);
        }

        private SqlTransaction GetActualSqlTransaction()
        {
            return (InternalTransaction ?? ExternalTransaction) as SqlTransaction;
        }

        #region site

        public bool IsLive(int siteId)
        {
            var site = GetSite(siteId);
            return site?.IsLive ?? true;
        }

        public bool ForceLive(int siteId)
        {
            var site = GetSite(siteId);
            return site?.AssembleFormatsInLive ?? false;
        }

        public bool CheckIsLive()
        {
            return AppSettings["isLive"] != "false";
        }

        public bool GetAllowUserSessions(int siteId)
        {
            var site = GetSite(siteId);
            return site?.AllowUserSessions ?? true;
        }

        public bool GetEnableOnScreen(int siteId)
        {
            var site = GetSite(siteId);
            return site?.EnableOnScreen ?? false;
        }

        #endregion

        #region article

        public void CopyArticleSchedule(int fromArticleId, int toArticleId)
        {
            var testcmd =
                new SqlCommand(
                    "select count(*) From information_schema.columns where column_name = 'use_service' and table_name = 'content_item_schedule'")
                {
                    CommandType = CommandType.Text
                };
            var colCount = (int)GetRealScalarData(testcmd);
            var serviceString = colCount == 0 ? "" : ", USE_SERVICE";

            using (var cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.Text;
                var sb = new StringBuilder();
                sb.AppendLine("update content_item_schedule set delete_job = 1 where content_item_id = @newId");
                sb.AppendLine("delete from content_item_schedule where content_item_id = @newId");
                sb.AppendFormatLine("insert into content_item_schedule (CONTENT_ITEM_ID, MAXIMUM_OCCURENCES, CREATED, MODIFIED, LAST_MODIFIED_BY, freq_type, freq_interval, freq_subday_type, freq_subday_interval, freq_relative_interval, freq_recurrence_factor, active_start_date, active_end_date, active_start_time, active_end_time, occurences, use_duration, duration, duration_units, DEACTIVATE, DELETE_JOB{0})", serviceString);
                sb.AppendFormatLine("select @newId, MAXIMUM_OCCURENCES, GETDATE(), GETDATE(), LAST_MODIFIED_BY, freq_type, freq_interval, freq_subday_type, freq_subday_interval, freq_relative_interval, freq_recurrence_factor, active_start_date, active_end_date, active_start_time, active_end_time, occurences, use_duration, duration, duration_units, DEACTIVATE, DELETE_JOB{0}", serviceString);
                sb.AppendLine("from content_item_schedule where content_item_id = @oldId");
                cmd.CommandText = sb.ToString();
                cmd.Parameters.AddWithValue("@oldId", fromArticleId);
                cmd.Parameters.AddWithValue("@newId", toArticleId);
                ProcessData(cmd);
            }
        }

        public bool IsSplitted(int articleId)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "select splitted from content_item where content_item_id = @id";
                cmd.Parameters.AddWithValue("@id", articleId);
                return CastDbNull.To(GetRealScalarData(cmd), false);
            }
        }

        public void MergeArticle(int articleId)
        {
            using (var cmd = new SqlCommand("qp_merge_article"))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@item_id", articleId);
                ProcessData(cmd);
            }
        }

        public bool IsTargetTableAsync(int id)
        {

            var cmd = new SqlCommand("qp_is_target_table_async") {CommandType = CommandType.StoredProcedure};
            var idParam = cmd.Parameters.Add("@content_item_id", SqlDbType.Decimal);
            idParam.Value = id;
            var returnParam = cmd.Parameters.Add("@is_target_table_async", SqlDbType.Bit);
            returnParam.Direction = ParameterDirection.Output;
            ProcessData(cmd);
            return (bool)returnParam.Value;
        }

        #endregion

        #endregion

        #region Statuses

        public int GetMaximumWeightStatusTypeId(int siteId)
        {
            return ((StatusType)GetStatusHashTable()[siteId]).Id;
        }

        public string GetMaximumWeightStatusTypeName(int siteId)
        {
            return ((StatusType)GetStatusHashTable()[siteId]).Name;
        }

        public DataRowView GetMaximumWeightStatusRow(int siteId)
        {
            var dv = GetStatuses("SITE_ID = " + siteId.ToString());
            dv.Sort = "WEIGHT DESC";
            return dv[0];
        }

        public int GetStatusTypeId(int siteId, string statusName)
        {
            var filter = $"SITE_ID = {siteId} AND STATUS_TYPE_NAME='{statusName}'";
            var dv = GetStatuses(filter);
            if (dv.Count > 0)
            {
                return GetNumInt(dv[0]["STATUS_TYPE_ID"]);
            }
            else
            {
                return LegacyNotFound;
            }
        }

        public DataRow GetPreviousStatusHistoryRecord(int id)
        {
            return Status.GetPreviousStatusHistoryRecord(id, this);
        }

        #endregion

        #region Full-text search

        public DataTable GetSearchResults(string expression, bool useMorphology, int startPos, long recordCount, string tabname, int minRank, ref long totalRecords)
        {

            const string searchSp = "qp_fulltextSiteSearch";
            var dt = new DataTable();
            var ds = new DataSet();
            const string strSql = "SELECT count(*) from sysobjects where name = '" + searchSp + "'";

            var useMorphologyInt = useMorphology ? 1 : 0;

            if (GetCachedData(strSql).Rows.Count > 0)
            {
                var adapter = new SqlDataAdapter();
                var connection = GetActualSqlConnection();
                try
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    adapter.SelectCommand = new SqlCommand(searchSp, connection)
                    {
                        Transaction = GetActualSqlTransaction(),
                        CommandType = CommandType.StoredProcedure
                    };
                    adapter.SelectCommand.Parameters.Add("@tabname", SqlDbType.NVarChar, 255).Value = tabname;
                    adapter.SelectCommand.Parameters.Add("@use_morphology", SqlDbType.Int, 4).Value = useMorphologyInt;
                    adapter.SelectCommand.Parameters.Add("@expression", SqlDbType.NVarChar, 1000).Value = Strings.LCase(expression);
                    adapter.SelectCommand.Parameters.Add("@minrank", SqlDbType.Int, 4).Value = minRank;
                    adapter.SelectCommand.Parameters.Add("@startpos", SqlDbType.Int, 4).Value = startPos;
                    adapter.SelectCommand.Parameters.Add("@count", SqlDbType.Int, 100).Value = recordCount;
                    adapter.AcceptChangesDuringFill = false;
                    adapter.Fill(ds);
                }
                finally
                {
                    if (NeedToDisposeActualSqlConnection)
                        connection.Dispose();
                }

                if (ds.Tables[0].Rows.Count > 0)
                {
                    dt = ds.Tables[1];
                    totalRecords = (long)ds.Tables[0].Rows[0]["total"];
                }


                adapter.AcceptChangesDuringFill = true;
            }


            return dt;
        }

        public DataTable GetSearchResults(string expression, bool useMorphology, int startPos, long recordCount, string tabname, int minRank, DateTime startDate, DateTime endDate, int showStaticContent, ref long totalRecords
        )
        {

            const string searchSp = "qp_fulltextSiteSearchWithDate";
            var dt = new DataTable();
            var ds = new DataSet();
            const string strSql = "SELECT count(*) from sysobjects where name = '" + searchSp + "'";

            var useMorphologyInt = useMorphology ? 1 : 0;

            if (GetCachedData(strSql).Rows.Count > 0)
            {
                var adapter = new SqlDataAdapter();
                var connection = GetActualSqlConnection();
                try
                {
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    adapter.SelectCommand = new SqlCommand(searchSp, connection)
                    {
                        Transaction = GetActualSqlTransaction(),
                        CommandType = CommandType.StoredProcedure
                    };
                    adapter.SelectCommand.Parameters.Add("@tabname", SqlDbType.NVarChar, 255).Value = tabname;
                    adapter.SelectCommand.Parameters.Add("@use_morphology", SqlDbType.Int, 4).Value = useMorphologyInt;
                    adapter.SelectCommand.Parameters.Add("@expression", SqlDbType.NVarChar, 1000).Value = Strings.LCase(expression);
                    adapter.SelectCommand.Parameters.Add("@minrank", SqlDbType.Int, 4).Value = minRank;
                    adapter.SelectCommand.Parameters.Add("@startpos", SqlDbType.Int, 4).Value = startPos;
                    adapter.SelectCommand.Parameters.Add("@count", SqlDbType.Int, 100).Value = recordCount;
                    adapter.SelectCommand.Parameters.Add("@startDate", SqlDbType.DateTime).Value = startDate;
                    adapter.SelectCommand.Parameters.Add("@endDate", SqlDbType.DateTime).Value = endDate;
                    adapter.SelectCommand.Parameters.Add("@showStatic", SqlDbType.Bit).Value = showStaticContent;

                    adapter.AcceptChangesDuringFill = false;
                    adapter.Fill(ds);
                }
                finally
                {
                    if (NeedToDisposeActualSqlConnection)
                        connection.Dispose();
                }

                if (ds.Tables[0].Rows.Count > 0)
                {
                    dt = ds.Tables[1];
                    totalRecords = (long)ds.Tables[0].Rows[0]["total"];
                }


                adapter.AcceptChangesDuringFill = true;
            }


            return dt;
        }

        #endregion

        #region Publishing Container

        public string CorrectStatuses(int srcSiteId, int destSiteId, string @where)
        {
            var result = @where;

            if (destSiteId != 0 && srcSiteId != destSiteId)
            {
                result = result.ToUpperInvariant();
                var simpleExpression = "AND C.STATUS_TYPE_ID = ";
                var complexExpressionBegin = "AND C.STATUS_TYPE_ID IN (";
                var complexExpressionEnd = ")";
                var divider = ",";
                var statusString = "";

                var simpleRegex = new Regex(Regex.Escape(simpleExpression) + "([\\d]+)");
                var complexRegex = new Regex(Regex.Escape(complexExpressionBegin) + "([^\\)]+)" + Regex.Escape(complexExpressionEnd));

                if (result.IndexOf(simpleExpression, StringComparison.Ordinal) >= 0)
                {
                    var simpleMatch = simpleRegex.Match(result);
                    if (simpleMatch.Success)
                    {
                        statusString = simpleMatch.Groups[1].Value;
                        result = result.Replace(simpleExpression + statusString, complexExpressionBegin + statusString + complexExpressionEnd);
                    }
                }
                else
                {
                    var complexMatch = complexRegex.Match(result);
                    if (complexMatch.Success)
                    {
                        statusString = complexMatch.Groups[1].Value;
                    }
                }

                if (!string.IsNullOrEmpty(statusString))
                {
                    var statuses = new ArrayList();
                    statuses.AddRange(statusString.Split(','));
                    foreach (var status in statusString.Split(','))
                    {
                        var statusesView = GetStatuses($"[STATUS_TYPE_ID] = {status}");
                        var statusName = statusesView[0]["STATUS_TYPE_NAME"].ToString();
                        var statusesView2 = GetStatuses(
                            $"[STATUS_TYPE_ID] <> {status} AND [STATUS_TYPE_NAME] = '{statusName}'");
                        foreach (DataRowView row in statusesView2)
                        {
                            statuses.Add(row["STATUS_TYPE_ID"].ToString());
                        }
                    }


                    if (statuses.Count > 0)
                    {
                        var newStatusString = string.Join(",", (string[])statuses.ToArray());
                        result = result.Replace(complexExpressionBegin + statusString + complexExpressionEnd, complexExpressionBegin + newStatusString + complexExpressionEnd);
                    }
                }
            }


            return result;
        }

        public DataTable GetContainerQueryResultTable(ContainerQueryObject obj, out long totalRecords)
        {
            obj.WithReset = false;
            var result = CacheManager.GetQueryResult(obj, out totalRecords);
            if (!result.Columns.Contains("content_item_id"))
            {
                obj.WithReset = true;
                result = CacheManager.GetQueryResult(obj, out totalRecords);
            }
            return new DataView(result).ToTable();
        }


        public DataTable GetPageData(string select, string from, string where, string orderBy, long startRow, long pageSize, byte getCount, out long totalRecords)
        {
            return GetContainerQueryResultTable(new ContainerQueryObject(this, select, from, where, orderBy, startRow.ToString(), pageSize.ToString()), out totalRecords);
        }

        #endregion

        #region LINQ-to-SQL classes support

        public string GetBinDirectory(int siteId, bool isLive)
        {
            var site = GetSite(siteId);
            if (site == null) return String.Empty;
            return isLive ? site.AssemblyDirectory : site.StageAssemblyDirectory;
        }

        public string GetAppDataDirectory(int siteId, bool isLive)
        {
            return GetBinDirectory(siteId, isLive).Replace("bin", "App_Data");
        }

        public string GetDefaultMapFileContents(int siteId, bool isLive)
        {
            var site = GetSite(siteId);
            return GetMapFileContents(siteId, isLive, $"{site.ContextClassName}.map");
        }

        public string GetMapFileContents(int siteId, bool isLive, string fileName)
        {
            return GetCachedFileContents(Path.Combine(GetAppDataDirectory(siteId, isLive), fileName));
        }

        public string GetDefaultMapFileContents(int siteId)
        {
            return GetDefaultMapFileContents(siteId, !IsStage);
        }

        public string GetMapFileContents(int siteId, string fileName)
        {
            return GetMapFileContents(siteId, !IsStage, fileName);
        }

        #endregion

    }

}
