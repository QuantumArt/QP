using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure.Web.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class CustomAction : EntityObject
    {
        private const string SidParamName = "backend_sid";
        private BackendAction _action;

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomAction;

        internal static CustomAction CreateNew() => new CustomAction
        {
            Action = new BackendAction
            {
                IsCustom = true,
                IsInterface = true,
                WindowHeight = 300,
                WindowWidth = 500
            },
            ShowInMenu = true,
            Sites = Enumerable.Empty<Site>(),
            Contents = Enumerable.Empty<Content>()
        };

        public override void Validate()
        {
            var errors = new RulesException<CustomAction>();
            base.Validate(errors);

            if (Order < 0)
            {
                errors.ErrorFor(a => a.Order, CustomActionStrings.OrderIsNotInRange);
            }
            else if (Order > 0 && Action.EntityTypeId > 0 && !CustomActionRepository.IsOrderUnique(Id, Order, Action.EntityTypeId))
            {
                var freeOrder = GetFreeOrder(Action.EntityTypeId);
                errors.ErrorFor(a => a.Order, string.Format(CustomActionStrings.OrderValueIsNotUniq, freeOrder));
            }

            if (Action.IsInterface && Action.IsWindow)
            {
                if (!Action.WindowHeight.HasValue)
                {
                    errors.ErrorFor(a => a.Action.WindowHeight, CustomActionStrings.WindowHeightIsNotEntered);
                }
                else if (!Action.WindowHeight.Value.IsInRange(100, 1000))
                {
                    errors.ErrorFor(a => a.Action.WindowHeight, string.Format(CustomActionStrings.WindowHeightIsNotInRange, 100, 1000));
                }

                if (!Action.WindowWidth.HasValue)
                {
                    errors.ErrorFor(a => a.Action.WindowWidth, CustomActionStrings.WindowWidthIsNotEntered);
                }
                else if (!Action.WindowWidth.Value.IsInRange(100, 1000))
                {
                    errors.ErrorFor(a => a.Action.WindowWidth, string.Format(CustomActionStrings.WindowWidthIsNotInRange, 100, 1000));
                }
            }

            if (!UrlHelpers.IsValidUrl(Url))
            {
                errors.ErrorFor(a => a.Url, string.Format(CustomActionStrings.UrlInvalidFormat));
            }

            if (!UrlHelpers.IsValidUrl(IconUrl))
            {
                errors.ErrorFor(a => a.IconUrl, string.Format(CustomActionStrings.IconUrlInvalidFormat));
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        public void CalculateOrder(int entityTypeId, bool force = false, int beginRange = 1)
        {
            if (Order <= 0 || force)
            {
                Order = GetFreeOrder(entityTypeId, beginRange);
            }
        }

        private static int GetFreeOrder(int entityTypeId, int beginRange = 1)
        {
            // получить минимальное из неиспользуемых значений Order
            var existingOrders = CustomActionRepository.GetActionOrdersForEntityType(entityTypeId).ToList();
            return existingOrders.Any() ? Enumerable.Range(beginRange, existingOrders.Max() + 1).Except(existingOrders).Min() : 1;
        }

        [LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NameMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        [RequiredValidator(MessageTemplateResourceName = "NameNotEntered", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        [FormatValidator(RegularExpressions.InvalidFieldName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        public override string Name { get; set; }

        [LocalizedDisplayName("Description", NameResourceType = typeof(EntityObjectStrings))]
        [MaxLengthValidator(512, MessageTemplateResourceName = "DescriptionMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        public override string Description { get; set; }

        public int ActionId { get; set; }

        [LocalizedDisplayName("Url", NameResourceType = typeof(CustomActionStrings))]
        [MaxLengthValidator(512, MessageTemplateResourceName = "UrlMaxLengthExceeded", MessageTemplateResourceType = typeof(CustomActionStrings))]
        [RequiredValidator(MessageTemplateResourceName = "UrlNotEntered", MessageTemplateResourceType = typeof(CustomActionStrings))]
        public string Url { get; set; }

        [LocalizedDisplayName("IconUrl", NameResourceType = typeof(CustomActionStrings))]
        [MaxLengthValidator(512, MessageTemplateResourceName = "IconUrlMaxLengthExceeded", MessageTemplateResourceType = typeof(CustomActionStrings))]
        public string IconUrl { get; set; }

        public int Order { get; set; }

        public bool SiteExcluded { get; set; }

        public bool ContentExcluded { get; set; }

        [LocalizedDisplayName("ShowInMenu", NameResourceType = typeof(CustomActionStrings))]
        public bool ShowInMenu { get; set; }

        [LocalizedDisplayName("ShowInToolbar", NameResourceType = typeof(CustomActionStrings))]
        public bool ShowInToolbar { get; set; }

        public BackendAction Action
        {
            get => _action;
            set
            {
                _action = value;
                ParentActions = _action.ToolbarButtons?.Select(n => n.ParentActionId).ToArray();
            }
        }

        public IEnumerable<Content> Contents { get; set; }

        public IEnumerable<Site> Sites { get; set; }

        public string SessionId { get; set; }

        public IEnumerable<int> Ids { get; set; }

        public int ParentId { get; set; }

        public string FullUrl => GetFullUrl(Url);

        private string GetFullUrl(string url)
        {
            string parentContextName = null;
            if (Action.EntityType.ParentId.HasValue)
            {
                var parentEt = EntityTypeRepository.GetById(Action.EntityType.ParentId.Value);
                parentContextName = parentEt.ContextName;
            }

            var symbol = Utils.Url.IsQueryEmpty(url) ? "?" : "&";
            var ids = Ids != null ? string.Join(",", Ids) : string.Empty;
            var paramName = Action.EntityType.ContextName ?? string.Empty;
            var sid = SessionId ?? string.Empty;
            var result = $"{url ?? string.Empty}{symbol}{SidParamName}={sid}&{paramName}={ids}&param_name={paramName}&customerCode={QPContext.CurrentCustomerCode}&actionCode={Action.Code}";
            if (parentContextName != null)
            {
                result = result + $"&{parentContextName}={ParentId}&parent_param_name={parentContextName}";
            }

            return result;
        }

        public string PreActionFullUrl
        {
            get
            {
                var url = Url.Substring(Url.Length - 1, 1) == "/" ? Url.Substring(0, Url.Length - 1) + "PreAction/" : Url + "PreAction";
                return GetFullUrl(url);
            }
        }

        public int[] ParentActions { get; set; }

        public int ForceActionId { get; set; }

        public string ForceActionCode { get; set; }
    }
}
