namespace Quantumart.QP8.BLL
{
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
