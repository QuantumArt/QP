using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Xml.Linq;
using Microsoft.Data.Extensions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.DAL
{
    // ReSharper disable once InconsistentNaming
    public partial class QP8Entities
    {
        private readonly XNamespace _mappingNamespace = "http://schemas.microsoft.com/ado/2008/09/mapping/cs";

        public static readonly string CountColumn = "ROWS_COUNT";

        private readonly ObjectCache _cache = MemoryCache.Default;

        private XDocument _storageModel;

        partial void OnContextCreated()
        {
            CommandTimeout = SqlCommandFactory.CommandTimeout;
        }

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

        private XDocument _conceptualModel;

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

        private XDocument _mapping;

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

        private readonly Materializer<UserDAL> _userMaterializer = new Materializer<UserDAL>(r => new UserDAL
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
            Created = r.Field<DateTime>(FieldName.Created),
            Modified = r.Field<DateTime>(FieldName.Modified),
            LastModifiedBy = r.Field<decimal>(FieldName.LastModifiedBy),
            LanguageId = r.Field<decimal?>("LANGUAGE_ID"),
            VMode = r.Field<decimal>("VMODE"),
            AdSid = r.Field<byte[]>("ad_sid"),
            AllowStageEditField = r.Field<decimal>("allow_stage_edit_field"),
            AllowStageEditObject = r.Field<decimal>("allow_stage_edit_object"),
            BuiltIn = r.Field<bool>("BUILT_IN"),
            LogOn = r.Field<string>("LOGIN"),
            PasswordModified = r.Field<DateTime>("PASSWORD_MODIFIED")
        });

        private readonly Materializer<UserDAL> _modifierMaterializer = new Materializer<UserDAL>(r => new UserDAL
        {
            Id = r.Field<decimal>("MODIFIER_USER_ID"),
            FirstName = r.Field<string>("MODIFIER_FIRST_NAME"),
            LastName = r.Field<string>("MODIFIER_LAST_NAME"),
            Email = r.Field<string>("MODIFIER_EMAIL"),
            LogOn = r.Field<string>(FieldName.ModifierLogin)
        });

        private readonly Materializer<SiteFolderDAL> _siteFolderMaterializer = new Materializer<SiteFolderDAL>(r => new SiteFolderDAL
        {
            Id = r.Field<decimal>("FOLDER_ID"),
            Name = r.Field<string>("NAME"),
            Created = r.Field<DateTime>(FieldName.Created),
            Modified = r.Field<DateTime>(FieldName.Modified),
            LastModifiedBy = r.Field<decimal>(FieldName.LastModifiedBy),
            HasChildren = r.Field<bool>("HAS_CHILDREN")
        });

        private readonly Materializer<ContentFolderDAL> _contentFolderMaterializer = new Materializer<ContentFolderDAL>(r => new ContentFolderDAL
        {
            Id = r.Field<decimal>("FOLDER_ID"),
            Name = r.Field<string>("NAME"),
            Created = r.Field<DateTime>(FieldName.Created),
            Modified = r.Field<DateTime>(FieldName.Modified),
            LastModifiedBy = r.Field<decimal>(FieldName.LastModifiedBy),
            HasChildren = r.Field<bool>("HAS_CHILDREN")
        });

        /// <summary>
        /// Преобразует название свойства концептуальное модели в название свойства модели хранилища
        /// </summary>
        /// <param name="conceptualPropertyName">название свойства концептуальное модели</param>
        /// <param name="entitySetName">название EntitySet</param>
        /// <param name="navigationPropertiesPrefixes">префиксы навигационных свойств</param>
        /// <returns>название свойства модели хранилища</returns>
        private string GetStoragePropertyName(string conceptualPropertyName, string entitySetName, Dictionary<string, string> navigationPropertiesPrefixes = null)
        {
            var storagePropertyName = conceptualPropertyName.IndexOf(".", StringComparison.Ordinal) == -1
                ? GetScalarPropertyColumnName(conceptualPropertyName, entitySetName)
                : GetNavigationPropertyColumnName(conceptualPropertyName, entitySetName, navigationPropertiesPrefixes);

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
            var mappingDoc = Mapping;
            if (mappingDoc == null)
            {
                throw new Exception("Элемент Mappings отсутствует в edmx-файле!");
            }

            var entitySetMappingElem = mappingDoc
                .Element(_mappingNamespace + "Mapping")?
                .Element(_mappingNamespace + "EntityContainerMapping")?
                .Elements(_mappingNamespace + "EntitySetMapping")
                .SingleOrDefault(e => (string)e.Attribute("Name") == entitySetName);

            if (entitySetMappingElem == null)
            {
                throw new Exception($"Элемент EntitySetMapping для {entitySetName} отсутствует в edmx-файле!");
            }

            var scalarPropertyElem = entitySetMappingElem
                .Element(_mappingNamespace + "EntityTypeMapping")?
                .Element(_mappingNamespace + "MappingFragment")?
                .Elements(_mappingNamespace + "ScalarProperty")
                .SingleOrDefault(e => (string)e.Attribute("Name") == name);

            if (scalarPropertyElem == null)
            {
                throw new Exception($"Скалярное свойство {name} ненайдено!");
            }

            return (string)scalarPropertyElem.Attribute("ColumnName");
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
            var navigationPropertyName = name.Split('.').First(); // название навигационного свойства
            var subPropertyName = string.Join(".", name.Split('.').Skip(1)); // название подсвойства

            // Получаем название типа с которым связан EntitySet
            var entityTypeName = this.GetEntityTypeName(entitySetName);
            if (string.IsNullOrWhiteSpace(entityTypeName))
            {
                throw new Exception($"Не задан тип сущности для EntitySet`а {entitySetName}!");
            }

            // Находим навигационное свойство
            var navigationProperty = this.GetNavigationProperty(entityTypeName, navigationPropertyName);
            if (navigationProperty == null)
            {
                throw new Exception($"В типе сущности {entityTypeName} отсутствует навигационное свойство под названием {navigationPropertyName}!");
            }

            // Получаем название EntitySet, связанного с заданным навигационным свойством
            var associatedEntityTypeName = navigationProperty.ToEndMember.GetEntityType().Name;
            var associatedEntitySetName = this.GetEntitySetName(associatedEntityTypeName);

            // Получаем название поля БД, которое связано с указанным свойством
            var columnPrefix = !string.IsNullOrWhiteSpace(navigationPropertiesPrefixes?[navigationPropertyName])
                ? navigationPropertiesPrefixes[navigationPropertyName]
                : Formatter.ToUppercaseStyle(navigationPropertyName);

            var subColumnName = GetStoragePropertyName(subPropertyName, associatedEntitySetName, navigationPropertiesPrefixes);
            return string.Concat(columnPrefix, "_", subColumnName);
        }

        /// <summary>
        /// Возвращает информацию о пользователя по его логину и паролю
        /// </summary>
        /// <param name="login">логин</param>
        /// <param name="password">пароль</param>
        /// <param name="useNtLogin">разрешает Windows-аутентификацию</param>
        /// <param name="checkAdminAccess">включает проверку наличия прав администратора</param>
        /// <returns>информация о пользователе</returns>
        public UserDAL Authenticate(string login, string password, bool useNtLogin, bool checkAdminAccess)
        {
            UserDAL user;
            object[] parameters =
            {
                new SqlParameter { ParameterName = "login", DbType = DbType.String, Size = 255, Value = login },
                new SqlParameter { ParameterName = "password", DbType = DbType.String, Size = 20, Value = password ?? string.Empty },
                new SqlParameter { ParameterName = "use_nt_login", DbType = DbType.Boolean, Value = useNtLogin },
                new SqlParameter { ParameterName = "check_admin_access", DbType = DbType.Boolean, Value = checkAdminAccess }
            };

            using (var dbCommand = this.CreateStoreCommand("qp_authenticate", CommandType.StoredProcedure, parameters))
            {
                user = _userMaterializer
                    .Materialize(dbCommand)
                    .Bind(UserSet)
                    .SingleOrDefault();
            }

            return user;
        }

        /// <summary>
        /// Возвращает список дочерних папок
        /// </summary>
        /// <param name="userId">идентификатор пользователя</param>
        /// <param name="siteId">идентификатор родителя</param>
        /// <param name="folderId">идентификатор папки</param>
        /// <param name="permissionLevel">уровень доступа</param>
        /// <param name="countOnly">признак, разрешающий вернуть только количество записей</param>
        /// <param name="totalRecords">количество записей</param>
        /// <returns>список дочерних папок</returns>
        public List<SiteFolderDAL> GetChildSiteFoldersList(int userId, int siteId, int? folderId, int permissionLevel, bool countOnly, out int totalRecords)
        {
            var articleList = new List<SiteFolderDAL>();
            totalRecords = -1;

            object[] parameters =
            {
                new SqlParameter { ParameterName = "user_id", DbType = DbType.Decimal, Value = userId },
                new SqlParameter { ParameterName = "parent_entity_id", DbType = DbType.Decimal, Value = siteId },
                new SqlParameter { ParameterName = "is_site", DbType = DbType.Boolean, Value = true },
                new SqlParameter { ParameterName = "parent_folder_id", DbType = DbType.Decimal, IsNullable = true, Value = folderId },
                new SqlParameter { ParameterName = "permission_level", DbType = DbType.Decimal, Value = permissionLevel },
                new SqlParameter { ParameterName = "count_only", DbType = DbType.Int32, Value = countOnly },
                new SqlParameter { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords }
            };

            using (var dbCommand = this.CreateStoreCommand("qp_get_folders_tree", CommandType.StoredProcedure, parameters))
            {
                using (dbCommand.Connection.CreateConnectionScope())
                {
                    using (var reader = dbCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IDataRecord record = reader;
                            var folder = _siteFolderMaterializer.Materialize(record);
                            if (folder != null)
                            {
                                folder.Bind(SiteFolderSet);

                                var modifier = _modifierMaterializer.Materialize(record);
                                modifier?.Bind(UserSet);
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
        public List<ContentFolderDAL> GetChildContentFoldersList(int userId, int contentId, int? folderId, int permissionLevel, bool countOnly, out int totalRecords)
        {
            var articleList = new List<ContentFolderDAL>();
            totalRecords = -1;

            object[] parameters =
            {
                new SqlParameter { ParameterName = "user_id", DbType = DbType.Decimal, Value = userId },
                new SqlParameter { ParameterName = "parent_entity_id", DbType = DbType.Decimal, Value = contentId },
                new SqlParameter { ParameterName = "is_site", DbType = DbType.Boolean, Value = false },
                new SqlParameter { ParameterName = "parent_folder_id", DbType = DbType.Decimal, IsNullable = true, Value = folderId },
                new SqlParameter { ParameterName = "permission_level", DbType = DbType.Decimal, Value = permissionLevel },
                new SqlParameter { ParameterName = "count_only", DbType = DbType.Int32, Value = countOnly },
                new SqlParameter { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords }
            };

            using (var dbCommand = this.CreateStoreCommand("qp_get_folders_tree", CommandType.StoredProcedure, parameters))
            {
                using (dbCommand.Connection.CreateConnectionScope())
                {
                    using (var reader = dbCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            IDataRecord record = reader;
                            var folder = _contentFolderMaterializer.Materialize(record);

                            if (folder != null)
                            {
                                folder.Bind(ContentFolderSet);

                                var modifier = _modifierMaterializer.Materialize(record);
                                modifier?.Bind(UserSet);
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
        /// <returns>название сущности</returns>
        public string GetEntityName(string entityTypeCode, int entityId, int parentEntityId)
        {
            var entityName = string.Empty;
            object[] parameters =
            {
                new SqlParameter { ParameterName = "entity_type_code", DbType = DbType.String, Size = 50, Value = entityTypeCode },
                new SqlParameter { ParameterName = "entity_id", DbType = DbType.Int32, Value = entityId },
                new SqlParameter { ParameterName = "parent_entity_id", DbType = DbType.Int32, IsNullable = true, Value = parentEntityId },
                new SqlParameter
                {
                    ParameterName = "title",
                    DbType = DbType.String,
                    Size = 255,
                    Direction = ParameterDirection.Output,
                    Value = entityName
                }
            };

            using (var dbCommand = this.CreateStoreCommand("qp_get_entity_title", CommandType.StoredProcedure, parameters))
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
            bool result;
            object[] parameters =
            {
                new SqlParameter { ParameterName = "entity_type_code", DbType = DbType.String, Size = 50, Value = entityTypeCode },
                new SqlParameter { ParameterName = "entity_id", DbType = DbType.Int32, Value = entityId }
            };

            using (var dbCommand = this.CreateStoreCommand("qp_check_entity_existence", CommandType.StoredProcedure, parameters))
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
        public void SetContentItemVisible(int contentItemId, bool isVisible, int lastModifiedBy = 1)
        {
            object[] parameters =
            {
                new SqlParameter { ParameterName = "id", DbType = DbType.Decimal, Value = contentItemId },
                new SqlParameter { ParameterName = "is_visible", DbType = DbType.Decimal, Value = isVisible ? 1 : 0 },
                new SqlParameter { ParameterName = "last_modified_by", DbType = DbType.Decimal, Value = lastModifiedBy }
            };

            using (var dbCommand = this.CreateStoreCommand("UPDATE content_item with(rowlock) SET visible = @is_visible, modified = getdate(), last_modified_by = @last_modified_by WHERE content_item_id = @id", CommandType.Text, parameters))
            {
                dbCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// qp_merge_article
        /// </summary>
        public void MergeArticle(int contentItemId, int lastModifiedBy = 1)
        {
            object[] parameters =
            {
                new SqlParameter { ParameterName = "item_id", DbType = DbType.Decimal, Value = contentItemId },
                new SqlParameter { ParameterName = "last_modified_by", DbType = DbType.Decimal, Value = lastModifiedBy }
            };

            using (var dbCommand = this.CreateStoreCommand("qp_merge_article", CommandType.StoredProcedure, parameters))
            {
                dbCommand.ExecuteNonQuery();
            }
        }

        public DataTable SearchInArticles(int siteId, int userId, string searchString, int? articleId, string sortExpression, int startRow, int pageSize, out int totalRecords)
        {
            totalRecords = -1;
            object[] parameters =
            {
                new SqlParameter { ParameterName = "p_site_id", DbType = DbType.Int32, Value = siteId },
                new SqlParameter { ParameterName = "p_user_id", DbType = DbType.Int32, Value = userId },
                new SqlParameter { ParameterName = "p_item_id", DbType = DbType.Int32, Value = articleId },
                new SqlParameter { ParameterName = "p_searchparam", DbType = DbType.String, Value = Cleaner.ToSafeSqlString(searchString) },
                new SqlParameter { ParameterName = "p_order_by", DbType = DbType.String, Value = sortExpression },
                new SqlParameter { ParameterName = "p_start_row", DbType = DbType.Int32, Value = startRow },
                new SqlParameter { ParameterName = "p_page_size", DbType = DbType.Int32, Value = pageSize },
                new SqlParameter { ParameterName = "total_records", Direction = ParameterDirection.InputOutput, DbType = DbType.Int32, Value = totalRecords }
            };

            using (var dbCommand = this.CreateStoreCommand("qp_all_article_search", CommandType.StoredProcedure, parameters))
            {
                var dt = new DataTable();
                new SqlDataAdapter((SqlCommand)dbCommand).Fill(dt);
                totalRecords = (int)dbCommand.Parameters["total_records"].Value;
                return dt;
            }
        }

        /// <summary>
        /// Получить словоформы для строки запроса
        /// </summary>
        public IEnumerable<string> GetWordForms(string searchString)
        {
            object[] parameters =
            {
                new SqlParameter { ParameterName = "searchString", DbType = DbType.String, Value = Cleaner.ToSafeSqlString(searchString) }
            };

            var dt = new DataTable();

            using (var dbCommand = this.CreateStoreCommand("usp_fts_parser", CommandType.StoredProcedure, parameters))
            {
                new SqlDataAdapter((SqlCommand)dbCommand).Fill(dt);
            }

            return dt.AsEnumerable().Select(r => r.Field<string>("display_term")).ToArray();
        }

        public Version GetSqlServerVersion()
        {
            string version;
            using (var dbCommand = this.CreateStoreCommand("SELECT SERVERPROPERTY('productversion') [version]", CommandType.Text))
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
        public IEnumerable<string> GetStopWordList()
        {
            const string query = "select stopword from sys.fulltext_system_stopwords " +
                "where language_id = (" +
                "select top 1 language_id from sys.fulltext_index_columns " +
                "where object_id = object_id('dbo.CONTENT_DATA') " +
                "and column_id = COLUMNPROPERTY (object_id('dbo.CONTENT_DATA'), 'DATA' , 'ColumnId' ))";

            var result = new List<string>();

            using (var dbCommand = this.CreateStoreCommand(query, CommandType.Text))
            {
                using (dbCommand.Connection.CreateConnectionScope())
                {
                    using (var reader = dbCommand.ExecuteReader())
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
