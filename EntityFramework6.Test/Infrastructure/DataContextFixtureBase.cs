using NUnit.Framework;
using EntityFramework6.Test.DataContext;
using System.IO;

namespace EntityFramework6.Test.Infrastructure
{
    public class DataContextFixtureBase
    {
        private const string DefaultSiteName = "original_site";
        private const string DynamicSiteName = "dynamic_site";
        private const string DefaultMappingResult = @"DataContext\ModelMappingResult.xml";
        private const string DynamicMappingResult = @"DataContext\DynamicMappingResult.xml";

        protected EF6Model GetDataContext(ContentAccess access, Mapping mapping)
        {

            switch (mapping)
            {
                case Mapping.StaticMapping:
                    return EF6Model.CreateWithStaticMapping(access);
                case Mapping.DatabaseDefaultMapping:
                    return EF6Model.CreateWithDatabaseMapping(access, DefaultSiteName);
                case Mapping.DatabaseDynamicMapping:
                    return EF6Model.CreateWithDatabaseMapping(access, DynamicSiteName);
                case Mapping.FileDefaultMapping:
                    return EF6Model.CreateWithFileMapping(access, GetPath(DefaultMappingResult));
                case Mapping.FileDynamicMapping:
                    return EF6Model.CreateWithFileMapping(access, GetPath(DynamicMappingResult));
                default:
                    return EF6Model.Create();
            }
        }

        protected string GetPath(string file)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, file);
        }
    }   
}
