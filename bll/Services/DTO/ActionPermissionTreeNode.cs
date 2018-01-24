using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.DTO
{
    /// <summary>
    /// Данные узла дерева для управления правилами доступа к действиям
    /// </summary>
    public class ActionPermissionTreeNode
    {
        public static readonly int ENTITY_TYPE_NODE = 1;
        public static readonly int ACTION_NODE = 2;

        public int Id { get; set; }
        public string Text { get; set; }
        public string IconUrl { get; set; }
        public bool HasChildren { get; set; }
        public int NodeType { get; set; }

        public IEnumerable<ActionPermissionTreeNode> Children { get; set; }
    }
}
