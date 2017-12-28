/* eslint max-lines: 'off' */
import { BackendEntityEditorActionEventArgs } from './BackendEntityEditorActionEventArgs';
import { BackendEventArgs } from '../Common/BackendEventArgs';
import { BackendExpandedContainer } from '../BackendExpandedContainer';
import { Observable } from '../Common/Observable';
import { $c } from '../ControlHelpers';
import { $q } from '../Utils';


window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTING = 'OnEntityEditorSubmitting';
window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED = 'OnEntityEditorSubmitted';
window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED_ERROR = 'OnEntityEditorSubmittedError';
window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_REFRESH_STARTING = 'OnEntityEditorRefreshStarting';
window.EVENT_TYPE_ENTITY_EDITOR_ACTION_EXECUTING = 'OnEntityEditorActionExecuting';

export class BackendEntityEditor extends Observable {
  static getComponent(componentElem) {
    return $(componentElem).data(BackendEntityEditor.componentRefDataKey);
  }

  static get componentRefDataKey() {
    return 'component_ref';
  }

  // eslint-disable-next-line max-statements, complexity, max-params
  constructor(
    editorGroupCode,
    documentWrapperElementId,
    entityTypeCode,
    entityId,
    actionTypeCode,
    options
  ) {
    super();

    this.FIELD_SELECTORS = ':input';
    this.FIELD_VALUE_SELECTORS = ":input[name ^= 'field_']";
    this.FIELD_CHANGE_TRACK_SELECTORS = ':input:not(.qp-notChangeTrack)';
    this.CHANGED_FIELD_SELECTOR = '.changed';
    this.CHANGED_EXCEPT_VARMODEL_SELECTOR = '.changed:not(.variationModel)';
    this.REAL_CHANGED_FIELD_SELECTOR = `.${window.CHANGED_FIELD_CLASS_NAME}:not(.${window.REFRESHED_FIELD_CLASS_NAME})`;
    this.REAL_CHANGED_EXCEPT_VARMODEL_SELECTOR
      = `.${window.CHANGED_FIELD_CLASS_NAME}:not(.${window.REFRESHED_FIELD_CLASS_NAME}):not(.variationModel)`;

    this.CONTEXT_SELECTOR = '.currentContext';
    this.CONTEXT_MODEL_SELECTOR = '.contextModel';
    this.VARIATION_SELECTOR = '.variationModel';
    this.VARIATION_INFO_SELECTOR = '.variationInfo';
    this.ERROR_SELECTOR = '.errorModel';
    this.OLD_CONTEXT_DATA_KEY = 'oldContext';

    this.editorGroupCode = '';
    this.entityTypeCode = '';
    this.entityId = 0;
    this.parentEntityId = 0;
    this.actionCode = '';
    this.actionTypeCode = '';
    this.documentWrapperElementId = '';
    this.formElementId = '';
    this.validationSummaryElementId = '';
    this.formHasErrors = false;
    this.hostIsWindow = false;
    this.restoring = false;
    this.variationsModelChanged = false;
    this.disableChangeTracking = false;
    this.needUp = false;

    this.isBindToExternal = false;
    this.entityTypeAllowedToAutosave = false;

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

      if (options.restoring) {
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

      if (!options.notifyCustomButtonExistence) {
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

    this._onActionExecutingHandler = $.proxy(this._onActionExecuting, this);
    this._onHtmlInputChangedHandler = $.proxy(this._onHtmlInputChanged, this);
    this._onBeforeSubmitHandler = $.proxy(this._onBeforeSubmit, this);
    this._onSuccessHandler = $.proxy(this._onSuccess, this);
    this._onErrorHandler = $.proxy(this._onError, this);
    this._onLibraryFilesUploadedHandler = $.proxy(this._onFileUploaded, this);

    this._customButtons = [];
    this._customLinkButtons = [];

    $(`#${this._documentWrapperElementId}`).data(BackendEntityEditor.componentRefDataKey, this);
  }

  // eslint-disable-next-line camelcase
  get_editorGroupCode() {
    return this._editorGroupCode;
  }

  // eslint-disable-next-line camelcase
  set_editorGroupCode(value) {
    this._editorGroupCode = value;
  }

  // eslint-disable-next-line camelcase
  get_documentWrapperElementId() {
    return this._documentWrapperElementId;
  }

  // eslint-disable-next-line camelcase
  set_documentWrapperElementId(value) {
    this._documentWrapperElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_entityTypeCode() {
    return this._entityTypeCode;
  }

  // eslint-disable-next-line camelcase
  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  }

  // eslint-disable-next-line camelcase
  get_entityId() {
    return this._entityId;
  }

  // eslint-disable-next-line camelcase
  set_entityId(value) {
    this._entityId = value;
  }

  // eslint-disable-next-line camelcase
  get_actionTypeCode() {
    return this._actionTypeCode;
  }

  // eslint-disable-next-line camelcase
  set_actionTypeCode(value) {
    this._actionTypeCode = value;
  }

  // eslint-disable-next-line camelcase
  get_formElementId() {
    return this._formElementId;
  }

  // eslint-disable-next-line camelcase
  set_formElementId(value) {
    this._formElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_validationSummaryElementId() {
    return this._validationSummaryElementId;
  }

  // eslint-disable-next-line camelcase
  set_validationSummaryElementId(value) {
    this._validationSummaryElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_editorManagerComponent() {
    return this._editorManagerComponent;
  }

  // eslint-disable-next-line camelcase
  set_editorManagerComponent(value) {
    this._editorManagerComponent = value;
  }

  // eslint-disable-next-line camelcase
  get_fieldValues() {
    return $c.getAllFieldValues(this._formElement);
  }

  // eslint-disable-next-line camelcase
  get_disabledFields() {
    return this._disabledFields;
  }

  // eslint-disable-next-line camelcase
  get_hideFields() {
    return this._hideFields;
  }

  // eslint-disable-next-line camelcase
  get_parentEntityId() {
    return this._parentEntityId;
  }

  // eslint-disable-next-line camelcase
  get_actionCode() {
    return this._actionCode;
  }

  // eslint-disable-next-line camelcase
  get_modifiedDateTime() {
    return this._modifiedDateTime;
  }

  // eslint-disable-next-line camelcase
  get_contextQuery() {
    if (this._contextBlockState) {
      return JSON.stringify(this._contextBlockState);
    }

    return '';
  }

  allowAutoSave() {
    return this._entityTypeAllowedToAutosave && !this._isBindToExternal;
  }

  initialize() {
    if (!this._needUp) {
      let $form = null;
      let formElementId = this._formElementId;

      if ($q.isNullOrWhiteSpace(formElementId)) {
        $form = $(`#${this._documentWrapperElementId} FORM:first`);
        formElementId = $form.attr('id');
      }

      $form.ajaxForm({
        beforeSubmit: this._onBeforeSubmitHandler,
        success: this._onSuccessHandler,
        error: this._onErrorHandler,
        iframe: false
      });

      this._formElementId = formElementId;
      this._formElement = $form.get(0);
      if (!$q.isNullOrWhiteSpace(this._validationSummaryElementId)) {
        const $validationSummary = $(`#${this._validationSummaryElementId}`);
        this._formHasErrors = $validationSummary.length > 0 && $validationSummary.find('UL').length > 0;
      }

      if (this._formHasErrors) {
        this._initFieldValues = null;
      }

      this._initAllFields();
      this._initAllFieldDescriptons();
    }
  }

  getFields() {
    return $(this._formElement).find(this.FIELD_SELECTORS);
  }

  // eslint-disable-next-line max-statements
  _initAllFields() {
    if (this._formElement) {
      const $form = $(this._formElement);
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
      }, $.proxy(function (eventType, sender, eventArgs) {
        this.notify(eventType, eventArgs);
        if (eventArgs.toggleDisableChangeTracking) {
          if (eventType === window.EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_UNLOADING) {
            this._disableChangeTracking = true;
          } else if (eventType === window.EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED) {
            this._disableChangeTracking = false;
          }
        } else if (eventType === window.EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED && this._editorManagerComponent) {
          this._editorManagerComponent.onAllFieldInvalidate(this._documentWrapperElementId);
        }
      }, this));

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

      const _onFieldValueChangedProxy = $.proxy(this._onFieldValueChanged, this);
      $form
        .on('change paste', this.FIELD_CHANGE_TRACK_SELECTORS, this._onHtmlInputChangedHandler)
        .on(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, this.CHANGED_FIELD_SELECTOR, _onFieldValueChangedProxy);

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

      const that = this;
      if (this._customButtonsSettings) {
        $.each(this._customButtonsSettings, (index, item) => {
          that.addCustomButton(item);
        });
      }

      if (this._customLinkButtonsSettings) {
        $.each(this._customLinkButtonsSettings, (index, item) => {
          that.addCustomLinkButton(item);
        });
      }
    }
  }

  _initAllFieldDescriptons() {
    $('span[data-field_description_text]', this._formElement).qtip({
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
  }

  _disposeAllFields() {
    if (this._formElement && !this._needUp) {
      const $form = $(this._formElement);
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

      BackendExpandedContainer.destroyAll($form);
      $c.destroyAllClassifierFields($form);

      $form
        .off('change paste', this.FIELD_CHANGE_TRACK_SELECTORS)
        .off(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, this.CHANGED_FIELD_SELECTOR);

      this._disposeContextModelField();
      this._disposeVariationInfo($form);
      this._disposeVariationsModelField();
      this._disposeErrorModelField();
    }
  }

  _initVariationsModelField($form) {
    const $modelElem = $(this.VARIATION_SELECTOR, $form);
    if ($modelElem.length === 1) {
      const model = {};
      $.each(JSON.parse($modelElem.val()), function () {
        model[this.Context] = { Id: this.Id, FieldValues: this.FieldValues };
      });

      this._variationsModel = model;
      this._variationsModelChanged = false;
    }
  }

  _disposeVariationsModelField() {
    this._variationsModel = null;
  }

  _initVariationInfo($form) {
    const $infoElem = $(this.VARIATION_INFO_SELECTOR, $form);
    $infoElem.find('.removeVariation').on('click', $.proxy(this._onRemoveVariation, this));
    $infoElem.find('.currentInfo').html($l.EntityEditor.baseArticle);
    $infoElem.find('.currentTotal').html(String(this._getVariationModelCount()));
  }

  _disposeVariationInfo($form) {
    const $infoElem = $(this.VARIATION_INFO_SELECTOR, $form);
    $infoElem.find('.removeVariation').off('click');
  }

  _onRemoveVariation() {
    const $form = $(this._formElement);
    const contextValue = $(this.CONTEXT_SELECTOR, $form).val();

    $q.removeProperty(this._variationsModel, contextValue);
    this._variationsModelChanged = true;
    this._saveVariationsModelData($form);
    this._restoreFromVariationsModel(contextValue);
    this._updateVariationInfo(contextValue);
  }

  _updateVariationInfo(contextValue) {
    const model = this._variationsModel[contextValue];
    const exists = !!model && !!contextValue;
    const isNew = model && model.Id === 0;
    const $infoElem = $(this.VARIATION_INFO_SELECTOR, this._formElement);

    let message = '';
    if (contextValue === '') {
      message = $l.EntityEditor.baseArticle;
    } else if (!exists) {
      message = $l.EntityEditor.variationNotExists;
    } else if (isNew) {
      message = $l.EntityEditor.newVariationExists;
    } else {
      message = String.format($l.EntityEditor.variationExists, model.Id);
    }

    $infoElem.find('.currentInfo').html(message);
    $infoElem.find('.currentTotal').html(String(this._getVariationModelCount()));
    $infoElem.find('.removeItem').toggle(exists);
  }

  _getVariationModelCount() {
    return this._variationsModel ? Object.keys(this._variationsModel).length : 0;
  }

  _initContextModelField($form) {
    const $modelElem = $(this.CONTEXT_MODEL_SELECTOR, $form);
    if ($modelElem.length === 1) {
      this._contextModel = JSON.parse($modelElem.val());
    }
  }

  _disposeContextModelField() {
    this._contextModel = null;
  }

  _initErrorModelField($form) {
    const $modelElem = $(this.ERROR_SELECTOR, $form);
    if ($modelElem.length === 1) {
      const model = {};
      $.each(JSON.parse($modelElem.val()), function () {
        model[this.Context] = this.Errors;
      });

      this._errorModel = model;
    }
  }

  _disposeErrorModelField() {
    this._errorModel = null;
  }

  _saveVariationsModelData($form) {
    if (this._variationsModelChanged) {
      const $modelElem = $(this.VARIATION_SELECTOR, $form);
      if ($modelElem.length === 1) {
        const model = [];
        Object.entries(this._variationsModel).forEach(([key, val]) => {
          model.push({ Context: key, Id: val.Id, FieldValues: val.FieldValues });
        });

        $modelElem.val(JSON.stringify(model)).change();
        this._variationsModelChanged = false;
      }
    }
  }

  _copyCurrentDataToVariationsModel($form) {
    const $contextElem = $(this.CONTEXT_SELECTOR, $form);
    if ($contextElem.length === 1) {
      $c.saveDataOfAllVisualEditors(this._formElement);
      this._copyToVariationsModel($contextElem.val());
    }
  }

  isFieldsValid() {
    return !this._formHasErrors;
  }

  isFieldsChanged() {
    const $changedFields = $(this._formElement).find(this.REAL_CHANGED_FIELD_SELECTOR);
    return $changedFields.length > 0;
  }

  isFieldValuesChanged() {
    const $changedFields = $(this._formElement)
      .find(this.VARIATION_SELECTOR)
      .closest('fieldset')
      .find(this.REAL_CHANGED_EXCEPT_VARMODEL_SELECTOR);

    return $changedFields.length > 0;
  }

  // eslint-disable-next-line max-statements
  addCustomButton(settings, $parent) {
    if (!settings.name && !settings.onClick && !settings.suffix && !settings.title) {
      $q.alertError('One of the required settings is missed: name, title, suffix, onClick');
    } else {
      const defaultSettings = {
        class: 'customButton'
      };

      const newSettings = Object.assign({}, defaultSettings, settings);
      const $form = $parent || $(this._formElement);
      const $input = $form.find(`[name='${newSettings.name}'],[data-content_field_name='${newSettings.name}']`);

      if ($input.length === 0) {
        if (this._notifyCustomButtonExistence) {
          $q.alertError(`Input ${newSettings.name} is not found`);
        }

        return undefined;
      }

      const et = $input.data('exact_type');
      const sTypes = [window.FILE_FIELD_TYPE, window.IMAGE_FIELD_TYPE];

      if ((!et && ($input.prop('type') !== 'text' || $input.parents('.t-numerictextbox').length > 0))
        || $.inArray(et, sTypes) === -1) {
        $q.alertError(`Input ${newSettings.name} type is not supported`);
        return undefined;
      }

      let $fieldWrapper = $input.parent('.fieldWrapper');
      if ($fieldWrapper.length === 0) {
        $input.wrap($('<div/>', { id: `${$input.prop('id')}_wrapper`, class: 'fieldWrapper group myClass' }));
        $fieldWrapper = $input.parent('.fieldWrapper');
      }

      const options = {
        id: `${$input.prop('id')}_${newSettings.suffix}`,
        class: newSettings.class,
        title: newSettings.title
      };

      if ($fieldWrapper.find(`#${options.id}`).length > 0) {
        if (this._notifyCustomButtonExistence) {
          $q.alertError(`Button ${options.id} already exists`);
        }

        return undefined;
      }

      const $div = $('<div/>', options);
      $div.append($('<img/>', { src: '/Backend/Content/Common/0.gif' }));

      if (newSettings.url) {
        $div.css({ 'background-image': `url(${newSettings.url})`, 'background-color': 'transparent' });
      }

      $fieldWrapper.append($div);
      $div.on('click', { $input, $form, newSettings }, newSettings.onClick);
      this._customButtons.push(options.id);
    }

    return undefined;
  }

  // eslint-disable-next-line max-statements
  addCustomLinkButton(settings, $parent) {
    if (!settings.name && !settings.onClick && !settings.suffix && !settings.title) {
      $q.alertError('One of the required settings is missed: name, title, suffix, onClick');
    } else {
      const defaultSettings = {
        class: 'pick'
      };

      const newSettings = Object.assign({}, defaultSettings, settings);
      const $form = $parent || $(this._formElement);
      const $input = $form.find(`[name='${newSettings.name}'],[data-content_field_name='${newSettings.name}']`);

      if ($input.length === 0) {
        if (this._notifyCustomButtonExistence) {
          $q.alertError(`Input ${newSettings.name} is not found`);
        }

        return undefined;
      }

      const et = $input.data('exact_type');
      const sTypes = [
        window.STRING_FIELD_TYPE,
        window.FILE_FIELD_TYPE,
        window.IMAGE_FIELD_TYPE,
        window.TEXTBOX_FIELD_TYPE,
        window.VISUAL_EDIT_FIELD_TYPE
      ];

      if ($.inArray(et, sTypes) === -1) {
        $q.alertError(`Input ${newSettings.name} type is not supported`);
        return undefined;
      }

      let $fieldWrapper = $input.parent('.fieldWrapper');
      if ($fieldWrapper.length === 0) {
        $input.wrap($('<div/>', { class: 'fieldWrapper group myClass' }));
        $fieldWrapper = $input.parent('.fieldWrapper');
      }

      let $ul = $fieldWrapper.find('ul.linkButtons');
      if ($ul.length === 0) {
        $fieldWrapper.prepend('<ul class="linkButtons group" style="display: block;" />');
        $ul = $fieldWrapper.find('ul.linkButtons');
      }

      const options = {
        id: `${$input.prop('id')}_${newSettings.suffix}`,
        class: newSettings.class,
        title: newSettings.title
      };

      if ($ul.find(`#${options.id}`).length > 0) {
        if (this._notifyCustomButtonExistence) {
          $q.alertError(`Button ${options.id} already exists`);
        }

        return undefined;
      }

      const builder = new $.telerik.stringBuilder();
      builder
        .cat(`<li><span id="${options.id}" class="linkButton actionLink">`)
        .cat('<a href="javascript:void(0);">')
        .cat(`<span class="icon ${options.class}"><img src="/Backend/Content/Common/0.gif"></span>`)
        .cat(`<span class="text">${options.title}</span>`)
        .cat('</a></span></li>');

      const $li = $(builder.string());
      if (newSettings.url) {
        $li.find('.icon').css({ 'background-image': `url(${newSettings.url})` });
      }

      $ul.append($li);
      $li.find('a').on('click', { $input, $form, newSettings }, newSettings.onClick);
      this._customLinkButtons.push(options.id);
    }

    return undefined;
  }

  saveEntity(actionCode) {
    $c.saveDataOfAllVisualEditors(this._formElement);
    $c.saveDataOfAllHighlightedTextAreas(this._formElement);
    $c.saveDataOfAllAggregationLists(this._formElement);
    if (this._entityTypeCode !== window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      if (!this.isFieldsChanged()) {
        $q.alertError($l.EntityEditor.fieldsNotChangedMessage);
        return false;
      }
    }

    const $form = $(this._formElement);

    this._copyCurrentDataToVariationsModel($form);
    this._saveVariationsModelData($form);

    $form.find(`input[name="${window.BACKEND_ACTION_CODE_HIDDEN_NAME}"]`).val(actionCode);
    $form.trigger('submit');

    return undefined;
  }

  refreshEditor(options) {
    const message = options && options.confirmMessageText
      ? options.confirmMessageText
      : $l.EntityEditor.autoRefreshConfirmMessage;

    if (this._confirmAction(message)) {
      const eventArgs = new BackendEventArgs();
      this.notify(window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_REFRESH_STARTING, eventArgs);
    }
  }

  fixEntityDataListsOverflow() {
    if (this._formElement) {
      const $form = $(this._formElement);
      $c.fixAllEntityDataListsOverflow($form);
    }
  }

  fixHtaInitializing() {
    if (this._formElement) {
      const $form = $(this._formElement);
      $c._refreshAllHta($form);
    }
  }

  confirmRefresh() {
    return this._confirmAction($l.EntityEditor.refreshConfirmMessage);
  }

  confirmChange() {
    return this._confirmAction($l.EntityEditor.changeConfirmMessage);
  }

  confirmClose() {
    return this._confirmAction($l.EntityEditor.closeConfirmMessage);
  }

  _confirmAction(message) {
    return !this.isFieldsChanged() || $q.confirmMessage(message);
  }

  onLoad() {
    const $form = $(this._formElement);
    $c.setAllEntityDataListValues($form, this._initFieldValues);

    this.fixEntityDataListsOverflow();
    $c.makeReadonlyEntityDataList($form, this._disabledFields);

    $c.setAllClassifierFieldValues($form, this._initFieldValues);
    $c.makeReadonlyClassifierFields($form, this._disabledFields);

    $c.initAllHighlightedTextAreas($form);
    $c.setAllHighlightedTextAreaValues($form, this._initFieldValues);

    BackendExpandedContainer.initAll($form);
    $c.setFieldRowsVisibility($form, this._hideFields, false);

    if (this._customLoad) {
      this._customLoad($form);
    }

    if (this._contextBlockState) {
      this.applyContext(this._contextBlockState);
    }

    this._initFieldValues = null;

    const $wrapper = $(`#${this._documentWrapperElementId}`);
    $wrapper.scrollTop(this._formHasErrors ? 0 : +$wrapper.data('scroll_position') || 0);
    this._editorManagerComponent.onEntityEditorReady(this._documentWrapperElementId);
  }

  onSelect() {
    this.fixEntityDataListsOverflow();
    this.fixHtaInitializing();
  }

  _onActionExecuting(eventType, sender, eventArgs) {
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_ACTION_EXECUTING, eventArgs);
  }

  _onHtmlInputChanged(e) {
    if (!this._disableChangeTracking) {
      const $field = $(e.currentTarget);
      $field.addClass(window.CHANGED_FIELD_CLASS_NAME);
      $field.removeClass(window.REFRESHED_FIELD_CLASS_NAME);

      let value;
      if ($field.is(':checkbox')) {
        value = $field.is(':checked');
      } else {
        value = $field.val();
      }

      $field.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, {
        fieldName: $field.attr('name'),
        value,
        contentFieldName: $field.data('content_field_name')
      });
    }
  }

  _onFieldValueChanged(e, data) {
    if (!this._disableChangeTracking) {
      if (data && data.fieldName) {
        if (this._customFieldValueChanged) {
          this._customFieldValueChanged(this, data);
        }

        this._editorManagerComponent.onFieldValueChanged(Object.assign({
          documentWrapperElementId: this._documentWrapperElementId
        }, data));
      }
    }
  }

  _onBeforeSubmit() {
    const $wrapper = $(`#${this._documentWrapperElementId}`);
    $wrapper.data('scroll_position', $wrapper.scrollTop());
    if (this._customBeforeSubmit && !this._customBeforeSubmit(this)) {
      return false;
    }

    const eventArgs = new BackendEntityEditorActionEventArgs();
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTING, eventArgs);
    return true;
  }

  _onSuccess(data) {
    let eventArgs = new BackendEntityEditorActionEventArgs();

    eventArgs.set_data(data);
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED, eventArgs);
    eventArgs = null;
  }

