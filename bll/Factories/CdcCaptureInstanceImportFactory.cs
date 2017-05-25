﻿using System;
using Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Factories
{
    public class CdcCaptureInstanceImportFactory
    {
        public static CdcImportProcessor Create(string captureInstanceName)
        {
            switch (captureInstanceName)
            {
                case CdcCaptureConstants.StatusType:
                    return new CdcStatusTypeProcessor(CdcCaptureConstants.StatusType);
                case CdcCaptureConstants.ContentAttribute:
                    return new CdcContentAttributeProcessor(CdcCaptureConstants.ContentAttribute);
                case CdcCaptureConstants.ContentItem:
                    return new CdcContentItemProcessor(CdcCaptureConstants.ContentItem);
                case CdcCaptureConstants.ContentData:
                    return new CdcContentDataProcessor(CdcCaptureConstants.ContentData);
                case CdcCaptureConstants.Content:
                    return new CdcContentProcessor(CdcCaptureConstants.Content);
                case CdcCaptureConstants.ContentToContent:
                    return new CdcContentToContentProcessor(CdcCaptureConstants.ContentToContent);
                case CdcCaptureConstants.ContentToContentRev:
                    return new CdcContentToContentRevProcessor(CdcCaptureConstants.ContentToContent);
                case CdcCaptureConstants.ContentToContentAsync:
                    return new CdcContentToContentAsyncProcessor(CdcCaptureConstants.ContentToContent);
                case CdcCaptureConstants.ContentToContentAsyncRev:
                    return new CdcContentToContentAsyncRevProcessor(CdcCaptureConstants.ContentToContent);
                case CdcCaptureConstants.ItemToItem:
                    return new CdcItemToItemProcessor(CdcCaptureConstants.ItemToItem);
                case CdcCaptureConstants.ItemLinkAsync:
                    return new CdcItemLinkAsyncProcessor(CdcCaptureConstants.ItemLinkAsync);
                default:
                    throw new Exception("Undefined processor requested");
            }
        }
    }
}
