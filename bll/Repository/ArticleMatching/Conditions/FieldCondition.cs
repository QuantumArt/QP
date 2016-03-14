using Quantumart.QP8.BLL.Repository.ArticleMatching.Models;
using System.Text;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions
{
	public class FieldCondition : ConditionBase
	{
		public QueryField[] Fields { get; set; }
		public string Type { get; set; }
		public bool CastToString { get; set; }

		public FieldCondition()
			: base()
		{
			CastToString = false;
		}

		public override string GetCurrentExpression()
		{
			if (Fields != null || Fields.Length > 0)
			{
				StringBuilder sb = new StringBuilder("root");

				for (int i = 0; i < Fields.Length; i++)
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
				else
				{
					return sb.ToString();
				}
			}
			else
			{
				return null;
			}
		}
	}
}
