using System;
using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure.Logging.Factories;
using QP8.Services.Tests.Infrastructure.Helpers;
using Quantumart.QP8.ArticleScheduler.Recurring;
using Xunit;

namespace QP8.Services.Tests.ArticleSchedulerTests.Recurring
{
    public class DateRangeExtensionsTests
    {
        public DateRangeExtensionsTests()
        {
            LogProvider.LogFactory = new NullLogFactory();
        }

        [Fact, Trait("DateRangeExtensions", "EveryFullMonth")]
        public void GivenSmallDatesRange_WhenGettingEveryFullMonthLimitedByFactor_ShouldReturnCorrectDateRange()
        {
            // Fixture setup
            const string rawDateStart = "05/07/2011 05:04:02";
            const string rawDateEnd = "05/27/2011 15:14:12";
            var rangeTuple = DateTimeHelpers.GetRangeTuple(rawDateStart, rawDateEnd);
            var expectedResult = new List<Tuple<DateTime, DateTime>>
            {
                DateTimeHelpers.GetRangeTuple("05/01/2011", "05/31/2011")
            };

            // Exercise system
            var actualResult = rangeTuple.GetEveryFullMonthLimitedByFactor(4).ToList();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "EveryFullMonth")]
        public void GivenLargeDatesRange_WhenGettingEveryFullMonthLimitedByFactor_ShouldReturnCorrectDateRanges()
        {
            // Fixture setup
            const string rawDateStart = "05/07/2011 05:04:02";
            const string rawDateEnd = "12/27/2011 15:14:12";
            var rangeTuple = DateTimeHelpers.GetRangeTuple(rawDateStart, rawDateEnd);
            var expectedResult = new List<Tuple<DateTime, DateTime>>
            {
                DateTimeHelpers.GetRangeTuple("05/01/2011", "05/31/2011"),
                DateTimeHelpers.GetRangeTuple("09/01/2011", "09/30/2011")
            };

            // Exercise system
            var actualResult = rangeTuple.GetEveryFullMonthLimitedByFactor(4).ToList();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "EveryFullWeek")]
        public void GivenDatesRange_WhenGettingEveryFullWeekLimitedByFactorFour_ShouldReturnCorrectDateRange()
        {
            // Fixture setup
            const string rawDateStart = "06/22/2011 05:04:02";
            const string rawDateEnd = "06/23/2011 05:04:02";
            var rangeTuple = DateTimeHelpers.GetRangeTuple(rawDateStart, rawDateEnd);
            var expectedResult = new List<Tuple<DateTime, DateTime>>
            {
                DateTimeHelpers.GetRangeTuple("06/20/2011", "06/26/2011")
            };

            // Exercise system
            var actualResult = rangeTuple.GetEveryFullWeekLimitedByFactor(4).ToList();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "EveryFullWeek")]
        public void GivenDatesRange_WhenGettingEveryFullWeekLimitedByFactorThree_ShouldReturnCorrectDateRanges()
        {
            // Fixture setup
            const string rawDateStart = "06/01/2011 05:04:02";
            const string rawDateEnd = "07/01/2011 05:04:02";
            var rangeTuple = DateTimeHelpers.GetRangeTuple(rawDateStart, rawDateEnd);
            var expectedResult = new List<Tuple<DateTime, DateTime>>
            {
                DateTimeHelpers.GetRangeTuple("05/30/2011", "06/05/2011"),
                DateTimeHelpers.GetRangeTuple("06/20/2011", "06/26/2011")
            };

            // Exercise system
            var actualResult = rangeTuple.GetEveryFullWeekLimitedByFactor(3).ToList();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "EveryFullDay")]
        public void GivenDatesRange_WhenGettingAllDaysFromRange_ShouldReturnCorrectDaysCount()
        {
            // Fixture setup
            var datesRangeTupleList = new List<Tuple<DateTime, DateTime>>
            {
                DateTimeHelpers.GetRangeTuple("05/01/2011", "05/31/2011"),
                DateTimeHelpers.GetRangeTuple("09/01/2011", "09/30/2011")
            };

            const int expectedResult = 61;

            // Exercise system
            var actualResult = datesRangeTupleList.GetAllDaysFromRange().Distinct().Count();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "EveryNDay")]
        public void GivenDatesRange_WhenGettingEveryNDayFromRange_ShouldReturnCorrectDates()
        {
            // Fixture setup
            const string rawDateStart = "06/01/2011 05:04:02";
            const string rawDateEnd = "06/10/2011 05:04:02";
            var rangeTuple = DateTimeHelpers.GetRangeTuple(rawDateStart, rawDateEnd);
            var expectedResult = new List<DateTime>
            {
                DateTimeHelpers.ParseDateTime("06/01/2011"),
                DateTimeHelpers.ParseDateTime("06/06/2011")
            };

            // Exercise system
            var actualResult = rangeTuple.GetEveryNDayFromRange(5).ToList();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "EveryNDayGrouppedByMonth")]
        public void GivenDatesList_WhenGettingNDaysGroupedByMonth_ShouldReturnCorrectDates()
        {
            // Fixture setup
            var datesList = new List<DateTime>
            {
                DateTimeHelpers.ParseDateTime("05/01/2011"),
                DateTimeHelpers.ParseDateTime("05/15/2011"),
                DateTimeHelpers.ParseDateTime("05/10/2011"),
                DateTimeHelpers.ParseDateTime("07/09/2011"),
                DateTimeHelpers.ParseDateTime("07/11/2011"),
                DateTimeHelpers.ParseDateTime("07/01/2011")
            };

            var expectedResult = new List<DateTime>
            {
                DateTimeHelpers.ParseDateTime("05/10/2011"),
                DateTimeHelpers.ParseDateTime("07/09/2011")
            };

            // Exercise system
            var actualResult = datesList.GetEveryNDayGroupedByMonth(2).ToList();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "EveryLastDayGrouppedByMonth")]
        public void GivenDatesList_WhenGettingLastDaysGroupedByMonth_ShouldReturnCorrectDates()
        {
            // Fixture setup
            var datesList = new List<DateTime>
            {
                DateTimeHelpers.ParseDateTime("05/01/2011"),
                DateTimeHelpers.ParseDateTime("05/15/2011"),
                DateTimeHelpers.ParseDateTime("05/10/2011"),
                DateTimeHelpers.ParseDateTime("07/09/2011"),
                DateTimeHelpers.ParseDateTime("07/11/2011"),
                DateTimeHelpers.ParseDateTime("07/01/2011")
            };

            var expectedResult = new List<DateTime>
            {
                DateTimeHelpers.ParseDateTime("05/15/2011"),
                DateTimeHelpers.ParseDateTime("07/11/2011")
            };

            // Exercise system
            var actualResult = datesList.GetEveryLastDayGroupedByMonth().ToList();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory, Trait("DateRangeExtensions", "NearestPreviousDate")]
        [InlineData("05/14/2011", "05/10/2011")]
        [InlineData("05/15/2011", "05/15/2011")]
        public void GivenDatesList_WhenGettingNearestPreviousDateFromList_ShouldReturnCorrectDate(string rawCurrentDate, string rawDateResult)
        {
            // Fixture setup
            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var datesList = new List<DateTime>
            {
                DateTimeHelpers.ParseDateTime("05/01/2011"),
                DateTimeHelpers.ParseDateTime("05/15/2011"),
                DateTimeHelpers.ParseDateTime("05/10/2011")
            };

            var expectedResult = DateTimeHelpers.ParseDateTime(rawDateResult);

            // Exercise system
            var actualResult = datesList.GetNearestPreviousDateFromList(currentDate);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "NearestPreviousDate")]
        public void GivenOutOfRangeDatesList_WhenGettingNearestPreviousDateFromList_ShouldReturnNull()
        {
            // Fixture setup
            var currentDate = DateTimeHelpers.ParseDateTime("04/30/2011");
            var datesList = new List<DateTime>
            {
                DateTimeHelpers.ParseDateTime("05/01/2011"),
                DateTimeHelpers.ParseDateTime("05/15/2011"),
                DateTimeHelpers.ParseDateTime("05/10/2011")
            };

            // Exercise system
            var actualResult = datesList.GetNearestPreviousDateFromList(currentDate);

            // Verify outcome
            Assert.Null(actualResult);
        }

        [Fact, Trait("DateRangeExtensions", "NearestPreviousDate")]
        public void GivenEmptyDatesList_WhenGettingNearestPreviousDateFromList_ShouldReturnNull()
        {
            // Fixture setup
            var currentDate = DateTimeHelpers.ParseDateTime("05/15/2011");
            var emptyDateTimeList = Enumerable.Empty<DateTime>();

            // Exercise system
            var actualResult = emptyDateTimeList.GetNearestPreviousDateFromList(currentDate);

            // Verify outcome
            Assert.Null(actualResult);
        }

        [InlineData("09/27/1917", "09/24/1917")]
        [InlineData("02/29/2020", "02/24/2020")]
        [InlineData("04/22/2117", "04/19/2117")]
        [Theory, Trait("DateRangeExtensions", "WeekStartDate")]
        public void GivenDatesList_WhenGettingWeekStartDate_ShouldReturnCorrectDate(string rawCurrentDate, string rawDateResult)
        {
            // Fixture setup
            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var expectedResult = DateTimeHelpers.ParseDateTime(rawDateResult);

            // Exercise system
            var actualResult = currentDate.GetWeekStartDate();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [InlineData("09/27/1917", "09/01/1917")]
        [InlineData("02/24/2020", "02/01/2020")]
        [InlineData("04/22/2117", "04/01/2117")]
        [Theory, Trait("DateRangeExtensions", "MonthStartDate")]
        public void GivenDatesList_WhenGettingMonthStartDate_ShouldReturnCorrectDate(string rawCurrentDate, string rawDateResult)
        {
            // Fixture setup
            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var expectedResult = DateTimeHelpers.ParseDateTime(rawDateResult);

            // Exercise system
            var actualResult = currentDate.GetMonthStartDate();

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
