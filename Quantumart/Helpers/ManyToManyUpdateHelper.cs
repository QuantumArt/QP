using System.Text;

namespace Quantumart.QPublishing.Helpers
{
    public class ManyToManyUpdateHelper
    {
        #region private

        public int Counter { get; set; }

        private StringBuilder Result { get; }

        #region table names

        private static readonly string InsertNonAsyncLinkTable = "item_to_item";

        private static readonly string DeleteNonAsyncLinkTable = "item_link_united_full";

        private static readonly string SelectNonAsyncLinkTable = "item_link";

        private static readonly string AsyncLinkTable = "item_link_async";

        #endregion

        #region parameter names

        private string LinkParamName => $"@link{Counter}";

        private string OldIdTableName => $"@ids{Counter}";

        private string NewIdTableName => $"@new_ids{Counter}";

        private string CrossIdTableName => $"@cross_ids{Counter}";

        private string ItemParamName => "@ItemId";

        private string SplittedParamName => "@splitted";

        private string CurrentItemVariableName => $"@current{Counter}";

        #endregion

        #region sql statements

        private string CreateOldValueTableSql
        {
            get
            {
                var sb = new StringBuilder();
                string selectSql =
                    $"insert into {OldIdTableName} select linked_item_id from {{0}} where link_id = {LinkParamName} and item_id = {ItemParamName}";
                sb.AppendFormatLine(IdTableSql, OldIdTableName);
                sb.AppendFormatLine("IF {0} = 1", SplittedParamName);
                sb.AppendFormatLine(selectSql, AsyncLinkTable);
                sb.AppendFormatLine("ELSE");
                sb.AppendFormatLine(selectSql, SelectNonAsyncLinkTable);
                return sb.ToString();
            }
        }

        private string CreateCrossValueTableSql
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendFormatLine(IdTableSql, CrossIdTableName);
                sb.AppendFormatLine("insert into {0} select t1.id from {1} t1 inner join {2} t2 on t1.id = t2.id", CrossIdTableName, OldIdTableName, NewIdTableName);
                return sb.ToString();
            }
        }

        private string CreateNewValueTableSql(string ids)
        {
            var sb = new StringBuilder();
            sb.AppendFormatLine(IdTableSql, NewIdTableName);
            if (!string.IsNullOrEmpty(ids))
                foreach (var id in ids.Split(','))
                {
                    sb.AppendFormatLine("insert into {0} values ({1});", NewIdTableName, id);
                }
            sb.AppendLine();
            return sb.ToString();
        }

        private string IdTableSql => "declare {0} table (id numeric primary key);";

        private string CorrectValueSqlTemplate => "delete from {0} where id in (select id from {1});";

        private string CorrectOldValueTableSql => string.Format(CorrectValueSqlTemplate, OldIdTableName, CrossIdTableName);

        private string CorrectNewValueTableSql => string.Format(CorrectValueSqlTemplate, NewIdTableName, CrossIdTableName);


        private string ConditionalDeleteAllAsyncLinkSql
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendFormatLine("IF {0} = 0", SplittedParamName);
                sb.AppendFormatLine("DELETE FROM {0} WHERE link_id = {1} AND item_id = {2};", AsyncLinkTable, LinkParamName, ItemParamName);
                return sb.ToString();
            }
        }

        private string DeleteLinkSql
        {
            get
            {
                string deleteSqlString =
                    $"DELETE FROM {{0}} WHERE link_id = {LinkParamName} AND item_id = {ItemParamName} and linked_item_id in (select id from {OldIdTableName});";
                var sb = new StringBuilder();
                sb.AppendFormatLine("IF {0} = 1", SplittedParamName);
                sb.AppendFormatLine(deleteSqlString, AsyncLinkTable);
                sb.AppendFormatLine("ELSE");
                sb.AppendFormatLine(deleteSqlString, DeleteNonAsyncLinkTable);
                return sb.ToString();
            }
        }

        private string InsertLinkSql
        {
            get
            {
                string insertSqlString =
                    $"INSERT INTO {{0}} SELECT {LinkParamName}, {ItemParamName}, id from {NewIdTableName};";
                var sb = new StringBuilder();
                sb.AppendFormatLine("IF {0} = 1", SplittedParamName);
                sb.AppendFormatLine(insertSqlString, AsyncLinkTable);
                sb.AppendFormatLine("ELSE");
                sb.AppendFormatLine(insertSqlString, InsertNonAsyncLinkTable);
                return sb.ToString();
            }
        }

        private string NewItemCycleSql
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendFormatLine("declare {0} numeric;", CurrentItemVariableName);
                sb.AppendFormatLine("while exists (select * from {0})", NewIdTableName);
                sb.AppendFormatLine("begin");
                sb.AppendFormatLine(" select {0} = id from {1};", CurrentItemVariableName, NewIdTableName);
                sb.AppendFormatLine(" exec qp_apply_link_id_to_data {0}, {1};", LinkParamName, CurrentItemVariableName);
                sb.AppendFormatLine("IF {0} = 0 and exists (select * from content_item where content_item_id = {2} and splitted = 1) and not exists (select * from item_link_async where link_id = {1} and item_id = {2} and linked_item_id = {3}) insert into item_link_async with(rowlock) values({1}, {2}, {3})", SplittedParamName, LinkParamName, CurrentItemVariableName, ItemParamName);
                sb.AppendFormatLine(" delete from {0} where id = {1};", NewIdTableName, CurrentItemVariableName);
                sb.AppendFormatLine("end;");
                return sb.ToString();
            }
        }

        #endregion

        #region collect sql


        private void СollectSqlForManyToMany(string value)
        {
            CollectValueTablesSql(value);

            Result.AppendLine(ConditionalDeleteAllAsyncLinkSql);
            Result.AppendLine(DeleteLinkSql);
            Result.AppendLine(InsertLinkSql);
            Result.AppendLine(NewItemCycleSql);
        }

        private void CollectValueTablesSql(string value)
        {
            Result.AppendLine(CreateOldValueTableSql);
            Result.AppendLine(CreateNewValueTableSql(value));
            Result.AppendLine(CreateCrossValueTableSql);

            Result.AppendLine(CorrectOldValueTableSql);
            Result.AppendLine(CorrectNewValueTableSql);
        }

        #endregion

        #endregion

        #region constructors
        public ManyToManyUpdateHelper(int counter)
        {
            Counter = counter;
            Result = new StringBuilder();
        }

        public static string GetSql(string value, int counter) {
            var helper = new ManyToManyUpdateHelper(counter);
            helper.СollectSqlForManyToMany(value);
            return helper.Result.ToString();
        }

        #endregion
    }
}
