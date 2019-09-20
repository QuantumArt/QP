namespace Quantumart.QP8.BLL.Services
{
    public interface IRecreateDynamicImagesService
    {
        MultistepActionSettings SetupAction(int contentId, int fieldId);

        MultistepActionStepResult Step(int step);

        void TearDown();
    }
}
