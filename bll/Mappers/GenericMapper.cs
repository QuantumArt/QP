using System.Collections.Generic;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
    internal abstract class GenericMapper
    {
        public abstract void CreateBizMapper();

        public abstract void CreateDalMapper();
    }

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

        public IMappingExpression<TDal, TBiz> GetBizMapper()
        {
            return Mapper.CreateMap<TDal, TBiz>();
        }

        public IMappingExpression<TBiz, TDal> GetDalMapper()
        {
            return Mapper.CreateMap<TBiz, TDal>();
        }

        public virtual TBiz GetBizObject(TDal dataObject)
        {
            return DefaultMapper.GetBizObject<TBiz, TDal>(dataObject);
        }

        public virtual List<TBiz> GetBizList(List<TDal> dataList)
        {
            return DefaultMapper.GetBizList<TBiz, TDal>(dataList);
        }

        public virtual TDal GetDalObject(TBiz bizObject)
        {
            return DefaultMapper.GetDalObject<TDal, TBiz>(bizObject);
        }

        public virtual List<TDal> GetDalList(List<TBiz> bizList)
        {
            return DefaultMapper.GetDalList<TDal, TBiz>(bizList);
        }
    }
}
