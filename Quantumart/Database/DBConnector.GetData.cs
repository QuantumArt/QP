using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Web.Caching;
using Quantumart.QPublishing.Info;

namespace Quantumart.QPublishing.Database
{
    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {
        #region GetData, GetContentData, ProcessData

        #region GetDataViaDataSet

        public DataTable GetDataViaDataSet(string queryString)
        {
            var dataset = new DataSet();
            var arr = queryString.Split(';');

            var connection = GetActualSqlConnection();
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                var adapter = new SqlDataAdapter { AcceptChangesDuringFill = false };
                dataset.EnforceConstraints = false;
                adapter.SelectCommand = new SqlCommand(queryString, connection)
                {
                    Transaction = GetActualSqlTransaction()
                };

                int i;
                for (i = arr.GetLowerBound(0); i <= arr.GetUpperBound(0); i++)
                {
                    dataset.Tables.Add("QTable" + i);
                }

                int j;
                for (j = 0; j <= i - 1; j++)
                {
                    dataset.Tables[j].BeginLoadData();
                    adapter.Fill(dataset.Tables[j]);
                    dataset.Tables[j].EndLoadData();
                    dataset.Tables[j].AcceptChanges();
                }

                dataset.EnforceConstraints = true;
                adapter.AcceptChangesDuringFill = true;
                return dataset.Tables[0];
            }
            finally
            {
                if (NeedToDisposeActualSqlConnection)
                {
                    connection.Dispose();
                }
            }
        }

        #endregion

        #region GetRealData

        public DataTable GetRealData(string queryString)
        {
            var cmd = new SqlCommand(queryString);
            return GetRealData(cmd);
        }

        public DataTable GetRealData(SqlCommand cmd)
        {
            return GetRealData(cmd, GetActualSqlConnection(), GetActualSqlTransaction(), NeedToDisposeActualSqlConnection);
        }

