namespace Quantumart.QP8.BLL.Repository.Results
{
	/// <summary>
	/// Данные о виртуальном поле
	/// (используется для построения join-view)
	/// </summary>
	internal class VirtualFieldData
	{
		public int Id { get; set; }

		public string Name {get; set;}

		public int Type { get; set; }

		/// <summary>
		/// Имя базового поля
		/// </summary>
		public string PersistentName { get; set; }
		/// <summary>
		/// Id базового поля
		/// </summary>
		public int PersistentId { get; set; }		
		/// <summary>
		/// Имя контента базового поля
		/// </summary>
		public int PersistentContentId { get; set; }

		/// <summary>
		/// Id виртуального поля - родителя
		/// </summary>
		public int? JoinId { get; set; }
		/// <summary>
		/// Id контента на который ссылается виртуальное поле по O2M
		/// </summary>
		public int? RelateToPersistentContentId { get; set; }

		
	}
}
