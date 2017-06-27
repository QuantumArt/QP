using System;
using Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors;
using Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors.Elastic;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Factories
{
    public class CdcElasticImportFactory
    {
        public static ICdcImportProcessor Create(string captureInstanceName)
        {
            switch (captureInstanceName)
            {
                case CdcCaptureConstants.ContentData:
                    return new CdcElasticContentDataProcessor(CdcCaptureConstants.ContentData);

                default:
                    throw new Exception("Undefined processor requested");
            }
        }
    }
}
