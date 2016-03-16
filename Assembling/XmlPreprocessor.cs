using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using QA_Assembling.Info;

namespace QA_Assembling
{
    public class XmlPreprocessor
    {
        public AssembleContentsController Controller { get; set; }

        public XmlPreprocessor(AssembleContentsController caller)
        {
            Controller = caller;
        }

        #region private members

        internal class ContentProperties
        {
	        public int Id { get; set; }

	        public bool IsVirtual { get; set; }

	        public bool IsUserQuery { get; set; }

            public ContentProperties(int id, bool isVirtual, bool isUserQuery)
            {
                Id = id;
                IsVirtual = isVirtual;
	            IsUserQuery = isUserQuery;
            }

        }

        private ContentProperties GetContentProperties(string name)
        {
            var dv = new DataView(Controller.ContentsTable)
            {
                RowFilter = String.Format(CultureInfo.InvariantCulture, "content_name = '{0}'", name)
            };
            if (dv.Count == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Cannot find content {0}", name));

            }
            else
            {
                var id = Int32.Parse(dv[0]["content_id"].ToString(), CultureInfo.InvariantCulture);
                var isVirtual = dv[0]["virtual_type"].ToString() != "0";
				var isUserQuery = dv[0]["virtual_type"].ToString() == "3";
				return new ContentProperties(id, isVirtual, isUserQuery);
            }
        }

        private static int? GetNullableInt32(DataRowView row, string key)
        {
            return !(row[key] is DBNull) ? (int?) GetInt32(row, key) : null;
        }


        private static int GetInt32(DataRowView row, string key)
        {
            return Int32.Parse(row[key].ToString(), CultureInfo.InvariantCulture);
        }

        private static int GetFieldId(DataRowView row)
        {
            return GetInt32(row, "attribute_id");
        }

        private static int GetFieldSize(DataRowView row)
        {
            return GetInt32(row, "attribute_size");
        }

        private static string GetFieldType(DataRowView row)
        {
            var result = row["type_name"].ToString();
            if ("Relation" == result)
            {
                if (row["link_id"] is DBNull)
                {
                    result = "O2M";
                }
                else
                {
                    result = "M2M";
                }
            }
            if ("Relation Many-to-One" == result)
                result = "M2O";
            return result;
        }

        private static int GetFieldLinkId(DataRowView row)
        {
            if (!(row["link_id"] is DBNull))
            {
                return GetInt32(row, "link_id");
            }
            else
            {
                throw new ArgumentNullException("Link_id is null for field " + GetInt32(row, "attribute_id"));
            }
        }

        private static int? GetRelatedM2OId(DataRowView row)
        {
            return GetNullableInt32(row, "related_m2o_id");
        }

        private static int? GetBaseM2OId(DataRowView row)
        {
            return GetNullableInt32(row, "back_related_attribute_id");
        }

        private DataRowView GetRootRow(DataRowView row)
        {
            var result = row;
            var fieldId = "";

            if (!DBNull.Value.Equals(row["persistent_attr_id"]))
            {
                fieldId = row["persistent_attr_id"].ToString();
            }
            else if (!DBNull.Value.Equals(row["union_attr_id"]))
            {
                fieldId = row["union_attr_id"].ToString();
            }
            else if (!DBNull.Value.Equals(row["uq_attr_id"]))
            {
                fieldId = row["uq_attr_id"].ToString();
            }

            if (!String.IsNullOrEmpty(fieldId))
            {
                var dv = new DataView(Controller.FieldsTable) {RowFilter = "attribute_id = " + fieldId};
                result = dv.Count == 0 ? null : GetRootRow(dv[0]);
            }
            return result;
        }

