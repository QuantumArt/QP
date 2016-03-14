using System;

namespace Quantumart.QP8.BLL.Repository.ArticleMatching.Models
{
	public interface IArticle
	{
		int Id { get; }
		DateTime Created { get; }
		IField this[string field] { get; }
		IField this[string field, int contentId] { get; }
	}
}
