using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EFCodeFirstV6Tests
{

    public class Localization
    {
        private Regex[] _patterns;
        private string[] _cultures;

        public bool UseSelectiveMappings { get; set; }
        public bool GenerateMappingsRuntime { get; set; }
        public bool CaseSensitive { get; set; }
        public string Pattern { get; set; }
        public Map[] CultureMappings { get; set; }

        public string DefaultCultureName { get; set; }

        public Localization Initialize()
        {
            _patterns = GetExpressions();
            _cultures = this.CultureMappings.Select(x => x.To.ToLower()).Distinct().ToArray();

            return this;
        }

        public bool ShouldSkip(IEnumerable<string> allAttributes, string netName)
        {
            return false;
        }

        public string ResolveColumnName(string netName, string cultureName)
        {
            return netName;
        }

        public IEnumerable<string> GetCultures()
        {
            return _cultures;
        }
        public IEnumerable<Map> GetCultureMappings(string netName)
        {
            int ci = Pattern.IndexOf("{fieldName}") < Pattern.IndexOf("{cultureAlias}") ? 2 : 1;
            int fi = ci == 1 ? 2 : 1;

            return _patterns.Select(p => p.Match(netName))
                 .Cast<Match>()
                 .Where(x => x.Groups.Count > 2)
                 .Select(x => new
                 {
                     fieldNameGroup =  x.Groups[fi],
                     cultureAliasGroup = x.Groups[ci]
                 })
                 .Where(x => x.fieldNameGroup.Success && x.cultureAliasGroup.Success)
                 .Select(x => new Map { CultureAlias = x.cultureAliasGroup.Value, To = x.fieldNameGroup.Value });

        }

        private Regex[] GetExpressions()
        {
            return CultureMappings
                    .Select(cm =>
                        GetExpression(cm)).ToArray();
        }

        private Regex GetExpression(Map cm)
        {
            // {fieldName}{cultureAlias}
            // ^(\w*)(EN)$
            return new Regex(string.Format(@"^{0}$", Pattern.Replace("{fieldName}", @"(\w*)").Replace("{cultureAlias}",
                                            string.Format("({0})", cm.CultureAlias)),
                                        !this.CaseSensitive ? RegexOptions.IgnoreCase : RegexOptions.None));
        }
    }
    public class Map
    {
        public string CultureAlias { get; set; }
        public string To { get; set; }
    }
    public class EDMXSettings
    {
        public string QPContextMappingResultPath { get; set; }
        public bool GenerateModel { get; set; }
        public bool LazyLoadingEnabled { get; set; }
        public bool GenerateLive { get; set; }
        public bool GenerateStage { get; set; }
        public bool GenerateUnion { get; set; }
        public bool InheritTableExtensions { get; set; }

        public bool GenerateClasses { get; set; }
        public bool GenerateExtensions { get; set; }
        public bool GenerateInterface { get; set; }
        public bool UseContextNameAsConnectionString { get; set; }
        public bool UseReversedAssociations { get; set; }

        public Localization Localization { get; set; }

        public static EDMXSettings Parse(string path)
        {
            var doc = System.Xml.Linq.XDocument.Load(path);

            var result = doc.Descendants("settings").Select(x => new EDMXSettings
            {
                QPContextMappingResultPath = GetElementValue<string>(x, "QPContextMappingResultPath"),
                GenerateModel = GetElementValue<bool>(x, "GenerateModel"),
                GenerateClasses = GetElementValue<bool>(x, "GenerateClasses"),
                LazyLoadingEnabled = GetElementValue<bool>(x, "LazyLoadingEnabled"),
                GenerateStage = GetElementValue<bool>(x, "GenerateStage"),
                GenerateLive = GetElementValue<bool>(x, "GenerateLive"),
                GenerateUnion = GetElementValue<bool>(x, "GenerateUnion"),
                UseContextNameAsConnectionString = GetElementValue<bool>(x, "UseContextNameAsConnectionString"),

                InheritTableExtensions = GetElementValue<bool>(x, "InheritTableExtensions"),
                GenerateExtensions = GetElementValue<bool>(x, "GenerateExtensions"),
                GenerateInterface = GetElementValue<bool>(x, "GenerateInterface"),
                UseReversedAssociations = GetElementValue<bool>(x, "UseReversedAssociations"),
                Localization = x.Descendants("Localization").Select(
                    l => new Localization
                    {
                        CaseSensitive = GetElementValue<bool>(l, "CaseSensitive"),
                        GenerateMappingsRuntime = GetElementValue<bool>(l, "GenerateMappingsRuntime"),
                        UseSelectiveMappings = GetElementValue<bool>(l, "UseSelectiveMappings"),
                        Pattern = GetElementValue<string>(l, "Pattern"),
                        CultureMappings = x.Descendants("CultureMappings").Select(
                             cm => new Map
                             {
                                 CultureAlias = GetAttribute<string>(cm, "cultureAlias"),
                                 To = GetAttribute<string>(cm, "to")
                             }).ToArray()
                    }).FirstOrDefault()
            }).First();



            return result;
        }

        public static T GetAttribute<T>(XElement e, string name, bool required = false, T fallbackValue = default(T))
        {
            try
            {
                var a = e.Attribute(name);
                if (a == null || a.Value == null || a.Value == "")
                {
                    if (required) throw new Exception("value should not be null or empty");
                    return fallbackValue;
                }
                if (typeof(T) == typeof(bool) && a.Value.Length == 1)
                    return (T)(object)Convert.ToBoolean(Convert.ToInt16(a.Value));

                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFromInvariantString(a.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("problem with element " + name, ex);
            }
        }

        public static T GetElementValue<T>(XElement e, string name, bool required = false)
        {
            try
            {
                var a = e.Elements().FirstOrDefault(x => x.Name == name);
                if (a == null || a.Value == null || a.Value == "")
                {
                    if (required) throw new Exception("value should not be null or empty");
                    return default(T);
                }
                if (typeof(T) == typeof(bool) && a.Value.Length == 1)
                    return (T)(object)Convert.ToBoolean(Convert.ToInt16(a.Value));

                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFromInvariantString(a.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("problem with element " + name, ex);
            }
        }
    }
}
