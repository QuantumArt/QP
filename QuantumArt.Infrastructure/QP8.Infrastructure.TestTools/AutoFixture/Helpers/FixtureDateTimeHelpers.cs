using System;
using Ploeh.AutoFixture;

namespace QP8.Infrastructure.TestTools.AutoFixture.Helpers
{
    public static class FixtureDateTimeHelpers
    {
        public static DateTime GenerateDateInPast(IFixture fixture)
        {
            var prevDays = new RandomDateTimeSequenceGenerator(DateTime.Today.AddYears(-30), DateTime.Today.AddYears(-5));
            using (new CustomizationClosure(fixture, prevDays))
            {
                return fixture.Create<DateTime>();
            }
        }

        public static DateTime GenerateDateInFuture(IFixture fixture)
        {
            var nextDays = new RandomDateTimeSequenceGenerator(DateTime.Today.AddYears(5), DateTime.Today.AddYears(30));
            using (new CustomizationClosure(fixture, nextDays))
            {
                return fixture.Create<DateTime>();
            }
        }
    }
}
