using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ArticleWorkflowBindMapper : GenericMapper<ArticleWorkflowBind, ArticleWorkflowBindDAL>
	{

		public override void CreateBizMapper()
		{
			Mapper.CreateMap<ArticleWorkflowBindDAL, ArticleWorkflowBind>()
			  .ForMember(biz => biz.Article, opt => opt.Ignore())
			  ;
		}

		public override void CreateDalMapper()
		{
			Mapper.CreateMap<ArticleWorkflowBind, ArticleWorkflowBindDAL>()
			  .ForMember(data => data.Article, opt => opt.Ignore())
			  ;
		}
	}
}
