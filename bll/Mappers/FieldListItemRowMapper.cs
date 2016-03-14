using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.ListItems;
using System.Data;
using Quantumart.QP8.Utils;
using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class FieldListItemRowMapper : GenericMapper<FieldListItem, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, FieldListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("Id"))))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(row => row.Field<string>("Name")))
				.ForMember(biz => biz.FieldName, opt => opt.MapFrom(row => row.Field<string>("FieldName")))
				.ForMember(biz => biz.ContentName, opt => opt.MapFrom(row => row.Field<string>("ContentName")))
				.ForMember(biz => biz.TypeCode, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("TypeCode"))))
                .ForMember(biz => biz.TypeIcon, opt => opt.MapFrom(row => row.Field<string>("TypeIcon")))
                .ForMember(biz => biz.FriendlyName, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("FriendlyName"), String.Empty)))
                .ForMember(biz => biz.Description, opt => opt.MapFrom(row => Converter.ToString(row.Field<string>("Description"), String.Empty)))
                .ForMember(biz => biz.Created, opt => opt.MapFrom(row => row.Field<DateTime>("Created")))
                .ForMember(biz => biz.Modified, opt => opt.MapFrom(row => row.Field<DateTime>("Modified")))
                .ForMember(biz => biz.LastModifiedByUser, opt => opt.MapFrom(row => row.Field<string>("LastModifiedByUser")))
                .ForMember(biz => biz.Size, opt => opt.MapFrom(row => row.Field<decimal>("Size").ToString()))

                .ForMember(biz => biz.Required, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("Required"))))
                .ForMember(biz => biz.Indexed, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("Indexed"))))
				.ForMember(biz => biz.ViewInList, opt => opt.MapFrom(row => row.Field<bool>("ViewInList")))
				.ForMember(biz => biz.MapAsProperty, opt => opt.MapFrom(row => row.Field<bool>("MapAsProperty")))
                .ForMember(biz => biz.LinkId, opt => opt.MapFrom(row => Converter.ToNullableInt32(row.Field<decimal?>("LinkId"))))
                .AfterMap(SetBizProperties);
        }


        private static void SetBizProperties(DataRow dataObject, FieldListItem bizObject)
        {
            bizObject.TypeName = bizObject.FieldTypeName;
        }


        public string TranslateSortExpression(string sortExpression)
        {
            Dictionary<string, string> replaces = new Dictionary<string, string>() { 
				{"TypeName", "TypeCode"},
			};
            return TranslateHelper.TranslateSortExpression(sortExpression, replaces);
        }
    }
}
