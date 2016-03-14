using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Runtime.Caching;
using Microsoft.Data.Extensions;
using Quantumart.QP8;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Utils.Sorting;

namespace Quantumart.QP8.DAL
{
	public partial class QP8Entities : ObjectContext
	{
		XNamespace MAPPING_NAMESPACE = "http://schemas.microsoft.com/ado/2008/09/mapping/cs";
		XNamespace CONCEPTUAL_MODEL_NAMESPACE = XNamespace.Get("http://schemas.microsoft.com/ado/2008/09/edm");

		public static readonly string COUNT_COLUMN = "ROWS_COUNT";

		private ObjectCache _cache = MemoryCache.Default; // кэш

		private XDocument _storageModel = null;

		partial void OnContextCreated()
		{
			this.CommandTimeout = SqlCommandFactory.CommandTimeout;
		}

		/// <summary>
		/// модель хранилища в формате XML
		/// </summary>
		public XDocument StorageModel
		{
			get
			{
				if (_storageModel == null)
				{
					_storageModel = _cache["StorageModel"] as XDocument;
				}

				if (_storageModel == null)
				{
					_storageModel = this.ExtractEdmxContent(DataSpace.SSpace, Assembly.GetExecutingAssembly());
					_cache.Add("StorageModel", _storageModel, null);
				}

				return _storageModel;
			}
		}

		private XDocument _conceptualModel = null;
		/// <summary>
		/// концептуальная модель в формате XML
		/// </summary>
		public XDocument ConceptualModel
		{
			get 
			{
				if (_conceptualModel == null)
				{
					_conceptualModel = _cache["ConceptualModel"] as XDocument;
				}

				if (_conceptualModel == null)
				{
					_conceptualModel = this.ExtractEdmxContent(DataSpace.CSpace, Assembly.GetExecutingAssembly());
					_cache.Add("ConceptualModel", _conceptualModel, null);
				}

				return _conceptualModel;
			}
		}

		private XDocument _mapping = null;
		/// <summary>
		/// настройки маппинга в формате XML
		/// </summary>
		public XDocument Mapping
		{
			get
			{
				if (_mapping == null)
				{
					_mapping = _cache["Mapping"] as XDocument;
				}

				if (_mapping == null)
				{
					_mapping = this.ExtractEdmxContent(DataSpace.CSSpace, Assembly.GetExecutingAssembly());
					_cache.Add("Mapping", _mapping, null);
				}

				return _mapping;
			}
		}

        /// <summary>
        /// материализатор для типа сущности UserDAL
        /// </summary>
        private Materializer<UserDAL> _userMaterializer = new Materializer<UserDAL>(r =>
            new UserDAL()
            {
                Id = r.Field<decimal>("USER_ID"),
                Disabled = r.Field<decimal>("DISABLED"),
                FirstName = r.Field<string>("FIRST_NAME"),
                LastName = r.Field<string>("LAST_NAME"),
                Email = r.Field<string>("EMAIL"),
                AutoLogOn = r.Field<decimal>("AUTO_LOGIN"),
                NTLogOn = r.Field<string>("NT_LOGIN"),
                LastLogOn = r.Field<DateTime?>("LAST_LOGIN"),
                Subscribed = r.Field<decimal>("SUBSCRIBED"),
                Created = r.Field<DateTime>("CREATED"),
                Modified = r.Field<DateTime>("MODIFIED"),
                LastModifiedBy = r.Field<decimal>("LAST_MODIFIED_BY"),
                LanguageId = r.Field<decimal?>("LANGUAGE_ID"),
                VMode = r.Field<decimal>("VMODE"),
                AdSid = r.Field<byte[]>("ad_sid"),
                AllowStageEditField = r.Field<decimal>("allow_stage_edit_field"),
                AllowStageEditObject = r.Field<decimal>("allow_stage_edit_object"),
                BuiltIn = r.Field<bool>("BUILT_IN"),
                LogOn = r.Field<string>("LOGIN"),
                PasswordModified = r.Field<DateTime>("PASSWORD_MODIFIED")
            }
        );


