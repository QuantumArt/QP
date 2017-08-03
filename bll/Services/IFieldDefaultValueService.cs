using Quantumart.QP8.BLL.Services.DTO;

namespace Quantumart.QP8.BLL.Services
{
    public interface IFieldDefaultValueService
    {
        /// <summary>
        /// PreAction
        /// </summary>
        MessageResult PreAction(int fieldId);

        /// <summary>
        /// Setup
        /// </summary>
        MultistepActionSettings SetupAction(int contentId, int fieldId);

        /// <summary>
        /// TearDown
        /// </summary>
        void TearDown();

        /// <summary>
        /// Step
        /// </summary>
        MultistepActionStepResult Step(int step);
    }
}
