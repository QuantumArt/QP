using System;
using System.Dynamic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.ViewModels.ArticleVersion
{
    public class ArticleVersionViewModel : EntityViewModel
    {
        public ArticleVersionViewType ViewType { get; set; }

        public new BLL.ArticleVersion Data
        {
            get
            {
                return (BLL.ArticleVersion)EntityData;
            }

            set
            {
                EntityData = value;
            }
        }

        public ArticleVersionViewModel()
        {
            ViewType = ArticleVersionViewType.Preview;
        }

        public static ArticleVersionViewModel Create(BLL.ArticleVersion version, string tabId, int parentId, bool? boundToExternal)
        {
            return Create(version, tabId, parentId, String.Empty, boundToExternal);
        }

        public static ArticleVersionViewModel Create(BLL.ArticleVersion version, string tabId, int parentId, string succesfulActionCode, bool? boundToExternal)
        {

            var model = Create<ArticleVersionViewModel>(version, tabId, parentId);
            model.SuccesfulActionCode = succesfulActionCode;
            model.BoundToExternal = boundToExternal;
            return model;
        }

        public bool? BoundToExternal { get; set; }

        public bool IsChangingActionsProhibited => !Data.Article.IsArticleChangingActionsAllowed(BoundToExternal);

        public override string Id => IsComparison ? "0" : base.Id;

        public override string EntityTypeCode => Constants.EntityTypeCode.ArticleVersion;

        public override string ActionCode
        {
            get
            {
                switch (ViewType)
                {
                    case ArticleVersionViewType.CompareWithCurrent:
                        return Constants.ActionCode.CompareArticleVersionWithCurrent;
                    case ArticleVersionViewType.CompareVersions:
                        return Constants.ActionCode.CompareArticleVersions;
                }

                return Constants.ActionCode.PreviewArticleVersion;
            }
        }

        public bool IsComparison => Data.VersionToMerge != null;

        public override ExpandoObject MainComponentParameters
        {
            get
            {
                dynamic result = base.MainComponentParameters;
                if (IsComparison)
                {
                    var firstId = Id;
                    var secondId = Data.VersionToMerge.Id.ToString();
                    result.entities = new[] { new ClientEntity { Id = firstId, Name = firstId }, new ClientEntity { Id = secondId, Name = secondId } };
                }

                return result;
            }
        }

        public override void Validate(ModelStateDictionary modelState)
        {
            try
            {
                Data.Article.Validate();
            }
            catch (RulesException ex)
            {
                ex.CopyTo(modelState, "Data");
                IsValid = false;
            }
        }

        public void CopyFieldValuesToArticle()
        {
            Data.Article.FieldValues = Data.FieldValues;
        }
    }
}
