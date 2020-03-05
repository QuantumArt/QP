using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.DAL
{
    public static class EntityTypeCache
    {
        private static readonly object Locker = new object();

        private static readonly Dictionary<string, List<EntityTypeDAL>> Cache
            = new Dictionary<string, List<EntityTypeDAL>>();

        public static string GetKey(string customerCode, int userId) => $"{customerCode}__{userId}";

        public static List<EntityTypeDAL> GetEntityTypes(QPModelDataContext context, string customerCode, int userId)
        {
            var key = GetKey(customerCode, userId);
            if (!Cache.TryGetValue(key, out var types))
            {
                lock (Locker)
                {
                    if (!Cache.TryGetValue(key, out types))
                    {
                        types = LoadEntityTypes(context);
                        Cache[key] = types;
                    }
                }
            }

            return types;
        }

        public static bool IsParentTypeForTree(QPModelDataContext context, string customerCode, int languageId, string entityTypeCode)
        {
            var entities = GetEntityTypes(context, customerCode, languageId);
            var id = entities.Single(n => n.Code == entityTypeCode).Id;
            return entities.Any(n => n.ParentId == id && !n.Disabled);
        }

        private static List<EntityTypeDAL> LoadEntityTypes(QPModelDataContext context)
        {
                return context.EntityTypeSet
                .Include(x => x.Parent)
                .Include(x => x.CancelAction)
                .Include(x => x.DefaultAction)
                .Include(x => x.ContextMenu)
                .ToList();
        }
    }
}
