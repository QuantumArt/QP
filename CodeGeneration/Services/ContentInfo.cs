using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.CodeGeneration.Services
{
    public class ContentInfo
    {
        public int Id { get; set; }
        public bool IsStageMode { get; set; }
        public string Name { get; set; }
        public string MappedName { get; set; }
        public string PluralMappedName { get; set; }
        public string Description { get; set; }
        public bool UseDefaultFiltration { get; set; }
        public bool IsVirtual { get; set; }
        public List<AttributeInfo> Attributes { get; set; }
        public IEnumerable<AttributeInfo> Columns { get { return Attributes.Where(x => !x.IsM2M && !x.IsM2O); } }
        public bool DoPostProcessing { get { return !SkipPostProcessing && Attributes.Any(x => x.CanContainPlaceholders); } }
        public bool SkipPostProcessing { get; set; }
        public bool SplitArticles { get; set; }

        public string TableName
        {
            get
            {
                var name = "content_" + Id;
                if (UseDefaultFiltration)
                    name += (IsStageMode ? "_stage_new" : "_live_new");
                else
                    name += "_united_new";
                return name;
            }
        }
    }
}
