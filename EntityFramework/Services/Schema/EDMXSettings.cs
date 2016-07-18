using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework.Services
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
                //TODO: implementation              
            }).First();

            return result;
        }
    }
}
