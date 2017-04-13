using QP8.Services.Tests.Infrastructure.Helpers;
using Quantumart.QP8.ArticleScheduler.Recurring.RecurringCalculators;
using Xunit;

namespace QP8.Services.Tests.ArticleSchedulerTests.Recurring
{
    public class RecurringStartCalculatorTests
    {
        [InlineData("12/30/2010 10:11:12")]
        [InlineData("01/05/2011 10:10:11")]
        [Theory, Trait("RecurringCalculator", "MonthlyRelativeStartCalculator")]
        public void GivenMonthlyRelativeStartCalculator_WhenDatesOutOfScheduleRange_ShouldNotReturnAnything(string rawCurrentDate)
        {
            // Fixture setup
            const int interval = 9;
            const int relativeInterval = 4;
            const int recurrenceFactor = 5;
            const string rawStartDate = "01/05/2011";
            const string rawEndDate = "12/31/2011";
            const string rawStartTime = "12:15:17";

            var startCalculator = new MonthlyRelativeStartCalculator(
                interval,
                relativeInterval,
                recurrenceFactor,
                DateTimeHelpers.ParseDateTime(rawStartDate),
                DateTimeHelpers.ParseDateTime(rawEndDate),
                DateTimeHelpers.ParseTime(rawStartTime)
            );

            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);

            // Exercise system
            var actualResult = startCalculator.GetNearesStartDateBeforeSpecifiedDate(currentDate);

            // Verify outcome
            Assert.Null(actualResult);
        }

        [InlineData("01/05/2011 12:15:17", "01/05/2011 12:15:17")]
        [InlineData("01/05/2011 13:15:17", "01/05/2011 12:15:17")]
        [InlineData("04/15/2011 13:15:17", "01/05/2011 12:15:17")]
        [InlineData("06/03/2011 10:15:17", "01/05/2011 12:15:17")]
        [InlineData("06/03/2011 12:15:17", "06/03/2011 12:15:17")]
        [InlineData("06/03/2011 13:15:17", "06/03/2011 12:15:17")]
        [InlineData("09/11/2011", "06/03/2011 12:15:17")]
        [InlineData("12/05/2011", "11/03/2011 12:15:17")]
        [InlineData("12/31/2011 12:15:17", "11/03/2011 12:15:17")]
        [InlineData("12/31/2011 14:15:17", "11/03/2011 12:15:17")]
        [InlineData("03/10/2012", "11/03/2011 12:15:17")]
        [Theory, Trait("RecurringCalculator", "MonthlyRelativeStartCalculator")]
        public void GivenMonthlyRelativeStartCalculator_WhenDatesWithinScheduleRange_ShouldReturnCorrectDate(string rawCurrentDate, string rawExpectedStartDate)
        {
            // Fixture setup
            const int interval = 9;
            const int relativeInterval = 4;
            const int recurrenceFactor = 5;
            const string rawStartDate = "01/05/2011";
            const string rawEndDate = "12/31/2011";
            const string rawStartTime = "12:15:17";

            var startCalculator = new MonthlyRelativeStartCalculator(
                interval,
                relativeInterval,
                recurrenceFactor,
                DateTimeHelpers.ParseDateTime(rawStartDate),
                DateTimeHelpers.ParseDateTime(rawEndDate),
                DateTimeHelpers.ParseTime(rawStartTime)
            );

            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var expectedResult = DateTimeHelpers.ParseDateTime(rawExpectedStartDate);

            // Exercise system
            var actualResult = startCalculator.GetNearesStartDateBeforeSpecifiedDate(currentDate);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [InlineData("03/15/2011 12:15:17")]
        [InlineData("04/10/2011 12:15:17")]
        [Theory, Trait("RecurringCalculator", "MonthlyStartCalculator")]
        public void GivenMonthlyStartCalculator_WhenDatesOutOfScheduleRange_ShouldNotReturnAnything(string rawCurrentDate)
        {
            // Fixture setup
            const int interval = 15;
            const int recurrenceFactor = 3;
            const string rawStartDate = "03/16/2011";
            const string rawEndDate = "11/24/2011";
            const string rawStartTime = "12:15:17";

            var startCalculator = new MonthlyStartCalculator(
                interval,
                recurrenceFactor,
                DateTimeHelpers.ParseDateTime(rawStartDate),
                DateTimeHelpers.ParseDateTime(rawEndDate),
                DateTimeHelpers.ParseTime(rawStartTime)
            );

            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);

            // Exercise system
            var actualResult = startCalculator.GetNearesStartDateBeforeSpecifiedDate(currentDate);

            // Verify outcome
            Assert.Null(actualResult);
        }

