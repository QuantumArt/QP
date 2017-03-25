namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Результат выполнения шага многошагового действия
    /// </summary>
    public class MultistepActionStepResult
    {
        /// <summary>
        /// Количество элементов обработанных на шаге
        /// </summary>
        public int ProcessedItemsCount { get; set; }

        public string AdditionalInfo { get; set; }
    }
}