  _onError(jqXHR) {
    $q.processGenericAjaxError(jqXHR);
    const eventArgs = new BackendEntityEditorActionEventArgs();
    this.notify(window.EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED_ERROR, eventArgs);
  }

  _onFileUploaded(eventType, sender, eventArgs) {
    this.notify(window.EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED, eventArgs);
  }

  _onChangeContext() {
    const $form = $(this._formElement);
    const oldContextValue = $form.data(this.OLD_CONTEXT_DATA_KEY);
    const newContextValue = $(this.CONTEXT_SELECTOR, $form).val();

    if (oldContextValue !== newContextValue) {
      if (typeof oldContextValue !== 'undefined') {
        this._copyToVariationsModel(oldContextValue);
        this._saveVariationsModelData($form);
      }

      if (!this._initFieldValues) {
        this._restoreFromVariationsModel(newContextValue);
      }

      this._updateVariationInfo(newContextValue);
      $form.data(this.OLD_CONTEXT_DATA_KEY, newContextValue);
    }
  }

  _copyToVariationsModel(contextValue) {
    if (this.isFieldValuesChanged()) {
      const result = {};

      $.each(this.get_fieldValues(), function () {
        if (this.fieldName.match(/^field_[\d]+$/)) {
          result[this.fieldName] = this.value === null ? '' : `${this.value}`;
        }
      });

      if (!this._variationsModel[contextValue]) {
        this._variationsModel[contextValue] = { Id: 0 };
      }

      this._variationsModel[contextValue].FieldValues = result;
      this._variationsModelChanged = true;
    }
  }

