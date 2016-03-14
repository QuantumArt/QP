using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Helpers
{
	public static class VeAggregationListItemsHelper
	{
		/// <summary>
		/// Заменяет элементы defaultElements соотв по Id командами из priorElements
		/// </summary>
		/// <param name="defaultElements"></param>
		/// <param name="priorElements"></param>
		/// <returns></returns>				
		public static IEnumerable<T> Merge<T>(IEnumerable<T> defaultElements, IEnumerable<T> priorElements ) where T : EntityObject
		{
			List<T> result = defaultElements.ToList();
			    foreach (var element in priorElements)
			    {
			        result.RemoveAll(c => c.Id == element.Id);
			        result.Add(element);
			    }
			    return result.OrderBy(c => c.Id);
		}

		/// <summary>
		/// Вычитает из defaultElements элементы, чьи Id леат в subElementIds
		/// </summary>		
		/// <returns></returns>
		public static IEnumerable<T> Subtract<T>(IEnumerable<T> defaultElements, int[] subElementIds) where T : EntityObject
		{
			List<T> result = defaultElements.ToList();
			foreach (int cId in subElementIds)
			{
				result.RemoveAll(c => c.Id == cId);				
			}
			return result.OrderBy(c => c.Id);
		}

		/// <summary>
		/// Формирует строку конфига VE из множества команд
		/// </summary>
		/// <param name="rows"></param>
		/// <returns></returns>
		public static string GenerateVeToolbar(IGrouping<int, VisualEditorCommand>[] rows)
		{			
			return String.Format("[{0}]", String.Join(",\n'/',\n", rows
				.Select(r => 
					GenerateRow(r.OrderBy(rr => rr.ToolbarInRowOrder)))));
		}

		private static string GenerateRow(IEnumerable<VisualEditorCommand> commands)
		{
			return string.Join(",\n", commands
				.GroupBy(c => c.ToolbarInRowOrder)
					.Select(t => 
						GenerateToolbar(t.OrderBy(tt => tt.GroupInToolbarOrder))));
		}

		private static string GenerateToolbar(IEnumerable<VisualEditorCommand> commands)
		{			
			return String.Format("[{0}]", String.Join(", '-', ", commands
				.GroupBy(c => c.GroupInToolbarOrder)
					.Select(g => 
						GenerateToolbarGroup(g.OrderBy(cc => cc.CommandInGroupOrder))
					)));
		}

		private static string GenerateToolbarGroup(IEnumerable<VisualEditorCommand> commands)
		{
			return String.Join(", ", commands.Select(c => String.Format("'{0}'", c.Name)));
		}
	}
}