		/// <summary>
		/// материализатор для навигационного свойства LastModifiedByUser типа сущности SiteDAL
		/// </summary>
		private Materializer<UserDAL> _modifierMaterializer = new Materializer<UserDAL>(r =>
			new UserDAL()
			{
				Id = r.Field<decimal>("MODIFIER_USER_ID"),
				FirstName = r.Field<string>("MODIFIER_FIRST_NAME"),
				LastName = r.Field<string>("MODIFIER_LAST_NAME"),
				Email = r.Field<string>("MODIFIER_EMAIL"),
				LogOn = r.Field<string>("MODIFIER_LOGIN")
			}
		);


        private Materializer<SiteFolderDAL> _siteFolderMaterializer = new Materializer<SiteFolderDAL>(r =>
            new SiteFolderDAL()
            {
                Id = r.Field<decimal>("FOLDER_ID"),
                Name = r.Field<string>("NAME"),
                Created = r.Field<DateTime>("CREATED"),
                Modified = r.Field<DateTime>("MODIFIED"),
                LastModifiedBy = r.Field<decimal>("LAST_MODIFIED_BY"),
				HasChildren = r.Field<bool>("HAS_CHILDREN")
            }
         );

		private Materializer<ContentFolderDAL> _contentFolderMaterializer = new Materializer<ContentFolderDAL>(r =>
			new ContentFolderDAL()
			{
				Id = r.Field<decimal>("FOLDER_ID"),
				Name = r.Field<string>("NAME"),
				Created = r.Field<DateTime>("CREATED"),
				Modified = r.Field<DateTime>("MODIFIED"),
				LastModifiedBy = r.Field<decimal>("LAST_MODIFIED_BY"),
				HasChildren = r.Field<bool>("HAS_CHILDREN")
			}
		);        

		/// <summary>
		/// Преобразует параметры сортировки концептуальной модели
		/// в параметры сортировки модели хранилища
		/// </summary>
		/// <param name="conceptualSortingParams">параметры сортировки концептуальной модели</param>
		/// <param name="entitySetName">название EntitySet</param>
		/// <param name="navigationPropertiesPrefixes">префиксы навигационных свойств</param>
		/// <returns>параметры сортировки модели хранилища</returns>
		private string GetStorageSortingParameters(string conceptualSortingParams, string entitySetName,
			Dictionary<string, string> navigationPropertiesPrefixes = null)
		{
			StringBuilder storageSortingParams = new StringBuilder();
			List<SortingInformation> sortInfoList = SqlSorting.GetSortingInformations(conceptualSortingParams);
			int sortInfoCount = sortInfoList.Count;

			if (sortInfoCount > 0)
			{
				for (int sortInfoIndex = 0; sortInfoIndex < sortInfoCount; sortInfoIndex++)
				{
					SortingInformation sortInfo = sortInfoList[sortInfoIndex];
					string storageFieldName = GetStoragePropertyName(sortInfo.FieldName, entitySetName, navigationPropertiesPrefixes);
					if (storageFieldName.Length > 0)
					{
						storageSortingParams.Append(storageFieldName);
						if (sortInfo.Direction == SortDirection.Descending)
						{
							storageSortingParams.Append(" DESC");
						}
						else
						{
							storageSortingParams.Append(" ASC");
						}
						if (sortInfoIndex < (sortInfoCount - 1))
						{
							storageSortingParams.Append(", ");
						}
					}
				}
			}

			sortInfoList.Clear();
			sortInfoList = null;

			return storageSortingParams.ToString();
		}

		/// <summary>
		/// Преобразует название свойства концептуальное модели в название свойства модели хранилища
		/// </summary>
		/// <param name="conceptualPropertyName">название свойства концептуальное модели</param>
		/// <param name="entitySetName">название EntitySet</param>
		/// <param name="navigationPropertiesPrefixes">префиксы навигационных свойств</param>
		/// <returns>название свойства модели хранилища</returns>
		private string GetStoragePropertyName(string conceptualPropertyName, string entitySetName, Dictionary<string, string> navigationPropertiesPrefixes = null)
		{
			string storagePropertyName = String.Empty;

			if (conceptualPropertyName.IndexOf(".") == -1)
			{
				// Скалярное свойство
				storagePropertyName = GetScalarPropertyColumnName(conceptualPropertyName, entitySetName);
			}
			else
			{
				// Навигационное свойство
				storagePropertyName = GetNavigationPropertyColumnName(conceptualPropertyName, entitySetName, navigationPropertiesPrefixes);
			}

			return storagePropertyName;
		}

