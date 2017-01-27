namespace Quantumart.QPublishing.Info
{
    public class SiteInfo
    {
        public SiteInfo(int id, string url)
        {
            Id = id;
            Url = url;
        }

        public int Id { get; }

        public string Url { get; }
    }
}
