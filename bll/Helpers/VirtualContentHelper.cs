using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Npgsql;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;

namespace Quantumart.QP8.BLL.Helpers
{
    public class VirtualContentHelper
    {
        public VirtualContentHelper()
        {
        }

        public VirtualContentHelper(List<int> forceIds)
            : this()
        {
            if (forceIds != null && forceIds.Any())
            {
                _forceNewFieldIds = new Queue<int>(forceIds);
            }
        }

        private readonly Queue<int> _forceNewFieldIds;

        public int[] ForceNewFieldIds => _forceNewFieldIds?.ToArray();

        private readonly List<int> _newFieldIds = new List<int>();

        public int[] NewFieldIds => _newFieldIds.ToArray();

        private class UpdateFieldCollectionOperationParams
        {
            public IEnumerable<Field> NewStateRemainingFields { get; set; }

            public IEnumerable<Field> NewFieldsToAdd { get; set; }

            public IEnumerable<Field> FieldsToRemove { get; set; }

            public IEnumerable<Field> FieldsToRemain { get; set; }
        }

        /// <summary>
        /// Логика сохранения нового Join-Контента
        /// </summary>
        internal Content SaveJoinContent(Content content, Content newContent)
        {
            SaveJoinContentFields(content.VirtualJoinFieldNodes.ToList(), newContent);
            CreateContentViews(newContent);
            return ContentRepository.GetById(newContent.Id);
        }

        /// <summary>
        /// Создать поля Join-контента
        /// </summary>
        private void SaveJoinContentFields(List<Content.VirtualFieldNode> newVirtualJoinFieldNodes, Content newContent)
        {
            // получить все поля на основе которых необходимо построить виртуальные поля
            IEnumerable<int> baseFieldsIDs = Content.VirtualFieldNode.Linearize(newVirtualJoinFieldNodes).Select(n => n.Id).Distinct().ToArray();
            var baseFields = FieldRepository.GetList(baseFieldsIDs);
            var baseFieldsDictionary = baseFields.ToDictionary(bs => bs.Id);
            var maxOrder = newContent.GetMaxFieldsOrder();

            // Сохранить иерархию полей
            void Traverse(Field parentVirtualField, IEnumerable<Content.VirtualFieldNode> nodes)
            {
                // сортируем в том порядке в котором расположены соответствующие базовые поля
                var orderedNodes = nodes.OrderBy(n => baseFieldsDictionary[Content.VirtualFieldNode.ParseFieldTreeId(n.TreeId)].Order);
                foreach (var node in orderedNodes)
                {
                    var baseField = baseFieldsDictionary[Content.VirtualFieldNode.ParseFieldTreeId(node.TreeId)];
                    var virtualField = baseField.GetVirtualCloneForJoin(newContent, parentVirtualField);

                    // Сохраняем только новые поля
                    if (!baseField.PersistentId.HasValue)
                    {
                        if (_forceNewFieldIds != null)
                        {
                            virtualField.ForceId = _forceNewFieldIds.Dequeue();
                        }

                        if (QPContext.DatabaseType != DatabaseType.SqlServer)
                        {
                            virtualField.Order = maxOrder + _newFieldIds.Count + 1;
                        }

                        var newVirtualField = VirtualFieldRepository.Save(virtualField);
                        _newFieldIds.Add(newVirtualField.Id);

                        // Заменяем id базового поля на id нового виртуального
                        node.Id = newVirtualField.Id;

                        // вниз по иерархии
                        Traverse(newVirtualField, node.Children);
                    }
                    else
                    {
                        // вниз по иерархии
                        Traverse(baseField, node.Children);
                    }
                }
            }

            Traverse(null, newVirtualJoinFieldNodes);
        }

        /// <summary>
        /// Сохраняет новый Union-контент
        /// </summary>
        internal Content SaveUnionContent(Content content, Content newContent)
        {
            VirtualContentRepository.RecreateUnionSourcesInfo(newContent, content.UnionSourceContentIDs);
            SaveUnionContentFields(content.UnionSourceContentIDs, newContent);
            CreateContentViews(newContent);
            return newContent;
        }

        /// <summary>
        /// Сохранить поля нового Union-контента
        /// </summary>
        private void SaveUnionContentFields(IEnumerable<int> unionSourceContentIDs, Content newContent)
        {
            // сформировать список уникальных (по имени) полей (достаточно взять первое поле в каждой группе)
            IEnumerable<Field> baseFields = VirtualContentRepository.GetFieldsOfContents(unionSourceContentIDs)
                .Distinct(Field.NameComparer)
                .OrderBy(f => f.Order)
                .ToArray();

            // сохранить поля
            foreach (var bf in baseFields)
            {
                var field = bf.GetVirtualClone(newContent);
                if (_forceNewFieldIds != null)
                {
                    field.ForceId = _forceNewFieldIds.Dequeue();
                }

                if (QPContext.DatabaseType != DatabaseType.SqlServer)
                {
                    field.Order = _newFieldIds.Count + 1;
                }

                field = VirtualFieldRepository.Save(field);
                _newFieldIds.Add(field.Id);
            }

            VirtualFieldRepository.RebuildUnionAttrs(newContent);
        }

        /// <summary>
        /// Сохранить новый UQ-контент
        /// </summary>
        internal Content SaveUserQueryContent(Content content, Content dbContent)
        {
            CreateContentViews(dbContent);
            SaveUserQueryContentFields(dbContent);
            VirtualFieldRepository.RebuildUserQueryAttrs(dbContent);
            VirtualContentRepository.RecreateUserQuerySourcesInfo(dbContent);
            return dbContent;
        }

        /// <summary>
        /// Сохранить поля нового UQ-контента
        /// </summary>
        private void SaveUserQueryContentFields(Content dbContent)
        {
            // получить базовые поля
            var viewColumns = dbContent.UserQueryContentViewSchema;

            var newFields = CreateFieldsForUserQueryColumns(dbContent, viewColumns, true).Select(t => t.Item1);
            foreach (var vField in newFields)
            {
                if (_forceNewFieldIds != null)
                {
                    vField.ForceId = _forceNewFieldIds.Dequeue();
                }

                if (QPContext.DatabaseType != DatabaseType.SqlServer)
                {
                    vField.Order = _newFieldIds.Count + 1;
                }

                var vFieldResult = VirtualFieldRepository.Save(vField);
                _newFieldIds.Add(vFieldResult.Id);
            }
        }

        /// <summary>
        /// Создать поля UQ-контента на основе колонок запроса
        /// </summary>
        private static IEnumerable<Tuple<Field, Field>> CreateFieldsForUserQueryColumns(Content dbContent, IEnumerable<UserQueryColumn> viewColumns, bool forNew)
        {
            var result = new List<Tuple<Field, Field>>();

            var groupedColumns = viewColumns.GroupBy(c => c.ColumnName, c => c, (name, c) =>
            {
                var columns = c.ToList();
                return new
                {
                    Name = name,
                    DbType = columns.Select(n => n.DbType).First(),
                    DecimalPlaces = columns.Select(n => n.NumericScale).First(),
                    CharMaxLength = columns.Select(n => n.CharMaxLength).First(),
                    BaseContentIds = string.Join(",", columns.Select(n => n.ContentId).Where(n => n.HasValue))
                };
            });

            foreach (var column in groupedColumns)
            {
                if (!string.IsNullOrEmpty(column.BaseContentIds))
                {
                    var baseField = VirtualContentRepository.GetAcceptableBaseFieldForCloning(column.Name, column.BaseContentIds, dbContent.Id, forNew);
                    var vField = baseField.GetVirtualClone(dbContent);
                    result.Add(new Tuple<Field, Field>(vField, baseField));
                }
                else
                {
                    var vField = Field.Create(dbContent, new FieldRepository(), new ContentRepository());
                    vField.Name = column.Name;
                    var stringSize = column.CharMaxLength ?? Field.StringSizeDefaultValue;
                    switch (column.DbType.ToLowerInvariant())
                    {
                        case ValidFieldColumnDbTypes.Numeric:
                        case ValidFieldColumnDbTypes.Int:
                        case ValidFieldColumnDbTypes.BigInt:
                        case ValidFieldColumnDbTypes.SmallInt:
                        case ValidFieldColumnDbTypes.TinyInt:
                            vField.TypeId = FieldTypeCodes.Numeric;
                            vField.DecimalPlaces = column.DecimalPlaces.Value;
                            break;
                        case ValidFieldColumnDbTypes.Datetime:
                        case ValidFieldColumnDbTypes.TimeStampWithoutTimeZone:
                            vField.TypeId = FieldTypeCodes.DateTime;
                            break;
                        case ValidFieldColumnDbTypes.Bit:
                            vField.TypeId = FieldTypeCodes.Boolean;
                            break;
                        case ValidFieldColumnDbTypes.Ntext:
                        case ValidFieldColumnDbTypes.Text:
                            vField.TypeId = FieldTypeCodes.Textbox;
                            break;
                        case ValidFieldColumnDbTypes.Nvarchar:
                        case ValidFieldColumnDbTypes.CharVarying:
                            vField.TypeId = stringSize == -1 ? FieldTypeCodes.Textbox : FieldTypeCodes.String;
                            vField.StringSize = stringSize;
                            vField.TextBoxRows = Field.TextBoxRowsDefaultValue;
                            break;
                        default:
                            throw new ApplicationException("Недопустимый тип колонки: " + column.DbType);
                    }

                    result.Add(new Tuple<Field, Field>(vField, null));
                }
            }

            return result;
        }

        /// <summary>
        /// Обновить виртуальные контенты при создании/обновлении поля реального контента
        /// </summary>
        internal void UpdateVirtualFields(Field field)
        {
            var content = ContentRepository.GetById(field.ContentId);
            content.Fields.Single(f => f.Id == field.Id).StoredName = field.StoredName;
            UpdateVirtualSubContents(content);
        }

