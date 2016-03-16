using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Script.Serialization;
using System.Linq;
using Quantumart.QP8.Merger;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.Helpers;


namespace Quantumart.QP8.BLL
{
	public class ArticleVersion : EntityObject
	{
		public ArticleVersion()
		{
			versionRowData = new Lazy<DataRow>(() => ArticleVersionRepository.GetData(Id, ArticleId));
		}

		#region Private Members
		private Dictionary<string, FieldValue> _fieldHash;
		private List<FieldValue> _fieldValues;
		private PathInfo _PathInfo;

		public List<FieldValue> GetFieldValues(bool forArticle = false)
		{
			if (Article == null)
				return null;
			else
			{
				List<FieldValue> result;
				if (Id == CurrentVersionId)
					result = Article.FieldValues;
				else
				{
					if (versionRowData.Value == null) throw new Exception(String.Format(ArticleStrings.ArticleVersionNotFoundForArticle, Id, ArticleId));
					List<Field> fields = FieldRepository.GetFullList(Article.DisplayContentId);
					result = Article.GetFieldValues(versionRowData.Value, fields, Article, Id);
				}
				
				if (!forArticle)
				{
					foreach (FieldValue item in result)
					{
						if (item.Field.IsDateTime && !String.IsNullOrEmpty(item.Value))
						{
							item.Value = Converter.ToDbDateTimeString(item.Value);
						}
						item.Version = this;
					}
				}

				return result;
			}
		}

		public IEnumerable<FieldValue> GetAggregatedFieldValues(Article aggregatedArticle, bool forArticle = false)
		{
			IEnumerable<FieldValue> result = Article.GetFieldValues(versionRowData.Value, aggregatedArticle.Content.Fields, aggregatedArticle, Id, aggregatedArticle.Content.Name);
			if (!forArticle)
			{
				foreach (FieldValue item in result)
				{
					if (item.Field.IsDateTime && !String.IsNullOrEmpty(item.Value))
					{
						item.Value = Converter.ToDbDateTimeString(item.Value);
					}
					item.Version = this;
				}
			}
			return result;
		}

		private Dictionary<string, FieldValue> GetFieldHash()
		{
			Dictionary<string, FieldValue> result = new Dictionary<string, FieldValue>();
			foreach (FieldValue value in FieldValues)
			{
				result.Add(value.Field.Name, value);
			}
			return result;
		}

		#endregion

		#region Internal Members


		/// <summary>
		/// Вспомогательная структура для слияния
		/// </summary>
		[ScriptIgnore]
		internal Dictionary<string, FieldValue> FieldHash
		{
			get
			{
				if (_fieldHash == null)
					_fieldHash = GetFieldHash();
				return _fieldHash;
			}
		}


		/// <summary>
		/// Траслирует SortExpression из Presentation в BLL
		/// </summary>
		/// <param name="sortExpression">SortExpression</param>
		/// <returns>SortExpression</returns>
		public new static string TranslateSortExpression(string sortExpression)
		{
			string result = EntityObject.TranslateSortExpression(sortExpression);
			Dictionary<string, string> replaces = new Dictionary<string, string>() { 
				{"Name", "Id"} 
			};
			return TranslateHelper.TranslateSortExpression(result, replaces);
		}


		/// <summary>
		/// Осуществляет слияние с указанной версией для реализации сравнения
		/// </summary>
		/// <param name="versionToMerge">версия для слияния</param>
		internal void MergeToVersion(ArticleVersion versionToMerge)
		{
			foreach (FieldValue item in FieldValues)
			{
				string valueToMerge = versionToMerge.FieldHash[item.Field.Name].Value;
				if (item.Field.Type.Name == FieldTypeName.Relation || item.Field.Type.Name == FieldTypeName.M2ORelation || item.Field.IsDateTime || item.Field.IsClassifier)
				{
					item.ValueToMerge = valueToMerge;
				}
				else
				{
					item.Value = ArticleVersion.Merge(Formatter.ProtectHtml(item.Value), Formatter.ProtectHtml(valueToMerge));
				}
			}
			VersionToMerge = versionToMerge;
		}
		#endregion

