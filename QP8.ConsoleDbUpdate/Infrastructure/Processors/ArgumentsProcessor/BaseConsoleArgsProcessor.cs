using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using QP8.Infrastructure.Logging;
using QP8.Infrastucture;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Enums;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal abstract class BaseConsoleArgsProcessor
    {
        protected internal bool ShouldShowHelp;

        protected internal string CustomerCode;

        protected internal IList<string> FilePathes;

        protected internal string ConfigPath;

        protected BaseConsoleArgsProcessor()
        {
            FilePathes = new List<string>();
        }

        protected internal abstract BaseSettingsModel CreateSettingsFromArguments();

        protected internal virtual OptionSet BuildOptionSet()
        {
            return new OptionSet();
        }

        protected internal virtual void PrintEnteredData()
        {
            Console.WriteLine("Customer Code: " + CustomerCode);
            Console.WriteLine("File Pathes: " + string.Join(", ", FilePathes));
            Console.WriteLine("Config: " + (string.IsNullOrWhiteSpace(ConfigPath) ? "disabled" : ConfigPath));
            Console.WriteLine("Verbosity Level: " + Program.VerboseLevel);
            Console.WriteLine("Is silent mode enabled: " + Program.IsSilentModeEnabled);
            Console.WriteLine();

            if (!Program.IsSilentModeEnabled)
            {
                ConsoleHelpers.MakeSureUserWantToContinue();
            }
        }

        public BaseSettingsModel ParseConsoleArguments(string[] args)
        {
            var optionSet = BuildOptionSet();
            optionSet = AddCommonOptions(optionSet);

            try
            {
                var noNamedOptions = optionSet.Parse(args);
                if (ShouldShowHelp)
                {
                    ShowCommandLineHelp(optionSet);
                }

                ValidateInputData(noNamedOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine);
                Console.WriteLine("Try `qpdbupdate --help' for more information.");
                Console.WriteLine(Environment.NewLine);
                throw;
            }

            Console.WriteLine();
            PrintEnteredData();

            return CreateSettingsFromArguments();
        }

        protected internal OptionSet AddCommonOptions(OptionSet optionSet)
        {
            optionSet.Add("p|path=", "single or multiple <path> to file|directory with xml|csv record actions to replay", p => FilePathes.Add(p));
            optionSet.Add("c|config=", "the <path> of xml|csv config file to apply", c => ConfigPath = c);
            optionSet.Add("v|verbose", "increase debug message verbosity [v|vv|vvv]:[error|warning|info].", v => { });
            optionSet.Add("s|silent", "enable silent mode for automatization.", s => { });
            optionSet.Add("m|mode=", "single value which represents utility mode [xml|csv]", m => { });
            optionSet.Add("h|help", "show this message and exit", h => ShouldShowHelp = h != null);
            return optionSet;
        }

        private void ValidateInputData(ICollection<string> noNamedOptions)
        {
            Ensure.That<OptionException>(noNamedOptions.Count == 1, "Should specify single not named parameter <customer_code>", "customer_code");
            Ensure.That<OptionException>(FilePathes.Any(), "Should specify at least one xml file or folder path", "path");
            CustomerCode = noNamedOptions.Single();

            Logger.Log.SetGlobalContext(LoggingData.CustomerCodeCustomVariable, CustomerCode);
        }

        private static void ShowCommandLineHelp(OptionSet optionsSet)
        {
            Console.WriteLine("USAGE: qpdbupdate [OPTIONS]+ <customer_code>" + Environment.NewLine);
            Console.WriteLine("OPTIONS:");
            optionsSet.WriteOptionDescriptions(Console.Out);
            ConsoleHelpers.ExitProgram(ExitCode.Success);
        }
    }
}
