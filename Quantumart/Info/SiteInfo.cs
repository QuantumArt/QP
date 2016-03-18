using System;

namespace Quantumart.QPublishing.Info
{
    public class SiteInfo
    {
        public SiteInfo(Int32 id, string url)
        {
            Id = id;
            Url = url;
        }
        
        public Int32 Id { get; }

        public string Url { get; }
    }
    
}
