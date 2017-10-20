using System.Collections.Generic;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Данные для выполнения многошагового действия
	/// </summary>
	public class MultistepActionSettings
	{
		/// <summary>
		/// Этапы
		/// </summary>
		public IEnumerable<MultistepStageSettings> Stages { get; set; }

        public int ParentId { get; set; }
	}

	/// <summary>
	/// Данные для выполнения этапа многошагового действия
	/// </summary>
	public class MultistepStageSettings
	{
		/// <summary>
		/// Название этапа
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Количество шагов
		/// </summary>
		public int StepCount { get; set; }
		/// <summary>
		/// Общее количество элементов
		/// </summary>
		public int ItemCount { get; set; }
	}
}
