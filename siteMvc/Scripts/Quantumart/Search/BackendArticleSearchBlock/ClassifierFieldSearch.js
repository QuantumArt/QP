import { BackendArticleSearchBlock } from '../BackendArticleSearchBlock';
import { FieldSearchBase, FieldSearchState } from './FieldSearchBase';
import { $c } from '../../ControlHelpers';
import { $q } from '../../Utils';

export class ClassifierFieldSearch extends FieldSearchBase {
  // eslint-disable-next-line max-params
  constructor(
    containerElement,
    parentEntityId,
    fieldID,
    contentID,
    fieldColumn,
    fieldName,
    fieldGroup,
    referenceFieldID,
    searchType
  ) {
    super(
      containerElement,
      parentEntityId,
      fieldID,
      contentID,
      fieldColumn,
      fieldName,
      fieldGroup,
      referenceFieldID
    );

    this._contentID = contentID;
    this._searchType = searchType;
    this._onIsNullCheckBoxChangeHandler = $.proxy(this._onIsNullCheckBoxChange, this);
  }

  _contentElement = null;
  initialize() {
    let serverContent;
    $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK}ContentsListForClassifier`, {
      elementIdPrefix: this._elementIdPrefix,
      fieldID: this._fieldID
    }, false, false, data => {
      if (data.success) {
        serverContent = data.view;
      } else {
        $q.alertError(data.message);
      }
    }, jqXHR => {
      serverContent = null;
      $q.processGenericAjaxError(jqXHR);
    });

    if (!$q.isNullOrWhiteSpace(serverContent)) {
      const isNullCheckBoxID = `${this._elementIdPrefix}_isNullCheckBox`;
      const contentID = `${this._elementIdPrefix}_contentID`;

      // полученную с сервера разметку добавить на страницу
      const $containerElement = $(this._containerElement);
      $containerElement.html(serverContent);

      const $content = $containerElement.find(`#${contentID}`);

      // назначить обработчик события change чекбоксу
      const $isNullCheckBoxElement = $containerElement.find(`#${isNullCheckBoxID}`);
      $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

      // запомнить ссылку на dom-элементы
      this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);
      this._contentElement = $content.get(0);
    }
  }

  getSearchQuery() {
    const contentObj = new Array($(this._contentElement).val());
    return BackendArticleSearchBlock.createFieldSearchQuery(
      this._searchType, this._fieldID, this._fieldColumn, this._contentID,
      this._referenceFieldID, contentObj, this.getIsNull(), false
    );
  }

  getBlockState() {
    return new FieldSearchState(
      this._searchType, this._fieldID, this._contentID, this._fieldColumn,
      this._fieldName, this._fieldGroup, this._referenceFieldID, {
        isNull: this.getIsNull(),
        contentID: $(this._contentElement).val()
      });
  }

  getFilterDetails() {
    const stateData = this.getBlockState().data;
    if (stateData.isNull) {
      return $l.SearchBlock.isNullCheckBoxLabelText;
    } else if (stateData.contentID) {
      return $q.cutShort($(this._contentElement).find(`[value=${stateData.contentID}]`).text(), 12);
    }
    return '';
  }

  restoreBlockState(state) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
        $isNullCheckBoxElement.prop('checked', state.isNull);
        $isNullCheckBoxElement.trigger('change');
        $isNullCheckBoxElement = null;
      }

      Sys.Debug.trace(state.contentID);
      if (!$q.isNull(state.contentID)) {
        $(this._contentElement).val(state.contentID);
      }
    }
  }

  _onIsNullCheckBoxChangeHandler = null;
  getIsNull() {
    if (this._isNullCheckBoxElement) {
      return $(this._isNullCheckBoxElement).is(':checked');
    }
    return false;
  }

  _onIsNullCheckBoxChange() {
    if (this.getIsNull()) {
      $(this._contentElement).prop('disabled', true);
    } else {
      $(this._contentElement).prop('disabled', false);
    }
  }

  dispose() {
    if (this._isNullCheckBoxElement) {
      const $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
      $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
    }

    $c.destroyAllNumericTextBoxes(this._contentElement);
    this._isNullCheckBoxElement = null;
    this._onIsNullCheckBoxChangeHandler = null;
    this._contentElement = null;

    super.dispose();
  }

  _searchType = 0;
  _isNullCheckBoxElement = null;
  _contentID = null;
}


import('../BackendArticleSearchBlock').then(() => {
  Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch = ClassifierFieldSearch;
});
