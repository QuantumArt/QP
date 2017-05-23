using System.Diagnostics.CodeAnalysis;

namespace Quantumart.QP8.DAL
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class SiteFolderDAL
    {
        public SiteFolderDAL()
        {
            HasChildren = false;
        }

        public bool HasChildren { get; set; }
    }
}
