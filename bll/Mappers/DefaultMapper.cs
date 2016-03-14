using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{

    internal class DefaultMapper
    {
        public static Biz GetBizObject<Biz, Dal>(Dal dataObject)
            where Biz : class
            where Dal : class
        {
            if (dataObject == null)
                return null;
            else
            {
                return Mapper.Map<Dal, Biz>(dataObject);
            }
        }

        public static List<Biz> GetBizList<Biz, Dal>(List<Dal> dataList)
            where Biz : class
            where Dal : class
        {
            return Mapper.Map<List<Dal>, List<Biz>>(dataList);
        }

        public static Dal GetDalObject<Dal, Biz>(Biz bizObject)
            where Biz : class
            where Dal : class
        {
            return Mapper.Map<Biz, Dal>(bizObject);
        }

        public static List<Dal> GetDalList<Dal, Biz>(List<Biz> bizList)
        {
            return Mapper.Map<List<Biz>, List<Dal>>(bizList);
        }
    }
}
