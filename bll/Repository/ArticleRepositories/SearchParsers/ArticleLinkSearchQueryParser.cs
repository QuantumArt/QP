using System;
using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure;
using Quantumart.QP8.DAL.DTO;

namespace Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers
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

            var searchQueryParamsList = searchQueryParams.ToList();
            if (!searchQueryParamsList.Any())
            {
                return null;
            }

            // оставляем параметры только тех типов которые обрабатываються данным методом
            var processedSqParams = searchQueryParamsList.Where(p => p.SearchType == ArticleFieldSearchType.M2MRelation || p.SearchType == ArticleFieldSearchType.M2ORelation).ToList();

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

        private ArticleLinkSearchParameter ParseParam(ArticleSearchQueryParam param)
        {
            Ensure.NotNull(param);
            Ensure.That(param.SearchType == ArticleFieldSearchType.M2MRelation || param.SearchType == ArticleFieldSearchType.M2ORelation);

            if (param == null)
            {
                throw new ArgumentException("p");
            }

            if (param.SearchType != ArticleFieldSearchType.M2MRelation && param.SearchType != ArticleFieldSearchType.M2ORelation)
            {
                throw new ArgumentException("Undefined search paramenter type: " + param.SearchType);
            }

            Ensure.NotNullOrWhiteSpace(param.FieldID, "FieldId");

            var fieldId = int.Parse(param.FieldID);
            Ensure.That<InvalidCastException>(param.QueryParams[0] is object[]);
            Ensure.That<InvalidCastException>(param.QueryParams[1] is bool);
            Ensure.That<InvalidCastException>(param.QueryParams[2] is bool);
            Ensure.That<InvalidCastException>(param.QueryParams[3] is bool);

            var isNull = (bool)param.QueryParams[1];
            var inverse = (bool)param.QueryParams[2];
            var unionAll = (bool)param.QueryParams[3];
            var values = Enumerable.Empty<int>();
            if (!isNull)
            {
                if (param.QueryParams[0] == null || ((object[])param.QueryParams[0]).Length == 0)
                {
                    return null;
                }

                values = ((object[])param.QueryParams[0]).Cast<int>().ToArray();
            }

            var field = _articleSearchRepository.GetFieldByID(fieldId);
            var linkedId = field.LinkId ?? field.BackRelationId;
            if (!linkedId.HasValue)
            {
                throw new ApplicationException("Не удалость получить LinkedId для поля с id = " + fieldId);
            }

            if (!int.TryParse(param.ContentID, out var extensionContentId))
            {
                extensionContentId = 0;
            }

            if (!int.TryParse(param.ReferenceFieldID, out var referenceFieldId))
            {
                referenceFieldId = 0;
            }

            var result = new ArticleLinkSearchParameter
            {
                LinkId = linkedId.Value,
                ExtensionContentId = extensionContentId,
                ReferenceFieldId = referenceFieldId,
                Ids = values.ToArray(),
                IsManyToMany = param.SearchType == ArticleFieldSearchType.M2MRelation,
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
