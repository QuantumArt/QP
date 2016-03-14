using Quantumart.QP8.DAL.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

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
            Contract.Requires(articleSearchRepository != null);
            _articleSearchRepository = articleSearchRepository;
        }

        public IEnumerable<ArticleLinkSearchParameter> Parse(IEnumerable<ArticleSearchQueryParam> searchQueryParams)
        {
            if (searchQueryParams == null)
                return null;

            if (!searchQueryParams.Any())
                return null;

            // оставляем параметры только тех типов которые обрабатываються данным методом
            var processedSQParams = searchQueryParams.Where(p => p.SearchType == ArticleFieldSearchType.M2MRelation || p.SearchType == ArticleFieldSearchType.M2ORelation);
            // если нет обрабатываемых параметров - то возвращаем null
            if (!processedSQParams.Any())
                return null;

            return processedSQParams
				.Select(ParseParam)
				.Where(p => p != null)
				.ToArray();
          }

		private ArticleLinkSearchParameter ParseParam(ArticleSearchQueryParam p)
        {
			//Contract.Requires(p != null);
			//Contract.Requires(p.SearchType == ArticleFieldSearchType.M2MRelation || p.SearchType == ArticleFieldSearchType.M2ORelation);

			if (p == null)
				throw new ArgumentException("p");
			if (p.SearchType != ArticleFieldSearchType.M2MRelation && p.SearchType != ArticleFieldSearchType.M2ORelation)
				throw new ArgumentException("Undefined search paramenter type: " + p.SearchType);

            if (String.IsNullOrWhiteSpace(p.FieldID))
                throw new ArgumentException("FieldID");
            // Парсим FieldID
            // тут может быть FormatException - это нормально
            var fieldID = Int32.Parse(p.FieldID);

            // параметры не пустые и их не меньше 2х (используем 1й и 2й - остальные отбрасываем)
            if (p.QueryParams == null || p.QueryParams.Length < 3)
                throw new ArgumentException();

			// первый параметр должен быть null или object[]
            if (p.QueryParams[0] != null && !(p.QueryParams[0] is object[]))
                throw new InvalidCastException();

			// второй параметр должен быть bool
			if (p.QueryParams[1] == null || !(p.QueryParams[1] is bool))
				throw new InvalidCastException();

			if (p.QueryParams[2] == null || !(p.QueryParams[2] is bool))
				throw new InvalidCastException();

			bool isNull = (bool)p.QueryParams[1];
			bool inverse = (bool)p.QueryParams[2];

			IEnumerable<int> values = Enumerable.Empty<int>();

			if (!isNull)
			{
				// Если массив null или пустой - то возвращаем null
				if (p.QueryParams[0] == null || ((object[])p.QueryParams[0]).Length == 0)
					return null;
				// Преобразуем массив к массиву int[]
				// тут возможен InvalidCastException, но это нормально, так как в этом случае действительно переданы некорректные данные
				values = ((object[])p.QueryParams[0]).Cast<int>().ToArray();
			}

			Field field = _articleSearchRepository.GetFieldByID(fieldID);
			int? linkedID = (field.LinkId.HasValue) ? field.LinkId.Value : (field.BackRelationId.HasValue) ? field.BackRelationId.Value : (int?)null;

			if (!linkedID.HasValue)
				throw new ApplicationException("Не удалость получить LinkedId для поля с id = " + fieldID);

			int exstensionContentId;
			int referenceFieldId;

			if (!int.TryParse(p.ContentID, out exstensionContentId))
			{
				exstensionContentId = 0;
			}

			if (!int.TryParse(p.ReferenceFieldID, out referenceFieldId))
			{
				referenceFieldId = 0;
			}

            var result = new ArticleLinkSearchParameter
			{
				LinkId = linkedID.Value,
				ExstensionContentId = exstensionContentId,
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
			        throw new ApplicationException("Не удалость получить BackRelation для поля с id = " + fieldID);
			    }

			    result.FieldName = field.BackRelation.Name;
				result.ContentId = field.BackRelation.ContentId;
			}

			return result;
        }
    }
}
