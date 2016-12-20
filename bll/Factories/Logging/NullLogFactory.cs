namespace Quantumart.QP8.BLL.Factories.Logging
{
    /// <summary>
    /// Creates a System.Diagnostics logger wrapper, that doesn't log anything, but useful for settings
    /// </summary>
    public class NullLogFactory : DiagnosticsLogFactory
    {
        public NullLogFactory()
            : base(false, false, false, false)
        {
        }
    }
}
