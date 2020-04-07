using System.Collections.Generic;

namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Данные для выполнения многошагового действия
    /// </summary>
    public class MultistepActionSettings
    {
        public MultistepActionSettings()
        {
            Stages = new List<MultistepStageSettings>();
        }
        /// <summary>
        /// Этапы
        /// </summary>
        public List<MultistepStageSettings> Stages { get; set; }

        public int ParentId { get; set; }
    }
}
