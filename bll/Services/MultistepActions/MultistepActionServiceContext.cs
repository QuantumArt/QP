using System;
using System.Collections.Generic;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.MultistepActions
{
    /// <summary>
    /// Контекст выполнения
    /// </summary>
    [Serializable]
    public class MultistepActionServiceContext
    {
        public MultistepActionServiceContext()
        {
            CommandStates = new List<MultistepActionStageCommandState>();
        }
        public List<MultistepActionStageCommandState> CommandStates { get; set; }
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

        public List<int> Ids { get; set; }

        public List<int> ExtensionContentIds { get; set; }

        public bool? BoundToExternal { get; set; }

        public int ItemsPerStep { get; set; }

        public S3Options S3Options { get; set; }
    }
}
