using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;

namespace Quantumart.QP8.BLL.Mappers
{
    
    internal abstract class GenericMapper
    {
        public abstract void CreateBizMapper();
        public abstract void CreateDalMapper();
    }
    
    internal class GenericMapper<Biz, Dal> : GenericMapper
        where Biz : class
        where Dal : class
    {

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<Dal, Biz>();
        }

        public override void CreateDalMapper()
        {
            Mapper.CreateMap<Biz, Dal>();
        }

        public virtual Biz GetBizObject(Dal dataObject)
        {
            if (dataObject == null)
                return null;
            else
            {
                return DefaultMapper.GetBizObject<Biz, Dal>(dataObject);
            }
        }


        public virtual List<Biz> GetBizList(List<Dal> dataList)
        {
            return DefaultMapper.GetBizList<Biz, Dal>(dataList);
        }


        public virtual Dal GetDalObject(Biz bizObject)
        {
            return DefaultMapper.GetDalObject<Dal, Biz>(bizObject);
        }


        public virtual List<Dal> GetDalList(List<Biz> bizList)
        {
            return DefaultMapper.GetDalList<Dal, Biz>(bizList);
        }

    }
}
