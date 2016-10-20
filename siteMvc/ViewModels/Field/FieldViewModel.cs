using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Field
{
    public class FieldViewModel : EntityViewModel
    {
        public static FieldViewModel Create(BLL.Field field, string tabId, int parentId)
        {
            var viewModel = Create<FieldViewModel>(field, tabId, parentId);
            var allVeStylesAndFormats = SiteService.GetAllVeStyles();
            viewModel.ActiveVeCommands = viewModel.Data.ActiveVeCommandIds.Select(c => new QPCheckedItem { Value = c.ToString() }).ToList();
            viewModel.DefaultCommandsListItems = FieldService.GetDefaultVisualEditorCommands().Select(c => new ListItem { Value = c.Id.ToString(), Text = c.Alias }).ToArray();
            viewModel.AllStylesListItems = allVeStylesAndFormats.Where(s => s.IsFormat == false).Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToArray();
            viewModel.AllFormatsListItems = allVeStylesAndFormats.Where(s => s.IsFormat).Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToArray();
            viewModel.ActiveVeStyles = allVeStylesAndFormats.Where(s => s.IsFormat == false && viewModel.Data.ActiveVeStyleIds.Contains(s.Id)).Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
            viewModel.ActiveVeFormats = allVeStylesAndFormats.Where(s => s.IsFormat && viewModel.Data.ActiveVeStyleIds.Contains(s.Id)).Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
            viewModel.DefaultArticleIds = viewModel.Data.DefaultArticleIds;
            viewModel.DefaultArticleListItems = viewModel.Data.DefaultArticleListItems;
            viewModel.Init();
            return viewModel;
        }

        public new BLL.Field Data
        {
            get { return (BLL.Field)EntityData; }
            set { EntityData = value; }
        }

        public bool OrderChanged { get; set; }

        public bool ViewInListAffected { get; set; }

        public string RelatedReadActionCode => Data.IsNew || Data.RelatedToContent == null ? string.Empty : (Data.RelatedToContent.IsVirtual ? Constants.ActionCode.VirtualContentProperties : Constants.ActionCode.ContentProperties);

        public IList<QPCheckedItem> InCombinationWith { get; set; }

        [LocalizedDisplayName("InputMaskType", NameResourceType = typeof(FieldStrings))]
        public InputMaskTypes InputMaskType { get; set; }

        [LocalizedDisplayName("InputMask", NameResourceType = typeof(FieldStrings))]
        public int? MaskTemplateId { get; set; }

        [LocalizedDisplayName("DynamicImageMode", NameResourceType = typeof(FieldStrings))]
        public DynamicImageMode DynamicImageSizeMode { get; set; }

        private string _entityTypeCode = Constants.EntityTypeCode.Field;

        public override string EntityTypeCode => _entityTypeCode;

        private string _actionCode = Constants.ActionCode.FieldProperties;

        public override string ActionCode
        {
            get
            {
                return IsNew ? Constants.ActionCode.AddNewField : _actionCode;
            }
        }

        public override ExpandoObject MainComponentOptions
        {
            get
            {
                dynamic result = base.MainComponentOptions;
                result.orderChanged = OrderChanged;
                result.viewInListAffected = ViewInListAffected;
                return result;
            }
        }

        public string RelateToSelectElementId => UniqueId("RelateToSelect");

        public string DisplayFieldSelectElementId => UniqueId("DisplayFieldSelect");

        public string CurrentFieldIdHiddenElementId => UniqueId("CurrentFieldIdHidden");

        public string InCombinationWithElementId => UniqueId("InCombinationWith");

        public string FileDefaultElementId => UniqueId("FileDefaultFileField");

        public string ExactSelectElementId => UniqueId("ExactSelect");

        public string O2MDefaultElementId => UniqueId("O2MSinglePicker");

        public string M2MDefaultElementId => UniqueId("M2MPicker");

        public string IndexedElementId => UniqueId("FieldIndexedCheckbox");

        public string OnScreenElementId => UniqueId("FieldOnScreenCheckbox");

        public string RelationFieldPanelElementId => UniqueId("RelationFieldPanel");

        public string O2MDefaultPanelElementId => UniqueId("O2MDefaultPanel");

        public string M2MBackwardFieldNamePanelElementId => UniqueId("M2MBackwardFieldNamePanel");

        public string O2MBackwardFieldNamePanelElementId => UniqueId("O2MBackwardFieldNamePanel");

        public string O2MTreePanelElementId => UniqueId("O2MTreePanel");

        public string ClassifierFieldPanelElementId => UniqueId("ClassifierFieldPanel");

        public string M2MDefaultPanelElementId => UniqueId("M2MDefaultPanel");

        public string ListTitleOptionsPanelElementId => UniqueId("ListTitleOptionsPanel");

        public string AggregatedElementId => UniqueId("AggregatedCheckbox");

        public string ClassifierSelectElementId => UniqueId("ClassifierSelect");

        public string ListOrderSelectElementId => UniqueId("ListOrderSelect");

        public string RelatedSettingsPanelsSelector => $"#{RelationFieldPanelElementId},#{O2MDefaultPanelElementId},#{M2MBackwardFieldNamePanelElementId},#{O2MBackwardFieldNamePanelElementId},#{O2MTreePanelElementId},#{ClassifierFieldPanelElementId},#{M2MDefaultPanelElementId},#{ListTitleOptionsPanelElementId}";

        public EntityDataListArgs InCombinationWithEventArgs => new EntityDataListArgs { MaxListHeight = 200 };

        [LocalizedDisplayName("ParentField", NameResourceType = typeof(FieldStrings))]
        public string ParentFieldName => Data.ParentField == null ? string.Empty : Data.ParentField.Name;

        [LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<ListItem> DefaultCommandsListItems { get; private set; }

        [LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
        public IList<QPCheckedItem> ActiveVeCommands { get; set; }

        public int[] ActiveVeCommandsIds { get { return ActiveVeCommands.Select(c => int.Parse(c.Value)).ToArray(); } }

        public int[] ActiveVeStyleIds { get { return ActiveVeStyles.Union(ActiveVeFormats).Select(c => int.Parse(c.Value)).ToArray(); } }

        public IEnumerable<ListItem> AllStylesListItems { get; private set; }

        public IEnumerable<ListItem> AllFormatsListItems { get; private set; }

        [LocalizedDisplayName("Styles", NameResourceType = typeof(VisualEditorStrings))]
        public IList<QPCheckedItem> ActiveVeStyles { get; set; }

        [LocalizedDisplayName("Formats", NameResourceType = typeof(VisualEditorStrings))]
        public IList<QPCheckedItem> ActiveVeFormats { get; set; }

        public string AggregationListItems_Data_ExternalCssItems { get; set; }

        public string AggregationListItems_Data_StringEnumItems { get; set; }

        private void Init()
        {
            InitInputMask();
            InitDynamicImage();
            InitOrder();

            InCombinationWith = Data.IsUnique ? GetCombinedFields().Select(f => new QPCheckedItem { Value = f.Id.ToString() }).ToList() : new List<QPCheckedItem>();
            _entityTypeCode = Data.Content.VirtualType == VirtualType.None ? Constants.EntityTypeCode.Field : Constants.EntityTypeCode.VirtualField;
            _actionCode = Data.Content.VirtualType == VirtualType.None ? Constants.ActionCode.FieldProperties : Constants.ActionCode.VirtualFieldProperties;
        }

        private void InitOrder()
        {
            if (IsNew && Data.Order == 0 && Data.Content.Fields.Any())
            {
                Data.Order = Data.Content.Fields.Select(f => f.Order).Max();
            }
        }

        private void InitInputMask()
        {

            InputMaskType = InputMaskTypes.Basic;
            MaskTemplateId = null;

            if (!IsNew && !string.IsNullOrEmpty(Data.InputMask))
            {
                var basicTemplate = BLL.Field.GetAllMaskTemplates().FirstOrDefault(t => t.Mask.Equals(Data.InputMask, StringComparison.InvariantCulture));
                if (basicTemplate != null)
                {
                    MaskTemplateId = basicTemplate.Id;
                    InputMaskType = InputMaskTypes.Basic;
                }
                else
                {
                    InputMaskType = InputMaskTypes.Custom;
                }
            }
        }

        private void InitDynamicImage()
        {
            if (Data.DynamicImage == null)
            {
                Data.DynamicImage = DynamicImage.Create(Data);
                DynamicImageSizeMode = DynamicImageMode.Size;
            }
            else
            {
                if (Data.DynamicImage.Height > 0 && Data.DynamicImage.Width > 0 || Data.DynamicImage.Height == 0 && Data.DynamicImage.Width == 0)
                {
                    DynamicImageSizeMode = DynamicImageMode.Size;
                }
                else if (Data.DynamicImage.Height > 0 && Data.DynamicImage.Width <= 0)
                {
                    DynamicImageSizeMode = DynamicImageMode.Height;
                }
                else if (Data.DynamicImage.Height <= 0 && Data.DynamicImage.Width > 0)
                {
                    DynamicImageSizeMode = DynamicImageMode.Width;
                }
            }
        }

        public void Update()
        {
            UpdateConstraint();
            Data.UpdateModel();
            UpdateInputMask();
            UpdateDynamicImage();

            if (Data.ExactType != FieldExactTypes.VisualEdit)
            {
                Data.FullPage = false;
            }

            if (Data.ExactType != FieldExactTypes.File && Data.ExactType != FieldExactTypes.Image)
            {
                Data.SubFolder = null;
                Data.UseSiteLibrary = false;
                Data.RenameMatched = false;
                Data.DisableVersionControl = false;
            }
            else
            {
                Data.SubFolder = PathUtility.CorrectSlashes(Data.SubFolder, CorrectSlashMode.ReplaceDoubleSlashes | CorrectSlashMode.ConvertSlashesToBackSlashes | CorrectSlashMode.WrapToSlashes | CorrectSlashMode.RemoveLastSlash);
            }
        }

        private void UpdateDynamicImage()
        {
            if (Data.ExactType != FieldExactTypes.DynamicImage)
            {
                Data.DynamicImage = null;
            }
            else
            {
                Data.DynamicImage.MaxSize = false;
                switch (DynamicImageSizeMode)
                {
                    case DynamicImageMode.Size:
                        Data.DynamicImage.MaxSize = true;
                        break;
                    case DynamicImageMode.Height:
                        Data.DynamicImage.Width = 0;
                        break;
                    case DynamicImageMode.Width:
                        Data.DynamicImage.Height = 0;
                        break;
                }

                if (Data.DynamicImage.Type != DynamicImage.JPG_EXTENSION)
                {
                    Data.DynamicImage.Quality = 0;
                }
            }
        }

        private void UpdateInputMask()
        {
            if (Data.ExactType != FieldExactTypes.String)
            {
                Data.UseInputMask = false;
            }

            if (Data.UseInputMask)
            {
                if (InputMaskType == InputMaskTypes.Basic)
                {
                    if (MaskTemplateId.HasValue)
                    {
                        Data.InputMask = BLL.Field.GetAllMaskTemplates().Single(t => t.Id == MaskTemplateId).Mask;
                    }
                }
            }
            else
            {
                Data.InputMask = null;
            }
        }

        /// <summary>
        /// Устанавливает ограничения по уникальности
        /// </summary>
        private void UpdateConstraint()
        {
            // Если поле уникальное, то добавить Constraint Rule для него если такого еще нет
            // Если не уникальное, то удалить правило для него если такое есть
            if (Data.IsUnique)
            {
                if (Data.Constraint == null)
                {
                    Data.Constraint = new ContentConstraint { ContentId = Data.ContentId };
                }

                Data.Constraint.Rules = InCombinationWith
                    .Distinct(QPCheckedItem.GetComparer<QPCheckedItem>())
                    .Select(c =>
                        new ContentConstraintRule
                        {
                            ConstraintId = Data.Constraint.Id,
                            FieldId = Converter.ToInt32(c.Value)
                        })
                    .ToList();
                Data.Constraint.Rules.Add(new ContentConstraintRule
                {
                    ConstraintId = Data.Constraint.Id,
                    FieldId = Data.Id
                });

            }
            else
            {
                // Удалаяем правило для текущего поля если оно есть
                var currentFieldRule = Data.Constraint?.Rules.FirstOrDefault(r => r.FieldId == Data.Id);
                if (currentFieldRule != null)
                {
                    Data.Constraint.Rules.Remove(currentFieldRule);
                }
            }
        }

        public bool IsViewInListShown => Data.ExactType != FieldExactTypes.Textbox && Data.ExactType != FieldExactTypes.VisualEdit && Data.ExactType != FieldExactTypes.M2MRelation && Data.ExactType != FieldExactTypes.M2ORelation;

        public bool IsNameReadOnly => !(Data.Content.VirtualType == VirtualType.None || Data.Content.VirtualType == VirtualType.Join && Data.JoinId.HasValue);

        /// <summary>
        /// Возвращает список допустимых типов для данного поля
        /// </summary>
        /// <returns></returns>
        public List<ListItem> GetAcceptableExactFieldTypes()
        {
            var acceptableFieldTypes = FieldService.GetAcceptableExactFieldTypes(IsNew ? FieldExactTypes.Undefined : Data.ExactType);
            Func<ListItem, bool> acceptableFilter = i => acceptableFieldTypes.Contains((FieldExactTypes)Enum.Parse(typeof(FieldExactTypes), i.Value));
            Func<int, string> getValue = id => Translator.Translate(FieldType.AllFieldTypes.Single(f => f.Id == id).Name);

            return new List<ListItem>
            {
                new ListItem { Value = FieldExactTypes.String.ToString(), Text = getValue(FieldTypeCodes.String), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen"} },
                new ListItem { Value = FieldExactTypes.Numeric.ToString(), Text = getValue(FieldTypeCodes.Numeric), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "IsNumericLong", "NumericIntegerProps"} },
                new ListItem { Value = FieldExactTypes.Boolean.ToString(), Text = getValue(FieldTypeCodes.Boolean), HasDependentItems = true, DependentItemIDs = new[]{"Unique", "OnScreen", "ViewInList"} },
                new ListItem { Value = FieldExactTypes.Date.ToString(), Text = getValue(FieldTypeCodes.Date), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Indexed", "ViewInList", "OnScreen"} },
                new ListItem { Value = FieldExactTypes.Time.ToString(), Text = getValue(FieldTypeCodes.Time), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Indexed", "ViewInList", "OnScreen"} },
                new ListItem { Value = FieldExactTypes.DateTime.ToString(), Text = getValue(FieldTypeCodes.DateTime), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Indexed", "ViewInList", "OnScreen"} },
                new ListItem { Value = FieldExactTypes.File.ToString(), Text = getValue(FieldTypeCodes.File), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "FileOrImage"} },
                new ListItem { Value = FieldExactTypes.Image.ToString(), Text = getValue(FieldTypeCodes.Image), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "FileOrImage"} },
                new ListItem { Value = FieldExactTypes.Textbox.ToString(), Text = getValue(FieldTypeCodes.Textbox), HasDependentItems = true, DependentItemIDs = new[]{"Required", "OnScreen", "ViewInList"} },
                new ListItem { Value = FieldExactTypes.VisualEdit.ToString(), Text = getValue(FieldTypeCodes.VisualEdit), HasDependentItems = true, DependentItemIDs = new[]{"Required", "OnScreen", "ViewInList"} },
                new ListItem { Value = FieldExactTypes.O2MRelation.ToString(), Text = FieldStrings.O2MRelation, HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "RelateTo", "LinqBackProperty", "O2MDefault", "O2MBackwardFieldName", "O2MMainProps", "RelationSecurity", "ListTitleOptions"} },
                new ListItem { Value = FieldExactTypes.M2MRelation.ToString(), Text = FieldStrings.M2MRelation, HasDependentItems = true, DependentItemIDs = new[]{"Required", "RelateTo", "M2MMaplink", "M2MBackwardFieldName", "RelationSecurity", "ViewInList", "ListOrderOptions"} },
                new ListItem { Value = FieldExactTypes.M2ORelation.ToString(), Text = getValue(FieldTypeCodes.M2ORelation), HasDependentItems = true, DependentItemIDs = new[]{"Required", "BaseRelation", "ViewInList", "ListOrderOptions" } },
                new ListItem { Value = FieldExactTypes.DynamicImage.ToString(), Text = getValue(FieldTypeCodes.DynamicImage), HasDependentItems = true },
                new ListItem { Value = FieldExactTypes.Classifier.ToString(), Text = FieldStrings.ClassifierId, HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "UseTypeSecurity" } },
                new ListItem { Value = FieldExactTypes.StringEnum.ToString(), Text = FieldStrings.StringEnum, HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen"} }
            }.Where(acceptableFilter).ToList();
        }

        /// <summary>
        /// Получить список полей соответствующего контетна которые могут быть в комбинации с текущим полем
        /// т.е. не входят не в один контент и иммеют разрешенный тип
        /// </summary>
        /// <returns></returns>
        private IEnumerable<BLL.Field> GetNonUniqueContentFields()
        {
            return Data.Content.Fields.Where(f => f.Constraint == null).Where(f => BLL.Field.UniqueFieldTypes.Contains(f.ExactType)).ToArray();
        }

        /// <summary>
        /// Скомбинированный с текущим полем
        /// </summary>
        /// <returns></returns>
        private IEnumerable<BLL.Field> GetCombinedFields()
        {
            return Data.Constraint?.Rules.Where(r => r.FieldId != Data.Id).Select(r => r.Field).Distinct(BLL.Field.IdComparer).ToArray() ?? Enumerable.Empty<BLL.Field>();
        }
        /// <summary>
        /// Получить список полей в комбинации с которыми находится данное поле
        /// а также те, которые могут быть в комбинации с текущим полем
        ///
        /// </summary>
        /// <returns></returns>
        public List<ListItem> GetAcceptableCombinationFields()
        {
            var result = GetNonUniqueContentFields();
            if (Data.IsUnique)
            {
                result = result.Concat(GetCombinedFields());
            }

            return result.Where(f => f.Id != Data.Id).OrderBy(f => f.Id).Distinct(BLL.Field.IdComparer).Select(f => new ListItem { Text = f.Name, Value = f.Id.ToString() }).ToList();
        }

        /// <summary>
        /// Возвращает список классификаторов из связанного контента
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetAcceptableClassifierFields()
        {
            return Data.RelatedToContent?.Fields.Where(f => f.IsClassifier).OrderBy(f => f.Id).Select(f => new ListItem { Text = f.Name, Value = f.Id.ToString() }).ToArray() ?? new[] { new ListItem("", FieldStrings.NoFields) };
        }

        /// <summary>
        /// Возвращает список контентов доступных для связи с текущим контентом
        /// </summary>
        public IEnumerable<ListItem> GetAcceptableContentForRelation()
        {
            var currentContentId = Data.ContentId;
            var contentForRelation = ContentService.GetAcceptableContentForRelation(currentContentId).ToArray();
            return new[] { new ListItem("", FieldStrings.SelectContent) }.Concat(contentForRelation);
        }

        /// <summary>
        /// Возвращает список полей для связи O2M по выбранному контенту
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetAcceptableRelatedFields()
        {
            return Data.RelateToContentId != null ? ContentService.GetRelateableFields(Data.RelateToContentId.Value, Data.Id) : new[] { new ListItem("", FieldStrings.NoFields) };
        }

        /// <summary>
        /// Возвращает список базовых полей для поля M2O
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetBaseFieldsForM2O()
        {
            return new[] { new ListItem("", FieldStrings.SelectField) }.Concat(FieldService.GetBaseFieldsForM2O(Data.ContentId, Data.Id));
        }

        /// <summary>
        /// Возвращает список типов маски ввода
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetInputMaskTypes()
        {
            return new[]
            {
                new ListItem(InputMaskTypes.Basic.ToString(), FieldStrings.BasicInputMask, true),
                new ListItem(InputMaskTypes.Custom.ToString(), FieldStrings.CustomInputMask, true)
            };
        }

        /// <summary>
        /// Возвращает список типов маски ввода
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetMaskTemplates()
        {
            return new[] { new ListItem("", FieldStrings.SelectMaskTemplate) }.Concat(BLL.Field.GetAllMaskTemplates().Select(t => new ListItem(t.Id.ToString(), t.Name)));
        }

        /// <summary>
        /// Возвращает список полей типа Image текущего контента
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetContentImageFields()
        {
            return new[] { new ListItem("", FieldStrings.SelectField) }.Concat(Data.Content.Fields.Where(f => f.ExactType == FieldExactTypes.Image && f.Id != Data.Id).Select(f => new ListItem(f.Id.ToString(), f.Name)));
        }

        /// <summary>
        /// Возвращает список режимов ресайза Dynamic Image
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetDynamicImageModes()
        {
            return new[]
            {
                new ListItem(DynamicImageMode.Size.ToString(), FieldStrings.DynamicImageModeSize, true){DependentItemIDs = new[]{"HeightModePanel", "WidthModePanel"}},
                new ListItem(DynamicImageMode.Height.ToString(), FieldStrings.DynamicImageModeHeight, true){DependentItemIDs = new[]{"HeightModePanel"}},
                new ListItem(DynamicImageMode.Width.ToString(), FieldStrings.DynamicImageModeWidth, true){DependentItemIDs = new[]{"WidthModePanel"}}
            };
        }

        /// <summary>
        /// Возвращает список типов файлов для Dynamic Image
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetDynamicImageFileTypes()
        {
            return new[]
            {
                new ListItem(DynamicImage.JPG_EXTENSION, DynamicImage.JPG_EXTENSION, true),
                new ListItem(DynamicImage.PNG_EXTENSION, DynamicImage.PNG_EXTENSION),
                new ListItem(DynamicImage.GIF_EXTENSION, DynamicImage.GIF_EXTENSION)
            };
        }

        /// <summary>
        /// Возвращает варианты размещения поля относительно других полей
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetOrderOptions()
        {
            var result = new List<ListItem>();
            var orderedField = Data.Content.Fields.OrderBy(f => f.Order).ToArray();
            if (orderedField.Length > 0)
            {
                if (orderedField[0].Id != Data.Id)
                {
                    result.Add(new ListItem("0", string.Format(FieldStrings.OrderOptionTemplate, orderedField[0].Name)));
                }

                if (orderedField.Length > 1)
                {
                    for (var i = 0; i < orderedField.Length - 1; i++)
                    {
                        if (orderedField[i + 1].Id != Data.Id)
                        {
                            result.Add(new ListItem(orderedField[i].Order.ToString(), string.Format(FieldStrings.OrderOptionTemplate, orderedField[i + 1].Name)));
                        }
                    }
                }

                result.Add(new ListItem(orderedField[orderedField.Length - 1].Order.ToString(), FieldStrings.OrderOptionAfterAll));
            }
            else
            {
                result.Add(new ListItem("0", FieldStrings.OrderOptionAfterAll));
            }

            return result;
        }

        public QPSelectListItem O2MDefaultValueListItem
        {
            get
            {
                if (Data.ExactType == FieldExactTypes.O2MRelation)
                {
                    return Data.O2MDefaultValue != null ? new QPSelectListItem { Value = Data.O2MDefaultValue, Text = Data.O2MDefaultValueName, Selected = true } : null;
                }

                return null;
            }
        }

        public QPSelectListItem M2MDefaultValueListItem => Data.M2MDefaultValue != null ? new QPSelectListItem { Value = Data.M2MDefaultValue, Text = Data.O2MDefaultValueName, Selected = true } : null;

        /// <summary>
        /// Возвращает список контентов для классификатора
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ListItem> GetAggregetableContentsForClassifier()
        {
            return FieldService.GetAggregetableContentsForClassifier(Data);
        }

        public IEnumerable<ListItem> GetFieldsForTreeOrder()
        {
            var contentId = Data.RelateToContentId ?? Data.ContentId;
            return FieldService.GetFieldsForTreeOrder(contentId, Data.Id);
        }

        public QPSelectListItem RelateToListItem => Data.RelateToContentId.HasValue ? new QPSelectListItem { Value = Data.RelateToContentId.Value.ToString(), Text = ContentService.GetNameById(Data.RelateToContentId.Value), Selected = true } : null;

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public IEnumerable<int> DefaultArticleIds { get; set; }

        public IEnumerable<ListItem> DefaultArticleListItems { get; set; }

        internal void DoCustomBinding()
        {
            if (!string.IsNullOrWhiteSpace(AggregationListItems_Data_ExternalCssItems))
            {
                Data.ExternalCssItems = new JavaScriptSerializer().Deserialize<List<ExternalCss>>(AggregationListItems_Data_ExternalCssItems);
                Data.ExternalCss = ExternalCssHelper.ConvertToString(Data.ExternalCssItems);
            }

            Data.ParseStringEnumJson(AggregationListItems_Data_StringEnumItems);
            Data.DefaultArticleIds = DefaultArticleIds.ToArray();
            Data.ActiveVeStyleIds = ActiveVeStyleIds;
            Data.ActiveVeCommandIds = ActiveVeCommandsIds;
        }

        public EntityDataListArgs EntityDataListArgs => new EntityDataListArgs
        {
            EntityTypeCode = Constants.EntityTypeCode.Article,
            ParentEntityId = Data.RelateToContentId,
            SelectActionCode = Constants.ActionCode.MultipleSelectArticle,
            Filter = Data.RelationFilter,
            ListId = Data.Id,
            MaxListHeight = 200,
            MaxListWidth = 350,
            ShowIds = true
        };
    }
}
