using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Resources;
using System.Data.SqlClient;
using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Helpers
{
    public class VirtualContentHelper
    {
        public VirtualContentHelper()
        {
        
        }
        
        public VirtualContentHelper(IEnumerable<int> forceIds) : this()
        {
            if (forceIds != null && forceIds.Any())
                forceNewFieldIds = new Queue<int>(forceIds);
        }

        private Queue<int> forceNewFieldIds;

        public int[] ForceNewFieldIds
        {
            get
            {
                return (forceNewFieldIds == null) ? null : forceNewFieldIds.ToArray();
            }
        }

        private List<int> newFieldIds = new List<int>();

        public int[] NewFieldIds 
        {
            get
            {
                return newFieldIds.ToArray();
            }
        }

        class UpdateFieldCollectionOperationParams
        {
            public IEnumerable<Field> NewStateRemainingFields { get; set; }
            public IEnumerable<Field> NewFieldsToAdd { get; set; }
            public IEnumerable<Field> FieldsToRemove { get; set; }
            public IEnumerable<Field> FieldsToRemain { get; set; }
        }

        #region save

        #region Join Virtual Content Save
        /// <summary>
        /// Логика сохранения нового Join-Контента
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newContent"></param>
        internal Content SaveJoinContent(Content content, Content newContent)
        {
            SaveJoinContentFields(content.VirtualJoinFieldNodes, newContent);

            CreateContentViews(newContent);

            return ContentRepository.GetById(newContent.Id);
        }

        /// <summary>
        /// Создать поля Join-контента
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newContent"></param>
        private void SaveJoinContentFields(IEnumerable<Content.VirtualFieldNode> newVirtualJoinFieldNodes, Content newContent)
        {
            // получить все поля на основе которых необходимо построить виртуальные поля
            IEnumerable<int> baseFieldsIDs = Content.VirtualFieldNode.Linearize(newVirtualJoinFieldNodes).Select(n => n.Id).Distinct().ToArray();
            IEnumerable<Field> baseFields = FieldRepository.GetList(baseFieldsIDs);
            Dictionary<int, Field> baseFieldsDictionary = baseFields.ToDictionary(bs => bs.Id);
            // Сохранить иерархию полей
            Action<Field, IEnumerable<Content.VirtualFieldNode>> traverse = null;
            traverse = (parentVirtualField, nodes) =>
            {
                // сортируем в том порядке в котором расположены соответствующие базовые поля
                var orderedNodes = nodes.OrderBy(n => baseFieldsDictionary[Content.VirtualFieldNode.ParseFieldTreeId(n.TreeId)].Order);
                foreach (var node in orderedNodes)
                {
                    Field baseField = baseFieldsDictionary[Content.VirtualFieldNode.ParseFieldTreeId(node.TreeId)];
                    Field virtualField = baseField.GetVirtualCloneForJoin(newContent, parentVirtualField);
                    // Сохраняем только новые поля
                    if (!baseField.PersistentId.HasValue)
                    {
                        if (forceNewFieldIds != null)
                            virtualField.ForceId = forceNewFieldIds.Dequeue();
                        Field newVirtualField = VirtualFieldRepository.Save(virtualField);
                        newFieldIds.Add(newVirtualField.Id);
                        // Заменяем id базового поля на id нового виртуального
                        node.Id = newVirtualField.Id;
                        // вниз по иерархии
                        traverse(newVirtualField, node.Children);
                    }
                    else
                        // вниз по иерархии
                        traverse(baseField, node.Children);
                }
            };
            traverse(null, newVirtualJoinFieldNodes);
        }
        #endregion

        #region Union Virtual Content Save
        /// <summary>
        /// Сохраняет новый Union-контент
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newContent"></param>
        internal Content SaveUnionContent(Content content, Content newContent)
        {
            // сохранить записи в таблице union_contents
            VirtualContentRepository.RecreateUnionSourcesInfo(newContent, content.UnionSourceContentIDs);

            SaveUnionContentFields(content.UnionSourceContentIDs, newContent);

            CreateContentViews(newContent);
            return newContent;
        }

        /// <summary>
        /// Сохранить поля нового Union-контента
        /// </summary>
        /// <param name="content"></param>
        /// <param name="newContent"></param>
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
                Field vField = bf.GetVirtualClone(newContent);
                if (forceNewFieldIds != null)
                    vField.ForceId = forceNewFieldIds.Dequeue();
                vField = VirtualFieldRepository.Save(vField);
                newFieldIds.Add(vField.Id);
            }


            VirtualFieldRepository.RebuildUnionAttrs(newContent);
        }

        #endregion

        #region User Query Virtual Content Save
        /// <summary>
        /// Сохранить новый UQ-контент
        /// </summary>
        /// <param name="content"></param>
        /// <param name="dbContent"></param>
        /// <returns></returns>
        internal Content SaveUserQueryContent(Content content, Content dbContent)
        {
            // создать view
            CreateContentViews(dbContent);

            // создать поля
            SaveUserQueryContentFields(dbContent);

            // Обновить привязку полей
            VirtualFieldRepository.RebuildUserQueryAttrs(dbContent);

            // сохранить записи в таблице user_query_contents
            VirtualContentRepository.RecreateUserQuerySourcesInfo(dbContent);

            return dbContent;
        }

        /// <summary>
        /// Сохранить поля нового UQ-контента
        /// </summary>
        /// <param name="dbContent"></param>
        private void SaveUserQueryContentFields(Content dbContent)
        {
            // получить базовые поля			
            IEnumerable<UserQueryColumn> viewColumns = dbContent.UserQueryContentViewSchema;

            IEnumerable<Field> newFields = CreateFieldsForUserQueryColumns(dbContent, viewColumns, true).Select(t => t.Item1);
            foreach (var vField in newFields)
            {
                if (forceNewFieldIds != null)
                    vField.ForceId = forceNewFieldIds.Dequeue();
                Field vFieldResult = VirtualFieldRepository.Save(vField);
                newFieldIds.Add(vFieldResult.Id);
            }
        }

        /// <summary>
        /// Создать поля UQ-контента на основе колонок запроса
        /// </summary>
        /// <param name="dbContent"></param>
        /// <param name="viewColumns"></param>
        /// <returns></returns>
        private IEnumerable<Tuple<Field, Field>> CreateFieldsForUserQueryColumns(Content dbContent, IEnumerable<UserQueryColumn> viewColumns, bool forNew)
        {
            List<Tuple<Field, Field>> result = new List<Tuple<Field, Field>>();

            var groupedColumns = viewColumns.GroupBy
            (
                c => c.ColumnName,
                c => c,
                (name, c) => new {
                    Name = name,
                    DbType = c.Select(n => n.DbType).First(),
                    DecimalPlaces = c.Select(n => n.NumericScale).First(),
                    CharMaxLength = c.Select(n => n.CharMaxLength).First(),
                    BaseContentIds = String.Join(",", c.Select(n => n.ContentId).Where(n => n.HasValue))
                }
            );

            // создать поля
            foreach (var column in groupedColumns)
            {
                if (!String.IsNullOrEmpty(column.BaseContentIds))
                {
                    Field baseField = VirtualContentRepository.GetAcceptableBaseFieldForCloning(column.Name, column.BaseContentIds, dbContent.Id, forNew);
                    // создать и сохранить новое виртуальное поле
                    Field vField = baseField.GetVirtualClone(dbContent);
                    result.Add(new Tuple<Field, Field>(vField, baseField));
                }
                else
                {
                    // создать "свободное" поле
                    Field vField = new Field(dbContent).Init();
                    vField.Name = column.Name;
                    int stringSize = column.CharMaxLength.HasValue ? column.CharMaxLength.Value : Field.StringSizeDefaultValue;
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
                            vField.TypeId = FieldTypeCodes.DateTime;
                            break;
                        case ValidFieldColumnDbTypes.Bit:
                            vField.TypeId = FieldTypeCodes.Boolean;
                            break;
                        case ValidFieldColumnDbTypes.Ntext:
                            vField.TypeId = FieldTypeCodes.Textbox;
                            break;
                        case ValidFieldColumnDbTypes.Nvarchar:
                            vField.TypeId = (stringSize == - 1) ? FieldTypeCodes.Textbox : FieldTypeCodes.String;
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

        #endregion

        #endregion

        #region update

        /// <summary>
        /// Обновить виртуальные контенты при создании/обновлении поля реального контента
        /// </summary>
        /// <param name="field"></param>
        internal void UpdateVirtualFields(Field field)
        {
            var content = ContentRepository.GetById(field.ContentId);
            content.Fields.Where(f => f.Id == field.Id).Single().StoredName = field.StoredName;
            UpdateVirtualSubContents(content);
        }		

        /// <summary>
        /// Обновление дочерних виртуальных контентов
        /// </summary>
        /// <param name="content"></param>
        private void UpdateVirtualSubContents(Content content)
        {
            
            List<Content.TreeItem> rebuildedViewSubContents = TraverseForUpdateVirtualSubContents(content);

            // перестроить View дочерних контентов
            RebuildSubContentViews(rebuildedViewSubContents);

            // Обновить все дочерние UQ-контенты
            UpdateUserQueryAsSubContents(rebuildedViewSubContents);
        }

        public List<Content.TreeItem> TraverseForUpdateVirtualSubContents(Content content)
        {
            List<Content.TreeItem> rebuildedViewSubContents = new List<Content.TreeItem>();			

            Action<Content, int> traverse = null;
            traverse = (parentContent, level) =>
            {
                foreach (var subContent in parentContent.VirtualSubContents)
                {
                    try
                    {
                        bool needToRebuildView = false;
                        if (subContent.VirtualType == VirtualType.Join)
                        {
                            needToRebuildView = AddNewFieldsToJoinSubContent(parentContent, subContent);
                        }
                        else if (subContent.VirtualType == VirtualType.Union)
                        {
                            // Обновить Union-контент как подчиненный
                            needToRebuildView = UpdateUnionAsSubContent(subContent);
                        }
                        else if (subContent.VirtualType == VirtualType.UserQuery)
                        {
                            needToRebuildView = true;
                        }
                        if (needToRebuildView)
                            rebuildedViewSubContents.Add(new Content.TreeItem { ContentId = subContent.Id, Level = level });
                    }
                    catch (VirtualContentProcessingException)
                    {
                        throw;
                    }
                    catch (Exception exp)
                    {
                        throw new VirtualContentProcessingException(subContent, exp);
                    }

                    traverse(subContent, level + 1);
                }
            };			

            // --- обновить виртуальные поля созданные на основе полей родительского контента ВО ВСЕХ JOIN-КОНТЕНТАХ которые содержат такие поля ---
            // определить в каких join-контентах есть виртуальные поля построенные на основе полей родительского контента			
            IEnumerable<Content> joinRelatedContents = VirtualContentRepository.GetJoinRelatedContents(content);
            // обновить поля найденных виртуальных контентов
            foreach (var jrc in joinRelatedContents)
            {
                try
                {
                    bool needToRebuildView = UpdateJoinRelatedContentFields(content, jrc);
                    if (needToRebuildView)
                    {
                        traverse(jrc, 1);
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
            traverse(content, 0);

            return rebuildedViewSubContents;
        }

        #region Join Virtual Content Update
        /// <summary>
        /// Обновление Join-контента
        /// </summary>
        /// <param name="content"></param>
        /// <param name="dbContent"></param>
        internal Content UpdateJoinContent(Content content, Content dbContent)
        {
            if (content.VirtualJoinFieldNodes.Any())
            {
                // сначала удалить поля в подчиненных контентах
                //IEnumerable<ContentTreeItem> rebuildedSubContentViews = RemoveSubContentVirtualFields(content, Except(content.VirtualJoinFieldNodes, dbContent.VirtualJoinFieldNodes));
                //RebuildSubContentViews(rebuildedSubContentViews);
                RemoveSubContentVirtualFields(content, Except(content.VirtualJoinFieldNodes, dbContent.VirtualJoinFieldNodes));

                // удалить из БД удаляемые поля
                RemoveJoinContentFields(content.VirtualJoinFieldNodes, dbContent.VirtualJoinFieldNodes);

                // Сохранить новые поля
                SaveJoinContentFields(content.VirtualJoinFieldNodes, dbContent);


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
        /// <param name="parentContent"></param>
        private bool AddNewFieldsToJoinSubContent(Content parentContent, Content virtualContent)
        {
            // виртуальные поля верхнего уровня 
            IEnumerable<Field> rootFields = FieldRepository.GetList(virtualContent.VirtualJoinFieldNodes.Select(n => n.Id));
            // получить id базовых полей для полей верхнего уровня дочернего join-контента
            IEnumerable<int> rootBaseFieldIDs = rootFields.Select(f => f.PersistentId.Value).Distinct();

            // --- добавить в дочерний join-контент на первый уровень те поля, которых там нет, но которые добавлены в родительский контент ---
            // получить id новых базовых полей на основе которых нужно создать виртуальные поля верхнего уровня в дочернем join-контенте
            IEnumerable<int> newFieldIds = parentContent.Fields
                                            .Select(f => f.Id)
                                            .Except(rootBaseFieldIDs);
            // создать ноды для новых полей верхнего уровня
            IEnumerable<Content.VirtualFieldNode> newFieldNodes = newFieldIds.Select(id =>
                new Content.VirtualFieldNode
                {
                    Id = id,
                    TreeId = Content.VirtualFieldNode.GetFieldTreeId(id)
                }
            );

            if (newFieldIds.Any())
            {
                // Добавить новые поля в корень дочернего контента
                List<Content.VirtualFieldNode> newVirtualFieldNodes = new List<Content.VirtualFieldNode>(virtualContent.VirtualJoinFieldNodes);
                newVirtualFieldNodes.AddRange(newFieldNodes);
                virtualContent.VirtualJoinFieldNodes = newVirtualFieldNodes;
                // Сохранить новую коллекцию полей дочернего контента			
                Content updatingContent = ContentRepository.GetById(virtualContent.Id);
                SaveJoinContentFields(virtualContent.VirtualJoinFieldNodes, updatingContent);
                // -----------------------------------								
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Обновление полей в связанном Join контенте
        /// </summary>
        /// <param name="content"></param>
        /// <param name="relatedContent"></param>
        private bool UpdateJoinRelatedContentFields(Content content, Content relatedContent)
        {
            bool needToViewRebuild = false;

            // получить словарь полей родительского контента			
            Dictionary<int, Field> rootPersintentField = content.Fields.ToDictionary(f => f.Id);

            // получить словарь имен виртуальных O2M полей
            IEnumerable<int> joinFieldsIDs = relatedContent.Fields.Select(f => f.JoinId).Where(j => j.HasValue).Select(j => j.Value).Distinct();
            IEnumerable<Field> joinFields = FieldRepository.GetList(joinFieldsIDs);
            Dictionary<int, String> joinFieldNames = joinFields.ToDictionary(f => f.Id, f => f.Name);

            // определить, у каких виртуальных полей значимые параметры отличаются от значимых параметров соответствующих полей родительского контента
            // и обновить значимые параметры у таких виртуальных полей			
            // (тут не нужен проход по иерархии виртуальных полей , так как значимые параметры полей типа O2M нельзя менять если на таких полях построены виртуальные поля)
            foreach (var vfield in relatedContent.Fields)
            {
                if (rootPersintentField.ContainsKey(vfield.PersistentId.Value))
                {
                    var pfield = rootPersintentField[vfield.PersistentId.Value];
                    bool isChanged = false;

                    // ---- Проверка на изменение имени ---
                    // определить, имя виртуального поля пользовательское или нет ?
                    bool vNameHasUserName = false;
                    if (vfield.JoinId.HasValue)
                    {
                        string joinFieldName = joinFieldNames[vfield.JoinId.Value];
                        var generatedName = joinFieldName + "." + pfield.StoredName;
                        vNameHasUserName = !generatedName.Equals(vfield.Name, StringComparison.InvariantCultureIgnoreCase);

                        // если имя пользовательское, и едет апдейт дочерного join-контента, то этот дочерний контент точно нужно перестроить
                        needToViewRebuild = !needToViewRebuild ? vNameHasUserName : true;
                    }

                    // если имя поля автосгенерировано, то перегенерировать имя поле
                    if (!vNameHasUserName)
                    {
                        // получить часть иерархического имени виртуального поля, производного от имени базового поля
                        // например для имени "m1.m2.m3.Title" нужно получить Title 
                        string pfn = GetPersistentFieldName(vfield.Name);
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
                        VirtualFieldRepository.Update(vfield);
                }

            }
            return needToViewRebuild;
        }
        #endregion

        #region Union Virtual Content Update
        /// <summary>
        /// Обновляет Union-контент
        /// </summary>
        /// <param name="content"></param>
        /// <param name="dbContent"></param>
        /// <returns></returns>
        internal Content UpdateUnionContent(Content content, Content dbContent)
        {
            // Если изменился состав контентов - то сохранить привязку контентов в БД
            bool contentSourceListIsChanged = content.UnionSourceContentIDs.Count() != dbContent.UnionSourceContentIDs.Count() ||
                        content.UnionSourceContentIDs.Except(dbContent.UnionSourceContentIDs).Any();
            if (contentSourceListIsChanged)
                VirtualContentRepository.RecreateUnionSourcesInfo(dbContent, content.UnionSourceContentIDs);

            // Обновить состав полей Union-контента
            bool isUpdated = UpdateUnionContentFieldCollection(content, dbContent);
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
        /// <param name="content"></param>
        /// <param name="dbContent"></param>
        /// <returns></returns>
        private bool UpdateUnionContentFieldCollection(Content content, Content dbContent)
        {
            // Определить какие поля необходимо удалить и какие добавить
            UpdateFieldCollectionOperationParams makeoutResult = MakeOutUnionFields(content, dbContent);

            if (makeoutResult.NewFieldsToAdd.Any() || makeoutResult.FieldsToRemove.Any() || makeoutResult.FieldsToRemain.Any())
            {
                // удалить поля в подчиненных контентах
                RemoveSubContentVirtualFields(content, makeoutResult.FieldsToRemove.Select(f => f.Id));

                // Удалить привязки полей
                VirtualFieldRepository.RemoveUnionAttrs(dbContent);

                // удалить поля из БД 
                RemoveContentFields(makeoutResult.FieldsToRemove.Select(f => f.Id));

                // Сохранить новые поля							
                foreach (var f in makeoutResult.NewFieldsToAdd)
                {
                    if (forceNewFieldIds != null)
                        f.ForceId = forceNewFieldIds.Dequeue();
                    Field fResult = VirtualFieldRepository.Save(f);
                    newFieldIds.Add(fResult.Id);
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
        /// <param name="newStateFields"></param>
        /// <param name="unionFields"></param>
        private void UpdateContentFields(IEnumerable<Field> newStateFields, IEnumerable<Field> oldStateFields)
        {
            Dictionary<int, Field> bflds = newStateFields.ToDictionary(f => f.Id);

            foreach (var uf in oldStateFields)
            {
                bool needToViewRebuild = false;
                bool isChanged = false;

                UpdateImportantFieldAttrribute(bflds[uf.Id], uf, ref isChanged, ref needToViewRebuild, true);
                // если необходимо - то изменить
                if (isChanged)
                    VirtualFieldRepository.Update(uf);
            }
        }

        /// <summary>
        /// Обновить Union-контент как подчиненный
        /// </summary>		
        private bool UpdateUnionAsSubContent(Content unionContent)
        {
            UpdateFieldCollectionOperationParams makeoutResult = MakeOutUnionFields(unionContent, unionContent);
            // Определить какие поля необходимо удалить и какие добавить
            //UpdateFieldCollectionOperationParams makeoutResult = MakeOutUnionFields(unionContent, unionContent);			

            if (makeoutResult.NewFieldsToAdd.Any() || makeoutResult.FieldsToRemove.Any() || makeoutResult.FieldsToRemain.Any())
            {
                VirtualFieldRepository.RemoveUnionAttrs(unionContent);

                // удалить поля из БД 
                RemoveContentFields(makeoutResult.FieldsToRemove.Select(f => f.Id));

                // Сохранить новые поля в БД									
                foreach (var f in makeoutResult.NewFieldsToAdd)
                {
                    if (forceNewFieldIds != null)
                        f.ForceId = forceNewFieldIds.Dequeue();
                    Field fResult = VirtualFieldRepository.Save(f);
                    newFieldIds.Add(fResult.Id);
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
        /// <param name="content"></param>
        /// <param name="dbContent"></param>
        /// <returns></returns>
        private UpdateFieldCollectionOperationParams MakeOutUnionFields(Content content, Content dbContent)
        {
            // Базовые поля Union-контента уникальные по имени
            IEnumerable<Field> allBaseFieldsAfterUpdate = VirtualContentRepository.GetFieldsOfContents(content.UnionSourceContentIDs)
                .Distinct(Field.NameComparer)
                .OrderBy(f => f.Order)
                .ToArray();

            // Информация о соответствии базовых полей и полей Union-контента
            IEnumerable<VirtualFieldsRelation> v2bRelations = VirtualFieldRepository.GetVirtualSubFields(allBaseFieldsAfterUpdate.Select(f => f.Id))
                                  .Where(r => r.VirtualFieldContentId == content.Id)
                                  .ToArray();


            // --- сначала обрабатываются виртуальные поля Union-контента, которые имеют только одно базовое поле ---
            IEnumerable<VirtualFieldsRelation> o2oRelations = v2bRelations
                .GroupBy(r => r.VirtualFieldId)
                .Where(g => g.Count() < 2)
                .SelectMany(g => g)
                .ToArray();
            // id виртуальных полей, которые должны остаться
            List<int> fieldToRemainIDs = o2oRelations.Select(r => r.VirtualFieldId).Distinct().ToList();
            // id базовых полей, виртуальные поля для которых должны быть
            IEnumerable<int> baseFieldToRemainIDs = o2oRelations.Select(r => r.BaseFieldId).Distinct().ToArray();
            // id виртуальных полей которые должны быть удалены
            List<int> fieldToRemoveIDs = dbContent.Fields.Select(f => f.Id).Except(fieldToRemainIDs).Distinct().ToList();
            // Базовые поля для новых виртуальных полей
            List<int> baseFieldsToAddIds = allBaseFieldsAfterUpdate
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
            List<Field> newStateRemainingFields = new List<Field>(fieldToRemainIDs.Count());
            foreach (var rid in fieldToRemainIDs)
            {
                int bfid = v2bRelations.Single(r => r.VirtualFieldId == rid).BaseFieldId;
                Field faupd = allBaseFieldsAfterUpdate.Single(f => f.Id == bfid).GetVirtualClone(dbContent);
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

        #endregion

        #region User Query Virtual Content Update

        /// <summary>
        /// Обновить UQ-контент
        /// </summary>
        /// <param name="content"></param>
        /// <param name="dbContent"></param>
        /// <returns></returns>
        internal Content UpdateUserQueryContent(Content content, Content dbContent)
        {
            IEnumerable<int> contentsBeforeUpdate = content.UserQueryContentViewSchema.SelectUniqContentIDs().OrderBy(i => i);

            // контент уже обновлен так что можно перестроить view
            DropContentViews(dbContent);
            CreateContentViews(dbContent);

            IEnumerable<int> contentsAfterUpdate = dbContent.UserQueryContentViewSchema.SelectUniqContentIDs().OrderBy(i => i);

            // пересоздать привязку контентов в БД, если необходимо
            bool contentSourceListIsChanged = !contentsBeforeUpdate.SequenceEqual(contentsAfterUpdate);
            if (contentSourceListIsChanged)
                VirtualContentRepository.RecreateUserQuerySourcesInfo(dbContent);

            if (CheckCycleInGraph())
                throw new CycleInContentGraphException();

            // Обновить состав полей UQ-контента
            bool isUpdated = UpdateUserQueryContentFieldCollection(content, dbContent);
            if (isUpdated || contentSourceListIsChanged)
            {
                // Обновить все дочерние контенты
                UpdateVirtualSubContents(dbContent);
            }

            return dbContent;
        }

        /// <summary>
        /// Обновить состав полей UQ-контента
        /// </summary>
        /// <param name="content"></param>
        /// <param name="dbContent"></param>
        /// <returns></returns>
        private bool UpdateUserQueryContentFieldCollection(Content content, Content dbContent)
        {
            // определить какие поля удалять, какие оставить и какие добавить			
            UpdateFieldCollectionOperationParams makeoutResult = MakeOutUserQueryFields(dbContent);

            if (makeoutResult.NewFieldsToAdd.Any() || makeoutResult.FieldsToRemove.Any() || makeoutResult.FieldsToRemain.Any())
            {
                // удалить поля в подчиненных контентах
                RemoveSubContentVirtualFields(content, makeoutResult.FieldsToRemove.Select(f => f.Id));

                // Удалить привязки полей
                VirtualFieldRepository.RemoveUserQueryAttrs(dbContent);

                // удалить поля из БД				
                RemoveContentFields(makeoutResult.FieldsToRemove.Select(f => f.Id));

                // Сохранить новые поля							
                foreach (var f in makeoutResult.NewFieldsToAdd)
                {
                    if (forceNewFieldIds != null)
                        f.ForceId = forceNewFieldIds.Dequeue();
                    Field fResult = VirtualFieldRepository.Save(f);
                    newFieldIds.Add(fResult.Id);
                    f.Id = fResult.Id;
                }

                // Обновить измененные поля					
                UpdateContentFields(makeoutResult.NewStateRemainingFields, makeoutResult.FieldsToRemain);

                // Обновить привязку полей
                VirtualFieldRepository.RebuildUserQueryAttrs(dbContent);

                return true;
            }
            else
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
                var uniqueItems = rebuildedViewSubContents.Distinct(new LambdaEqualityComparer<Content.TreeItem>((x, y) => x.ContentId.Equals(y.ContentId), x => x.ContentId));

                IEnumerable<Content> updatedContents = ContentRepository.GetList(uniqueItems.Select(c => c.ContentId))
                    .Where(c => c.VirtualType == VirtualType.UserQuery);
                Dictionary<int, Content> updatedContentsDict = updatedContents.ToDictionary(c => c.Id);

                IEnumerable<Content> uqContents = uniqueItems
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
        /// <param name="content"></param>
        /// <param name="dbContent"></param>
        /// <returns></returns>
        public bool UpdateUserQueryAsSubContent(Content dbContent)
        {
            try
            {
            // определить какие поля удалять, какие оставить и какие добавить						
            
            UpdateFieldCollectionOperationParams makeoutResult = MakeOutUserQueryFields(dbContent);

            if (makeoutResult.NewFieldsToAdd.Any() || makeoutResult.FieldsToRemove.Any() || makeoutResult.FieldsToRemain.Any())
            {
                // Удалить привязки полей
                VirtualFieldRepository.RemoveUserQueryAttrs(dbContent);

                // удалить поля из БД				
                RemoveContentFields(makeoutResult.FieldsToRemove.Select(f => f.Id));

                // Сохранить новые поля							
                foreach (var f in makeoutResult.NewFieldsToAdd)
                {
                    if (forceNewFieldIds != null)
                        f.ForceId = forceNewFieldIds.Dequeue();
                    Field fResult = VirtualFieldRepository.Save(f);
                    newFieldIds.Add(fResult.Id);
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
            else
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
        /// <param name="content"></param>
        /// <param name="updatedContent"></param>
        /// <returns></returns>
        private UpdateFieldCollectionOperationParams MakeOutUserQueryFields(Content dbContent)
        {
            IEnumerable<Tuple<Field, Field>> newFieldsAfterUpdateInfo = CreateFieldsForUserQueryColumns(dbContent, dbContent.UserQueryContentViewSchema, false);

            // получить базовые поля
            IEnumerable<Field> allBaseFieldsAfterUpdate = newFieldsAfterUpdateInfo.Where(t => t.Item2 != null).Select(t => t.Item2).ToArray();
            // определить что делать с полями основанными на полях контента
            UpdateFieldCollectionOperationParams upateParamsForContentBasedFields = MakeOutUserQueryFields(dbContent, allBaseFieldsAfterUpdate);

            //получить требуемые поля основанные на полях контента
            //IEnumerable<Field> allBasedFieldsAfterUpdate = newFieldsAfterUpdateInfo.Where(t => t.Item2 != null).Select(t => t.Item1);

            // определить что делать со свободными полями
            // получить несвободные поля контента
            IEnumerable<int> notFreeFieldIDs = VirtualFieldRepository.GetVirtualBaseFieldIDs(dbContent.Fields.Select(f => f.Id).ToArray())
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
            List<Field> newStateRemainingFreeFields = new List<Field>();
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

        private UpdateFieldCollectionOperationParams MakeOutUserQueryFields(Content updatedContent, IEnumerable<Field> allBaseFieldsAfterUpdate)
        {
            IEnumerable<VirtualFieldsRelation> v2bRelations = VirtualFieldRepository.GetVirtualSubFields(allBaseFieldsAfterUpdate.Select(f => f.Id))
                                  .Where(r => r.VirtualFieldContentId == updatedContent.Id)
                                  .ToArray();

            // рассматриваем только поля основанные на полях контентов
            IEnumerable<int> dbContentFieldIDs = VirtualFieldRepository.GetVirtualBaseFieldIDs(updatedContent.Fields.Select(f => f.Id).ToArray())
                .Select(r => r.VirtualFieldId)
                .ToArray();
            IEnumerable<Field> dbContentFields = FieldRepository.GetList(dbContentFieldIDs);


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
            List<Field> newStateRemainingFields = new List<Field>(fieldToRemainIDs.Count());
            foreach (var rid in fieldToRemainIDs)
            {
                int bfid = v2bRelations.Single(r => r.VirtualFieldId == rid).BaseFieldId;
                Field faupd = allBaseFieldsAfterUpdate.Single(f => f.Id == bfid).GetVirtualClone(updatedContent);
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

        #endregion

        #endregion

        #region remove

        /// <summary>
        /// Удаляет поля в подчиненных контентах
        /// </summary>
        /// <param name="content"></param>
        public IEnumerable<Content.TreeItem> RemoveSubContentVirtualFields(Content content, IEnumerable<int> removingFieldIds)
        {
            List<Content.TreeItem> rebuildedViewSubContents = new List<Content.TreeItem>();

            IEnumerable<Field> removingFields = FieldRepository.GetList(removingFieldIds);

            Action<Content, int, IEnumerable<int>> traverse = null;
            traverse = (parentContent, level, baseRemovingFields) =>
            {
                foreach (var subContent in parentContent.VirtualSubContents)
                {
                    try
                    {
                        // реализовать получение списка наследников удаляемых полей (curlevRemovingFields) на текущем уровне иерархии контентов (subContent)
                        IEnumerable<int> curlevRemovingFieldIds = VirtualFieldRepository.GetVirtualSubFields(baseRemovingFields)
                            .Where(r => r.VirtualFieldContentId == subContent.Id)
                            .Select(r => r.VirtualFieldId)
                            .ToArray();

                        // вниз по иерархии
                        traverse(subContent, level + 1, curlevRemovingFieldIds);

                        bool needToRebuildView = false;
                        if (subContent.VirtualType == Constants.VirtualType.Union)
                            needToRebuildView = RemoveChildUnionVirtualFields(curlevRemovingFieldIds, subContent, baseRemovingFields);
                        else if (subContent.VirtualType == Constants.VirtualType.UserQuery)
                            needToRebuildView = RemoveChildUserQueryVirtualFields(curlevRemovingFieldIds, subContent, baseRemovingFields);

                        if (needToRebuildView)
                            rebuildedViewSubContents.Add(new Content.TreeItem { ContentId = subContent.Id, Level = level });
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
            };

            // определить в каких join-контентах есть виртуальные поля построенные на основе полей родительского контента			
            IEnumerable<Content> joinRelatedContents = VirtualContentRepository.GetJoinRelatedContents(content);
            if (joinRelatedContents.Any())
            {
                IEnumerable<VirtualFieldsRelation> vbfRel = VirtualFieldRepository.GetVirtualSubFields(removingFieldIds);
                // удалить поля в найденных виртуальных контентов
                foreach (var jrc in joinRelatedContents)
                {
                    try
                    {
                        // получить список наследников удаляемых полей на текущем уровне иерархии контентов
                        IEnumerable<int> removingSubFieldsIds = vbfRel
                            .Where(r => r.VirtualFieldContentId == jrc.Id)
                            .Select(r => r.VirtualFieldId);
                        // Получить иерархию удаляемых полей JOIN-контента
                        IEnumerable<Content.VirtualFieldNode> removingFieldNodes = GetRemovingFieldNodes(jrc, removingFieldIds);
                        // получить полный список удаляемых из JOIN полей
                        IEnumerable<int> allRemovingSubFieldsIds = Content.VirtualFieldNode.Linearize(removingFieldNodes).Select(n => n.Id).ToArray();

                        // удалить поля в дочерних Union и UQ контентах
                        traverse(jrc, 1, allRemovingSubFieldsIds);

                        bool needToRebuildView = RemoveJoinRelatedContentFields(jrc, removingFieldNodes);
                        if (needToRebuildView)
                            rebuildedViewSubContents.Add(new Content.TreeItem { ContentId = jrc.Id, Level = 0 });
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
            traverse(content, 0, removingFieldIds);

            return rebuildedViewSubContents;
        }

        /// <summary>
        /// Удаляет связанную с контентом информацию
        /// </summary>
        /// <param name="content"></param>
        internal void RemoveContentData(Content content)
        {
            Content dbContentVersion = ContentRepository.GetById(content.Id);
            if (content.StoredVirtualType == VirtualType.Join)
            {
                DropContentViews(content);
                // сначала удалить поля в подчиненных контентах
                var rebuildedSubContentViews = RemoveSubContentVirtualFields(content, Except(Enumerable.Empty<Content.VirtualFieldNode>(), dbContentVersion.VirtualJoinFieldNodes));
                RebuildSubContentViews(rebuildedSubContentViews);

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



        #region Join Virtual Remove Operation
        /// <summary>
        /// Удалить дочерние виртуальные поля из JOIN-контента 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="removingFieldIds"></param>
        /// <returns></returns>
        private bool RemoveJoinRelatedContentFields(Content content, IEnumerable<Content.VirtualFieldNode> removingFieldNodes)
        {
            bool result = false;

            // удаление
            Action<IEnumerable<Content.VirtualFieldNode>> removingTraverse = null;
            removingTraverse = (nodes) =>
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
        /// <param name="content"></param>
        /// <param name="removingPersistentFieldIds"></param>
        /// <returns></returns>
        private List<Content.VirtualFieldNode> GetRemovingFieldNodes(Content content, IEnumerable<int> removingPersistentFieldIds)
        {
            Dictionary<int, Field> contentFieldsDictionary = content.Fields.ToDictionary(f => f.Id);

            HashSet<int> removingPersistentFieldIdsHashset = new HashSet<int>(removingPersistentFieldIds);

            // ветки полей которые надо будет удалить
            List<Content.VirtualFieldNode> removingFieldNodes = new List<Content.VirtualFieldNode>();

            // идем по дереву полей контента, и если PersisntentID поля есть в коллекции id удаляемых полей, то такое поле и все его наследники должно быть удалено
            // помещаем его в коллекцию удаляемых полей (включая подчиненные)
            Action<IEnumerable<Content.VirtualFieldNode>> traverse = null;
            traverse = (nodes) =>
            {
                foreach (var node in nodes)
                {
                    if (contentFieldsDictionary.ContainsKey(node.Id))
                    {
                        int? contentFieldPersistentId = contentFieldsDictionary[node.Id].PersistentId;
                        if (contentFieldPersistentId.HasValue && removingPersistentFieldIdsHashset.Contains(contentFieldPersistentId.Value))
                        {
                            // добавляем в удаляемые
                            removingFieldNodes.Add(node);
                        }
                        else
                            traverse(node.Children);
                    }
                }
            };
            traverse(content.VirtualJoinFieldNodes);

            return removingFieldNodes;
        }

        // удалить из БД удаляемые поля
        private void RemoveJoinContentFields(IEnumerable<Content.VirtualFieldNode> newVirtualJoinFieldNodes, IEnumerable<Content.VirtualFieldNode> oldVirtualJoinFieldNodes)
        {
            HashSet<int> virtualFieldsIDs = new HashSet<int>(Content.VirtualFieldNode.Linearize(newVirtualJoinFieldNodes)
                .Select(n => n.Id)
                .Distinct()
            );

            // идем по дереву полей (которые в БД) контента, и если id поля нет в новой virtualJoinFieldNodes, то такое поле должно быть удалено
            Action<IEnumerable<Content.VirtualFieldNode>> traverse = null;
            traverse = (nodes) =>
            {
                foreach (var node in nodes)
                {
                    traverse(node.Children);
                    if (!virtualFieldsIDs.Contains(node.Id))
                    {
                        Field.Die(node.Id, false);
                    }
                }
            };
            traverse(oldVirtualJoinFieldNodes);
        }
        #endregion

        #region Union Virtual Remove Operation
        /// <summary>
        /// Удалить поля Union
        /// </summary>
        /// <param name="fieldToRemove"></param>
        private void RemoveContentFields(IEnumerable<int> fieldToRemoveIds)
        {
            foreach (var fid in fieldToRemoveIds)
            {
                Field.Die(fid, false);
            }
        }

        /// <summary>
        /// Удаляет дочерние виртуальные поля из UNION-контента
        /// </summary>
        /// <param name="removingFields"></param>
        /// <param name="parentContent"></param>
        /// <param name="unionContent"></param>
        /// <returns></returns>
        private bool RemoveChildUnionVirtualFields(IEnumerable<int> removingUnionFieldIds, Content unionContent, IEnumerable<int> removingBaseFieldIds)
        {
            // теперь отсеять те поля из удаляемых, для которых есть есть более одной связи в union_attr - это значит что в других контентах-источниках есть поля которые могут быть основой для удаляемого поля 
            IEnumerable<int> fieldToRemoveIds = VirtualFieldRepository.GetUnionFieldRelationCount(removingUnionFieldIds)
                .Where(c => c.Count < 2)
                .Select(c => c.UnionFieldId);

            if (fieldToRemoveIds.Any())
            {
                VirtualFieldRepository.RemoveUnionAttrs(unionContent);

                // удалить поля из БД 
                RemoveContentFields(fieldToRemoveIds);

                VirtualFieldRepository.RebuildUnionAttrs(unionContent);
            }
            else
            {
                // удалить только связи только для удаляемых базовых полей
                VirtualFieldRepository.RemoveUnionAttrs(removingBaseFieldIds);
            }

            return true;
        }

        /// <summary>
        /// Удаляет контент источник из связанных union-контентов
        /// </summary>
        /// <param name="sourceContent"></param>
        internal void RemoveSourceContentFromUnions(Content sourceContent)
        {
            IEnumerable<Content> dbUnionSubContents = sourceContent.VirtualSubContents.Where(c => c.VirtualType == Constants.VirtualType.Union);
            Dictionary<int, Content> dbUnionSubContentsDict = dbUnionSubContents.ToDictionary(c => c.Id);

            // получить вторую копию union-контентов
            IEnumerable<Content> unionSubContents = ContentRepository.GetById(sourceContent.Id).VirtualSubContents.Where(c => c.VirtualType == Constants.VirtualType.Union);
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


        #endregion

        #region User Query Virtual Remove Operation
        /// <summary>
        /// Удаляет дочерние виртуальные поля из UQ-контента
        /// </summary>
        /// <param name="removingUnionFieldIds"></param>
        /// <param name="unionContent"></param>
        /// <param name="removingBaseFieldIds"></param>
        /// <returns></returns>
        private bool RemoveChildUserQueryVirtualFields(IEnumerable<int> removingUserQueryFieldIds, Content uqContent, IEnumerable<int> removingBaseFieldIds)
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
            else
                return false;
        }
        #endregion

        #endregion

        #region View Operations
        /// <summary>
        /// Создать вьюхи для контента
        /// </summary>
        /// <param name="newContent"></param>
        internal void CreateContentViews(Content newContent)
        {
            if (newContent.VirtualType == VirtualType.Join)
                CreateJoinContentViews(newContent);
            else if (newContent.VirtualType == VirtualType.Union)
                CreateUnionContentViews(newContent);
            else if (newContent.VirtualType == VirtualType.UserQuery)
                CreateUserQueryViews(newContent);
        }

        #region Join content View
        /// <summary>
        /// Создать вьюхи для JOIN-контента
        /// </summary>
        /// <param name="newContent"></param>
        private void CreateJoinContentViews(Content newContent)
        {
            if (newContent.VirtualType == VirtualType.Join)
            {
                IEnumerable<VirtualFieldData> vfData = VirtualContentRepository.GetJoinFieldData(newContent.Id);
                string viewCreateDDL = GenerateCreateJoinViewDDL(newContent.Id, newContent.JoinRootId.Value, vfData);
                string asyncViewCreateDDL = GenerateCreateJoinAsyncViewDDL(newContent.Id, newContent.JoinRootId.Value, vfData);
                VirtualContentRepository.RunCreateViewDDL(viewCreateDDL);
                VirtualContentRepository.RunCreateViewDDL(asyncViewCreateDDL);
                VirtualContentRepository.CreateUnitedView(newContent.Id);
                VirtualContentRepository.CreateFrontedViews(newContent.Id);
            }
        }

        /// <summary>
        /// Генерирует DDL-запрос для создания View для JOIN-контента 
        /// </summary>
        /// <param name="virtualContentId"></param>
        /// <param name="joinRootContentId"></param>
        /// <param name="virtualFieldsData"></param>
        /// <returns></returns>
        internal string GenerateCreateJoinViewDDL(int virtualContentId, int joinRootContentId, IEnumerable<VirtualFieldData> virtualFieldsData)
        {
            string viewNameTemplate = "CREATE VIEW [dbo].[content_{0}] AS ";
            string joinTableNameTemplate = "dbo.CONTENT_{0} ";
            string rootContentNameTemplate = "dbo.CONTENT_{0}";
            return GenerateCreateJoinViewDDL(virtualContentId, joinRootContentId, virtualFieldsData, viewNameTemplate, joinTableNameTemplate, rootContentNameTemplate);
        }

        /// <summary>
        /// Генерирует DDL-запрос для создания Async View для JOIN-контента 
        /// </summary>
        /// <param name="virtualContentId"></param>
        /// <param name="joinRootContentId"></param>
        /// <param name="virtualFieldsData"></param>
        /// <returns></returns>
        internal string GenerateCreateJoinAsyncViewDDL(int virtualContentId, int joinRootContentId, IEnumerable<VirtualFieldData> virtualFieldsData)
        {
            string viewNameTemplate = "CREATE VIEW [dbo].[content_{0}_async] AS ";
            string joinTableNameTemplate = "dbo.CONTENT_{0}_united ";
            string rootContentNameTemplate = "dbo.CONTENT_{0}_async";
            return GenerateCreateJoinViewDDL(virtualContentId, joinRootContentId, virtualFieldsData, viewNameTemplate, joinTableNameTemplate, rootContentNameTemplate);
        }

        /// <summary>
        /// Генерирует DDL-запрос для создания View для JOIN-контента 
        /// </summary>
        /// <param name="virtualContentId"></param>
        /// <param name="joinRootContentId"></param>
        /// <param name="virtualFieldsData"></param>
        /// <param name="viewNameTemplate"></param>
        /// <param name="joinTableNameTemplate"></param>
        /// <param name="rootContentNameTemplate"></param>
        /// <returns></returns>
        private string GenerateCreateJoinViewDDL(int virtualContentId, int joinRootContentId, IEnumerable<VirtualFieldData> virtualFieldsData, string viewNameTemplate, string joinTableNameTemplate, string rootContentNameTemplate)
        {

            string joinRootTableAlias = "c_0";

            StringBuilder selectBlock = new StringBuilder();
            selectBlock.AppendFormat(viewNameTemplate, virtualContentId);

            selectBlock.AppendFormat("SELECT {0}.CONTENT_ITEM_ID,{0}.STATUS_TYPE_ID,{0}.VISIBLE,{0}.ARCHIVE,{0}.CREATED,{0}.MODIFIED,{0}.LAST_MODIFIED_BY,", joinRootTableAlias);

            StringBuilder fromBlock = new StringBuilder();
            fromBlock
                .Append("FROM ")
                .AppendFormat(rootContentNameTemplate, joinRootContentId)
                .AppendFormat(" AS {0} ", joinRootTableAlias);

            if (virtualFieldsData != null && virtualFieldsData.Any())
            {
                Action<string, VirtualFieldData, int> traverse = null;
                traverse = (parentTableAlias, parentFieldData, i) =>
                {
                    var cfTableAlias = String.Concat(parentTableAlias, '_', i);

                    fromBlock
                        .Append("LEFT OUTER JOIN ")
                        .AppendFormat(joinTableNameTemplate, parentFieldData.RelateToPersistentContentId)
                        .AppendFormat("AS {0} WITH (nolock) ON {0}.CONTENT_ITEM_ID = {1}.[{2}] ", cfTableAlias, parentTableAlias, parentFieldData.PersistentName);

                    //fromBlock.AppendFormat("LEFT OUTER JOIN dbo.CONTENT_{0} AS {1} WITH (nolock) ON {1}.CONTENT_ITEM_ID = {2}.{3} ", parentFieldData.RelateToPersistentContentId, cfTableAlias, parentTableAlias, parentFieldData.PersistentName);
                    int c = 0;
                    foreach (var cf in virtualFieldsData.Where(f => f.JoinId == parentFieldData.Id))
                    {
                        selectBlock.AppendFormat("{0}.[{1}] as [{2}],", cfTableAlias, cf.PersistentName, cf.Name);

                        if (cf.Type == FieldTypeCodes.Relation && cf.RelateToPersistentContentId.HasValue)
                        {
                            traverse(cfTableAlias, cf, c);
                            c++;
                        }
                    }
                };

                // получить поля верхнего уровня
                int rc = 0;
                foreach (var rf in virtualFieldsData.Where(d => !d.JoinId.HasValue))
                {
                    selectBlock.AppendFormat("{0}.[{1}] as [{2}],", joinRootTableAlias, rf.PersistentName, rf.Name);

                    if (rf.Type == FieldTypeCodes.Relation && rf.RelateToPersistentContentId.HasValue)
                    {
                        // далее вниз по иерархии
                        traverse(joinRootTableAlias, rf, rc);
                        rc++;
                    }
                }
            }

            return selectBlock.Replace(',', ' ', selectBlock.Length - 1, 1).Append(fromBlock).ToString();
        }
        #endregion

        #region Union Content Views
        private void CreateUnionContentViews(Content content)
        {
            // Построить словарь по которому можно определить есть ли в контенте поля с указанным именем
            var fieldNameInContentsTemp =
                VirtualContentRepository.GetFieldsOfContents(content.UnionSourceContentIDs)
                .GroupBy(f => f.Name.ToLowerInvariant())
                .Select(g => new { key = g.Key, data = new HashSet<int>(g.Select(f => f.ContentId).ToArray()) })
                .ToArray();

            Dictionary<string, HashSet<int>> fieldNameInSourceContents = fieldNameInContentsTemp.ToDictionary(d => d.key, d => d.data, StringComparer.InvariantCultureIgnoreCase);

            IEnumerable<string> contentFieldNames = content.Fields.Select(f => f.Name);

            string viewCreateDDL = GenerateCreateUnionViewDDL(content.Id, content.UnionSourceContentIDs, contentFieldNames, fieldNameInSourceContents);
            string asyncViewCreateDDL = GenerateCreateUnionAsyncViewDDL(content.Id, content.UnionSourceContentIDs, contentFieldNames, fieldNameInSourceContents);
            VirtualContentRepository.RunCreateViewDDL(viewCreateDDL);
            VirtualContentRepository.RunCreateViewDDL(asyncViewCreateDDL);
            VirtualContentRepository.CreateUnitedView(content.Id);
            VirtualContentRepository.CreateFrontedViews(content.Id);
        }

        internal string GenerateCreateUnionViewDDL(int contentId, IEnumerable<int> unionSourceContentIDs, IEnumerable<string> contentFieldNames, Dictionary<string, HashSet<int>> fieldNameInSourceContents)
        {
            string viewNameTemplate = "CREATE VIEW [dbo].[content_{0}] AS";
            string sourceTableNameTemplate = "dbo.CONTENT_{0}";
            return GenerateCreateUnionViewDDL(contentId, unionSourceContentIDs, contentFieldNames, fieldNameInSourceContents, viewNameTemplate, sourceTableNameTemplate);
        }

        internal string GenerateCreateUnionAsyncViewDDL(int contentId, IEnumerable<int> unionSourceContentIDs, IEnumerable<string> contentFieldNames, Dictionary<string, HashSet<int>> fieldNameInSourceContents)
        {
            string viewNameTemplate = "CREATE VIEW [dbo].[content_{0}_async] AS";
            string sourceTableNameTemplate = "dbo.CONTENT_{0}_async";
            return GenerateCreateUnionViewDDL(contentId, unionSourceContentIDs, contentFieldNames, fieldNameInSourceContents, viewNameTemplate, sourceTableNameTemplate);
        }

        private string GenerateCreateUnionViewDDL(int contentId, IEnumerable<int> unionSourceContentIDs, IEnumerable<string> contentFieldNames, Dictionary<string, HashSet<int>> fieldNameInSourceContents,
                                                         string viewNameTemplate, string sourceTableNameTemplate)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(viewNameTemplate, contentId);

            int i = unionSourceContentIDs.Count();
            foreach (var usId in unionSourceContentIDs)
            {
                sb.AppendFormat(" SELECT {0} content_id,content_item_id,created,modified,last_modified_by,status_type_id,visible,archive,", usId);
                int j = contentFieldNames.Count();
                foreach (var fname in contentFieldNames)
                {
                    if (fieldNameInSourceContents[fname].Contains(usId))
                        sb.AppendFormat("[{0}] [{0}]", fname);
                    else
                        sb.AppendFormat("NULL [{0}]", fname);
                    j--;
                    if (j > 0)
                        sb.Append(',');
                }
                sb.Replace(',', ' ', sb.Length - 1, 1);
                sb.Append(" FROM ");
                sb.AppendFormat(sourceTableNameTemplate, usId);

                i--;
                if (i > 0)
                    sb.Append(" UNION ALL");
            }

            return sb.ToString();
        }
        #endregion

        #region Union Content Views
        private void CreateUserQueryViews(Content content)
        {
            try
            {
                string createViewTemplate = "CREATE VIEW [dbo].{0} AS {1}";

                // View
                string viewName = String.Format("content_{0}", content.Id);
                string viewCreateDDL = String.Format(createViewTemplate, viewName, content.UserQuery);
                VirtualContentRepository.RunCreateViewDDL(viewCreateDDL);

                // united view
                string unitedViewName = String.Format("content_{0}_united", content.Id);
                string viewUnitedCreateDDL = null;
                if (String.IsNullOrEmpty(content.UserQueryAlternative))
                {
                    string unitedViewQuery = String.Format("select * from {0}", viewName);
                    viewUnitedCreateDDL = String.Format(createViewTemplate, unitedViewName, unitedViewQuery);
                }
                else
                {
                    viewUnitedCreateDDL = String.Format(createViewTemplate, unitedViewName, content.UserQueryAlternative);
                }
                VirtualContentRepository.RunCreateViewDDL(viewUnitedCreateDDL);

                // live и stage
                VirtualContentRepository.CreateFrontedViews(content.Id);
            }
            catch (SqlException ex)
            {
                string message = String.Format(ContentStrings.ErrorInSubContent, content.Name, ex.ErrorsToString());
                throw new UserQueryContentCreateViewException(message);
            }
        }
        #endregion

        /// <summary>
        /// Возвращает имена всех View которые создаються для виртуального контента
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<string> GetVirtualContentAllViewNames(int contentId)
        {
            string contentIdString = contentId.ToString();
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
                "content_" + contentIdString + "_new",
            };
        }

        /// <summary>
        /// Удалить view контента
        /// </summary>
        /// <param name="content"></param>
        internal void DropContentViews(Content content)
        {
            foreach (string viewName in GetVirtualContentAllViewNames(content.Id))
            {
                VirtualContentRepository.DropView(viewName);
            }
        }

        /// <summary>
        /// Refresh view контента
        /// </summary>
        /// <param name="content"></param>
        internal void RefreshContentViews(Content content)
        {
            foreach (string viewName in GetVirtualContentAllViewNames(content.Id).Where(n => !n.Contains("async")).Reverse())
            {
                VirtualContentRepository.RefreshView(viewName);
            }
        }


        internal void RebuildSubContentViews(IEnumerable<Content.TreeItem> rebuildedViewSubContents)
        {
            if (rebuildedViewSubContents.Any())
            {
                // Уникальные контенты по id
                var uniqueItems = rebuildedViewSubContents.Distinct(new LambdaEqualityComparer<Content.TreeItem>((x, y) => x.ContentId.Equals(y.ContentId), x => x.ContentId));

                IEnumerable<Content> updatedContents = ContentRepository.GetList(uniqueItems.Select(i => i.ContentId));
                Dictionary<int, Content> updatedContentsDict = updatedContents.ToDictionary(c => c.Id);

                // сортируем контенты по уровню иерархии и пересоздаем вью в соответствии с порядком в иерархии				
                foreach (var contentId in uniqueItems.OrderBy(c => c.Level).Select(c => c.ContentId))
                {
                    Content content = updatedContentsDict[contentId];
                    RebuildSubContentView(content);
                }
            }
        }

        public void RebuildSubContentView(Content content)
        {
                    try
                    {
                        if (content.VirtualType == VirtualType.UserQuery)
                            RefreshContentViews(content);
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
        #endregion

        #region helpers

        private void UpdateImportantFieldAttrribute(Field sourceField, Field destField, ref bool isChanged, ref bool needToViewRebuild, bool matchName = false)
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
                case Constants.FieldTypeCodes.Textbox:
                    {
                        if (destField.TextBoxRows != sourceField.TextBoxRows)
                        {
                            destField.TextBoxRows = sourceField.TextBoxRows;
                            isChanged = true;
                        }
                        break;
                    }
                case Constants.FieldTypeCodes.VisualEdit:
                    {
                        if (destField.VisualEditorHeight != sourceField.VisualEditorHeight)
                        {
                            destField.VisualEditorHeight = sourceField.VisualEditorHeight;
                            isChanged = true;
                        }
                        break;
                    }
                case Constants.FieldTypeCodes.Numeric:
                    {
                        if (destField.DecimalPlaces != sourceField.DecimalPlaces)
                        {
                            destField.DecimalPlaces = sourceField.DecimalPlaces;
                            isChanged = true;
                            needToViewRebuild = true;
                        }
                        break;
                    }
                case Constants.FieldTypeCodes.String:
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
        /// Возвращает часть иерархического имени виртуального поля, производного от имени базового поля
        /// например для имени "m1.m2.m3.Title" нужно получить Title 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal string GetPersistentFieldName(string name)
        {
            int lastPointIndex = name.LastIndexOf('.');
            if (lastPointIndex < 0)
                return name;
            else
                return name.Remove(0, lastPointIndex + 1);
        }

        /// <summary>
        /// Заменяет часть имени производную от базового поля
        /// например для имени "m1.m2.m3.Title"  нужно получить "m1.m2.m3.Header" если persistentFieldName = "Header"
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal string ReplacePersistentFieldName(string oldName, string persistentFieldName)
        {
            int lastPointIndex = oldName.LastIndexOf('.');
            if (lastPointIndex < 0)
                return persistentFieldName;
            else
                return oldName.Remove(lastPointIndex + 1, oldName.Length - lastPointIndex - 1) + persistentFieldName;
        }

        /// <summary>
        /// Определить каких полей нет в newVirtualJoinFieldNodes по сравнению с oldVirtualJoinFieldNodes
        /// и вернуть их ID
        /// </summary>
        /// <param name="newVirtualJoinFieldNodes"></param>
        /// <param name="oldVirtualJoinFieldNodes"></param>
        /// <returns></returns>
        private IEnumerable<int> Except(IEnumerable<Content.VirtualFieldNode> newVirtualJoinFieldNodes, IEnumerable<Content.VirtualFieldNode> oldVirtualJoinFieldNodes)
        {
            IEnumerable<int> newFieldIds = Content.VirtualFieldNode.Linearize(newVirtualJoinFieldNodes).Select(n => n.Id).Distinct();
            IEnumerable<int> oldFieldIds = Content.VirtualFieldNode.Linearize(oldVirtualJoinFieldNodes).Select(n => n.Id).Distinct();
            return oldFieldIds.Except(newFieldIds).ToArray();
        }

        private bool CheckCycleInGraph()
        {
            var graph = VirtualContentRepository.GetContentRelationGraph();
            return GraphHepler.CheckCycleInGraph(graph);
        }

        /// <summary>
        /// Возвращает виртуальные поля join-контена которые имеют пользовательское имя
        /// </summary>
        /// <param name="parentContent"></param>
        /// <param name="subContent"></param>
        /// <returns></returns>
        internal List<Field> GetJoinVirtualFieldsWithChangedName(Content parentContent, Content subContent)
        {
            List<Field> virtualFieldsWithChangedName = new List<Field>();

            // получить реальные поля для виртуальных полей контента
            IEnumerable<int> persistentFiedIDs = subContent.Fields.Select(f => f.PersistentId.Value).Distinct();
            Dictionary<int, Field> persistentFieds = FieldRepository.GetList(persistentFiedIDs).ToDictionary(f => f.Id);

            // Определить у каких полей дочернего Join-контента имя было изменено
            // сделать это можно сравнив текущее поля с тем, что должно быть сгенерено автоматически
            Action<Field, IEnumerable<Field>> joinFieldsTraverse = null;
            joinFieldsTraverse = (o2mField, fields) =>
            {
                if (o2mField == null) // первый уровень
                {
                    // на первом уровне ирархии имена порожденных полей должны совпадать с именами пораждающих полей родительского контента
                    // если это не так - то имя поля верхнего уровня было изменено
                    virtualFieldsWithChangedName.AddRange(
                        (
                            from scf in parentContent.Fields
                            join f in fields on scf.Id equals f.PersistentId
                            select new { PersistentField = scf, VirtualField = f }
                        )
                        .Where(p => !p.PersistentField.Name.Equals(p.VirtualField.Name, StringComparison.InvariantCultureIgnoreCase))
                        .Select(p => p.VirtualField)
                    );

                    // Вниз по иерархии полей 
                    foreach (var f in fields)
                    {
                        if (f.ExactType == FieldExactTypes.O2MRelation)
                        {
                            f.Name = persistentFieds[f.PersistentId.Value].Name;
                            joinFieldsTraverse(f, subContent.Fields.Where(cf => cf.JoinId == f.Id));
                        }
                    }
                }
                else
                {
                    foreach (var f in fields)
                    {
                        // сгенерить имя поля
                        var generatedName = String.Concat(o2mField.Name, '.', persistentFieds[f.PersistentId.Value].Name);
                        // Если сгенеренное имя и реальное имя поля не совпадают, то имя поля было изменено пользователем
                        // добавляем его в коллекцию на основании которой будем проводить валидацию
                        if (!generatedName.Equals(f.Name, StringComparison.InvariantCultureIgnoreCase))
                            virtualFieldsWithChangedName.Add(f);
                        // Вниз по иерархии полей 
                        if (f.ExactType == FieldExactTypes.O2MRelation)
                        {
                            f.Name = generatedName; // Это важно, так как на нижних уровнях имена полей должны генериться на основе СГЕНЕРЕННОГО имени родительского  O2M поля
                            joinFieldsTraverse(f, subContent.Fields.Where(cf => cf.JoinId == f.Id));
                        }
                    }
                }
            };
            joinFieldsTraverse(null, subContent.Fields);
            return virtualFieldsWithChangedName;
        }

        /// <summary>
        /// возвращает рутовые поля
        /// </summary>
        /// <param name="virtualContentId"></param>
        /// <param name="joinedContentId"></param>
        /// <param name="selectItemIDs"></param>
        /// <returns></returns>
        internal IEnumerable<EntityTreeItem> GetRootFieldList(int virtualContentId, int? joinedContentId, string selectItemIDs)
        {
            Content content = null;
            if (virtualContentId > 0)
            {
                content = ContentRepository.GetById(virtualContentId);
                // если id реального контента отличается от значения из БД для текущего виртуального контента, 
                // то получаем поля выбранного пользователем контента
                if (joinedContentId.HasValue && (!content.JoinRootId.HasValue || content.JoinRootId.Value != joinedContentId.Value))
                    content = ContentRepository.GetById(joinedContentId.Value);
            }
            else
                content = ContentRepository.GetById(joinedContentId.Value);

            Func<Field, string, string, IEnumerable<EntityTreeItem>> getChildren = null;
            getChildren = (f, eid, alias) =>
            {
                // если у поля есть выбранные подчиненные поля на любом уровне иерархии, то получаем дочернии поля
                if (f.ExactType == Constants.FieldExactTypes.O2MRelation && !String.IsNullOrWhiteSpace(selectItemIDs) && selectItemIDs.IndexOf(eid.TrimEnd(']') + ".", 0, StringComparison.InvariantCultureIgnoreCase) > 0)
                    return GetChildFieldList(eid, alias, getChildren);
                else
                    return Enumerable.Empty<EntityTreeItem>();
            };

            return content.Fields
                        .Where(f => f.JoinId == null)
                        .Select(f => new EntityTreeItem
                        {
                            Id = Content.VirtualFieldNode.GetFieldTreeId(f.Id),
                            Alias = f.Name,
                            HasChildren = f.RelationId.HasValue,
                            Children = getChildren(f, Content.VirtualFieldNode.GetFieldTreeId(f.Id), f.Name),
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
            IEnumerable<EntityTreeItem> result = Enumerable.Empty<EntityTreeItem>();
            int parentFieldId = Content.VirtualFieldNode.ParseFieldTreeId(entityId);
            Field field = FieldRepository.GetById(parentFieldId);

            alias = String.IsNullOrWhiteSpace(alias) ? field.Name : alias;
            Func<Field, string> getAlias = (f => !f.PersistentId.HasValue ? String.Concat(alias, '.', f.Name) : f.Name);

            if (field.ExactType == Constants.FieldExactTypes.O2MRelation)
            {
                IEnumerable<Field> fieldSelect = Enumerable.Empty<Field>();

                // если поле виртуальное (для типа контента JOIN) - то получить его дочерние виртуальные поля
                if (field.PersistentId.HasValue)
                    fieldSelect = ContentRepository.GetById(field.ContentId).Fields.Where(f => f.JoinId == field.Id);

                // получить id реальных полей для которых уже получены виртуальные поля
                var joinedFieldPersistentIDs = new HashSet<int>(fieldSelect.Select(f => f.PersistentId.Value).ToArray());
                // добавить только те реальные поля, для которых нет виртуальных
                fieldSelect = fieldSelect.Concat(ContentRepository.GetById(field.RelateToContentId.Value)
                                                    .Fields
                                                    .Where(f => f.ExactType != FieldExactTypes.M2MRelation && f.ExactType != FieldExactTypes.M2ORelation)
                                                    .Where(f => !joinedFieldPersistentIDs.Contains(f.Id))
                                                 );

                result = fieldSelect.Select(f => new EntityTreeItem
                {
                    Id = Content.VirtualFieldNode.GetFieldTreeId(f.Id, entityId),
                    Alias = getAlias(f),
                    HasChildren = f.RelationId.HasValue,
                    Enabled = true,
                    Checked = f.PersistentId.HasValue,
                    IconUrl = f.Type.Icon,
                    IconTitle = f.Type.Name,
                    Children = getChildren(f, Content.VirtualFieldNode.GetFieldTreeId(f.Id, entityId), getAlias(f))
                });
            }
            return result;
        }

        #endregion


    }
}
