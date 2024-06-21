using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL.Services
{
    public interface IRecreateDynamicImagesService
    {
        MultistepActionSettings SetupAction(int contentId, int fieldId);

        MultistepActionStepResult Step(int step, PathHelper pathHelper);

        void TearDown();
    }
}
