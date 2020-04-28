using System;
using Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors;
using Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors.Elastic;
using Quantumart.QP8.Constants.Cdc;

namespace Quantumart.QP8.BLL.Factories
{
    public class ElasticCdcImportFactory : ICdcImportFactory
    {
        public ICdcImportProcessor Create(string captureInstanceName)
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
