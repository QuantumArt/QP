using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NLog;
using QP8.Infrastructure;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Tarantool;

namespace Quantumart.QP8.BLL.Services.CdcImport
{
    public class TarantoolCdcImportService : BaseCdcImportService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public TarantoolCdcImportService()
            : base(new TarantoolCdcImportFactory())
        {
        }

       public override List<CdcTableTypeModel> GetCdcDataFromTables(string fromLsn = null, string toLsn = null)
        {
            Logger.Trace($"Getting cdc data for with fromLsn:{fromLsn} and toLsn:{toLsn}");
            var tablesGroup = new List<List<CdcTableTypeModel>>
            {
                ImportData(CdcCaptureConstants.Content, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.StatusType, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.ContentAttribute, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.ContentToContent, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.VirtualContentAsync, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.VirtualContentAttributeAsync, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.VirtualContentToContentRev, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.VirtualContentToContentAsync, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.VirtualContentToContentAsyncRev, fromLsn, toLsn),
                GetItemToItemDtosFilteredByNetChanges(fromLsn, toLsn).ToList(),
                GetItemLinkAsyncDtosFilteredByNetChanges(fromLsn, toLsn).ToList(),
                GetContentArticleDtoFilteredByNetChanges(fromLsn, toLsn).ToList()
            };

            return tablesGroup.SelectMany(tg => tg).OrderBy(t => t.TransactionLsn).ToList();
        }

        private IEnumerable<CdcTableTypeModel> GetItemToItemDtosFilteredByNetChanges(string fromLsn = null, string toLsn = null)
        {
            var itemToItemModel = ImportData(CdcCaptureConstants.ItemToItem, fromLsn, toLsn);
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
                    yield return ConvertItemToItemToRev(data);
                }

                yield return data;
            }
        }

        private IEnumerable<CdcTableTypeModel> GetItemLinkAsyncDtosFilteredByNetChanges(string fromLsn = null, string toLsn = null)
        {
            var itemLinkAsyncModel = ImportData(CdcCaptureConstants.ItemLinkAsync, fromLsn, toLsn);
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
                    yield return ConvertItemLinkAsyncToRev(data);
                }

                yield return data;
            }
        }

        private IEnumerable<CdcTableTypeModel> GetContentArticleDtoFilteredByNetChanges(string fromLsn = null, string toLsn = null)
        {
            var contentItemModel = ImportData(CdcCaptureConstants.ContentItem, fromLsn, toLsn);
            var contentDataModel = ImportData(CdcCaptureConstants.ContentData, fromLsn, toLsn);

            var filteredContentItemModel = FilterByIsSplitted(contentItemModel);
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

        private static IEnumerable<CdcTableTypeModel> FilterByIsSplitted(IReadOnlyList<CdcTableTypeModel> contentItemModel)
        {
            if (!contentItemModel.Any())
            {
                yield break;
            }

            for (var i = 1; i < contentItemModel.Count; i++)
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

        internal static CdcTableTypeModel ConvertItemLinkAsyncToRev(CdcTableTypeModel model)
        {
            Ensure.Not((bool)model.Entity.MetaData[TarantoolItemLinkAsyncModel.IsRev]);

            var proto = Mapper.Map<CdcTableTypeModel, CdcTableTypeModel>(model);
            proto.Entity.Columns[TarantoolItemLinkAsyncModel.Id] = model.Entity.Columns[TarantoolItemLinkAsyncModel.LinkedId];
            proto.Entity.Columns[TarantoolItemLinkAsyncModel.LinkedId] = model.Entity.Columns[TarantoolItemLinkAsyncModel.Id];
            proto.Entity.InvariantName = TarantoolItemLinkAsyncModel.GetInvariantName((decimal)proto.Entity.MetaData[TarantoolItemLinkAsyncModel.LinkId], true);

            return proto;
        }

        internal static CdcTableTypeModel ConvertItemToItemToRev(CdcTableTypeModel model)
        {
            Ensure.Not((bool)model.Entity.MetaData[TarantoolItemToItemModel.IsRev]);

            var proto = Mapper.Map<CdcTableTypeModel, CdcTableTypeModel>(model);
            proto.Entity.Columns[TarantoolItemToItemModel.Id] = model.Entity.Columns[TarantoolItemToItemModel.LinkedId];
            proto.Entity.Columns[TarantoolItemToItemModel.LinkedId] = model.Entity.Columns[TarantoolItemToItemModel.Id];
            proto.Entity.InvariantName = TarantoolItemToItemModel.GetInvariantName((decimal)proto.Entity.MetaData[TarantoolItemToItemModel.LinkId], true);

            return proto;
        }

    }
}
