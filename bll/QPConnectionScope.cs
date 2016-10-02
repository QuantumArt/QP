using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class QPConnectionScope : IDisposable
    {
        [ThreadStatic]
        private static QPConnectionScope _currentScope;

        private string ConnectionString { get; }

        public static QPConnectionScope Current
        {
            get { return _currentScope; }
            private set { _currentScope = value; }
        }

        public static string SetIsolationLevelCommandText => "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";

        private EntityConnection _efConnection;

        /// <summary>
        /// Количество scope вложенных друг в друга
        /// </summary>
        private int _scopeCount;

        public QPConnectionScope(string connectionString)
        {
            // Если scope для потока не существует, то делаем создаваемый текущим для потока
            if (Current == null)
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ArgumentNullException(nameof(connectionString));
                }

                Current = this;
                ConnectionString = connectionString;
            }
            else if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Equals(Current.ConnectionString, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Attempt to create connection in the existing scope with different connection string.");
            }

            // увеличиваем счетчик вложенных scope
            Current._scopeCount++;
        }

        public QPConnectionScope() : this(QPContext.CurrentDbConnectionString) { }

        public QPConnectionScope(string connectionString, HashSet<string> identityInsertOptions) : this(connectionString)
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
                Current._efConnection.StoreConnection.Close();
                Current._efConnection.StoreConnection.Dispose();
                Current._efConnection.Close();
                Current._efConnection.Dispose();
                Current._efConnection = null;
                Current._scopeCount = 0;
            }

            Current = null;
        }

        /// <summary>
        /// Получить SqlConnection
        /// </summary>
        public SqlConnection DbConnection => (SqlConnection)EfConnection.StoreConnection;

        /// <summary>
        /// Получить EntityConnection
        /// </summary>
        public EntityConnection EfConnection
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
                var sqlConnection = new SqlConnection(ConnectionString);
                var efc = new EntityConnection(MetadataWorkspace, sqlConnection);
                sqlConnection.Open();
                efc.Open();
                Current._efConnection = efc;
                if (Transaction.Current == null)
                {
                    using (var cmd = SqlCommandFactory.Create(SetIsolationLevelCommandText, sqlConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private static readonly MetadataWorkspace Mdw = new MetadataWorkspace(new[]
        {
            "res://*/QP8Model.csdl",
            "res://*/QP8Model.ssdl",
            "res://*/QP8Model.msl"
        }, new[] { typeof(QP8Entities).Assembly });

        private MetadataWorkspace MetadataWorkspace
        {
            get
            {
                if (IdentityInsertOptions == null || !IdentityInsertOptions.Any())
                {
                    return Mdw;
                }

                var edmAssembly = typeof(QP8Entities).Assembly;
                var metaReader = XmlReader.Create(edmAssembly.GetManifestResourceStream("QP8Model.ssdl"));
                var ssdl = XElement.Load(metaReader);
                CorrectSsdl(ssdl);

                var rdr = new List<XmlReader> { ssdl.CreateReader() };
                var sic = new StoreItemCollection(rdr);
                rdr[0] = XmlReader.Create(edmAssembly.GetManifestResourceStream("QP8Model.csdl"));

                var eic = new EdmItemCollection(rdr);
                rdr[0] = XmlReader.Create(edmAssembly.GetManifestResourceStream("QP8Model.msl"));

                var smic = new StorageMappingItemCollection(eic, sic, rdr);
                var workspace = new MetadataWorkspace();
                workspace.RegisterItemCollection(eic);
                workspace.RegisterItemCollection(sic);
                workspace.RegisterItemCollection(smic);

                return workspace;
            }
        }

        private void CorrectSsdl(XContainer ssdl)
        {
            var ns = XNamespace.Get(@"http://schemas.microsoft.com/ado/2009/11/edm/ssdl");
            CorrectEntityType(ssdl, ns, EntityTypeCode.Site, "SITE", "SITE_ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.Content, "CONTENT", "CONTENT_ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.ContentGroup, "content_group", "content_group_id");
            CorrectEntityType(ssdl, ns, EntityTypeCode.Field, "CONTENT_ATTRIBUTE", "ATTRIBUTE_ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.ContentLink, "content_to_content", "link_id");
            CorrectEntityType(ssdl, ns, EntityTypeCode.CustomAction, "CUSTOM_ACTION", "ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.BackendAction, "BACKEND_ACTION", "ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.VisualEditorCommand, "VE_COMMAND", "ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.VisualEditorPlugin, "VE_PLUGIN", "ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.VisualEditorStyle, "VE_STYLE", "ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.User, "USERS", "USER_ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.UserGroup, "USER_GROUP", "GROUP_ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.Workflow, "WORKFLOW", "WORFKLOW_ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.StatusType, "STATUS_TYPE", "STATUS_TYPE_ID");
            CorrectEntityType(ssdl, ns, EntityTypeCode.Notification, "NOTIFICATIONS", "NOTIFICATION_ID");
        }

        private void CorrectEntityType(XContainer ssdl, XNamespace ns, string entityTypeCode, string tableName, string keyName)
        {
            if (IdentityInsertOptions != null && IdentityInsertOptions.Contains(entityTypeCode))
            {
                XElement table;
                try
                {
                    table = ssdl.Descendants(ns + "EntityType").Single(n => n.Attribute("Name").Value == tableName);
                }
                catch (InvalidOperationException)
                {
                    throw new ApplicationException($"Table {tableName} with namespace {ns} is not found in ssdl");
                }

                XElement column;
                try
                {
                    column = table.Elements(ns + "Property").Single(n => n.Attribute("Name").Value == keyName);
                }
                catch (InvalidOperationException)
                {
                    throw new ApplicationException($"Column {keyName} with namespace {ns} for table {tableName} is not found in ssdl");
                }

                column.Attribute("StoreGeneratedPattern").Value = "None";
            }
        }
    }
}
