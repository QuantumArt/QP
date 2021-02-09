import { BackendArticleSearchBlock } from '../BackendArticleSearchBlock';
import { FieldSearchBase, FieldSearchState } from './FieldSearchBase';
import { $q } from '../../Utils';

export class TextFieldSearch extends FieldSearchBase {
  // eslint-disable-next-line max-params
  constructor(
    containerElement,
    parentEntityId,
    fieldID,
    contentID,
    fieldColumn,
    fieldName,
    fieldGroup,
    referenceFieldID
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
    this._onByValueSelectorChangedHandler = jQuery.proxy(this._onByValueSelectorChanged, this);
    this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
  }

  initialize() {
    let serverContent;
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK}TextSearch`,
      {
        elementIdPrefix: this._elementIdPrefix,
        fieldID: this._fieldID
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
      const queryTextBoxID = `${this._elementIdPrefix}_textBox`;
      const inverseCheckBoxID = `${this._elementIdPrefix}_inverseCheckBox`;
      const isNullCheckBoxID = `${this._elementIdPrefix}_isNullCheckBox`;
      const exactMatchCheckBoxID = `${this._elementIdPrefix}_exactCheckBox`;
      const beginningCheckBoxID = `${this._elementIdPrefix}_beginningCheckBox`;
      const queryListTextBoxID = `${this._elementIdPrefix}_listTextBox`;

      const $containerElement = $(this._containerElement);
      $containerElement.append(serverContent);

      const $isNullCheckBoxElement = $containerElement.find(`#${isNullCheckBoxID}`);
      $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

      $(".radioButtonsList input[type='radio']", $containerElement).click(this._onByValueSelectorChangedHandler);

      this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

      const $inverseCheckBoxElement = $containerElement.find(`#${inverseCheckBoxID}`);
      this._inverseCheckBoxElement = $inverseCheckBoxElement.get(0);

      const $exactMatchCheckBoxElement = $containerElement.find(`#${exactMatchCheckBoxID}`);
      this._exactMatchCheckBoxElement = $exactMatchCheckBoxElement.get(0);

