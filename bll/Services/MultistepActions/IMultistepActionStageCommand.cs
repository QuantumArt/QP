namespace Quantumart.QP8.BLL.Services.MultistepActions
{
    /// <summary>
    /// Команда этапа удаление данных
    /// </summary>
    public interface IMultistepActionStageCommand
	{
		/// <summary>
		/// Выполняет команду для шага
		/// </summary>
		MultistepActionStepResult Step(int step);
	}
}
