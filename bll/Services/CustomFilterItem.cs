using Quantumart.QP8.DAL.DTO;

namespace Quantumart.QP8.BLL.Services
{
    public class CustomFilterItem
    {
        public string Filter { get; set; }
        public string Field { get; set; }
        public bool AllowNull { get; set; }
        public object Value { get; set; }

        public static CustomFilterItem GetArchiveFilter(int value)
        {
            var filter = CustomFilter.GetArchiveFilter(value);

            return new CustomFilterItem
            {
                Filter = filter.Filter,
                Field = filter.Field,
                Value = filter.Value
            };
        }
    }
}
