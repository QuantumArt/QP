using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Validators
{
    /// <summary>
    /// Выполняет проверку на наличие конфликтов с полями в подчиненных контентах
    /// </summary>
    internal class FieldsConflictValidator
    {
        public IEnumerable<RuleViolation> SubContentsCheck(Content content, Field checkedField) => SubContentsCheck(content, new[] { checkedField }.ToList());

        public IEnumerable<RuleViolation> SubContentsCheck(Content content, List<Field> checkedFields)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (checkedFields == null)
            {
                throw new ArgumentNullException(nameof(checkedFields));
            }

            var result = new List<RuleViolation>();

            void Traverse(Content parentContent)
            {
                foreach (var subContent in parentContent.VirtualSubContents)
                {
                    if (subContent.VirtualType == VirtualType.Join || subContent.VirtualType == VirtualType.UserQuery)
                    {
                        CheckForJoinAndUserQuery(parentContent, subContent, checkedFields, result);
                    }
                    else if (subContent.VirtualType == VirtualType.Union)
                    {
                        CheckForUnion(content, parentContent, subContent, checkedFields, result);
                    }
                    Traverse(subContent);
                }
            }

            Traverse(content);
            return result;
        }

        /// <summary>
        /// Проверка виртуальных полей Join-контента
        /// </summary>
        /// <param name="content"></param>
        /// <param name="virtualJoinFieldNodes"></param>
        public IEnumerable<RuleViolation> SubContentsCheck(Content content, IEnumerable<Content.VirtualFieldNode> virtualJoinFieldNodes)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (virtualJoinFieldNodes == null)
            {
                throw new ArgumentNullException(nameof(virtualJoinFieldNodes));
            }

            if (content.VirtualType != VirtualType.Join)
            {
                throw new ApplicationException("Content Vitrual Type is not expected. Expected is Join. Actual Virtual Type Code is:" + content.VirtualType);
            }

            IEnumerable<int> virtualFieldsIDs = Content.VirtualFieldNode.Linearize(virtualJoinFieldNodes).Select(n => n.Id).Distinct().ToArray();
            var virtualFields = FieldRepository.GetList(virtualFieldsIDs).ToList();
            var virtualFieldsDictionary = new Dictionary<int, Field>(virtualFields.Count);
            foreach (var bs in virtualFields)
            {
                virtualFieldsDictionary.Add(bs.Id, bs);
            }

            // Получить список новых виртуальных полей в Join-контент
            var newVirtualFields = new List<Field>(virtualFields.Count);

            void Traverse(Field parentVirtualField, IEnumerable<Content.VirtualFieldNode> nodes)
            {
                foreach (var node in nodes)
                {
                    var virtualField = virtualFieldsDictionary[node.Id];

                    // Добавляем в коллекцию только новые виртуальные поля
                    // так как проверку достаточно выполнять только для них
                    if (!virtualField.PersistentId.HasValue)
                    {
                        var newVirtualField = virtualField.GetVirtualCloneForJoin(content, parentVirtualField);
                        newVirtualFields.Add(virtualField);

                        // вниз по иерархии
                        Traverse(newVirtualField, node.Children);
                    }
                    else
                    {
                        // вниз по иерархии
                        Traverse(virtualField, node.Children);
                    }
                }
            }

            Traverse(null, content.VirtualJoinFieldNodes);
            return SubContentsCheck(content, newVirtualFields.ToList());
        }

        /// <summary>
        /// Проверка полей базовых контентов для UNION на соответствие
        /// </summary>
        public IEnumerable<RuleViolation> UnionFieldsMatchCheck(List<int> unionSourceContentIds)
        {
            var contents = ContentRepository.GetList(unionSourceContentIds.Distinct()).ToList();
            var contentNames = new Dictionary<int, string>(contents.Count);
            foreach (var c in contents)
            {
                contentNames.Add(c.Id, c.Name);
            }

            var unionBaseContentFields = VirtualContentRepository.GetFieldsOfContents(unionSourceContentIds);

            // Группируем по имени поля
            var _ = unionBaseContentFields

                //.GroupBy(f => f.Name, new LambdaComparer<string>((s1, s2) => s1.Equals(s2, StringComparison.InvariantCultureIgnoreCase)));
                .GroupBy(f => f.Name.ToLowerInvariant());

            var result = new List<RuleViolation>();

            // Проверяет поля на соответствие
            var __ = new LambdaEqualityComparer<Field>(
                (f1, f2) =>
                {
                    var violation = СheckUnionMatchedFields(contentNames[f1.ContentId], contentNames[f2.ContentId], f1, f2);
                    if (violation != null)
                    {
                        result.Add(violation);
                    }
                    return true;
                },
                f => f.Name.ToLowerInvariant().GetHashCode()
            );

            return result;
        }

        /// <summary>
        /// Валидация на полях Union контента
        /// </summary>
        private void CheckForUnion(EntityObject checkedContent, EntityObject parentContent, Content unionSubContent, IReadOnlyCollection<Field> checkedFields, List<RuleViolation> ruleViolation)
        {
            // Если у Union всего один источник, то можно не проверять, так как в этом случае при обновлении checkedContent можно обновить и Union
            if (unionSubContent.UnionSourceContentIDs.Count() > 1)
            {
                // проверить есть ли в ветках других контентов-источников поля с такими же именами
                // если поле единственное - то можно смело обновлять Union								
                var checkedFieldNames = checkedFields.Distinct(Field.NameComparer).Select(f => f.Name);
                var allUnionBaseFields = VirtualContentRepository.GetFieldsOfContents(unionSubContent.UnionSourceContentIDs).Where(f => f.ContentId != parentContent.Id);
                var nameMatchedBaseFieldsCount = allUnionBaseFields.Count(f => checkedFieldNames.Contains(f.Name, Field.NameStringComparer));

                if (nameMatchedBaseFieldsCount > 0)
                {
                    var checkedContentName = string.IsNullOrWhiteSpace(checkedContent.Name) ? ContentStrings.CurrentContent : checkedContent.Name;

                    // получить пары полей с одинаковыми именами
                    var violations = (from scf in unionSubContent.Fields
                            join f in checkedFields on scf.Name.ToLowerInvariant() equals f.Name.ToLowerInvariant()
                            select Tuple.Create(scf, f)
                        ).Distinct(new LambdaEqualityComparer<Tuple<Field, Field>>((t1, t2) => t1.Item1.Id == t2.Item1.Id, t => t.Item1.Name.ToLowerInvariant().GetHashCode()))
                        .Select(t => СheckUnionMatchedFields(checkedContentName, unionSubContent.Name, t.Item1, t.Item2))
                        .Where(v => v != null);

                    // Выполнить проверку на соответствие полей с одинаковым именем
                    ruleViolation.AddRange(violations);
                }
            }
        }

        /// <summary>
        /// Проверка соответствия полей с одинаковыми именами
        /// </summary>
        private static RuleViolation СheckUnionMatchedFields(string contentName, string subContentName, Field existsField, Field checkedField)
        {
            RuleViolation result = null;

            // типы полей должны совпадать
            if (existsField.ExactType == checkedField.ExactType)
            {
                // Если тип Image или File - то должно совпадать свойство UseSiteLibrary
                if ((existsField.ExactType == FieldExactTypes.Image || existsField.ExactType == FieldExactTypes.File) && existsField.UseSiteLibrary != checkedField.UseSiteLibrary)
                {
                    result = new RuleViolation { Message = string.Format(FieldStrings.MatchUseSiteLibraryConflict, checkedField.Name, existsField.Name, contentName, subContentName) };
                }

                // если тип поля Relation, то поля должны ссылаться на один и тот же контент
                else if ((existsField.Type.Id == FieldTypeCodes.Relation || existsField.ExactType == FieldExactTypes.M2ORelation) && existsField.RelateToContentId != checkedField.RelateToContentId)
                {
                    result = new RuleViolation { Message = string.Format(FieldStrings.RelateToContensMismatch, checkedField.Name, existsField.Name, contentName, subContentName) };
                }

                // Если тип поля - DynamicImage, то поля должны ссылаться на одни и тот же Image
                else if (existsField.ExactType == FieldExactTypes.DynamicImage && existsField.BaseImageId != checkedField.BaseImageId)
                {
                    result = new RuleViolation { Message = string.Format(FieldStrings.BaseImageIdMismatch, checkedField.Name, existsField.Name, contentName, subContentName) };
                }
            }
            else
            {
                result = new RuleViolation { Message = string.Format(FieldStrings.MatchTypeConflict, checkedField.Name, existsField.Name, contentName, subContentName) };
            }

            return result;
        }

        /// <summary>
        /// Валидация на полях Join или User Query контентов
        /// </summary>
        private void CheckForJoinAndUserQuery(Content parentContent, Content subContent, List<Field> checkedFields, List<RuleViolation> ruleViolation)
        {
            var fieldNameComparer = Field.NameComparer;
            RuleViolation NewRuleViolation(Field f) => new RuleViolation { Message = string.Format(FieldStrings.MatchNameConflict, f.Name, subContent.Name) };

            // "Простым" алгоритмом проверяем только новые поля. Сравниваем с полями (по имени) и Join и UserQuery контентов
            ruleViolation.AddRange(
                subContent.Fields
                    .Intersect(checkedFields.Where(f => f.IsNew), fieldNameComparer)
                    .Select(NewRuleViolation)
            );
            if (subContent.VirtualType == VirtualType.Join)
            {
                CheckForJoin(parentContent, subContent, checkedFields, ruleViolation, fieldNameComparer, NewRuleViolation);
            }
        }

        /// <summary>
        /// Проверяем только те поля, у которых пользователь изменил имя. Сравниваем только с теми полями Join-контена, у которых было изменено имя (отличается от сгенерированного)
        /// </summary>
        /// <param name="parentContent"></param>
        /// <param name="subContent"></param>
        /// <param name="checkedFields"></param>
        /// <param name="ruleViolation"></param>
        /// <param name="fieldNameComparer"></param>
        /// <param name="newRuleViolation"></param>
        private void CheckForJoin(Content parentContent, Content subContent, IEnumerable<Field> checkedFields, List<RuleViolation> ruleViolation, IEqualityComparer<Field> fieldNameComparer, Func<Field, RuleViolation> newRuleViolation)
        {
            var helper = new VirtualContentHelper();
            var virtualFieldsWithChangedName = helper.GetJoinVirtualFieldsWithChangedName(parentContent, subContent);

            // А теперь сравнить имена полей 
            ruleViolation.AddRange(
                virtualFieldsWithChangedName
                    .Intersect(checkedFields.Where(f => !f.IsNew && f.IsNameChanged), fieldNameComparer)
                    .Select(newRuleViolation)
            );
        }
    }
}
