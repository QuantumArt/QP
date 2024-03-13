using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Validators.TextFieldTag;

public class TextFieldTagValidator
{
    private record TagInfo(string Name, string Contents);

    private static readonly Regex AllTagsRegex = new("<[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TagNameRegex = new("<(?<TagName>[^\\s>/]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SrcAttributeRegex = new("(formaction|codebase|cite|background|srcset|src|href|action|longdesc|profile|usemap|data|classid|icon|manifest|poster|archive)(?:[\\s=]+)(?<Qoute>[\"'])?(?<Addresses>(?(Qoute)[^\"']+|[^\\s>]+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly string[] UrlSeparators = { " ", ",", ";" };

    public static void Validate(string formName, string value, RulesException<Article> errors)
    {
        List<TagInfo> tagInfos = new();
        MatchCollection tags = AllTagsRegex.Matches(value);

        foreach (Match tag in tags)
        {
            string tagName = TagNameRegex.Match(tag.Value).Groups["TagName"].Value;
            tagInfos.Add(new(tagName, tag.Value));
        }

        List<TagInfo> allowedTags = tagInfos.Where(x => QPContext.TextFieldTagValidation.AllowedTags.Any(y => y.Tag == x.Name)).ToList();
        List<TagInfo> disallowedTags = tagInfos.Where(x => QPContext.TextFieldTagValidation.AllowedTags.All(y => y.Tag != x.Name)).ToList();

        if (disallowedTags.Count > 0)
        {
            errors.Error(formName, value, string.Format(ArticleStrings.RestictedHtmlTag, string.Join(",", disallowedTags.Select(x => x.Name))));
        }

        foreach (TagInfo tag in allowedTags)
        {
            List<string> allowedDomains = QPContext.TextFieldTagValidation.AllowedTags
               .Where(x => x.Tag == tag.Name)
               .Select(x => x.AllowedDomains)
               .First();

            if (allowedDomains is { Count: 0 })
            {
                continue;
            }

            MatchCollection srcAttributes = SrcAttributeRegex.Matches(tag.Contents);
            foreach (Match srcAttribute in srcAttributes)
            {
                string[] addresses = srcAttribute.Groups["Addresses"].Value.Split(UrlSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                foreach (string address in addresses)
                {
                    Uri url;
                    try
                    {
                        url = new(address, UriKind.RelativeOrAbsolute);
                    }
                    catch (UriFormatException)
                    {
                        continue;
                    }

                    if (!url.IsAbsoluteUri || string.IsNullOrWhiteSpace(url.Host))
                    {
                        continue;
                    }

                    if (!allowedDomains.Contains(url.Host))
                    {
                        errors.Error(formName, value, string.Format(ArticleStrings.RestictedSourceInHtmlTag, url.Host, tag.Name));
                    }
                }
            }
        }
    }
}
