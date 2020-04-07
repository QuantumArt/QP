using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.DAL
{
    public static class EntityNameExtensions
    {
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        public static string FixColumnName(this string input, DatabaseType databaseType)
        {
            if (string.IsNullOrEmpty(input) || databaseType != DatabaseType.Postgres)
            {
                return input;
            }

            var value = input.Replace("[", string.Empty).Replace("]", string.Empty).ToSnakeCase();
            if (PostgresReservedWords.Contains(value))
            {
                value = $"\"{value}\"";
            }

            return value;
        }

        private static string[] PostgresReservedWords = new[] { "order" };
    }
}
