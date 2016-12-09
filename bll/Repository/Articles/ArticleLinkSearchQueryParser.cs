using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.DAL.DTO;

namespace Quantumart.QP8.BLL.Repository.Articles
{
    /// <summary>
    /// Парсер параметров поиска многие ко многим
    /// </summary>
    public class ArticleLinkSearchQueryParser
    {
        private readonly IArticleSearchRepository _articleSearchRepository;

        public ArticleLinkSearchQueryParser(IArticleSearchRepository articleSearchRepository)
        {
            _articleSearchRepository = articleSearchRepository;
        }

        public IEnumerable<ArticleLinkSearchParameter> Parse(IEnumerable<ArticleSearchQueryParam> searchQueryParams)
        {
            if (searchQueryParams == null)
            {
                return null;
            }

            if (!searchQueryParams.Any())
            {
                return null;
            }

            // оставляем параметры только тех типов которые обрабатываються данным методом
            var processedSqParams = searchQueryParams.Where(p => p.SearchType == ArticleFieldSearchType.M2MRelation || p.SearchType == ArticleFieldSearchType.M2ORelation).ToList();

            // если нет обрабатываемых параметров - то возвращаем null
            if (!processedSqParams.Any())
            {
                return null;
            }

            return processedSqParams
                .Select(ParseParam)
                .Where(p => p != null)
                .ToArray();
        }

        private ArticleLinkSearchParameter ParseParam(ArticleSearchQueryParam p)
        {
            //Contract.Requires(p != null);
            //Contract.Requires(p.SearchType == ArticleFieldSearchType.M2MRelation || p.SearchType == ArticleFieldSearchType.M2ORelation);

            if (p == null)
            {
                throw new ArgumentException("p");
            }
            if (p.SearchType != ArticleFieldSearchType.M2MRelation && p.SearchType != ArticleFieldSearchType.M2ORelation)
            {
                throw new ArgumentException("Undefined search paramenter type: " + p.SearchType);
            }

            if (string.IsNullOrWhiteSpace(p.FieldID))
            {
                throw new ArgumentException("FieldID");
            }

            // Парсим FieldID
            // тут может быть FormatException - это нормально
            var fieldId = int.Parse(p.FieldID);

            // параметры не пустые и их не меньше 2х (используем 1й и 2й - остальные отбрасываем)
            if (p.QueryParams == null || p.QueryParams.Length < 3)
            {
                throw new ArgumentException();
            }

            // первый параметр должен быть null или object[]
            if (p.QueryParams[0] != null && !(p.QueryParams[0] is object[]))
            {
                throw new InvalidCastException();
            }

            // второй параметр должен быть bool
            if (!(p.QueryParams[1] is bool))
            {
                throw new InvalidCastException();
            }

            if (!(p.QueryParams[2] is bool))
            {
                throw new InvalidCastException();
            }

            var isNull = (bool)p.QueryParams[1];
            var inverse = (bool)p.QueryParams[2];
            var values = Enumerable.Empty<int>();
            if (!isNull)
            {
                // Если массив null или пустой - то возвращаем null
                if (p.QueryParams[0] == null || ((object[])p.QueryParams[0]).Length == 0)
                {
                    return null;
                }

                // Преобразуем массив к массиву int[]
                // тут возможен InvalidCastException, но это нормально, так как в этом случае действительно переданы некорректные данные
                values = ((object[])p.QueryParams[0]).Cast<int>().ToArray();
            }

            var field = _articleSearchRepository.GetFieldByID(fieldId);
            var linkedId = field.LinkId ?? field.BackRelationId;
            if (!linkedId.HasValue)
            {
                throw new ApplicationException("Не удалость получить LinkedId для поля с id = " + fieldId);
            }

            int extensionContentId;
            int referenceFieldId;
            if (!int.TryParse(p.ContentID, out extensionContentId))
            {
                extensionContentId = 0;
            }

            if (!int.TryParse(p.ReferenceFieldID, out referenceFieldId))
            {
                referenceFieldId = 0;
            }

            var result = new ArticleLinkSearchParameter
            {
                LinkId = linkedId.Value,
                ExtensionContentId = extensionContentId,
                ReferenceFieldId = referenceFieldId,
                Ids = values.ToArray(),
                IsManyToMany = p.SearchType == ArticleFieldSearchType.M2MRelation,
                IsNull = isNull,
                Inverse = inverse
            };

            if (!result.IsManyToMany)
            {
                if (field.BackRelation == null)
                {
                    throw new ApplicationException("Не удалость получить BackRelation для поля с id = " + fieldId);
                }

                result.FieldName = field.BackRelation.Name;
                result.ContentId = field.BackRelation.ContentId;
            }

            return result;
        }
    }
}
