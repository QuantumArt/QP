using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching
{
	internal static class ArticleMatchRepository
	{
		#region Constants
		private const string OtMJoinTemplate = "\tLEFT JOIN CONTENT_{0}_UNITED {1}_{2} ON {1}.{2} = {1}_{2}.CONTENT_ITEM_ID";
		private const string LinkJoinTemplate = "\tLEFT JOIN item_link_united link_{0}_to_{0}_{1} ON {0}.{1} = link_{0}_to_{0}_{1}.link_id AND {0}.CONTENT_ITEM_ID = link_{0}_to_{0}_{1}.item_id";
		private const string MtMJoinTemplate = "\tLEFT JOIN CONTENT_{0}_UNITED {1}_{2} ON link_{1}_to_{1}_{2}.linked_item_id = {1}_{2}.CONTENT_ITEM_ID";
		private const string MoOJoinTemplate = "\tLEFT JOIN CONTENT_{0}_UNITED {1}_{2} ON {1}.CONTENT_ITEM_ID = {1}_{2}.{3}";
		#endregion

		#region Internal members
		internal static ArticleInfo[] MatchArticles(int[] contentIds, ConditionBase condition, MatchMode mode)
		{
			var fields = GetFieldInfo(condition);
			int count = GetFieldCount(condition);

			var schema = MatchContents(contentIds, fields);
			schema = Validate(schema, contentIds, mode, count);

			if (schema.Any())
			{
				var parameters = new Dictionary<string, object>();

				var cnd = condition.Clone().Select<ValueCondition>(c =>
				{
					string parameterKey;

					if (parameters.ContainsValue(c.Value))
					{
						parameterKey = parameters.Single(e => e.Value == c.Value).Key;
					}
					else
					{
						parameterKey = "@p" + parameters.Count;
						parameters[parameterKey] = c.Value;
					}

					return new ParameterCondition { Parameter = parameterKey };
				});

				string query = GetQuery(schema, cnd);

				return MatchArticles(parameters, query);
			}
			else
			{
				return new ArticleInfo[0];
			}
		}
		#endregion

		#region Private members
		private static ArticleInfo[] MatchArticles(Dictionary<string, object> parameters, string query)
		{
			using (new QPConnectionScope())
			{
				var rows = Common.MatchArticles(QPConnectionScope.Current.DbConnection, parameters, query);
				return MappersRepository.DataRowMapper.Map<ArticleInfo>(rows);
			}
		}

		private static SchemaInfo[] MatchContents(int[] contentIds, FieldInfo[] fields)
		{
			var fieldsDoc = GetFieldsDocument(fields);

			using (new QPConnectionScope())
			{
				var rows = Common.MatchContents(QPConnectionScope.Current.DbConnection, contentIds, fieldsDoc);
				return MappersRepository.DataRowMapper.Map<SchemaInfo>(rows);
			}
		}

		private	static XDocument GetFieldsDocument(FieldInfo[] fields)
		{
			var document = new XDocument();

			using (var writer = document.CreateWriter())
			{
				var serializer = new XmlSerializer(typeof(FieldInfo[]));
				serializer.Serialize(writer, fields);
			}

			return document;
		}
		
		private static FieldInfo[] GetFieldInfo(ConditionBase condition)
		{
			var result = new Dictionary<string, FieldInfo>();
			int id = 0;

			foreach (var fieldCondition in condition.OfType<FieldCondition>())
			{
				string key = "";
				int n = 1;
				int? parentId = null;

				foreach (var field in fieldCondition.Fields)
				{
					bool isLast = fieldCondition.Fields.Length == n;
					key += "_" + field.Name + (isLast ? "_" + fieldCondition.Type : null);
					FieldInfo info;

					if (result.TryGetValue(key, out info))
					{
						parentId = info.Id;
					}
					else
					{
						result[key] = new FieldInfo
						{
							Id = id,
							ParentId = parentId,
							Name = field.Name,
							Type = isLast ? fieldCondition.Type : null,
							ContentId = field.ContentId
						};

						parentId = id;

						id++;
					}
					n++;
				}
			}

			return result.Values.ToArray();
		}

		private static int GetFieldCount(ConditionBase condition)
		{
			return condition
				.OfType<FieldCondition>()
				.SelectMany(c => c.Fields)
				.Distinct()
				.Count();
		}

		private static SchemaInfo[] Validate(SchemaInfo[] schema, int[] contentIds, MatchMode mode, int count)
		{
			var unmatchedContentIds = contentIds.Where(contentId => schema.Count(s => s.RootContentId == contentId) < count).ToArray();

			if (mode == MatchMode.Strict && unmatchedContentIds.Any())
			{
				throw new Exception("Contents " + string.Join(", ", unmatchedContentIds) + " don't match filter condition");
			}
			else if (mode == MatchMode.StrictIfPossible)
			{
				return schema.Where(d => !unmatchedContentIds.Contains(d.RootContentId)).ToArray();
			}
			else
			{
				return schema;
			}
		}

		private static bool ComparitionPredicate(ComparitionCondition condition, Func<string, int> getTypeId)
		{
			var data = condition.Conditions.OfType<FieldCondition>().ToArray();

			if (data.Length == 2)
			{
				int leftTypeId = getTypeId(data[0].GetCurrentExpression());
				int rightTypeId = getTypeId(data[1].GetCurrentExpression());
				string leftType = GetTypeByTypeId(leftTypeId);
				string rightType = GetTypeByTypeId(rightTypeId);

				return leftType == rightType && leftType != null;
			}
			else
			{
				return true;
			}
		}

		private static string GetTypeByTypeId(int typeId)
		{
			if (new[] { 4, 5, 6 }.Contains(typeId))
			{
				return "date";
			}
			else if (new[] { 1, 7, 8, 9, 10, 12 }.Contains(typeId))
			{
				return "string";
			}
			else if (new[] { 2, 3 }.Contains(typeId))
			{
				return "numeric";
			}
			else
			{
				return null;
			}
		}

		private static string GetQuery(SchemaInfo[] schema, ConditionBase condition)
		{
			var sb = new StringBuilder();
			var groups = schema.GroupBy(s => s.RootContentId).ToArray();
			int n = 1;

			foreach (var group in groups)
			{
				int contentId = group.Key;
				bool needGrouping = false;

				var aliaces = group
					.Where(s => s.RefContentId == null)
					.Select(s => new { Alias = s.Alias + "." + s.Field, Type = s.DataType, TypeId = s.AttributeTypeId })
					.Distinct()
					.ToArray();

				var where = condition
					.Clone()
					.Where<FieldCondition>(c => aliaces.Any(a => a.Alias == c.GetCurrentExpression() && a.Type == c.Type))
					.Where<ComparitionCondition>(c => c.Conditions.Length > 1)
					.Where<ComparitionCondition>(c => ComparitionPredicate(c, e => aliaces.First(a => e == a.Alias).TypeId))
					.Where<NotCondition>(c => c.Conditions.Length > 0)
					.Select<LogicalCondition>(c => c.Conditions.Length == 1 ? c.Conditions[0] : c)
					.Update<FieldCondition>(c => c.CastToString = aliaces.Any(a => a.Alias == c.GetCurrentExpression() && new[] { 9, 10 }.Contains(a.TypeId)))
					.GetCurrentExpression();

				sb.AppendLine("SELECT");
				sb.AppendLine("\troot.CONTENT_ITEM_ID [Id],");
				sb.AppendFormat("\t{0} [ContentId]", contentId);
				sb.AppendLine();
				sb.AppendLine("FROM");
				sb.AppendFormat("\tCONTENT_{0}_UNITED root", contentId);
				sb.AppendLine();

				foreach (var s in group.Where(s => s.RefContentId != null))
				{
					if (s.LinkId != null)
					{
						needGrouping = true;
						sb.AppendFormat(LinkJoinTemplate, s.Alias, s.Field);
						sb.AppendLine();
						sb.AppendFormat(MtMJoinTemplate, s.RefContentId, s.Alias, s.Field);
					}
					if (s.BackwardField != null)
					{
						needGrouping = s.AttributeTypeId != 2;
						sb.AppendFormat(MoOJoinTemplate, s.RefContentId, s.Alias, s.Field, s.BackwardField);
					}
					else
					{
						sb.AppendFormat(OtMJoinTemplate, s.RefContentId, s.Alias, s.Field);
					}

					sb.AppendLine();
				}

				sb.AppendLine("WHERE");
				sb.Append("\t");
				sb.AppendLine(where);

				if (needGrouping)
				{
					sb.AppendLine("GROUP BY");
					sb.AppendLine("\troot.CONTENT_ITEM_ID");
				}

				if (n < groups.Length)
				{
					sb.AppendLine();
					sb.AppendLine("UNION");
					sb.AppendLine();
				}

				n++;
			}

			return sb.ToString();
		}
		#endregion
	}
}
