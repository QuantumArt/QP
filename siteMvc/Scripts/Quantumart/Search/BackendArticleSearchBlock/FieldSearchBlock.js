Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock = function (fieldSearchBlockElement, parentEntityId) {
  this._fieldSearchBlockElement = fieldSearchBlockElement;
  this._parentEntityId = parentEntityId;
  this._elementIdPrefix = Quantumart.QP8.BackendSearchBlockBase.generateElementPrefix();

  this._fieldSearchContainerList = {};

  this._onAddFieldClickHandler = jQuery.proxy(this._onAddFieldClick, this);
  this._onFieldSearchContainerCloseHandler = jQuery.proxy(this._onFieldSearchContainerClose, this);
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock.prototype = {
  _fieldSearchBlockElement: null,
  _fieldSearchListElement: null,
  _fieldsComboElement: null,
  _addFieldSearchButtonElement: null,

  _parentEntityId: 0,
  _elementIdPrefix: '',

  _fieldSearchContainerList: null,

  _onAddFieldClickHandler: null,
  _onFieldSearchContainerCloseHandler: null,

  initialize() {
    let serverContent;
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK}FieldSearchBlock`,
      {
        parentEntityId: this._parentEntityId,
        elementIdPrefix: this._elementIdPrefix
      },
      false,
      false,
      data => {
        if (data.success) {
          serverContent = data.view;
        } else {
          $q.alertFail(data.message);
        }
      },
      jqXHR => {
        serverContent = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );
    if (!$q.isNullOrWhiteSpace(serverContent)) {
      let $fieldSearchBlockElement = jQuery(this._fieldSearchBlockElement);
      $fieldSearchBlockElement.html(serverContent);

      this._fieldSearchListElement = $fieldSearchBlockElement.find(`#${this._elementIdPrefix}_FieldSearchList`).get(0);
      this._fieldsComboElement = $fieldSearchBlockElement.find(`#${this._elementIdPrefix}_FieldsCombo`).get(0);
      this._addFieldSearchButtonElement = $fieldSearchBlockElement.find(
        `#${this._elementIdPrefix}_AddFieldSearchButton`).get(0);
      this._attachFieldSearchBlockEventHandlers();

      $fieldSearchBlockElement = null;
    }
  },

  get_searchQuery() {
    const result = [];
    for (const fieldID in this._fieldSearchContainerList) {
      if (fieldID && this._fieldSearchContainerList[fieldID]) {
        const fscsq = this._fieldSearchContainerList[fieldID].get_searchQuery();
        if (fscsq) {
          result.push(fscsq);
        }
      }
    }

    if (result.length > 0) {
      return result;
    }
    return null;
  },

  get_blockState() {
    const r = jQuery.grep(
      jQuery.map(this._fieldSearchContainerList, fsc => fsc ? fsc.get_blockState() : null),
      fsc => fsc
    );
    if (r && r.length > 0) {
      return r;
    }
    return undefined;
  },

  restore_blockState(state) {
    if (state) {
      const that = this;
      const $options = jQuery('option', this._fieldsComboElement);
      jQuery.each(state, (index, s) => {
        if (s.fieldID && !that._fieldSearchContainerList[s.fieldID]) {
          const is = $options.is(function () {
            const $option = jQuery(this);
            return s.fieldID === $option.data('field_id')
                 && s.fieldName === $option.text()
                 && s.searchType === $option.data('search_type')
                 && s.fieldColumn === $option.data('field_column');
          });
          if (is) {
            const newContainer = that._createFieldSearchContainerInner(
              s.fieldID, s.contentID, s.searchType, s.fieldName, s.fieldColumn, s.fieldGroup, s.referenceFieldID
            );
            if (s.data) {
              newContainer.restore_blockState(s.data);
            }
          }
        }
      });
    }
  },

  clear() {
    if (this._fieldsComboElement) {
      jQuery(this._fieldsComboElement).find('option:first').prop('selected', true);
    }

    this._destroyAllFieldSearchContainers();
  },

  _attachFieldSearchBlockEventHandlers() {
    if (this._fieldsComboElement) {
      let $combo = jQuery(this._fieldsComboElement);
      $combo.bind('change', this._onAddFieldClickHandler);
      $combo = null;
    }
  },

  _detachFieldSearchBlockEventHandlers() {
    if (this._fieldsComboElement) {
      let $combo = jQuery(this._fieldsComboElement);
      $combo.unbind('change', this._onAddFieldClickHandler);
      $combo = null;
    }
  },

  _createFieldSearchContainer() {
    const $combo = jQuery(this._fieldsComboElement);
    const $selectedField = $combo.find('option:selected');

    if ($selectedField) {
      const fieldID = $selectedField.data('field_id');

      if (fieldID && !this._fieldSearchContainerList[fieldID]) {
        const contentID = $selectedField.data('content_id');
        const fieldName = $selectedField.text();
        const fieldSearchType = $selectedField.data('search_type');
        const fieldColumn = $selectedField.data('field_column');
        const fieldGroup = $selectedField.data('field_group');
        const referenceFieldID = $selectedField.data('reference_field_id');
        this._createFieldSearchContainerInner(
          fieldID, contentID, fieldSearchType, fieldName, fieldColumn, fieldGroup, referenceFieldID
        );
      }

      $combo.val('');
    }
  },

  _createFieldSearchContainerInner(
    fieldID, contentID, fieldSearchType, fieldName, fieldColumn, fieldGroup, referenceFieldID
  ) {
    let $fieldSearchContainerElement = jQuery('<div />', { class: 'fieldSearchContainer' });
    jQuery(this._fieldSearchListElement).append($fieldSearchContainerElement);
    const newFieldSearchContainer = new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchContainer(
      $fieldSearchContainerElement.get(0), this._parentEntityId,
      fieldID, contentID, fieldName, fieldSearchType, fieldColumn, fieldGroup, referenceFieldID
    );
    newFieldSearchContainer.initialize();
    newFieldSearchContainer.attachObserver(
      window.EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE, this._onFieldSearchContainerCloseHandler
    );
    this._fieldSearchContainerList[fieldID] = newFieldSearchContainer;
    $fieldSearchContainerElement = null;
    return newFieldSearchContainer;
  },

  _destroyFieldSearchContainer(fieldID) {
    if (this._fieldSearchContainerList[fieldID]) {
      let fieldSearchContainer = this._fieldSearchContainerList[fieldID];
      let $fsContainer = jQuery(fieldSearchContainer.get_ContainerElement());
      fieldSearchContainer.detachObserver(
        window.EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE, this._onFieldSearchContainerCloseHandler
      );
      fieldSearchContainer.dispose();
      $q.removeProperty(this._fieldSearchContainerList, fieldID);
      $fsContainer.empty().remove();
      $fsContainer = null;
      fieldSearchContainer = null;
    }
  },

  _destroyAllFieldSearchContainers() {
    for (const fieldID in this._fieldSearchContainerList) {
      this._destroyFieldSearchContainer(fieldID);
    }
  },

  _onAddFieldClick() {
    this._createFieldSearchContainer();
  },

  _onFieldSearchContainerClose(eventType, sender, args) {
    this._destroyFieldSearchContainer(args.fieldID);
  },

  dispose() {
    this._detachFieldSearchBlockEventHandlers();
    this._destroyAllFieldSearchContainers();
    this._fieldSearchContainerList = null;
    this._fieldSearchBlockElement = null;
    this._fieldSearchListElement = null;
    this._fieldsComboElement = null;
    this._addFieldSearchButtonElement = null;
    this._onAddFieldClickHandler = null;
    this._onFieldSearchContainerCloseHandler = null;
  }
};

Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock.registerClass(
  'Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock', null, Sys.IDisposable
);
