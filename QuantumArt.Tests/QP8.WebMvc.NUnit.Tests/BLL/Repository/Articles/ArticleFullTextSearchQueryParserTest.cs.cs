using System;
using System.Collections.Generic;
using NUnit.Framework;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.Utils.FullTextSearch;

namespace QP8.WebMvc.NUnit.Tests.BLL.Repository.Articles
{
    [TestFixture]
    public class ArticleFullTextSearchQueryParserTest
    {
        private class FullTextParserTestData
        {
            public string Description { get; set; }

            public IEnumerable<ArticleSearchQueryParam> SearchQueryParams { get; set; }

            public ISearchGrammarParser SearchGrammarParser { get; set; }

            public bool ExpectedResult { get; set; }

            public bool? ExpectedHasError { get; set; }

            public string ExpectedFieldIdList { get; set; }

            public string ExpectedQueryString { get; set; }

            public Type ExpectedExceptionType { get; set; }
        }

        private class SearchGrammarParserMock : ISearchGrammarParser
        {
            private readonly bool _hasError;
            private readonly string _queryString;
            private readonly string _result;

            public SearchGrammarParserMock()
                : this(false, null, null)
            {
            }

            public SearchGrammarParserMock(bool hasError, string queryString, string result)
            {
                _hasError = hasError;
                _queryString = queryString;
                _result = result;
            }

            public bool TryParse(string queryString, out string result)
            {
                Assert.AreEqual(_queryString, queryString);
                result = _result;
                return !_hasError;
            }
        }

        [Test]
        public void ParseTest_IncorrectParams_Exceptions()
        {
            var testParams = new[]
            {
                new FullTextParserTestData
                {
                    Description = "FieldID has incorrect format 1",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1,string,222",
                            QueryParams = new object[] { "test" }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(),

                    ExpectedExceptionType = typeof(FormatException)
                },

                new FullTextParserTestData
                {
                    Description = "FieldID has incorrect format 2",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1,,222",
                            QueryParams = new object[] { "test" }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(),

                    ExpectedExceptionType = typeof(FormatException)
                },

                new FullTextParserTestData
                {
                    Description = "QueryParams is null",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1,2,3",
                            QueryParams = null
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(),

                    ExpectedExceptionType = typeof(ArgumentException)
                },
                new FullTextParserTestData
                {
                    Description = "QueryParams is empty",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1,2,3",
                            QueryParams = new object[] { }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(),

                    ExpectedExceptionType = typeof(ArgumentException)
                },
                new FullTextParserTestData
                {
                    Description = "QueryParams has incorrect type",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1,2,3",
                            QueryParams = new object[] { 1 }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(),

                    ExpectedExceptionType = typeof(InvalidCastException)
                }
            };
            ProcessParserTest(testParams);
        }

        [Test]
        public void ParseTest_IronParserHasError_Exceptions()
        {
            var testParams = new[]
            {
                new FullTextParserTestData
                {
                    Description = "Iron Parser HasError",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1,2,3",
                            QueryParams = new object[] { "test" }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(true, "test", null),

                    ExpectedResult = true,
                    ExpectedFieldIdList = null,
                    ExpectedHasError = true,
                    ExpectedQueryString = null
                }
            };

            ProcessParserTest(testParams);
        }

        [Test]
        public void ParseTest_NotProcessedSearchType_EmptyResult()
        {
            var testParams = new[]
            {
                new FullTextParserTestData
                {
                    Description = "SearchQueryParams contain only Not Processed Search Type",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.Boolean },
                        new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.DateRange },
                        new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.M2MRelation },
                        new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.NumericRange },
                        new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.O2MRelation },
                        new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.Text },
                        new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.TimeRange }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(),

                    ExpectedResult = false
                }
            };
            ProcessParserTest(testParams);
        }

        [Test]
        public void ParseTest_EmptyInput_EmptyQueryResult()
        {
            var testParams = new[]
            {
                new FullTextParserTestData
                {
                    Description = "SearchQueryParams is null",
                    SearchQueryParams = null,
                    SearchGrammarParser = new SearchGrammarParserMock(false, null, null),

                    ExpectedResult = false
                },
                new FullTextParserTestData
                {
                    Description = "SearchQueryParams is empty",
                    SearchQueryParams = new ArticleSearchQueryParam[] { },
                    SearchGrammarParser = new SearchGrammarParserMock(false, null, null),

                    ExpectedResult = false
                },
                new FullTextParserTestData
                {
                    Description = "Input string is null",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1,2,3",
                            QueryParams = new object[] { null }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(false, null, null),

                    ExpectedResult = false
                },

                new FullTextParserTestData
                {
                    Description = "Input string is empty",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1,2,3",
                            QueryParams = new object[] { "" }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(false, "", null),

                    ExpectedResult = false
                }
            };

            ProcessParserTest(testParams);
        }

        [Test]
        public void ParseTest_CorrectInput_CorrectQuery()
        {
            var testParams = new[]
            {
                new FullTextParserTestData
                {
                    Description = "Input string is null",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = " 1, 2,3 ",
                            QueryParams = new object[] { "test" }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(false, "test", "res'ult"),

                    ExpectedResult = true,
                    ExpectedFieldIdList = "1,2,3",
                    ExpectedHasError = false,
                    ExpectedQueryString = "res''ult"
                },
                new FullTextParserTestData
                {
                    Description = "Input string is null",
                    SearchQueryParams = new[]
                    {
                        new ArticleSearchQueryParam
                        {
                            SearchType = ArticleFieldSearchType.FullText,
                            FieldID = "1",
                            QueryParams = new object[] { "test" }
                        }
                    },
                    SearchGrammarParser = new SearchGrammarParserMock(false, "test", "res'ult"),

                    ExpectedResult = true,
                    ExpectedFieldIdList = "1",
                    ExpectedHasError = false,
                    ExpectedQueryString = "res''ult"
                }
            };

            ProcessParserTest(testParams);
        }

        private static void ProcessParserTest(IEnumerable<FullTextParserTestData> testDataCollection)
        {
            foreach (var testData in testDataCollection)
            {
                try
                {
                    var result = new ArticleFullTextSearchQueryParser(testData.SearchGrammarParser).Parse(testData.SearchQueryParams, out var hasError, out var fieldIdList, out var queryString, out var _);
                    if (testData.ExpectedExceptionType != null)
                    {
                        Assert.Fail($"\"{testData.Description}\" test is failed. No thrown exception.");
                    }

                    Assert.AreEqual(testData.ExpectedResult, result, $"\"{testData.Description}\" test is failed.");
                    Assert.AreEqual(testData.ExpectedHasError, hasError, $"\"{testData.Description}\" test is failed.");
                    Assert.AreEqual(testData.ExpectedFieldIdList, fieldIdList, $"\"{testData.Description}\" test is failed.");
                    Assert.AreEqual(testData.ExpectedQueryString, queryString, $"\"{testData.Description}\" test is failed.");
                }
                catch (Exception ex)
                {
                    if (testData.ExpectedExceptionType == null)
                    {
                        Assert.Fail($"\"{testData.Description}\" test is failed. Unexpected exception.");
                    }

                    Assert.IsInstanceOf(testData.ExpectedExceptionType, ex, $"\"{testData.Description}\" test is failed. Unexpected exception type.");
                }
            }
        }
    }
}