		/// <summary>
		/// Возвращает название поля БД, которое ассоциировано с указанным скалярным свойством
		/// </summary>
		/// <param name="name">название скалярного свойства</param>
		/// <param name="entitySetName">название EntitySet</param>
		/// <returns>название поля БД</returns>
		private string GetScalarPropertyColumnName(string name, string entitySetName)
		{
			string columnName = String.Empty; // название поля БД, которое связано с указанным свойством

			XDocument mappingDoc = this.Mapping; // настройки маппинга
			if (mappingDoc == null)
			{
				throw new Exception("Элемент Mappings отсутствует в edmx-файле!");
			}

			XElement entitySetMappingElem = mappingDoc
				.Element(MAPPING_NAMESPACE + "Mapping")
				.Element(MAPPING_NAMESPACE + "EntityContainerMapping")
				.Elements(MAPPING_NAMESPACE + "EntitySetMapping")
				.Where(e => (string)e.Attribute("Name") == entitySetName)
				.SingleOrDefault()
				;

			if (entitySetMappingElem == null)
			{
				throw new Exception(String.Format("Элемент EntitySetMapping для {0} отсутствует в edmx-файле!", entitySetName));
			}

			XElement scalarPropertyElem = entitySetMappingElem
				.Element(MAPPING_NAMESPACE + "EntityTypeMapping")
				.Element(MAPPING_NAMESPACE + "MappingFragment")
				.Elements(MAPPING_NAMESPACE + "ScalarProperty")
				.Where(e => (string)e.Attribute("Name") == name)
				.SingleOrDefault()
				;

			entitySetMappingElem = null;

			if (scalarPropertyElem == null)
			{
				throw new Exception(String.Format("Скалярное свойство {0} ненайдено!", name));
			}

			columnName = (string)scalarPropertyElem.Attribute("ColumnName");

			scalarPropertyElem = null;

			return columnName;
		}

		/// <summary>
		/// Возвращает название поля БД, которое ассоциировано с указанным навигационным свойством
		/// </summary>
		/// <param name="name">название навигационного свойства</param>
		/// <param name="entitySetName">название EntitySet</param>
		/// <param name="navigationPropertiesPrefixes">префиксы навигационных свойств</param>
		/// <returns>название поля БД</returns>
		private string GetNavigationPropertyColumnName(string name, string entitySetName, Dictionary<string, string> navigationPropertiesPrefixes = null)
		{
			string navigationPropertyName = name.Split('.').First(); // название навигационного свойства
			string subPropertyName = String.Join(".", name.Split('.').Skip(1)); // название подсвойства
			string columnName = String.Empty; // название поля БД, которое связано с указанным свойством

			// Получаем название типа с которым связан EntitySet
			string entityTypeName = this.GetEntityTypeName(entitySetName);
			if (String.IsNullOrWhiteSpace(entityTypeName))
			{
				throw new Exception(String.Format("Не задан тип сущности для EntitySet`а {0}!", entitySetName));
			}

			// Находим навигационное свойство
			NavigationProperty navigationProperty = this.GetNavigationProperty(entityTypeName, navigationPropertyName);

			if (navigationProperty == null)
			{
				throw new Exception(String.Format("В типе сущности {0} отсутствует навигационное свойство под названием {1}!",
					entityTypeName, navigationPropertyName));
			}

			// Получаем название EntitySet, связанного с заданным навигационным свойством
			string associatedEntityTypeName = navigationProperty.ToEndMember.GetEntityType().Name;
			string associatedEntitySetName = this.GetEntitySetName(associatedEntityTypeName);

			// Получаем название поля БД, которое связано с указанным свойством
			string columnPrefix = String.Empty;
			if (navigationPropertiesPrefixes != null
				&& !String.IsNullOrWhiteSpace(navigationPropertiesPrefixes[navigationPropertyName]))
			{
				columnPrefix = navigationPropertiesPrefixes[navigationPropertyName];
			}
			else
			{
				columnPrefix = Utils.Formatter.ToUppercaseStyle(navigationPropertyName);
			}
			string subColumnName = GetStoragePropertyName(subPropertyName, associatedEntitySetName, navigationPropertiesPrefixes);

			columnName = String.Concat(columnPrefix, "_", subColumnName);

			return columnName;
		}

