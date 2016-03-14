using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils
{
	public static class GraphHepler
	{
		/// <summary>
		/// Имеет ли орграф циклы
		/// </summary>
		/// <param name="graphData"></param>
		/// <returns>True - граф имеет циклы</returns>
		public static bool CheckCycleInGraph(Dictionary<int, int[]> graphData)
		{
			Dictionary<int, byte> color = new Dictionary<int, byte>();
			Func<int, bool> dfs = null;
			dfs = (baseID) => 
			{
				if (!color.ContainsKey(baseID))
				{
					color[baseID] = 1;
					if (graphData.ContainsKey(baseID))
					{
						foreach (int cid in graphData[baseID])
						{
							if (dfs(cid))
								return true;
						}
					}
					color[baseID] = 2;
					return false;
				}
				else
					return color[baseID] == 1;
			};

			foreach (int bid in graphData.Keys)
			{
				if (dfs(bid))
					return true;
			}
			return false;
		}
	}
}