		#region Public Constants

		/// <summary>
		/// Фальшивый идентификатор для текущей версии
		/// </summary>
		public static readonly int CurrentVersionId = 1;


		/// <summary>
		/// Подпапка для версий
		/// </summary>
		public static readonly string RootFolder = "_qp7_article_files_versions";

		#endregion

		#region Public Properties

		public int ArticleId { get; set; }

		public Article Article { get; set; }

		/// <summary>
		/// Версия, с которой происходит сравнение
		/// </summary>
		public ArticleVersion VersionToMerge { get; set; }


		/// <summary>
		/// ID версии, с которой происходит сравнение
		/// </summary>
		public int MergedId
		{
			get
			{
				return (VersionToMerge == null) ? 0 : VersionToMerge.Id;
			}
		}


		/// <summary>
		/// Имя версии (используется в табличном режиме)
		/// </summary>
		public override string Name
		{
			get
			{
				return Id.ToString();
			}
		}

		/// <summary>
		/// Имя версии (используется в режиме просмотра)
		/// </summary>
		[LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
		public string ExpandedName
		{
			get
			{
				return (Id == CurrentVersionId) ? ArticleStrings.CurrentVersion : String.Format(ArticleStrings.VersionN, Id);
			}
		}

		/// <summary>
		/// Поля данных версии
		/// </summary>
		[ScriptIgnore]
		public List<FieldValue> FieldValues
		{
			get
			{
				if (_fieldValues == null)
					_fieldValues = GetFieldValues();
				return _fieldValues;
			}
			set { _fieldValues = value; }
		}

		private Lazy<DataRow> versionRowData = null;
		//[ScriptIgnore]
		//public DataRow VersionRowData { get { return versionRowData.Value; } }

		/// <summary>
		/// Библиотека версии
		/// </summary>
		public override PathInfo PathInfo
		{
			get
			{
				if (_PathInfo == null)
					_PathInfo = Article.GetVersionPathInfo(Id);
				return _PathInfo;
			}
		}

		#endregion


		/// <summary>
		/// Осуществляет слияние 2 строк с помощью QA_Merger
		/// </summary>
		/// <param name="s1">строка 1</param>
		/// <param name="s2">строка 2</param>
		/// <returns>результат слияния</returns>
		public static string Merge(string s1, string s2)
		{
			string prefix = "<html><body>";
			string suffix = "</body></html>";
			string mergeFormat = "{0}{1}{2}";
			Merger.MergeProcessor mergeProcessor = new Merger.MergeProcessor(String.Format(mergeFormat, prefix, s1, suffix), String.Format(mergeFormat, prefix, s2, suffix));
			string result = mergeProcessor.Merge();
			return result.Replace(prefix, "").Replace(suffix, "");
		}

		/// <summary>
		/// Осуществляет слияние 2х коллекций связанных сущьностей
		/// </summary>
		/// <param name="titles1"></param>
		/// <param name="titles2"></param>
		/// <returns></returns>
		public static string MergeRelation(IEnumerable<ListItem> titles1, IEnumerable<ListItem> titles2)
		{
			IEqualityComparer<ListItem> comparer = new LambdaEqualityComparer<ListItem>((x, y) => x.Value.Equals(y.Value), x => x.Value.GetHashCode());

			var same = titles1.Intersect(titles2, comparer)
				.Select(i => new { id = i.Value, title = String.Format("(#{0}) - {1}", i.Value, i.Text) });
			var removed = titles1.Except(titles2, comparer)
				.Select(i => new { id = i.Value, title = String.Format("<span style='text-decoration: line-through; color: red'>(#{0}) - {1}</span>", i.Value, i.Text) });
			var added = titles2.Except(titles1, comparer)
				.Select(i => new { id = i.Value, title = String.Format("<span style='background: #FFFF4D'>(#{0}) - {1}</span>", i.Value, i.Text) });

			var result = same.Concat(removed).Concat(added)
				.OrderBy(i => i.id)
				.Select(i => i.title);

			return String.Join("<br />", result);
		}

	}
}