        /// <summary>
        /// Возвращает информацию о пользователя по его логину и паролю
        /// </summary>
        /// <param name="login">логин</param>
        /// <param name="password">пароль</param>
        /// <param name="customerCode">код клиента</param>
        /// <param name="useNtLogin">разрешает Windows-аутентификацию</param>
        /// <param name="checkAdminAccess">включает проверку наличия прав администратора</param>
        /// <returns>информация о пользователе</returns>
        public UserDAL Authenticate(string login, string password, bool useNtLogin, bool checkAdminAccess)
        {
            UserDAL user = null;

            object[] parameters = 
			{
                new SqlParameter() { ParameterName = "login", DbType = DbType.String, Size = 255, Value = login },
                new SqlParameter() { ParameterName = "password", DbType = DbType.String, Size = 20, Value = password ?? String.Empty },
                new SqlParameter() { ParameterName = "use_nt_login", DbType = DbType.Boolean, Value = useNtLogin },
                new SqlParameter() { ParameterName = "check_admin_access", DbType = DbType.Boolean, Value = checkAdminAccess }
            };

            using (DbCommand dbCommand = this.CreateStoreCommand("qp_authenticate", CommandType.StoredProcedure, parameters))
            {
                user = _userMaterializer
                    .Materialize(dbCommand)
                    .Bind(this.UserSet)
                    .SingleOrDefault()
                    ;
            }

            return user;
        }

		/// <summary>
		/// Возвращает список дочерних папок
		/// </summary>
		/// <param name="userId">идентификатор пользователя</param>
		/// <param name="siteId">идентификатор родителя</param>
		/// <param name="isSite">флаг, определяющий является ли родитель сайтом или контентом</param>
		/// <param name="folderId">идентификатор папки</param>
		/// <param name="permissionLevel">уровень доступа</param>
		/// <param name="countOnly">признак, разрешающий вернуть только количество записей</param>
		/// <param name="totalRecords">количество записей</param>
		/// <returns>список дочерних папок</returns>
		public List<SiteFolderDAL> GetChildSiteFoldersList(int userId, int siteId, int? folderId, int permissionLevel,
			bool countOnly, out int totalRecords)
		{
			List<SiteFolderDAL> articleList = new List<SiteFolderDAL>();
			totalRecords = -1;

			object[] parameters = 
			{
				new SqlParameter() { ParameterName = "user_id", DbType = DbType.Decimal, Value = userId }, 
				new SqlParameter() { ParameterName = "parent_entity_id", DbType = DbType.Decimal, Value = siteId },
				new SqlParameter() { ParameterName = "is_site", DbType = DbType.Boolean, Value = true },
				new SqlParameter() { ParameterName = "parent_folder_id", DbType = DbType.Decimal, IsNullable = true, Value = folderId },
				new SqlParameter() { ParameterName = "permission_level", DbType = DbType.Decimal, Value = permissionLevel },
				new SqlParameter() { ParameterName = "count_only", DbType = DbType.Int32, Value = countOnly },
				new SqlParameter() { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords }
			};

			using (DbCommand dbCommand = this.CreateStoreCommand("qp_get_folders_tree", CommandType.StoredProcedure, parameters))
			{
				using (dbCommand.Connection.CreateConnectionScope())
				{
					using (DbDataReader reader = dbCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							IDataRecord record = (IDataRecord)reader;
							SiteFolderDAL folder = _siteFolderMaterializer.Materialize(record);

							if (folder != null)
							{
								((SiteFolderDAL)folder).Bind(this.SiteFolderSet);

								UserDAL modifier = _modifierMaterializer.Materialize(record);
								if (modifier != null)
								{
									modifier.Bind(this.UserSet);
								}

								articleList.Add(folder);
							}
						}
					}

					totalRecords = (int)dbCommand.Parameters["total_records"].Value;
				}
			}

			return articleList;
		}

