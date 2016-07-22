using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quantumart.QP8.EntityFramework.Services
{
    public class DatabaseModelMappingService
    {
        public DatabaseModelMappingService()
        {

        }

        public XDocument GetModelMappingResult()
        {
            var doc = XDocument.Load(@"C:\Source\Repos\QP\EntityFramework6.Test\DataContext\ModelMappingResult.xml");
            return doc;
        }
    }
}
