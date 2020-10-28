using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository
{
    internal class MaskTemplateRepository
    {
        /// <summary>
        /// Получить все шаблоны масок
        /// </summary>
        public static IEnumerable<MaskTemplate> GetAllMaskTemplates()
        {
            return MapperFacade.MaskTemplateMapper.GetBizList(
                QPContext.EFContext.MaskTemplateSet.ToList()
            );
        }
    }
}
