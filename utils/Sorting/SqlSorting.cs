using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Quantumart.QP8.Utils.Sorting
{
    /// <summary>
    /// Направление сортировки
    /// </summary>
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    /// <summary>
    /// Сортировочная информация
    /// </summary>
    public class SortingInformation
    {
        public SortingInformation(string fieldName, SortDirection direction)
        {
            FieldName = fieldName;
            Direction = direction;
        }

        /// <summary>
        /// название поля, по которому осуществляется сортировка
        /// </summary>
        public string FieldName
        {
            get;
            set;
        }

        /// <summary>
        /// направление сортировки
        /// </summary>
        public SortDirection Direction
        {
            get;
            set;
        }
    }

    public static class SortingInformationExtensions
    {
        /// <summary>
        /// Возвращает настройки сортировки в виде SQL-кода
        /// </summary>
        /// <param name="sortInfoList">настройки сортировки</param>
        /// <returns>настройки сортировки в виде SQL-кода</returns>
        public static string ToSqlSortExpression(this IList<SortingInformation> sortInfoList)
        {
            var sqlSortExpression = new StringBuilder();
            var sortInfoCount = sortInfoList.Count;

            for (var sortInfoIndex = 0; sortInfoIndex < sortInfoCount; sortInfoIndex++)
            {
                var sortInfo = sortInfoList[sortInfoIndex];
                var sortColumnName = sortInfo.FieldName;
                var sortDirection = (sortInfo.Direction == SortDirection.Descending) ? "DESC" : "ASC";

                if (sortInfoIndex > 0)
                {
                    sqlSortExpression.Append(", ");
                }

                sqlSortExpression.AppendFormat("{0} {1}", sortColumnName, sortDirection);
            }

            return sqlSortExpression.ToString();
        }
    }

    public static class SqlSorting
    {
        // Константы
        private const string SqlDirectionAscending = "ASC";
        private const string SqlDirectionDescending = "DESC";

        // Регулярные выражения
        private static readonly Regex SqlDirectionRegExp = new Regex(@"\b(ASC|DESC)\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex SqlAscendingDirectionRegExp = new Regex(@"\b(ASC)\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex SqlDescendingDirectionRegExp = new Regex(@"\b(DESC)\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex SqlAscendingDirectionWithIndentsRegExp = new Regex(@"\s+(ASC)\s*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex SqlDescendingDirectionWithIndentsRegExp = new Regex(@"\s+(DESC)\s*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex SquareBracketsRegExp = new Regex(@"[\[|\]]", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Изменяет направление сортировки на противоположное
        /// </summary>
        /// <param name="sortExpression">SQL-код сортировки</param>
        /// <returns>альтернативные SQL-код сортировки</returns>
        public static string ReverseSortExpression(string sortExpression)
        {
            var alternateSortExpression = sortExpression.Trim();

            if (alternateSortExpression.Length > 0 && alternateSortExpression.IndexOf(',') != -1)
            {
                var sortParameterList = Converter.ToStringCollection(alternateSortExpression, ',', true);
                var sortParameterCount = sortParameterList.Length;

                if (sortParameterCount > 0)
                {
                    alternateSortExpression = "";

                    for (var i = 0; i < sortParameterCount; i++)
                    {
                        var sortParameter = sortParameterList[i].Trim();
                        var alternateSortParameter = sortParameter;

                        if (SqlDescendingDirectionRegExp.IsMatch(sortParameter))
                        {
                            alternateSortParameter = SqlDescendingDirectionRegExp.Replace(sortParameter, SqlDirectionAscending);
                        }
                        else
                        {
                            if (SqlAscendingDirectionRegExp.IsMatch(sortParameter))
                            {
                                alternateSortParameter = SqlAscendingDirectionRegExp.Replace(sortParameter, SqlDirectionDescending);
                            }
                        }

                        if (i > 0)
                        {
                            alternateSortExpression += ", ";
                        }

                        alternateSortExpression += alternateSortParameter;
                    }
                }
            }
            else
            {
                if (SqlDescendingDirectionRegExp.IsMatch(alternateSortExpression))
                {
                    alternateSortExpression = SqlDescendingDirectionRegExp.Replace(alternateSortExpression, SqlDirectionAscending);
                }
                else
                {
                    if (SqlAscendingDirectionRegExp.IsMatch(alternateSortExpression))
                    {
                        alternateSortExpression = SqlAscendingDirectionRegExp.Replace(alternateSortExpression, SqlDirectionDescending);
                    }
                    else
                    {
                        alternateSortExpression += " " + SqlDirectionDescending;
                    }
                }
            }

            return alternateSortExpression;
        }

        /// <summary>
        /// Определяет направление сортировки
        /// </summary>
        /// <param name="sortExpression">SQL-код сортировки</param>
        /// <returns>направление сортировки</returns>
        public static SortDirection RecognizeSortDirection(string sortExpression)
        {
            var result = SortDirection.Ascending;
            var processedSortExpression = sortExpression.Trim();

            if (processedSortExpression.Length > 0 && processedSortExpression.IndexOf(',') != -1)
            {
                var sortParameterList = Converter.ToStringCollection(processedSortExpression, ',', true);
                if (sortParameterList.Length > 0)
                {
                    var firstSortParameter = sortParameterList[0];

                    if (SqlDescendingDirectionRegExp.IsMatch(firstSortParameter))
                    {
                        result = SortDirection.Descending;
                    }
                }
            }
            else
            {
                if (SqlDescendingDirectionRegExp.IsMatch(processedSortExpression))
                {
                    result = SortDirection.Descending;
                }
            }

            return result;
        }

        /// <summary>
        /// Сравнивает SQL-коды сортировки
        /// </summary>
        /// <param name="firstSortExpression">первый SQL-код сортировки</param>
        /// <param name="secondSortExpression">второй SQL-код сортировки</param>
        /// <returns>результат сравнения</returns>
        public static bool CompareSortExpressions(string firstSortExpression, string secondSortExpression)
        {
            return CompareSortExpressions(firstSortExpression, secondSortExpression, false);
        }

        /// <summary>
        /// Сравнивает SQL-коды сортировки
        /// </summary>
        /// <param name="firstSortExpression">первый SQL-код сортировки</param>
        /// <param name="secondSortExpression">второй SQL-код сортировки</param>
        /// <param name="ignoreSortDirection">разрешает не учитывать направление
        /// сортировки при сравнении</param>
        /// <returns>результат сравнения</returns>
        public static bool CompareSortExpressions(
            string firstSortExpression,
            string secondSortExpression,
            bool ignoreSortDirection)
        {
            var firstString = firstSortExpression.Trim();
            firstString = firstString.ToLower();
            firstString = Regex.Replace(firstString, @"\s+", " ", RegexOptions.Multiline);
            firstString = RemoveSquareBrackets(firstString);
            firstString = SqlAscendingDirectionWithIndentsRegExp.Replace(firstString, "");
            if (ignoreSortDirection)
            {
                firstString = SqlDescendingDirectionWithIndentsRegExp.Replace(firstString, "");
            }

            var secondString = secondSortExpression.Trim();
            secondString = secondString.ToLower();
            secondString = Regex.Replace(secondString, @"\s+", " ", RegexOptions.Multiline);
            secondString = RemoveSquareBrackets(secondString);
            secondString = SqlAscendingDirectionWithIndentsRegExp.Replace(secondString, "");
            if (ignoreSortDirection)
            {
                secondString = SqlDescendingDirectionWithIndentsRegExp.Replace(secondString, "");
            }

            var result = (firstString == secondString);
            return result;
        }

        /// <summary>
        /// Удаляет информацию о направлении сортировки из SQL-кода
        /// </summary>
        /// <param name="sortExpression">SQL-код сортировки</param>
        /// <returns>SQL-код сортировки без информации о направлении сортировки</returns>
        public static string RemoveSortDirection(string sortExpression)
        {
            var processedSortExpression = sortExpression.Trim();

            if (processedSortExpression.Length > 0)
            {
                processedSortExpression = SqlAscendingDirectionWithIndentsRegExp.Replace(
                    processedSortExpression, "");
                processedSortExpression = SqlDescendingDirectionWithIndentsRegExp.Replace(
                    processedSortExpression, "");
            }

            return processedSortExpression;
        }

        /// <summary>
        /// Удаляет слово ASC из SQL-кода
        /// </summary>
        /// <param name="sortExpression">SQL-код сортировки</param>
        /// <returns>SQL-код сортировки без слова ASC</returns>
        public static string RemoveAscendingSortDirection(string sortExpression)
        {
            var processedSortExpression = sortExpression.Trim();

            if (processedSortExpression.Length > 0)
            {
                processedSortExpression = SqlAscendingDirectionWithIndentsRegExp.Replace(
                    processedSortExpression, "");
            }

            return processedSortExpression;
        }

        /// <summary>
        /// Проверяет содержит ли SQL-код сортировки квадратные скобки
        /// </summary>
        /// <param name="sortExpression">SQL-код сортировки</param>
        /// <returns>результат проверки (true – содержит; false – не содержит)</returns>
        public static bool ContainsSquareBrackets(string sortExpression)
        {
            return SquareBracketsRegExp.IsMatch(sortExpression);
        }

        /// <summary>
        /// Удаляет квадратные скобки из SQL-кода
        /// </summary>
        /// <param name="sortExpression">SQL-код сортировки</param>
        /// <returns>SQL-код сортировки без квадратных скобок</returns>
        public static string RemoveSquareBrackets(string sortExpression)
        {
            return SquareBracketsRegExp.Replace(sortExpression, "");
        }

        /// <summary>
        /// Изменяет направление сортировки на заданное
        /// </summary>
        /// <param name="sortExpression">SQL-код сортировки</param>
        /// <param name="sortDirection">направление сортировки</param>
        /// <returns>новый SQL-код сортировки</returns>
        public static string ChangeSortExpression(string sortExpression, SortDirection sortDirection)
        {
            var newSortExpression = sortExpression;
            var currentSortDirection = RecognizeSortDirection(sortExpression);

            if (currentSortDirection != sortDirection)
            {
                newSortExpression = ReverseSortExpression(sortExpression);
            }

            return newSortExpression;
        }

        /// <summary>
        /// Возвращает информацию о сортировке
        /// </summary>
        /// <param name="sortExpression">параметры сортировки</param>
        /// <returns>информация о сортировке</returns>
        public static List<SortingInformation> GetSortingInformations(string sortExpression)
        {
            var sortInfoList = new List<SortingInformation>();
            var processedSortExpression = Converter.ToString(sortExpression).Trim();

            if (processedSortExpression.Length > 0)
            {
                if (processedSortExpression.IndexOf(",", StringComparison.Ordinal) != -1)
                {
                    var sortParameterList = Converter.ToStringCollection(processedSortExpression, ',', true);
                    var sortParameterCount = sortParameterList.Length;

                    if (sortParameterCount > 0)
                    {
                        for (var sortParameterIndex = 0; sortParameterIndex < sortParameterCount; sortParameterIndex++)
                        {
                            var sortInfo = GetSortingInformation(sortParameterList[sortParameterIndex]);
                            if (sortInfo != null)
                            {
                                sortInfoList.Add(sortInfo);
                            }
                        }
                    }
                }
                else
                {
                    var sortInfo = GetSortingInformation(processedSortExpression);
                    if (sortInfo != null)
                    {
                        sortInfoList.Add(sortInfo);
                    }
                }
            }

            return sortInfoList;
        }

        /// <summary>
        /// Возвращает информацию о сортировке
        /// </summary>
        /// <param name="sortParameter">параметр сортировки</param>
        /// <returns>информация о сортировке</returns>
        private static SortingInformation GetSortingInformation(string sortParameter)
        {
            SortingInformation sortInfo = null;
            var processedSortParameter = Converter.ToString(sortParameter).Trim();

            if (processedSortParameter.Length > 0)
            {
                string fieldName;
                var direction = SortDirection.Ascending;

                if (SqlDirectionRegExp.IsMatch(processedSortParameter))
                {
                    fieldName = RemoveSortDirection(processedSortParameter);
                    if (SqlDescendingDirectionRegExp.IsMatch(processedSortParameter))
                    {
                        direction = SortDirection.Descending;
                    }
                }
                else
                {
                    fieldName = processedSortParameter;
                }
                fieldName = RemoveSquareBrackets(fieldName);

                sortInfo = new SortingInformation(fieldName, direction);
            }

            return sortInfo;
        }
    }
}
