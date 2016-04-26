using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantumart.QPublishing.Info;

namespace Quantumart.QPublishing.Resizer
{
    public interface IDynamicImage
    {
        void CreateDynamicImage(DynamicImageInfo image);
    }
}
