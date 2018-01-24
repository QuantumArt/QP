using System;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal class XmlConsoleArgsProcessor : BaseConsoleArgsProcessor
    {
        private bool _disableFieldIdentity;
        private bool _disableContentIdentity;
        private bool _useGuidSubstitution;
        private bool _disableDataIntegrity;

        protected internal override OptionSet BuildOptionSet()
        {
            return new OptionSet
            {
                { "d|disable=", "disable identity options: [field|content]", ParseDisableOption },
                { "useGuid", "enable guid substitution mode", ug => _useGuidSubstitution = ug != null },
                { "disableDataIntegrity", "enable guid substitution mode", ug => _disableDataIntegrity = ug != null }
            };
        }

        protected internal override BaseSettingsModel CreateSettingsFromArguments() =>
            new XmlSettingsModel(FilePathes, CustomerCode, ConfigPath, _disableFieldIdentity, _disableContentIdentity, _useGuidSubstitution, _disableDataIntegrity);

        protected internal override void PrintEnteredData()
        {
            Console.WriteLine("Disable Field Identity: " + _disableFieldIdentity);
            Console.WriteLine("Disable Content Identity: " + _disableContentIdentity);
            Console.WriteLine("Use Guid substitution: " + _useGuidSubstitution);
            base.PrintEnteredData();
        }

        private void ParseDisableOption(string disableOption)
        {
            if (!string.IsNullOrWhiteSpace(disableOption))
            {
                foreach (var option in disableOption.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (option.ToLower())
                    {
                        case "field":
                            _disableFieldIdentity = true;
                            break;
                        case "content":
                            _disableContentIdentity = true;
                            break;
                        default:
                            throw new NotImplementedException($"Unknow option \"d|disable=\": \"{option.ToLower()}\"");
                    }
                }
            }
        }
    }
}
