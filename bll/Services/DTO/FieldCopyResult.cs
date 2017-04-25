namespace Quantumart.QP8.BLL.Services.DTO
{
	public class FieldCopyResult : CopyResult
	{
		public int? LinkId { get; set; }

		public int[] VirtualFieldIds { get; set; }
 
		public int[] ChildLinkIds { get; set; }

		public int[] ChildFieldIds { get; set; }
	}
}
