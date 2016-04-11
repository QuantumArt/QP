using Quantumart.QP8.BLL.Extensions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.VisualEditor;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Helpers
{
    public static class VisualEditorHelpers
    {
        /// <summary>
        /// Заменяет элементы defaultElements соотв по Id командами из priorElements
        /// </summary>
        public static IEnumerable<T> Merge<T>(IEnumerable<T> defaultElements, IEnumerable<T> priorElements) where T : EntityObject
        {
            var result = defaultElements.ToList();
            foreach (var element in priorElements)
            {
                result.RemoveAll(c => c.Id == element.Id);
                result.Add(element);
            }

            return result.OrderBy(c => c.Id);
        }

        /// <summary>
        /// Вычитает из defaultElements элементы, чьи Id лежат в subElementIds
        /// </summary>
        public static IEnumerable<T> Subtract<T>(IEnumerable<T> defaultElements, int[] subElementIds) where T : EntityObject
        {
            var result = defaultElements.ToList();
            foreach (var cId in subElementIds)
            {
                result.RemoveAll(c => c.Id == cId);
            }

            return result.OrderBy(c => c.Id);
        }

        public static IEnumerable<VisualEditorPlugin> GetVisualEditorPlugins(IEnumerable<VisualEditorCommand> veCommands)
        {
            return veCommands.Where(vec => vec.PluginId.HasValue).Select(c => c.PluginId.Value).Distinct().Select(VisualEditorRepository.GetPluginPropertiesById);
        }

        public static IList<object> GenerateToolbar(IEnumerable<VisualEditorCommand> veCommands)
        {
            const string delimeter = "/";
            var result = veCommands.GroupBy(c => c.RowOrder).Select(GenerateRowGroups).ToList();
            return result.Aggregate(new List<object>(), (prev, curr) =>
            {
                if (prev.Count != 0 && curr.Count != 0)
                {
                    prev.Add(delimeter);
                }

                prev.AddRange(curr);
                return prev;
            });
        }

        private static IList<IList<object>> GenerateRowGroups(IEnumerable<VisualEditorCommand> commands)
        {
            return commands.OrderBy(rr => rr.ToolbarInRowOrder).GroupBy(c => c.ToolbarInRowOrder).Select(GenerateCommandsInRow).ToList();
        }

        private static IList<object> GenerateCommandsInRow(IEnumerable<VisualEditorCommand> commands)
        {
            const string delimeter = "-";
            var result = commands.OrderBy(tt => tt.GroupInToolbarOrder).GroupBy(c => c.GroupInToolbarOrder).Select(GenerateCommandGroupsInRow);
            return result.Aggregate(new List<object>(), (prev, curr) =>
            {
                if (prev.Count != 0 && curr.Count != 0)
                {
                    prev.Add(delimeter);
                }

                prev.AddRange(curr);
                return prev;
            });
        }

        private static IList<object> GenerateCommandGroupsInRow(IEnumerable<VisualEditorCommand> commands)
        {
            return commands.OrderBy(cc => cc.CommandInGroupOrder).Select(c => (object)c.Name).EmptyIfNull().ToList();
        }
    }
}
