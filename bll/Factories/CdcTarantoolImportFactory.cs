using System;
using Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors;
using Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors.Tarantool;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Factories
{
    public class CdcTarantoolImportFactory
    {
        public static ICdcImportProcessor Create(string captureInstanceName)
        {
            switch (captureInstanceName)
            {
                case CdcCaptureConstants.StatusType:
                    return new CdcTarantoolStatusTypeProcessor(CdcCaptureConstants.StatusType);

                case CdcCaptureConstants.ContentAttribute:
                    return new CdcTarantoolContentAttributeProcessor(CdcCaptureConstants.ContentAttribute);

                case CdcCaptureConstants.ContentItem:
                    return new CdcTarantoolContentItemProcessor(CdcCaptureConstants.ContentItem);

                case CdcCaptureConstants.ContentData:
                    return new CdcTarantoolContentDataProcessor(CdcCaptureConstants.ContentData);

                case CdcCaptureConstants.Content:
                    return new CdcTarantoolContentProcessor(CdcCaptureConstants.Content);

                case CdcCaptureConstants.ContentToContent:
                    return new CdcTarantoolContentToContentProcessor(CdcCaptureConstants.ContentToContent);

                case CdcCaptureConstants.VirtualContentAsync:
                    return new CdcTarantoolContentAsyncProcessor(CdcCaptureConstants.Content);

                case CdcCaptureConstants.VirtualContentAttributeAsync:
                    return new CdcTarantoolContentAttributeAsyncProcessor(CdcCaptureConstants.ContentAttribute);

                case CdcCaptureConstants.VirtualContentToContentRev:
                    return new CdcTarantoolContentToContentRevProcessor(CdcCaptureConstants.ContentToContent);

                case CdcCaptureConstants.VirtualContentToContentAsync:
                    return new CdcTarantoolContentToContentAsyncProcessor(CdcCaptureConstants.ContentToContent);

                case CdcCaptureConstants.VirtualContentToContentAsyncRev:
                    return new CdcTarantoolContentToContentAsyncRevProcessor(CdcCaptureConstants.ContentToContent);

                case CdcCaptureConstants.ItemToItem:
                    return new CdcTarantoolItemToItemProcessor(CdcCaptureConstants.ItemToItem);

                case CdcCaptureConstants.ItemLinkAsync:
                    return new CdcTarantoolItemLinkAsyncProcessor(CdcCaptureConstants.ItemLinkAsync);

                default:
                    throw new Exception("Undefined processor requested");
            }
        }
    }
}
