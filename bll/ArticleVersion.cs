using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Merger;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class ArticleVersion : EntityObject
    {
        public ArticleVersion()
        {
            _versionRowData = new Lazy<DataRow>(() => ArticleVersionRepository.GetData(Id, ArticleId));
        }

        private Dictionary<string, FieldValue> _fieldHash;
        private Dictionary<int, Dictionary<string, FieldValue>> _aggregatedArticleHash;
        private List<FieldValue> _fieldValues;
        private PathInfo _pathInfo;

        public List<Article> AggregatedArticles { get; private set; } = new List<Article>();

        public List<FieldValue> LoadFieldValues(bool forArticle = false)
        {
            if (Article == null)
            {
                return null;
            }

            List<FieldValue> result;
            if (Id == CurrentVersionId)
            {
                result = Article.FieldValues;
            }
            else
            {
                if (_versionRowData.Value == null)
                {
                    throw new Exception(string.Format(ArticleStrings.ArticleVersionNotFoundForArticle, Id, ArticleId));
                }

                var fields = FieldRepository.GetFullList(Article.DisplayContentId);
                result = Article.GetFieldValues(_versionRowData.Value, fields, Article, Id);
            }

            ProcessFieldValues(result, forArticle);
            LoadAggregateArticles(result, forArticle);
            _fieldValues = result;

            return result;
        }

        private void LoadAggregateArticles(IEnumerable<FieldValue> result, bool forArticle)
        {
            if (Id == CurrentVersionId)
            {
                AggregatedArticles = Article.AggregatedArticles.ToList();
            }
            else
            {
                foreach (var item in result.Where(n => n.Field.IsClassifier))
                {
                    if (!string.IsNullOrEmpty(item.Value) && int.TryParse(item.Value, out var contentId))
                    {
                        var id = Article.AggregatedArticles.Where(n => n.ContentId == contentId).Select(n => n.Id).SingleOrDefault();
                        var aggArticle = new Article(ContentRepository.GetById(contentId)) { Id = id };
                        aggArticle.FieldValues = Article.GetFieldValues(_versionRowData.Value, aggArticle.Content.Fields, aggArticle, Id, aggArticle.Content.Name);
                        ProcessFieldValues(aggArticle.FieldValues, forArticle);
                        AggregatedArticles.Add(aggArticle);
                    }
                }
            }
        }

        public Content GetAggregatedContent(string value) => GetAggregatedArticle(Converter.ToInt32(value, 0))?.Content;

        public Article GetAggregatedArticle(int contentId)
        {
            return AggregatedArticles.SingleOrDefault(n => n.ContentId == contentId);
        }

        private void ProcessFieldValues(IEnumerable<FieldValue> result, bool forArticle)
        {
            if (!forArticle)
            {
                foreach (var item in result)
                {
                    if (item.Field.IsDateTime && !string.IsNullOrEmpty(item.Value))
                    {
                        item.Value = Converter.ToDbDateTimeString(item.Value);
                    }
                    item.Version = this;
                }
            }
        }

        private Dictionary<string, FieldValue> GetFieldHash()
        {
            return FieldValues.ToDictionary(value => value.Field.Name);
        }

        private Dictionary<int, Dictionary<string, FieldValue>> GetAggregatedArticleHash()
        {
            return AggregatedArticles.ToDictionary(n => n.ContentId, m => m.FieldValues.ToDictionary(value => value.Field.Name));
        }

        /// <summary>
        /// Вспомогательная структура для слияния
        /// </summary>
        [ScriptIgnore]
        internal Dictionary<string, FieldValue> FieldHash => _fieldHash ?? (_fieldHash = GetFieldHash());

        /// <summary>
        /// Вспомогательная структура для слияния
        /// </summary>
        [ScriptIgnore]
        internal Dictionary<int, Dictionary<string, FieldValue>> AggregatedArticlesHash => _aggregatedArticleHash ?? (_aggregatedArticleHash = GetAggregatedArticleHash());

        /// <summary>
        /// Траслирует SortExpression из Presentation в BLL
        /// </summary>
        /// <param name="sortExpression">SortExpression</param>
        /// <returns>SortExpression</returns>
        public new static string TranslateSortExpression(string sortExpression)
        {
            var result = EntityObject.TranslateSortExpression(sortExpression);
            var replaces = new Dictionary<string, string>
            {
                { "Name", "Id" }
            };

            return TranslateHelper.TranslateSortExpression(result, replaces);
        }

        /// <summary>
        /// Осуществляет слияние с указанной версией для реализации сравнения
        /// </summary>
        /// <param name="versionToMerge">версия для слияния</param>
        internal void MergeToVersion(ArticleVersion versionToMerge)
        {
            VersionToMerge = versionToMerge;
            foreach (var item in FieldValues)
            {
                MergeValue(item, versionToMerge.FieldHash[item.Field.Name].Value);
            }

            foreach (var aggArticle in AggregatedArticles)
            {
                if (!versionToMerge.AggregatedArticlesHash.ContainsKey(aggArticle.ContentId))
                {
                    continue;
                }

                foreach (var item in aggArticle.FieldValues)
                {
                    MergeValue(item, versionToMerge.AggregatedArticlesHash[aggArticle.ContentId][item.Field.Name].Value);
                }
            }
        }

        private static void MergeValue(FieldValue item, string valueToMerge)
        {
            if (item.Field.Type.Name == FieldTypeName.Relation || item.Field.Type.Name == FieldTypeName.M2ORelation || item.Field.IsDateTime || item.Field.IsClassifier)
            {
                item.ValueToMerge = valueToMerge;
            }
            else
            {
                item.Value = Merge(Formatter.ProtectHtml(item.Value), Formatter.ProtectHtml(valueToMerge));
            }
        }

        /// <summary>
        /// Фальшивый идентификатор для текущей версии
        /// </summary>
        public static readonly int CurrentVersionId = 1;

        /// <summary>
        /// Подпапка для версий
        /// </summary>
        public static readonly string RootFolder = "_qp7_article_files_versions";

        public int ArticleId { get; set; }

        public Article Article { get; set; }

        /// <summary>
        /// Версия, с которой происходит сравнение
        /// </summary>
        public ArticleVersion VersionToMerge { get; set; }

        public int CreatedBy { get; set; }

        public virtual User CreatedByUser { get; set; }

        /// <summary>
        /// ID версии, с которой происходит сравнение
        /// </summary>
        public int MergedId => VersionToMerge?.Id ?? 0;

        /// <summary>
        /// Имя версии (используется в табличном режиме)
        /// </summary>
        public override string Name => Id.ToString();

        /// <summary>
        /// Имя версии (используется в режиме просмотра)
        /// </summary>
        [LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
        public string ExpandedName => Id == CurrentVersionId ? ArticleStrings.CurrentVersion : string.Format(ArticleStrings.VersionN, Id);

        /// <summary>
        /// Поля данных версии
        /// </summary>
        [ScriptIgnore]
        public List<FieldValue> FieldValues
        {
            get => _fieldValues ?? LoadFieldValues();
            set => _fieldValues = value;
        }

        private readonly Lazy<DataRow> _versionRowData;

        //[ScriptIgnore]
        //public DataRow VersionRowData { get { return versionRowData.Value; } }

        /// <summary>
        /// Библиотека версии
        /// </summary>
        public override PathInfo PathInfo => _pathInfo ?? (_pathInfo = Article.GetVersionPathInfo(Id));

        /// <summary>
        /// Осуществляет слияние 2 строк с помощью QA_Merger
        /// </summary>
        /// <param name="s1">строка 1</param>
        /// <param name="s2">строка 2</param>
        /// <returns>результат слияния</returns>
        public static string Merge(string s1, string s2)
        {
            const string prefix = "<html><body>";
            const string suffix = "</body></html>";
            const string mergeFormat = "{0}{1}{2}";
            var mergeProcessor = new MergeProcessor(string.Format(mergeFormat, prefix, s1, suffix), string.Format(mergeFormat, prefix, s2, suffix));
            var result = mergeProcessor.Merge();
            return result.Replace(prefix, "").Replace(suffix, "");
        }

        /// <summary>
        /// Осуществляет слияние 2х коллекций связанных сущностей
        /// </summary>
        public static string MergeRelation(IEnumerable<ListItem> titles1, IEnumerable<ListItem> titles2)
        {
            IEqualityComparer<ListItem> comparer = new LambdaEqualityComparer<ListItem>((x, y) => x.Value.Equals(y.Value), x => x.Value.GetHashCode());

            var titlesArr1 = titles1?.ToArray() ?? new ListItem[0];
            var titlesArr2 = titles2?.ToArray() ?? new ListItem[0];
            var same = titlesArr2.Intersect(titlesArr1, comparer).Select(i => new { id = i.Value, title = $"(#{i.Value}) - {i.Text}" });
            var removed = titlesArr2.Except(titlesArr1, comparer).Select(i => new { id = i.Value, title = $"<span style='text-decoration: line-through; color: red'>(#{i.Value}) - {i.Text}</span>" });
            var added = titlesArr1.Except(titlesArr2, comparer).Select(i => new { id = i.Value, title = $"<span style='background: #FFFF4D'>(#{i.Value}) - {i.Text}</span>" });
            var result = same.Concat(removed).Concat(added).OrderBy(i => i.id).Select(i => i.title);

            return string.Join("<br />", result);
        }
    }
}
