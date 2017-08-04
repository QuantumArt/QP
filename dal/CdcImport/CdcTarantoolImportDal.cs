using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;

namespace Quantumart.QP8.DAL.CdcImport
{
    [SuppressMessage("ReSharper", "RedundantVerbatimPrefix")]
    public static class CdcTarantoolImportDal
    {
        private const string @CaptureInstance = "@captureInstance";
        private const string @RawLsnFrom = "@rawLsnFrom";
        private const string @RawLsnTo = "@rawLsnTo";

        public static DataTable GetCdcTableData(SqlConnection connection, string captureInstance, string fromLsn = null, string toLsn = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine(BuildQueryHeader(fromLsn, toLsn));
            sb.AppendLine(BuildQueryBody(captureInstance));

            using (var cmd = SqlCommandFactory.Create(sb.ToString(), connection))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue(@CaptureInstance, captureInstance);
                cmd.Parameters.AddWithValue(@RawLsnFrom, (object)fromLsn ?? DBNull.Value);
                cmd.Parameters.AddWithValue(@RawLsnTo, (object)toLsn ?? DBNull.Value);

                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private static string BuildQueryBody(string captureInstance)
        {
            var filterClause = string.Empty;
            switch (captureInstance)
            {
                case CdcCaptureConstants.ContentAttribute:
                    filterClause = @"
LEFT JOIN (
	SELECT
		attrt.ATTRIBUTE_TYPE_ID AS attr_type_id,
		attrt.TYPE_NAME AS attr_type_name,
		attrt.DATABASE_TYPE AS attr_type_db
	FROM dbo.ATTRIBUTE_TYPE attrt
) result ON result.attr_type_id = cdc.ATTRIBUTE_TYPE_ID";
                    break;

                case CdcCaptureConstants.ContentItem:
                    filterClause = @"
WHERE __$operation = 2 OR not_for_replication = 0";
                    break;

                case CdcCaptureConstants.ContentData:
                    filterClause = $@"
INNER JOIN 
(
	SELECT
		ca.ATTRIBUTE_ID,
		ca.ATTRIBUTE_TYPE_ID
	FROM dbo.CONTENT_ATTRIBUTE ca
) ca ON ca.ATTRIBUTE_ID = cdc.ATTRIBUTE_ID
INNER JOIN 
(
	SELECT
		attrt.ATTRIBUTE_TYPE_ID AS attr_type_id,
		attrt.TYPE_NAME AS attr_type_name,
		attrt.DATABASE_TYPE AS attr_type_db
	FROM dbo.ATTRIBUTE_TYPE attrt
) result ON result.attr_type_id = ca.ATTRIBUTE_TYPE_ID
WHERE __$operation = {(int)CdcOperationType.Insert} OR not_for_replication = 0";
                    break;
            }

            return $@"
SELECT
    CASE __$operation
        WHEN {(int)CdcOperationType.Delete} THEN '{CdcOperationType.Delete}'
        WHEN {(int)CdcOperationType.Insert} THEN '{CdcOperationType.Insert}'
        WHEN {(int)CdcOperationType.PreUpdate} THEN '{CdcOperationType.PreUpdate}'
        WHEN {(int)CdcOperationType.Update} THEN '{CdcOperationType.Update}'
    END AS operation,
    CONVERT(varchar(22), @from_lsn, 1) AS fromLsn,
    CONVERT(varchar(22), @to_lsn, 1) AS toLsn,
    sys.fn_cdc_map_lsn_to_time(__$start_lsn) AS transactionDate,
    CONVERT(varchar(22), __$start_lsn, 1) AS transactionLsn,
    CONVERT(varchar(22), __$seqval, 1) AS sequenceLsn,
    *
FROM cdc.fn_cdc_get_all_changes_{captureInstance}(@from_lsn, @to_lsn, N'all') cdc
{filterClause}
ORDER BY __$seqval";
        }

        private static string BuildQueryHeader(string fromLsn = null, string toLsn = null)
        {
            var result = new StringBuilder();
            var validateInputDataQuery = $"IF {@RawLsnFrom} = {@RawLsnTo} RETURN;";
            const string declareLsnQuery = "DECLARE @from_lsn binary(10), @to_lsn binary(10);";
            var fromLsnQuery = string.IsNullOrWhiteSpace(fromLsn)
                ? $"SET @from_lsn = sys.fn_cdc_get_min_lsn({@CaptureInstance});"
                : $@"
DECLARE @saved_lsn binary(10), @min_lsn binary(10);
SET @saved_lsn = sys.fn_cdc_increment_lsn(CONVERT(binary, {@RawLsnFrom}, 1));
SET @min_lsn = sys.fn_cdc_get_min_lsn({@CaptureInstance});
IF @saved_lsn > @min_lsn SET @from_lsn = @saved_lsn ELSE SET @from_lsn = @min_lsn;
";

            var toLsnQuery = string.IsNullOrWhiteSpace(toLsn)
                ? "SET @to_lsn = sys.fn_cdc_get_max_lsn();"
                : $"SET @to_lsn = CONVERT(binary(10), {@RawLsnTo}, 1);";

            result.AppendLine(validateInputDataQuery);
            result.AppendLine(declareLsnQuery);
            result.AppendLine(fromLsnQuery);
            result.AppendLine(toLsnQuery);

            return result.ToString();
        }
    }
}