      const $beginningStartCheckBoxElement = $containerElement.find(`#${beginningCheckBoxID}`);
      this._beginningStartChechBoxElement = $beginningStartCheckBoxElement.get(0);
      this._queryTextBoxElement = $containerElement.find(`#${queryTextBoxID}`).get(0);
      this._queryListTextBoxElement = $containerElement.find(`#${queryListTextBoxID}`).get(0);
    }
  }

  getSearchQuery() {
    return BackendArticleSearchBlock.createFieldSearchQuery(
      Quantumart.QP8.Enums.ArticleFieldSearchType.Text,
      this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, this.getIsNull(),
      this._isByList ? '' : $(this._queryTextBoxElement).val(), this.getInverse(), this.getExactMatch(), this.getBeginningStart(),
      this._isByList ? this._getStrings($(this._queryListTextBoxElement).val()) : null);
  }

  getBlockState() {
    return new FieldSearchState(
      Quantumart.QP8.Enums.ArticleFieldSearchType.Text,
      this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
      {
        isByList: this._isByList,
        text: this._isByList ? '' : $(this._queryTextBoxElement).val(),
        listText: this._isByList ? $(this._queryListTextBoxElement).val() : '',
        inverse: this.getInverse(),
        isNull: this.getIsNull(),
        exactMatch: this.getExactMatch(),
        beginningStart: this.getBeginningStart()
      });
  }

  getFilterDetails() {
    const stateData = this.getBlockState().data;
    let result;
    const text = stateData.isByList ? stateData.listText : stateData.text;
    if (text) {
      result = `"${$q.cutShort(text, 8)}"`;
    } else {
      result = '""';
    }

    if (stateData.isNull) {
      result = $l.SearchBlock.isNullCheckBoxLabelText;
    } else if (stateData.exactMatch) {
      result = `=${result}`;
    } else if (stateData.beginningStart) {
      if (stateData.inverse) {
        return $l.SearchBlock.endText + result;
      }

      return $l.SearchBlock.fromText + result;
    }

    if (stateData.inverse) {
      result = `${$l.SearchBlock.notText}(${result})`;
    }

    return result;
  }

  restoreBlockState(state) {
    if (state) {
      const value = $q.isNull(state.isByList) ? 0 : 1;
      $(`.radioButtonsList input:radio[value=${value}]`, this._containerElement)
        .prop('checked', true).trigger('click');

      if (value === 1) {
        $(this._queryListTextBoxElement).val(state.listText);
      } else {
        if (this._isNullCheckBoxElement) {
          let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
          $isNullCheckBoxElement.prop('checked', state.isNull);
          $isNullCheckBoxElement.trigger('change');
          $isNullCheckBoxElement = null;
        }

        if (this._beginningStartChechBoxElement) {
          const $beginningStartCheckBoxElement = $(this._beginningStartChechBoxElement);
          $beginningStartCheckBoxElement.prop('checked', state.beginningStart);
        }

        if (this._exactMatchCheckBoxElement) {
          const $exactMatchCheckBoxElement = $(this._exactMatchCheckBoxElement);
          $exactMatchCheckBoxElement.prop('checked', state.exactMatch);
        }

        $(this._queryTextBoxElement).val(state.text);
      }

      if (this._inverseCheckBoxElement) {
        const $inverseCheckBoxElement = $(this._inverseCheckBoxElement);
        $inverseCheckBoxElement.prop('checked', state.inverse);
      }
    }
  }

  _onIsNullCheckBoxChange() {
    $(this._queryTextBoxElement).prop('disabled', this.getIsNull());
    $(this._exactMatchCheckBoxElement).prop('disabled', this.getIsNull());
    $(this._beginningStartChechBoxElement).prop('disabled', this.getIsNull());
  }

  _onByValueSelectorChanged(e) {
    this._isByList = +$(e.currentTarget).val() === 1;

    if (this._isByList) {
      $(this._queryListTextBoxElement).closest('.row').show();
      $(this._queryTextBoxElement).closest('.row').hide();
      $(this._isNullCheckBoxElement).closest('.row').hide();
      $(this._exactMatchCheckBoxElement).closest('.row').hide();
      $(this._beginningStartChechBoxElement).closest('.row').hide();
    } else {
      $(this._queryListTextBoxElement).closest('.row').hide();
      $(this._queryTextBoxElement).closest('.row').show();
      $(this._isNullCheckBoxElement).closest('.row').show();
      $(this._exactMatchCheckBoxElement).closest('.row').show();
      $(this._beginningStartChechBoxElement).closest('.row').show();
    }
  }

  dispose() {
    if (this._isNullCheckBoxElement) {
      let $containerElement = $(this._containerElement);
      let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
      $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
      $(".radioButtonsList input[type='radio']", $containerElement).unbind();
      $isNullCheckBoxElement = null;
      $containerElement = null;
    }

    this._isNullCheckBoxElement = null;
    this._inverseCheckBoxElement = null;
    this._queryTextBoxElement = null;
    this._queryListTextBoxElement = null;
    this._beginningStartChechBoxElement = null;
    this._exactMatchCheckBoxElement = null;
    this._onIsNullCheckBoxChangeHandler = null;

    super.dispose();
  }

  _onIsNullCheckBoxChangeHandler = null;

  getIsNull() {
    if (this._isNullCheckBoxElement) {
      return $(this._isNullCheckBoxElement).is(':checked');
    }

    return false;
  }

  getInverse() {
    if (this._inverseCheckBoxElement) {
      return $(this._inverseCheckBoxElement).is(':checked');
    }

    return false;
  }

  getExactMatch() {
    if (this._exactMatchCheckBoxElement) {
      return $(this._exactMatchCheckBoxElement).is(':checked');
    }

    return false;
  }

  getBeginningStart() {
    if (this._beginningStartChechBoxElement) {
      return $(this._beginningStartChechBoxElement).is(':checked');
    }

    return false;
  }

  _queryTextBoxElement = null;
  _isNullCheckBoxElement = null;
  _inverseCheckBoxElement = null;
  _beginningStartChechBoxElement = null;
  _exactMatchCheckBoxElement = null;
}


import('../BackendArticleSearchBlock').then(() => {
  Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch = TextFieldSearch;
});
