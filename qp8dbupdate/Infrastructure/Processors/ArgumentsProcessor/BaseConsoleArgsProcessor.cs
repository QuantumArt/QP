using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Helpers;
using Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Models;
using Quantumart.QP8.WebMvc.Infrastructure.Exceptions;

namespace Quantumart.QP8.ConsoleDbUpdate.Infrastructure.Processors.ArgumentsProcessor
{
    internal abstract class BaseConsoleArgsProcessor
    {
        protected internal int VerboseLevel;

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
            Console.WriteLine("Verbosity Level: " + VerboseLevel);
            Console.WriteLine();
            ConsoleHelpers.MakeSureUserWantToContinue();
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
            catch (OptionException ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine);
                Console.WriteLine("Try `qp8update --help' for more information.");
                Console.WriteLine(Environment.NewLine);
                throw new XmlDbUpdateReplayActionException("There was an error while parsing options", ex);
            }

            Console.WriteLine("Parsing is started. Selected options: ");
            PrintEnteredData();

            return CreateSettingsFromArguments();
        }

        protected internal OptionSet AddCommonOptions(OptionSet optionSet)
        {
            optionSet.Add("p|path=", "single or multiple <path> to file|directory with xml|csv record actions to replay.", p => FilePathes.Add(p));
            optionSet.Add("c|config=", "the <path> of xml|csv config file to apply", c => ConfigPath = c);
            optionSet.Add("v|verbose", "increase debug message verbosity", v => ++VerboseLevel);
            optionSet.Add("h|help", "show this message and exit", h => ShouldShowHelp = h != null);
            return optionSet;
        }

        private void ValidateInputData(ICollection<string> noNamedOptions)
        {
            if (noNamedOptions.Count != 1)
            {
                throw new OptionException("Should specify single not named parameter <customer_code>", "customer_code");
            }

            if (!FilePathes.Any())
            {
                throw new OptionException("Should specify at least one xml file or folder path", "path");
            }

            CustomerCode = noNamedOptions.Single();
            if (string.IsNullOrWhiteSpace(CustomerCode))
            {
                throw new OptionException("Should specify <customer_code>", "customer_code");
            }
        }

        private static void ShowCommandLineHelp(OptionSet optionsSet)
        {
            Console.WriteLine("USAGE: qp8update [OPTIONS]+ <customer_code>" + Environment.NewLine);
            Console.WriteLine("OPTIONS:");
            optionsSet.WriteOptionDescriptions(Console.Out);

            Console.WriteLine(Environment.NewLine);
            Environment.Exit(0);
        }
    }
}