        private int GetRelatedContentId(DataRowView row)
        {
            var fieldType = GetFieldType(row);
            if ("O2M" == fieldType || "M2O" == fieldType)
            {
                var dv = new DataView(Controller.FieldsTable);
                var relatedId = row["O2M" == fieldType ? "related_attribute_id" : "back_related_attribute_id"].ToString();
                if (String.IsNullOrEmpty(relatedId))
                {
                    return 0;
                }
                else
                {
                    dv.RowFilter = "attribute_id = " + relatedId;
                    return dv.Count == 0 ? 0 : Int32.Parse(dv[0]["content_id"].ToString(), CultureInfo.InvariantCulture);
                }
            }
            else if ("M2M" == fieldType)
            {
                var rootRow = GetRootRow(row);
                if (rootRow == null)
                {
                    return 0;
                }
                else
                {
                    var dv = new DataView(Controller.LinkTable)
                    {
                        RowFilter =
                            string.Format(CultureInfo.InvariantCulture, "link_id = {0} and content_id = {1}",
                                rootRow["link_id"], rootRow["content_id"])
                    };
                    return dv.Count == 0 ? 0 : Int32.Parse(dv[0]["linked_content_id"].ToString(), CultureInfo.InvariantCulture);
                }
            }
            else
            {
                return 0;
            }

        }


        private DataRowView GetFieldRow(string name, int contentId)
        {
            var dv = new DataView(Controller.FieldsTable)
            {
                RowFilter =
                    String.Format(CultureInfo.InvariantCulture, "attribute_name = '{0}' and content_id = {1}", name,
                        contentId)
            };
            if (dv.Count == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Cannot find field {0} in content {1}", name, contentId.ToString(CultureInfo.InvariantCulture)));
            }
            else
            {
                return dv[0];
            }
        }

