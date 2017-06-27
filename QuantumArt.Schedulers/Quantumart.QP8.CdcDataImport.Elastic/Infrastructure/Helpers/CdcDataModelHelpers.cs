using AutoMapper;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Tarantool;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Helpers
{
    internal static class CdcDataModelHelpers
    {
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