		/// <summary>
		/// Возвращает список дочерних папок
		/// </summary>
		/// <param name="userId">идентификатор пользователя</param>
		/// <param name="contentId">идентификатор контента</param>
		/// <param name="folderId">идентификатор папки</param>
		/// <param name="permissionLevel">уровень доступа</param>
		/// <param name="countOnly">признак, разрешающий вернуть только количество записей</param>
		/// <param name="totalRecords">количество записей</param>
		/// <returns>список дочерних папок</returns>
		public List<ContentFolderDAL> GetChildContentFoldersList(int userId, int contentId, int? folderId, int permissionLevel,
			bool countOnly, out int totalRecords)
		{
			List<ContentFolderDAL> articleList = new List<ContentFolderDAL>();
			totalRecords = -1;

			object[] parameters = 
			{
				new SqlParameter() { ParameterName = "user_id", DbType = DbType.Decimal, Value = userId }, 
				new SqlParameter() { ParameterName = "parent_entity_id", DbType = DbType.Decimal, Value = contentId },
				new SqlParameter() { ParameterName = "is_site", DbType = DbType.Boolean, Value = false },
				new SqlParameter() { ParameterName = "parent_folder_id", DbType = DbType.Decimal, IsNullable = true, Value = folderId },
				new SqlParameter() { ParameterName = "permission_level", DbType = DbType.Decimal, Value = permissionLevel },
				new SqlParameter() { ParameterName = "count_only", DbType = DbType.Int32, Value = countOnly },
				new SqlParameter() { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords }
			};

			using (DbCommand dbCommand = this.CreateStoreCommand("qp_get_folders_tree", CommandType.StoredProcedure, parameters))
			{
				using (dbCommand.Connection.CreateConnectionScope())
				{
					using (DbDataReader reader = dbCommand.ExecuteReader())
					{
						while (reader.Read())
						{
							IDataRecord record = (IDataRecord)reader;
							ContentFolderDAL folder = _contentFolderMaterializer.Materialize(record);

							if (folder != null)
							{
								((ContentFolderDAL)folder).Bind(this.ContentFolderSet);

								UserDAL modifier = _modifierMaterializer.Materialize(record);
								if (modifier != null)
								{
									modifier.Bind(this.UserSet);
								}

								articleList.Add(folder);
							}
						}
					}

					totalRecords = (int)dbCommand.Parameters["total_records"].Value;
				}
			}

			return articleList;
		}



		/// <summary>
		/// Возвращает название сущности
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <param name="entityId">идентификатор сущности</param>
		/// <returns>название сущности</returns>
		public string GetEntityName(string entityTypeCode, int entityId, int parentEntityId)
		{
			string entityName = String.Empty;

			object[] parameters = 
			{
				new SqlParameter() { ParameterName = "entity_type_code", DbType = DbType.String, Size = 50, Value = entityTypeCode },
				new SqlParameter() { ParameterName = "entity_id", DbType = DbType.Int32, Value = entityId },
				new SqlParameter() { ParameterName = "parent_entity_id", DbType = DbType.Int32, IsNullable = true, Value = parentEntityId },
				new SqlParameter() { ParameterName = "title", DbType = DbType.String, Size = 255,
					Direction = ParameterDirection.Output, Value = entityName }
			};

			using (DbCommand dbCommand = this.CreateStoreCommand("qp_get_entity_title", CommandType.StoredProcedure, parameters))
			{
				using (dbCommand.Connection.CreateConnectionScope())
				{
					dbCommand.ExecuteNonQuery();

					entityName = dbCommand.Parameters["title"].Value.ToString();
				}
			}

			return entityName;
		}

		/// <summary>
		/// Проверяет существование сущности 
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <param name="entityId">идентификатор сущности</param>
		/// <returns>результат проверки (true - существует; false - не существует)</returns>
		public bool CheckEntityExistence(string entityTypeCode, decimal entityId)
		{
			bool result = false;

			object[] parameters = 
			{
				new SqlParameter() { ParameterName = "entity_type_code", DbType = DbType.String, Size = 50, Value = entityTypeCode },
				new SqlParameter() { ParameterName = "entity_id", DbType = DbType.Int32, Value = entityId }
			};

            using (DbCommand dbCommand = this.CreateStoreCommand("qp_check_entity_existence", CommandType.StoredProcedure, parameters))
            {
				using (dbCommand.Connection.CreateConnectionScope())
				{
					result = bool.Parse(dbCommand.ExecuteScalar().ToString());
				}
			}

			return result;
		}
		

		/// <summary>
		/// Устанавливает свойство Visible для content item
		/// </summary>
		/// <param name="articleId"></param>
		public void SetContentItemVisible(int contentItemId, bool isVisible)
		{
			object[] parameters = 
			{
                new SqlParameter() { ParameterName = "id", DbType = DbType.Decimal, Value = contentItemId },
				new SqlParameter() { ParameterName = "is_visible", DbType = DbType.Decimal, Value = isVisible ? 1 : 0 }
            };

			using (DbCommand dbCommand = this.CreateStoreCommand("UPDATE content_item with(rowlock) SET visible = @is_visible, modified = getdate(), last_modified_by = 1 WHERE content_item_id = @id", CommandType.Text, parameters))
			{
				dbCommand.ExecuteNonQuery();	
			}
		}

