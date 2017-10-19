using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.Articles;

namespace Quantumart.QP8.BLL.Helpers
{
    public class OptimizeHierarchyHelper
    {
        private Dictionary<int, HierarchyItem> _hierarchyItems;
        private Dictionary<int, List<HierarchyItem>> _levelItems;
        private readonly List<HierarchyItem> _roots = new List<HierarchyItem>();

        public OptimizeHierarchyHelper(FieldValue fv)
        {
            FieldValue = fv;
        }

        public FieldValue FieldValue { get; set; }

        public void Process()
        {
            var m = FieldValue.RelatedItems;
            if (m.Length <= 1)
            {
                return;
            }

            if (FieldValue.Field.RelateToContentId == null)
            {
                return;
            }

            var data = ArticleRepository.GetHierarchy(FieldValue.Field.RelateToContentId.Value);
            if (data == null)
            {
                return;
            }

            BuildHierarchy(data);
            OptimizeHierarchyFor(FieldValue.RelatedItems);
            FieldValue.Value = string.Join(",", GetOptimizedValue());
        }

        private int[] GetOptimizedValue()
        {
            return _hierarchyItems.Values.Where(n => n.IsChecked).Select(n => n.Id).ToArray();
        }

        private void OptimizeHierarchyFor(int[] relatedItems)
        {
            ApplyCheckedItems(relatedItems);
            RemoveRedundantChildren();
            PropagateToParents();
        }

        private void PropagateToParents()
        {
            var levelOrder = _levelItems.Keys.OrderByDescending(n => n).ToArray();
            foreach (var level in levelOrder)
            {
                var parentsToTest = _levelItems[level].Where(n => n.IsChecked).Select(n => n.Parent).Where(n => n != null).Distinct().ToArray();
                var parentsToProcess = parentsToTest.Where(n => n.Children.All(m => m.IsChecked) && !n.IsChecked).ToList();
                parentsToProcess.ForEach(n =>
                {
                    n.IsChecked = true;
                    n.Children.ForEach(m => m.IsChecked = false);
                });
            }
        }

        private void RemoveRedundantChildren()
        {
            foreach (var n in _hierarchyItems.Values)
            {
                if (n.IsChecked)
                {
                    n.UncheckChildren();
                }
            }
        }

        private void BuildHierarchy(Dictionary<int, int> hierarchy)
        {
            _hierarchyItems = hierarchy.AsEnumerable().Select(n => new HierarchyItem { Id = n.Key, ParentId = n.Value }).ToDictionary(n => n.Id, m => m);
            foreach (var n in _hierarchyItems.Values)
            {
                n.Parent = n.ParentId != 0 ? _hierarchyItems[n.ParentId] : null;
                if (n.ParentId == 0)
                {
                    _roots.Add(n);
                }
            }

            foreach (var n in _hierarchyItems.Values)
            {
                n.Parent?.Children.Add(_hierarchyItems[n.Id]);
            }

            _levelItems = new Dictionary<int, List<HierarchyItem>>();
            _roots.ForEach(n => n.SetLevel(1, _levelItems));
        }

        private void ApplyCheckedItems(IEnumerable<int> relatedItems)
        {
            var s = new HashSet<int>(relatedItems);
            foreach (var item in _hierarchyItems.Values)
            {
                item.IsChecked = s.Contains(item.Id);
            }
        }

        private class HierarchyItem
        {
            public HierarchyItem()
            {
                Children = new List<HierarchyItem>();
            }

            public int Id { get; set; }

            public int ParentId { get; set; }

            public bool IsChecked { get; set; }

            private int Level { get; set; }

            public HierarchyItem Parent { get; set; }

            public List<HierarchyItem> Children { get; }

            public void SetLevel(int level, IDictionary<int, List<HierarchyItem>> levelItems)
            {
                if (!levelItems.TryGetValue(level, out List<HierarchyItem> list))
                {
                    list = new List<HierarchyItem>();
                    levelItems.Add(level, list);
                }

                list.Add(this);
                Level = level;
                Children.ForEach(n => n.SetLevel(Level + 1, levelItems));
            }

            public void UncheckChildren()
            {
                Children.ForEach(n =>
                {
                    n.IsChecked = false;
                    n.UncheckChildren();
                });
            }
        }
    }
}
