using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.ArticleRepositories.SearchParsers;

namespace QP8.WebMvc.NUnit.Tests.BLL.Repository.Articles
{
    [TestFixture]
    public class ArticleFilterSearchQueryParserTest
    {
        private class FilterParserTestData
        {
            public string Description { get; set; }

            public IEnumerable<ArticleSearchQueryParam> SearchQueryParams { get; set; }

            public string ExpectedValue { get; set; }

            public Type ExpectedExceptionType { get; set; }
        }

        private static IEnumerable<FilterParserTestData> CreateCommonFilterParseTestData() => new[]
        {
            new FilterParserTestData
            {
                Description = "SearchQueryParams is null",
                SearchQueryParams = null,
                ExpectedValue = null
            },

            new FilterParserTestData
            {
                Description = "SearchQueryParams is empty",
                SearchQueryParams = new ArticleSearchQueryParam[] { },
                ExpectedValue = null
            },

            // тут не обрабатываемые парсером для фильтра параметры поиска
            new FilterParserTestData
            {
                Description = "Thre are no processed SearchQueryParams",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.FullText },
                    new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.M2MRelation },
                    new ArticleSearchQueryParam { SearchType = ArticleFieldSearchType.None }
                },
                ExpectedValue = null
            }
        };

        private static IEnumerable<FilterParserTestData> CreateAllFilterSearchTypeTestData() => new[]
        {
            new FilterParserTestData
            {
                Description = "All type Search: Empty Search",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.Text,
                        FieldColumn = "Text",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, false }
                    },

                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, false }
                    },

                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, false }
                    },
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, false, false }
                    }
                },
                ExpectedValue = null
            },

            new FilterParserTestData
            {
                Description = "All type Search",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.Text,
                        FieldColumn = "Text",
                        FieldId = "1",
                        QueryParams = new object[] { false, "TesT", false }
                    },

                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "5/18/2011", "5/17/2011", false }
                    },

                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "10:58:41 PM", "10:58:41 AM", false }
                    },
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, 1, 0, false, false }
                    },
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "boolean",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { false, true }
                    },
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "o2m",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1, 2, 3 }, false, false }
                    }
                },
                ExpectedValue = "(c.[text] LIKE '%TesT%') AND (c.[date] BETWEEN '20110517 00:00:00' AND '20110518 23:59:59') AND (dbo.qp_abs_time_seconds(c.[time]) BETWEEN 39521 AND 82721) AND (c.[number] BETWEEN 0 AND 1) AND (c.[boolean] = 1) AND (c.[o2m] IN (select id from @field i))"
            }
        };

        private static IEnumerable<FilterParserTestData> CreateTextSearchTestData() => new[]
        {
            new FilterParserTestData
            {
                Description = "Text Search: QueryParams = null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Text,
                        QueryParams = null
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Text Search: QueryParams is Empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Text,
                        QueryParams = new object[] { }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Text Search: QueryParams 1'st param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Text,
                        QueryParams = new object[] { null, "123", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "Text Search: QueryParams 1'st param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Text,
                        QueryParams = new object[] { "string", "123", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "Text Search: QueryParams 2'nd param is absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Text,
                        QueryParams = new object[] { true }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Text Search: QueryParams 2'nd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Text,
                        QueryParams = new object[] { true, 123, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "Text Search: FieldColumn is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = null,
                        SearchType = ArticleFieldSearchType.Text,
                        QueryParams = new object[] { true, "string", false }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Text Search: FieldColumn is empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "",
                        SearchType = ArticleFieldSearchType.Text,
                        QueryParams = new object[] { true, "string", false }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Text Search: IsNull=true, Value=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.Text,
                        FieldColumn = "Text",
                        FieldId = "1",
                        QueryParams = new object[] { true, null, false }
                    }
                },
                ExpectedValue = "(c.[text] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "Text Search: IsNull=true, Value=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.Text,
                        FieldColumn = "Text",
                        FieldId = "1",
                        QueryParams = new object[] { true, "", false }
                    }
                },
                ExpectedValue = "(c.[text] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "Text Search: IsNull=true, Value is not empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.Text,
                        FieldColumn = "Text",
                        FieldId = "1",
                        QueryParams = new object[] { true, "test", false }
                    }
                },
                ExpectedValue = "(c.[text] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "Text Search: IsNull=false, Value=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.Text,
                        FieldColumn = "Text",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "Text Search: IsNull=false, Value=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.Text,
                        FieldColumn = "Text",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "Text Search: IsNull=false, Value is not empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.Text,
                        FieldColumn = "Text",
                        FieldId = "1",
                        QueryParams = new object[] { false, "  ' [ % _ TesT  ", false }
                    }
                },
                ExpectedValue = "(c.[text] LIKE '%'' [[] [%] [_] TesT%')"
            }
        };

        private static IEnumerable<FilterParserTestData> CreateDateRangeTestData() => new[]
        {
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams = null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = null
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams is Empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 1'st param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { null, "5/17/2011", "5/17/2011", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 1'st param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { "string", "5/17/2011", "5/17/2011", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 2'nd; 3'd and 4'd param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 3'd and 4'd param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true, "5/17/2011" }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 4'd param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true, "5/17/2011", "5/17/2011" }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 2'nd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true, 123, "5/17/2011", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 2'nd param has incorect format",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "erkerekj", "", false }
                    }
                },
                ExpectedExceptionType = typeof(FormatException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 3'd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true, "5/17/2011", 321, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 3'd param has incorect format",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "sksfdkgj", false }
                    }
                },
                ExpectedExceptionType = typeof(FormatException)
            },

            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 4'd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true, "5/17/2011", "5/17/2011", "string" }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 4'st param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, "5/17/2011", "5/17/2011", null }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },

            new FilterParserTestData
            {
                Description = "DateRange Search: FieldColumn is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = null,
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true, "5/17/2011", "5/17/2011", false }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: FieldColumn is empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true, "5/17/2011", "5/17/2011", false }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=true, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, null, null, false }
                    }
                },
                ExpectedValue = "(c.[date] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=true, Value1='', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, "", "", false }
                    }
                },
                ExpectedValue = "(c.[date] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=true, Value1='5/17/2011', Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, "5/17/2011", null, false }
                    }
                },
                ExpectedValue = "(c.[date] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=true, Value1='5/17/2011', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, "5/17/2011", "", true }
                    }
                },
                ExpectedValue = "(c.[date] IS NULL)"
            },

            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=true, Value1=null, Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, null, "5/17/2011", false }
                    }
                },
                ExpectedValue = "(c.[date] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=true, Value1='', Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, "", "5/17/2011", false }
                    }
                },
                ExpectedValue = "(c.[date] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=true, Value1='5/17/2011', Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, "5/17/2011", "5/17/2011", false }
                    }
                },
                ExpectedValue = "(c.[date] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1='', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "", false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1='5/17/2011', Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "5/17/2011", null, false }
                    }
                },
                ExpectedValue = "(c.[date] >= '20110517 00:00:00')"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1='5/17/2011', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "5/17/2011", "", false }
                    }
                },
                ExpectedValue = "(c.[date] >= '20110517 00:00:00')"
            },

            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1=null, Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, "5/17/2011", false }
                    }
                },
                ExpectedValue = "(c.[date] <= '20110517 23:59:59')"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1='', Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "5/17/2011", false }
                    }
                },
                ExpectedValue = "(c.[date] <= '20110517 23:59:59')"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1='5/17/2011', Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "5/17/2011", "5/17/2011", false }
                    }
                },
                ExpectedValue = "(c.[date] BETWEEN '20110517 00:00:00' AND '20110517 23:59:59')"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1='5/17/2011', Value2='5/18/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "5/17/2011", "5/18/2011", false }
                    }
                },
                ExpectedValue = "(c.[date] BETWEEN '20110517 00:00:00' AND '20110518 23:59:59')"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, Value1='5/18/2011', Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "5/18/2011", "5/17/2011", false }
                    }
                },
                ExpectedValue = "(c.[date] BETWEEN '20110517 00:00:00' AND '20110518 23:59:59')"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, ByValue = true, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, true }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, ByValue = true, Value1='', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "", true }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, ByValue = true, Value1='5/17/2011', Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "5/17/2011", null, true }
                    }
                },
                ExpectedValue = "(c.[date] BETWEEN '20110517 00:00:00' AND '20110517 23:59:59')"
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, ByValue = true, Value1='5/17/2011', Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "5/17/2011", "5/17/2011", true }
                    }
                },
                ExpectedValue = "(c.[date] BETWEEN '20110517 00:00:00' AND '20110517 23:59:59')"
            },

            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, ByValue = true, Value1=null, Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, "5/17/2011", true }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: IsNull=false, ByValue = true, Value1='', Value2='5/17/2011'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "5/17/2011", true }
                    }
                },
                ExpectedValue = null
            }
        };

        private static IEnumerable<FilterParserTestData> CreateTimeRangeTestData() => new[]
        {
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams = null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = null
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: TimeRange is Empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 1'st param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { null, "10:58:41 PM", "10:58:41 PM", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 1'st param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { "string", "10:58:41 PM", "10:58:41 PM", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 2'nd; 3'd and 4th param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { true }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 3'd and 4th param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { true, "10:58:41 PM" }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 4th param is absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { true, "10:58:41 PM", "10:58:41 PM" }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 2'nd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { true, 123, "10:58:41 PM", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 2'nd param has incorect format",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "erkerekj", "", false }
                    }
                },
                ExpectedExceptionType = typeof(FormatException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 3'd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { true, "10:58:41 PM", 321, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: QueryParams 3'd param has incorect format",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "sksfdkgj", false }
                    }
                },
                ExpectedExceptionType = typeof(FormatException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: FieldColumn is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = null,
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { true, "10:58:41 PM", "10:58:41 PM", false }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 4'd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.DateRange,
                        QueryParams = new object[] { true, "10:58:41 PM", "10:58:41 PM", "string" }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "DateRange Search: QueryParams 4'st param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.DateRange,
                        FieldColumn = "Date",
                        FieldId = "1",
                        QueryParams = new object[] { true, "10:58:41 PM", "10:58:41 PM", null }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: FieldColumn is empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "",
                        SearchType = ArticleFieldSearchType.TimeRange,
                        QueryParams = new object[] { true, "10:58:41 PM", "10:58:41 PM", false }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=true, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "time",
                        FieldId = "1",
                        QueryParams = new object[] { true, null, null, false }
                    }
                },
                ExpectedValue = "(c.[time] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=true, Value1='', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { true, "", "", true }
                    }
                },
                ExpectedValue = "(c.[time] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=true, Value1='10:58:41 PM', Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { true, "10:58:41 PM", null, false }
                    }
                },
                ExpectedValue = "(c.[time] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=true, Value1='10:58:41 PM', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { true, "10:58:41 PM", "", false }
                    }
                },
                ExpectedValue = "(c.[time] IS NULL)"
            },

            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=true, Value1=null, Value2='10:58:41 PM'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { true, null, "10:58:41 PM", false }
                    }
                },
                ExpectedValue = "(c.[time] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=true, Value1='', Value2='10:58:41 PM'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { true, "", "10:58:41 PM", false }
                    }
                },
                ExpectedValue = "(c.[time] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=true, Value1='10:58:41 PM', Value2='10:58:41 PM'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { true, "10:58:41 PM", "10:58:41 PM", false }
                    }
                },
                ExpectedValue = "(c.[time] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "time",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "", false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='10:58:41 PM', Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "10:58:41 PM", null, false }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) >= 82721)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='10:58:41 PM', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "10:58:41 PM", "", false }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) >= 82721)"
            },

            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1=null, Value2='10:58:41 PM'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, "10:58:41 PM", false }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) <= 82721)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='', Value2='10:58:41 PM'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "10:58:41 PM", false }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) <= 82721)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='10:58:41 PM', Value2='10:58:41 PM' aaa",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "10:58:41 PM", "10:58:41 PM", false }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) = 82721)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='10:58:41 AM', Value2='11:58:41 PM'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "10:58:41 AM", "10:58:41 PM", false }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) BETWEEN 39521 AND 82721)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='10:58:41 PM', Value2='11:58:41 AM'",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "10:58:41 PM", "10:58:41 AM", false }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) BETWEEN 39521 AND 82721)"
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "time",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, true }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='', Value2=''",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "", "", false, true }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='10:58:41 PM', Value2=null fff",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "10:58:41 PM", null, true }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) = 82721)"
            },

            new FilterParserTestData
            {
                Description = "TimeRange Search: IsNull=false, Value1='10:58:41 PM', Value2='10:58:41 PM' ddd",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.TimeRange,
                        FieldColumn = "Time",
                        FieldId = "1",
                        QueryParams = new object[] { false, "10:58:41 PM", "10:58:41 AM", true }
                    }
                },
                ExpectedValue = "(dbo.qp_abs_time_seconds(c.[time]) = 82721)"
            }
        };

        private static IEnumerable<FilterParserTestData> CreateNumericRangeTestData() => new[]
        {
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams = null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = null
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams is Empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 1'st param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { null, 0, 1, false, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 1'st param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { "string", 0, 1, false, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 2'nd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { true, "string", 1, false, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 3'd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { true, 0, "string", false, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 2'nd; 3'd and 4th param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { true }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 3'd and 4th param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { true, 0 }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 4th param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { true, 0, 1 }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 4'th param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { false, 0, 1, null, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: QueryParams 4'th param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { false, 0, 1, "string", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: FieldColumn is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = null,
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { true, 0, 1 }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: FieldColumn is empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "",
                        SearchType = ArticleFieldSearchType.NumericRange,
                        QueryParams = new object[] { true, 0, 1 }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=true, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { true, null, null, false, false }
                    }
                },
                ExpectedValue = "(c.[number] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=true, Value1=1, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { true, 1, null, true, false }
                    }
                },
                ExpectedValue = "(c.[number] IS NULL)"
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=true, Value1=null, Value2=1",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { true, null, 1, true, false }
                    }
                },
                ExpectedValue = "(c.[number] IS NULL)"
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=true, Value1=0, Value2=1",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { true, 0, 1, false, false }
                    }
                },
                ExpectedValue = "(c.[number] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, false, false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=1, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, 1, null, false, false }
                    }
                },
                ExpectedValue = "(c.[number] >= 1)"
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=null, Value2=1",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, 1, false, false }
                    }
                },
                ExpectedValue = "(c.[number] <= 1)"
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=1, Value2=1",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, 1, 1, false, false }
                    }
                },
                ExpectedValue = "(c.[number] = 1)"
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=0, Value2=1",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, 0, 1, false, false }
                    }
                },
                ExpectedValue = "(c.[number] BETWEEN 0 AND 1)"
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=1, Value2=0",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, 1, 0, false, false }
                    }
                },
                ExpectedValue = "(c.[number] BETWEEN 0 AND 1)"
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=null, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, null, null, true, false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=1, Value2=null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, 1, null, true, false }
                    }
                },
                ExpectedValue = "(c.[number] = 1)"
            },

            new FilterParserTestData
            {
                Description = "NumericRange Search: IsNull=false, Value1=1, Value2=1",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        SearchType = ArticleFieldSearchType.NumericRange,
                        FieldColumn = "Number",
                        FieldId = "1",
                        QueryParams = new object[] { false, 1, 2, true, false }
                    }
                },
                ExpectedValue = "(c.[number] = 1)"
            }
        };

        private static IEnumerable<FilterParserTestData> CreateBooleanTestData() => new[]
        {
            new FilterParserTestData
            {
                Description = "Boolean Search: QueryParams = null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = null
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: QueryParams is Empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: QueryParams 1'st param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { null, true }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: QueryParams 1'st param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { "string", true }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: QueryParams 2'd param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { true }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: QueryParams 2'nd param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { true, null }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: QueryParams 2'nd param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { true, "string" }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },

            new FilterParserTestData
            {
                Description = "Boolean Search: FieldColumn is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = null,
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { true, true }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: FieldColumn is empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { true, true }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: 1'st param is true 2'nd param is false",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { true, false }
                    }
                },
                ExpectedValue = "(c.[testcolumn] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: 1'st param is true 2'nd param is false",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { true, true }
                    }
                },
                ExpectedValue = "(c.[testcolumn] IS NULL)"
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: 1'st param is false 2'nd param is false",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { false, false }
                    }
                },
                ExpectedValue = "(c.[testcolumn] = 0)"
            },
            new FilterParserTestData
            {
                Description = "Boolean Search: 1'st param is false 2'nd param is true",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.Boolean,
                        QueryParams = new object[] { false, true }
                    }
                },
                ExpectedValue = "(c.[testcolumn] = 1)"
            }
        };

        private static IEnumerable<FilterParserTestData> CreateO2MRelationTestData() => new[]
        {
            new FilterParserTestData
            {
                Description = "O2MRelation Search: QueryParams = null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = null
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "O2MRelation Search: QueryParams is Empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "O2MRelation Search: QueryParams 1'st param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { "string", false, false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },

            new FilterParserTestData
            {
                Description = "O2MRelation Search: QueryParams 2'st param has incorect type",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1, 2 }, "true", false }
                    }
                },
                ExpectedExceptionType = typeof(InvalidCastException)
            },
            new FilterParserTestData
            {
                Description = "O2MRelation Search: QueryParams 2'st param are absent",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1, 2 } }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },

            new FilterParserTestData
            {
                Description = "O2MRelation Search: QueryParams 1'st param array containes incorect type items",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1, "string" }, false, false }
                    }
                },
                ExpectedExceptionType = typeof(FormatException)
            },

            new FilterParserTestData
            {
                Description = "O2MRelation Search: FieldColumn is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = null,
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1, 2 }, false, false }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "O2MRelation Search: FieldColumn is empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1, 2 }, false, false }
                    }
                },
                ExpectedExceptionType = typeof(ArgumentException)
            },
            new FilterParserTestData
            {
                Description = "O2MRelation Search: 1'st param is null",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { null, false, false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "O2MRelation Search: 1'st param is empty",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { }, false, false }
                    }
                },
                ExpectedValue = null
            },
            new FilterParserTestData
            {
                Description = "O2MRelation Search: 1'st param is correct",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1 }, false, false }
                    }
                },
                ExpectedValue = "(c.[testcolumn] = @field)"
            },
            new FilterParserTestData
            {
                Description = "O2MRelation Search: 1'st param is correct",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1, 2, 3 }, false, false }
                    }
                },
                ExpectedValue = "(c.[testcolumn] IN (select id from @field i))"
            },

            new FilterParserTestData
            {
                Description = "O2MRelation Search: 1'nd is null; 2'st param is True (IsNull == True)",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { null, true, false }
                    }
                },
                ExpectedValue = "(c.[testcolumn] IS  NULL)"
            },

            new FilterParserTestData
            {
                Description = "O2MRelation Search: 1'nd is empty; 2'st param is True (IsNull == True)",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { }, true, false }
                    }
                },
                ExpectedValue = "(c.[testcolumn] IS  NULL)"
            },

            new FilterParserTestData
            {
                Description = "O2MRelation Search: 2'st param is True (IsNull == True)",
                SearchQueryParams = new[]
                {
                    new ArticleSearchQueryParam
                    {
                        FieldColumn = "TestColumn",
                        SearchType = ArticleFieldSearchType.O2MRelation,
                        QueryParams = new object[] { new object[] { 1, 2, 3 }, true, false }
                    }
                },
                ExpectedValue = "(c.[testcolumn] IS  NULL)"
            }
        };

        [Test]
        public void GetFilter_CommonTest()
        {
            ProcessGetFilterTest(CreateCommonFilterParseTestData());
        }

        [Test]
        public void GetFilter_AllFilterSearchTypes_Test()
        {
            ProcessGetFilterTest(CreateAllFilterSearchTypeTestData());
        }

        [Test]
        public void GetFilter_TextSearch_Test()
        {
            ProcessGetFilterTest(CreateTextSearchTestData());
        }

        [Test]
        public void GetFilter_DateRange_Test()
        {
            ProcessGetFilterTest(CreateDateRangeTestData());
        }

        [Test]
        public void GetFilter_TimeRange_Test()
        {
            ProcessGetFilterTest(CreateTimeRangeTestData());
        }

        [Test]
        public void GetFilter_NumericRange_Test()
        {
            ProcessGetFilterTest(CreateNumericRangeTestData());
        }

        [Test]
        public void GetFilter_O2MRelation_Test()
        {
            ProcessGetFilterTest(CreateO2MRelationTestData());
        }

        [Test]
        public void GetFilter_Boolean_Test()
        {
            ProcessGetFilterTest(CreateBooleanTestData());
        }

        private static void ProcessGetFilterTest(IEnumerable<FilterParserTestData> testDataCollection)
        {
            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            foreach (var testData in testDataCollection)
            {
                try
                {
                    var sqlParams = new List<DbParameter>();
                    var actual = new ArticleFilterSearchQueryParser().GetFilter(testData.SearchQueryParams?.ToList(), sqlParams);
                    if (testData.ExpectedExceptionType != null)
                    {
                        Assert.Fail($"\"{testData.Description}\" test is failed. No thrown exception.");
                    }

                    Assert.AreEqual(testData.ExpectedValue, actual, $"\"{testData.Description}\" test is failed. Expected value: {testData.ExpectedValue}, actual value: {actual} ");
                }
                catch (Exception ex)
                {
                    if (testData.ExpectedExceptionType == null)
                    {
                        try
                        {
                            var sqlParams = new List<DbParameter>();
                            new ArticleFilterSearchQueryParser().GetFilter(testData.SearchQueryParams?.ToList(), sqlParams);
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                        }

                        Assert.Fail($"\"{testData.Description}\" test is failed. Unexpected exception: {ex.GetType()}.");
                    }
                    Assert.IsInstanceOf(testData.ExpectedExceptionType, ex, $"\"{testData.Description}\" test is failed. Unexpected exception: {ex.GetType()}.");
                }
            }

            CultureInfo.CurrentCulture = previousCulture;
        }
    }
}