		/// <summary>
		/// qp_merge_article
		/// </summary>
		/// <param name="articleId"></param>
		public void MergeArticle(int contentItemId)
		{
			object[] parameters = 
			{
                new SqlParameter() { ParameterName = "item_id", DbType = DbType.Decimal, Value = contentItemId },				
            };

			using (DbCommand dbCommand = this.CreateStoreCommand("qp_merge_article", CommandType.StoredProcedure, parameters))
			{
				dbCommand.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="siteId"></param>
		/// <param name="searchString"></param>
		/// <param name="sortExpression"></param>
		/// <param name="startRow"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public DataTable SearchInArticles(int siteId, int userId, string searchString, int? articleId, string sortExpression, int startRow, int pageSize, out int totalRecords)
		{
			totalRecords = -1;
			object[] parameters = 
			{
                new SqlParameter() { ParameterName = "p_site_id", DbType = DbType.Int32, Value = siteId},
				new SqlParameter() { ParameterName = "p_user_id", DbType = DbType.Int32, Value = userId},
				new SqlParameter() { ParameterName = "p_item_id", DbType = DbType.Int32, Value = articleId, },
				new SqlParameter() { ParameterName = "p_searchparam", DbType = DbType.String, Value = Cleaner.ToSafeSqlString(searchString)},
				new SqlParameter() { ParameterName = "p_order_by", DbType = DbType.String, Value = sortExpression},
				new SqlParameter() { ParameterName = "p_start_row", DbType = DbType.Int32, Value = startRow},
				new SqlParameter() { ParameterName = "p_page_size", DbType = DbType.Int32, Value = pageSize},
				new SqlParameter() { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords },
            };

			using (DbCommand dbCommand = this.CreateStoreCommand("qp_all_article_search", CommandType.StoredProcedure, parameters))
			{
				DataTable dt = new DataTable();
				new SqlDataAdapter((SqlCommand)dbCommand).Fill(dt);
				totalRecords = (int)dbCommand.Parameters["total_records"].Value;
				return dt;
			}
		}

		/// <summary>
		/// Получить словоформы для строки запроса
		/// </summary>
		/// <param name="searchString"></param>
		/// <returns></returns>
		public IEnumerable<string> GetWordForms(string searchString)
		{
			object[] parameters = 
			{
                new SqlParameter() { ParameterName = "searchString", DbType = DbType.String, Value = Cleaner.ToSafeSqlString(searchString)},				
            };

			DataTable dt = new DataTable();

			using (DbCommand dbCommand = this.CreateStoreCommand("usp_fts_parser", CommandType.StoredProcedure, parameters))
			{				
				new SqlDataAdapter((SqlCommand)dbCommand).Fill(dt);				
			}

			return dt.AsEnumerable().Select(r => r.Field<string>("display_term")).ToArray();
		}

		public Version GetSqlServerVersion()
		{
			string version;
			using (DbCommand dbCommand = this.CreateStoreCommand("SELECT SERVERPROPERTY('productversion') [version]", CommandType.Text))
			{
				using (dbCommand.Connection.CreateConnectionScope())
				{
					version = dbCommand.ExecuteScalar().ToString();
				}
			}

			return new Version(version);
		}

		/// <summary>
		/// Получает список стоп-слов для языка который указан в full text serach индексе на столбце dbo.CONTENT_DATA.DATA
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetStopWordList()
		{
			string query = "select stopword from sys.fulltext_system_stopwords " +
							"where language_id = (" +
							"select top 1 language_id from sys.fulltext_index_columns " +
							"where object_id = object_id('dbo.CONTENT_DATA') " +
							"and column_id = COLUMNPROPERTY (object_id('dbo.CONTENT_DATA'), 'DATA' , 'ColumnId' ))";

			List<string> result = new List<string>();

			using (DbCommand dbCommand = this.CreateStoreCommand(query, CommandType.Text))
			{
				using (dbCommand.Connection.CreateConnectionScope())
				{
					using (DbDataReader reader = dbCommand.ExecuteReader())
                    {
						while (reader.Read())
						{
							result.Add(reader.GetString(0));
						}
					}
				}
			}
			return result;
		}		
	}
}
