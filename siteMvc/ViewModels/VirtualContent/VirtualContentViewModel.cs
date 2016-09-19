using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.VirtualContent
{
    public class VirtualContentViewModel : ContentViewModelBase
    {
        public static VirtualContentViewModel Create(BLL.Content content, string tabId, int parentId)
        {
            var viewModel = Create<VirtualContentViewModel>(content, tabId, parentId);
            viewModel.Init();
            return viewModel;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.VirtualContent;

        public override string ActionCode => IsNew ? Constants.ActionCode.AddNewVirtualContents : Constants.ActionCode.VirtualContentProperties;

        [LocalizedDisplayName("JoinFields", NameResourceType = typeof(ContentStrings))]
        public IList<QPTreeCheckedNode> JoinFields { get; set; }
        [LocalizedDisplayName("ToRebuild", NameResourceType = typeof(ContentStrings))]
        public bool ToBuild { get; set; }

        public string JoinRootElementId => UniqueId("JoinRootSelect");

        public string JoinFieldsElementId => UniqueId("JoinFieldsTree");

        public string UnionSourcesElementId => UniqueId("UnionSources");

        public string VTypeRadioContainerElementId => UniqueId("VRadioContainer");

        /// <summary>
        /// Используется ли альтернативный запрос
        /// </summary>
        [LocalizedDisplayName("IsAltUserQueryUsed", NameResourceType = typeof(ContentStrings))]
        public bool IsAltUserQueryUsed { get; set; }

        private void Init()
        {
            if (Data.IsNew)
            {
                Data.VirtualType = Constants.VirtualType.Join;
            }

            JoinFields = new List<QPTreeCheckedNode>(0);
            if (Data.VirtualType == Constants.VirtualType.Join)
            {
                JoinFields = BLL.Content.VirtualFieldNode.Linearize(Data.VirtualJoinFieldNodes).Select(vfn => new QPTreeCheckedNode { Value = vfn.TreeId }).ToList();
            }

            IsAltUserQueryUsed = !string.IsNullOrWhiteSpace(Data.UserQueryAlternative);
            ToBuild = IsNew;
        }

        internal void Update()
        {
            // Если не перестраиваем контент - то удалить список Join полей
            if (!ToBuild)
            {
                JoinFields = new List<QPTreeCheckedNode>(0);
                Data.VirtualJoinFieldNodes = Enumerable.Empty<BLL.Content.VirtualFieldNode>();
            }
            else
            {
                if (Data.VirtualType == Constants.VirtualType.Join)
                {
                    Data.VirtualJoinFieldNodes = BLL.Content.VirtualFieldNode.Parse(BLL.Content.VirtualFieldNode.NormalizeFieldTreeIdSeq(JoinFields.Select(f => f.Value)));
                }
                else
                {
                    JoinFields = new List<QPTreeCheckedNode>(0);
                    Data.VirtualJoinFieldNodes = Enumerable.Empty<BLL.Content.VirtualFieldNode>();
                    Data.JoinRootId = null;
                }

                if (Data.VirtualType != Constants.VirtualType.Union)
                {
                    Data.UnionSourceContentIDs = Enumerable.Empty<int>();
                }

                if (Data.VirtualType != Constants.VirtualType.UserQuery)
                {
                    Data.UserQuery = null;
                    Data.UserQueryAlternative = null;
                }
                else if (Data.VirtualType == Constants.VirtualType.UserQuery && !IsAltUserQueryUsed)
                {
                    Data.UserQueryAlternative = null;
                }
            }
        }

        /// <summary>
        /// Возвращает список контентов доступных для связи с текущим контентом
        /// </summary>
        public IEnumerable<BLL.ListItem> GetAcceptableContentForVirtualJoin()
        {
            var currentContentSiteId = Data.SiteId;
            var contentForJoin = VirtualContentService.GetAcceptableContentForVirtualJoin(currentContentSiteId).Select(c => { c.DependentItemIDs = new[] { "JoinFieldsPanel" }; c.HasDependentItems = true; return c; }).ToArray();
            return new[] { new BLL.ListItem(string.Empty, ContentStrings.SelectContent) }.Concat(contentForJoin);
        }

        public IEnumerable<BLL.ListItem> GetVirtualTypes()
        {
            return new[]
            {
                new BLL.ListItem(Constants.VirtualType.Join.ToString(), BLL.Content.GetVirtualTypeString(Constants.VirtualType.Join), "JoinTypePanel"),
                new BLL.ListItem(Constants.VirtualType.Union.ToString(), BLL.Content.GetVirtualTypeString(Constants.VirtualType.Union), "UnionTypePanel"),
                new BLL.ListItem(Constants.VirtualType.UserQuery.ToString(), BLL.Content.GetVirtualTypeString(Constants.VirtualType.UserQuery), "UserQueryTypePanel")
            };
        }

        public IEnumerable<BLL.ListItem> GetContentsForUnion()
        {
            var callIds = new List<int>();
            if (Data.UnionSourceContentIDs != null)
            {
                callIds.AddRange(Data.UnionSourceContentIDs);
            }

            return ContentService.GetContentsForUnion(Data.SiteId, callIds);
        }

        public QPSelectListItem JoinListItem => Data.JoinRootId.HasValue ? new QPSelectListItem { Value = Data.JoinRootId.Value.ToString(), Text = ContentService.GetNameById(Data.JoinRootId.Value), Selected = true } : null;
    }
}
