using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Quantumart.QP8.DAL.CdcImport
{
    [SuppressMessage("ReSharper", "RedundantVerbatimPrefix")]
    public static class CdcImportDal
    {
        private const string @LastExecutedLsn = "@lastExecutedLsn";
        private const string @LastPushedLsn = "@lastPushedLsn";
        private const string @ProviderName = "@providerName";
        private const string @ProviderUrl = "@providerUrl";

        public static string GetLastExecutedLsn(DbConnection connection, string providerUrl)
        {
            using (var cmd = DbCommandFactory.Create($"SELECT TOP(1) LastExecutedLsn FROM [dbo].[CdcLastExecutedLsn] WHERE ProviderUrl = {@ProviderUrl};", connection))
            {
                cmd.Parameters.AddWithValue(@ProviderUrl, providerUrl);
                return (string)cmd.ExecuteScalar();
            }
        }

        public static string GetMaxLsn(DbConnection connection)
        {
            using (var cmd = DbCommandFactory.Create("SELECT CONVERT(varchar(22), sys.fn_cdc_get_max_lsn(), 1);", connection))
            {
                return (string)cmd.ExecuteScalar();
            }
        }

        public static int PostLastExecutedLsn(DbConnection connection, string providerName, string providerUrl, string lastPushedLsn, string lastExecutedLsn)
        {
            var query = $@"
IF EXISTS (SELECT * FROM [dbo].[CdcLastExecutedLsn] WHERE providerUrl = {@ProviderUrl})
    UPDATE [dbo].[CdcLastExecutedLsn]
    SET
        ProviderName = {@ProviderName},
        TransactionLsn = COALESCE({@LastPushedLsn}, TransactionLsn),
        TransactionDate = sys.fn_cdc_map_lsn_to_time(CONVERT(binary, COALESCE({@LastPushedLsn}, TransactionLsn), 1)),
        LastExecutedLsn = {@LastExecutedLsn}
    OUTPUT inserted.id
    WHERE providerUrl = {@ProviderUrl}
ELSE
    INSERT INTO [dbo].[CdcLastExecutedLsn] (
        [ProviderName],
        [ProviderUrl],
        [TransactionLsn],
        [TransactionDate],
        [LastExecutedLsn]
    )
    OUTPUT inserted.id
    VALUES (
        {@ProviderName},
        {@ProviderUrl},
        {@LastPushedLsn},
        sys.fn_cdc_map_lsn_to_time(CONVERT(binary, {@LastPushedLsn}, 1)),
        {@LastExecutedLsn}
    )
";

            using (var cmd = DbCommandFactory.Create(query, connection))
            {
                cmd.Parameters.AddWithValue(@ProviderName, providerName);
                cmd.Parameters.AddWithValue(@ProviderUrl, providerUrl);
                cmd.Parameters.AddWithValue(@LastPushedLsn, (object)lastPushedLsn ?? DBNull.Value);
                cmd.Parameters.AddWithValue(@LastExecutedLsn, lastExecutedLsn);
                return (int)cmd.ExecuteScalar();
            }
        }
    }
}
