using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{
    internal class VirtualFieldRepository
    {
        internal static Field Save(Field item)
        {
            DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Field, item);
            var newItem = DefaultRepository.Save<Field, FieldDAL>(item);
            DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Field);
            return newItem;
        }

        internal static Field Update(Field item) => DefaultRepository.Update<Field, FieldDAL>(item);

        /// <summary>
        /// Определяет, существуют ли виртуальные поля в Join-контентах построенные на текущем поле
        /// </summary>
        internal static bool JoinVirtualChildrenFieldsExist(Field field)
        {
            return QPContext.EFContext.FieldSet.Any(f => f.PersistentId != null && f.PersistentId.Value == field.Id);
        }

        /// <summary>
        /// Удалить записи в таблице union_contents для всех полей union-контента
        /// </summary>
        internal static void RemoveUnionAttrs(Content unionContent)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveUnionAttrsByUnionContent(scope.DbConnection, unionContent.Id);
            }
        }

        /// <summary>
        /// Удалить записи в таблице union_contents для базовых полей
        /// </summary>
        internal static void RemoveUnionAttrs(List<int> baseFieldIds)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveUnionAttrsByBaseFields(scope.DbConnection, baseFieldIds);
            }
        }

        internal static void RebuildUnionAttrs(Content unionContent)
        {
            var dbUnionContent = ContentRepository.GetById(unionContent.Id);
            var baseFields = VirtualContentRepository.GetFieldsOfContents(dbUnionContent.UnionSourceContentIDs).Select(f => new { f.Id, Name = f.Name.ToLowerInvariant() }).ToArray();
            var virtualFields = dbUnionContent.Fields.Select(f => new { f.Id, Name = f.Name.ToLowerInvariant() }).ToArray();

            var newUnionAttr =
                from sf in baseFields
                join uf in virtualFields on sf.Name equals uf.Name
                select new UnionAttr
                {
                    VirtualFieldId = uf.Id,
                    BaseFieldId = sf.Id
                };

            // сохранить новые привязки
            IEnumerable<UnionAttrDAL> records = MapperFacade.UnionAttrMapper.GetDalList(newUnionAttr.ToList());
            using (var scope = new QPConnectionScope())
            {
                Common.BatchInsertUnionAttrs(scope.DbConnection, records);
            }
        }

        private class VirtualFieldRelationStorageItem
        {
            public int ScopeCount { get; set; }

            public IEnumerable<VirtualFieldsRelation> Data { get; set; }
        }

        /// <summary>
        /// Хранит данные о связи полей в памяти
        /// </summary>
        private class VirtualFieldRelationsStorage : IDisposable
        {
            [ThreadStatic]
            private static Dictionary<int, VirtualFieldRelationStorageItem> _storage;

            [ThreadStatic]
            private static Queue<int> _contentIds;

            public VirtualFieldRelationsStorage(int rootContentId)
            {
                if (_storage == null)
                {
                    _storage = new Dictionary<int, VirtualFieldRelationStorageItem>();
                }

                if (_contentIds == null)
                {
                    _contentIds = new Queue<int>();
                }

                _contentIds.Enqueue(rootContentId);
                if (!_storage.ContainsKey(rootContentId))
                {
                    var item = new VirtualFieldRelationStorageItem { ScopeCount = 1 };
                    _storage.Add(rootContentId, item);
                    using (var scope = new QPConnectionScope())
                    {
                        var dt = Common.LoadVirtualFieldsRelations(scope.DbConnection, rootContentId);
                        item.Data = MapperFacade.VirtualFieldsRelationMapper.GetBizList(dt.AsEnumerable().ToList()).Distinct().ToArray();
                    }
                }
                else
                {
                    _storage[rootContentId].ScopeCount++;
                }
            }

            public void Dispose()
            {
                var rootContentId = _contentIds.Dequeue();
                if (!_contentIds.Any())
                {
                    _contentIds = null;
                }

                if (_storage != null && _storage.ContainsKey(rootContentId))
                {
                    // уменьшаем счетчик вложенных scope
                    _storage[rootContentId].ScopeCount--;

                    // если это последний, то все очищаем

                    if (_storage[rootContentId].ScopeCount == 0)
                    {
                        _storage.Remove(rootContentId);
                    }

                    if (!_storage.Keys.Any())
                    {
                        _storage = null;
                    }
                }
            }

            public static IEnumerable<VirtualFieldsRelation> Data
            {
                get
                {
                    var currentRootContentId = _contentIds.Peek();
                    return _storage != null && _storage.ContainsKey(currentRootContentId) ? _storage[currentRootContentId].Data : null;
                }
            }
        }

        public static IDisposable LoadVirtualFieldsRelationsToMemory(int rootContentId) => new VirtualFieldRelationsStorage(rootContentId);

        /// <summary>
        /// Возвращает информацию о дочерних витуальных полях
        /// </summary>
        public static IEnumerable<VirtualFieldsRelation> GetVirtualSubFields(List<int> rootFieldId)
        {
            if (VirtualFieldRelationsStorage.Data == null)
            {
                throw new ApplicationException("GetVirtualSubFields is invoke before LoadVirtualFieldsRelationsToMemory");
            }

            if (!rootFieldId.Any())
            {
                return Enumerable.Empty<VirtualFieldsRelation>();
            }

            using (var scope = new QPConnectionScope())
            {
                var dt = Common.GetVirtualSubFields(scope.DbConnection, rootFieldId);
                var result = MapperFacade.VirtualFieldsRelationMapper.GetBizList(dt.AsEnumerable().ToList());
                result.AddRange(VirtualFieldRelationsStorage.Data);
                return result.Where(r => rootFieldId.Contains(r.BaseFieldId)).Distinct().ToArray();
            }
        }

        public static IEnumerable<VirtualFieldsRelation> GetVirtualBaseFieldIDs(List<int> subFieldIds)
        {
            var result = new List<VirtualFieldsRelation>(VirtualFieldRelationsStorage.Data.Where(r => subFieldIds.Contains(r.VirtualFieldId)));
            using (var scope = new QPConnectionScope())
            {
                var dt = Common.GetVirtualBaseFieldIDs(scope.DbConnection, subFieldIds);
                result.AddRange(MapperFacade.VirtualFieldsRelationMapper.GetBizList(dt.AsEnumerable().ToList()));
            }

            return result.Distinct().ToArray();
        }

        /// <summary>
        /// Получить количество полей связей в Union_ATTR для union-полей
        /// </summary>
        public static IEnumerable<UnionFieldRelationCount> GetUnionFieldRelationCount(List<int> unionFieldIds)
        {
            if (!unionFieldIds.Any())
            {
                return Enumerable.Empty<UnionFieldRelationCount>();
            }

            using (var scope = new QPConnectionScope())
            {
                var dt = Common.GetUnionFieldRelationCount(scope.DbConnection, unionFieldIds);
                return MapperFacade.UnionFieldRelationCountMapper.GetBizList(dt.AsEnumerable().ToList());
            }
        }

        /// <summary>
        /// Обновить привязку базовых полей к UQ-контенту
        /// </summary>
        internal static void RebuildUserQueryAttrs(Content dbContent)
        {
            dbContent = ContentRepository.GetById(dbContent.Id);

            // определить какие контенты используются
            // и построить словарь используемых контентов
            var usedContentIds = dbContent.UserQueryContentViewSchema.SelectUniqContentIDs();
            var usedContents = ContentRepository.GetList(usedContentIds);
            var usedContentDict = new Dictionary<int, Dictionary<string, int>>();
            foreach (var uc in usedContents)
            {
                usedContentDict.Add(uc.Id, new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase));
                foreach (var f in uc.Fields)
                {
                    usedContentDict[uc.Id].Add(f.Name, f.Id);
                }
            }

            var userQueryAttr = dbContent.UserQueryContentViewSchema
                .Where(c => c.ContentId.HasValue) // только колонки из контентов
                .Where(c => usedContentDict[c.ContentId.Value].ContainsKey(c.ColumnName))
                .Select(c => new UserQueryAttr
                {
                    UserQueryContentId = dbContent.Id,
                    BaseFieldId = usedContentDict[c.ContentId.Value][c.ColumnName]
                });

            var records = MapperFacade.UserQueryAttrMapper.GetDalList(userQueryAttr.ToList()).AsEnumerable();
            using (var scope = new QPConnectionScope())
            {
                Common.BatchInsertUserQueryAttrs(scope.DbConnection, records);
            }
        }

        internal static void RemoveUserQueryAttrs(Content dbContent)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveUserQueryAttrs(scope.DbConnection, dbContent.Id);
            }
        }

        /// <summary>
        /// Для виртуального поля, возвращает ID всех реальных базовых полей
        /// </summary>
        internal static IEnumerable<int> GetRealBaseFieldIds(int virtualFieldId)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetRealBaseFieldIds(scope.DbConnection, virtualFieldId);
            }
        }
    }
}
