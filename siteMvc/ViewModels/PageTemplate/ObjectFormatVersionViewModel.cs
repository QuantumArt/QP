using System.Dynamic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using Quantumart.QP8.WebMvc.ViewModels.ArticleVersion;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class ObjectFormatVersionViewModel : EntityViewModel
    {
        protected bool PageOrTemplate;
        protected int ParentId;

        public override string EntityTypeCode => PageOrTemplate ? Constants.EntityTypeCode.PageObjectFormatVersion : Constants.EntityTypeCode.TemplateObjectFormatVersion;

        public override string ActionCode => PageOrTemplate ? Constants.ActionCode.PageObjectFormatVersionProperties : Constants.ActionCode.TemplateObjectFormatVersionProperties;

        internal static ObjectFormatVersionViewModel Create(ObjectFormatVersion version, string tabId, int parentId, bool pageOrTemplate)
        {
            var model = Create<ObjectFormatVersionViewModel>(version, tabId, parentId);
            model.ParentId = parentId;
            model.PageOrTemplate = pageOrTemplate;
            return model;
        }

        public ObjectFormatVersion Data
        {
            get => (ObjectFormatVersion)EntityData;
            set => EntityData = value;
        }
    }

    public class ObjectFormatVersionCompareViewModel : EntityViewModel
    {
        private bool _pageOrTemplate;

        public bool IsComparison => Data.VersionToMerge != null;

        public override string EntityTypeCode => _pageOrTemplate ? Constants.EntityTypeCode.PageObjectFormatVersion : Constants.EntityTypeCode.TemplateObjectFormatVersion;

        public override string ActionCode => _pageOrTemplate ? Constants.ActionCode.ComparePageObjectFormatVersions : Constants.ActionCode.CompareTemplateObjectFormatVersions;

        public ObjectFormatVersion Data
        {
            get => (ObjectFormatVersion)EntityData;
            set => EntityData = value;
        }

        public override string Id => IsComparison ? "0" : base.Id;

        public override ExpandoObject MainComponentParameters
        {
            get
            {
                dynamic result = base.MainComponentParameters;
                if (IsComparison)
                {
                    var firstId = Id;
                    var secondId = Data.VersionToMerge.Id.ToString();
                    result.entities = new[]
                    {
                        new ClientEntity { Id = firstId, Name = firstId },
                        new ClientEntity { Id = secondId, Name = secondId }
                    };
                }

                return result;
            }
        }

        public static ObjectFormatVersionCompareViewModel Create(ObjectFormatVersion version, string tabId, int parentId, bool pageOrTemplate)
        {
            var model = Create<ObjectFormatVersionCompareViewModel>(version, tabId, parentId);
            model.SuccesfulActionCode = string.Empty;
            model._pageOrTemplate = pageOrTemplate;
            return model;
        }
    }
}
