using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Utils.FullTextSearch;
using System.Collections;

namespace Quantumart.QP8.BLL
{
	public class StopWordList : IStopWordList
	{
		private static readonly Lazy<HashSet<string>> stopListHashSet = new Lazy<HashSet<string>>(CreateStopListHashSet, true);		

		private static HashSet<string> CreateStopListHashSet()
		{			
				// получить версию sql server
				var version = QPContext.EFContext.GetSqlServerVersion();
				// если это 2008 или старше - то получить стоп-лист из sql server
				if (version.Major >= 10)
				{
					IEnumerable<string> stopList = QPContext.EFContext.GetStopWordList();
					return new HashSet<string>(stopList, StringComparer.InvariantCultureIgnoreCase);
				}
				else
					return new HashSet<string>();
		}		
		
		#region IStopWordList Members

		public bool ContainsWord(string word)
		{
			return stopListHashSet.Value.Contains(word);			
		}

		#endregion
	}
}