        [InlineData("06/15/2011 12:15:17", "06/15/2011 12:15:17")]
        [InlineData("08/10/2011 12:15:17", "06/15/2011 12:15:17")]
        [InlineData("09/15/2011 10:15:17", "06/15/2011 12:15:17")]
        [InlineData("09/15/2011 12:15:17", "09/15/2011 12:15:17")]
        [InlineData("09/15/2011 12:15:18", "09/15/2011 12:15:17")]
        [InlineData("10/10/2011 12:15:17", "09/15/2011 12:15:17")]
        [InlineData("11/24/2011 12:15:17", "09/15/2011 12:15:17")]
        [InlineData("01/15/2012 12:15:17", "09/15/2011 12:15:17")]
        [Theory, Trait("RecurringCalculator", "MonthlyStartCalculator")]
        public void GivenMonthlyStartCalculator_WhenDatesWithinScheduleRange_ShouldReturnCorrectDate(string rawCurrentDate, string rawExpectedStartDate)
        {
            // Fixture setup
            const int interval = 15;
            const int recurrenceFactor = 3;
            const string rawStartDate = "03/16/2011";
            const string rawEndDate = "11/24/2011";
            const string rawStartTime = "12:15:17";

            var startCalculator = new MonthlyStartCalculator(
                interval,
                recurrenceFactor,
                DateTimeHelpers.ParseDateTime(rawStartDate),
                DateTimeHelpers.ParseDateTime(rawEndDate),
                DateTimeHelpers.ParseTime(rawStartTime)
            );

            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var expectedResult = DateTimeHelpers.ParseDateTime(rawExpectedStartDate);

            // Exercise system
            var actualResult = startCalculator.GetNearesStartDateBeforeSpecifiedDate(currentDate);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [InlineData("05/20/2011 12:15:17")]
        [InlineData("06/01/2011 10:15:17")]
        [Theory, Trait("RecurringCalculator", "WeeklyStartCalculator")]
        public void GivenWeeklyStartCalculator_WhenDatesOutOfScheduleRange_ShouldNotReturnAnything(string rawCurrentDate)
        {
            // Fixture setup
            const int interval = 42;
            const int recurrenceFactor = 3;
            const string rawStartDate = "06/01/2011";
            const string rawEndDate = "07/12/2011";
            const string rawStartTime = "12:15:17";

            var startCalculator = new WeeklyStartCalculator(
                interval,
                recurrenceFactor,
                DateTimeHelpers.ParseDateTime(rawStartDate),
                DateTimeHelpers.ParseDateTime(rawEndDate),
                DateTimeHelpers.ParseTime(rawStartTime)
            );

            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);

            // Exercise system
            var actualResult = startCalculator.GetNearesStartDateBeforeSpecifiedDate(currentDate);

            // Verify outcome
            Assert.Null(actualResult);
        }

