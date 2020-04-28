using Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors;

namespace Quantumart.QP8.BLL.Factories
{
    public interface ICdcImportFactory
    {
        ICdcImportProcessor Create(string captureInstanceName);
    }
}
