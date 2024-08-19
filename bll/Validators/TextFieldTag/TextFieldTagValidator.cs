using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Validators.TextFieldTag;

public class TextFieldTagValidator
{
    private record TagInfo(string Name, string Contents);

    private static readonly Regex AllTagsRegex = new("<[^/][^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex TagNameRegex = new("<(?<TagName>[^\\s>/]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SrcAttributeRegex = new("(?:formaction|codebase|cite|background|srcset|src|href|action|longdesc|profile|usemap|data|classid|icon|manifest|poster|archive)(?:[\\s=]+)(?<Quote>[\"'])?(?<Addresses>(?(Quote)[^\"']+|[^\\s>]+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EventAttributeRegex = new("(?:\\s)(?<Event>on[a-z]+)\\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex RestrictedText = new("(?<Text>javascript|expression)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly string[] UrlSeparators = { " ", ",", ";" };

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public static void Validate(FieldValue fieldValue, RulesException<Article> errors)
    {
        List<TagInfo> tagInfos = new();
        MatchCollection tags = AllTagsRegex.Matches(fieldValue.Value);

        foreach (Match tag in tags)
        {
            string tagName = TagNameRegex.Match(tag.Value).Groups["TagName"].Value;
            tagInfos.Add(new(tagName, tag.Value));
        }

        List<TagInfo> allowedTags = tagInfos.Where(x => QPContext.TextFieldTagValidation.AllowedTags.Any(y => y.Tag == x.Name)).ToList();
        CheckDisallowedTags(fieldValue, tagInfos, errors);

        foreach (TagInfo tag in allowedTags)
        {
            CheckEventAttributes(fieldValue, errors, tag);
            CheckJsInjectionInAttributes(fieldValue, errors, tag);
            CheckAllowedDomains(fieldValue, errors, tag);
        }
    }

    private static void CheckAllowedDomains(FieldValue fieldValue, RulesException<Article> errors, TagInfo tag)
    {
        List<string> allowedDomains = QPContext.TextFieldTagValidation.AllowedTags
            .Where(x => x.Tag == tag.Name)
            .Select(x => x.AllowedDomains)
            .First();

        if (allowedDomains is null or { Count: 0 })
        {
            return;
        }

        MatchCollection srcAttributes = SrcAttributeRegex.Matches(tag.Contents);

        foreach (Match srcAttribute in srcAttributes)
        {
            string[] addresses = srcAttribute.Groups["Addresses"].Value.Split(UrlSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            foreach (string address in addresses)
            {
                CheckAddressIsAllowed(fieldValue,
                    errors,
                    tag,
                    address,
                    allowedDomains);
            }
        }
    }

    private static void CheckAddressIsAllowed(
        FieldValue fieldValue,
        RulesException<Article> errors,
        TagInfo tag,
        string address,
        ICollection<string> allowedDomains
    )
    {
        Uri url;
        try
        {
            url = new(address, UriKind.RelativeOrAbsolute);
        }
        catch (UriFormatException)
        {
            return;
        }

        if (!url.IsAbsoluteUri || string.IsNullOrWhiteSpace(url.Host))
        {
            return;
        }

        if (!allowedDomains.Contains(url.Host))
        {
            AddError(fieldValue, string.Format(ArticleStrings.RestictedSourceInHtmlTag, url.Host, tag.Name), errors);
        }
    }

    private static void CheckJsInjectionInAttributes(FieldValue fieldValue, RulesException<Article> errors, TagInfo tag)
    {
        string decodedValue = HttpUtility.UrlDecode(tag.Contents);

        if (string.IsNullOrWhiteSpace(decodedValue))
        {
            return;
        }

        MatchCollection restrictedTexts = RestrictedText.Matches(decodedValue);

        if (restrictedTexts.Any())
        {
            AddError(fieldValue, string.Format(ArticleStrings.JavaScriptInjectionError, string.Join(',', restrictedTexts.Select(x => x.Groups["Text"].Value).Distinct()), tag.Name), errors);
        }
    }

    private static void CheckEventAttributes(FieldValue fieldValue, RulesException<Article> errors, TagInfo tag)
    {
        if (QPContext.TextFieldTagValidation.AllowEventAttributes)
        {
            return;
        }

        MatchCollection eventAttributes = EventAttributeRegex.Matches(tag.Contents);

        if (eventAttributes.Any())
        {
            AddError(fieldValue, string.Format(ArticleStrings.RestrictedEventAttribute, string.Join(',', eventAttributes.Select(x => x.Groups["Event"].Value).Distinct()), tag.Name), errors);
        }
    }

    private static void CheckDisallowedTags(FieldValue fieldValue, IEnumerable<TagInfo> tagInfos, RulesException<Article> errors)
    {
        List<TagInfo> disallowedTags = tagInfos.Where(x => QPContext.TextFieldTagValidation.AllowedTags.All(y => y.Tag != x.Name)).ToList();

        if (disallowedTags.Count > 0)
        {
            AddError(fieldValue, string.Format(ArticleStrings.RestictedHtmlTag, string.Join(',', disallowedTags.Select(x => x.Name).Distinct())), errors);
        }
    }

    private static void AddError(FieldValue fieldValue, string error, RulesException<Article> errors)
    {
        errors.Error(fieldValue.Field.FormName, fieldValue.Value, error);

        if (QPContext.TextFieldTagValidation.LogValidationError)
        {
            Logger.Warn()
                .Message("Attempt to save malicious text in article. User Name: {User}\nIP: {IP}\nContent Id: {ContentId}\nContent Name: {ContentName}\nArticle Id: {ArticleId}\nField: {FieldName}\nData: {UserInput}\nValidation Error: {ValidationError}",
                    QPContext.CurrentUserName,
                    QPContext.GetUserIpAddress(),
                    fieldValue.Article.ContentId,
                    fieldValue.Article.DisplayContentName,
                    fieldValue.Article.Id,
                    fieldValue.Field.Name,
                    fieldValue.Value,
                    error)
                .Write();
        }
    }
}
