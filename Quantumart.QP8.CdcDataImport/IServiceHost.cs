using Topshelf;

namespace Quantumart.QP8.CdcDataImport
{
    public interface IServiceHost
    {
        bool Start(HostControl hostControl);

        bool Stop();

        bool Pause();

        bool Continue();
    }
}
