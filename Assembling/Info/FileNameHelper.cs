using System.IO;

namespace QA_Assembling.Info
{
    public class FileNameHelper
    {

        public string SiteRoot { get; set; }

        public string DataContextClass { get; set; }

        public bool ProceedMappingWithDb { get; set; }
        
        private string AppDataFile(string fileName)
        {
            return $@"{SiteRoot}\App_Data\{fileName}";
        }

        private string AppCodeFile(string fileName)
        {
            return $@"{SiteRoot}\App_Code\{fileName}";
        }

        private string GetPrefixedFileName(string fileName)
        {
            return $"{DataContextClass}{fileName}";
        }

        public string OldGeneratedMappingXmlFileName => OldDefaultMappingXmlFileName;

        public string ImportedMappingXmlFileName => File.Exists(OldMappingXmlFileName) ? OldMappingXmlFileName : OldDefaultMappingXmlFileName;

        public string UsableMappingXmlFileName => ProceedMappingWithDb ? MappingXmlFileName : OldMappingXmlFileName;

        public string OldDefaultMappingXmlFileName => AppDataFile("DefaultMapping.xml");

        public string OldMappingXmlFileName => AppDataFile("Mapping.xml");

        public string OldMappingResultXmlFileName => AppDataFile("MappingResult.xml");

        public string MappingXmlFileName => AppDataFile(GetPrefixedFileName("Mapping.xml"));

        public string MappingResultXmlFileName => AppDataFile(GetPrefixedFileName("MappingResult.xml"));

        public string MappingXsltFileName => AppDataFile("Mapping.xslt");

        public string ManyXsltFileName => AppDataFile("Many.xslt");

        public string ModificationXsltFileName => AppDataFile("Modifications.xslt");

        public string ExtensionsXsltFileName => AppDataFile("Extensions.xslt");


        public string DbmlFileName => AppDataFile(GetPrefixedFileName(".dbml"));


        public string MapFileName => AppDataFile(GetPrefixedFileName(".map"));

        public string SqlMetalLogFileName => AppDataFile(GetPrefixedFileName(".log"));

        public string MainCodeFileName => AppCodeFile(GetPrefixedFileName(".cs"));

        public string FakeCodeFileName => AppDataFile(GetPrefixedFileName(".cs"));

        public string ExtendCodeFileName => AppCodeFile(GetPrefixedFileName("Many.cs"));

        public string ModificationCodeFileName => AppCodeFile(GetPrefixedFileName("Modifications.cs"));

        public string ExtensionsCodeFileName => AppCodeFile(GetPrefixedFileName("Extensions.cs"));


        public string OldExtensionsCodeFileName => AppCodeFile("UserExtensions.cs");
    }
}
