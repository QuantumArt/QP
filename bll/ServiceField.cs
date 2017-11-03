using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Тип служебного поля
    /// </summary>
    public enum ServiceFieldType
    {
        None = 0,
        ID = -1,
        Created = -2,
        Modified = -3,
        LastModifiedBy = -4,
        StatusType = -5
    }

    /// <summary>
    /// Служебное поле статьи
    /// </summary>
    public class ServiceField
    {
        private ServiceField()
        {
        }

        public ServiceFieldType Type { get; private set; }
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string ColumnName { get; private set; }
        public ArticleFieldSearchType ArticleFieldSearchType { get; private set; }

        /// <summary>
        /// Создать служебное поле заданного типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ServiceField Create(ServiceFieldType type)
        {
            switch (type)
            {
                case ServiceFieldType.ID:
                    return new ServiceField
                    {
                        Type = type,
                        ID = (int)type,
                        Name = EntityObjectStrings.ID,
                        ColumnName = FieldName.ContentItemId,
                        ArticleFieldSearchType = ArticleFieldSearchType.Identifier
                    };
                case ServiceFieldType.Created:
                    return new ServiceField
                    {
                        Type = type,
                        ID = (int)type,
                        Name = EntityObjectStrings.Created,
                        ColumnName = FieldName.Created,
                        ArticleFieldSearchType = ArticleFieldSearchType.DateRange
                    };
                case ServiceFieldType.Modified:
                    return new ServiceField
                    {
                        Type = type,
                        ID = (int)type,
                        Name = EntityObjectStrings.Modified,
                        ColumnName = FieldName.Modified,
                        ArticleFieldSearchType = ArticleFieldSearchType.DateRange
                    };
                case ServiceFieldType.LastModifiedBy:
                    return new ServiceField
                    {
                        Type = type,
                        ID = (int)type,
                        Name = EntityObjectStrings.LastModifiedBy,
                        ColumnName = FieldName.LastModifiedBy,
                        ArticleFieldSearchType = ArticleFieldSearchType.O2MRelation
                    };
                case ServiceFieldType.StatusType:
                    return new ServiceField
                    {
                        Type = type,
                        ID = (int)type,
                        Name = ArticleStrings.Status,
                        ColumnName = FieldName.StatusTypeId,
                        ArticleFieldSearchType = ArticleFieldSearchType.O2MRelation
                    };
                default:
                    return new ServiceField();
            }
        }

        /// <summary>
        /// Получить все служебные поля статьи
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ServiceField> CreateAll()
        {
            return Enum.GetValues(typeof(ServiceFieldType))
                .OfType<ServiceFieldType>()
                .Where(t => t != ServiceFieldType.None)
                .Reverse()
                .Select(t => Create(t))
                .AsEnumerable();
        }
    }
}