        public DataTable GetRealData(SqlCommand cmd, SqlConnection cnn, SqlTransaction tr, bool disposeConnection)
        {
            try
            {
                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }

                cmd.Connection = cnn;
                cmd.Transaction = tr;
                var adapter = new SqlDataAdapter
                {
                    SelectCommand = cmd
                };

                return GetFilledDataTable(adapter);
            }
            finally
            {
                if (disposeConnection)
                {
                    cnn.Dispose();
                }
            }
        }

        public DataTable GetRealDataWithDependency(string queryString, ref SqlCacheDependency dep)
        {
            var connection = GetActualSqlConnection();
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                var adapter = new SqlDataAdapter();
                var cmd = new SqlCommand(queryString, connection) { Transaction = GetActualSqlTransaction() };
                dep = new SqlCacheDependency(cmd);
                adapter.SelectCommand = cmd;
                return GetFilledDataTable(adapter);
            }
            finally
            {
                if (NeedToDisposeActualSqlConnection)
                {
                    connection.Dispose();
                }
            }
        }

        public object GetRealScalarData(SqlCommand command)
        {
            var connection = GetActualSqlConnection();
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                command.Connection = connection;
                command.Transaction = GetActualSqlTransaction();
                return command.ExecuteScalar();
            }
            finally
            {
                if (NeedToDisposeActualSqlConnection)
                {
                    connection.Dispose();
                }
            }
        }

        #endregion

        #region GetData

        public DataTable GetData(string queryString)
        {
            return GetData(queryString, 0, true);
        }

        public DataTable GetData(string queryString, double cacheInterval)
        {
            return GetData(queryString, cacheInterval, false);
        }

        private DataTable GetData(string queryString, double cacheInterval, bool useDefaultInterval)
        {
            return AppSettings["CacheGetData"] == "1" ? GetCachedData(queryString, cacheInterval, useDefaultInterval, false) : GetRealData(queryString);
        }

        #endregion

        #region GetCachedData

        public DataTable GetCachedData(string queryString, double cacheInterval)
        {
            return GetCachedData(queryString, cacheInterval, false, false);
        }

        public DataTable GetCachedData(string queryString)
        {
            return GetCachedData(queryString, 0, true, false);
        }

        public DataTable GetCachedData(string queryString, bool useDependency)
        {
            return GetCachedData(queryString, 0, false, true);
        }

        private DataTable GetCachedData(string queryString, double cacheInterval, bool useDefaultInterval, bool useDependency)
        {
            if (useDependency)
            {
                return CacheManager.GetCachedTable(CacheManager.GetDataKeyPrefix + queryString, 0, true);
            }

            if (useDefaultInterval)
            {
                return CacheManager.GetCachedTable(CacheManager.GetDataKeyPrefix + queryString);
            }

            return CacheManager.GetCachedTable(CacheManager.GetDataKeyPrefix + queryString, cacheInterval, false);
        }

        #endregion

        #region GetContentDataWithSecurity

        public string GetSecuritySql(int contentId, long userId, long groupId, long startLevel, long endLevel)
        {
            string result;
            var cmd = new SqlCommand
            {
                CommandType = CommandType.StoredProcedure,
                CommandText = "qp_GetPermittedItemsAsQuery"
            };
            cmd.Parameters.Add(new SqlParameter("@user_id", SqlDbType.Decimal) { Value = userId });
            cmd.Parameters.Add(new SqlParameter("@group_id", SqlDbType.Decimal) { Value = groupId });
            cmd.Parameters.Add(new SqlParameter("@start_level", SqlDbType.Int) { Value = startLevel });
            cmd.Parameters.Add(new SqlParameter("@end_level", SqlDbType.Int) { Value = endLevel });
            cmd.Parameters.Add(new SqlParameter("@entity_name", SqlDbType.VarChar, 100) { Value = "content_item" });
            cmd.Parameters.Add(new SqlParameter("@parent_entity_name", SqlDbType.VarChar, 100) { Value = "content" });
            cmd.Parameters.Add(new SqlParameter("@parent_entity_id", SqlDbType.Decimal) { Value = contentId });
            cmd.Parameters.Add(new SqlParameter("@SQLOut", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output });

            var connection = GetActualSqlConnection();
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                result = (string)cmd.Parameters["@SQLOut"].Value;
            }
            finally
            {
                if (NeedToDisposeActualSqlConnection)
                {
                    connection.Dispose();
                }
            }
            return result;
        }

        // ReSharper disable once RedundantAssignment
        public DataTable GetContentDataWithSecurity(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle,
        byte includeArchive, long lngUserId, long lngGroupId, int intStartLevel, int intEndLevel, bool blnFilterRecords)
        {

            var obj = new ContentDataQueryObject(this, siteName, contentName, String.Empty, whereExpression,
                orderExpression, startRow, pageSize, useSchedule, statusName, showSplittedArticle, includeArchive, false,
                0, false, false)
            {
                UseSecurity = true,
                UserId = lngUserId,
                GroupId = lngGroupId,
                StartLevel = intStartLevel,
                EndLevel = intEndLevel,
                FilterRecords = blnFilterRecords
            };
            var result = CacheManager.GetQueryResult(obj, out totalRecords);
            var dv = new DataView(result);
            return dv.ToTable();
        }

        #endregion

        #region GetFilledDataTable

        internal QueryResult GetFilledDataTable(IQueryObject obj)
        {
            var adapter = new SqlDataAdapter();
            var cmd = obj.GetSqlCommand();
            adapter.SelectCommand = cmd;
            var cnn = GetActualSqlConnection(obj.Cnn.InstanceConnectionString);
            try
            {
                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }
                adapter.SelectCommand.Connection = cnn;
                adapter.SelectCommand.Transaction = GetActualSqlTransaction();
                var result = GetFilledDataTable(adapter);
                long totalRecords;
                if (obj.GetCount)
                {
                    if (obj.GetCount && obj.GetCountInTable)
                    {
                        totalRecords = result.Rows.Count > 0 ? (int)result.Rows[0]["ROWS_COUNT"] : 0;
                        if (!obj.IsFirstPage && totalRecords == 0)
                        {
                            var countCmd = new SqlCommand(obj.CountSql);
                            var sqlParams = new SqlParameter[cmd.Parameters.Count];
                            cmd.Parameters.CopyTo(sqlParams, 0);
                            cmd.Parameters.Clear();
                            countCmd.Parameters.AddRange(sqlParams);
                            totalRecords = (long)GetRealScalarData(countCmd);
                        }

                    }
                    else
                    {
                        totalRecords = (int)adapter.SelectCommand.Parameters[obj.OutputParamName].Value;
                    }
                }
                else
                {
                    totalRecords = result.Rows.Count;
                }
                return new QueryResult { DataTable = result, TotalRecords = totalRecords };
            }
            finally
            {
                if (NeedToDisposeActualSqlConnection)
                {
                    cnn.Dispose();
                }
            }

        }

        internal DataTable GetFilledDataTable(SqlDataAdapter adapter)
        {
            var dt = new DataTable
            {
                CaseSensitive = false,
                Locale = CultureInfo.InvariantCulture
            };
            adapter.Fill(dt);
            return dt;
        }

        #endregion

        #region GetContentData

        public DataTable GetContentData(string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName,
        byte showSplittedArticle, byte includeArchive, bool cacheResult, double cacheInterval, bool useClientSelection, bool withReset)
        {
            var obj = new ContentDataQueryObject(this, siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, useSchedule, statusName, showSplittedArticle, includeArchive, cacheResult, cacheInterval, useClientSelection, withReset);
            return GetContentData(obj, ref totalRecords);
        }

        public DataTable GetContentData(ContentDataQueryObject obj)
        {
            long total = 0;
            obj.GetCount = false;
            return GetContentData(obj, ref total);
        }

        public SqlDataReader GetContentDataReader(ContentDataQueryObject obj, CommandBehavior readerParams = CommandBehavior.Default)
        {
            if (ExternalConnection == null)
            {
                throw new ApplicationException("ExternalConnection for DbConnector instance has not been defined");
            }
            {
                obj.GetCount = false;
                obj.UseClientSelection = false;
                var cmd = obj.GetSqlCommand();
                cmd.Connection = ExternalConnection as SqlConnection;
                cmd.Transaction = GetActualSqlTransaction();
                if (cmd.Connection != null && cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }
                return cmd.ExecuteReader(readerParams);
            }
        }

        // ReSharper disable once RedundantAssignment
        public DataTable GetContentData(ContentDataQueryObject obj, ref long totalRecords)
        {
            var result = CacheManager.GetQueryResult(obj, out totalRecords);

            if ((!CacheData || !obj.CacheResult) && !obj.UseClientSelection)
            {
                return result;
            }

            var dv = new DataView(result);

            if (obj.UseClientSelection)
            {
                var hasRegionId = dv.Table.Columns.Contains("RegionId");
                if (!string.IsNullOrEmpty(obj.Where))
                {
                    dv.RowFilter = obj.Where;
                }
                totalRecords = dv.Count;
                if (!string.IsNullOrEmpty(obj.OrderBy))
                {
                    dv.Sort = obj.OrderBy;
                }
                if (obj.StartRow < 1)
                {
                    obj.StartRow = 1;
                }
                if (obj.PageSize < 0)
                {
                    obj.PageSize = 0;
                }
                if (obj.StartRow > 1 || obj.PageSize > 0)
                {
                    if (dv.Count > 0)
                    {
                        if ((int)obj.StartRow <= dv.Count)
                        {
                            int endRow;
                            if (obj.PageSize == 0)
                            {
                                endRow = dv.Count;
                            }
                            else if (obj.StartRow + obj.PageSize > dv.Count)
                            {
                                endRow = dv.Count;
                            }
                            else
                            {
                                endRow = (int)obj.StartRow + (int)obj.PageSize - 1;
                            }

                            var ids = new string[endRow - (int)obj.StartRow + 1];
                            int i;
                            var j = 0;
                            for (i = (int)obj.StartRow - 1; i <= endRow - 1; i++)
                            {
                                if (hasRegionId)
                                {
                                    ids[j] =
                                        $"(CONTENT_ITEM_ID = {dv[i]["CONTENT_ITEM_ID"]} and RegionId = {dv[i]["RegionId"]})";
                                }
                                else
                                {
                                    ids[j] = $"CONTENT_ITEM_ID = {dv[i]["CONTENT_ITEM_ID"]}";
                                }
                                j = j + 1;
                            }
                            dv.RowFilter = string.Join(" or ", ids);
                        }
                        else
                        {
                            dv.RowFilter = "CONTENT_ITEM_ID = 0";
                        }
                    }
                }
            }

            return dv.ToTable();
        }

        public DataTable GetContentData(string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName,
        byte showSplittedArticle, byte includeArchive, bool cacheResult, double cacheInterval)
        {
            return GetContentData(siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, cacheResult, cacheInterval, false, false);
        }

        //' All fields
        public DataTable GetContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle,
        byte includeArchive)
        {
            return GetContentData(siteName, contentName, "", whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive);
        }

        //' Specific fields
        public DataTable GetContentData(string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName,
        byte showSplittedArticle, byte includeArchive)
        {
            return GetContentData(siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, false, 0);
        }

        #endregion

        #region GetCachedContentData

        public DataTable GetCachedContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle,
        byte includeArchive)
        {
            return GetCachedContentData(siteName, contentName, "", whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive);
        }

        public DataTable GetCachedContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle,
        byte includeArchive, bool useClientSelection)
        {
            return GetContentData(siteName, contentName, "", whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, true, DbCacheManager.DefaultExpirationTime, useClientSelection, false);
        }

        public DataTable GetCachedContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle,
        byte includeArchive, double cacheInterval)
        {
            return GetContentData(siteName, contentName, "", whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, true, cacheInterval);
        }

        public DataTable GetCachedContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle,
        byte includeArchive, double cacheInterval, bool useClientSelection)
        {
            return GetContentData(siteName, contentName, "", whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, true, cacheInterval, useClientSelection, false);
        }

        public DataTable GetCachedContentData(string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName,
        byte showSplittedArticle, byte includeArchive)
        {
            return GetContentData(siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, true, DbCacheManager.DefaultExpirationTime);
        }

        public DataTable GetCachedContentData(string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName,
        byte showSplittedArticle, byte includeArchive, bool useClientSelection)
        {
            return GetContentData(siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, true, DbCacheManager.DefaultExpirationTime, useClientSelection, false);
        }

        public DataTable GetCachedContentData(string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName,
        byte showSplittedArticle, byte includeArchive, double cacheInterval)
        {
            return GetContentData(siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, true, cacheInterval);
        }

        public DataTable GetCachedContentData(string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName,
        byte showSplittedArticle, byte includeArchive, double cacheInterval, bool useClientSelection)
        {
            return GetContentData(siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, true, cacheInterval, useClientSelection, false);
        }


        public DataTable GetCachedContentData(string siteName, string contentName, string fields, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName,
        byte showSplittedArticle, byte includeArchive, bool useClientSelection, bool withReset)
        {
            return GetContentData(siteName, contentName, fields, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName,
            showSplittedArticle, includeArchive, true, DbCacheManager.DefaultExpirationTime, useClientSelection, withReset);
        }

        #endregion

        #region ProcessData

        public void ProcessData(string queryString)
        {
            var command = new SqlCommand(queryString);
            ProcessData(command);
        }

        public void ProcessData(SqlCommand command)
        {
            ProcessData(command, GetActualSqlConnection(), GetActualSqlTransaction(), NeedToDisposeActualSqlConnection);
        }

        public void ProcessData(SqlCommand command, SqlConnection cnn, SqlTransaction tr, bool disposeConnection)
        {
            try
            {
                if (cnn.State == ConnectionState.Closed)
                {
                    cnn.Open();
                }

                command.Connection = cnn;
                command.Transaction = tr;
                command.ExecuteNonQuery();
            }
            finally
            {
                if (disposeConnection)
                {
                    cnn.Dispose();
                }
            }
        }

        private void ProcessDataAsNewTransaction(SqlCommand command)
        {
            const int maxRetries = 5;
            var retry = maxRetries;
            while (retry > 0)
            {
                var connection = GetActualSqlConnection();
                var extTran = GetActualSqlTransaction();
                try
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    try
                    {
                        command.Connection = connection;
                        command.Transaction = extTran ?? connection.BeginTransaction();
                        command.ExecuteNonQuery();
                        if (extTran == null)
                        {
                            command.Transaction.Commit();
                        }
                        retry = -1;
                    }
                    catch (SqlException e)
                    {
                        try
                        {
                            if (extTran == null)
                            {
                                command.Transaction.Rollback();
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMessage = $"DbConnector.cs, ProcessDataAsNewTransaction(SqlCommand command), MESSAGE: {ex.Message} STACK TRACE: {ex.StackTrace}";
                            EventLog.WriteEntry("Application", errorMessage);
                        }

                        //error with rollback
                        //assume rollback already happened automatically
                        //do nothing
                        if (e.Number == 1205)
                        {
                            //deadlock
                            retry = retry - 1;
                        }
                        else
                        {
                            //try just one more time (maybe a timeout)
                            if (retry == maxRetries)
                            {
                                retry = 1;
                            }
                            else
                            {
                                throw;
                            }
                        }

                        if (retry == 0)
                        {
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"DbConnector.cs, ProcessDataAsNewTransaction(SqlCommand command), MESSAGE: {ex.Message} STACK TRACE: {ex.StackTrace}";
                        EventLog.WriteEntry("Application", errorMessage);
                    }
                }
                finally
                {
                    if (NeedToDisposeActualSqlConnection)
                    {
                        connection.Dispose();
                    }
                }
            }
        }
        #endregion
        #endregion
    }
}
