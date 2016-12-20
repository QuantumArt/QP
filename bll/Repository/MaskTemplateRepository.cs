using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL.Repository
{
	internal class MaskTemplateRepository
	{
		private static Lazy<IEnumerable<MaskTemplate>> allMaskTemplates = new Lazy<IEnumerable<MaskTemplate>>(
				() => MapperFacade.MaskTemplateMapper.GetBizList(QPContext.EFContext.MaskTemplateSet.ToList()),
				true
			);
		/// <summary>
		/// Получить все шаблоны масок 
		/// </summary>
		public static IEnumerable<MaskTemplate> GetAllMaskTemplates()
		{
			return allMaskTemplates.Value;
		}
	}
}
