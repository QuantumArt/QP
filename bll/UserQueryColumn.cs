using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Utils;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Информация о столбце запроса
	/// </summary>
	internal class UserQueryColumn : IEquatable<UserQueryColumn>
	{
		public string TableName { get; set; }
		public string ColumnName { get; set; }
		public string DbType { get; set; }
		public int? NumericScale { get; set; }

		public int? CharMaxLength { get; set; }

		public string NumericScaleString {
			get { return NumericScale.HasValue ? NumericScale.Value.ToString() : ""; }
		}

		public string CharMaxLengthString
		{
			get { return CharMaxLength.HasValue ? CharMaxLength.Value.ToString() : ""; }
		}
		public string TableDbType { get; set; }


		bool isContentBasedReset = false;
		public void ResetContentBased()
		{
			isContentBasedReset = true;
		}
		
		static readonly Regex contentBasedFieldExpression = new Regex(@"^content_\d+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		/// <summary>
		/// Колонка основана на колонке контента? 
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		public bool IsColumnContentFieldBased
		{
			get
			{
				return !isContentBasedReset && !String.IsNullOrEmpty(TableName) && contentBasedFieldExpression.IsMatch(TableName);
			}
		}		

		public int? ContentId
		{
			get
			{
				if (IsColumnContentFieldBased)
					return Converter.ToInt32(TableName.Remove(0, 8));
				else
					return null;
			}
		}


		#region Compare
		/// <summary>
		/// Сравнивает столбцы с выбирая наиболее подходящие в качестве основы для полей UQ-контента 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		private static int SelectColumnCompareTo(UserQueryColumn x, UserQueryColumn y)
		{
			if (x.IsColumnContentFieldBased && y.IsColumnContentFieldBased)
			{
				// обе колонки из контента
				return y.ContentId.Value - x.ContentId.Value;
			}
			else if (x.IsColumnContentFieldBased && !y.IsColumnContentFieldBased)
			{				
				return 1;
			}
			else if (!x.IsColumnContentFieldBased && y.IsColumnContentFieldBased)
			{
				return -1;
			}
			else
			{
				// обе не из контента
				return StringComparer.InvariantCultureIgnoreCase.Compare(x.TableName, y.TableName);
			}			
		}
		/// <summary>
		/// Сравнивает столбцы с выбирая наиболее подходящие в качестве основы для полей UQ-контента 
		/// </summary>
		public static readonly IComparer<UserQueryColumn> SelectBaseColumnComparer = new LambdaComparer<UserQueryColumn>(SelectColumnCompareTo);

		#endregion		

		#region Equatable

		public bool Equals(UserQueryColumn other)
		{
			if (other == null)
				return false;
			else
				return Field.NameComparerPredicate(this.ColumnName, other.ColumnName) &&
				       Field.NameComparerPredicate(this.DbType, other.DbType) &&
				       Field.NameComparerPredicate(this.TableName, other.TableName) &&
				       Field.NameComparerPredicate(this.NumericScaleString, other.NumericScaleString) &&
					   Field.NameComparerPredicate(this.CharMaxLengthString, other.CharMaxLengthString);

		}

		public override bool Equals(object obj)
		{
			UserQueryColumn other = obj as UserQueryColumn;
			if (other == null)
				return false;
			else
				return Equals(other);
		}

		public override int GetHashCode()
		{
			int hash = Field.GetNameHashCode(ColumnName);
			hash = _factor * hash + Field.GetNameHashCode(TableName);
			hash = _factor * hash + Field.GetNameHashCode(DbType);
			hash = _factor * hash + Field.GetNameHashCode(NumericScaleString);
			hash = _factor * hash + Field.GetNameHashCode(CharMaxLengthString);
			return hash;
		}

		public static readonly LambdaEqualityComparer<UserQueryColumn> TableNameIgnoreEqualityComparer = new LambdaEqualityComparer<UserQueryColumn>(
				(c1, c2) => Field.NameComparerPredicate(c1.ColumnName, c2.ColumnName) 
					&& Field.NameComparerPredicate(c1.DbType, c2.DbType)
					&& Field.NameComparerPredicate(c1.NumericScaleString, c2.NumericScaleString),
				c =>
				{
					int hash = Field.GetNameHashCode(c.ColumnName);
					hash = _factor * hash + Field.GetNameHashCode(c.DbType);
					hash = _factor * hash + Field.GetNameHashCode(c.NumericScaleString);
					hash = _factor * hash + Field.GetNameHashCode(c.CharMaxLengthString);
					return hash;
				}
			);

		private static readonly int _factor = 31;

		#endregion
		
	}

	internal static class UserQueryColumnIEnumerableExtensions
	{

		/// <summary>
		/// Возвращает id всех контентов
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IEnumerable<int> SelectUniqContentIDs(this IEnumerable<UserQueryColumn> source)
		{
			return source
					.Where(c => c.ContentId.HasValue)
					.Select(c => c.ContentId.Value)
					.Distinct()
					.ToArray();
		}
	}
}
