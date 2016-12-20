using System.Collections.Generic;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class DefaultMapper
    {
        public static TBiz GetBizObject<TBiz, TDal>(TDal dataObject)
            where TBiz : class
            where TDal : class
        {
            return dataObject == null ? null : Mapper.Map<TDal, TBiz>(dataObject);
        }

        public static List<TBiz> GetBizList<TBiz, TDal>(List<TDal> dataList)
            where TBiz : class
            where TDal : class
        {
            return Mapper.Map<List<TDal>, List<TBiz>>(dataList);
        }

        public static TDal GetDalObject<TDal, TBiz>(TBiz bizObject)
            where TBiz : class
            where TDal : class
        {
            return Mapper.Map<TBiz, TDal>(bizObject);
        }

        public static List<TDal> GetDalList<TDal, TBiz>(List<TBiz> bizList)
        {
            return Mapper.Map<List<TBiz>, List<TDal>>(bizList);
        }
    }
}
