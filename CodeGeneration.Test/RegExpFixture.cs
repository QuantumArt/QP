using System;
using System.Linq;
using NUnit.Framework;
using Quantumart.QP8.CodeGeneration.Services;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;

namespace CodeGeneration.Test
{    
    [TestFixture]
    public class RegExpFixture
    {
        private static Regex IncludeRegex = new Regex(@"#@\s*include\s+file=""(.*\\([^\\]*))""\s*#", RegexOptions.Compiled);
        private static Regex CodeFileRegex = new Regex(@"AddCodeFile\(@?""(.*)""", RegexOptions.Compiled);
        private static Regex ReplaceIncludeRegex = new Regex(@"(#@\s*include\s+file="")(.*)(\\[^\\]*)(""\s*#)", RegexOptions.Compiled);
        private static Regex ReplaceCodeFileRegex = new Regex(@"(AddCodeFile\(@?"")(.*)(\\[^\\""]*)("")", RegexOptions.Compiled);


        [Test]
        public void RegExp_test1()
        {
            var files = new List<string>();

            var text = GetText();

            foreach (Regex re in new[] { IncludeRegex, CodeFileRegex })
            {
                foreach (Match match in re.Matches(text))
                {
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var file = match.Groups[1].Value;
                    files.Add(file);
                }
            }

            Assert.That(files, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void RegExp_test2()
        {
            var text = GetText();
            var newText = ReplaceIncludeRegex.Replace(text, @"$1QPDataContextInclude$3$4");
            newText = ReplaceCodeFileRegex.Replace(newText, @"$1QPDataContextInclude$3$4");
            Assert.That(newText, Is.Not.Null);
        }

        private string GetText()
        {
            string path = @"C:\Source\Repos\QP\EntityFramework6.Test\DataContext\QPDataContextGenerator.tt";
            string text = File.ReadAllText(path);
            return text;
        }   
    }     
}
