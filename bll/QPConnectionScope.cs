using System;
using System.Collections.Generic;
using System.Data.Common;

// using System.Data.Entity.Core.EntityClient;
// using System.Data.Entity.Core.Mapping;
// using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using AutoMapper;
using Npgsql;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class QPConnectionScope : IDisposable
    {
        private DbConnection _efConnection;

        private int _scopeCount;

        public static QPConnectionScope Current
        {
            get => QPContext.CurrentConnectionScope;
            private set => QPContext.CurrentConnectionScope = value;
        }

        public string ConnectionString { get; }

        public DatabaseType DbType { get; set; }

        public DatabaseType CurrentDbType => Current.DbType;

        static QPConnectionScope()
        {
            if (!IsMapperInitialized())
            {
                Mapper.Initialize(MapperFacade.CreateAllMappings);
            }
        }

        public static bool IsMapperInitialized()
        {
            try
            {
                Mapper.Configuration.AssertConfigurationIsValid();
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (AutoMapperConfigurationException)
            {
                return true;
            }

            return true;
        }

        public QPConnectionScope()
            : this(QPContext.CurrentDbConnectionInfo)
        {
        }

        public QPConnectionScope(string connectionString, DatabaseType dbType = default(DatabaseType))
            : this(new QpConnectionInfo(connectionString, dbType))
        {
        }

        public QPConnectionScope(QpConnectionInfo info)
        {
            if (Current == null)
            {
                Ensure.NotNull(info, "Connection info should not be null");
                Ensure.NotNullOrWhiteSpace(info.ConnectionString, "Connection string should not be null or empty");
                Current = this;
                ConnectionString = info.ConnectionString;
                DbType = info.DbType;
            }
            else if (info != null && !string.IsNullOrWhiteSpace(info.ConnectionString) && !info.ConnectionString.Equals(Current.ConnectionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Attempt to create connection in the existing scope with different connection string.");
            }

            Current._scopeCount++;
        }

        public QPConnectionScope(QpConnectionInfo info, HashSet<string> identityInsertOptions)
            : this(info)
        {
            IdentityInsertOptions = identityInsertOptions;
        }

        public void Dispose()
        {
            if (Current != null)
            {
                // уменьшаем счетчик вложенных scope
                // если это последний, то все очищаем
                Current._scopeCount--;
                if (Current._scopeCount == 0)
                {
                    ForcedDispose();
                }
            }
        }

        public void ForcedDispose()
        {
            if (Current._efConnection != null)
            {
                Current._efConnection.Close();
                Current._efConnection.Dispose();

                Current._efConnection = null;
                Current._scopeCount = 0;
            }

            Current = null;
        }

        public DbConnection DbConnection => EfConnection;

        public DbConnection EfConnection
        {
            get
            {
                Current.CreateEfConnection();
                return Current._efConnection;
            }
        }

        public HashSet<string> IdentityInsertOptions { get; } = new HashSet<string>();

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        [SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "Object will dispose in QPConnectionScope.Dispose")]
        private void CreateEfConnection()
        {
            if (Current._efConnection == null)
            {
                var usePostgres = DbType == DatabaseType.Postgres;
                var dbConnection = usePostgres ? (DbConnection)new NpgsqlConnection(ConnectionString) : new SqlConnection(ConnectionString);

                dbConnection.Open();

                Current._efConnection = dbConnection;
                if (Transaction.Current == null && !usePostgres)
                {
                    using (var cmd = DbCommandFactory.Create("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED", dbConnection as SqlConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
