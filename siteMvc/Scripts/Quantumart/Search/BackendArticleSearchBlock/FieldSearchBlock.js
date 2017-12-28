import { BackendSearchBlockBase } from '../BackendSearchBlockBase';
import { FieldSearchContainer } from './FieldSearchContainer';
import { $q } from '../../Utils';

export class FieldSearchBlock {
  constructor(fieldSearchBlockElement, parentEntityId) {
    this._fieldSearchBlockElement = fieldSearchBlockElement;
    this._parentEntityId = parentEntityId;
    this._elementIdPrefix = BackendSearchBlockBase.generateElementPrefix();

    this._fieldSearchContainerList = {};

    this._onAddFieldClickHandler = $.proxy(this._onAddFieldClick, this);
    this._onFieldSearchContainerCloseHandler = $.proxy(this._onFieldSearchContainerClose, this);
  }

  _fieldSearchBlockElement = null;
  _fieldSearchListElement = null;
  _fieldsComboElement = null;
  _addFieldSearchButtonElement = null;

  _parentEntityId = 0;
  _elementIdPrefix = '';

  _fieldSearchContainerList = null;

  _onAddFieldClickHandler = null;
  _onFieldSearchContainerCloseHandler = null;

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
      const $fieldSearchBlockElement = $(this._fieldSearchBlockElement);
      $fieldSearchBlockElement.html(serverContent);

      this._fieldSearchListElement = $fieldSearchBlockElement.find(`#${this._elementIdPrefix}_FieldSearchList`).get(0);
      this._fieldsComboElement = $fieldSearchBlockElement.find(`#${this._elementIdPrefix}_FieldsCombo`).get(0);
      this._addFieldSearchButtonElement = $fieldSearchBlockElement
        .find(`#${this._elementIdPrefix}_AddFieldSearchButton`)
        .get(0);

      this._attachFieldSearchBlockEventHandlers();
    }
  }

  getSearchQuery() {
    const result = [];
    Object.keys(this._fieldSearchContainerList).forEach(fieldID => {
      if (fieldID && this._fieldSearchContainerList[fieldID]) {
        const fscsq = this._fieldSearchContainerList[fieldID].getSearchQuery();
        if (fscsq) {
          result.push(fscsq);
        }
      }
    });

    if (result.length > 0) {
      return result;
    }

    return null;
  }

  getBlockState() {
    const result = Object.values(this._fieldSearchContainerList).map(fsc => fsc.getBlockState());
    if (result && result.length > 0) {
      return result;
    }

    return undefined;
  }

  restoreBlockState(state) {
    if (state) {
      const that = this;
      const $options = $('option', this._fieldsComboElement);
      $.each(state, (index, st) => {
        if (st.fieldID && !that._fieldSearchContainerList[st.fieldID]) {
          const isValid = $options.is(function () {
            const $option = $(this);
            return st.fieldID === $option.data('field_id')
                 && st.fieldName === $option.text()
                 && st.searchType === $option.data('search_type')
                 && st.fieldColumn === $option.data('field_column');
          });

          if (isValid) {
            const newContainer = that._createFieldSearchContainerInner(
              st.fieldID, st.contentID, st.searchType, st.fieldName, st.fieldColumn, st.fieldGroup, st.referenceFieldID
            );

            if (st.data) {
              newContainer.restoreBlockState(st.data);
            }
          }
        }
      });
    }
  }

  clear() {
    if (this._fieldsComboElement) {
      $(this._fieldsComboElement).find('option:first').prop('selected', true);
    }

    this._destroyAllFieldSearchContainers();
  }

  _attachFieldSearchBlockEventHandlers() {
    if (this._fieldsComboElement) {
      const $combo = $(this._fieldsComboElement);
      $combo.bind('change', this._onAddFieldClickHandler);
    }
  }

  _detachFieldSearchBlockEventHandlers() {
    if (this._fieldsComboElement) {
      const $combo = $(this._fieldsComboElement);
      $combo.unbind('change', this._onAddFieldClickHandler);
    }
  }

  _createFieldSearchContainer() {
    const $combo = $(this._fieldsComboElement);
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
          fieldID,
          contentID,
          fieldSearchType,
          fieldName,
          fieldColumn,
          fieldGroup,
          referenceFieldID
        );
      }

      $combo.val('');
    }
  }

  // eslint-disable-next-line max-params
  _createFieldSearchContainerInner(
    fieldID,
    contentID,
    fieldSearchType,
    fieldName,
    fieldColumn,
    fieldGroup,
    referenceFieldID
  ) {
    const $fieldSearchContainerElement = $('<div />', { class: 'fieldSearchContainer' });
    $(this._fieldSearchListElement).append($fieldSearchContainerElement);
    const newFieldSearchContainer = new FieldSearchContainer(
      $fieldSearchContainerElement.get(0), this._parentEntityId,
      fieldID, contentID, fieldName, fieldSearchType, fieldColumn, fieldGroup, referenceFieldID
    );

    newFieldSearchContainer.initialize();
    newFieldSearchContainer.attachObserver(
      window.EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE, this._onFieldSearchContainerCloseHandler
    );

    this._fieldSearchContainerList[fieldID] = newFieldSearchContainer;
    return newFieldSearchContainer;
  }

  _destroyFieldSearchContainer(fieldID) {
    if (this._fieldSearchContainerList[fieldID]) {
      const fieldSearchContainer = this._fieldSearchContainerList[fieldID];
      const $fsContainer = $(fieldSearchContainer.get_ContainerElement());
      fieldSearchContainer.detachObserver(
        window.EVENT_TYPE_CONRETE_FIELD_SEARCH_CONTAINER_CLOSE, this._onFieldSearchContainerCloseHandler
      );

      fieldSearchContainer.dispose();
      $q.removeProperty(this._fieldSearchContainerList, fieldID);
      $fsContainer.empty().remove();
    }
  }

  _destroyAllFieldSearchContainers() {
    Object.keys(this._fieldSearchContainerList).forEach(fieldId => this._destroyFieldSearchContainer(fieldId), this);
  }

  _onAddFieldClick() {
    this._createFieldSearchContainer();
  }

  _onFieldSearchContainerClose(eventType, sender, args) {
    this._destroyFieldSearchContainer(args.fieldID);
  }

  dispose() {
    this._detachFieldSearchBlockEventHandlers();
    this._destroyAllFieldSearchContainers();

    $q.dispose.call(this, [
      '_fieldSearchContainerList',
      '_fieldSearchBlockElement',
      '_fieldSearchListElement',
      '_fieldsComboElement',
      '_addFieldSearchButtonElement',
      '_onAddFieldClickHandler',
      '_onFieldSearchContainerCloseHandler'
    ]);

    $q.collectGarbageInIE();
  }
}


import('../BackendArticleSearchBlock').then(() => {
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBlock = FieldSearchBlock;
});
