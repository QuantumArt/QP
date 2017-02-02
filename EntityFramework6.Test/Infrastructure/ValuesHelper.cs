using System.Linq;

namespace EntityFramework6.Test.Infrastructure
{
    public class ValuesHelper
    {
        public static int GetNonPublishedStatus(Mapping mapping)
        {
            if (IsDynamicMapping(mapping))
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
            if (IsDynamicMapping(mapping))
            {
                return "771";
            }
            else
            {
                return "628";
            }
        }

        public static int GetSchemaContentId(Mapping mapping)
        {
            if (IsDynamicMapping(mapping))
            {
                return 758;
            }
            else
            {
                return 620;
            }
        }

        public static int GetSchemaTitleFieldId(Mapping mapping)
        {
            if (IsDynamicMapping(mapping))
            {
                return 38529;
            }
            else
            {
                return 38031;
            }
        }

        private static bool IsDynamicMapping(Mapping mapping)
        {
            return new[] { Mapping.DatabaseDynamicMapping, Mapping.FileDynamicMapping }.Contains(mapping);
        }
    }
}
