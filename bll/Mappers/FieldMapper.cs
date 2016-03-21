using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Helpers;
using System.Web.Script.Serialization;


namespace Quantumart.QP8.BLL.Mappers
{
    internal class FieldMapper : GenericMapper<Field, FieldDAL>
	{
		#region Biz Mapper		
		public override void CreateBizMapper()
        {
            Mapper.CreateMap<FieldDAL, Field>()
              .ForMember(biz => biz.Indexed, opt => opt.MapFrom(src => Utils.Converter.ToBoolean(src.IndexFlag)))
			  .ForMember(biz => biz.OnScreen, opt => opt.MapFrom(src => Utils.Converter.ToBoolean(src.AllowStageEdit)))
			  .ForMember(biz => biz.JoinId, opt => opt.MapFrom(src => Utils.Converter.ToNullableInt32(src.JoinId)))
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
			  .ForMember(biz => biz.ExternalCss, opt => opt.Ignore())
			  .ForMember(biz => biz.RootElementClass, opt => opt.Ignore())
			  .ForMember(biz => biz.ParentField, opt => opt.Ignore())
			  .ForMember(biz => biz.ChildFields, opt => opt.Ignore())
			  .ForMember(biz => biz.ClassifierId, opt => opt.MapFrom(src => Utils.Converter.ToNullableInt32(src.ClassifierId)))
			  .AfterMap(SetBizProperties); 
        }

		private static void SetBizProperties(FieldDAL dataObject, Field bizObject)
		{
			bizObject.Size = Utils.Converter.ToInt32(dataObject.Size);

			bizObject.TextBoxRows = Field.TextBoxRowsDefaultValue;
			bizObject.VisualEditorHeight = Field.VisualEditorHeightDefaultValue;
			bizObject.StringSize = Field.StringSizeDefaultValue;
			bizObject.DecimalPlaces = Field.DecimalPlacesDefaultValue;

			switch (bizObject.TypeId)
			{
				case Constants.FieldTypeCodes.Textbox:
					bizObject.TextBoxRows = Utils.Converter.ToInt32(dataObject.Size);
					break;
				case Constants.FieldTypeCodes.VisualEdit:
					bizObject.VisualEditorHeight = Utils.Converter.ToInt32(dataObject.Size);					
					break;
				case Constants.FieldTypeCodes.Numeric:
					bizObject.DecimalPlaces = Utils.Converter.ToInt32(dataObject.Size);
					if (bizObject.DecimalPlaces != 0 && bizObject.IsLong)
					{
						bizObject.IsLong = false;
						bizObject.IsDecimal = true;
					}
					if (dataObject.IsClassifier)
						bizObject.UseTypeSecurity = dataObject.UseRelationSecurity;
                    break;
				case Constants.FieldTypeCodes.String:
					bizObject.StringSize = Utils.Converter.ToInt32(dataObject.Size);
					break;
				default:
					break;
			}

			bizObject.UseInputMask = !String.IsNullOrEmpty(bizObject.InputMask);

			bizObject.ParseStringEnumJson(dataObject.EnumValues);

			bizObject.Init();
		}
		#endregion

		#region DAL Mapper		
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<Field, FieldDAL>()			  
			  .ForMember(data => data.IndexFlag, opt => opt.MapFrom(src => Utils.Converter.ToDecimal(src.Indexed)))
			  .ForMember(data => data.AllowStageEdit, opt => opt.MapFrom(src => Utils.Converter.ToDecimal(src.OnScreen)))
			  .ForMember(data => data.NetName, opt => opt.MapFrom(src => src.LinqPropertyName))
			  .ForMember(data => data.NetBackName, opt => opt.MapFrom(src => src.LinqBackPropertyName))
			  .ForMember(data => data.JoinId, opt => opt.MapFrom(src => Utils.Converter.ToNullableDecimal(src.JoinId)))
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
			  .ForMember(data => data.ExternalCss, opt => opt.Ignore())
			  .ForMember(data => data.RootElementClass, opt => opt.Ignore())
			  .ForMember(data => data.Aggregators, opt => opt.Ignore())
			  .ForMember(data => data.Classifier, opt => opt.Ignore())
			  .ForMember(data => data.ClassifierId, opt => opt.MapFrom(src => Utils.Converter.ToNullableDecimal(src.ClassifierId)))
			  .ForMember(data => data.ParentField, opt => opt.Ignore())
			  .ForMember(data => data.ChildFields, opt => opt.Ignore())
              .ForMember(data => data.OrderField, opt => opt.Ignore())
			  .AfterMap(SetDalProperties);
			
		}

		private static void SetDalProperties(Field bizObject, FieldDAL dataObject)
		{
			switch (bizObject.TypeId)
			{
				case Constants.FieldTypeCodes.Textbox:
					dataObject.Size = bizObject.TextBoxRows;
					break;
				case Constants.FieldTypeCodes.VisualEdit:
					dataObject.Size = bizObject.VisualEditorHeight;
					break;
				case Constants.FieldTypeCodes.Numeric:
					dataObject.Size = bizObject.DecimalPlaces;
					if (bizObject.DecimalPlaces != 0)
						dataObject.IsLong = bizObject.IsDecimal;
					if (dataObject.IsClassifier)
						dataObject.UseRelationSecurity = bizObject.UseTypeSecurity;
					break;

				case Constants.FieldTypeCodes.String:
					dataObject.Size = bizObject.StringSize;
					break;
				case Constants.FieldTypeCodes.File:
				case Constants.FieldTypeCodes.Image:
				case Constants.FieldTypeCodes.DynamicImage:
					dataObject.Size = Field.StringSizeDefaultValue;
					break;
				default:
					dataObject.Size = 0;
					break;
			}

			if (bizObject.TypeId == Constants.FieldTypeCodes.VisualEdit) 
			{
				if (bizObject.PEnterMode != bizObject.Content.Site.PEnterMode)
					dataObject.PEnterMode = bizObject.PEnterMode;
				if (bizObject.UseEnglishQuotes != bizObject.Content.Site.UseEnglishQuotes)
					dataObject.UseEnglishQuotes = bizObject.UseEnglishQuotes;
				if (bizObject.RootElementClass != bizObject.Content.Site.RootElementClass)
					dataObject.RootElementClass = bizObject.RootElementClass;
				if (bizObject.ExternalCss != bizObject.Content.Site.ExternalCss)
				{
					dataObject.ExternalCss = (!String.IsNullOrEmpty(bizObject.ExternalCss)) ? bizObject.ExternalCss : ExternalCssHelper.Delimiter;
				}
			}
			else
			{
				dataObject.PEnterMode = null;
				dataObject.UseEnglishQuotes = null;
				dataObject.RootElementClass = null;
				dataObject.ExternalCss = null;
			}

			if (bizObject.StringEnumItems.Any())
			{
				dataObject.EnumValues = new JavaScriptSerializer().Serialize(
					bizObject.StringEnumItems.Select(v => new
					{
						Value = v.Value,
						Alias = v.Alias,
						IsDefault = v.GetIsDefault()
					})
				);
			}
			else
				dataObject.EnumValues = null;
		}


		#endregion
	}
}
