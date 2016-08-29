// ****************************************************************************
// *** Компонент "Редактор сущности"                    ***
// ****************************************************************************

//#region event types of entity editor
// === Типы событий редактора сущностей ===
var EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTING = 'OnEntityEditorSubmitting';
var EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED = 'OnEntityEditorSubmitted';
var EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED_ERROR = 'OnEntityEditorSubmittedError';
var EVENT_TYPE_ENTITY_EDITOR_ENTITY_REFRESH_STARTING = 'OnEntityEditorRefreshStarting';
var EVENT_TYPE_ENTITY_EDITOR_ACTION_EXECUTING = 'OnEntityEditorActionExecuting';

//#endregion

//#region class BackendEntityEditor
// === Класс "Редактор сущности" ===
Quantumart.QP8.BackendEntityEditor = function(editorGroupCode, documentWrapperElementId, entityTypeCode, entityId, actionTypeCode, options) {
  Quantumart.QP8.BackendEntityEditor.initializeBase(this);

  this._editorGroupCode = editorGroupCode;
  this._entityTypeCode = entityTypeCode;
  this._entityId = entityId;
  this._parentEntityId = options.parentEntityId;
  this._actionCode = options.actionCode;
  this._actionTypeCode = actionTypeCode;
  this._documentWrapperElementId = documentWrapperElementId;
  if (!$q.isNull(options)) {
    if (options.formElementId) {
      this._formElementId = options.formElementId;
    }

    if (options.validationSummaryElementId) {
      this._validationSummaryElementId = options.validationSummaryElementId;
    }

    if (options.isWindow) {
      this._hostIsWindow = options.isWindow;
    }

    if (options.initFieldValues) {
      this._initFieldValues = options.initFieldValues;
    }

    if (options.disabledFields) {
      this._disabledFields = options.disabledFields;
    }

    if (options.hideFields) {
      this._hideFields = options.hideFields;
    }

    if (options.restoring === true) {
      this._restoring = true;
    }

    if (options.modifiedDateTime) {
      this._modifiedDateTime = options.modifiedDateTime;
    }

    if (options.contextQuery) {
      this._contextBlockState = JSON.parse(options.contextQuery);
    }

    if (options.needUp) {
      this._needUp = options.needUp;
    }

    if (options.customButtonsSettings) {
      this._customButtonsSettings = options.customButtonsSettings;
    }

    if (options.customLinkButtonsSettings) {
      this._customLinkButtonsSettings = options.customLinkButtonsSettings;
    }

    if (options.notifyCustomButtonExistence === false) {
      this._notifyCustomButtonExistence = options.notifyCustomButtonExistence;
    }

    if (options.customHandlers) {
      if (options.customHandlers.beforeSubmit) {
        this._customBeforeSubmit = options.customHandlers.beforeSubmit;
      }

      if (options.customHandlers.init) {
        this._customInit = options.customHandlers.init;
      }

      if (options.customHandlers.load) {
        this._customLoad = options.customHandlers.load;
      }

      if (options.customHandlers.dispose) {
        this._customDispose = options.customHandlers.dispose;
      }

      if (options.customHandlers.fieldValueChanged) {
        this._customFieldValueChanged = options.customHandlers.fieldValueChanged;
      }
    }

    this._entityTypeAllowedToAutosave = $q.toBoolean(options.entityTypeAllowedToAutosave, false);
    this._isBindToExternal = $q.toBoolean(options.isBindToExternal, false);
  }

  this._onActionExecutingHandler = jQuery.proxy(this._onActionExecuting, this);
  this._onHtmlInputChangedHandler = jQuery.proxy(this._onHtmlInputChanged, this);
  this._onBeforeSubmitHandler = jQuery.proxy(this._onBeforeSubmit, this);
  this._onSuccessHandler = jQuery.proxy(this._onSuccess, this);
  this._onErrorHandler = jQuery.proxy(this._onError, this);
  this._onLibraryFilesUploadedHandler = jQuery.proxy(this._onFileUploaded, this);

  this._customButtons = [];
  this._customLinkButtons = [];

  jQuery('#' + this._documentWrapperElementId).data(Quantumart.QP8.BackendEntityEditor.componentRefDataKey, this);
};

Quantumart.QP8.BackendEntityEditor.componentRefDataKey = 'component_ref';
Quantumart.QP8.BackendEntityEditor.getComponent = function(componentElem) {
  return jQuery(componentElem).data(Quantumart.QP8.BackendEntityEditor.componentRefDataKey);
};

