using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.NotificationSender;
using Quantumart.QP8.CdcDataImport.Common.Infrastructure.Extensions;
using Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Cdc;

namespace Quantumart.QP8.CdcDataImport.Tarantool.Infrastructure.Services
{
    public class CdcDataImportProcessor : ICdcDataImportProcessor
    {
        private readonly ICdcImportService _cdcImportService;

        public CdcDataImportProcessor(ICdcImportService cdcImportService)
        {
            _cdcImportService = cdcImportService;
        }

        public List<CdcTableTypeModel> GetCdcDataFromTables(string fromLsn = null, string toLsn = null)
        {
            Logger.Log.Trace($"Getting cdc data for with fromLsn:{fromLsn} and toLsn:{toLsn}");
            var tablesGroup = new List<List<CdcTableTypeModel>>
            {
                _cdcImportService.ImportData(CdcCaptureConstants.Content, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.StatusType, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentAttribute, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentToContent, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.VirtualContentAsync, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.VirtualContentAttributeAsync, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.VirtualContentToContentRev, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.VirtualContentToContentAsync, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.VirtualContentToContentAsyncRev, fromLsn, toLsn),
                GetItemToItemDtosFilteredByNetChanges(fromLsn, toLsn).ToList(),
                GetItemLinkAsyncDtosFilteredByNetChanges(fromLsn, toLsn).ToList(),
                GetContentArticleDtoFilteredByNetChanges(fromLsn, toLsn).ToList()
            };

            return tablesGroup.SelectMany(tg => tg).OrderBy(t => t.TransactionLsn).ToList();
        }

        private IEnumerable<CdcTableTypeModel> GetItemToItemDtosFilteredByNetChanges(string fromLsn = null, string toLsn = null)
        {
            var itemToItemModel = _cdcImportService.ImportData(CdcCaptureConstants.ItemToItem, fromLsn, toLsn);
            var itemToItemFilteredByNetChanges = itemToItemModel.GetCdcDataFilteredByLsnNetChanges(iti => new
            {
                id = iti.Entity.Columns[TarantoolItemToItemModel.Id].ToString(),
                linkedId = iti.Entity.Columns[TarantoolItemToItemModel.LinkedId].ToString(),
                linkId = iti.Entity.MetaData[TarantoolItemToItemModel.LinkId].ToString(),
                iti.TransactionLsn
            });

            foreach (var data in itemToItemFilteredByNetChanges)
            {
                var isSelf = (bool)data.Entity.MetaData[TarantoolItemToItemModel.IsSelf];
                if (isSelf)
                {
                    yield return CdcDataModelHelpers.ConvertItemToItemToRev(data);
                }

                yield return data;
            }
        }

        private IEnumerable<CdcTableTypeModel> GetItemLinkAsyncDtosFilteredByNetChanges(string fromLsn = null, string toLsn = null)
        {
            var itemLinkAsyncModel = _cdcImportService.ImportData(CdcCaptureConstants.ItemLinkAsync, fromLsn, toLsn);
            var itemLinkAsyncFilteredByNetChanges = itemLinkAsyncModel.GetCdcDataFilteredByLsnNetChanges(ila => new
            {
                id = ila.Entity.Columns[TarantoolItemLinkAsyncModel.Id].ToString(),
                linkedId = ila.Entity.Columns[TarantoolItemLinkAsyncModel.LinkedId].ToString(),
                linkId = ila.Entity.MetaData[TarantoolItemLinkAsyncModel.LinkId].ToString(),
                ila.TransactionLsn
            });

            foreach (var data in itemLinkAsyncFilteredByNetChanges)
            {
                var isSelf = (bool)data.Entity.MetaData[TarantoolItemLinkAsyncModel.IsSelf];
                if (isSelf)
                {
                    yield return CdcDataModelHelpers.ConvertItemLinkAsyncToRev(data);
                }

                yield return data;
            }
        }

