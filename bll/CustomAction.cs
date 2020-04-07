using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using QP8.Infrastructure.Web.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class CustomAction : EntityObject
    {
        private const string SidParamName = "backend_sid";
        private BackendAction _action;
        private static Regex LastSlashRegex = new Regex(@"\/$");
        private static Regex ActionSlugRegex = new Regex(@"\/action\/", RegexOptions.IgnoreCase);

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

        public CustomAction Clone()
        {
            var ca = (CustomAction)MemberwiseClone();
            ca.Action = ca.Action.Clone();
            ca.ContentIds = new List<int>(ContentIds);
            ca.SiteIds = new List<int>(SiteIds);
            return ca;
        }

        private static int GetFreeOrder(int entityTypeId, int beginRange = 1)
        {
            // получить минимальное из неиспользуемых значений Order
            var existingOrders = CustomActionRepository.GetActionOrdersForEntityType(entityTypeId).ToList();
            return existingOrders.Any() ? Enumerable.Range(beginRange, existingOrders.Max() + 1).Except(existingOrders).Min() : 1;
        }

        [Display(Name = "Name", ResourceType = typeof(EntityObjectStrings))]
        [StringLength(255, ErrorMessageResourceName = "NameMaxLengthExceeded", ErrorMessageResourceType = typeof(EntityObjectStrings))]
        [Required(ErrorMessageResourceName = "NameNotEntered", ErrorMessageResourceType = typeof(EntityObjectStrings))]
        [RegularExpression(RegularExpressions.FieldName, ErrorMessageResourceName = "NameInvalidFormat", ErrorMessageResourceType = typeof(EntityObjectStrings))]
        public override string Name { get; set; }

        [Display(Name = "Description", ResourceType = typeof(EntityObjectStrings))]
        [StringLength(512, ErrorMessageResourceName = "DescriptionMaxLengthExceeded", ErrorMessageResourceType = typeof(EntityObjectStrings))]
        public override string Description { get; set; }

        [Display(Name = "Alias", ResourceType = typeof(CustomActionStrings))]
        [StringLength(255, ErrorMessageResourceName = "AliasExceeded", ErrorMessageResourceType = typeof(CustomActionStrings))]
        [RegularExpression(RegularExpressions.NetName, ErrorMessageResourceName = "AliasInvalidFormat", ErrorMessageResourceType = typeof(CustomActionStrings))]

        public string Alias { get; set; }

        public int ActionId { get; set; }

        [Display(Name = "Url", ResourceType = typeof(CustomActionStrings))]
        [StringLength(512, ErrorMessageResourceName = "UrlMaxLengthExceeded", ErrorMessageResourceType = typeof(CustomActionStrings))]
        [Required(ErrorMessageResourceName = "UrlNotEntered", ErrorMessageResourceType = typeof(CustomActionStrings))]
        public string Url { get; set; }

        [Display(Name = "IconUrl", ResourceType = typeof(CustomActionStrings))]
        [StringLength(512, ErrorMessageResourceName = "IconUrlMaxLengthExceeded", ErrorMessageResourceType = typeof(CustomActionStrings))]
        public string IconUrl { get; set; }

        public int Order { get; set; }

        public bool SiteExcluded { get; set; }

        public bool ContentExcluded { get; set; }

        [Display(Name = "ShowInMenu", ResourceType = typeof(CustomActionStrings))]
        public bool ShowInMenu { get; set; }

        [Display(Name = "ShowInToolbar", ResourceType = typeof(CustomActionStrings))]
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

        public IEnumerable<int> ContentIds { get; set; }

        public IEnumerable<int> SiteIds { get; set; }

        public string SessionId { get; set; }

        public IEnumerable<int> Ids { get; set; }

        public int ParentId { get; set; }

        [BindNever]
        [ValidateNever]
        public string FullUrl => GetFullUrl(Url);

        private string GetFullUrl(string url, bool includeSid = true)
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
            var sidStr = includeSid ? $"&{SidParamName}={sid}" : "";
            var result = $"{url ?? string.Empty}{symbol}{paramName}={ids}&param_name={paramName}&customerCode={QPContext.CurrentCustomerCode}&actionCode={Action.Code}{sidStr}";
            if (parentContextName != null)
            {
                result = result + $"&{parentContextName}={ParentId}&parent_param_name={parentContextName}";
            }

            return result;
        }

        [BindNever]
        [ValidateNever]
        public string PreActionFullUrl
        {
            get
            {
                var url = Url;

                if (LastSlashRegex.IsMatch(url))
                {
                    url = LastSlashRegex.Replace(url, "PreAction/");
                }
                else if (ActionSlugRegex.IsMatch(url))
                {
                    url = ActionSlugRegex.Replace(url, "/PreAction/");
                }
                else if (url.Contains("?"))
                {
                    url = url.Replace("?", "?isPreAction=true&");
                }
                else
                {
                    url += "PreAction";
                }

                return GetFullUrl(url, false);
            }
        }

        public int[] ParentActions { get; set; }

        public int ForceActionId { get; set; }

        public string ForceActionCode { get; set; }
    }


}
