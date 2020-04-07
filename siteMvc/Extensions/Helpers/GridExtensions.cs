using System.Text;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class GridExtensions
    {
        /// <summary>
        /// Трансформирует строковую настройку сортировки в SQL-код
        /// </summary>
        /// <param name="sorting">настройки сортировки</param>
        /// <returns>настройка сортировки в виде SQL-кода</returns>
        public static string ToSqlSortExpression(this string sorting)
        {
            if (sorting.Length != 0)
            {
                var sortExpression = new StringBuilder(sorting);
                return sortExpression
                    .Replace("asc", "ASC")
                    .Replace("desc", "DESC")
                    .Replace("-", " ")
                    .ToString();
            }
            return sorting;
        }
    }
}