        [InlineData("06/01/2011 12:15:17", "06/01/2011 12:15:17")]
        [InlineData("06/01/2011 14:15:17", "06/01/2011 12:15:17")]
        [InlineData("06/02/2011 14:15:17", "06/01/2011 12:15:17")]
        [InlineData("06/03/2011 10:15:17", "06/01/2011 12:15:17")]
        [InlineData("06/03/2011 12:15:17", "06/03/2011 12:15:17")]
        [InlineData("06/03/2011 14:15:17", "06/03/2011 12:15:17")]
        [InlineData("06/23/2011 14:15:17", "06/22/2011 12:15:17")]
        [InlineData("07/10/2011 14:15:17", "06/24/2011 12:15:17")]
        [InlineData("07/11/2011 10:15:17", "06/24/2011 12:15:17")]
        [InlineData("07/11/2011 12:15:17", "07/11/2011 12:15:17")]
        [InlineData("07/11/2011 14:15:17", "07/11/2011 12:15:17")]
        [InlineData("07/20/2011 14:15:17", "07/11/2011 12:15:17")]
        [Theory, Trait("RecurringCalculator", "WeeklyStartCalculator")]
        public void GivenWeeklyStartCalculator_WhenDatesWithinScheduleRange_ShouldReturnCorrectDate(string rawCurrentDate, string rawExpectedStartDate)
        {
            // Fixture setup
            const int interval = 42;
            const int recurrenceFactor = 3;
            const string rawStartDate = "06/01/2011";
            const string rawEndDate = "07/12/2011";
            const string rawStartTime = "12:15:17";

            var startCalculator = new WeeklyStartCalculator(
                interval,
                recurrenceFactor,
                DateTimeHelpers.ParseDateTime(rawStartDate),
                DateTimeHelpers.ParseDateTime(rawEndDate),
                DateTimeHelpers.ParseTime(rawStartTime)
            );

            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var expectedResult = DateTimeHelpers.ParseDateTime(rawExpectedStartDate);

            // Exercise system
            var actualResult = startCalculator.GetNearesStartDateBeforeSpecifiedDate(currentDate);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [InlineData("05/30/2011 12:15:17")]
        [InlineData("06/01/2011 10:15:17")]
        [Theory, Trait("RecurringCalculator", "DailyStartCalculator")]
        public void GivenDailyStartCalculator_WhenDatesOutOfScheduleRange_ShouldNotReturnAnything(string rawCurrentDate)
        {
            // Fixture setup
            const int interval = 4;
            const string rawStartDate = "06/01/2011";
            const string rawEndDate = "06/10/2011";
            const string rawStartTime = "12:15:17";

            var startCalculator = new DailyStartCalculator(
                interval,
                DateTimeHelpers.ParseDateTime(rawStartDate),
                DateTimeHelpers.ParseDateTime(rawEndDate),
                DateTimeHelpers.ParseTime(rawStartTime)
            );

            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);

            // Exercise system
            var actualResult = startCalculator.GetNearesStartDateBeforeSpecifiedDate(currentDate);

            // Verify outcome
            Assert.Null(actualResult);
        }

        [InlineData("06/03/2011 12:15:17", "06/01/2011 12:15:17")]
        [InlineData("06/07/2011 14:15:17", "06/05/2011 12:15:17")]
        [InlineData("06/10/2011 14:15:17", "06/09/2011 12:15:17")]
        [InlineData("06/12/2011 10:15:17", "06/09/2011 12:15:17")]
        [Theory, Trait("RecurringCalculator", "DailyStartCalculator")]
        public void GivenDailyStartCalculator_WhenDatesWithinScheduleRange_ShouldReturnCorrectDate(string rawCurrentDate, string rawExpectedStartDate)
        {
            // Fixture setup
            const int interval = 4;
            const string rawStartDate = "06/01/2011";
            const string rawEndDate = "06/10/2011";
            const string rawStartTime = "12:15:17";

            var startCalculator = new DailyStartCalculator(
                interval,
                DateTimeHelpers.ParseDateTime(rawStartDate),
                DateTimeHelpers.ParseDateTime(rawEndDate),
                DateTimeHelpers.ParseTime(rawStartTime)
            );

            var currentDate = DateTimeHelpers.ParseDateTime(rawCurrentDate);
            var expectedResult = DateTimeHelpers.ParseDateTime(rawExpectedStartDate);

            // Exercise system
            var actualResult = startCalculator.GetNearesStartDateBeforeSpecifiedDate(currentDate);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
