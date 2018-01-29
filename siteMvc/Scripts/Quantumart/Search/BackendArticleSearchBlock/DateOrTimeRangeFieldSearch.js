import { BackendArticleSearchBlock } from '../BackendArticleSearchBlock';
import { FieldSearchBase, FieldSearchState } from './FieldSearchBase';
import { $c } from '../../ControlHelpers';
import { $q } from '../../Utils';

export class DateOrTimeRangeFieldSearch extends FieldSearchBase {
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
    rangeType
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

    this._rangeType = rangeType;
    this._onIsNullCheckBoxChangeHandler = $.proxy(this._onIsNullCheckBoxChange, this);
    this._onByValueSelectorChangedHandler = $.proxy(this._onByValueSelectorChanged, this);
    this._onLoadHandler = $.proxy(this._onLoad, this);
  }

  initialize() {
    let serverContent;
    let url = window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK;
    switch (this._rangeType) {
      case $e.ArticleFieldSearchType.DateRange:
        url += 'DateRange';
        break;
      case $e.ArticleFieldSearchType.TimeRange:
        url += 'TimeRange';
        break;
      case $e.ArticleFieldSearchType.DateTimeRange:
        url += 'DateTimeRange';
        break;
      default:
        url += 'TimeRange';
    }

    $q.getJsonFromUrl(
      'GET',
      url,
      {
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
      const isNullCheckBoxID = `${this._elementIdPrefix}_isNullCheckBox`;
      const dateFromID = `${this._elementIdPrefix}_dateFrom`;
      const dateToID = `${this._elementIdPrefix}_dateTo`;

      // полученную с сервера разметку добавить на страницу
      let $containerElement = $(this._containerElement);
      $containerElement.html(serverContent);

      // получить ссылки на dom-элеметы со значениями
      this._dateFromElement = $containerElement.find(`#${dateFromID}`).get(0);
      this._dateToElement = $containerElement.find(`#${dateToID}`).get(0);
      $c.disableDateTimePicker(this._dateToElement);

      // назначить обработчик события change чекбоксу
      let $isNullCheckBoxElement = $containerElement.find(`#${isNullCheckBoxID}`);
      $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

      // запомнить ссылку на dom-элемент чекбокса
      this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);


      $(".radioButtonsList input[type='radio']", $containerElement).click(this._onByValueSelectorChangedHandler);

      $(document).ready(this._onLoadHandler);

      $isNullCheckBoxElement = null;
      $containerElement = null;
    }
  }

  getSearchQuery() {
    return BackendArticleSearchBlock.createFieldSearchQuery(
      this._rangeType, this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID,
      this.getIsNull(),
      $c.getDateTimePickerValue(this._dateFromElement),
      $c.getDateTimePickerValue(this._dateToElement),
      this._isByValue);
  }

  getBlockState() {
    return new FieldSearchState(
      this._rangeType, this._fieldID, this._contentID,
      this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
      {
        isNull: this.getIsNull(),
        from: $c.getDateTimePickerValue(this._dateFromElement),
        to: $c.getDateTimePickerValue(this._dateToElement),
        isByValue: this._isByValue
      });
  }

  getFilterDetails() {
    const stateData = this.getBlockState().data;
    if (stateData.isNull) {
      return $l.SearchBlock.isNullCheckBoxLabelText;
    } else if (stateData.isByValue) {
      return stateData.from ? stateData.from : '?';
    }
    return `${stateData.from ? stateData.from : '?'} - ${stateData.to ? stateData.to : '?'}`;
  }

  restoreBlockState(state) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
        $isNullCheckBoxElement.prop('checked', state.isNull);
        $isNullCheckBoxElement.trigger('change');
        $isNullCheckBoxElement = null;
      }

      if (!$q.isNull(state.isByValue)) {
        if (state.isByValue) {
          $('.radioButtonsList input:radio[value=0]', this._containerElement)
            .prop('checked', true)
            .trigger('click');
        } else if (!state.isByValue) {
          $('.radioButtonsList input:radio[value=1]', this._containerElement)
            .prop('checked', true)
            .trigger('click');
        }
      }

      $c.setDateTimePickerValue(this._dateFromElement, state.from);
      $c.setDateTimePickerValue(this._dateToElement, state.to);
    }
  }

  _onIsNullCheckBoxChange() {
    if (this.getIsNull()) {
      $c.disableDateTimePicker(this._dateFromElement);
      $c.disableDateTimePicker(this._dateToElement);
    } else {
      $c.enableDateTimePicker(this._dateFromElement);
      if (!this._isByValue) {
        $c.enableDateTimePicker(this._dateToElement);
      }
    }
  }

  _onByValueSelectorChanged(e) {
    this._isByValue = +$(e.currentTarget).val() === 0;

    if (this._isByValue) {
      $c.disableDateTimePicker(this._dateToElement);
      $(this._dateToElement).closest('.row').hide();
      $(`label[for='${$(this._dateFromElement).attr('id')}']`, this._containerElement).text($l.SearchBlock.valueText);
    } else {
      if (!this.getIsNull()) {
        $c.enableDateTimePicker(this._dateToElement);
      }
      $(`label[for='${$(this._dateFromElement).attr('id')}']`, this._containerElement).text($l.SearchBlock.fromText);
      $(this._dateToElement).closest('.row').show();
    }
  }

  _onLoad() {
    $c.initAllDateTimePickers(this._containerElement);
  }

  dispose() {
    if (this._isNullCheckBoxElement) {
      let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
      $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
      $isNullCheckBoxElement = null;
    }

    let $containerElement = $(this._containerElement);
    $(".radioButtonsList input[type='radio']", $containerElement).unbind();
    $containerElement = null;

    $c.destroyAllDateTimePickers(this._containerElement);

    this._isNullCheckBoxElement = null;
    this._dateFromElement = null;
    this._dateToElement = null;

    this._onIsNullCheckBoxChangeHandler = null;
    this._onByValueSelectorChangedHandler = null;
    this._onLoadHandler = null;

    super.dispose();
  }

  _onIsNullCheckBoxChangeHandler = null;
  getIsNull() {
    if (this._isNullCheckBoxElement) {
      return $(this._isNullCheckBoxElement).is(':checked');
    }
    return false;
  }

  _rangeType = null;

  _isByValue = true;
  _isNullCheckBoxElement = null;
  _dateFromElement = null;
  _dateToElement = null;
}


import('../BackendArticleSearchBlock').then(() => {
  Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch = DateOrTimeRangeFieldSearch;
});
