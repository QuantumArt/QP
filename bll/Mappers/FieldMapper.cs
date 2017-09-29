using System.Linq;
using System.Web.Script.Serialization;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class FieldMapper : GenericMapper<Field, FieldDAL>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<FieldDAL, Field>()
                .ForMember(biz => biz.Indexed, opt => opt.MapFrom(src => Converter.ToBoolean(src.IndexFlag)))
                .ForMember(biz => biz.OnScreen, opt => opt.MapFrom(src => Converter.ToBoolean(src.AllowStageEdit)))
                .ForMember(biz => biz.JoinId, opt => opt.MapFrom(src => Converter.ToNullableInt32(src.JoinId)))
                .ForMember(biz => biz.ReadOnly, opt => opt.MapFrom(src => src.ReadonlyFlag))
                .ForMember(biz => biz.LinqPropertyName, opt => opt.MapFrom(src => src.NetName))
                .ForMember(biz => biz.LinqBackPropertyName, opt => opt.MapFrom(src => src.NetBackName))
                .ForMember(biz => biz.StoredName, opt => opt.MapFrom(src => src.Name))
                .ForMember(biz => biz.Constraint, opt => opt.Ignore())
                .ForMember(biz => biz.ContentLink, opt => opt.Ignore())
                .ForMember(biz => biz.DynamicImage, opt => opt.Ignore())
                .ForMember(biz => biz.RelateToContentId, opt => opt.Ignore())
                .ForMember(biz => biz.Size, opt => opt.Ignore())
                .ForMember(biz => biz.PEnterMode, opt => opt.Ignore())
                .ForMember(biz => biz.UseEnglishQuotes, opt => opt.Ignore())
                .ForMember(biz => biz.DisableListAutoWrap, opt => opt.Ignore())
                .ForMember(biz => biz.ExternalCss, opt => opt.Ignore())
                .ForMember(biz => biz.RootElementClass, opt => opt.Ignore())
                .ForMember(biz => biz.ParentField, opt => opt.Ignore())
                .ForMember(biz => biz.ChildFields, opt => opt.Ignore())
                .ForMember(biz => biz.ClassifierId, opt => opt.MapFrom(src => Converter.ToNullableInt32(src.ClassifierId)))
                .AfterMap(SetBizProperties);
        }

        private static void SetBizProperties(FieldDAL dataObject, Field bizObject)
        {
            bizObject.Size = Converter.ToInt32(dataObject.Size);
            bizObject.TextBoxRows = Field.TextBoxRowsDefaultValue;
            bizObject.VisualEditorHeight = Field.VisualEditorHeightDefaultValue;
            bizObject.StringSize = Field.StringSizeDefaultValue;
            bizObject.DecimalPlaces = Field.DecimalPlacesDefaultValue;

            switch (bizObject.TypeId)
            {
                case FieldTypeCodes.Textbox:
                    bizObject.TextBoxRows = Converter.ToInt32(dataObject.Size);
                    bizObject.HighlightType = dataObject.TaHighlightType ?? string.Empty;
                    break;
                case FieldTypeCodes.VisualEdit:
                    bizObject.VisualEditorHeight = Converter.ToInt32(dataObject.Size);
                    break;
                case FieldTypeCodes.Numeric:
                    bizObject.DecimalPlaces = Converter.ToInt32(dataObject.Size);
                    if (bizObject.DecimalPlaces != 0 && bizObject.IsLong)
                    {
                        bizObject.IsLong = false;
                        bizObject.IsDecimal = true;
                    }

                    if (dataObject.IsClassifier)
                    {
                        bizObject.UseTypeSecurity = dataObject.UseRelationSecurity;
                    }

                    break;
                case FieldTypeCodes.String:
                    bizObject.StringSize = Converter.ToInt32(dataObject.Size);
                    break;
            }

            bizObject.UseInputMask = !string.IsNullOrEmpty(bizObject.InputMask);
            bizObject.ParseStringEnumJson(dataObject.EnumValues);
            bizObject.Init();
        }

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<Field, FieldDAL>()
              .ForMember(data => data.IndexFlag, opt => opt.MapFrom(src => Converter.ToDecimal(src.Indexed)))
              .ForMember(data => data.AllowStageEdit, opt => opt.MapFrom(src => Converter.ToDecimal(src.OnScreen)))
              .ForMember(data => data.NetName, opt => opt.MapFrom(src => src.LinqPropertyName))
              .ForMember(data => data.NetBackName, opt => opt.MapFrom(src => src.LinqBackPropertyName))
              .ForMember(data => data.JoinId, opt => opt.MapFrom(src => src.JoinId))
              .ForMember(data => data.ReadonlyFlag, opt => opt.MapFrom(src => src.ReadOnly))
              .ForMember(data => data.BaseImage, opt => opt.Ignore())
              .ForMember(data => data.ConstraintRule, opt => opt.Ignore())
              .ForMember(data => data.Content, opt => opt.Ignore())
              .ForMember(data => data.ContentData, opt => opt.Ignore())
              .ForMember(data => data.ContentToContent, opt => opt.Ignore())
              .ForMember(data => data.DependentNotifications, opt => opt.Ignore())
              .ForMember(data => data.DynamicImageSettings, opt => opt.Ignore())
              .ForMember(data => data.ItemToItemVersion, opt => opt.Ignore())
              .ForMember(data => data.JoinedFields, opt => opt.Ignore())
              .ForMember(data => data.JoinKeyField, opt => opt.Ignore())
              .ForMember(data => data.JoinSourceField, opt => opt.Ignore())
              .ForMember(data => data.JoinVirtualFields, opt => opt.Ignore())
              .ForMember(data => data.LastModifiedBy, opt => opt.Ignore())
              .ForMember(data => data.RelatedFields, opt => opt.Ignore())
              .ForMember(data => data.RelationField, opt => opt.Ignore())
              .ForMember(data => data.Thumbnails, opt => opt.Ignore())
              .ForMember(data => data.Type, opt => opt.Ignore())
              .ForMember(data => data.VersionContentData, opt => opt.Ignore())
              .ForMember(data => data.Size, opt => opt.Ignore())
              .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
              .ForMember(data => data.PEnterMode, opt => opt.Ignore())
              .ForMember(data => data.UseEnglishQuotes, opt => opt.Ignore())
              .ForMember(data => data.DisableListAutoWrap, opt => opt.Ignore())
              .ForMember(data => data.ExternalCss, opt => opt.Ignore())
              .ForMember(data => data.RootElementClass, opt => opt.Ignore())
              .ForMember(data => data.Aggregators, opt => opt.Ignore())
              .ForMember(data => data.Classifier, opt => opt.Ignore())
              .ForMember(data => data.ClassifierId, opt => opt.MapFrom(src => src.ClassifierId))
              .ForMember(data => data.ParentField, opt => opt.Ignore())
              .ForMember(data => data.ChildFields, opt => opt.Ignore())
              .ForMember(data => data.OrderField, opt => opt.Ignore())
              .AfterMap(SetDalProperties);
        }

        private static void SetDalProperties(Field bizObject, FieldDAL dataObject)
        {
            switch (bizObject.TypeId)
            {
                case FieldTypeCodes.Textbox:
                    dataObject.Size = bizObject.TextBoxRows;
                    dataObject.TaHighlightType = bizObject.HighlightType;
                    break;
                case FieldTypeCodes.VisualEdit:
                    dataObject.Size = bizObject.VisualEditorHeight;
                    break;
                case FieldTypeCodes.Numeric:
                    dataObject.Size = bizObject.DecimalPlaces;
                    dataObject.MaxDataListItemCount = bizObject.MaxDataListItemCount;
                    if (bizObject.DecimalPlaces != 0)
                    {
                        dataObject.IsLong = bizObject.IsDecimal;
                    }

                    if (dataObject.IsClassifier)
                    {
                        dataObject.UseRelationSecurity = bizObject.UseTypeSecurity;
                    }

                    break;
                case FieldTypeCodes.String:
                    dataObject.Size = bizObject.StringSize;
                    break;
                case FieldTypeCodes.File:
                case FieldTypeCodes.Image:
                case FieldTypeCodes.DynamicImage:
                    dataObject.Size = Field.StringSizeDefaultValue;
                    break;
                default:
                    dataObject.Size = 0;
                    break;
            }

            if (bizObject.TypeId == FieldTypeCodes.VisualEdit)
            {
                if (bizObject.PEnterMode != bizObject.Content.Site.PEnterMode)
                {
                    dataObject.PEnterMode = bizObject.PEnterMode;
                }

                if (bizObject.UseEnglishQuotes != bizObject.Content.Site.UseEnglishQuotes)
                {
                    dataObject.UseEnglishQuotes = bizObject.UseEnglishQuotes;
                }

                if (bizObject.DisableListAutoWrap != bizObject.Content.Site.DisableListAutoWrap)
                {
                    dataObject.DisableListAutoWrap = bizObject.DisableListAutoWrap;
                }

                if (bizObject.RootElementClass != bizObject.Content.Site.RootElementClass)
                {
                    dataObject.RootElementClass = bizObject.RootElementClass;
                }

                if (bizObject.ExternalCss != bizObject.Content.Site.ExternalCss)
                {
                    dataObject.ExternalCss = !string.IsNullOrEmpty(bizObject.ExternalCss) ? bizObject.ExternalCss : ExternalCssHelper.Delimiter;
                }
            }
            else
            {
                dataObject.PEnterMode = null;
                dataObject.UseEnglishQuotes = null;
                dataObject.DisableListAutoWrap = false;
                dataObject.RootElementClass = null;
                dataObject.ExternalCss = null;
            }

            if (bizObject.StringEnumItems.Any())
            {
                dataObject.EnumValues = new JavaScriptSerializer().Serialize(bizObject.StringEnumItems.Select(v => new
                {
                    v.Value,
                    v.Alias,
                    IsDefault = v.GetIsDefault()
                }));
            }
            else
            {
                dataObject.EnumValues = null;
            }
        }
    }
}