        /// <summary>
        /// Обновление дочерних виртуальных контентов
        /// </summary>
        private void UpdateVirtualSubContents(Content content)
        {
            var rebuildedViewSubContents = TraverseForUpdateVirtualSubContents(content);
            RebuildSubContentViews(GetVirtualContentsToRebuild(rebuildedViewSubContents.ToArray()));
            UpdateUserQueryAsSubContents(rebuildedViewSubContents);
        }

        public List<Content.TreeItem> TraverseForUpdateVirtualSubContents(Content content)
        {
            var rebuildedViewSubContents = new List<Content.TreeItem>();

            void Traverse(Content parentContent, int level)
            {
                foreach (var subContent in parentContent.VirtualSubContents)
                {
                    try
                    {
                        var needToRebuildView = false;
                        if (subContent.VirtualType == VirtualType.Join)
                        {
                            needToRebuildView = AddNewFieldsToJoinSubContent(parentContent, subContent);
                        }
                        else if (subContent.VirtualType == VirtualType.Union)
                        {
                            needToRebuildView = UpdateUnionAsSubContent(subContent);
                        }
                        else if (subContent.VirtualType == VirtualType.UserQuery)
                        {
                            needToRebuildView = true;
                        }
                        if (needToRebuildView)
                        {
                            rebuildedViewSubContents.Add(new Content.TreeItem { ContentId = subContent.Id, Level = level });
                        }
                    }
                    catch (VirtualContentProcessingException)
                    {
                        throw;
                    }
                    catch (Exception exp)
                    {
                        throw new VirtualContentProcessingException(subContent, exp);
                    }

                    Traverse(subContent, level + 1);
                }
            }

            // --- обновить виртуальные поля созданные на основе полей родительского контента ВО ВСЕХ JOIN-КОНТЕНТАХ которые содержат такие поля ---
            // определить в каких join-контентах есть виртуальные поля построенные на основе полей родительского контента
            var joinRelatedContents = VirtualContentRepository.GetJoinRelatedContents(content);

            // обновить поля найденных виртуальных контентов
            foreach (var jrc in joinRelatedContents)
            {
                try
                {
                    var needToRebuildView = UpdateJoinRelatedContentFields(content, jrc);
                    if (needToRebuildView)
                    {
                        Traverse(jrc, 1);
                        rebuildedViewSubContents.Add(new Content.TreeItem { ContentId = jrc.Id, Level = 0 });
                    }
                }
                catch (VirtualContentProcessingException)
                {
                    throw;
                }
                catch (Exception exp)
                {
                    throw new VirtualContentProcessingException(jrc, exp);
                }
            }

            // Обновление дочерних контентов
            Traverse(content, 0);
            return rebuildedViewSubContents;
        }

        /// <summary>
        /// Обновление Join-контента
        /// </summary>
        internal Content UpdateJoinContent(Content content, Content dbContent)
        {
            if (content.VirtualJoinFieldNodes.Any())
            {
                // сначала удалить поля в подчиненных контентах
                RemoveSubContentVirtualFields(content, Except(content.VirtualJoinFieldNodes, dbContent.VirtualJoinFieldNodes).ToList());

                // удалить из БД удаляемые поля
                RemoveJoinContentFields(content.VirtualJoinFieldNodes, dbContent.VirtualJoinFieldNodes);

                // Сохранить новые поля
                SaveJoinContentFields(content.VirtualJoinFieldNodes.ToList(), dbContent);

                dbContent = ContentRepository.GetById(dbContent.Id);

                // перестроить view
                DropContentViews(dbContent);
                CreateContentViews(dbContent);

                // Обновить все дочерние контенты
                UpdateVirtualSubContents(dbContent);
            }

            return dbContent;
        }

