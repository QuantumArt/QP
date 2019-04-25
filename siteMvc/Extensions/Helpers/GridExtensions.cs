using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL;
using Telerik.Web.Mvc;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class GridExtensions
    {
        /// <summary>
        /// Возвращает настройки сортировки в виде SQL-кода
        /// </summary>
        /// <param name="sortDescriptors">настройки сортировки</param>
        /// <returns>настройки сортировки в виде SQL-кода</returns>
        public static string ToSqlSortExpression(this IList<SortDescriptor> sortDescriptors)
        {
            var sortExpression = new StringBuilder();
            var sortDescriptorCount = sortDescriptors.Count;

            for (var sortDescriptorIndex = 0; sortDescriptorIndex < sortDescriptorCount; sortDescriptorIndex++)
            {
                var sortDescriptor = sortDescriptors[sortDescriptorIndex];
                var sortColumnName = sortDescriptor.Member;
                var sortDirection = sortDescriptor.SortDirection == ListSortDirection.Ascending ? "ASC" : "DESC";

                if (sortDescriptorIndex > 0)
                {
                    sortExpression.Append(", ");
                }

                sortExpression.AppendFormat("{0} {1}", sortColumnName, sortDirection);
            }

            return sortExpression.ToString();
        }

        /// <summary>
        /// Трансформирует строковую настройку сортировки в SQL-код
        /// </summary>
        /// <param name="sorting">настройки сортировки</param>
        /// <returns>настройка сортировки в виде SQL-кода</returns>
        public static string ToSqlSortExpression(this string sorting)
        {
            var sortExpression = new StringBuilder(sorting);
            return sortExpression
                .Replace("asc", "ASC")
                .Replace("desc", "DESC")
                .Replace("-", " ")
                .ToString();
        }

        public static ListCommand GetListCommand(this GridCommand command)
        {
            var descriptors = command.SortDescriptors.Where(item => !string.IsNullOrEmpty(item.Member)).ToList();
            return new ListCommand { SortExpression = descriptors.ToSqlSortExpression(), StartPage = command.Page, PageSize = command.PageSize };
        }
    }
}
