using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Repository.Articles;
using System.Runtime.Serialization.Formatters.Binary;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import.Csv
{
    public class CsvReader
    {
        private int contentId;
        private int siteId;
        private ImportSettings importSetts;
        private List<string> titleHeaders;
        private List<Field> baseFields;
        private List<Field> classifierFields;
        private Dictionary<int, List<Field>> fieldsMap;
        private Dictionary<Field, List<Article>> exstensionArticlesMap;
        private Dictionary<Field, int> headersMap;
        private Dictionary<int, Content> aggregatedContentsMap;
        private ExstendedArticleList articlesListFromCsv;
        private List<string> uniqueValuesList;
        private IEnumerable<Line> csvLines;
        private FileReader reader;

        public CsvReader(int siteId, int contentId, ImportSettings setts)
        {
            this.contentId = contentId;
            this.siteId = siteId;
            this.importSetts = setts;
            reader = new FileReader(setts);
        }

        public void Process(int step, int itemsPerStep, out int processedItemsCount)
        {
            csvLines = GetLinesFromFile(step, itemsPerStep);
            titleHeaders = MultistepActionHelper.GetFileFields(importSetts, reader);
            InitFields();
            ConvertCsvLinesToArticles();
            WriteArticlesToDb();
            processedItemsCount = csvLines.Count();
        }

        public int ArticleCount
        {
            get
            {
                return reader.Lines.Where(s => !s.Skip).Count();
            }
        }

        public string LastProcessed
        {
            get
            {

                if (csvLines == null || !csvLines.Any())
                    return @"N/A";
                else
                {
                    int num = csvLines.First().Number - 1;
                    if (num == 0 || num == 1 && reader.Lines.First().Skip || num == 2 && reader.Lines.First().Skip && reader.Lines.Skip(1).First().Skip)
                        return @"N/A";
                    else
                        return num.ToString();
                }
            }
        }

        private void WriteArticlesToDb()
        {
            switch (this.importSetts.ImportAction)
            {
                case (int)ImportActions.InsertAll:
                    InsertArticles(this.articlesListFromCsv);
                    break;
                case (int)ImportActions.InsertAndUpdate:
                    var existingArticles = UpdateArticles(this.articlesListFromCsv);
                    SaveUpdatedArticleIdsToSettings(existingArticles.GetBaseArticleIds());
                    var nonExistingArticles = articlesListFromCsv.Filter(a => !existingArticles.GetBaseArticleIds().Contains<int>(a.Id));
                    if (nonExistingArticles.Count > 0)
                    {
                        InsertArticles(nonExistingArticles);
                        SaveInsertedArticleIdsToSettings(nonExistingArticles.GetBaseArticleIds());
                    }
                    break;
                case (int)ImportActions.Update:
                    UpdateArticles(this.articlesListFromCsv);
                    break;
                case (int)ImportActions.UpdateIfChanged:
                    var changedArticles = articlesListFromCsv.Filter(a => a.Created == DateTime.MinValue);
                    UpdateArticles(changedArticles);
                    SaveUpdatedArticleIdsToSettings(changedArticles.GetBaseArticleIds());
                    break;
                case (int)ImportActions.InsertNew:
                    var nonExistArticles = GetNonExistingArticles(articlesListFromCsv);
                    if (nonExistArticles.Count > 0)
                    {
                        InsertArticles(nonExistArticles);
                        SaveInsertedArticleIdsToSettings(nonExistArticles.GetBaseArticleIds());
                    }
                    break;
                default:
                    InsertArticles(this.articlesListFromCsv);
                    break;
            }
        }

        #region ConvertCsvLinesToArticles
        private void ConvertCsvLinesToArticles()
        {
            foreach (var line in csvLines)
            {
                if (String.IsNullOrEmpty(line.Value))
                    continue;
                //Parsing line to get field values
                string[] fieldValues = this.SplitToValues(titleHeaders.Count, line.Value);

                //InitializeArticle article with default values

                Article baseArticle = InitializeArticle(contentId);
                ReadLineFields(baseArticle, fieldValues, contentId, line.Number);

                var article = new ExstendedArticle(baseArticle);

                foreach (var fv in article.BaseArticle.FieldValues)
                {
                    if (fv.Field.IsClassifier)
                    {
                        int classifierContentId;

                        if (int.TryParse(fv.Value, out classifierContentId))
                        {
                            Article exstensionArticle = InitializeArticle(classifierContentId);
                            ReadLineFields(exstensionArticle, fieldValues, classifierContentId, line.Number);

                            var content = aggregatedContentsMap[classifierContentId];
                            var field = content.Fields.First(f => f.Aggregated);
                            exstensionArticle.FieldValues.Add(new FieldValue() { Field = field });
                            article.Exstensions[fv.Field] = exstensionArticle;
                            articlesListFromCsv.ExstensionFields.Add(fv.Field);
                        }
                        else
                        {
                            article.Exstensions[fv.Field] = null;
                        }
                    }
                }

                articlesListFromCsv.Add(article);
            }
        }

        private void ReadLineFields(Article article, string[] fieldValues, int contentId, int lineNumber)
        {
            List<Field> fields;

            if (fieldsMap.TryGetValue(contentId, out fields))
            {
                foreach (var field in fields)
                {
                    int titleIndex = headersMap[field];

                    if (titleIndex == -1)
                    {
                        //Column with a title doesnt exist in first line of file or client didnt map the field, so skip it
                        continue;
                    }

                    string value = PrepareValue(fieldValues[titleIndex]);
                    FieldValue fieldDbValue = new FieldValue { Field = field };

                    if (!IsEmpty(value))
                    {
                        try
                        {
                            FormatFieldValue(field, value, ref fieldDbValue);
                        }
                        catch (FormatException ex)
                        {
                            throw new FormatException(String.Format(ImportStrings.ErrorInColumn, lineNumber, field.Name, ex.Message), ex);
                        }
                    }
                    else if (fieldDbValue.Field.Required)
                        throw new FormatException(String.Format(ImportStrings.ErrorInRequiredColumn, lineNumber, field.Name, fieldDbValue.Field.Name));

                    article.FieldValues.Add(fieldDbValue);
                }
            }

            // Adding values of columns like content_item_id, ischanged, etc.
            ReadAdditionalFields(ref article, fieldValues);
        }

        private static bool IsEmpty(string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return true;
            else if (value == "NULL")
                return true;
            else if (value.Length >= 2 && value.First() == '"' && value.Last() == '"' && (value.Length == 2 || String.IsNullOrWhiteSpace(value.Substring(1, value.Length - 2))))
                return true;
            else
                return false;
        }

        private string PrepareValue(string inittialValue)
        {
            string value = inittialValue.Replace("\"\"", "\"");
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                value = value.Remove(0, 1);
                if (value.Length > 0)
                    value = value.Remove(value.Length - 1);
            }
            return value;
        }

        private void ReadAdditionalFields(ref Article article, string[] fieldValues)
        {
            if (importSetts.ImportAction != (int)ImportActions.InsertAll)
                ReadUniqueField(ref article, fieldValues);

            if (importSetts.ImportAction == (int)ImportActions.UpdateIfChanged)
                ReadChangedStatus(ref article, fieldValues);
        }

        private void ReadChangedStatus(ref Article article, string[] fieldValues)
        {
            int isChangedindex = GetFieldIndex(ArticleStrings.IsChanged);

            if (isChangedindex == -1)
            {
                throw new ArgumentException(String.Format("There is no column {0} in the specified file", ArticleStrings.IsChanged));
            }

            if (fieldValues[isChangedindex] == "1")
            {
                article.Created = DateTime.MinValue;
            }
            else
            {
                article.Created = DateTime.Now;
            }
        }

        private void ReadUniqueField(ref Article article, string[] fieldValues)
        {
            string key = null;

            if (article.ContentId == contentId)
            {
                key = importSetts.UniqueFieldToUpdate;
            }
            else
            {
                if (importSetts.UniqueAggregatedFieldsToUpdate.ContainsKey(article.ContentId))
                {
                    key = importSetts.UniqueAggregatedFieldsToUpdate[article.ContentId];
                }
            }

            int uindex = GetFieldIndex(key);
            if (uindex != -1)
            {
                if (importSetts.UniqueContentField == null || article.ContentId != contentId)
                {

                    int dbId = 0;
                    //if value is not empty and doesnt contain id then throw an exception. If its empty, just skip it in order to save later
                    if (!String.IsNullOrEmpty(fieldValues[uindex]) && !Int32.TryParse(fieldValues[uindex], out dbId))
                    {
                        throw new ArgumentException(ImportStrings.InvalidIdentity);
                    }
                    if (dbId != 0)
                    {
                        article.Id = dbId;
                    }
                }
                else
                {
                    string value = PrepareValue(fieldValues[uindex]);

                    if (value != "NULL" && !String.IsNullOrEmpty(value) && value != "\"\"")
                    {
                        FieldValue fieldDbValue = new FieldValue { Field = importSetts.UniqueContentField };
                        FormatFieldValue(importSetts.UniqueContentField, value, ref fieldDbValue);
                        uniqueValuesList.Add(fieldDbValue.Value);
                    }
                    else
                    {
                        uniqueValuesList.Add(null);
                    }
                }
            }
        }

        private int GetFieldIndex(string fieldName)
        {
            int fieldIndex = titleHeaders.IndexOf(fieldName);

            if (fieldName == "-1")
            {
                throw new ArgumentException(ImportStrings.UniqueNotSpecified);
            }
            return fieldIndex;
        }

        private void FormatFieldValue(Field field, string value, ref FieldValue fieldDbValue)
        {
            switch (field.ExactType)
            {
                case FieldExactTypes.Numeric:
                    fieldDbValue.Value = MultistepActionHelper.NumericCultureFormat(value, this.importSetts.Culture, "en-US");
                    break;
                case FieldExactTypes.Date:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(value, this.importSetts.Culture, "en-US");
                    break;
                case FieldExactTypes.Time:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(value, this.importSetts.Culture, "en-US");
                    break;
                case FieldExactTypes.DateTime:
                    fieldDbValue.Value = MultistepActionHelper.DateCultureFormat(value, this.importSetts.Culture, "en-US");
                    break;
                case FieldExactTypes.O2MRelation:
                    fieldDbValue.Value = MultistepActionHelper.O2MFormat(value);
                    break;
                case FieldExactTypes.M2MRelation:
                    fieldDbValue.NewRelatedItems = MultistepActionHelper.M2MFormat(value).ToArray();
                    fieldDbValue.Value = field.LinkId.Value.ToString();
                    break;
                case FieldExactTypes.M2ORelation:
                    break;
                default:
                    fieldDbValue.Value = value;
                    break;
            }
        }

        #endregion

        #region SaveAndUpdate articles methods
        private ExstendedArticleList GetExistingArticles(ExstendedArticleList articlesList)
        {
            return GetArticles(articlesList, true);
        }

        private ExstendedArticleList GetNonExistingArticles(ExstendedArticleList articlesList)
        {
            return GetArticles(articlesList, false);
        }

        private ExstendedArticleList GetArticles(ExstendedArticleList articlesList, bool onlyExisting)
        {
            ExstendedArticleList existingArticles;
            Field uniqueField = importSetts.UniqueContentField;

            if (uniqueField == null)
            {
                List<int> existingIds = GetExistingArticleIds(articlesList.GetBaseArticleIds());
                existingArticles = articlesList.Filter(a => !onlyExisting ^ existingIds.Contains(a.Id));
            }
            else
            {
                existingArticles = new ExstendedArticleList(articlesList);
                var existingIdsMap = GetExistingArticleIdsMap(uniqueValuesList, uniqueField.Name);

                for (int i = 0; i < articlesList.Count; i++)
                {
                    var article = articlesList[i];
                    var uniqueValue = uniqueValuesList[i];
                    bool articleExists = existingIdsMap.ContainsKey(uniqueValue);

                    if (articleExists)
                    {
                        article.BaseArticle.Id = existingIdsMap[uniqueValue];
                    }

                    if (!onlyExisting ^ articleExists)
                    {
                        existingArticles.Add(article);
                    }
                }
            }

            return existingArticles;
        }

        private string GetValue(Article article, string fieldName)
        {
            return article.FieldValues.Find(fv => fv.Field.Name == fieldName).Value;
        }

        // Добавляет статьи
        private void InsertArticles(ExstendedArticleList articleList)
        {
            List<Article> baseArticles = articleList.GetBaseArticles();
            var idsList = InsertArticlesIds(baseArticles);
            InsertArticleValues(idsList.ToArray(), baseArticles);

            if (this.importSetts.ContainsO2MRelationOrM2MRelationFields)
            {
                SaveNewRelationsToFile(baseArticles, idsList);
            }

            for (int i = 0; i < idsList.Count; i++)
            {
                int id = idsList[i];
                var article = articleList[i];

                foreach (var aggregatedArticle in article.Exstensions.Values)
                {
                    var parent = aggregatedArticle.FieldValues.Find(fv => fv.Field.Aggregated);
                    parent.Value = id.ToString();
                }
            }

            var aggregatedArticles = articleList.GetAllAggregatedArticles().ToList();

            foreach (var aggregatedArticleList in aggregatedArticles)
            {
                var aggregatedIdsList = InsertArticlesIds(aggregatedArticleList);
                InsertArticleValues(aggregatedIdsList.ToArray(), aggregatedArticleList);

                if (this.importSetts.ContainsO2MRelationOrM2MRelationFields)
                {
                    SaveNewRelationsToFile(aggregatedArticleList, aggregatedIdsList);
                }
            }
        }

        //Обновление статей
        private ExstendedArticleList UpdateArticles(ExstendedArticleList articlesList)
        {
            var existingArticles = GetExistingArticles(articlesList);
            var exstensionsMap = ContentRepository.GetAggregatedArticleIdsMap(contentId, existingArticles.GetBaseArticleIds().ToArray());
            InsertArticleValues(existingArticles.GetBaseArticleIds().ToArray(), existingArticles.GetBaseArticles(), updateArticles: true);

            var idsToDelete = new List<int>();
            var articlesToInsert = new List<Article>();
            var idsToUpdate = new List<int>();
            var articlesToUpdate = new List<Article>();

            foreach (var article in existingArticles)
            {
                foreach (var afv in article.Exstensions)
                {
                    var aggregatedArticle = afv.Value;
                    int fieldId = afv.Key.Id;

                    if (exstensionsMap.ContainsKey(article.BaseArticle.Id) && exstensionsMap[article.BaseArticle.Id].ContainsKey(fieldId))
                    {
                        int currentId = exstensionsMap[article.BaseArticle.Id][fieldId];

                        if (currentId == aggregatedArticle.Id)
                        {
                            idsToUpdate.Add(aggregatedArticle.Id);
                            articlesToUpdate.Add(aggregatedArticle);
                        }
                        else
                        {
                            aggregatedArticle.Id = currentId;
                            idsToDelete.Add(aggregatedArticle.Id);
                            articlesToInsert.Add(aggregatedArticle);
                        }
                    }
                    else
                    {
                        articlesToInsert.Add(aggregatedArticle);
                    }

                    var parent = aggregatedArticle.FieldValues.Find(fv => fv.Field.Aggregated);
                    parent.Value = article.BaseArticle.Id.ToString();
                }
            }

            if (idsToDelete.Any())
            {
                ArticleRepository.MultipleDelete(idsToDelete);
            }

            if (articlesToInsert.Any())
            {
                var inserdedIds = InsertArticlesIds(articlesToInsert);

                if (this.importSetts.ContainsO2MRelationOrM2MRelationFields)
                {
                    SaveNewRelationsToFile(articlesToInsert, inserdedIds);
                }

                idsToUpdate.AddRange(inserdedIds);
                articlesToUpdate.AddRange(articlesToInsert);
            }

            InsertArticleValues(idsToUpdate.ToArray(), articlesToUpdate, updateArticles: true);
            return existingArticles;
        }

        private static void ProcessSecurityCheckResult(Dictionary<int, bool> relCheckResult, string messageText)
        {
            var relIdsString = string.Join(",", relCheckResult.Where(n => !n.Value).Select(n => n.Key));
            if (!string.IsNullOrEmpty(relIdsString))
            {
                throw new ArgumentException(string.Format(messageText, relIdsString));
            }
        }

        private List<int> GetAggegatedIds(Article article)
        {
            return ArticleRepository.LoadAggregatedArticles(article).Select(a => a.Id).ToList();
        }

        // Добавление статей (id)
        private List<int> InsertArticlesIds(IList<Article> articleList)
        {
            string insertTemplate = @"SELECT {0}, {1}, {2}, {3} {4}";
            StringBuilder query = new StringBuilder();
            string unionAll = " UNION ALL ";
            int i = 1;
            foreach (Article article in articleList)
            {
                if (i == articleList.Count())
                {
                    unionAll = String.Empty;
                }
                int visible = (article.Visible) ? 1 : 0;
                query.AppendFormat(insertTemplate, visible, article.StatusTypeId, article.ContentId, QPContext.CurrentUserId, unionAll);
                i++;
            }
            string result = query.ToString();
            return ArticleRepository.InsertArticleIds(result);
        }

        //Добавление значений полей статей
        private void InsertArticleValues(int[] idsList, IList<Article> articleList, bool updateArticles = false)
        {
            if (updateArticles)
            {
                UpdateArticlesDateTime(idsList);
            }
            var doc = new XDocument();
            var o2MDoc = new XDocument();
            var parametersXml = new XElement("PARAMETERS");
            doc.Add(parametersXml);
            o2MDoc.Add(parametersXml);

            var k = 0;

            var m2MValues = new List<KeyValuePair<int, FieldValue>>();
            var o2MValues = new List<KeyValuePair<int, FieldValue>>();

            foreach (int articleId in idsList)
            {
                var article = articleList[k];
                if (article != null)
                {
                    foreach (var fieldValue in article.FieldValues)
                    {
                        var fieldValueXml = GetFieldValueElement(fieldValue, articleId);

                        doc.Root.Add(fieldValueXml);

                        if (fieldValue.Field.ExactType == FieldExactTypes.M2MRelation)
                        {
                            m2MValues.Add(new KeyValuePair<int, FieldValue>(articleId, fieldValue));
                        }
                        else if (fieldValue.Field.ExactType == FieldExactTypes.O2MRelation)
                        {
                            o2MValues.Add(new KeyValuePair<int, FieldValue>(articleId, fieldValue));
                            o2MDoc.Root.Add(fieldValueXml);
                        }
                    }
                }
                k++;
            }
            ArticleRepository.ValidateO2MValues(o2MDoc.ToString(SaveOptions.None));
            ValidateO2MRelationSecurity(o2MValues);
            ArticleRepository.InsertArticleValues(doc.ToString(SaveOptions.None));
            UpdateM2MValues(m2MValues, updateArticles);
        }

        private static XElement GetFieldValueElement(FieldValue fieldValue, int articleId)
        {
            XElement fieldValueXml = new XElement("FIELDVALUE");
            string value = fieldValue.Value;
            if (fieldValue.Value == "NULL")
            {
                value = String.Empty;
            }

            fieldValueXml.Add(new XElement("CONTENT_ITEM_ID", articleId));
            fieldValueXml.Add(new XElement("ATTRIBUTE_ID", fieldValue.Field.Id));

            switch (fieldValue.Field.Type.DatabaseType)
            {
                case "NTEXT":
                    fieldValueXml.Add(new XElement("DATA", null));
                    fieldValueXml.Add(new XElement("BLOB_DATA", value));
                    break;
                case "NVARCHAR":
                default:
                    fieldValueXml.Add(new XElement("DATA", value));
                    fieldValueXml.Add(new XElement("BLOB_DATA", null));
                    break;
            }
            return fieldValueXml;
        }

        private static void ValidateO2MRelationSecurity(List<KeyValuePair<int, FieldValue>> o2mValues)
        {
            var fieldsToCheck = o2mValues.Select(n => n.Value.Field).Distinct().Where(n => n.UseRelationSecurity);
            foreach (var field in fieldsToCheck)
            {
                var ids =
                    o2mValues.Where(n => n.Value.Field == field)
                        .Select(n => n.Value.Value)
                        .Distinct()
                        .Select(s => int.Parse(s))
                        .ToArray();
                var notAccessed = new HashSet<string>
                    (
                    ArticleRepository.CheckRelationSecurity(field.RelateToContentId.Value, ids, false)
                        .Where(n => !n.Value)
                        .Select(n => n.Key.ToString())
                        .ToArray()
                    );

                if (notAccessed.Any())
                {
                    var errorItem = o2mValues.First(n => notAccessed.Contains(n.Value.Value));
                    throw new ArgumentException(String.Format(ImportStrings.InaccessibleO2M, errorItem.Key, field.Name,
                        errorItem.Value.Value));
                }
            }
        }

        #region Update M2MRelation And O2MRelation Fields
        // Добавление значений m2m и o2m полей
        public void PostUpdateM2MRelationAndO2MRelationFields()
        {
            if (this.importSetts.ContainsO2MRelationOrM2MRelationFields)
            {
                List<RelSourceDestinationValue> values = new List<RelSourceDestinationValue>();
                //get all relations between old and new article ids
                GetNewValues(ref values);

                PostUpdateM2MValues(values);
                PostUpdateO2MValues(values);
            }
        }

        // Обновление значений o2m полей
        private void PostUpdateO2MValues(List<RelSourceDestinationValue> m2mValues)
        {
            XDocument doc = new XDocument();
            XElement items = new XElement("items");
            doc.Add(items);

            foreach (RelSourceDestinationValue item in m2mValues.Where(s => !s.IsM2M))
            {
                if (item.NewRelatedItems != null)
                {
                    int oldId = item.NewRelatedItems[0];
                    var oldElem = m2mValues.FirstOrDefault(s => s.OldId == oldId);
                    if (oldElem != null && item.OldId != oldId)
                    {
                        XElement itemXML = new XElement("item");
                        itemXML.Add(new XAttribute("id", item.NewId));
                        itemXML.Add(new XAttribute("linked_id", oldElem.NewId));
                        itemXML.Add(new XAttribute("field_id", item.FieldId));
                        doc.Root.Add(itemXML);

                    }
                }
            }
            ArticleRepository.InsertO2MFieldValues(doc.ToString(SaveOptions.None));
        }

        // Обновление значений m2m полей
        private void PostUpdateM2MValues(List<RelSourceDestinationValue> m2mValues)
        {
            XDocument doc = new XDocument();
            XElement items = new XElement("items");
            doc.Add(items);

            foreach (RelSourceDestinationValue item in m2mValues.Where(s => s.IsM2M))
            {
                if (item.NewRelatedItems != null)
                {
                    List<int> result = GetM2MRelatedArtsWithNewIds(item.NewRelatedItems, m2mValues);
                    XElement itemXML = new XElement("item");
                    itemXML.Add(new XAttribute("id", item.NewId));
                    itemXML.Add(new XAttribute("linkId", item.FieldId));
                    itemXML.Add(new XAttribute("value", string.Join(",", result)));
                    doc.Root.Add(itemXML);
                }

            }
            ArticleRepository.UpdateM2MValues(doc.ToString(SaveOptions.None));
        }

        // Получение списка серверных id вместо пользовательских
        private List<int> GetM2MRelatedArtsWithNewIds(IEnumerable<int> oldRelatedIds, List<RelSourceDestinationValue> m2mValues)
        {
            List<int> result = new List<int>();
            foreach (int oldId in oldRelatedIds)
            {
                if (m2mValues.FirstOrDefault(s => s.OldId == oldId) != null)
                {
                    result.Add(m2mValues.FirstOrDefault(s => s.OldId == oldId).NewId);
                }

            }
            return result;
        }
        #endregion

        #region Insert M2M relation field values

        private void UpdateM2MValues(List<KeyValuePair<int, FieldValue>> values, bool updateArticles = false)
        {
            Dictionary<string, Field> M2MFields = new Dictionary<string, Field>();
            // Getting m2m fields
            foreach (var fieldV in values)
            {
                if (!M2MFields.Keys.Contains<string>(fieldV.Value.Field.Name))
                {
                    M2MFields.Add(fieldV.Value.Field.Name, fieldV.Value.Field);
                }
            }

            XDocument doc = new XDocument();
            XElement items = new XElement("items");
            doc.Add(items);

            foreach (var field in M2MFields.Values)
            {

                Field m2mField = field;
                string condition = m2mField.RelationCondition;
                int linkId = m2mField.LinkId.HasValue ? m2mField.LinkId.Value : 0;
                int contentId = m2mField.RelateToContentId.Value;

                //Filtering values with m2mFields
                IEnumerable<KeyValuePair<int, FieldValue>> filteredValues = values.Where(f => f.Value.Field.Name == m2mField.Name);
                List<int> relatedIds = filteredValues
                    .Where(n => n.Value.NewRelatedItems != null)
                    .SelectMany(n => n.Value.NewRelatedItems)
                    .Distinct()
                    .ToList();

                var validatedIds = new HashSet<int>(ArticleRepository.CheckForArticleExistence(relatedIds, condition, contentId));
                var grantedIds = (field.UseRelationSecurity) ?
                    new HashSet<int>(
                        ArticleRepository.CheckRelationSecurity(contentId, validatedIds.ToArray(), false)
                            .Where(n => n.Value).Select(m => m.Key)
                    ) : validatedIds;

                foreach (var item in filteredValues)
                {
                    string value = "";
                    if (item.Value.NewRelatedItems != null)
                    {
                        var notValidIds = item.Value.NewRelatedItems.Where(n => !validatedIds.Contains(n)).ToArray();
                        if (notValidIds.Any())
                            throw new ArgumentException(String.Format(ImportStrings.IncorrectM2M, item.Key, item.Value.Field.Name, String.Join(",", notValidIds)));

                        var notGrantedIds = item.Value.NewRelatedItems.Where(n => !grantedIds.Contains(n)).ToArray();
                        if (notGrantedIds.Any())
                            throw new ArgumentException(String.Format(ImportStrings.InaccessibleM2M, item.Key, item.Value.Field.Name, String.Join(",", notGrantedIds)));

                        value = string.Join(",", item.Value.NewRelatedItems);
                    }

                    XElement itemXML = new XElement("item");
                    itemXML.Add(new XAttribute("id", item.Key));
                    itemXML.Add(new XAttribute("linkId", linkId));
                    itemXML.Add(new XAttribute("value", value));
                    doc.Root.Add(itemXML);
                }
            }
            ArticleRepository.UpdateM2MValues(doc.ToString(SaveOptions.None));
        }

        #endregion

        #region File methods
        // Сериализация соответствий новых статей в файл
        private void SaveNewRelationsToFile(IList<Article> articles, IEnumerable<int> idsList)
        {
            int k = 0;
            List<RelSourceDestinationValue> m2mValues = new List<RelSourceDestinationValue>();

            // idsList - список добавленнынх id, необходимо пройти по каждому из них и обновить данные полей.
            foreach (int id in idsList)
            {
                Article art = articles[k];
                foreach (var fv in art.FieldValues)
                {
                    if (fv.Field.ExactType == FieldExactTypes.M2MRelation && fv.Field.RelateToContentId.Value == art.ContentId)
                    {
                        RelSourceDestinationValue val = new RelSourceDestinationValue()
                        {
                            NewId = id,
                            OldId = art.Id,
                            FieldId = fv.Field.LinkId.Value,
                            NewRelatedItems = fv.NewRelatedItems,
                            IsM2M = true
                        };
                        m2mValues.Add(val);
                    }
                    else if (fv.Field.ExactType == FieldExactTypes.O2MRelation && fv.Field.RelateToContentId.Value == art.ContentId)
                    {
                        RelSourceDestinationValue val = new RelSourceDestinationValue()
                        {
                            NewId = id,
                            OldId = art.Id,
                            FieldId = fv.Field.Id,
                            NewRelatedItems = (string.IsNullOrEmpty(fv.Value)) ? null : new int[1] { Int32.Parse(fv.Value) },
                            IsM2M = false
                        };

                        m2mValues.Add(val);
                    }
                }
                k++;
            }
            try
            {
                using (Stream sw = File.Open(this.importSetts.TempFileForRelFields, FileMode.Append))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(sw, m2mValues);
                }
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        // Десериализация всех соответствий (после добавления статей)
        private void GetNewValues(ref List<RelSourceDestinationValue> m2mValues)
        {
            if (!File.Exists(this.importSetts.TempFileForRelFields))
            {
                return;
            }
            try
            {
                using (Stream stream = File.Open(this.importSetts.TempFileForRelFields, FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();

                    while (stream.Position != stream.Length)
                    {
                        var vals = (List<RelSourceDestinationValue>)bin.Deserialize(stream);
                        m2mValues.AddRange(vals);
                    }
                }
            }
            catch
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #endregion

        #region Help private methods

        private void SaveUpdatedArticleIdsToSettings(IEnumerable<int> articleIds)
        {
            ImportSettings setts = HttpContext.Current.Session["ImportArticlesService.Settings"] as ImportSettings;
            setts.UpdatedArticleIds.AddRange(articleIds);
            HttpContext.Current.Session["ImportArticlesService.Settings"] = setts;
        }
        private void SaveInsertedArticleIdsToSettings(IEnumerable<int> articleIds)
        {
            ImportSettings setts = HttpContext.Current.Session["ImportArticlesService.Settings"] as ImportSettings;
            setts.InsertedArticleIds.AddRange(articleIds);
            HttpContext.Current.Session["ImportArticlesService.Settings"] = setts;
        }
        private List<int> GetExistingArticleIds(List<int> articlesIdList)
        {
            return ArticleRepository.CheckForArticleExistence(articlesIdList, String.Empty, contentId);
        }

        private Dictionary<string, int> GetExistingArticleIdsMap(List<string> values, string fieldName)
        {
            return ArticleRepository.GetExistingArticleIdsMap(values, fieldName, String.Empty, contentId);
        }

        // Обновление даты изменения статей
        private void UpdateArticlesDateTime(int[] articlesIds)
        {
            XDocument doc = new XDocument();
            XElement items = new XElement("items");
            doc.Add(items);

            foreach (int id in articlesIds)
            {
                XElement itemXML = new XElement("item");
                itemXML.Add(new XAttribute("id", id));
                itemXML.Add(new XAttribute("modifiedBy", QPContext.CurrentUserId));
                doc.Root.Add(itemXML);

            }

            ArticleRepository.UpdateArticlesDateTime(doc.ToString(SaveOptions.None));
        }

        private IEnumerable<Line> GetLinesFromFile(int step, int itemsPerStep)
        {
            return reader.Lines.Where(s => !s.Skip).Skip(step * itemsPerStep).Take(itemsPerStep);
        }

        private string[] SplitToValues(int countColumns, string line)
        {
            if (line.Split(this.importSetts.Delimiter).Count() == countColumns)
            {
                return line.Split(this.importSetts.Delimiter);
            }
            else
            {
                return ParseLine(line);
            }
        }
        private string[] ParseLine(string line)
        {
            List<string> fieldValues = new List<string>();
            char quote = '"';
            char[] lineByChar = line.ToCharArray();

            int _startIndex = 0;
            int quoteCounter = 0;
            bool isFieldQuoted = false;
            for (int i = 0; i < lineByChar.Length; i++)
            {
                quoteCounter = (lineByChar[i] == quote) ? quoteCounter + 1 : 0;

                if (lineByChar[i] == quote && (i == (lineByChar.Length - 1) || lineByChar[i + 1] != quote) && quoteCounter % 2 != 0)
                {
                    isFieldQuoted = !isFieldQuoted;
                }
                else if (lineByChar[i] == this.importSetts.Delimiter)
                {
                    if (isFieldQuoted)
                        continue;
                    else
                    {
                        fieldValues.Add(GetStringFromChars(_startIndex, i, lineByChar));
                        _startIndex = i + 1;
                    }
                }

                if (i == (lineByChar.Length - 1) && lineByChar[i] != quote && isFieldQuoted)
                {
                    throw new FormatException(String.Format(ImportStrings.LineFormatException, line));
                }

            }
            if (lineByChar.Length != _startIndex)
            {
                fieldValues.Add(GetStringFromChars(_startIndex, lineByChar.Length, lineByChar));
            }
            return fieldValues.ToArray();
        }
        private string GetStringFromChars(int startIndex, int endIndex, char[] charLine)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = startIndex; i < endIndex; i++)
            {
                sb.Append(charLine[i].ToString());
            }
            return sb.ToString();
        }
        private Article InitializeArticle(int contentId)
        {
            Article article = new Article();
            article.Status = StatusType.GetPublished(this.siteId);
            article.StatusTypeId = StatusTypeRepository.GetPublishedStatusIdBySiteId(this.siteId);
            article.Visible = true;
            article.ContentId = contentId;
            article.FieldValues = new List<FieldValue>();
            return article;
        }

        #endregion

        #region Public static methods
        public static List<string> GetFieldNames(IEnumerable<string> csvLines, char delimiter, bool noHeaders)
        {
            string firstLine = csvLines.First();
            if (!noHeaders)
            {
                if (String.IsNullOrEmpty(firstLine))
                {
                    throw new ArgumentException(ImportStrings.FirstLineEmpty);
                }
                int columnsCount = firstLine.Split(delimiter).Count();
                return firstLine.Split(delimiter).ToList();
            }
            else
            {
                int columnsCount = firstLine.Split(delimiter).Count();
                int[] columnIndexes = Enumerable.Range(0, columnsCount).ToArray();
                List<string> result = new List<string>();
                foreach (int k in columnIndexes)
                {
                    result.Add(k.ToString());
                }
                return result;
            }
        }
        #endregion

        #region extension methods
        private void InitFields()
        {
            baseFields = importSetts.FieldsList.Where(f => f.Value.ContentId == contentId).Select(f => f.Value).ToList();
            classifierFields = baseFields.Where(f => f.IsClassifier).ToList();

            fieldsMap = (from f in importSetts.FieldsList.Select(f => f.Value)
                         group f by f.ContentId into g
                         select g)
                         .ToDictionary(g => g.Key, g => g.ToList());

            exstensionArticlesMap = new Dictionary<Field, List<Article>>();

            foreach (var classifier in classifierFields)
            {
                exstensionArticlesMap[classifier] = new List<Article>();
            }

            headersMap = importSetts.FieldsList.ToDictionary(f => f.Value, f => titleHeaders.FindIndex(s => s == f.Key));

            aggregatedContentsMap = ContentRepository.GetAggregatedContents(contentId).ToDictionary(c => c.Id, c => c);

            articlesListFromCsv = new ExstendedArticleList();

            uniqueValuesList = new List<string>();
        }
        #endregion

    }

    public class FileReader
    {
        private ImportSettings setts;
        private Lazy<IEnumerable<Line>> lines;

        public FileReader(ImportSettings settings)
        {
            this.setts = settings;
            lines = new Lazy<IEnumerable<Line>>(() => ReadFile(this.setts));
        }

        public IEnumerable<Line> Lines
        {
            get
            {
                return lines.Value;
            }
        }

        public void CopyFileToTempDir()
        {
            FileInfo fileInfo = new FileInfo(HttpUtility.UrlDecode(this.setts.UploadFilePath));
            if (fileInfo.Exists)
            {
                string newFileUploadPath = String.Format("{0}\\{1}", QPConfiguration.TempDirectory, fileInfo.Name);
                if (!File.Exists(newFileUploadPath))
                    File.Copy(this.setts.UploadFilePath, newFileUploadPath, true);
            }
            else
            {
                throw new FileNotFoundException(String.Format("File {0} was not found.", this.setts.UploadFilePath));
            }
        }

        public static IEnumerable<Line> ReadFile(ImportSettings setts)
        {
            using (StreamReader reader = new StreamReader(setts.UploadFilePath))
            {
                CustomStreamReader rdr = new CustomStreamReader(reader.BaseStream, Encoding.GetEncoding(setts.Encoding), setts.LineSeparator, setts.Delimiter);
                string line = String.Empty;
                int i = 0;
                int headerNum = 1;
                while (!String.IsNullOrEmpty(line = rdr.ReadLine()))
                {
                    i++;
                    var value = line.Trim('\n', '\r');
                    bool isSep = value.StartsWith("sep=");
                    if (isSep)
                        headerNum++;
                    bool skip = (!setts.NoHeaders && i == headerNum) || String.IsNullOrEmpty(value) || isSep;
                    yield return new Line { Value = value, Number = i, Skip = skip };
                }
            }
        }

        public int RowsCount()
        {
            return Lines.Where(s => !s.Skip).Count();
        }
    }
    public class CustomStreamReader : StreamReader
    {
        private string lineDelimiter;
        private char fieldDelimiter;
        public CustomStreamReader(Stream stream, Encoding encoding, string lineDelimiter, char fieldDelimiter)
            : base(stream, encoding)
        {
            this.lineDelimiter = lineDelimiter;
            this.fieldDelimiter = fieldDelimiter;
        }
        public override Encoding CurrentEncoding
        {
            get
            {
                return base.CurrentEncoding;
            }
        }
        private bool IsEmpty(string line)
        {
            string res = line.Trim(new char[] { this.fieldDelimiter, '"', '\r', '\n' });
            if (String.IsNullOrEmpty(res) || String.IsNullOrWhiteSpace(line))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override string ReadLine()
        {
            int c;

            c = Read();
            if (c == -1)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            char lastCh = Char.MinValue;
            bool quoteOpen = false;
            do
            {
                if ((char)c == '"' && !quoteOpen)
                {
                    quoteOpen = true;
                }
                else if ((char)c == '"' && quoteOpen)
                {
                    quoteOpen = false;
                }
                char[] lineSepArr = this.lineDelimiter.ToCharArray();
                char sep = lineSepArr[0];
                if (lineSepArr.Length == 2)
                {
                    sep = lineSepArr[1];
                }

                char ch = (char)c;

                if (ch == sep && !quoteOpen && (lineSepArr.Length == 1 || (lineSepArr.Length == 2 && lastCh == lineSepArr[0])))
                {
                    if (!IsEmpty(sb.ToString()))
                    {
                        return sb.ToString();
                    }
                    else
                    {
                        sb.Remove(0, sb.Length);
                    }
                }
                else
                {
                    sb.Append(ch);
                }
                lastCh = (char)c;

            } while ((c = Read()) != -1);
            return sb.ToString();
        }
    }

    [Serializable]
    public class RelSourceDestinationValue
    {
        public int OldId { get; set; }
        public int NewId { get; set; }
        public int FieldId { get; set; }
        public int[] NewRelatedItems { get; set; }
        public bool IsM2M { get; set; }
    }

    public class ExstendedArticle
    {
        public Article BaseArticle { get; private set; }
        public Dictionary<Field, Article> Exstensions { get; private set; }

        public ExstendedArticle(Article baseArticle)
        {
            BaseArticle = baseArticle;
            Exstensions = new Dictionary<Field, Article>();
        }
    }

    public class ExstendedArticleList : List<ExstendedArticle>
    {
        public HashSet<Field> ExstensionFields { get; private set; }

        public ExstendedArticleList()
        {
            ExstensionFields = new HashSet<Field>();
        }

        public ExstendedArticleList(ExstendedArticleList articles) : base()
        {
            ExstensionFields = new HashSet<Field>(articles.ExstensionFields);
        }

        public List<Article> GetBaseArticles()
        {
            return this.Select(a => a.BaseArticle).ToList();
        }

        public List<int> GetBaseArticleIds()
        {
            return this.Select(a => a.BaseArticle.Id).ToList();
        }

        public List<Article> GetAggregatedArticles(Field field)
        {
            return this.Select(a => a.Exstensions[field]).ToList();
        }

        public IEnumerable<List<Article>> GetAllAggregatedArticles()
        {
            foreach (var field in ExstensionFields)
            {
                yield return GetAggregatedArticles(field);
            }
        }

        public ExstendedArticleList Filter(Func<Article, bool> predicate)
        {
            var result = new ExstendedArticleList(this);
            foreach (var article in this)
            {
                if (predicate(article.BaseArticle))
                {
                    result.Add(article);
                }
            }
            return result;
        }
    }

    public class Line
    {
        public int Number { get; set; }

        public string Value { get; set; }

        public bool Skip { get; set; }
    }

}
