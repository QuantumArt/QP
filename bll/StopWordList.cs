using System;
using System.Collections.Generic;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils.FullTextSearch;

namespace Quantumart.QP8.BLL
{
    public class StopWordList : IStopWordList
    {
        private static readonly Lazy<HashSet<string>> stopListHashSet = new Lazy<HashSet<string>>(CreateStopListHashSet, true);

        private static HashSet<string> CreateStopListHashSet()
        {
            using (var scope = new QPConnectionScope())
            {
                // получить версию sql server
                var version = Common.GetSqlServerVersion(scope.DbConnection);

                // если это 2008 или старше - то получить стоп-лист из sql server
                if (version.Major >= 10)
                {
                    var stopList = Common.GetStopWordList(scope.DbConnection);
                    return new HashSet<string>(stopList, StringComparer.InvariantCultureIgnoreCase);
                }

                return new HashSet<string>();
            }
        }

        #region IStopWordList Members

        public bool ContainsWord(string word) => QPContext.DatabaseType == DatabaseType.SqlServer && stopListHashSet.Value.Contains(word);

        #endregion
    }
}
