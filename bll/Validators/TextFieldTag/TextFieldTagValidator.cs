using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Validators.TextFieldTag;

public class TextFieldTagValidator
{
    private static readonly Regex AllTagsRegex = new("<[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TagNameRegex = new("<([^\\s>/]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SrcAttributeRegex = new("(srcset|src|href)([\\s=]+)([\"']?)([^\\s\"'>/]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static void Validate(string formName, string value, RulesException<Article> errors)
    {
        List<TagInfo> tagInfos = new();
        MatchCollection tags = AllTagsRegex.Matches(value);

        foreach (Match tag in tags)
        {
            string tagName = TagNameRegex.Match(tag.Value).Groups[1].Value;
            tagInfos.Add(new() { Tag = tag.Value, TagName = tagName});
        }

        List<TagInfo> allowedTags = tagInfos.Where(x => QPContext.TextFieldTagValidation.AllowedTags.Any(y => y.Tag == x.TagName)).ToList();
        List<TagInfo> disallowedTags = tagInfos.Where(x => QPContext.TextFieldTagValidation.AllowedTags.All(y => y.Tag != x.TagName)).ToList();

        if (disallowedTags.Count > 0)
        {
            errors.Error(formName, value, string.Format(ArticleStrings.RestictedHtmlTag, string.Join(",", disallowedTags.Select(x => x.TagName))));
        }

        foreach (TagInfo tag in allowedTags)
        {
            List<string> allowedDomains = QPContext.TextFieldTagValidation.AllowedTags
               .Where(x => x.Tag == tag.TagName)
               .Select(x => x.AllowedDomains)
               .First();

            if (allowedDomains is null)
            {
                continue;
            }

            MatchCollection srcAttributes = SrcAttributeRegex.Matches(tag.Tag);
            foreach (Match srcAttribute in srcAttributes)
            {
                if (srcAttribute.Groups.Count == 0)
                {
                    continue;
                }

                Uri url = new(srcAttribute.Groups[4].Value);

                if (!allowedDomains.Contains(url.Host))
                {
                    errors.Error(formName, value, string.Format(ArticleStrings.RestictedSourceInHtmlTag, url.Host, tag.TagName));
                }
            }
        }
    }
}
