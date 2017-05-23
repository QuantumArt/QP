using System.Collections.Generic;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class GenericMapper<TBiz, TDal> : GenericMapper
        where TBiz : class
        where TDal : class
    {
        public override void CreateBizMapper()
        {
            GetBizMapper();
        }

        public override void CreateDalMapper()
        {
            GetDalMapper();
        }

        public IMappingExpression<TDal, TBiz> GetBizMapper() => Mapper.CreateMap<TDal, TBiz>();

        public IMappingExpression<TBiz, TDal> GetDalMapper() => Mapper.CreateMap<TBiz, TDal>();

        public virtual TBiz GetBizObject(TDal dataObject) => DefaultMapper.GetBizObject<TBiz, TDal>(dataObject);

        public virtual List<TBiz> GetBizList(List<TDal> dataList) => DefaultMapper.GetBizList<TBiz, TDal>(dataList);

        public virtual TDal GetDalObject(TBiz bizObject) => DefaultMapper.GetDalObject<TDal, TBiz>(bizObject);

        public virtual List<TDal> GetDalList(List<TBiz> bizList) => DefaultMapper.GetDalList<TDal, TBiz>(bizList);
    }
}
