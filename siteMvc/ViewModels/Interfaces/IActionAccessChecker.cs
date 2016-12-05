namespace Quantumart.QP8.WebMvc.ViewModels.Interfaces
{
    public interface IActionAccessChecker
    {
        bool IsActionAccessible(string actionCode);
    }
}
