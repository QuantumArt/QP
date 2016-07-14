using System;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace EFCodeFirstV6Tests
{
    [TestClass]
    public class ModelGeneratorTests
    {
        [TestMethod]
        public void Test_ModelGenerator()
        {
            var mr = new ModelReader(Path.Combine(Environment.CurrentDirectory, "EF6ModelMappingResult.xml"), Console.WriteLine);
            foreach (var c in mr.Contents)
            {
                Console.WriteLine(c.Name);
                foreach (var at in c.Attributes)
                {
                    Console.WriteLine("\t" + at.Name);

                    Assert.IsNotNull(at.Content, "content");
                    Assert.IsNotNull(at.MappedName, "MappedName");
                    Assert.AreEqual(c, at.Content);

                    if (at.IsRelation)
                    {

                        Assert.IsNotNull(at.RelatedContent, "related content");
                    }

                    if (at.IsO2M)
                    {
                        Assert.IsNotNull(at.RelatedAttribute, "RelatedAttribute");
                    }

                    if (at.IsM2O)
                    {
                        Assert.IsNotNull(at.RelatedAttribute, "RelatedAttribute");
                    }

                    if (at.IsM2M)
                    {
                        var link = mr.Links.FirstOrDefault(x => x.Id == at.LinkId);
                        Assert.IsNotNull(link, "has link");
                        Assert.AreEqual(link, at.Link);
                        Assert.IsTrue(at.IsSource.HasValue || at.IsTarget.HasValue, "is source or target");

                        Assert.IsNotNull(at.RelatedContent, "RelatedContent");

                        if (at.IsSource == true)
                        {
                            Assert.AreEqual(link.ContentId, at.ContentId);
                        }

                        if (at.IsTarget == true)
                        {
                            Assert.AreEqual(link.LinkedContentId, at.ContentId);
                        }

                        Assert.IsNotNull(at.RelatedAttribute, "RelatedAttribute m2m");

                    }

                }
            }
        }

        [TestMethod]
        public void Test_Localization_extraction()
        {
            var pattern = "{fieldName}{cultureAlias}";
            Localization localization = GetLocalization(pattern);

            var result = localization.GetCultureMappings("TitleTAT").ToList();
            Assert.AreEqual(1, result.Count, "count");
            Assert.AreEqual("TAT", result[0].CultureAlias);
            Assert.AreEqual("Title", result[0].To);
        }

        [TestMethod]
        public void Test_Localization_extraction_reversed_pattern()
        {
            var pattern = "{cultureAlias}_{fieldName}";
            Localization localization = GetLocalization(pattern);

            var result = localization.GetCultureMappings("TAT_Title").ToList();
            Assert.AreEqual(1, result.Count, "count");
            Assert.AreEqual("TAT", result[0].CultureAlias);
            Assert.AreEqual("Title", result[0].To);
        }

        private static Localization GetLocalization(string pattern)
        {
            return new Localization
            {
                CaseSensitive = true,
                Pattern = pattern,
                CultureMappings = new Map[] {
                    new Map { CultureAlias="EN", To="en-us" },
                    new Map { CultureAlias="TAT", To="ru-tt" },
                    new Map { CultureAlias = "RUS", To="ru-ru"}
                }
            }.Initialize();
        }
    }
}
