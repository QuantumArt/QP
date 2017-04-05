namespace QP8.Infrastructure.Logging.Factories
{
    /// <summary>
    /// Creates a System.Diagnostics logger wrapper, that doesn't log anything, but useful for settings
    /// </summary>
    public class NullLogFactory : DiagnosticsLogFactory
    {
        public NullLogFactory()
            : base(debugEnabled: false, traceEnabled: false, infoEnabled: false, warnEnabled: false, errorEnabled: false, fatalEnabled: false)
        {
        }
    }
}
