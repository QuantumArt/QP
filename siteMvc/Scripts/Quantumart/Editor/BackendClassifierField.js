var EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED = 'OnClassifierFieldArticleLoaded';
var EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_UNLOADING = 'OnClassifierFieldArticleUnloading';

Quantumart.QP8.BackendClassifierField = function (componentElem, editorExecutingHandler, options) {
  Quantumart.QP8.BackendClassifierField.initializeBase(this);
  this._editorExecutingHandler = editorExecutingHandler;
  if (!$q.isNull(options)) {
    if (options.hostIsWindow) {
      this._hostIsWindow = options.hostIsWindow;
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

    if (options.customInit) {
      this._customInit = options.customInit;
    }

    if (options.customDispose) {
      this._customDispose = options.customDispose;
    }

    if (options.parentEditor) {
      this._parentEditor = options.parentEditor;
    }

    if (options.customButtonsSettings) {
      this._customButtonsSettings = options.customButtonsSettings;
    }

    if (options.customLinkButtonsSettings) {
      this._customLinkButtonsSettings = options.customLinkButtonsSettings;
    }
  }

  this._$componentElem = $(componentElem);
  this._$componentElem.data(Quantumart.QP8.BackendClassifierField.componentRefDataKey, this);
};

Quantumart.QP8.BackendClassifierField.componentRefDataKey = 'component_ref';
Quantumart.QP8.BackendClassifierField.getComponent = function(componentElem){
  return $(componentElem).data(Quantumart.QP8.BackendClassifierField.componentRefDataKey);
};

Quantumart.QP8.BackendClassifierField.prototype = {
  _$componentElem: null, // главный элемент компонента
  _$articleWrapper: null, // контейнер содержимого агрегированной статьи
  _$contentList: null, // список контентов
  _hostIsWindow: false,
  _initFieldValues: null, // значения для инициализации полей
  _disabledFields: null, // идентификаторы полей которые должны быть disable (массив имен полей)
  _hideFields: null,  // идентификаторы полей которые должны быть скрыты (массив имен полей)
  _disableChangeTracking: false,
  _parentEditor: null,
  _customInit: null,
  _customDispose: null,

  initialize: function () {
    var selectedContentId = this._$componentElem.data('aggregated_content_id') || '';

    this._$articleWrapper = $('.articleWrapper_' + selectedContentId);
    this._$contentList = this._$componentElem.find('select.classifierContentList');

    this._$contentList.find('option[value=' + selectedContentId + ']').prop('selected', true);
    this._$contentList.on('change', this._onContentSelected.bind(this));

    if ($q.toBoolean(this._$componentElem.data('is_not_changeable'))) {
      this.makeReadonly();
    }
  },

  selectContent: function (contentId) {
    if (!$q.isNull(contentId) && $.isNumeric(contentId)) {
      this._$contentList.find('OPTION[value="' + contentId + '"]').prop('selected', true).change();
    } else {
      this._$contentList.find('OPTION:selected').prop('selected', false).change();
    }
  },

  getSelectedContent: function() {
    return $q.toInt(this._$contentList.find('option:selected').val());
  },

  makeReadonly: function() {
    var selectedVal = this._$contentList.find('OPTION:selected').val();
    if (!$q.isNullOrEmpty(selectedVal)) {
      var $hidden = this._$contentList.siblings('input[name="' + this._$contentList.prop("name") + '"]:hidden')
      if ($hidden.length) {
        $hidden.val(selectedVal);
      } else {
        this._$contentList.after('<input type="hidden" name="' + this._$contentList.prop("name") + '" value="' + selectedVal + '" />');
      }
    }

    this._$contentList.addClass(this.LIST_DISABLED_CLASS_NAME).prop('disabled', true);
  },

  _onContentSelected: function(e) {
    var tabId = this._$componentElem.data('host_id');
    var rootContentId = +this._$componentElem.data('root_content_id') || 0;
    var rootArticleId = +this._$componentElem.data('root_article_id') || 0;
    var aggregatedContentId = +this._$contentList.find('option:selected').val() || 0;

    if(aggregatedContentId) {
      $q.showLoader();
      $q.getAjax(Url.Content('~/Article/GetAggregatedArticle'), {
        id: rootArticleId,
        parentId: rootContentId,
        aggregatedContentId: aggregatedContentId
      }, this._renderAggregatedDataView.bind(this)).error($q.processGenericAjaxError);

      $q.getAjax(Url.Content('~/Content/GetContentFormScript'), {
        contentId: aggregatedContentId
      }, this._executeAggregatedDataScript.bind(this, aggregatedContentId)).error($q.processGenericAjaxError);
    } else {
      this._$articleWrapper.empty();
      this._disposeAggregatedFields();
    }
  },

  _renderAggregatedDataView: function(articleViewData) {
    this.notify(EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_UNLOADING, {
      articleWrapper: this._$articleWrapper,
      toggleDisableChangeTracking: this.set_disableChangeTracking
    });

    this._$articleWrapper.empty();
    this._disposeAggregatedFields();
    if (articleViewData) {
      if (!this._$articleWrapper.length) {
        this._$articleWrapper = $('<div />', {
          class: 'articleWrapper'
        });

        this._$componentElem.closest("dl.row").after(this._$articleWrapper);
      }

      this._$articleWrapper.html(articleViewData);
      this._initAllFields();
      this.notify(EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED, {
        articleWrapper: this._$articleWrapper,
        toggleDisableChangeTracking: this.set_disableChangeTracking
      });
    }

    if (this._disableChangeTracking) {
      this._$articleWrapper.find('.' + CHANGED_FIELD_CLASS_NAME).removeClass(CHANGED_FIELD_CLASS_NAME);
      this._disableChangeTracking = false;
    }
  },

  _executeAggregatedDataScript: function(aggregatedContentId, articleScriptData) {
    var scriptId = 'aggregated_script_' + aggregatedContentId;
    if (articleScriptData) {
      var $scriptTag = $('#' + scriptId);
      if ($scriptTag.length) {
        $scriptTag.remove();
      }

      var script = window.document.createElement('script');
      script.id = 'aggregated_script_' + aggregatedContentId;
      script.src = 'data:text/javascript,' + window.encodeURIComponent(articleScriptData)
      window.document.body.appendChild(script);
    }
  },

  _initAllFields: function () {
    var $form = this._$articleWrapper;

    $c.setAllVisualEditorValues($form, this._initFieldValues);
    $c.makeReadonlyVisualEditors($form, this._disabledFields);

    $c.initAllDateTimePickers($form);
    $c.initAllVisualEditors($form);
    $c.initAllNumericTextBoxes($form);
    $c.initAllFileFields($form);
    $c.initAllEntityDataLists($form, this._editorExecutingHandler, { hostIsWindow: this._hostIsWindow });
    $c.initAllEntityDataTrees($form);
    $c.initAllAggregationLists($form);
    $c.initAllHighlightedTextAreas($form);
    Quantumart.QP8.BackendExpandedContainer.initAll($form);

    $c.setAllSimpleTextBoxValues($form, this._initFieldValues);
    $c.setAllBooleanValues($form, this._initFieldValues);
    $c.setAllNumericBoxValues($form, this._initFieldValues);
    $c.setAllDateTimePickersValues($form, this._initFieldValues);
    $c.setAllRadioListValues($form, this._initFieldValues);

    $c.initAllCheckboxToggles($form);
    $c.initAllSwitcherLists($form);

    $c.makeReadonlySimpleTextBoxes($form, this._disabledFields);
    $c.makeReadonlyBooleans($form, this._disabledFields);
    $c.makeReadonlyNumericBox($form, this._disabledFields);
    $c.makeReadonlyDateTimePickers($form, this._disabledFields);
    $c.makeReadonlyFileFields($form, this._disabledFields);
    $c.makeReadonlyRadioList($form, this._disabledFields);

    $c.setAllEntityDataListValues($form, this._initFieldValues);
    $c.fixAllEntityDataListsOverflow($form);
    $c.makeReadonlyEntityDataList($form, this._disabledFields);

    $c.setFieldRowsVisibility($form, this._hideFields, false);

    if (this._customInit) {
      this._customInit(this._parentEditor, $form);
    }

    var self = this;
    if (this._customButtonsSettings) {
      $.each(this._customButtonsSettings, function (index, item) { self._parentEditor.addCustomButton(item, $form) });
    }

    if (this._customLinkButtonsSettings) {
      $.each(this._customLinkButtonsSettings, function (index, item) { self._parentEditor.addCustomLinkButton(item, $form) });
    }

    $form = null;
  },

  set_initFieldValues: function (value) {
      this._initFieldValues = value;
  },

  set_disableChangeTracking: function (value) {
      this._disableChangeTracking = value;
  },

  _disposeAggregatedFields: function () {
    var $form = this._$articleWrapper;
    if (this._customDispose) {
      this._customDispose(this._parentEditor, $form);
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
  },

  dispose: function () {
    this._customInit = null;
    this._customDispose = null;
    this._parentEditor = null;
    if (this._$contentList) {
      this._$contentList.off();
      this._$contentList = null;
    }

    this._$articleWrapper = null;
    if (this._$componentElem) {
      this._$componentElem.removeData();
      this._$componentElem = null;
    }
  }
};

Quantumart.QP8.BackendClassifierField.registerClass('Quantumart.QP8.BackendClassifierField', Quantumart.QP8.Observable);
