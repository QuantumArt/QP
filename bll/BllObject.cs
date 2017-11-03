using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class BllObject : LockableEntityObject
    {
        private int _ContainerObjectTypeId = ObjectType.GetContainer().Id;

        private readonly int _PublishingFormObjectTypeId = ObjectType.GetForm().Id;

        private readonly int _JavaScriptObjectTypeId = ObjectType.GetJavaScript().Id;

        private readonly int _CssObjectTypeId = ObjectType.GetCss().Id;

        private readonly int _GenericTypeId = ObjectType.GetGeneric().Id;

        [LocalizedDisplayName("ParentTemplateObject", NameResourceType = typeof(TemplateStrings))]
        public int? ParentObjectId { get; set; }

        [LocalizedDisplayName("Type", NameResourceType = typeof(TemplateStrings))]
        public int TypeId { get; set; }

        [LocalizedDisplayName("Global", NameResourceType = typeof(TemplateStrings))]
        public bool Global { get; set; }

        [LocalizedDisplayName("DefaultFormat", NameResourceType = typeof(TemplateStrings))]
        public int? DefaultFormatId { get; set; }

        public IEnumerable<ObjectFormat> ChildObjectFormats { get; set; }

        [LocalizedDisplayName("EnableViewState", NameResourceType = typeof(TemplateStrings))]
        public bool EnableViewState { get; set; }

        [LocalizedDisplayName("DisableAutoDataBinding", NameResourceType = typeof(TemplateStrings))]
        public bool DisableDatabind { get; set; }

        [LocalizedDisplayName("CustomClass", NameResourceType = typeof(TemplateStrings))]
        public string ControlCustomClass { get; set; }

        [LocalizedDisplayName("UseDefaultValues", NameResourceType = typeof(TemplateStrings))]
        public bool UseDefaultValues { get; set; }

        [LocalizedDisplayName("NetClassName", NameResourceType = typeof(TemplateStrings))]
        public string NetName { get; set; }

        [LocalizedDisplayName("EnableOnScreen", NameResourceType = typeof(TemplateStrings))]
        public bool AllowStageEdit { get; set; }

        public ObjectFormat DefaultFormat { get; set; }

        public BllObject()
        {
            _contentForm = new InitPropertyValue<ContentForm>(() => InitContentForm());
            _container = new InitPropertyValue<Container>(() => InitContainer());
            InitDefaultValues();
        }

        public void InitDefaultValues()
        {
            _DefaultValues = ObjectRepository.GetDefaultValuesByObjectId(Id).Select(x => new DefaultValue { VariableName = x.VariableName, VariableValue = x.VariableValue }).ToList();
        }

        private ContentForm InitContentForm()
        {
            if (IsObjectFormType)
            {
                return PageTemplateRepository.GetContentFormByObjectId(Id) ?? new ContentForm { ObjectId = Id, ContentId = null, GenerateUpdateScript = true };
            }

            return new ContentForm();
        }

        private Container InitContainer()
        {
            if (IsObjectContainerType)
            {
                var container = PageTemplateRepository.GetContainerByObjectId(Id) ?? new Container { ObjectId = Id, ContentId = null };
                if (container.ContentId != null)
                {
                    container.AdditionalDataForAggregationList = new Dictionary<string, string>
                    {
                        {
                            "fields", string.Join(",", ServiceField.CreateAll().Select(f => f.ColumnName)
                                .Concat(container.Content.Fields.Select(x => x.Name)))
                        },
                        { "orders", TemplateStrings.Ascending + "," + TemplateStrings.Descending }
                    };
                }
                container.AllowDynamicContentChanging = !string.IsNullOrWhiteSpace(container.DynamicContentVariable);
                if (!container.ApplySecurity)
                {
                    container.UseLevelFiltration = false;
                    container.StartLevel = EntityPermissionLevel.GetList().Id;
                    container.EndLevel = EntityPermissionLevel.GetFullAccess().Id;
                }
                return container;
            }

            return new Container
            {
                UseLevelFiltration = false,
                AdditionalDataForAggregationList =
                    new Dictionary<string, string>
                    {
                        { "fields", string.Join(",", ServiceField.CreateAll().Select(f => f.ColumnName)) },
                        { "orders", TemplateStrings.Ascending + "," + TemplateStrings.Descending }
                    }
            };
        }

        internal static BllObject Create(int parentId, bool pageOrTemplate) //true-page false-template
        {
            var obj = new BllObject();
            if (pageOrTemplate)
            {
                obj.PageId = parentId;
                obj.PageTemplateId = obj.page.TemplateId;
                obj.PageTemplate = PageTemplateRepository.GetPageTemplatePropertiesById(obj.page.TemplateId);
            }
            else
            {
                obj.PageTemplateId = parentId;
                obj.PageTemplate = PageTemplateRepository.GetPageTemplatePropertiesById(parentId);
            }
            obj.TypeId = ObjectType.GetGeneric().Id;
            return obj;
        }

        public int? PageId { get; set; }

        public IEnumerable<BllObject> InheritedObjects { get; set; }

        public BllObject ObjectInheritedFrom { get; set; }

        public int PageTemplateId { get; set; }

        public PageTemplate PageTemplate { get; set; }

        public ObjectType ObjectType { get; set; }

        /// <summary>
        /// TRUE- it`s page object, false -it`s template object
        /// </summary>
        public bool PageOrTemplate => PageId.HasValue;

        public override string LockedByAnyoneElseMessage => string.Format(TemplateStrings.ObjectLockedByAnyoneElse, LockedBy);

        public override string EntityTypeCode => PageOrTemplate ? Constants.EntityTypeCode.PageObject : Constants.EntityTypeCode.TemplateObject;

        public override int ParentEntityId => PageOrTemplate ? PageId.Value : PageTemplateId;

        public bool IsSiteDotNet => PageTemplate.SiteIsDotNet;

        private Page _page;

        public Page page
        {
            get
            {
                if (_page == null && PageId.HasValue)
                {
                    _page = PageRepository.GetPagePropertiesById(PageId.Value);
                }
                return _page;
            }
        }

        private List<DefaultValue> _DefaultValues;

        public IEnumerable<DefaultValue> DefaultValues
        {
            get => _DefaultValues;
            set => _DefaultValues = value.ToList();
        }

        public override void Validate()
        {
            var errors = new RulesException<BllObject>();
            base.Validate(errors);

            if (OverrideTemplateObject)
            {
                if (ParentObjectId == null)
                {
                    errors.ErrorFor(x => x.ParentObjectId, TemplateStrings.ParentObjectIdRequired);
                }
            }

            if (PageTemplate.SiteIsDotNet)
            {
                if (!string.IsNullOrWhiteSpace(NetName))
                {
                    if (!Regex.IsMatch(NetName, RegularExpressions.NetName))
                    {
                        errors.ErrorFor(x => x.NetName, TemplateStrings.NetNameInvalidFormat);
                    }
                    if (!ObjectRepository.ObjectNetNameUnique(NetName, PageId, PageTemplateId, PageOrTemplate, Id))
                    {
                        errors.ErrorFor(x => x.NetName, TemplateStrings.NetNameNotUnique);
                    }
                }

                if (!string.IsNullOrWhiteSpace(ControlCustomClass))
                {
                    if (!Regex.IsMatch(ControlCustomClass, RegularExpressions.NetName))
                    {
                        errors.ErrorFor(x => x.ControlCustomClass, TemplateStrings.CustomClassInvalidFormat);
                    }
                    if (ControlCustomClass.Length > 255)
                    {
                        errors.ErrorFor(x => x.ControlCustomClass, TemplateStrings.CustomClassMaxLengthExceeded);
                    }
                }
            }

            if (IsObjectContainerType)
            {
                if (Container.ContentId == null)
                {
                    errors.ErrorFor(x => x.Container.ContentId, TemplateStrings.ContentRequired);
                }
                if (!string.IsNullOrWhiteSpace(Container.FilterValue) && Container.FilterValue.Length > 255)
                {
                    errors.ErrorFor(x => x.Container.FilterValue, TemplateStrings.FilterMaxLengthExceeded);
                }
                if (Container.AllowOrderDynamic)
                {
                    if (string.IsNullOrWhiteSpace(Container.OrderDynamic))
                    {
                        errors.ErrorFor(x => x.Container.OrderDynamic, TemplateStrings.DynamicSortingExpressionRequired);
                    }
                    else if (Container.OrderDynamic.Length > 512)
                    {
                        errors.ErrorFor(x => x.Container.OrderDynamic, TemplateStrings.DynamicSortingExpressionMaxLengthExceeded);
                    }
                }
                if (Container.AllowDynamicContentChanging)
                {
                    if (string.IsNullOrWhiteSpace(Container.DynamicContentVariable))
                    {
                        errors.ErrorFor(x => x.Container.DynamicContentVariable, TemplateStrings.DynamicContentVariableRequired);
                    }
                    else if (Container.DynamicContentVariable.Length > 255)
                    {
                        errors.ErrorFor(x => x.Container.DynamicContentVariable, TemplateStrings.DynamicContentVariableMaxLengthExceeded);
                    }
                }
                if (Container.AllowOrderDynamic)
                {
                    if (string.IsNullOrWhiteSpace(Container.OrderDynamic))
                    {
                        errors.ErrorFor(x => x.Container.OrderDynamic, TemplateStrings.OrderDynamicRequired);
                    }
                    else if (Container.OrderDynamic.Length > 255)
                    {
                        errors.ErrorFor(x => x.Container.OrderDynamic, TemplateStrings.OrderDynamicMaxLengthExceeded);
                    }
                }

                if (!Container.StartsFromFirstArticle)
                {
                    if (string.IsNullOrWhiteSpace(Container.SelectStart))
                    {
                        errors.ErrorFor(x => x.Container.SelectStart, TemplateStrings.SelectStartRequired);
                    }
                    else if (Container.SelectStart.Length > 255)
                    {
                        errors.ErrorFor(x => x.Container.SelectStart, TemplateStrings.SelectStartMaxLengthExceeded);
                    }
                }

                if (!Container.IncludesAllArticles)
                {
                    if (string.IsNullOrWhiteSpace(Container.SelectTotal))
                    {
                        errors.ErrorFor(x => x.Container.SelectTotal, TemplateStrings.SelectTotalRequired);
                    }
                    else if (Container.SelectTotal.Length > 255)
                    {
                        errors.ErrorFor(x => x.Container.SelectTotal, TemplateStrings.SelectTotalMaxLengthExceeded);
                    }
                }
            }

            else if (IsObjectFormType)
            {
                if (ContentForm.ContentId == null)
                {
                    errors.ErrorFor(x => x.ContentForm.ContentId, TemplateStrings.ContentRequired);
                }
            }

            if (UseDefaultValues)
            {
                var duplicateNames = DefaultValues.GroupBy(c => c.VariableName).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
                var defaultArray = DefaultValues.ToArray();
                for (var i = 0; i < defaultArray.Length; i++)
                {
                    ValidateDefaultValue(defaultArray[i], errors, i + 1, duplicateNames);
                }
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private void ValidateDefaultValue(DefaultValue dValue, RulesException<BllObject> errors, int index, string[] dupNames)
        {
            if (dupNames.Contains(dValue.VariableName))
            {
                errors.ErrorForModel(string.Format(TemplateStrings.DefaultValueNameNotUnique, index));
                dValue.Invalid = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(dValue.VariableName))
            {
                errors.ErrorForModel(string.Format(TemplateStrings.DefaultValueNameRequired, index));
                dValue.Invalid = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(dValue.VariableValue))
            {
                errors.ErrorForModel(string.Format(TemplateStrings.DefaultValueValueRequired, index));
                dValue.Invalid = true;
                return;
            }

            if (dValue.VariableName.Length > 255)
            {
                errors.ErrorForModel(string.Format(TemplateStrings.DefaultValueNameMaxLengthExceeded, index));
                dValue.Invalid = true;
                return;
            }

            if (dValue.VariableValue.Length > 255)
            {
                errors.ErrorForModel(string.Format(TemplateStrings.DefaultValueValueMaxLengthExceeded, index));
                dValue.Invalid = true;
            }
        }

        [LocalizedDisplayName("OverrideTemplateObject", NameResourceType = typeof(TemplateStrings))]
        public bool OverrideTemplateObject { get; set; }

        public bool IsObjectContainerType => TypeId == 2;

        public bool IsObjectFormType => TypeId == _PublishingFormObjectTypeId;

        public bool IsJavaScriptType => TypeId == _JavaScriptObjectTypeId;

        public bool IsCssType => TypeId == _CssObjectTypeId;

        public bool IsGenericType => TypeId == _GenericTypeId;

        private readonly InitPropertyValue<ContentForm> _contentForm;

        public ContentForm ContentForm
        {
            get => _contentForm.Value;
            set => _contentForm.Value = value;
        }

        private readonly InitPropertyValue<Container> _container;

        public Container Container
        {
            get => _container.Value;
            set => _container.Value = value;
        }

        internal void BindWithStatuses(IEnumerable<int> activeStatuses, bool isContainer)
        {
            var stats = activeStatuses.ToList();
            ObjectRepository.SetObjectActiveStatuses(Id, stats, isContainer);
        }

        public void GenerateNetName()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                NetName = Name.Replace(" ", "_");
            }
        }
    }
}