        private IEnumerable<CdcTableTypeModel> GetContentArticleDtoFilteredByNetChanges(string fromLsn = null, string toLsn = null)
        {
            var contentItemModel = _cdcImportService.ImportData(CdcCaptureConstants.ContentItem, fromLsn, toLsn);
            var contentDataModel = _cdcImportService.ImportData(CdcCaptureConstants.ContentData, fromLsn, toLsn);

            var filteredContentItemModel = FilterByIsSplitted(contentItemModel, contentDataModel);
            var contentItemsFilteredByNetChanges = filteredContentItemModel.GetCdcDataFilteredByLsnNetChanges(cim => new
            {
                contentItemId = cim.Entity.Columns[TarantoolContentArticleModel.ContentItemId].ToString(),
                isSplitted = (bool)cim.Entity.MetaData[TarantoolContentArticleModel.IsSplitted],
                cim.TransactionLsn
            });

            var contentDataFilteredByNetChanges = contentDataModel.GetCdcDataFilteredByLsnNetChangesWithColumnsCopy(cdm => new
            {
                attributeId = cdm.Entity.Columns[TarantoolContentArticleModel.AttributeId].ToString(),
                contentItemId = cdm.Entity.Columns[TarantoolContentArticleModel.ContentItemId].ToString(),
                isSplitted = (bool)cdm.Entity.MetaData[TarantoolContentArticleModel.IsSplitted],
                cdm.TransactionLsn
            });

            var contentDataToItemLookup = contentDataFilteredByNetChanges.ToLookup(cdm => new
            {
                cdm.TransactionLsn,
                contentItemId = cdm.Entity.Columns[TarantoolContentArticleModel.ContentItemId],
                isSplitted = cdm.Entity.MetaData[TarantoolContentArticleModel.IsSplitted]
            });

            return contentItemsFilteredByNetChanges.AsParallel().Select(cim =>
            {
                var fieldColumns = contentDataToItemLookup[new
                {
                    cim.TransactionLsn,
                    contentItemId = cim.Entity.Columns[TarantoolContentArticleModel.ContentItemId],
                    isSplitted = cim.Entity.MetaData[TarantoolContentArticleModel.IsSplitted]
                }].OrderBy(cdm => cdm.SequenceLsn).Select(cdm => new KeyValuePair<string, object>(
                    TarantoolContentArticleModel.GetFieldName(cdm.Entity.Columns[TarantoolContentArticleModel.AttributeId]),
                    cdm.Entity.Columns[TarantoolContentArticleModel.Data])
                );

                cim.Entity.Columns.AddRange(fieldColumns);
                return cim;
            });
        }

        private IEnumerable<CdcTableTypeModel> FilterByIsSplitted(List<CdcTableTypeModel> contentItemModel, List<CdcTableTypeModel> contentDataModel)
        {
            if (!contentItemModel.Any())
            {
                yield break;
            }

            for (int i = 1; i < contentItemModel.Count; i++)
            {
                var currCim = contentItemModel[i];
                var prevCim = contentItemModel[i - 1];
                if ((bool)currCim.Entity.MetaData[TarantoolContentArticleModel.IsSplitted])
                {
                    if (Equals(prevCim.TransactionLsn, currCim.TransactionLsn)
                        && Equals(prevCim.Entity.Columns[TarantoolContentArticleModel.ContentItemId], currCim.Entity.Columns[TarantoolContentArticleModel.ContentItemId])
                        && Equals(prevCim.Entity.Columns[TarantoolContentArticleModel.StatusTypeId], currCim.Entity.Columns[TarantoolContentArticleModel.StatusTypeId])
                        && Equals(prevCim.Entity.Columns[TarantoolContentArticleModel.Visible], currCim.Entity.Columns[TarantoolContentArticleModel.Visible])
                        && Equals(prevCim.Entity.Columns[TarantoolContentArticleModel.Archive], currCim.Entity.Columns[TarantoolContentArticleModel.Archive])
                        && Equals(prevCim.Entity.Columns[TarantoolContentArticleModel.LastModifiedBy], currCim.Entity.Columns[TarantoolContentArticleModel.LastModifiedBy])
                        && !(bool)prevCim.Entity.MetaData[TarantoolContentArticleModel.IsSplitted]
                    )
                    {
                        continue;
                    }
                }

                yield return prevCim;
            }

            yield return contentItemModel.Last();
        }
    }
}
