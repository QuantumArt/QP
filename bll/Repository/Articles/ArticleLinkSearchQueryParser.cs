using System;
using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure;
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
            Ensure.NotNull(articleSearchRepository);
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
            Ensure.NotNull(p);
            Ensure.That(p.SearchType == ArticleFieldSearchType.M2MRelation || p.SearchType == ArticleFieldSearchType.M2ORelation);

            if (p == null)
            {
                throw new ArgumentException("p");
            }

            if (p.SearchType != ArticleFieldSearchType.M2MRelation && p.SearchType != ArticleFieldSearchType.M2ORelation)
            {
                throw new ArgumentException("Undefined search paramenter type: " + p.SearchType);
            }

            Ensure.NotNullOrWhiteSpace(p.FieldID, "FieldId");

            var fieldId = int.Parse(p.FieldID);
            Ensure.That<InvalidCastException>(p.QueryParams[0] is object[]);
            Ensure.That<InvalidCastException>(p.QueryParams[1] is bool);
            Ensure.That<InvalidCastException>(p.QueryParams[2] is bool);
            Ensure.That<InvalidCastException>(p.QueryParams[3] is bool);

            var isNull = (bool)p.QueryParams[1];
            var inverse = (bool)p.QueryParams[2];
            var unionAll = (bool)p.QueryParams[3];
            var values = Enumerable.Empty<int>();
            if (!isNull)
            {
                if (p.QueryParams[0] == null || ((object[])p.QueryParams[0]).Length == 0)
                {
                    return null;
                }

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
                Inverse = inverse,
                UnionAll = unionAll
            };

            if (!result.IsManyToMany)
            {
                Ensure.NotNull(field.BackRelation, $"Не удалость получить BackRelation для поля с id = {fieldId}");
                result.FieldName = field.BackRelation.Name;
                result.ContentId = field.BackRelation.ContentId;
            }

            return result;
        }
    }
}