  _getActualContext(contextValue) {
    if (this._contextModel && this._variationsModel) {
      if (contextValue in this._variationsModel) {
        return contextValue;
      }

      const contextValues = contextValue.split(',');

      let found, testContextValue;
      while (!found && contextValues.length > 0) {
        const lastIndex = contextValues.length - 1;
        const lastValue = contextValues[lastIndex];
        const parentValue = this._contextModel[lastIndex].Ids[lastValue];

        if (parentValue === '0') {
          contextValues.length -= 1;
        } else {
          contextValues[lastIndex] = parentValue;
        }

        testContextValue = contextValues.join();
        found = testContextValue in this._variationsModel;
      }

      return testContextValue;
    }

    return undefined;
  }

  _restoreFromVariationsModel(contextValue) {
    this._disableChangeTracking = true;
    const actualContextValue = this._getActualContext(contextValue);
    const fieldValues = this._variationsModel[actualContextValue].FieldValues;
    const errors = this._errorModel[actualContextValue];
    const initFieldValues = [];

    $.each(fieldValues, (key, value) => {
      const currentResult = { fieldName: key, value };
      if (!$q.isNullOrEmpty(errors)) {
        currentResult.errors = $.grep(errors, elem => elem.Name === key);
      }

      initFieldValues.push(currentResult);
    });

    const $form = $(this._formElement);

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
    $(this.VARIATION_SELECTOR, $form)
      .closest('fieldset')
      .find(this.CHANGED_EXCEPT_VARMODEL_SELECTOR)
      .removeClass(window.CHANGED_FIELD_CLASS_NAME);
  }

