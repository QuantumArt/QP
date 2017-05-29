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
                _cdcImportService.ImportData(CdcCaptureConstants.ContentAttribute, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentToContent, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentToContentRev, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentToContentAsync, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.ContentToContentAsyncRev, fromLsn, toLsn),
                _cdcImportService.ImportData(CdcCaptureConstants.StatusType, fromLsn, toLsn),
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

            var contentItemsFilteredByNetChanges = contentItemModel.GetCdcDataFilteredByLsnNetChanges(cim => new
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

            foreach (var cim in contentItemsFilteredByNetChanges)
            {
                var fieldColumns = contentDataFilteredByNetChanges
                    .Where(cdc => cdc.TransactionLsn == cim.TransactionLsn)
                    .Where(cdm => Equals(
                        cdm.Entity.MetaData[TarantoolContentArticleModel.IsSplitted],
                        cim.Entity.MetaData[TarantoolContentArticleModel.IsSplitted]))
                    .Where(cdm => Equals(
                        cdm.Entity.Columns[TarantoolContentArticleModel.ContentItemId],
                        cim.Entity.Columns[TarantoolContentArticleModel.ContentItemId]))
                    .OrderBy(cdm => cdm.SequenceLsn)
                    .Select(cdm => new KeyValuePair<string, object>(
                        TarantoolContentArticleModel.GetFieldName(cdm.Entity.Columns[TarantoolContentArticleModel.AttributeId]),
                        cdm.Entity.Columns[TarantoolContentArticleModel.Data]));

                cim.Entity.Columns.AddRange(fieldColumns);
                yield return cim;
            }
        }
    }
}
