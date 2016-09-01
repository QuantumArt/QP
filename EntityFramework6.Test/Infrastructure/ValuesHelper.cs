using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework6.Test.Infrastructure
{
    public class ValuesHelper
    {
        public static int GetNonPublishedStatus(Mapping mapping)
        {
            if (new[] { Mapping.DatabaseDynamicMapping, Mapping.FileDynamicMapping }.Contains(mapping))
            {
                return 240;
            }
            else
            {
                return 144;
            }
        }

        public static string GetFileContentId(Mapping mapping)
        {
            if (new[] { Mapping.DatabaseDynamicMapping, Mapping.FileDynamicMapping }.Contains(mapping))
            {
                return "771";
            }
            else
            {
                return "628";
            }
        }
    }
}