  applyContext(contextState) {
    this._contextBlockState = contextState;
    const result = [];

    $.each(contextState, function () {
      if (this.Value) {
        result.push(this.Value);
      }
    });

    $(this.CONTEXT_SELECTOR, this._formElement).val(result.join());
    this._onChangeContext();
  }

  dispose() {
    super.dispose();
    $('span[data-tooltip]').qtip('destroy', true);

    this._disposeAllFields();
    if (this._editorManagerComponent) {
      // @ts-ignore FIXME
      const editorCode = this._editorCode;

      if (!$q.isNullOrWhiteSpace(editorCode)) {
        this._editorManagerComponent.removeEditor(editorCode);
      }

      this._editorManagerComponent.onEntityEditorDisposed(this._documentWrapperElementId);
      this._editorManagerComponent = null;
    }

    $.each(this._customButtons, (index, value) => {
      $(`#${value}`).off('click');
    });

    $.each(this._customLinkButtons, (index, value) => {
      $(`#${value}`).find('a').off('click');
    });

    $(`#${this._documentWrapperElementId}`).removeData();

    $q.dispose.call(this, [
      '_validationSummaryElement',
      '_formElement',

      '_onActionExecutingHandler',
      '_onHtmlInputChangedHandler',
      '_onBeforeSubmitHandler',
      '_onSuccessHandler',
      '_onErrorHandler'
    ]);

    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendEntityEditor = BackendEntityEditor;
