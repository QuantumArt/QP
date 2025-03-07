using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.DAL
{
    public class RelationSecurityInfo
    {
        public RelationSecurityInfo()
        {
            Data = new Dictionary<int, Dictionary<int, int[]>>();
            ContentData = new Dictionary<int, int>();
        }

        private Dictionary<int, Dictionary<int, int[]>> Data { get; set; }

        private Dictionary<int, int> ContentData { get; }

        public IEnumerable<int> ContentIds => Data.Keys;

        public Dictionary<int, int[]> GetItemMapping(int contentId)
        {
            Dictionary<int, int[]> result;
            if (Data.TryGetValue(contentId, out result))
            {
                return result;
            }

            throw new ApplicationException("Security mapping not exists:" + contentId);
        }

        public bool IsEmpty => Data == null || ContentData.Count == 0;

        public void AddContentInItemMapping(int contentId, Dictionary<int, int[]> initValues)
        {
            Data.Add(contentId, initValues);
        }

        internal void AppendToItemMapping(int contentId, int id, int[] ids)
        {
            if (!Data[contentId].ContainsKey(id))
            {
                Data[contentId].Add(id, ids);
            }
            else
            {
                Data[contentId][id] = Data[contentId][id].Concat(ids).ToArray();
            }
        }

        public bool IsItemMappingExists(int contentId, int id) => Data[contentId].ContainsKey(id);

        public void MakeEmpty()
        {
            Data = null;
        }

        public void AppendToContentMapping(int contentId, int id)
        {
            if (!ContentData.ContainsKey(id))
            {
                ContentData.Add(id, contentId);
            }
        }

        public int[] GetContentIdsFromContentMapping()
        {
            return ContentData.Select(n => n.Value).Distinct().ToArray();
        }

        public Dictionary<int, int> GetContentMapping() => ContentData;
    }
}
