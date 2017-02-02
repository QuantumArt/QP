using System;

namespace Quantumart.QP8.BLL.Services.MultistepActions
{
    /// <summary>
    /// Контекст выполнения
    /// </summary>
    [Serializable]
    public class MultistepActionServiceContext
    {
        public MultistepActionStageCommandState[] CommandStates { get; set; }
    }

    /// <summary>
    /// Состояние команды
    /// (хранится в сессии)
    /// </summary>
    [Serializable]
    public class MultistepActionStageCommandState
    {
        public int Type { get; set; }

        public int ParentId { get; set; }

        public int Id { get; set; }

        public int[] Ids { get; set; }

        public bool? BoundToExternal { get; set; }

        public int ItemsPerStep { get; set; }
    }
}
