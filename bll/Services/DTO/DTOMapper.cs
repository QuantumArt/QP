using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.BLL.Services.DTO
{
	public class DTOMapper
	{
		public static void CreateAllMappings()
		{
			Mapper.CreateMap<Folder, EntityTreeItem>()
				.ForMember(data => data.Alias, opt => opt.MapFrom(src => src.OutputName))
			;
		}
	}
}
