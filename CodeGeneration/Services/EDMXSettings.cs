using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.CodeGeneration.Services
{
    public class EDMXSettings
    {
        public string QPContextMappingResultPath { get; set; }
        public bool GenerateModel { get; set; }
        public bool GenerateMappings { get; set; }
        public bool LazyLoadingEnabled { get; set; }
        public bool GenerateLive { get; set; }
        public bool GenerateStage { get; set; }
        public bool GenerateUnion { get; set; }
        public bool InheritTableExtensions { get; set; }
        public bool GenerateClasses { get; set; }
        public bool GenerateExtensions { get; set; }
        public bool GenerateInterface { get; set; }
        public bool GenerateMappingInterface { get; set; }
        public bool UseContextNameAsConnectionString { get; set; }
        public bool UseReversedAssociations { get; set; }
        public bool PlaceContentsInSeparateFiles { get; set; }
        public string Usings { get; set; }
        public Localization Localization { get; set; }

        public static EDMXSettings Parse(string path)
        {
            var doc = System.Xml.Linq.XDocument.Load(path);

            var result = doc.Descendants("settings").Select(x => new EDMXSettings
            {
                QPContextMappingResultPath = RootUtil.GetElementValue<string>(x, "QPContextMappingResultPath"),
                GenerateModel = RootUtil.GetElementValue<bool>(x, "GenerateModel"),
                GenerateMappings = RootUtil.GetElementValue<bool>(x, "GenerateMappings"),
                GenerateMappingInterface = RootUtil.GetElementValue<bool>(x, "GenerateMappingInterface"),
                GenerateClasses = RootUtil.GetElementValue<bool>(x, "GenerateClasses"),
                LazyLoadingEnabled = RootUtil.GetElementValue<bool>(x, "LazyLoadingEnabled"),
                GenerateStage = RootUtil.GetElementValue<bool>(x, "GenerateStage"),
                GenerateLive = RootUtil.GetElementValue<bool>(x, "GenerateLive"),
                GenerateUnion = RootUtil.GetElementValue<bool>(x, "GenerateUnion"),
                UseContextNameAsConnectionString = RootUtil.GetElementValue<bool>(x, "UseContextNameAsConnectionString"),
                Usings = RootUtil.GetElementValue<string>(x, "Usings") ?? "",
                InheritTableExtensions = RootUtil.GetElementValue<bool>(x, "InheritTableExtensions"),
                GenerateExtensions = RootUtil.GetElementValue<bool>(x, "GenerateExtensions"),
                GenerateInterface = RootUtil.GetElementValue<bool>(x, "GenerateInterface"),
                PlaceContentsInSeparateFiles = RootUtil.GetElementValue<bool>(x, "PlaceContentsInSeparateFiles"),
                UseReversedAssociations = RootUtil.GetElementValue<bool>(x, "UseReversedAssociations"),
                Localization = x.Descendants("Localization").Select(
                    l => new Localization
                    {
                        CaseSensitive = RootUtil.GetElementValue<bool>(l, "CaseSensitive"),
                        GenerateMappingsRuntime = RootUtil.GetElementValue<bool>(l, "GenerateMappingsRuntime"),
                        UseSelectiveMappings = RootUtil.GetElementValue<bool>(l, "UseSelectiveMappings"),
                        Pattern = RootUtil.GetElementValue<string>(l, "Pattern"),
                        CultureMappings = x.Descendants("CultureMappings").Select(
                             cm => new Map
                             {
                                 CultureAlias = RootUtil.GetAttribute<string>(cm, "cultureAlias"),
                                 To = RootUtil.GetAttribute<string>(cm, "to")
                             }).ToArray()
                    }).FirstOrDefault()
            }).First();

            return result;
        }
    }
}