Quantumart.QP8.BackendEntityEditor.prototype = {
  _editorGroupCode: '', // код группы редакторов
  _entityTypeCode: '', // код типа сущности
  _entityId: 0, // идентификатор сущности
  _parentEntityId: 0,
  _actionCode: '',
  _actionTypeCode: '', // код типа действия
  _documentWrapperElementId: '', // идентификатор DOM-элемента, образующего документ
  _formElementId: '', // идентификатор формы
  _formElement: null, // форма
  _validationSummaryElementId: '', // идентификатор списка ошибок
  _validationSummaryElement: null, // DOM-элемент, образующий список ошибок
  _formHasErrors: false, // признак, того что форма содержит ошибки
  _editorManagerComponent: null, // менеджер редакторов сущностей
  _hostIsWindow: false,
  _initFieldValues: null, // значения для инициализации полей
  _disabledFields: null, // идентификаторы полей который должны быть disable (массив имен полей)
  _hideFields: null, // идентификаторы полей которые должны быть скрыты (массив имен полей)
  _restoring: false, // документ восстановлен
  _contextBlockState: null,
  _variationsModel: null,
  _errorModel: null,
  _variationsModelChanged: false,
  _contextModel: null,
  _disableChangeTracking: false,
  _needUp: false, // вызван функционал Save & Up, можно пропустить инициализацию

  _modifiedDateTime: null,
  _isBindToExternal: false,
  _entityTypeAllowedToAutosave: false,

  _customBeforeSubmit: null,
  _customInit: null,
  _customDispose: null,
  _customFieldValueChanged: null,
  _customLoad: null,
  _notifyCustomButtonExistence: true,

  _customButtons: null,
  _customLinkButtons: null,
  _customButtonsSettings: null,
  _customLinkButtonsSettings: null,

  FIELD_SELECTORS: ':input',
  FIELD_VALUE_SELECTORS: ":input[name ^= 'field_']",
  FIELD_CHANGE_TRACK_SELECTORS: ':input:not(.qp-notChangeTrack)',
  CHANGED_FIELD_SELECTOR: '.changed',
  CHANGED_EXCEPT_VARMODEL_SELECTOR: '.changed:not(.variationModel)',
  REAL_CHANGED_FIELD_SELECTOR: '.' + CHANGED_FIELD_CLASS_NAME + ':not(.' + REFRESHED_FIELD_CLASS_NAME + ')',
  REAL_CHANGED_EXCEPT_VARMODEL_SELECTOR: '.' + CHANGED_FIELD_CLASS_NAME + ':not(.' + REFRESHED_FIELD_CLASS_NAME + '):not(.variationModel)',

  CONTEXT_SELECTOR: '.currentContext',
  CONTEXT_MODEL_SELECTOR: '.contextModel',
  VARIATION_SELECTOR: '.variationModel',
  VARIATION_INFO_SELECTOR: '.variationInfo',
  ERROR_SELECTOR: '.errorModel',

  OLD_CONTEXT_DATA_KEY: 'oldContext',

  get_editorGroupCode: function() {
    return this._editorGroupCode;
  },

  set_editorGroupCode: function(value) {
    this._editorGroupCode = value;
  },

  get_documentWrapperElementId: function() {
    return this._documentWrapperElementId;
  },

  set_documentWrapperElementId: function(value) {
    this._documentWrapperElementId = value;
  },

  get_entityTypeCode: function() {
    return this._entityTypeCode;
  },

  set_entityTypeCode: function(value) {
    this._entityTypeCode = value;
  },

  get_entityId: function() {
    return this._entityId;
  },

  set_entityId: function(value) {
    this._entityId = value;
  },

  get_actionTypeCode: function() {
    return this._actionTypeCode;
  },

  set_actionTypeCode: function(value) {
    this._actionTypeCode = value;
  },

  get_formElementId: function() {
    return this._formElementId;
  },

  set_formElementId: function(value) {
    this._formElementId = value;
  },

  get_validationSummaryElementId: function() {
    return this._validationSummaryElementId;
  },

  set_validationSummaryElementId: function(value) {
    this._validationSummaryElementId = value;
  },

  get_editorManagerComponent: function() {
    return this._editorManagerComponent;
  },

  set_editorManagerComponent: function(value) {
    this._editorManagerComponent = value;
  },

  get_fieldValues: function() {
    return $c.getAllFieldValues(this._formElement);
  },

  get_disabledFields: function() {
    return this._disabledFields;
  },

  get_hideFields: function() {
    return this._hideFields;
  },

  get_parentEntityId: function() {
    return this._parentEntityId;
  },

  get_actionCode: function() {
    return this._actionCode;
  },

  // Дата модификации сущности (ticks)
  get_modifiedDateTime: function() {
    return this._modifiedDateTime;
  },

  get_contextQuery: function() {
    if (this._contextBlockState)
    return JSON.stringify(this._contextBlockState);
    else
    return '';
  },

  allowAutoSave: function() {
    return this._entityTypeAllowedToAutosave && !this._isBindToExternal;
  },

  _onHtmlInputChangedHandler: null,
  _onActionExecutingHandler: null,
  _onLibraryFilesUploadedHandler: null,

  initialize: function() {
    if (!this._needUp) {
      var $form = null;
      var formElementId = this._formElementId;

      if ($q.isNullOrWhiteSpace(formElementId)) {
        $form = jQuery('#' + this._documentWrapperElementId + ' FORM:first');
        formElementId = $form.attr('id');
      }

      $form.ajaxForm({
        beforeSubmit: this._onBeforeSubmitHandler,
        success: this._onSuccessHandler,
        error: this._onErrorHandler,
        iframe: false
      }
  );

      this._formElementId = formElementId;
      this._formElement = $form.get(0);

      $form = null;

      if (!$q.isNullOrWhiteSpace(this._validationSummaryElementId)) {
        var $validationSummary = jQuery('#' + this._validationSummaryElementId);

        this._formHasErrors = $validationSummary.length > 0 && $validationSummary.find('UL').length > 0;
        $validationSummary = null;
      }

      if (this._formHasErrors) {
        this._initFieldValues = null;
      }

      this._initAllFields();
      this._initAllFieldDescriptons();
    }
  },

  getFields: function() {
    var $fields = jQuery(this._formElement).find(this.FIELD_SELECTORS);

    return $fields;
  },

  _initAllFields: function() {
    if (this._formElement) {
      var $form = jQuery(this._formElement);

      $c.initAllClassifierFields($form, this._onActionExecutingHandler, {
        hostIsWindow: this._hostIsWindow,
        initFieldValues: this._initFieldValues,
        disabledFields: this._disabledFields,
        hideFields: this._hideFields,
        customInit: this._customInit,
        customDispose: this._customDispose,
        parentEditor: this,
        customButtonsSettings: this._customButtonsSettings,
        customLinkButtonsSettings: this._customLinkButtonsSettings
      }, jQuery.proxy(function(eventType, sender, eventArgs) {
          this.notify(eventType, eventArgs);
          if (eventArgs.toggleDisableChangeTracking) {
            if (eventType === EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_UNLOADING) {
              this._disableChangeTracking = true;
            } else if (eventType === EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED) {
              this._disableChangeTracking = false;
            }
          } else {
            if (eventType === EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED && this._editorManagerComponent) {
              this._editorManagerComponent.onAllFieldInvalidate(this._documentWrapperElementId);
            }
          }
        }, this)
      );

      $c.setAllVisualEditorValues($form, this._initFieldValues);
      $c.makeReadonlyVisualEditors($form, this._disabledFields);

      $c.initAllDateTimePickers($form);
      $c.initAllVisualEditors($form);
      $c.initAllNumericTextBoxes($form);
      $c.initAllFileFields($form, this._onLibraryFilesUploadedHandler);
      $c.initAllEntityDataLists($form, this._onActionExecutingHandler, { hostIsWindow: this._hostIsWindow });
      $c.initAllEntityDataTrees($form);
      $c.initAllAggregationLists($form);
      $c.initAllWorkflows($form);
      $form
        .on('change paste', this.FIELD_CHANGE_TRACK_SELECTORS, this._onHtmlInputChangedHandler)
        .on(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, this.CHANGED_FIELD_SELECTOR, jQuery.proxy(this._onFieldValueChanged, this));

      $c.setAllSimpleTextBoxValues($form, this._initFieldValues);
      $c.setAllBooleanValues($form, this._initFieldValues);
      $c.setAllNumericBoxValues($form, this._initFieldValues);
      $c.setAllDateTimePickersValues($form, this._initFieldValues);
      $c.setAllRadioListValues($form, this._initFieldValues);
      $c.setAllAggregationListValues($form, this._initFieldValues);

      $c.initAllCheckboxToggles($form);
      $c.initAllSwitcherLists($form);

      $c.makeReadonlySimpleTextBoxes($form, this._disabledFields);
      $c.makeReadonlyBooleans($form, this._disabledFields);
      $c.makeReadonlyNumericBox($form, this._disabledFields);
      $c.makeReadonlyDateTimePickers($form, this._disabledFields);
      $c.makeReadonlyFileFields($form, this._disabledFields);
      $c.makeReadonlyRadioList($form, this._disabledFields);

      this._initVariationsModelField($form);
      this._initVariationInfo($form);
      this._initContextModelField($form);
      this._initErrorModelField($form);

      if (this._customInit) {
        this._customInit(this, $form);
      }

      var self = this;

      if (this._customButtonsSettings) jQuery.each(this._customButtonsSettings, function(index, item) {
        self.addCustomButton(item);
      });

      if (this._customLinkButtonsSettings) jQuery.each(this._customLinkButtonsSettings, function(index, item) {
        self.addCustomLinkButton(item);
      });

      $form = null;
    }
  },

  _initAllFieldDescriptons: function() {
    jQuery('span[data-field_description_text]', this._formElement).qtip({
      style: 'qtip-light',
      content: {
        attr: 'data-field_description_text'
      },
      hide: {
        fixed: true,
        delay: 300
      },
      show: {
        solo: true,
        event: 'click',
        delay: 0
      }
    });
  },

  _disposeAllFieldDescriptons: function() {
    jQuery('span[data-tooltip]').qtip('destroy', true);
  },

  _disposeAllFields: function() {
    if (this._formElement && !this._needUp) {
      var $form = jQuery(this._formElement);

      if (this._customDispose) {
        this._customDispose(this, $form);
      }

      $c.destroyAllCheckboxToggles($form);
      $c.destroyAllSwitcherLists($form);
      $c.destroyAllDateTimePickers($form);
      $c.destroyAllVisualEditors($form);
      $c.destroyAllNumericTextBoxes($form);
      $c.destroyAllFileFields($form);
      $c.destroyAllEntityDataLists($form);
      $c.destroyAllEntityDataTrees($form);
      $c.destroyAllAggregationLists($form);
      $c.destroyAllHighlightedTextAreas($form);
      Quantumart.QP8.BackendExpandedContainer.destroyAll($form);
      $c.destroyAllClassifierFields($form);

      $form
        .off('change paste', this.FIELD_CHANGE_TRACK_SELECTORS)
        .off(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, this.CHANGED_FIELD_SELECTOR);

      this._disposeContextModelField($form);
      this._disposeVariationInfo($form);
      this._disposeVariationsModelField($form);
      this._disposeErrorModelField($form);

      $form = null;
    }
  },

  _initVariationsModelField: function($form) {
    var $modelElem = jQuery(this.VARIATION_SELECTOR, $form);

    if ($modelElem.length == 1) {
      var model = {};

      jQuery.each(JSON.parse($modelElem.val()), function() {
        model[this.Context] = { Id: this.Id, FieldValues: this.FieldValues };
      });

      this._variationsModel = model;
      this._variationsModelChanged = false;
    }
  },

  _disposeVariationsModelField: function() {
    this._variationsModel = null;
  },

  _initVariationInfo: function($form) {
    var $infoElem = jQuery(this.VARIATION_INFO_SELECTOR, $form);

    $infoElem.find('.removeVariation').on('click', jQuery.proxy(this._onRemoveVariation, this));
    $infoElem.find('.currentInfo').html($l.EntityEditor.baseArticle);
    $infoElem.find('.currentTotal').html(this._getVariationModelCount());
  },

  _disposeVariationInfo: function($form) {
    var $infoElem = jQuery(this.VARIATION_INFO_SELECTOR, $form);

    $infoElem.find('.removeVariation').off('click');
  },

  _onRemoveVariation: function() {
    var $form = jQuery(this._formElement);
    var contextValue = jQuery(this.CONTEXT_SELECTOR, $form).val();

    $q.removeProperty(this._variationsModel, contextValue);
    this._variationsModelChanged = true;
    this._saveVariationsModelData($form);
    this._restoreFromVariationsModel(contextValue);
    this._updateVariationInfo(contextValue);
  },

  _updateVariationInfo: function(contextValue) {
    var model = this._variationsModel[contextValue];
    var exists = !!model && !!contextValue;
    var isNew = model && model.Id === 0;
    var $infoElem = jQuery(this.VARIATION_INFO_SELECTOR, this._formElement);

    var message = '';

    if (contextValue == '') {
      message = $l.EntityEditor.baseArticle;
    } else if (!exists) {
      message = $l.EntityEditor.variationNotExists;
    } else if (isNew) {
      message = $l.EntityEditor.newVariationExists;
    } else {
      message = String.format($l.EntityEditor.variationExists, model.Id);
    }

    $infoElem.find('.currentInfo').html(message);
    $infoElem.find('.currentTotal').html(this._getVariationModelCount());
    $infoElem.find('.removeItem').toggle(exists);
  },

  _getVariationModelCount: function() {
    var count = 0;

    for (k in this._variationsModel) if (k && this._variationsModel.hasOwnProperty(k)) count++;
    return count;
  },

  _initContextModelField: function($form) {
    var $modelElem = jQuery(this.CONTEXT_MODEL_SELECTOR, $form);

    if ($modelElem.length == 1) {
      this._contextModel = JSON.parse($modelElem.val());
    }
  },

  _disposeContextModelField: function() {
    this._contextModel = null;
  },

  _initErrorModelField: function($form) {
    var $modelElem = jQuery(this.ERROR_SELECTOR, $form);

    if ($modelElem.length == 1) {
      var model = {};

      jQuery.each(JSON.parse($modelElem.val()), function() {
        model[this.Context] = this.Errors;
      });

      this._errorModel = model;
    }
  },

  _disposeErrorModelField: function() {
    this._errorModel = null;
  },

  _saveVariationsModelData: function($form) {
    if (this._variationsModelChanged) {
      var $modelElem = jQuery(this.VARIATION_SELECTOR, $form);

      if ($modelElem.length == 1) {
        var model = [];

        for (var prop in this._variationsModel) {
          var currentItem = this._variationsModel[prop];

          model.push({ Context: prop, Id: currentItem.Id, FieldValues: currentItem.FieldValues });
        }

        $modelElem.val(JSON.stringify(model)).change();
        this._variationsModelChanged = false;
      }
    }
  },

  _copyCurrentDataToVariationsModel: function($form) {
    var $contextElem = jQuery(this.CONTEXT_SELECTOR, $form);

    if ($contextElem.length == 1) {
      $c.saveDataOfAllVisualEditors(this._formElement);
      this._copyToVariationsModel($contextElem.val());
    }
  },

  isFieldsValid: function() {
    return !this._formHasErrors;
  },

  isFieldsChanged: function() {
    var $changedFields = jQuery(this._formElement).find(this.REAL_CHANGED_FIELD_SELECTOR);

    return ($changedFields.length > 0);
  },

  isFieldValuesChanged: function() {
    var $changedFields = jQuery(this._formElement)
      .find(this.VARIATION_SELECTOR)
      .closest('fieldset')
      .find(this.REAL_CHANGED_EXCEPT_VARMODEL_SELECTOR);

    return ($changedFields.length > 0);
  },

  addCustomButton: function(settings, $parent) {
    if (!settings.name && !settings.onClick && !settings.suffix && !settings.title)
    alert('One of the required settings is missed: name, title, suffix, onClick');
    else {
      var defaultSettings =
      {
        'class': 'customButton'
      };

      settings = jQuery.extend(defaultSettings, settings);
      var $form = $parent || jQuery(this._formElement);
      var $input = $form.find("[name='" + settings.name + "']");

      if ($input.length == 0) {
        if (this._notifyCustomButtonExistence) alert('Input ' + settings.name + ' is not found');
        return;
      }

      var et = $input.data('exact_type');
      var sTypes = [FILE_FIELD_TYPE, IMAGE_FIELD_TYPE];

      if (!et && ($input.prop('type') != 'text' || $input.parents('.t-numerictextbox').length > 0) || jQuery.inArray(et, sTypes) == -1) {
        alert('Input ' + settings.name + ' type is not supported');
        return;
      }

      var $fieldWrapper = $input.parent('.fieldWrapper');

      if ($fieldWrapper.length == 0) {
        $input.wrap($('<div/>', { id: $input.prop('id') + '_wrapper', 'class': 'fieldWrapper group myClass' }));
        $fieldWrapper = $input.parent('.fieldWrapper');
      }

      var options = {
        id: $input.prop('id') + '_' + settings.suffix,
        'class': settings['class'],
        title: settings.title
      };

      if ($fieldWrapper.find('#' + options.id).length > 0) {
        if (this._notifyCustomButtonExistence) alert('Button ' + options.id + ' already exists');
        return;
      }

      var $div = $('<div/>', options);

      $div.append($('<img/>', { src: '/Backend/Content/Common/0.gif' }));
      if (settings.url) {
        $div.css({ 'background-image': 'url(' + settings.url + ')', 'background-color': 'transparent' });
      }

      $fieldWrapper.append($div);
      var onClick = function() {
    settings.onClick.call(this, $input, $form);
  };

      $div.on('click', { $input: $input, $form: $form, settings: settings }, settings.onClick);

      this._customButtons.push(options.id);
    }
  },

  addCustomLinkButton: function(settings, $parent) {
    if (!settings.name && !settings.onClick && !settings.suffix && !settings.title)
    alert('One of the required settings is missed: name, title, suffix, onClick');
    else {
      var defaultSettings =
      {
        'class': 'pick'
      };

      settings = jQuery.extend(defaultSettings, settings);
      $form = $parent || jQuery(this._formElement);
      var $input = $form.find("[name='" + settings.name + "']");

      if ($input.length == 0) {
        if (this._notifyCustomButtonExistence) {
          alert('Input ' + settings.name + ' is not found');
        }

        return;
      }

      var et = $input.data('exact_type');
      var sTypes = [STRING_FIELD_TYPE, FILE_FIELD_TYPE, IMAGE_FIELD_TYPE, TEXTBOX_FIELD_TYPE, VISUAL_EDIT_FIELD_TYPE];

      if (jQuery.inArray(et, sTypes) == -1) {
        alert('Input ' + settings.name + ' type is not supported');
        return;
      }

      var $fieldWrapper = $input.parent('.fieldWrapper');

      if ($fieldWrapper.length == 0) {
        $input.wrap($('<div/>', { 'class': 'fieldWrapper group myClass' }));
        $fieldWrapper = $input.parent('.fieldWrapper');
      }

      var $ul = $fieldWrapper.find('ul.linkButtons');

      if ($ul.length == 0) {
        $fieldWrapper.prepend('<ul class="linkButtons group" style="display: block;" />');
        $ul = $fieldWrapper.find('ul.linkButtons');
      }

      var options = {
        id: $input.prop('id') + '_' + settings.suffix,
        'class': settings['class'],
        title: settings.title
      };

      if ($ul.find('#' + options.id).length > 0) {
        if (this._notifyCustomButtonExistence) alert('Button ' + options.id + ' already exists');
        return;
      }

      var builder = new $.telerik.stringBuilder();

      builder
        .cat('<li><span id="' + options.id + '" class="linkButton actionLink">')
        .cat('<a href="javascript:void(0);">')
        .cat('<span class="icon ' + options['class'] + '"><img src="/Backend/Content/Common/0.gif"></span>')
        .cat('<span class="text">' + options.title + '</span>')
        .cat('</a></span></li>');

      var $li = jQuery(builder.string());

      if (settings.url) {
        $li.find('.icon').css({ 'background-image': 'url(' + settings.url + ')' });
      }

      $ul.append($li);
      var onClick = function() {
        settings.onClick.call(this, $input, $form);
      };

      $li.find('a').on('click', { $input: $input, $form: $form, settings: settings }, settings.onClick);
      this._customLinkButtons.push(options.id);
    }
  },

  saveEntity: function(actionCode) {
    $c.saveDataOfAllVisualEditors(this._formElement);
    $c.SaveDataOfAllHighlightedTextAreas(this._formElement);
    $c.saveDataOfAllAggregationLists(this._formElement);
    $c.saveDataOfAllWorkflows(this._formElement);

    if (this._entityTypeCode !== ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      if (!this.isFieldsChanged()) {
        alert($l.EntityEditor.fieldsNotChangedMessage);
        return false;
      }
    }

    this._copyCurrentDataToVariationsModel($form);
    this._saveVariationsModelData($form);

    var $form = jQuery(this._formElement);

    $form.find('input[name="' + BACKEND_ACTION_CODE_HIDDEN_NAME + '"]').val(actionCode);
    $form.trigger('submit');
  },

  refreshEditor: function(options) {
    var message = (options && options.confirmMessageText) ? options.confirmMessageText : $l.EntityEditor.autoRefreshConfirmMessage;

    if (this._confirmAction(message)) {
      var eventArgs = new Quantumart.QP8.BackendEventArgs();

      this.notify(EVENT_TYPE_ENTITY_EDITOR_ENTITY_REFRESH_STARTING, eventArgs);
      eventArgs = null;
    }
  },

  fixEntityDataListsOverflow: function() {
    if (this._formElement) {
      var $form = jQuery(this._formElement);

      $c.fixAllEntityDataListsOverflow($form);
      $form = null;
    }
  },

  fixHtaInitializing: function() {
    if (this._formElement) {
      var $form = jQuery(this._formElement);

      $c._refreshAllHta($form);
      $form = null;
    }
  },

  confirmRefresh: function() {
    return this._confirmAction($l.EntityEditor.refreshConfirmMessage);
  },

  confirmChange: function() {
    return this._confirmAction($l.EntityEditor.changeConfirmMessage);
  },

  confirmClose: function() {
    return this._confirmAction($l.EntityEditor.closeConfirmMessage);
  },

  _confirmAction: function(message) {
    return (!this.isFieldsChanged() || confirm(message));
  },

  onLoad: function() {
    var $form = jQuery(this._formElement);

    $c.setAllEntityDataListValues($form, this._initFieldValues);
    this.fixEntityDataListsOverflow();
    $c.makeReadonlyEntityDataList($form, this._disabledFields);

    $c.setAllClassifierFieldValues($form, this._initFieldValues);
    $c.makeReadonlyClassifierFields($form, this._disabledFields);

    $c.initAllHighlightedTextAreas($form);
    $c.setAllHighlightedTextAreaValues($form, this._initFieldValues);

    Quantumart.QP8.BackendExpandedContainer.initAll($form);

    $c.setFieldRowsVisibility($form, this._hideFields, false);

    if (this._customLoad)
    this._customLoad($form);

    if (this._contextBlockState) {
      this.applyContext(this._contextBlockState);
    }

    this._initFieldValues = null;

    var $wrapper = jQuery('#' + this._documentWrapperElementId);

    $wrapper.scrollTop((this._formHasErrors) ? 0 : $q.toInt($wrapper.data('scroll_position'), 0));
    $wrapper = null;

    $form = null;

    this._editorManagerComponent.onEntityEditorReady(this._documentWrapperElementId);
  },

  onSelect: function() {
    this.fixEntityDataListsOverflow();
    this.fixHtaInitializing();
  },

  _onActionExecuting: function(eventType, sender, eventArgs) {
    this.notify(EVENT_TYPE_ENTITY_EDITOR_ACTION_EXECUTING, eventArgs);
  },

  _onHtmlInputChanged: function(e) {
    if (!this._disableChangeTracking) {
      var $field = jQuery(e.currentTarget);

      $field.addClass(CHANGED_FIELD_CLASS_NAME);
      $field.removeClass(REFRESHED_FIELD_CLASS_NAME);

      var value;

      if ($field.is(':checkbox')) {
        value = $field.is(':checked');
      } else {
        value = $field.val();
      }

      $field.trigger(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { fieldName: $field.attr('name'), value: value });
      $field = null;
    }
  },

  _onFieldValueChanged: function(e, data) {
    if (!this._disableChangeTracking) {
      if (data && data.fieldName) {
        if (this._customFieldValueChanged)
        this._customFieldValueChanged(this, data);
        this._editorManagerComponent.onFieldValueChanged(jQuery.extend({ documentWrapperElementId: this._documentWrapperElementId }, data));
      }
    }
  },
  _onBeforeSubmit: function() {
    var $wrapper = jQuery('#' + this._documentWrapperElementId);

    $wrapper.data('scroll_position', $wrapper.scrollTop());
    $wrapper = null;

    if (this._customBeforeSubmit && !this._customBeforeSubmit(this)) {
      return false;
    }

    var eventArgs = new Quantumart.QP8.BackendEntityEditorActionEventArgs();

    this.notify(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTING, eventArgs);
    eventArgs = null;

    return true;
  },
  _onSuccess: function(data) {
    var eventArgs = new Quantumart.QP8.BackendEntityEditorActionEventArgs();

    eventArgs.set_data(data);
    this.notify(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED, eventArgs);
    eventArgs = null;
  },
  _onError: function(jqXHR) {
    $q.processGenericAjaxError(jqXHR);
    var eventArgs = new Quantumart.QP8.BackendEntityEditorActionEventArgs();

    this.notify(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED_ERROR, eventArgs);
    eventArgs = null;
  },

  _onFileUploaded: function(eventType, sender, eventArgs) {
    this.notify(EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED, eventArgs);
  },

  _onChangeContext: function() {
    var $form = jQuery(this._formElement);
    var oldContextValue = $form.data(this.OLD_CONTEXT_DATA_KEY);
    var newContextValue = jQuery(this.CONTEXT_SELECTOR, $form).val();

    if (oldContextValue != newContextValue) {
      if (typeof oldContextValue !== 'undefined') {
        this._copyToVariationsModel(oldContextValue);
        this._saveVariationsModelData($form);
      }

      if (this._initFieldValues == null) {
        this._restoreFromVariationsModel(newContextValue);
      }

      this._updateVariationInfo(newContextValue);
      $form.data(this.OLD_CONTEXT_DATA_KEY, newContextValue);
    }
  },
  _copyToVariationsModel: function(contextValue) {
    if (this.isFieldValuesChanged()) {
      var result = {};

      jQuery.each(this.get_fieldValues(), function() {
        if (this.fieldName.match(/^field_[\d]+$/)) {
          result[this.fieldName] = (this.value === null) ? '' : this.value + '';
        }
      });

      if (!this._variationsModel[contextValue]) {
        this._variationsModel[contextValue] = { Id: 0 };
      }

      this._variationsModel[contextValue].FieldValues = result;
      this._variationsModelChanged = true;
    }
  },

  _getActualContext: function(contextValue) {
    if (this._contextModel && this._variationsModel) {
      if (contextValue in this._variationsModel) {
        return contextValue;
      } else {
        var contextValues = contextValue.split(',');
        var found = false;

        while (!found && contextValues.length > 0) {
          var lastIndex = contextValues.length - 1;
          var lastValue = contextValues[lastIndex];
          var parentValue = this._contextModel[lastIndex].Ids[lastValue];

          if (parentValue == '0') {
            contextValues.length--;
          } else {
            contextValues[lastIndex] = parentValue;
          }

          var testContextValue = contextValues.join();

          found = testContextValue in this._variationsModel;
        }

        return testContextValue;
      }
    } else
    return '';
  },

  _restoreFromVariationsModel: function(contextValue) {
    this._disableChangeTracking = true;
    var actualContextValue = this._getActualContext(contextValue);
    var fieldValues = this._variationsModel[actualContextValue].FieldValues;
    var errors = this._errorModel[actualContextValue];
    var initFieldValues = [];

    jQuery.each(fieldValues, function(key, value) {
      var currentResult = { fieldName: key, value: value };

      if (!$q.isNullOrEmpty(errors)) {
        currentResult.errors = jQuery.grep(errors, function(elem) {
          return elem.Name == key;
        });
      }

      initFieldValues.push(currentResult);
    });

    var $form = jQuery(this._formElement);

    $c.setAllEntityDataListValues($form, initFieldValues);
    this.fixEntityDataListsOverflow();

    $c.setAllClassifierFieldValues($form, initFieldValues, true);
    $c.setAllVisualEditorValues($form, initFieldValues);
    $c.setAllSimpleTextBoxValues($form, initFieldValues);
    $c.setAllBooleanValues($form, initFieldValues);
    $c.setAllNumericBoxValues($form, initFieldValues);
    $c.setAllDateTimePickersValues($form, initFieldValues);
    $c.setAllRadioListValues($form, initFieldValues);
    $c.setAllAggregationListValues($form, initFieldValues);

    this._disableChangeTracking = false;
    jQuery(this.VARIATION_SELECTOR, $form).closest('fieldset').find(this.CHANGED_EXCEPT_VARMODEL_SELECTOR).removeClass(CHANGED_FIELD_CLASS_NAME);
  },

  applyContext: function(contextState) {
    this._contextBlockState = contextState;
    var result = [];

    jQuery.each(contextState, function() {
      if (this.Value) {
        result.push(this.Value);
      }
    });

    jQuery(this.CONTEXT_SELECTOR, this._formElement).val(result.join());
    this._onChangeContext();
  },

  dispose: function() {
    Quantumart.QP8.BackendEntityEditor.callBaseMethod(this, 'dispose');
    this._disposeAllFieldDescriptons();
    this._disposeAllFields();
    if (this._editorManagerComponent) {
      var editorCode = this._editorCode;

      if (!$q.isNullOrWhiteSpace(editorCode)) {
        this._editorManagerComponent.removeEditor(editorCode);
      }

      this._editorManagerComponent.onEntityEditorDisposed(this._documentWrapperElementId);
      this._editorManagerComponent = null;
    }

    jQuery.each(this._customButtons, function(index, value) {
      jQuery('#' + value).off('click');
    });

    jQuery.each(this._customLinkButtons, function(index, value) {
      jQuery('#' + value).find('a').off('click');
    });

    jQuery('#' + this._documentWrapperElementId).removeData();

    this._validationSummaryElement = null;
    this._formElement = null;

    this._onActionExecutingHandler = null;
    this._onHtmlInputChangedHandler = null;
    this._onBeforeSubmitHandler = null;
    this._onSuccessHandler = null;
    this._onErrorHandler = null;

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEntityEditor.registerClass('Quantumart.QP8.BackendEntityEditor', Quantumart.QP8.Observable);

//#endregion

//#region class BackendEntityEditorActionEventArgs
// === Класс "Аргументы события, вызванного редактором сущности" ===
Quantumart.QP8.BackendEntityEditorActionEventArgs = function() {
  Quantumart.QP8.BackendEntityEditorActionEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendEntityEditorActionEventArgs.prototype = {
  _data: null, // данные, в формате JSON

  get_data: function() {
    return this._data;
  },

  set_data: function(value) {
    this._data = value;
  }
};

Quantumart.QP8.BackendEntityEditorActionEventArgs.registerClass('Quantumart.QP8.BackendEntityEditorActionEventArgs', Quantumart.QP8.BackendEventArgs);

//#endregion
