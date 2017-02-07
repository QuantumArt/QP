namespace Quantumart.QP8.BLL
{
    public class SimpleListItem
    {
        public SimpleListItem()
        {
        }

        public SimpleListItem(string value, string text)
        {
            Value = value;
            Text = text;
        }

        public string Value { get; set; }

        public string Text { get; set; }
    }
}
