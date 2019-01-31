﻿using System;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ObjectMapper : GenericMapper<BllObject, ObjectDAL>
    {
        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BllObject, ObjectDAL>(MemberList.Destination)
                .ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
                .ForMember(data => data.StatusType, opt => opt.Ignore())
                .ForMember(data => data.PageTemplate, opt => opt.Ignore())
                .ForMember(data => data.Page, opt => opt.Ignore())
                .ForMember(data => data.ObjectValues, opt => opt.Ignore())
                .ForMember(data => data.ObjectType, opt => opt.Ignore())
                .ForMember(data => data.DefaultFormat, opt => opt.Ignore())
                .ForMember(data => data.ChildObjectFormats, opt => opt.Ignore())
                .ForMember(data => data.InheritedObjects, opt => opt.Ignore())
                .ForMember(data => data.ObjectInheritedFrom, opt => opt.Ignore())
                .ForMember(data => data.Container, opt => opt.Ignore())
                .ForMember(data => data.ContentForm, opt => opt.Ignore())
                .ForMember(data => data.Locked, opt => opt.MapFrom(src => src.LockedBy == 0 ? null : (DateTime?)src.Locked))
                .ForMember(data => data.LockedBy, opt => opt.MapFrom(src => src.LockedBy == 0 ? null : (int?)src.LockedBy))
                .ForMember(data => data.LockedByUser, opt => opt.Ignore())
                .AfterMap(SetDalProperties);
        }

        private static void SetDalProperties(BllObject bizObject, ObjectDAL dataObject)
        {
            if (!bizObject.OverrideTemplateObject || !bizObject.IsNew || !bizObject.PageOrTemplate)
            {
                dataObject.ParentObjectId = null;
            }

            if (!(!bizObject.PageOrTemplate && (bizObject.IsCssType || bizObject.IsJavaScriptType))) // не объект шаблона - js / css
            {
                dataObject.Global = false;
            }
        }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<ObjectDAL, BllObject>(MemberList.Source)
                .ForMember(data => data.ObjectInheritedFrom, opt => opt.Ignore())
                .ForMember(data => data.Container, opt => opt.Ignore())
                .ForMember(data => data.ContentForm, opt => opt.Ignore());
        }
    }
}
