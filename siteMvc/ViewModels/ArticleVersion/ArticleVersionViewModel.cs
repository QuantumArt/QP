using System;
using System.Dynamic;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.ArticleVersion
{

    public enum ArticleVersionViewType
    {
        Preview,
        CompareWithCurrent,
        CompareVersions
    }

    public class ClientEntity
    {
        public string Id { get; set;  }
        public string Name { get; set; }
    }

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

        #region creation

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
            
            ArticleVersionViewModel model = EntityViewModel.Create<ArticleVersionViewModel>(version, tabId, parentId);
            model.SuccesfulActionCode = succesfulActionCode;
            model.BoundToExternal = boundToExternal;
            return model;
        }
        #endregion

        public bool? BoundToExternal { get; set; }

        public bool IsChangingActionsProhibited => !Data.Article.IsArticleChangingActionsAllowed(BoundToExternal);

        #region read-only members

        public override string Id => (IsComparison) ? "0" : base.Id;

        public override string EntityTypeCode => C.EntityTypeCode.ArticleVersion;

        public override string ActionCode
        {
            get
            {
                if (ViewType == ArticleVersionViewType.CompareWithCurrent)
                    return C.ActionCode.CompareArticleVersionWithCurrent;
                else if (ViewType == ArticleVersionViewType.CompareVersions)
                    return C.ActionCode.CompareArticleVersions;
                else
                    return C.ActionCode.PreviewArticleVersion;
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
                    string firstId = Id;
                    string secondId = Data.VersionToMerge.Id.ToString();
                    result.entities = new[] { new ClientEntity { Id = firstId, Name = firstId }, new ClientEntity { Id = secondId, Name = secondId } };
                }
                return result;
            }
        }
        #endregion

        #region methods
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

        #endregion
    }
}