		private DataRowView GetFieldInfoRow(string name, int contentId)
		{
		    var dv = new DataView(Controller.FieldsInfoTable)
		    {
		        RowFilter =
		            String.Format(CultureInfo.InvariantCulture, "COLUMN_NAME = '{0}' and TABLE_NAME = 'content_{1}'", name,
		                contentId)
		    };
		    if (dv.Count == 0)
			{
				throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "Cannot find field {0} in table content_{1}", name, contentId.ToString(CultureInfo.InvariantCulture)));
			}
			else
			{
				return dv[0];
			}
		}

		private void AppendToSchema(XDocument doc, XElement elem)
        {
            if (elem != null)
            {
                var xElement = doc.Element("schema");
                xElement?.Add(elem);
            }
        }

        private void CorrectLinks(XDocument doc)
        {
            var usedLinks = doc.Descendants("attribute").Where(el => el.Attribute("link_id") != null).Select(el => el.Attribute("link_id")?.Value).Where(n => !String.IsNullOrEmpty(n)).Distinct().ToArray();
            var definedLinks = doc.Descendants("link").Select(el => el.Attribute("id")?.Value).ToArray();
            var undefinedLinks = usedLinks.Where(n => !definedLinks.Contains(n));
            var unusedLinks = definedLinks.Where(n => !usedLinks.Contains(n));
            var equalCounts = doc.Descendants("link").GroupBy(n =>
                $"{n.Attribute("content_id")}_{n.Attribute("linked_content_id")}").Select(g => new { key = g.Key, count = g.Count() }).ToDictionary(n => n.key, n => n.count);

            doc.Descendants("link").Where(n =>
            {
                var xAttribute = n.Attribute("id");
                return xAttribute != null && unusedLinks.Contains(xAttribute.Value);
            }).Remove();

            var dv = new DataView(Controller.ContentToContentTable);
            foreach (var linkId in undefinedLinks)
            {
                dv.RowFilter = "link_id = " + linkId;
                AppendToSchema(doc, GetLinkElement(dv[0].Row, equalCounts, true));
            }

            foreach (var linkId in definedLinks.ToList())
            {
                dv.RowFilter = "link_id = " + linkId;
                var node = doc.Descendants("link").Single(n => n.Attribute("id")?.Value == linkId.ToString());
                node.ReplaceWith(GetLinkElement(dv[0].Row, equalCounts, node.Attribute("mapped_name")?.Value, node.Attribute("plural_mapped_name")?.Value, true));
            }
        }

        private void AppendStatuses(XDocument doc)
        {
            foreach (DataRow row in Controller.StatusTable.Rows)
            {
                var id = Int32.Parse(row["STATUS_TYPE_ID"].ToString());
                var name = row["STATUS_TYPE_NAME"].ToString();
                var siteId = Int32.Parse(row["SITE_ID"].ToString());

                AppendToSchema(doc,
                    new XElement("status_type",
                        new XAttribute("id", id),
                        new XAttribute("name", name),
                        new XAttribute("site_id", siteId)
                    )
                );
            }
        }

        private void RemoveEmptyRelations(XDocument doc)
        {
            doc.Root?.Descendants("attribute").Where(s => (string)s.Attribute("related_content_id") == "0").Remove();
        }


        private void SetFieldParams(XDocument doc)
        {
            if (doc.Root != null)
                foreach (var content in doc.Root.Descendants("content"))
                {
                    var contentId = SetContentParams(content);
                    var isUserQuery = content.Attribute("user_query")?.Value == "1";

                    foreach (var contentField in content.Descendants("attribute"))
                    {
                        SetFieldParams(contentId, contentField, isUserQuery);
                    }
                }
        }

        private void SetFieldParams(int contentId, XElement contentField, bool isUserQuery)
        {
            var fieldName = contentField.Attribute("name")?.Value;
            var row = GetFieldRow(fieldName, contentId);
	        var row2 = isUserQuery ? GetFieldInfoRow(fieldName, contentId) : null;
			SetFieldParams(contentField, row, row2);
        }

        private void SetFieldParams(XElement contentField, DataRowView field, DataRowView fieldInfo)
        {
	        if (fieldInfo != null)
	        {
		        var dataType = fieldInfo["DATA_TYPE"].ToString();
		        var resultDataType = GetResultDataType(dataType);
		        if (!string.IsNullOrEmpty(resultDataType))
					contentField.SetAttributeValue("force_db_type", resultDataType);

			}
			contentField.SetAttributeValue("id", GetFieldId(field));
            var fieldType = GetFieldType(field);
            contentField.SetAttributeValue("type", fieldType);

            if ("String" == fieldType || "Numeric" == fieldType)
            {
                contentField.SetAttributeValue("size", GetFieldSize(field));
            }

			if ("Numeric" == fieldType)
			{
				contentField.SetAttributeValue("is_long", (bool)field["IS_LONG"]);
			}

            var isClassifier = (bool)field["IS_CLASSIFIER"];

            if (isClassifier)
            {
                contentField.SetAttributeValue("is_classifier", "true");
                contentField.SetAttributeValue("use_inheritance", "true");
            }

            if ("M2M" == fieldType || "O2M" == fieldType || "M2O" == fieldType)
            {
                var relId = GetRelatedContentId(field);
                contentField.SetAttributeValue("related_content_id", relId);
                var classifierAttributeId = GetNullableInt32(field, "CLASSIFIER_ATTRIBUTE_ID");

                if (classifierAttributeId != null)
                {
                    contentField.Add(
                        new XAttribute("classifier_attribute_id", classifierAttributeId)
                    );
                }

                if ("M2O" == fieldType)
                {
                    var baseM2OId = GetBaseM2OId(field);
                    if (baseM2OId != null)
                        contentField.SetAttributeValue("related_attribute_id", baseM2OId.Value);
                }
                else if ("M2M" == fieldType)
                {
                    if (relId != 0)
                        contentField.SetAttributeValue("link_id", GetFieldLinkId(field));
                }
                else if ("O2M" == fieldType)
                {
                    contentField.SetAttributeValue("has_m2o", GetRelatedM2OId(field).HasValue.ToString().ToLowerInvariant());

                    if (contentField.Attribute("mapped_back_name") == null)
                        if (contentField.Parent != null)
                            contentField.SetAttributeValue("mapped_back_name", contentField.Parent.Attribute("plural_mapped_name")?.Value);
                }
            }
        }

	    private static string GetResultDataType(string dataType)
	    {
		    var resultDataType = "";
		    switch (dataType)
		    {
			    case "int":
				    resultDataType = "Int";
				    break;
			    case "tinyint":
					resultDataType = "TinyInt";
				
				    break;
			    case "smallint":
				    resultDataType = "SmallInt";
				    break;
			    case "bit":
				    resultDataType = "Bit";
				    break;
			    case "bigint":
				    resultDataType = "BigInt";
				    break;
		    }
		    return resultDataType;
	    }

	    private int SetContentParams(XElement content)
        {
	        var cp = GetContentProperties(content.Attribute("name")?.Value);
            content.SetAttributeValue("id", cp.Id);
            content.SetAttributeValue("virtual", cp.IsVirtual ? "1" : "0");
			content.SetAttributeValue("user_query", cp.IsUserQuery ? "1" : "0");
            return cp.Id;
        }

        private static void CreateEmptyMappedNames(XDocument doc)
        {
            if (doc.Root != null)
            {
                var fields = from el in doc.Root.Descendants("attribute") where el.Attribute("mapped_name") == null select el;
                foreach (var field in fields)
                {
                    field.SetAttributeValue("mapped_name", field.Attribute("name")?.Value);
                }
            }
        }

        private void SetRootParams(XDocument doc, AssembleContentsController cnt)
        {
            var root = doc.Root;
            if (root != null)
            {
                var attr = root.Attribute("namespace");
                if (attr != null) cnt.NameSpace = attr.Value;
                root.SetAttributeValue("siteId", Controller.SiteId.ToString(CultureInfo.InvariantCulture));
                root.SetAttributeValue("forStage", (!Controller.IsLive).ToString().ToLower());
                SetDefaultClass(root);
                SetConnectionString(root);
            }
        }

        private void SetConnectionString(XElement elem)
        {
            elem.SetAttributeValue("connectionString", Controller.Cnn.ConnectionString);
            if (elem.Attribute("connectionStringObject") == null)
            {
                elem.SetAttributeValue("connectionStringObject", "System.Configuration.ConfigurationManager.ConnectionStrings");
            }
        }

        private static void SetDefaultClass(XElement elem)
        {
            if (elem.Attribute("class") == null)
            {
                elem.SetAttributeValue("class", "QPDataContext");
            }
        }

        private static  bool HasRussianChars(string text)
        {
            return Regex.IsMatch(text, @"[а-яА-Я]");
        }

        private static bool IsValidIdentifier(string text)
        {
            return Regex.IsMatch(text, @"^[a-zA-Z][0-9a-zA-Z_]+$");
        }

        private static string GetDefaultName(int id, bool isContent)
        {
            return isContent ? "Content" + id : "Field" + id;
        }

        public static string GetMappedName(string name, int id, bool isContent)
        {
            var mappedName = Regex.Replace(name, @"[\s\._]+", String.Empty);
            if (!IsValidIdentifier(mappedName))
            {
                if (HasRussianChars(mappedName))
                {
                    mappedName = TranslateRusEng(mappedName);
                    if (!IsValidIdentifier(mappedName))
                        mappedName = GetDefaultName(id, isContent);
                }
                else
                {
                    mappedName = GetDefaultName(id, isContent);
                }
            }

            return mappedName;
        }


        private static Hashtable GetRusEngTranslator()
        {
            var dict = new Hashtable
            {
                {"а", "a"},
                {"б", "b"},
                {"в", "v"},
                {"г", "g"},
                {"д", "d"},
                {"е", "e"},
                {"ё", "e"},
                {"ж", "zh"},
                {"з", "z"},
                {"и", "i"},
                {"й", "y"},
                {"к", "k"},
                {"л", "l"},
                {"м", "m"},
                {"н", "n"},
                {"о", "o"},
                {"п", "p"},
                {"р", "r"},
                {"с", "s"},
                {"т", "t"},
                {"у", "u"},
                {"ф", "f"},
                {"х", "kh"},
                {"ц", "ts"},
                {"ч", "ch"},
                {"ш", "sh"},
                {"щ", "shch"},
                {"ъ", ""},
                {"ы", "y"},
                {"ь", ""},
                {"э", "e"},
                {"ю", "yu"},
                {"я", "ya"},
                {"А", "A"},
                {"Б", "B"},
                {"В", "V"},
                {"Г", "G"},
                {"Д", "D"},
                {"Е", "E"},
                {"Ё", "E"},
                {"Ж", "Zh"},
                {"З", "Z"},
                {"И", "I"},
                {"Й", "Y"},
                {"К", "K"},
                {"Л", "L"},
                {"М", "M"},
                {"Н", "N"},
                {"О", "O"},
                {"П", "P"},
                {"Р", "R"},
                {"С", "S"},
                {"Т", "T"},
                {"У", "U"},
                {"Ф", "F"},
                {"Х", "Kh"},
                {"Ц", "Ts"},
                {"Ч", "Ch"},
                {"Ш", "Sh"},
                {"Щ", "Shch"},
                {"Ъ", ""},
                {"Ы", "Y"},
                {"Ь", ""},
                {"Э", "E"},
                {"Ю", "Yu"},
                {"Я", "Ya"}
            };
            return dict;

        }
        private static string TranslateRusEng(string mappedName)
        {
            var sb = new StringBuilder();
            var dict = GetRusEngTranslator();
            foreach (var c in mappedName.ToCharArray())
            {
                var s = c.ToString();
                var s2 = dict.Contains(s) ? dict[s].ToString() : s;
                sb.Append(s2);
            }
            return sb.ToString();
        }

        private XElement GetSchemaElement(SchemaInfo info)
        {
            var result = new XElement("schema",
                new XAttribute("connectionStringName", info.ConnectionStringName),
                new XAttribute("class", info.ClassName)
            );

            if (!String.IsNullOrEmpty(info.NamespaceName))
                result.SetAttributeValue("namespace", info.NamespaceName);

            result.SetAttributeValue("useLongUrls", info.UseLongUrls);
            result.SetAttributeValue("replaceUrls", info.ReplaceUrls);
            result.SetAttributeValue("dbIndependent", info.DbIndependent);
            result.SetAttributeValue("isPartial", info.IsPartial);
			result.SetAttributeValue("sendNotifications", info.SendNotifications);
            result.SetAttributeValue("siteName", info.SiteName);
            return result;
        }

        private XElement GetLinkElement(DataRow row, Dictionary<string, int> equalCounts, bool useDb)
        {
            return GetLinkElement(row, equalCounts, String.Empty, String.Empty, useDb);
        }

        private XElement GetLinkElement(DataRow row, Dictionary<string, int> equalCounts, string customMappedName, string customPluralMappedName, bool useDb)
        {
            var linkId = Int32.Parse(row["LINK_ID"].ToString());
            var contentId = Int32.Parse(row["L_CONTENT_ID"].ToString());
            var linkedContentId = Int32.Parse(row["R_CONTENT_ID"].ToString());

            var mappedName = GetLinkedMappedName(contentId, linkedContentId);
            if (String.IsNullOrEmpty(mappedName))
                return null;

            var count = RegisterHit(equalCounts, $"{contentId} {linkedContentId}");
            mappedName = mappedName + "Article";
            var pluralMappedName = mappedName + "s";
            if (count > 1)
            {
                mappedName = $"{mappedName}_{count}";
                pluralMappedName = $"{pluralMappedName}_{count}";
            }


            if (useDb)
            {
                var dbMappedName = DbConnector.GetValue(row, "NET_LINK_NAME", "");
                var dbPluralMappedName = DbConnector.GetValue(row, "NET_PLURAL_LINK_NAME", "");
                mappedName = String.IsNullOrEmpty(dbMappedName) ? mappedName : dbMappedName;
                pluralMappedName = String.IsNullOrEmpty(dbPluralMappedName) ? pluralMappedName : dbPluralMappedName;
            }


            var linkElement =
                new XElement("link",
                    new XAttribute("id", linkId),
                    new XAttribute("self", contentId == linkedContentId ? "1" : "0"),
                    new XAttribute("content_id", contentId),
                    new XAttribute("linked_content_id", linkedContentId),
                    new XAttribute("mapped_name", !String.IsNullOrEmpty(customMappedName) ? customMappedName : mappedName),
                    new XAttribute("plural_mapped_name", !String.IsNullOrEmpty(customPluralMappedName) ? customPluralMappedName : pluralMappedName)
                );
            return linkElement;

        }

        private string GetLinkedMappedName(int contentId, int linkedContentId)
        {
            var dv = new DataView(Controller.ContentsTable) {RowFilter = "content_id = " + contentId};
            if (dv.Count == 0) return String.Empty;
            var firstMappedName = GetMappedName(dv[0]["CONTENT_NAME"].ToString(), Int32.Parse(dv[0]["CONTENT_ID"].ToString()), true);
            dv.RowFilter = "content_id = " + linkedContentId;
            if (dv.Count == 0) return String.Empty;
            var secondMappedName = GetMappedName(dv[0]["CONTENT_NAME"].ToString(), Int32.Parse(dv[0]["CONTENT_ID"].ToString()), true);
            var mappedName = $"{firstMappedName}{secondMappedName}";
            return mappedName;
        }

        private XElement GetContentElement(DataRow row, bool useDb)
        {
            var id = Int32.Parse(row["CONTENT_ID"].ToString());
            var name = row["CONTENT_NAME"].ToString();
            var mappedName = GetMappedName(name, id, true);

            mappedName = mappedName + "Article";
            var pluralMappedName = mappedName + "s";



            var dbMappedName = DbConnector.GetValue(row, "NET_CONTENT_NAME", "");
            var dbPluralMappedName = DbConnector.GetValue(row, "NET_PLURAL_CONTENT_NAME", "");
            var useDefaultFiltration = ((bool)row["use_default_filtration"]).ToString().ToLower();
            mappedName = String.IsNullOrEmpty(dbMappedName) ? mappedName : dbMappedName;
            pluralMappedName = String.IsNullOrEmpty(dbPluralMappedName) ? pluralMappedName : dbPluralMappedName;


            var contentElement =
                new XElement("content",
                    new XAttribute("id", id),
                    new XAttribute("name", name),
                    new XAttribute("mapped_name", mappedName),
                    new XAttribute("plural_mapped_name", pluralMappedName),
                    new XAttribute("use_default_filtration", useDefaultFiltration)

                );


            var relatedCounts = new Dictionary<string, int>();
            var dv = new DataView(Controller.FieldsTable) {RowFilter = "content_id = " + id};
            if (useDb)
                dv.RowFilter += " and map_as_property = 1";
            dv.Sort = "attribute_order";
            foreach (DataRowView drv in dv)
            {
                contentElement.Add(GetFieldElement(pluralMappedName, relatedCounts, drv, useDb));
            }

            return contentElement;
        }

        private XElement GetFieldElement(string pluralMappedName, Dictionary<string, int> relatedCounts, DataRowView drv, bool useDb)
        {

            var fieldName = drv["ATTRIBUTE_NAME"].ToString();
            var fieldId = Int32.Parse(drv["ATTRIBUTE_ID"].ToString());    
            var mappedFieldName = GetMappedName(fieldName, fieldId, false);

            if (useDb)
            {
                var dbMappedName = DbConnector.GetValue(drv.Row, "NET_ATTRIBUTE_NAME", "");
                mappedFieldName = String.IsNullOrEmpty(dbMappedName) ? mappedFieldName : dbMappedName;
            }
            var fieldElement =
                new XElement("attribute",
                    new XAttribute("name", fieldName)
                );

            if (!String.IsNullOrEmpty(mappedFieldName) && !String.Equals(mappedFieldName, fieldName))
            {
                fieldElement.Add(
                    new XAttribute("mapped_name", mappedFieldName)
                );
            }
            var fieldType = GetFieldType(drv);
            if (fieldType == "O2M")
            {
                var relCount = RegisterHit(relatedCounts, GetRelatedContentId(drv).ToString());
                var mappedBackFieldName = relCount > 1 ? $"{pluralMappedName}_{relCount}" : pluralMappedName;

                if (useDb)
                {
                    var dbBackMappedName = DbConnector.GetValue(drv.Row, "NET_BACK_ATTRIBUTE_NAME", "");
                    mappedBackFieldName = String.IsNullOrEmpty(dbBackMappedName) ? mappedBackFieldName : dbBackMappedName;
                }

                fieldElement.Add(
                    new XAttribute("mapped_back_name", mappedBackFieldName)
                );
            }
            return fieldElement;
        }

        private static int RegisterHit(Dictionary<string, int> relatedCounts, string key)
        {
            int relCount;
            if (!relatedCounts.ContainsKey(key))
            {
                relCount = 1;
                relatedCounts.Add(key, relCount);

            }
            else
            {
                relCount = relatedCounts[key] + 1;
                relatedCounts[key] = relCount;
            }
            return relCount;
        }


        private static object AttributeWithDefault(XElement elem, string name, object defaultValue)
        {
            var attr = elem.Attribute(name);
            return attr?.Value ?? defaultValue;
        }

        private void ProcessRootNode(XElement schema)
        {
            var generatedNamespace = AttributeWithDefault(schema, "namespace", DBNull.Value);
            var generatedClass = AttributeWithDefault(schema, "class", "QPDataContext");
            var cnnStringName = AttributeWithDefault(schema, "connectionStringName", "qp_database");
            var replaceUrls = String.Equals(AttributeWithDefault(schema, "replaceUrls", "false").ToString(), "true");
            var useLongUrls = String.Equals(AttributeWithDefault(schema, "useLongUrls", "false").ToString(), "true");
            ImportMappedRoot(Controller.SiteId, generatedNamespace, replaceUrls, useLongUrls, cnnStringName, generatedClass);
        }

        private void ProcessLinkNode(XElement link)
        {
            var xAttribute = link.Attribute("id");
            if (xAttribute != null)
            {
                var linkId = Decimal.Parse(xAttribute.Value);
                var mappedName = AttributeWithDefault(link, "mapped_name", DBNull.Value);
                var mappedPluralName = AttributeWithDefault(link, "plural_mapped_name", DBNull.Value);
                ImportMappedLink(linkId, mappedName, mappedPluralName);
            }
        }

        private void ProcessFieldNode(decimal contentId, XElement field)
        {
            var fieldName = field.Attribute("name")?.Value;
            var mappedFieldName = AttributeWithDefault(field, "mapped_name", DBNull.Value);
            var mappedBackFieldName = AttributeWithDefault(field, "mapped_back_name", DBNull.Value);
            ImportMappedField(contentId, fieldName, mappedFieldName, mappedBackFieldName);
        }

        private decimal ProcessContentNode(XElement content)
        {
            var xAttribute = content.Attribute("id");
            var contentId = xAttribute == null ? 0 : Decimal.Parse(xAttribute.Value);
            var mappedName = AttributeWithDefault(content, "mapped_name", DBNull.Value);
            var mappedPluralName = AttributeWithDefault(content, "plural_mapped_name", DBNull.Value);
            ImportMappedContent(contentId, mappedName, mappedPluralName);
            return contentId;

        }

        private void ImportMappedContent(decimal contentId, object mappedName, object mappedPluralName)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "update content set map_as_class = 1, use_default_filtration = 1, net_content_name = @mapped_name, net_plural_content_name = @mapped_plural_name where content_id = @content_id";
                cmd.Parameters.Add("@mapped_name", SqlDbType.NVarChar, 255).Value = mappedName;
                cmd.Parameters.Add("@mapped_plural_name", SqlDbType.NVarChar, 255).Value = mappedPluralName;
                cmd.Parameters.Add("@content_id", SqlDbType.Decimal).Value = contentId;
                Controller.Cnn.ExecuteCmd(cmd);
            }
        }

        private void ImportMappedField(decimal contentId, string fieldName, object mappedFieldName, object mappedBackFieldName)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "update content_attribute set map_as_property = 1, net_attribute_name = @mapped_name, net_back_attribute_name = @mapped_back_name where content_id = @content_id and attribute_name = @attribute_name";
                cmd.Parameters.Add("@mapped_name", SqlDbType.NVarChar, 255).Value = mappedFieldName;
                cmd.Parameters.Add("@mapped_back_name", SqlDbType.NVarChar, 255).Value = mappedBackFieldName;
                cmd.Parameters.Add("@content_id", SqlDbType.Decimal).Value = contentId;
                cmd.Parameters.Add("@attribute_name", SqlDbType.NVarChar, 255).Value = fieldName;
                Controller.Cnn.ExecuteCmd(cmd);
            }
        }

        private void ImportMappedLink(decimal linkId, object mappedName, object mappedPluralName)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "update content_to_content set map_as_class = 1, net_link_name = @mapped_name, net_plural_link_name = @mapped_plural_name where link_id = @link_id";
                cmd.Parameters.Add("@mapped_name", SqlDbType.NVarChar, 255).Value = mappedName;
                cmd.Parameters.Add("@mapped_plural_name", SqlDbType.NVarChar, 255).Value = mappedPluralName;
                cmd.Parameters.Add("@link_id", SqlDbType.Decimal).Value = linkId;
                Controller.Cnn.ExecuteCmd(cmd);
            }
        }

        private void ImportMappedRoot(decimal siteId, object generatedNamespace, bool replaceUrls, bool useLongUrls, object cnnStringName, object generatedClass)
        {
            using (var cmd = new SqlCommand())
            {
                cmd.CommandText = "update site set import_mapping_to_db = 0, proceed_mapping_with_db = 1, replace_urls = @replace_urls, use_long_urls = @use_long_urls, namespace = @namespace, context_class_name = @context_class_name, connection_string_name = @connection_string_name where site_id = @site_id";
                cmd.Parameters.Add("@replace_urls", SqlDbType.Bit).Value = replaceUrls;
                cmd.Parameters.Add("@use_long_urls", SqlDbType.Bit).Value = useLongUrls;
                cmd.Parameters.Add("@namespace", SqlDbType.NVarChar, 255).Value = generatedNamespace;
                cmd.Parameters.Add("@context_class_name", SqlDbType.NVarChar, 255).Value = generatedClass;
                cmd.Parameters.Add("@connection_string_name", SqlDbType.NVarChar, 255).Value = cnnStringName;
                cmd.Parameters.Add("@site_id", SqlDbType.Decimal).Value = siteId;
                Controller.Cnn.ExecuteCmd(cmd);
            }
        }


        #endregion


        public void ResolveMapping(string source, string destination) //helper.UsableMappingXmlFileName //helper.Mapping2XmlFileName
        {
            if (!File.Exists(source))
                throw new FileNotFoundException($"Cannot find mapping file {source}");
            var doc = XDocument.Load(source);

            SetRootParams(doc, Controller);
            CreateEmptyMappedNames(doc);
            SetFieldParams(doc);
            CorrectLinks(doc);
            RemoveEmptyRelations(doc);
            AppendStatuses(doc);

            doc.Save(destination);
        }

        public void GenerateMainMapping(FileNameHelper helper)
        {
            var info = SchemaInfo.Create(Controller.SiteRow);
            var contentView = new DataView(Controller.ContentsTable);
            var contentToContentView = new DataView(Controller.ContentToContentTable);
            CreateMapping(false, info, contentView, contentToContentView, helper);
            if (Controller.ProceedMappingWithDb)
            {
                contentView.RowFilter = "map_as_class = 1";
                contentToContentView.RowFilter = "map_as_class = 1";
                CreateMapping(true, info, contentView, contentToContentView, helper);
            }
            ResolveMapping(helper.UsableMappingXmlFileName, helper.MappingResultXmlFileName);
        }

        public void GeneratePartialMapping(FileNameHelper helper, ContextClassInfo info)
        {
            var schemaInfo = SchemaInfo.Create(Controller.SiteRow);
            schemaInfo.IsPartial = true;
            if (!String.IsNullOrEmpty(info.NamespaceName))
                schemaInfo.NamespaceName = info.NamespaceName;
            schemaInfo.ClassName = info.ClassName;
            var contentView = new DataView(Controller.ContentsTable)
            {
                RowFilter = $"add_context_class_name = '{info.FullClassName}'"
            };
            var contentToContentView = new DataView(Controller.ContentToContentTable) {RowFilter = "map_as_class = 1"};
            CreateMapping(true, schemaInfo, contentView, contentToContentView, helper);
            ResolveMapping(helper.UsableMappingXmlFileName, helper.MappingResultXmlFileName);
        }

        public void CreateMapping(bool useDb, SchemaInfo info, DataView contentView, DataView contentToContentView, FileNameHelper helper)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                GetSchemaElement(info)
            );

            foreach (DataRowView drv in contentView)
            {
                AppendToSchema(doc, GetContentElement(drv.Row, useDb));
            }

            var equalCounts = new Dictionary<string, int>();
            foreach (DataRowView drv in contentToContentView)
            {
                AppendToSchema(doc, GetLinkElement(drv.Row, equalCounts, useDb));
            }

            doc.Save(useDb ? helper.MappingXmlFileName : helper.OldGeneratedMappingXmlFileName);
        }

        public void ImportMapping(FileNameHelper helper)
        {

            ResolveMapping(helper.ImportedMappingXmlFileName, helper.OldMappingResultXmlFileName);

            var doc = XDocument.Load(helper.OldMappingResultXmlFileName);

            ProcessRootNode(doc.Root);

            if (doc.Root != null)
            {
                foreach (var content in doc.Root.Descendants("content"))
                {
                    var contentId = ProcessContentNode(content);

                    foreach (var field in content.Descendants("attribute"))
                    {
                        ProcessFieldNode(contentId, field);
                    }
                }
                foreach (var link in doc.Root.Descendants("link"))
                {
                    ProcessLinkNode(link);
                }
            }

            File.Delete(helper.OldMappingResultXmlFileName);
        }
    }
}
