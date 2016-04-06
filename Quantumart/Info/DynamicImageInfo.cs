namespace Quantumart.QPublishing.Info
{
    public class DynamicImageInfo
    {
         
        public string ContentLibraryPath { get; set; }
        
        public string ImagePath { get; set; }
        
        public int AttrId { get; set; }

        public short Width { get; set; }

        public short Height { get; set; }

        public short Quality { get; set; }
        
        public string FileType { get; set; }
        
        public bool MaxSize { get; set; } 
        
        public string ImageName { get; set; } 
        
        public string DynamicUrl { get; set; }

        public int ArticleId { get; set; } 
    }
}
