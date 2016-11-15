namespace Quantumart.QP8.BLL
{
    public class ListCommand
    {
        public string SortExpression { get; set; }

        public string FilterExpression { get; set; }

        public int StartPage { get; set; }

        public int PageSize { get; set; }

        public int StartRecord => (StartPage - 1) * PageSize + 1;
    }
}
