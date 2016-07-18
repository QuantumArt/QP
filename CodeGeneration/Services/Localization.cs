using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.CodeGeneration.Services
{
    public class Localization
    {
        public bool UseSelectiveMappings { get; set; }
        public bool GenerateMappingsRuntime { get; set; }
        public bool CaseSensitive { get; set; }
        public string Pattern { get; set; }
        public Map[] CultureMappings { get; set; }

    }
}
