using System.Text;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
	public class FieldCondition : ConditionBase
	{
		public QueryField[] Fields { get; set; }
		public string Type { get; set; }
		public bool CastToString { get; set; }

		public FieldCondition()
		{
			CastToString = false;
		}

		public override string GetCurrentExpression()
		{
		    if (Fields != null || Fields.Length > 0)
			{
				var sb = new StringBuilder("root");

				for (var i = 0; i < Fields.Length; i++)
				{
					if (i + 1 < Fields.Length)
					{
						sb.Append("_");
					}
					else
					{
						sb.Append(".");
					}

					sb.Append(Fields[i].Name);
				}

				if (CastToString)
				{
					return string.Format("CAST({0} AS NVARCHAR(MAX))", sb);
				}

			    return sb.ToString();
			}

		    return null;
		}
	}
}
