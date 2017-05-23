namespace Quantumart.QP8.DAL
{
    public partial class ContentFolderDAL
    {
        public ContentFolderDAL()
        {
            HasChildren = false;
        }

        public bool HasChildren { get; set; }
    }
}
