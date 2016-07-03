using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using System.Data.SqlClient;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Web;
using System.Reflection;
using System.Transactions;
using System.Xml;
using System.Xml.Linq;
using System.Data.Mapping;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL
{
    public sealed class QPConnectionScope : IDisposable
    {
        [ThreadStatic]
        private static QPConnectionScope currentScope = null;

		private string ConnectionString { get; set; }

		public static QPConnectionScope Current
		{
			get { return currentScope; }
			private set { currentScope = value; }
		}

        public static string SetIsolationLevelCommandText
        {
            get
            {
                return "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
            }
        }

        private EntityConnection efConnection = null;

        /// <summary>
        /// Количество scope вложенных друг в друга
        /// </summary>
        private int scopeCount = 0;

		private HashSet<string> identityInsertOptions = new HashSet<string>();

		public QPConnectionScope(string connectionString)
        {
            // Если scope для потока не существует, то делаем создаваемый текущим для потока
			if (Current == null)
			{
				if (string.IsNullOrWhiteSpace(connectionString))
					throw new ArgumentNullException("connectionString");
				Current = this;
				ConnectionString = connectionString;
			}
			else if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Equals(Current.ConnectionString, StringComparison.OrdinalIgnoreCase))
				throw new ArgumentException("Attempt to create connection in the existing scope with different connection string.");

            // увеличиваем счетчик вложенных scope
            Current.scopeCount++;
        }

		public QPConnectionScope() : this(QPContext.CurrentDBConnectionString) { }

		public QPConnectionScope(string connectionString, HashSet<string> identityInsertOptions) : this(connectionString)
		{
			this.identityInsertOptions = identityInsertOptions;
		}

        public void Dispose()
        {
            if (Current != null)
            {
                // уменьшаем счетчик вложенных scope
                Current.scopeCount--;
                // если это последний, то все очищаем
				if (Current.scopeCount == 0)
					ForcedDispose();
            }
        }

		public void ForcedDispose()
		{
			if (Current.efConnection != null)
			{
				Current.efConnection.StoreConnection.Close();
				Current.efConnection.StoreConnection.Dispose();
				Current.efConnection.Close();
				Current.efConnection.Dispose();
				Current.efConnection = null;
				Current.scopeCount = 0;
			}
			Current = null;
		}

        /// <summary>
        /// Получить SqlConnection
        /// </summary>
        public SqlConnection DbConnection
        {
            get
            {
				return (SqlConnection)EFConnection.StoreConnection;
            }
        }

        /// <summary>
        /// Получить EntityConnection
        /// </summary>
        public EntityConnection EFConnection
        {
            get
            {
                Current.CreateEFConnection();
                return Current.efConnection;
            }
        }

		public HashSet<string> IdentityInsertOptions
		{
			get
			{
				return identityInsertOptions;
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000",
		Justification = "Object will dispose in QPConnectionScope.Dispose")]
        private void CreateEFConnection()
        {
			if (Current.efConnection == null)
            {
                SqlConnection sqlConnection = new SqlConnection(this.ConnectionString);
                var efc = new EntityConnection(MetadataWorkspace, sqlConnection);
                sqlConnection.Open();
                efc.Open();
				Current.efConnection = efc;
				if (Transaction.Current == null)
				{
					using (SqlCommand cmd = SqlCommandFactory.Create(SetIsolationLevelCommandText, sqlConnection))
					{
						cmd.ExecuteNonQuery();
					}
				}
            }
        }

		private static MetadataWorkspace mdw = new MetadataWorkspace(new[]
                    {
                        "res://*/QP8Model.csdl",
                        "res://*/QP8Model.ssdl",
                        "res://*/QP8Model.msl"
                    }, new[] { typeof(QP8Entities).Assembly });

		private MetadataWorkspace MetadataWorkspace
		{
			get
			{
				if (!identityInsertOptions.Any())
					return mdw;
				else
				{
					Assembly edmAssembly = typeof(QP8Entities).Assembly;
					XmlReader metaReader = XmlReader.Create(edmAssembly.GetManifestResourceStream("QP8Model.ssdl"));
					XElement ssdl = XElement.Load(metaReader);
					CorrectSsdl(ssdl);

					List<XmlReader> rdr = new List<XmlReader>();
					rdr.Add(ssdl.CreateReader());
					StoreItemCollection sic = new StoreItemCollection(rdr);
					rdr[0] = XmlReader.Create(edmAssembly.GetManifestResourceStream("QP8Model.csdl"));
					EdmItemCollection eic = new EdmItemCollection(rdr);
					rdr[0] = XmlReader.Create(edmAssembly.GetManifestResourceStream("QP8Model.msl"));
					StorageMappingItemCollection smic = new StorageMappingItemCollection(eic, sic, rdr);

					MetadataWorkspace workspace = new MetadataWorkspace();
					workspace.RegisterItemCollection(eic);
					workspace.RegisterItemCollection(sic);
					workspace.RegisterItemCollection(smic);

					return workspace;
				}

			}
		}

        private void CorrectSsdl(XElement ssdl)
        {
            XNamespace ns = XNamespace.Get(@"http://schemas.microsoft.com/ado/2009/11/edm/ssdl");
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

        private void CorrectEntityType(XElement ssdl, XNamespace ns, string entityTypeCode, string tableName, string keyName )
        {
            if (identityInsertOptions != null && identityInsertOptions.Contains(entityTypeCode))
            {
                XElement table;
                try
                {
                    table = ssdl.Descendants(ns + "EntityType")
                        .Single(n => n.Attribute("Name").Value == tableName);
                }
                catch (InvalidOperationException)
                {
                    throw new ApplicationException($"Table {tableName} with namespace {ns} is not found in ssdl");
                }

                XElement column;
                try
                {
                    column = table.Elements(ns + "Property")
                        .Single(n => n.Attribute("Name").Value == keyName);
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