        /// <summary>
        /// Добавление новых полей в дочерний Join-контент
        /// </summary>
        private bool AddNewFieldsToJoinSubContent(Content parentContent, Content virtualContent)
        {
            // виртуальные поля верхнего уровня
            var rootFields = FieldRepository.GetList(virtualContent.VirtualJoinFieldNodes.Select(n => n.Id));

            // получить id базовых полей для полей верхнего уровня дочернего join-контента
            var rootBaseFieldIDs = rootFields.Select(f => f.PersistentId.Value).Distinct();

            // --- добавить в дочерний join-контент на первый уровень те поля, которых там нет, но которые добавлены в родительский контент ---
            // получить id новых базовых полей на основе которых нужно создать виртуальные поля верхнего уровня в дочернем join-контенте
            var newFieldIds = parentContent.Fields.Select(f => f.Id).Except(rootBaseFieldIDs).ToList();

            // создать ноды для новых полей верхнего уровня
            var newFieldNodes = newFieldIds.Select(id => new Content.VirtualFieldNode
            {
                Id = id,
                TreeId = Content.VirtualFieldNode.GetFieldTreeId(id)
            });

            if (newFieldIds.Any())
            {
                // Добавить новые поля в корень дочернего контента
                var newVirtualFieldNodes = new List<Content.VirtualFieldNode>(virtualContent.VirtualJoinFieldNodes);
                newVirtualFieldNodes.AddRange(newFieldNodes);
                virtualContent.VirtualJoinFieldNodes = newVirtualFieldNodes;

                // Сохранить новую коллекцию полей дочернего контента
                var updatingContent = ContentRepository.GetById(virtualContent.Id);
                SaveJoinContentFields(virtualContent.VirtualJoinFieldNodes.ToList(), updatingContent);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Обновление полей в связанном Join контенте
        /// </summary>
        private bool UpdateJoinRelatedContentFields(Content content, Content relatedContent)
        {
            var needToViewRebuild = false;

            // получить словарь полей родительского контента
            var rootPersintentField = content.Fields.ToDictionary(f => f.Id);

            // получить словарь имен виртуальных O2M полей
            var joinFieldsIDs = relatedContent.Fields.Select(f => f.JoinId).Where(j => j.HasValue).Select(j => j.Value).Distinct();
            var joinFields = FieldRepository.GetList(joinFieldsIDs);
            var joinFieldNames = joinFields.ToDictionary(f => f.Id, f => f.Name);

            // определить, у каких виртуальных полей значимые параметры отличаются от значимых параметров соответствующих полей родительского контента
            // и обновить значимые параметры у таких виртуальных полей
            // (тут не нужен проход по иерархии виртуальных полей , так как значимые параметры полей типа O2M нельзя менять если на таких полях построены виртуальные поля)
            foreach (var vfield in relatedContent.Fields)
            {
                if (rootPersintentField.ContainsKey(vfield.PersistentId.Value))
                {
                    var pfield = rootPersintentField[vfield.PersistentId.Value];
                    var isChanged = false;

                    // ---- Проверка на изменение имени ---
                    // определить, имя виртуального поля пользовательское или нет ?
                    var vNameHasUserName = false;
                    if (vfield.JoinId.HasValue)
                    {
                        var joinFieldName = joinFieldNames[vfield.JoinId.Value];
                        var generatedName = joinFieldName + "." + pfield.StoredName;
                        vNameHasUserName = !generatedName.Equals(vfield.Name, StringComparison.InvariantCultureIgnoreCase);

                        // если имя пользовательское, и едет апдейт дочерного join-контента, то этот дочерний контент точно нужно перестроить
                        needToViewRebuild = needToViewRebuild || vNameHasUserName;
                    }

                    // если имя поля автосгенерировано, то перегенерировать имя поле
                    if (!vNameHasUserName)
                    {
                        // получить часть иерархического имени виртуального поля, производного от имени базового поля
                        // например для имени "m1.m2.m3.Title" нужно получить Title
                        var pfn = GetPersistentFieldName(vfield.Name);
                        if (!pfn.Equals(pfield.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            vfield.Name = ReplacePersistentFieldName(vfield.Name, pfield.Name);
                            isChanged = true;
                            needToViewRebuild = true;
                        }
                    }

                    //--------------------

                    UpdateImportantFieldAttrribute(pfield, vfield, ref isChanged, ref needToViewRebuild);

                    // если необходимо - то изменить
                    if (isChanged)
                    {
                        VirtualFieldRepository.Update(vfield);
                    }
                }
            }

            return needToViewRebuild;
        }

        /// <summary>
        /// Обновляет Union-контент
        /// </summary>
        internal Content UpdateUnionContent(Content content, Content dbContent)
        {
            // Если изменился состав контентов - то сохранить привязку контентов в БД
            var contentSourceListIsChanged = content.UnionSourceContentIDs.Count() != dbContent.UnionSourceContentIDs.Count() || content.UnionSourceContentIDs.Except(dbContent.UnionSourceContentIDs).Any();
            if (contentSourceListIsChanged)
            {
                VirtualContentRepository.RecreateUnionSourcesInfo(dbContent, content.UnionSourceContentIDs);
            }

            // Обновить состав полей Union-контента
            var isUpdated = UpdateUnionContentFieldCollection(content, dbContent);
            if (isUpdated || contentSourceListIsChanged)
            {
                dbContent = ContentRepository.GetById(dbContent.Id);

                // перестроить view
                DropContentViews(dbContent);
                CreateContentViews(dbContent);

                // Обновить все дочерние контенты
                UpdateVirtualSubContents(dbContent);
            }

            return dbContent;
        }

        /// <summary>
        /// Обновить состав полей Union-контента
        /// </summary>
        private bool UpdateUnionContentFieldCollection(Content content, Content dbContent)
        {
            // Определить какие поля необходимо удалить и какие добавить
            var makeoutResult = MakeOutUnionFields(content, dbContent);
            if (makeoutResult.NewFieldsToAdd.Any() || makeoutResult.FieldsToRemove.Any() || makeoutResult.FieldsToRemain.Any())
            {
                // удалить поля в подчиненных контентах
                RemoveSubContentVirtualFields(content, makeoutResult.FieldsToRemove.Select(f => f.Id).ToList());

                // Удалить привязки полей
                VirtualFieldRepository.RemoveUnionAttrs(dbContent);

                // удалить поля из БД
                RemoveContentFields(makeoutResult.FieldsToRemove.Select(f => f.Id));

                var maxOrder = dbContent.GetMaxFieldsOrder();

                // Сохранить новые поля
                foreach (var f in makeoutResult.NewFieldsToAdd)
                {
                    if (_forceNewFieldIds != null)
                    {
                        f.ForceId = _forceNewFieldIds.Dequeue();
                    }

                    if (QPContext.DatabaseType != DatabaseType.SqlServer)
                    {
                        f.Order = maxOrder + _newFieldIds.Count + 1;
                    }

                    var fResult = VirtualFieldRepository.Save(f);
                    _newFieldIds.Add(fResult.Id);
                    f.Id = fResult.Id;
                }

                // Обновить измененные поля
                UpdateContentFields(makeoutResult.NewStateRemainingFields, makeoutResult.FieldsToRemain);

                // Обновить привязку полей
                VirtualFieldRepository.RebuildUnionAttrs(dbContent);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Обновить изменившиеся поля контента
        /// </summary>
        private static void UpdateContentFields(IEnumerable<Field> newStateFields, IEnumerable<Field> oldStateFields)
        {
            var bflds = newStateFields.ToDictionary(f => f.Id);
            foreach (var uf in oldStateFields)
            {
                var needToViewRebuild = false;
                var isChanged = false;

                UpdateImportantFieldAttrribute(bflds[uf.Id], uf, ref isChanged, ref needToViewRebuild, true);
                if (isChanged)
                {
                    VirtualFieldRepository.Update(uf);
                }
            }
        }

        /// <summary>
        /// Обновить Union-контент как подчиненный
        /// </summary>
        private bool UpdateUnionAsSubContent(Content unionContent)
        {
            var makeoutResult = MakeOutUnionFields(unionContent, unionContent);
            if (makeoutResult.NewFieldsToAdd.Any() || makeoutResult.FieldsToRemove.Any() || makeoutResult.FieldsToRemain.Any())
            {
                VirtualFieldRepository.RemoveUnionAttrs(unionContent);

                // удалить поля из БД
                RemoveContentFields(makeoutResult.FieldsToRemove.Select(f => f.Id));

                var maxOrder = unionContent.GetMaxFieldsOrder();

                // Сохранить новые поля в БД
                foreach (var f in makeoutResult.NewFieldsToAdd)
                {
                    if (_forceNewFieldIds != null)
                    {
                        f.ForceId = _forceNewFieldIds.Dequeue();
                    }

                    if (QPContext.DatabaseType != DatabaseType.SqlServer)
                    {
                        f.Order = maxOrder + _newFieldIds.Count + 1;
                    }

                    var fResult = VirtualFieldRepository.Save(f);
                    _newFieldIds.Add(fResult.Id);
                    f.Id = fResult.Id;
                }

                // Обновить измененные поля
                UpdateContentFields(makeoutResult.NewStateRemainingFields, makeoutResult.FieldsToRemain);

                // Обновить привязку полей
                VirtualFieldRepository.RebuildUnionAttrs(unionContent);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Определить какие поля необходимо удалить и какие добавить
        /// </summary>
        private static UpdateFieldCollectionOperationParams MakeOutUnionFields(Content content, Content dbContent)
        {
            // Базовые поля Union-контента уникальные по имени
            IEnumerable<Field> allBaseFieldsAfterUpdate = VirtualContentRepository.GetFieldsOfContents(content.UnionSourceContentIDs)
                .Distinct(Field.NameComparer)
                .OrderBy(f => f.Order)
                .ToArray();

            // Информация о соответствии базовых полей и полей Union-контента
            IEnumerable<VirtualFieldsRelation> v2bRelations = VirtualFieldRepository.GetVirtualSubFields(allBaseFieldsAfterUpdate.Select(f => f.Id).ToList())
                .Where(r => r.VirtualFieldContentId == content.Id)
                .ToArray();

            // --- сначала обрабатываются виртуальные поля Union-контента, которые имеют только одно базовое поле ---
            IEnumerable<VirtualFieldsRelation> o2oRelations = v2bRelations
                .GroupBy(r => r.VirtualFieldId)
                .Where(g => g.Count() < 2)
                .SelectMany(g => g)
                .ToArray();

            // id виртуальных полей, которые должны остаться
            var fieldToRemainIDs = o2oRelations.Select(r => r.VirtualFieldId).Distinct().ToList();

            // id базовых полей, виртуальные поля для которых должны быть
            IEnumerable<int> baseFieldToRemainIDs = o2oRelations.Select(r => r.BaseFieldId).Distinct().ToArray();

            // id виртуальных полей которые должны быть удалены
            var fieldToRemoveIDs = dbContent.Fields.Select(f => f.Id).Except(fieldToRemainIDs).Distinct().ToList();

            // Базовые поля для новых виртуальных полей
            var baseFieldsToAddIds = allBaseFieldsAfterUpdate
                .Where(f => !baseFieldToRemainIDs.Contains(f.Id))
                .Select(f => f.Id)
                .ToList();

            //-----------------------------

            // --- теперь обрабатываются виртуальные поля Union-контента, которые имеют несколько базовых полей ---
            var o2mRelationGroups = v2bRelations
                .GroupBy(r => r.VirtualFieldId)
                .Where(g => g.Count() > 1)
                .ToArray();

            foreach (var o2mRelations in o2mRelationGroups)
            {
                // так как allBaseFieldsAfterUpdate уникальны по имени, то обрабатываемые тут базовые поля имеют разные имена
                // поэтому удаляем виртуальное поле и создаем новые, для уникальных по имени базовых полей
                fieldToRemoveIDs.Add(o2mRelations.Key);

                IEnumerable<int> baseFieldIDs = o2mRelations.Select(r => r.BaseFieldId).Distinct().ToArray();
                baseFieldsToAddIds.AddRange(baseFieldIDs);
            }

            //------------------

            // --- получить окончательный результат ---
            IEnumerable<Field> fieldToRemain = dbContent.Fields.Where(f => fieldToRemainIDs.Contains(f.Id)).ToArray();

            // получить поля в состоянии после обновления
            var newStateRemainingFields = new List<Field>(fieldToRemainIDs.Count);
            foreach (var rid in fieldToRemainIDs)
            {
                var bfid = v2bRelations.Single(r => r.VirtualFieldId == rid).BaseFieldId;
                var faupd = allBaseFieldsAfterUpdate.Single(f => f.Id == bfid).GetVirtualClone(dbContent);
                faupd.Id = rid;
                newStateRemainingFields.Add(faupd);
            }

            baseFieldsToAddIds = baseFieldsToAddIds.Distinct().ToList();
            IEnumerable<Field> newFieldsToAdd = allBaseFieldsAfterUpdate
                .Where(f => baseFieldsToAddIds.Contains(f.Id))
                .Distinct(Field.NameComparer)
                .Select(f => f.GetVirtualClone(dbContent))
                .ToArray();

            IEnumerable<Field> fieldToRemove = dbContent.Fields.Where(f => fieldToRemoveIDs.Contains(f.Id)).ToArray();

            return new UpdateFieldCollectionOperationParams
            {
                NewStateRemainingFields = newStateRemainingFields.ToArray(),
                NewFieldsToAdd = newFieldsToAdd,
                FieldsToRemove = fieldToRemove,
                FieldsToRemain = fieldToRemain
            };

            //----------------
        }

        /// <summary>
        /// Обновить UQ-контент
        /// </summary>
        internal Content UpdateUserQueryContent(Content content, Content dbContent)
        {
            IEnumerable<int> contentsBeforeUpdate = content.UserQueryContentViewSchema.SelectUniqContentIDs().OrderBy(i => i);

            // контент уже обновлен так что можно перестроить view
            DropContentViews(dbContent);
            CreateContentViews(dbContent, false);

            IEnumerable<int> contentsAfterUpdate = dbContent.UserQueryContentViewSchema.SelectUniqContentIDs().OrderBy(i => i);

            // пересоздать привязку контентов в БД, если необходимо
            var contentSourceListIsChanged = !contentsBeforeUpdate.SequenceEqual(contentsAfterUpdate);
            if (contentSourceListIsChanged)
            {
                VirtualContentRepository.RecreateUserQuerySourcesInfo(dbContent);
            }

            if (CheckCycleInGraph())
            {
                throw new CycleInContentGraphException();
            }

            // Обновить состав полей UQ-контента
            var isUpdated = UpdateUserQueryContentFieldCollection(content, dbContent);
            if (isUpdated || contentSourceListIsChanged)
            {
                // Обновить все дочерние контенты
                UpdateVirtualSubContents(dbContent);
            }

            CreateFrontendViews(dbContent);

            return dbContent;
        }

        /// <summary>
        /// Обновить состав полей UQ-контента
        /// </summary>
        private bool UpdateUserQueryContentFieldCollection(Content content, Content dbContent)
        {
            // определить какие поля удалять, какие оставить и какие добавить
            var makeoutResult = MakeOutUserQueryFields(dbContent);

            if (makeoutResult.NewFieldsToAdd.Any() || makeoutResult.FieldsToRemove.Any() || makeoutResult.FieldsToRemain.Any())
            {
                // удалить поля в подчиненных контентах
                RemoveSubContentVirtualFields(content, makeoutResult.FieldsToRemove.Select(f => f.Id).ToList());

                // Удалить привязки полей
                VirtualFieldRepository.RemoveUserQueryAttrs(dbContent);

                // удалить поля из БД
                RemoveContentFields(makeoutResult.FieldsToRemove.Select(f => f.Id));

                var maxOrder = content.GetMaxFieldsOrder();

                // Сохранить новые поля
                foreach (var f in makeoutResult.NewFieldsToAdd)
                {
                    if (_forceNewFieldIds != null)
                    {
                        f.ForceId = _forceNewFieldIds.Dequeue();
                    }

                    if (QPContext.DatabaseType != DatabaseType.SqlServer)
                    {
                        f.Order = maxOrder + _newFieldIds.Count + 1;
                    }

                    var fResult = VirtualFieldRepository.Save(f);
                    _newFieldIds.Add(fResult.Id);
                    f.Id = fResult.Id;
                }

                // Обновить измененные поля
                UpdateContentFields(makeoutResult.NewStateRemainingFields, makeoutResult.FieldsToRemain);

                // Обновить привязку полей
                VirtualFieldRepository.RebuildUserQueryAttrs(dbContent);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Обновить все дочерние UQ-контенты
        /// </summary>
        /// <param name="rebuildedViewSubContents"></param>
        private void UpdateUserQueryAsSubContents(List<Content.TreeItem> rebuildedViewSubContents)
        {
            if (rebuildedViewSubContents.Any())
            {
                // Уникальные контенты по id
                var uniqueItems = rebuildedViewSubContents.Distinct(new LambdaEqualityComparer<Content.TreeItem>((x, y) => x.ContentId.Equals(y.ContentId), x => x.ContentId)).ToList();
                var updatedContents = ContentRepository.GetList(uniqueItems.Select(c => c.ContentId)).Where(c => c.VirtualType == VirtualType.UserQuery);
                var updatedContentsDict = updatedContents.ToDictionary(c => c.Id);
                var uqContents = uniqueItems
                    .Where(c => updatedContentsDict.ContainsKey(c.ContentId))
                    .OrderBy(c => c.Level)
                    .Select(c => updatedContentsDict[c.ContentId]);

                // сортируем контенты по уровню иерархии и пересоздаем вью в соответствии с порядком в иерархии
                foreach (var uqcontent in uqContents)
                {
                    UpdateUserQueryAsSubContent(uqcontent);
                }
            }
        }

        /// <summary>
        /// Обновить состав полей UQ-контента
        /// </summary>
        public bool UpdateUserQueryAsSubContent(Content dbContent)
        {
            try
            {
                // определить какие поля удалять, какие оставить и какие добавить
                var makeoutResult = MakeOutUserQueryFields(dbContent);
                if (makeoutResult.NewFieldsToAdd.Any() || makeoutResult.FieldsToRemove.Any() || makeoutResult.FieldsToRemain.Any())
                {
                    // Удалить привязки полей
                    VirtualFieldRepository.RemoveUserQueryAttrs(dbContent);

                    var maxOrder = dbContent.GetMaxFieldsOrder();

                    // удалить поля из БД
                    RemoveContentFields(makeoutResult.FieldsToRemove.Select(f => f.Id));

                    // Сохранить новые поля
                    foreach (var f in makeoutResult.NewFieldsToAdd)
                    {
                        if (_forceNewFieldIds != null)
                        {
                            f.ForceId = _forceNewFieldIds.Dequeue();
                        }
                        if (QPContext.DatabaseType != DatabaseType.SqlServer)
                        {
                            f.Order = maxOrder + _newFieldIds.Count + 1;
                        }

                        var fResult = VirtualFieldRepository.Save(f);
                        _newFieldIds.Add(fResult.Id);

                        f.Id = fResult.Id;
                    }

                    // Обновить измененные поля
                    UpdateContentFields(makeoutResult.NewStateRemainingFields, makeoutResult.FieldsToRemain);

                    // Обновить привязку полей
                    VirtualFieldRepository.RebuildUserQueryAttrs(dbContent);

                    // сохранить записи в таблице user_query_contents
                    VirtualContentRepository.RecreateUserQuerySourcesInfo(dbContent);
                    return true;
                }

                return false;
            }
            catch (VirtualContentProcessingException)
            {
                throw;
            }
            catch (Exception exp)
            {
                throw new VirtualContentProcessingException(dbContent, exp);
            }
        }

        /// <summary>
        /// Определить какие поля необходимо удалить и какие добавить
        /// </summary>
        private static UpdateFieldCollectionOperationParams MakeOutUserQueryFields(Content dbContent)
        {
            var newFieldsAfterUpdateInfo = CreateFieldsForUserQueryColumns(dbContent, dbContent.UserQueryContentViewSchema, false).ToList();

            // получить базовые поля
            IEnumerable<Field> allBaseFieldsAfterUpdate = newFieldsAfterUpdateInfo.Where(t => t.Item2 != null).Select(t => t.Item2).ToArray();

            // определить что делать с полями основанными на полях контента
            var upateParamsForContentBasedFields = MakeOutUserQueryFields(dbContent, allBaseFieldsAfterUpdate.ToList());

            // определить что делать со свободными полями
            // получить несвободные поля контента
            IEnumerable<int> notFreeFieldIDs = VirtualFieldRepository.GetVirtualBaseFieldIDs(dbContent.Fields.Select(f => f.Id).ToList())
                .Select(r => r.VirtualFieldId)
                .Distinct()
                .ToArray();

            IEnumerable<Field> freeFields = dbContent.Fields.Where(f => !notFreeFieldIDs.Contains(f.Id)).ToArray();

            //IEnumerable<Field> freeFields = dbContent.Fields.Where(f => !f.GetVirtualBaseFieldIDs().Any()).ToArray();

            //получить требуемые свободные поля
            IEnumerable<Field> allFreeFieldsAfterUpdate = newFieldsAfterUpdateInfo.Where(t => t.Item2 == null).Select(t => t.Item1).ToArray();

            // определить свободные поля которые необходимо удалить
            IEnumerable<Field> freeFieldsToRemove = freeFields.Except(allFreeFieldsAfterUpdate, Field.NameComparer).ToArray();

            // определить свободные поля которые необходимо добавить
            IEnumerable<Field> freeFieldsToAdd = allFreeFieldsAfterUpdate.Except(freeFields, Field.NameComparer).ToArray();

            // определить свободные поля которые остаются
            IEnumerable<Field> freeFieldsToRemain = freeFields.Except(freeFieldsToRemove, Field.NameComparer).ToArray();

            // проставить id соответствующим полям
            foreach (var rff in freeFieldsToRemain)
            {
                allFreeFieldsAfterUpdate.Single(f => Field.NameComparerPredicate(f.Name, rff.Name)).Id = rff.Id;
            }

            return new UpdateFieldCollectionOperationParams
            {
                NewStateRemainingFields = upateParamsForContentBasedFields.NewStateRemainingFields.Concat(allFreeFieldsAfterUpdate).ToArray(),
                NewFieldsToAdd = upateParamsForContentBasedFields.NewFieldsToAdd.Concat(freeFieldsToAdd).ToArray(),
                FieldsToRemain = upateParamsForContentBasedFields.FieldsToRemain.Concat(freeFieldsToRemain).ToArray(),
                FieldsToRemove = upateParamsForContentBasedFields.FieldsToRemove.Concat(freeFieldsToRemove).ToArray()
            };
        }

        private static UpdateFieldCollectionOperationParams MakeOutUserQueryFields(Content updatedContent, List<Field> allBaseFieldsAfterUpdate)
        {
            IEnumerable<VirtualFieldsRelation> v2bRelations = VirtualFieldRepository.GetVirtualSubFields(allBaseFieldsAfterUpdate.Select(f => f.Id).ToList())
                .Where(r => r.VirtualFieldContentId == updatedContent.Id)
                .ToArray();

            // рассматриваем только поля основанные на полях контентов
            IEnumerable<int> dbContentFieldIds = VirtualFieldRepository.GetVirtualBaseFieldIDs(updatedContent.Fields.Select(f => f.Id).ToList())
                .Select(r => r.VirtualFieldId)
                .ToList();

            var dbContentFields = FieldRepository.GetList(dbContentFieldIds).ToList();

            // id виртуальных полей, которые должны остаться
            IEnumerable<int> fieldToRemainIDs = v2bRelations.Select(r => r.VirtualFieldId).Distinct().ToArray();

            // id базовых полей виртуальных полей, которые должны остаться
            IEnumerable<int> baseFieldToRemainIDs = v2bRelations.Select(r => r.BaseFieldId).Distinct().ToArray();

            // id виртуальных полей которые должны быть удалены
            IEnumerable<int> fieldToRemoveIDs = dbContentFields.Select(f => f.Id).Except(fieldToRemainIDs).ToArray();

            // получить поля
            IEnumerable<Field> newFieldsToAdd = allBaseFieldsAfterUpdate
                .Where(f => !baseFieldToRemainIDs.Contains(f.Id))
                .Select(f => f.GetVirtualClone(updatedContent))
                .ToArray();

            IEnumerable<Field> fieldToRemain = dbContentFields.Where(f => fieldToRemainIDs.Contains(f.Id)).ToArray();
            IEnumerable<Field> fieldToRemove = dbContentFields.Where(f => fieldToRemoveIDs.Contains(f.Id)).ToArray();

            // получить поля в состоянии после обновления
            var newStateRemainingFields = new List<Field>(fieldToRemainIDs.Count());
            foreach (var rid in fieldToRemainIDs)
            {
                var bfid = v2bRelations.Single(r => r.VirtualFieldId == rid).BaseFieldId;
                var faupd = allBaseFieldsAfterUpdate.Single(f => f.Id == bfid).GetVirtualClone(updatedContent);
                faupd.Id = rid;
                newStateRemainingFields.Add(faupd);
            }

            return new UpdateFieldCollectionOperationParams
            {
                NewStateRemainingFields = newStateRemainingFields.ToArray(),
                NewFieldsToAdd = newFieldsToAdd,
                FieldsToRemove = fieldToRemove,
                FieldsToRemain = fieldToRemain
            };
        }

        /// <summary>
        /// Удаляет поля в подчиненных контентах
        /// </summary>
        public IEnumerable<Content.TreeItem> RemoveSubContentVirtualFields(Content content, ICollection<int> removingFieldIds)
        {
            var rebuildedViewSubContents = new List<Content.TreeItem>();
            FieldRepository.GetList(removingFieldIds);

            void Traverse(Content parentContent, int level, List<int> baseRemovingFields)
            {
                foreach (var subContent in parentContent.VirtualSubContents)
                {
                    try
                    {
                        // реализовать получение списка наследников удаляемых полей (curlevRemovingFields) на текущем уровне иерархии контентов (subContent)
                        var curlevRemovingFieldIds = VirtualFieldRepository.GetVirtualSubFields(baseRemovingFields).Where(r => r.VirtualFieldContentId == subContent.Id).Select(r => r.VirtualFieldId).ToList();

                        // вниз по иерархии
                        Traverse(subContent, level + 1, curlevRemovingFieldIds);

                        var needToRebuildView = false;
                        switch (subContent.VirtualType)
                        {
                            case VirtualType.Union:
                                needToRebuildView = RemoveChildUnionVirtualFields(curlevRemovingFieldIds, subContent, baseRemovingFields);
                                break;
                            case VirtualType.UserQuery:
                                needToRebuildView = RemoveChildUserQueryVirtualFields(curlevRemovingFieldIds, subContent);
                                break;
                        }

                        if (needToRebuildView)
                        {
                            rebuildedViewSubContents.Add(new Content.TreeItem { ContentId = subContent.Id, Level = level });
                        }
                    }
                    catch (VirtualContentProcessingException)
                    {
                        throw;
                    }
                    catch (Exception exp)
                    {
                        throw new VirtualContentProcessingException(subContent, exp);
                    }
                }
            }

            // определить в каких join-контентах есть виртуальные поля построенные на основе полей родительского контента
            var joinRelatedContents = VirtualContentRepository.GetJoinRelatedContents(content).ToList();
            if (joinRelatedContents.Any())
            {
                VirtualFieldRepository.GetVirtualSubFields(removingFieldIds.ToList());

                // удалить поля в найденных виртуальных контентов
                foreach (var jrc in joinRelatedContents)
                {
                    try
                    {
                        // Получить иерархию удаляемых полей JOIN-контента
                        var removingFieldNodes = GetRemovingFieldNodes(jrc, removingFieldIds);

                        // получить полный список удаляемых из JOIN полей
                        var allRemovingSubFieldsIds = Content.VirtualFieldNode.Linearize(removingFieldNodes).Select(n => n.Id).ToList();

                        // удалить поля в дочерних Union и UQ контентах
                        Traverse(jrc, 1, allRemovingSubFieldsIds);

                        var needToRebuildView = RemoveJoinRelatedContentFields(removingFieldNodes);
                        if (needToRebuildView)
                        {
                            rebuildedViewSubContents.Add(new Content.TreeItem { ContentId = jrc.Id, Level = 0 });
                        }
                    }
                    catch (VirtualContentProcessingException)
                    {
                        throw;
                    }
                    catch (Exception exp)
                    {
                        throw new VirtualContentProcessingException(jrc, exp);
                    }
                }
            }

            // обновить Union и UQ контенты
            Traverse(content, 0, removingFieldIds.ToList());

            return rebuildedViewSubContents;
        }

        /// <summary>
        /// Удаляет связанную с контентом информацию
        /// </summary>
        /// <param name="content"></param>
        internal void RemoveContentData(Content content)
        {
            var dbContentVersion = ContentRepository.GetById(content.Id);
            if (content.StoredVirtualType == VirtualType.Join)
            {
                DropContentViews(content);

                // сначала удалить поля в подчиненных контентах
                var rebuildedSubContentViews = RemoveSubContentVirtualFields(content, Except(Enumerable.Empty<Content.VirtualFieldNode>(), dbContentVersion.VirtualJoinFieldNodes).ToList());
                RebuildSubContentViews(GetVirtualContentsToRebuild(rebuildedSubContentViews.ToArray()));
                RemoveJoinContentFields(Enumerable.Empty<Content.VirtualFieldNode>(), dbContentVersion.VirtualJoinFieldNodes);
            }
            else if (content.StoredVirtualType == VirtualType.Union)
            {
                VirtualFieldRepository.RemoveUnionAttrs(content);
                VirtualContentRepository.RemoveUnionSourcesInfo(content);
                foreach (var f in content.Fields)
                {
                    f.Die(false);
                }

                DropContentViews(content);
            }
            else if (content.StoredVirtualType == VirtualType.UserQuery)
            {
                VirtualFieldRepository.RemoveUserQueryAttrs(content);
                VirtualContentRepository.RemoveUserQuerySourcesInfo(content);
                foreach (var f in content.Fields)
                {
                    f.Die(false);
                }

                DropContentViews(content);
            }
        }

        /// <summary>
        /// Удалить дочерние виртуальные поля из JOIN-контента
        /// </summary>
        [SuppressMessage("ReSharper", "ConvertToLocalFunction")]
        private static bool RemoveJoinRelatedContentFields(IEnumerable<Content.VirtualFieldNode> removingFieldNodes)
        {
            var result = false;

            // check local function conversion
            Action<IEnumerable<Content.VirtualFieldNode>> removingTraverse = null;
            removingTraverse = nodes =>
            {
                foreach (var node in nodes)
                {
                    removingTraverse(node.Children);
                    Field.Die(node.Id, false);
                    result = true;
                }
            };

            removingTraverse(removingFieldNodes);
            return result;
        }

        /// <summary>
        /// Получить иерархию удаляемых полей JOIN-контента
        /// </summary>
        private static List<Content.VirtualFieldNode> GetRemovingFieldNodes(Content content, IEnumerable<int> removingPersistentFieldIds)
        {
            var contentFieldsDictionary = content.Fields.ToDictionary(f => f.Id);
            var removingPersistentFieldIdsHashset = new HashSet<int>(removingPersistentFieldIds);

            // ветки полей которые надо будет удалить
            var removingFieldNodes = new List<Content.VirtualFieldNode>();

            // идем по дереву полей контента, и если PersisntentID поля есть в коллекции id удаляемых полей, то такое поле и все его наследники должно быть удалено
            // помещаем его в коллекцию удаляемых полей (включая подчиненные)
            void Traverse(IEnumerable<Content.VirtualFieldNode> nodes)
            {
                foreach (var node in nodes)
                {
                    if (contentFieldsDictionary.ContainsKey(node.Id))
                    {
                        var contentFieldPersistentId = contentFieldsDictionary[node.Id].PersistentId;
                        if (contentFieldPersistentId.HasValue && removingPersistentFieldIdsHashset.Contains(contentFieldPersistentId.Value))
                        {
                            // добавляем в удаляемые
                            removingFieldNodes.Add(node);
                        }
                        else
                        {
                            Traverse(node.Children);
                        }
                    }
                }
            }

            Traverse(content.VirtualJoinFieldNodes);
            return removingFieldNodes;
        }

        /// <summary>
        /// Удалить из БД удаляемые поля
        /// </summary>
        [SuppressMessage("ReSharper", "ConvertToLocalFunction")]
        private static void RemoveJoinContentFields(IEnumerable<Content.VirtualFieldNode> newVirtualJoinFieldNodes, IEnumerable<Content.VirtualFieldNode> oldVirtualJoinFieldNodes)
        {
            var virtualFieldsIds = new HashSet<int>(Content.VirtualFieldNode.Linearize(newVirtualJoinFieldNodes).Select(n => n.Id).Distinct());

            // идем по дереву полей (которые в БД) контента, и если id поля нет в новой virtualJoinFieldNodes, то такое поле должно быть удалено
            // check local function conversion
            Action<IEnumerable<Content.VirtualFieldNode>> traverse = null;
            traverse = nodes =>
            {
                foreach (var node in nodes)
                {
                    traverse(node.Children);
                    if (!virtualFieldsIds.Contains(node.Id))
                    {
                        Field.Die(node.Id, false);
                    }
                }
            };

            traverse(oldVirtualJoinFieldNodes);
        }

        /// <summary>
        /// Удалить поля Union
        /// </summary>
        private static void RemoveContentFields(IEnumerable<int> fieldToRemoveIds)
        {
            foreach (var fid in fieldToRemoveIds)
            {
                Field.Die(fid, false);
            }
        }

        /// <summary>
        /// Удаляет дочерние виртуальные поля из UNION-контента
        /// </summary>
        private static bool RemoveChildUnionVirtualFields(List<int> removingUnionFieldIds, Content unionContent, IEnumerable<int> removingBaseFieldIds)
        {
            // теперь отсеять те поля из удаляемых, для которых есть есть более одной связи в union_attr - это значит что в других контентах-источниках есть поля которые могут быть основой для удаляемого поля
            var fieldToRemoveIds = VirtualFieldRepository.GetUnionFieldRelationCount(removingUnionFieldIds)
                .Where(c => c.Count < 2)
                .Select(c => c.UnionFieldId)
                .ToList();

            if (fieldToRemoveIds.Any())
            {
                VirtualFieldRepository.RemoveUnionAttrs(unionContent);
                RemoveContentFields(fieldToRemoveIds);
                VirtualFieldRepository.RebuildUnionAttrs(unionContent);
            }
            else
            {
                // удалить только связи только для удаляемых базовых полей
                VirtualFieldRepository.RemoveUnionAttrs(removingBaseFieldIds.ToList());
            }

            return true;
        }

        /// <summary>
        /// Удаляет контент источник из связанных union-контентов
        /// </summary>
        /// <param name="sourceContent"></param>
        internal void RemoveSourceContentFromUnions(Content sourceContent)
        {
            var dbUnionSubContents = sourceContent.VirtualSubContents.Where(c => c.VirtualType == VirtualType.Union);
            var dbUnionSubContentsDict = dbUnionSubContents.ToDictionary(c => c.Id);

            // получить вторую копию union-контентов
            var unionSubContents = ContentRepository.GetById(sourceContent.Id).VirtualSubContents.Where(c => c.VirtualType == VirtualType.Union).ToList();

            // удалить контент из списка источников копий
            // (по сути смоделировать удаление пользователем контента из списка)
            foreach (var usb in unionSubContents)
            {
                usb.UnionSourceContentIDs = usb.UnionSourceContentIDs.Where(cid => cid != sourceContent.Id).ToArray();
            }

            // обновить union-контенты
            foreach (var usb in unionSubContents)
            {
                try
                {
                    UpdateUnionContent(usb, dbUnionSubContentsDict[usb.Id]);
                }
                catch (VirtualContentProcessingException)
                {
                    throw;
                }
                catch (Exception exp)
                {
                    throw new VirtualContentProcessingException(usb, exp);
                }
            }
        }

        /// <summary>
        /// Удаляет дочерние виртуальные поля из UQ-контента
        /// </summary>
        private static bool RemoveChildUserQueryVirtualFields(List<int> removingUserQueryFieldIds, Content uqContent)
        {
            if (removingUserQueryFieldIds.Any())
            {
                // Удалить привязки полей
                VirtualFieldRepository.RemoveUserQueryAttrs(uqContent);

                // удалить поля из БД
                RemoveContentFields(removingUserQueryFieldIds);

                // Обновить привязку полей
                VirtualFieldRepository.RebuildUserQueryAttrs(uqContent);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Создать вьюхи для контента
        /// </summary>
        internal void CreateContentViews(Content newContent, bool withFront = true)
        {
            if (newContent.VirtualType == VirtualType.Join)
            {
                CreateJoinContentViews(newContent);
            }
            else if (newContent.VirtualType == VirtualType.Union)
            {
                CreateUnionContentViews(newContent);
            }
            else if (newContent.VirtualType == VirtualType.UserQuery)
            {
                CreateUserQueryViews(newContent);
            }

            if (withFront)
            {
                CreateFrontendViews(newContent);
            }
        }

        /// <summary>
        /// Создать вьюхи для JOIN-контента
        /// </summary>
        private void CreateJoinContentViews(Content newContent)
        {
            if (newContent.VirtualType == VirtualType.Join)
            {
                var vfData = VirtualContentRepository.GetJoinFieldData(newContent.Id).ToList();
                var viewCreateDdl = GenerateCreateJoinViewDdl(newContent.Id, newContent.JoinRootId.Value, vfData);
                var asyncViewCreateDdl = GenerateCreateJoinAsyncViewDdl(newContent.Id, newContent.JoinRootId.Value, vfData);
                VirtualContentRepository.RunCreateViewDdl(viewCreateDdl);
                VirtualContentRepository.RunCreateViewDdl(asyncViewCreateDdl);
                VirtualContentRepository.CreateUnitedView(newContent.Id);
            }
        }

        private static void CreateFrontendViews(EntityObject newContent)
        {
            VirtualContentRepository.CreateFrontendViews(newContent.Id);
        }

        /// <summary>
        /// Генерирует DDL-запрос для создания View для JOIN-контента
        /// </summary>
        internal string GenerateCreateJoinViewDdl(int virtualContentId, int joinRootContentId, IEnumerable<VirtualFieldData> virtualFieldsData)
        {
            var schema = DAL.SqlQuerySyntaxHelper.DbSchemaName(QPContext.DatabaseType);
            string viewNameTemplate = $"CREATE VIEW {schema}.content_{{0}} AS ";
            string joinTableNameTemplate = $"{schema}.CONTENT_{{0}} ";
            string rootContentNameTemplate = $"{schema}.CONTENT_{{0}}";
            return GenerateCreateJoinViewDdl(virtualContentId, joinRootContentId, virtualFieldsData.ToList(), viewNameTemplate, joinTableNameTemplate, rootContentNameTemplate);
        }

        /// <summary>
        /// Генерирует DDL-запрос для создания Async View для JOIN-контента
        /// </summary>
        internal string GenerateCreateJoinAsyncViewDdl(int virtualContentId, int joinRootContentId, IEnumerable<VirtualFieldData> virtualFieldsData)
        {
            var schema = DAL.SqlQuerySyntaxHelper.DbSchemaName(QPContext.DatabaseType);
            string viewNameTemplate = $"CREATE VIEW {schema}.content_{{0}}_async AS ";
            string joinTableNameTemplate = $"{schema}.CONTENT_{{0}}_united ";
            string rootContentNameTemplate = $"{schema}.CONTENT_{{0}}_async";
            return GenerateCreateJoinViewDdl(virtualContentId, joinRootContentId, virtualFieldsData.ToList(), viewNameTemplate, joinTableNameTemplate, rootContentNameTemplate);
        }

        /// <summary>
        /// Генерирует DDL-запрос для создания View для JOIN-контента
        /// </summary>
        private static string GenerateCreateJoinViewDdl(int virtualContentId, int joinRootContentId, List<VirtualFieldData> virtualFieldsData, string viewNameTemplate, string joinTableNameTemplate, string rootContentNameTemplate)
        {
            var dbType = QPContext.DatabaseType;
            var schema = DAL.SqlQuerySyntaxHelper.DbSchemaName(dbType);
            var withNolock = DAL.SqlQuerySyntaxHelper.WithNoLock(dbType);
            Func<string, string> escape = (string name) => DAL.SqlQuerySyntaxHelper.EscapeEntityName(dbType, name);

            const string joinRootTableAlias = "c_0";
            var selectBlock = new StringBuilder();
            selectBlock.AppendFormat(viewNameTemplate, virtualContentId);
            selectBlock.AppendFormat("SELECT {0}.CONTENT_ITEM_ID,{0}.STATUS_TYPE_ID,{0}.VISIBLE,{0}.ARCHIVE,{0}.CREATED,{0}.MODIFIED,{0}.LAST_MODIFIED_BY,", joinRootTableAlias);

            var fromBlock = new StringBuilder();
            fromBlock.Append("FROM ").AppendFormat(rootContentNameTemplate, joinRootContentId).AppendFormat(" AS {0} ", joinRootTableAlias);

            if (virtualFieldsData != null && virtualFieldsData.Any())
            {
                void Traverse(string parentTableAlias, VirtualFieldData parentFieldData, int i)
                {
                    var cfTableAlias = string.Concat(parentTableAlias, '_', i);
                    fromBlock.Append("LEFT OUTER JOIN ").AppendFormat(joinTableNameTemplate, parentFieldData.RelateToPersistentContentId).AppendFormat("AS {0} {3} ON {0}.CONTENT_ITEM_ID = {1}.{2} ", cfTableAlias, parentTableAlias, escape(parentFieldData.PersistentName), withNolock);

                    //fromBlock.AppendFormat("LEFT OUTER JOIN dbo.CONTENT_{0} AS {1} WITH (nolock) ON {1}.CONTENT_ITEM_ID = {2}.{3} ", parentFieldData.RelateToPersistentContentId, cfTableAlias, parentTableAlias, parentFieldData.PersistentName);
                    var c = 0;
                    foreach (var cf in virtualFieldsData.Where(f => f.JoinId == parentFieldData.Id))
                    {
                        selectBlock.AppendFormat("{0}.{1} as {2},", cfTableAlias, escape(cf.PersistentName), escape(cf.Name));

                        if (cf.Type == FieldTypeCodes.Relation && cf.RelateToPersistentContentId.HasValue)
                        {
                            Traverse(cfTableAlias, cf, c);
                            c++;
                        }
                    }
                }

                // получить поля верхнего уровня
                var rc = 0;
                foreach (var rf in virtualFieldsData.Where(d => !d.JoinId.HasValue))
                {
                    selectBlock.AppendFormat("{0}.{1} as {2},", joinRootTableAlias, escape(rf.PersistentName), escape(rf.Name));

                    if (rf.Type == FieldTypeCodes.Relation && rf.RelateToPersistentContentId.HasValue)
                    {
                        // далее вниз по иерархии
                        Traverse(joinRootTableAlias, rf, rc);
                        rc++;
                    }
                }
            }

            return selectBlock.Replace(',', ' ', selectBlock.Length - 1, 1).Append(fromBlock).ToString();
        }

        private void CreateUnionContentViews(Content content)
        {
            // Построить словарь по которому можно определить есть ли в контенте поля с указанным именем
            var fieldNameInContentsTemp =
                VirtualContentRepository.GetFieldsOfContents(content.UnionSourceContentIDs)
                    .GroupBy(f => f.Name.ToLowerInvariant())
                    .Select(g => new { key = g.Key, data = new HashSet<int>(g.Select(f => f.ContentId).ToArray()) })
                    .ToArray();

            var fieldNameInSourceContents = fieldNameInContentsTemp.ToDictionary(d => d.key, d => d.data, StringComparer.InvariantCultureIgnoreCase);

            var contentFieldNames = content.Fields.Select(f => f.Name).ToList();
            var viewCreateDdl = GenerateCreateUnionViewDdl(content.Id, content.UnionSourceContentIDs, contentFieldNames, fieldNameInSourceContents);
            var asyncViewCreateDdl = GenerateCreateUnionAsyncViewDdl(content.Id, content.UnionSourceContentIDs, contentFieldNames, fieldNameInSourceContents);
            VirtualContentRepository.RunCreateViewDdl(viewCreateDdl);
            VirtualContentRepository.RunCreateViewDdl(asyncViewCreateDdl);
            VirtualContentRepository.CreateUnitedView(content.Id);
        }

        internal string GenerateCreateUnionViewDdl(int contentId, IEnumerable<int> unionSourceContentIds, IEnumerable<string> contentFieldNames, Dictionary<string, HashSet<int>> fieldNameInSourceContents)
        {
            var schema = DAL.SqlQuerySyntaxHelper.DbSchemaName(QPContext.DatabaseType);
            string viewNameTemplate = $"CREATE VIEW {schema}.content_{{0}} AS ";
            string sourceTableNameTemplate = $"{schema}.CONTENT_{{0}}";
            return GenerateCreateUnionViewDdl(contentId, unionSourceContentIds.ToList(), contentFieldNames.ToList(), fieldNameInSourceContents, viewNameTemplate, sourceTableNameTemplate);
        }

        internal string GenerateCreateUnionAsyncViewDdl(int contentId, IEnumerable<int> unionSourceContentIds, IEnumerable<string> contentFieldNames, Dictionary<string, HashSet<int>> fieldNameInSourceContents)
        {
            var schema = DAL.SqlQuerySyntaxHelper.DbSchemaName(QPContext.DatabaseType);
            string viewNameTemplate = $"CREATE VIEW {schema}.content_{{0}}_async AS ";
            string sourceTableNameTemplate = $"{schema}.CONTENT_{{0}}_async";
            return GenerateCreateUnionViewDdl(contentId, unionSourceContentIds.ToList(), contentFieldNames.ToList(), fieldNameInSourceContents, viewNameTemplate, sourceTableNameTemplate);
        }

        private static string GenerateCreateUnionViewDdl(int contentId, IReadOnlyCollection<int> unionSourceContentIds, IReadOnlyCollection<string> contentFieldNames, IReadOnlyDictionary<string, HashSet<int>> fieldNameInSourceContents, string viewNameTemplate, string sourceTableNameTemplate)
        {
            var dbType = QPContext.DatabaseType;
            var schema = DAL.SqlQuerySyntaxHelper.DbSchemaName(dbType);
            var withNolock = DAL.SqlQuerySyntaxHelper.WithNoLock(dbType);
            Func<string, string> escape = (string name) => DAL.SqlQuerySyntaxHelper.EscapeEntityName(dbType, name);
            var sb = new StringBuilder();
            sb.AppendFormat(viewNameTemplate, contentId);

            var i = unionSourceContentIds.Count;
            foreach (var usId in unionSourceContentIds)
            {
                sb.AppendFormat(" SELECT {0} content_id,content_item_id,created,modified,last_modified_by,status_type_id,visible,archive,", usId);
                var j = contentFieldNames.Count;
                foreach (var fname in contentFieldNames)
                {
                    sb.AppendFormat(fieldNameInSourceContents[fname].Contains(usId) ? "{0} {0}" : "NULL {0}", escape(fname));

                    j--;
                    if (j > 0)
                    {
                        sb.Append(',');
                    }
                }

                sb.Replace(',', ' ', sb.Length - 1, 1);
                sb.Append(" FROM ");
                sb.AppendFormat(sourceTableNameTemplate, usId);

                i--;
                if (i > 0)
                {
                    sb.Append(" UNION ALL");
                }
            }

            return sb.ToString();
        }

        private static void CreateUserQueryViews(Content content)
        {
            try
            {
                const string createViewTemplate = "CREATE VIEW {0}.{1} AS {2}";

                // View
                var viewName = $"content_{content.Id}";
                var schemaName = DAL.SqlQuerySyntaxHelper.DbSchemaName(QPContext.DatabaseType);
                var viewCreateDdl = string.Format(createViewTemplate, schemaName, viewName, content.UserQuery);
                VirtualContentRepository.RunCreateViewDdl(viewCreateDdl);

                // united view
                var unitedViewName = $"content_{content.Id}_united";
                string viewUnitedCreateDdl;
                if (string.IsNullOrEmpty(content.UserQueryAlternative))
                {
                    var unitedViewQuery = $"select * from {viewName}";
                    viewUnitedCreateDdl = string.Format(createViewTemplate, schemaName, unitedViewName, unitedViewQuery);
                }
                else
                {
                    viewUnitedCreateDdl = string.Format(createViewTemplate, schemaName, unitedViewName, content.UserQueryAlternative);
                }

                VirtualContentRepository.RunCreateViewDdl(viewUnitedCreateDdl);
            }
            catch (SqlException ex)
            {
                var message = string.Format(ContentStrings.ErrorInSubContent, content.Name, ex.ErrorsToString());
                throw new UserQueryContentCreateViewException(message);
            }
            catch (NpgsqlException ex)
            {
                var message = string.Format(ContentStrings.ErrorInSubContent, content.Name, ex.Message);
                throw new UserQueryContentCreateViewException(message);
            }
        }

        /// <summary>
        /// Возвращает имена всех View которые создаються для виртуального контента
        /// </summary>
        internal IEnumerable<string> GetVirtualContentAllViewNames(int contentId)
        {
            var contentIdString = contentId.ToString();
            return new[]
            {
                "content_" + contentIdString + "_live",
                "content_" + contentIdString + "_stage",
                "content_" + contentIdString + "_united",
                "content_" + contentIdString + "_async",
                "content_" + contentIdString,
                "content_" + contentIdString + "_live_new",
                "content_" + contentIdString + "_stage_new",
                "content_" + contentIdString + "_united_new",
                "content_" + contentIdString + "_async_new",
                "content_" + contentIdString + "_new"
            };
        }

        /// <summary>
        /// Удалить view контента
        /// </summary>
        public void DropContentViews(Content content)
        {
            foreach (var viewName in GetVirtualContentAllViewNames(content.Id))
            {
                VirtualContentRepository.DropView(viewName);
            }
        }

        /// <summary>
        /// Refresh view контента
        /// </summary>
        internal void RefreshContentViews(Content content)
        {
            foreach (var viewName in GetVirtualContentAllViewNames(content.Id).Where(n => !n.Contains("async")).Reverse())
            {
                VirtualContentRepository.RefreshView(viewName);
            }
        }

        internal void RebuildSubContentViews(Content[] contents)
        {
            foreach (var content in contents)
            {
                RebuildSubContentView(content);
            }
        }

        internal Content[] GetVirtualContentsToRebuild(Content.TreeItem[] rebuildedViewSubContents)
        {
            if (rebuildedViewSubContents.Any())
            {
                // Уникальные контенты по id
                var uniqueItems = rebuildedViewSubContents.Distinct(new LambdaEqualityComparer<Content.TreeItem>((x, y) => x.ContentId.Equals(y.ContentId), x => x.ContentId)).ToList();
                var updatedContents = ContentRepository.GetList(uniqueItems.Select(i => i.ContentId));
                var updatedContentsDict = updatedContents.ToDictionary(c => c.Id);

                // сортируем контенты по уровню иерархии и пересоздаем вью в соответствии с порядком в иерархии
                return uniqueItems.OrderBy(c => c.Level).Select(c => c.ContentId).Select(n => updatedContentsDict[n]).ToArray();
            }

            return new Content[] { };
        }

        public void RebuildSubContentView(Content content)
        {
            try
            {
                if (content.VirtualType == VirtualType.UserQuery && QPContext.DatabaseType == DatabaseType.SqlServer)
                {
                    RefreshContentViews(content);
                }
                else
                {
                    DropContentViews(content);
                    CreateContentViews(content);
                }
            }
            catch (VirtualContentProcessingException)
            {
                throw;
            }
            catch (Exception exp)
            {
                throw new VirtualContentProcessingException(content, exp);
            }
        }

        private static void UpdateImportantFieldAttrribute(Field sourceField, Field destField, ref bool isChanged, ref bool needToViewRebuild, bool matchName = false)
        {
            if (matchName && !destField.Name.Equals(sourceField.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                destField.Name = sourceField.Name;
                isChanged = true;
                needToViewRebuild = true;
            }
            if (destField.TypeId != sourceField.TypeId)
            {
                destField.TypeId = sourceField.TypeId;
                isChanged = true;
                needToViewRebuild = true;
            }

            switch (destField.TypeId)
            {
                case FieldTypeCodes.Textbox:
                {
                    if (destField.TextBoxRows != sourceField.TextBoxRows)
                    {
                        destField.TextBoxRows = sourceField.TextBoxRows;
                        isChanged = true;
                    }
                    break;
                }
                case FieldTypeCodes.VisualEdit:
                {
                    if (destField.VisualEditorHeight != sourceField.VisualEditorHeight)
                    {
                        destField.VisualEditorHeight = sourceField.VisualEditorHeight;
                        isChanged = true;
                    }
                    break;
                }
                case FieldTypeCodes.Numeric:
                {
                    if (destField.DecimalPlaces != sourceField.DecimalPlaces)
                    {
                        destField.DecimalPlaces = sourceField.DecimalPlaces;
                        isChanged = true;
                        needToViewRebuild = true;
                    }
                    break;
                }
                case FieldTypeCodes.String:
                {
                    if (destField.StringSize != sourceField.StringSize)
                    {
                        destField.StringSize = sourceField.StringSize;
                        isChanged = true;
                        needToViewRebuild = true;
                    }
                    break;
                }
            }

            if (destField.BaseImageId != sourceField.BaseImageId)
            {
                destField.BaseImageId = sourceField.BaseImageId;
                isChanged = true;
            }
            if (destField.UseSiteLibrary != sourceField.UseSiteLibrary)
            {
                destField.UseSiteLibrary = sourceField.UseSiteLibrary;
                isChanged = true;
            }

            if (destField.UseForTree != sourceField.UseForTree)
            {
                destField.UseForTree = sourceField.UseForTree;
                isChanged = true;
            }

            if (destField.UseRelationSecurity != sourceField.UseRelationSecurity)
            {
                destField.UseRelationSecurity = sourceField.UseRelationSecurity;
                isChanged = true;
            }

            if (destField.AutoCheckChildren != sourceField.AutoCheckChildren)
            {
                destField.AutoCheckChildren = sourceField.AutoCheckChildren;
                isChanged = true;
            }
        }

        /// <summary>
        /// Возвращает часть иерархического имени виртуального поля, производного от имени базового поля например для имени "m1.m2.m3.Title" нужно получить Title
        /// </summary>
        internal string GetPersistentFieldName(string name)
        {
            var lastPointIndex = name.LastIndexOf('.');
            return lastPointIndex < 0 ? name : name.Remove(0, lastPointIndex + 1);
        }

        /// <summary>
        /// Заменяет часть имени производную от базового поля например для имени "m1.m2.m3.Title"  нужно получить "m1.m2.m3.Header" если persistentFieldName = "Header"
        /// </summary>
        internal string ReplacePersistentFieldName(string oldName, string persistentFieldName)
        {
            var lastPointIndex = oldName.LastIndexOf('.');
            if (lastPointIndex < 0)
            {
                return persistentFieldName;
            }

            return oldName.Remove(lastPointIndex + 1, oldName.Length - lastPointIndex - 1) + persistentFieldName;
        }

        /// <summary>
        /// Определить каких полей нет в newVirtualJoinFieldNodes по сравнению с oldVirtualJoinFieldNodes и вернуть их ID
        /// </summary>
        private static IEnumerable<int> Except(IEnumerable<Content.VirtualFieldNode> newVirtualJoinFieldNodes, IEnumerable<Content.VirtualFieldNode> oldVirtualJoinFieldNodes)
        {
            var newFieldIds = Content.VirtualFieldNode.Linearize(newVirtualJoinFieldNodes).Select(n => n.Id).Distinct();
            var oldFieldIds = Content.VirtualFieldNode.Linearize(oldVirtualJoinFieldNodes).Select(n => n.Id).Distinct();
            return oldFieldIds.Except(newFieldIds).ToArray();
        }

        private static bool CheckCycleInGraph()
        {
            var graph = VirtualContentRepository.GetContentRelationGraph();
            return GraphHepler.CheckCycleInGraph(graph);
        }

        /// <summary>
        /// Возвращает виртуальные поля join-контена которые имеют пользовательское имя
        /// </summary>
        internal List<Field> GetJoinVirtualFieldsWithChangedName(Content parentContent, Content subContent)
        {
            var virtualFieldsWithChangedName = new List<Field>();

            // получить реальные поля для виртуальных полей контента
            var persistentFiedIDs = subContent.Fields.Select(f => f.PersistentId.Value).Distinct();
            var persistentFieds = FieldRepository.GetList(persistentFiedIDs).ToDictionary(f => f.Id);

            // Определить у каких полей дочернего Join-контента имя было изменено
            // сделать это можно сравнив текущее поля с тем, что должно быть сгенерено автоматически
            void JoinFieldsTraverse(EntityObject o2mField, List<Field> fields)
            {
                if (o2mField == null) // первый уровень
                {
                    // на первом уровне ирархии имена порожденных полей должны совпадать с именами пораждающих полей родительского контента
                    // если это не так - то имя поля верхнего уровня было изменено
                    virtualFieldsWithChangedName.AddRange((from scf in parentContent.Fields join f in fields on scf.Id equals f.PersistentId select new { PersistentField = scf, VirtualField = f }).Where(p => !p.PersistentField.Name.Equals(p.VirtualField.Name, StringComparison.InvariantCultureIgnoreCase)).Select(p => p.VirtualField));

                    // Вниз по иерархии полей
                    foreach (var f in fields)
                    {
                        if (f.ExactType == FieldExactTypes.O2MRelation)
                        {
                            f.Name = persistentFieds[f.PersistentId.Value].Name;
                            JoinFieldsTraverse(f, subContent.Fields.Where(cf => cf.JoinId == f.Id).ToList());
                        }
                    }
                }
                else
                {
                    foreach (var f in fields)
                    {
                        // сгенерить имя поля
                        var generatedName = string.Concat(o2mField.Name, '.', persistentFieds[f.PersistentId.Value].Name);

                        // Если сгенеренное имя и реальное имя поля не совпадают, то имя поля было изменено пользователем
                        // добавляем его в коллекцию на основании которой будем проводить валидацию
                        if (!generatedName.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            virtualFieldsWithChangedName.Add(f);
                        }

                        // Вниз по иерархии полей
                        if (f.ExactType == FieldExactTypes.O2MRelation)
                        {
                            f.Name = generatedName; // Это важно, так как на нижних уровнях имена полей должны генериться на основе СГЕНЕРЕННОГО имени родительского  O2M поля
                            JoinFieldsTraverse(f, subContent.Fields.Where(cf => cf.JoinId == f.Id).ToList());
                        }
                    }
                }
            }

            JoinFieldsTraverse(null, subContent.Fields.ToList());
            return virtualFieldsWithChangedName;
        }

        /// <summary>
        /// возвращает рутовые поля
        /// </summary>
        internal IEnumerable<EntityTreeItem> GetRootFieldList(int virtualContentId, int? joinedContentId, string selectItemIDs)
        {
            Content content;
            if (virtualContentId > 0)
            {
                content = ContentRepository.GetById(virtualContentId);

                // если id реального контента отличается от значения из БД для текущего виртуального контента,
                // то получаем поля выбранного пользователем контента
                if (joinedContentId.HasValue && (!content.JoinRootId.HasValue || content.JoinRootId.Value != joinedContentId.Value))
                {
                    content = ContentRepository.GetById(joinedContentId.Value);
                }
            }
            else
            {
                content = ContentRepository.GetById(joinedContentId.Value);
            }

            IEnumerable<EntityTreeItem> GetChildren(Field f, string eid, string alias)
            {
                // если у поля есть выбранные подчиненные поля на любом уровне иерархии, то получаем дочернии поля
                if (f.ExactType == FieldExactTypes.O2MRelation && !string.IsNullOrWhiteSpace(selectItemIDs) && selectItemIDs.IndexOf(eid.TrimEnd(']') + ".", 0, StringComparison.InvariantCultureIgnoreCase) > 0)
                {
                    return GetChildFieldList(eid, alias, GetChildren);
                }

                return Enumerable.Empty<EntityTreeItem>();
            }

            return content.Fields
                .Where(f => f.JoinId == null)
                .Select(f => new EntityTreeItem
                    {
                        Id = Content.VirtualFieldNode.GetFieldTreeId(f.Id),
                        Alias = f.Name,
                        HasChildren = f.RelationId.HasValue,
                        Children = GetChildren(f, Content.VirtualFieldNode.GetFieldTreeId(f.Id), f.Name),
                        Enabled = false,
                        Checked = true,
                        IconUrl = f.Type.Icon,
                        IconTitle = f.Type.Name
                    }
                );
        }

        /// <summary>
        /// Возвращает дерево дочерних полей
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="alias"></param>
        /// <param name="getChildren"></param>
        /// <returns></returns>
        internal IEnumerable<EntityTreeItem> GetChildFieldList(string entityId, string alias, Func<Field, string, string, IEnumerable<EntityTreeItem>> getChildren)
        {
            var result = Enumerable.Empty<EntityTreeItem>();
            var parentFieldId = Content.VirtualFieldNode.ParseFieldTreeId(entityId);
            var field = FieldRepository.GetById(parentFieldId);

            alias = string.IsNullOrWhiteSpace(alias) ? field.Name : alias;
            string GetAlias(Field f) => !f.PersistentId.HasValue ? string.Concat(alias, '.', f.Name) : f.Name;

            if (field.ExactType == FieldExactTypes.O2MRelation)
            {
                var fieldSelect = Enumerable.Empty<Field>().ToList();

                // если поле виртуальное (для типа контента JOIN) - то получить его дочерние виртуальные поля
                if (field.PersistentId.HasValue)
                {
                    fieldSelect = ContentRepository.GetById(field.ContentId).Fields.Where(f => f.JoinId == field.Id).ToList();
                }

                // получить id реальных полей для которых уже получены виртуальные поля
                var joinedFieldPersistentIDs = new HashSet<int>(fieldSelect.Select(f => f.PersistentId.Value).ToArray());

                // добавить только те реальные поля, для которых нет виртуальных
                fieldSelect = fieldSelect.Concat(
                    ContentRepository.GetById(field.RelateToContentId.Value).Fields
                        .Where(f => f.ExactType != FieldExactTypes.M2MRelation && f.ExactType != FieldExactTypes.M2ORelation)
                        .Where(f => !joinedFieldPersistentIDs.Contains(f.Id)
                        )
                ).ToList();

                result = fieldSelect.Select(f => new EntityTreeItem
                {
                    Id = Content.VirtualFieldNode.GetFieldTreeId(f.Id, entityId),
                    Alias = GetAlias(f),
                    HasChildren = f.RelationId.HasValue,
                    Enabled = true,
                    Checked = f.PersistentId.HasValue,
                    IconUrl = f.Type.Icon,
                    IconTitle = f.Type.Name,
                    Children = getChildren(f, Content.VirtualFieldNode.GetFieldTreeId(f.Id, entityId), GetAlias(f))
                });
            }

            return result;
        }
    }
}
