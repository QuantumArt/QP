import { BackendArticleSearchBlock } from '../BackendArticleSearchBlock';
import { FieldSearchBase, FieldSearchState } from './FieldSearchBase';
import { $c } from '../../ControlHelpers';
import { $q } from '../../Utils';

export class IdentifierFieldSearch extends FieldSearchBase {
  // eslint-disable-next-line max-params
  constructor(
    containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
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
    this._onNumericInputFocusHandler = jQuery.proxy(this._onNumericInputFocus, this);
    this._onLoadHandler = jQuery.proxy(this._onLoad, this);
  }

  initialize() {
    let serverContent;

    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK}Identifier`,
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
      const inverseCheckBoxID = `${this._elementIdPrefix}_inverseCheckBox`;
      const numberFromID = `${this._elementIdPrefix}_numberFrom`;
      const numberToID = `${this._elementIdPrefix}_numberTo`;

      let $containerElement = $(this._containerElement);
      $containerElement.html(serverContent);

      let $numberFrom = $containerElement.find(`#${numberFromID}`);
      let $numberTo = $containerElement.find(`#${numberToID}`);
      $numberFrom.focus(this._onNumericInputFocusHandler);
      $numberTo.focus(this._onNumericInputFocusHandler);

      this._numberFromElement = $numberFrom.get(0);
      this._numberToElement = $numberTo.get(0);
      this._textAreaElement = $containerElement.find(`#${this._elementIdPrefix}_text`).get(0);

      let $inverseCheckBoxElement = $containerElement.find(`#${inverseCheckBoxID}`);
      this._inverseCheckBoxElement = $inverseCheckBoxElement.get(0);

      $(".radioButtonsList input[type='radio']", $containerElement).click(this._onByValueSelectorChangedHandler);
      $(document).ready(this._onLoadHandler);
      $numberTo.data('tTextBox').disable();

      $numberFrom = null;
      $numberTo = null;
      $inverseCheckBoxElement = null;
      $containerElement = null;
    }
  }

  getSearchQuery() {
    return BackendArticleSearchBlock.createFieldSearchQuery(
      Quantumart.QP8.Enums.ArticleFieldSearchType.Identifier,
      this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID,
      this.getIsNull(),
      $(this._numberFromElement).data('tTextBox').value(),
      $(this._numberToElement).data('tTextBox').value(),
      this._isByValue,
      this._getIds(jQuery(this._textAreaElement).val()),
      this._isByText);
  }

  getBlockState() {
    return new FieldSearchState(
      Quantumart.QP8.Enums.ArticleFieldSearchType.Identifier,
      this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
      {
        inverse: this.getIsNull(),
        from: $(this._numberFromElement).data('tTextBox').value(),
        to: $(this._numberToElement).data('tTextBox').value(),
        text: $(this._textAreaElement).val(),
        isByValue: this._isByValue,
        isByText: this._isByText
      });
  }

  getFilterDetails() {
    const stateData = this.getBlockState().data;
    let result;

    if (stateData.isByText) {
      const ids = this._getIds(stateData.text);
      result = ids.length === 0 ? '?' : this._getText(ids);
    } else if (stateData.isByValue) {
      result = stateData.from ? stateData.from : '?';
    } else {
      result = `[${stateData.from ? stateData.from : '?'}..${stateData.to ? stateData.to : '?'}]`;
    }

    if (stateData.inverse) {
      result = `${$l.SearchBlock.notText}(${result})`;
    }

    return result;
  }

  restoreBlockState(state) {
    if (state) {
      if (this._inverseCheckBoxElement) {
        let $inverseCheckBoxElement = $(this._inverseCheckBoxElement);
        $inverseCheckBoxElement.prop('checked', state.inverse);
        $inverseCheckBoxElement = null;
      }

      if (!$q.isNull(state.isByValue) && !$q.isNull(state.isByText)) {
        let value = 1;
        if (state.isByText) {
          value = 2;
        } else if (state.isByValue) {
          value = 0;
        }
        $(`.radioButtonsList input:radio[value=${value}]`, this._containerElement)
          .prop('checked', true).trigger('click');
      }

      $(this._numberFromElement).data('tTextBox').value(state.from);
      $(this._numberToElement).data('tTextBox').value(state.to);
      $(this._textAreaElement).val(state.text);
    }
  }

  _onByValueSelectorChanged(e) {
    this._isByValue = +$(e.currentTarget).val() === 0;
    this._isByText = +$(e.currentTarget).val() === 2;

    if (this._isByText) {
      $(this._textAreaElement).closest('.row').show();
      $(this._numberFromElement).closest('.row').hide();
      $(this._numberToElement).closest('.row').hide();
    } else if (this._isByValue) {
      $(this._numberToElement).data('tTextBox').disable();
      $(this._textAreaElement).closest('.row').hide();
      $(this._numberFromElement).closest('.row').show();
      $(this._numberToElement).closest('.row').hide();
      $(`label[for='${$(this._numberFromElement).attr('id')}']`, this._containerElement).text($l.SearchBlock.valueText);
    } else {
      $(this._numberToElement).data('tTextBox').enable();
      $(`label[for='${$(this._numberFromElement).attr('id')}']`, this._containerElement).text($l.SearchBlock.fromText);
      $(this._textAreaElement).closest('.row').hide();
      $(this._numberFromElement).closest('.row').show();
      $(this._numberToElement).closest('.row').show();
    }
  }

  // перенести значение из одного numeric textbox в другой если другой - пустой
  _onNumericInputFocus(e) {
    const focusedNumeric = $(e.currentTarget).data('tTextBox');
    let otherInput;
    if (e.currentTarget === this._numberFromElement) {
      otherInput = $(this._numberToElement).data('tTextBox');
    } else if (e.currentTarget === this._numberToElement) {
      otherInput = $(this._numberFromElement).data('tTextBox');
    }

    if (otherInput && otherInput.value() && focusedNumeric && !focusedNumeric.value()) {
      focusedNumeric.value(otherInput.value());
    }
  }

  _onLoad() {
    $c.initAllNumericTextBoxes(this._containerElement);
  }

  dispose() {
    if (this._numberFromElement) {
      $(this._numberFromElement).unbind('focus', this._onNumericInputFocusHandler);
    }
    if (this._numberToElement) {
      $(this._numberToElement).unbind('focus', this._onNumericInputFocusHandler);
    }

    $c.destroyAllNumericTextBoxes(this._containerElement);

    let $containerElement = $(this._containerElement);
    $(".radioButtonsList input[type='radio']", $containerElement).unbind();
    $containerElement = null;

    this._inverseCheckBoxElement = null;
    this._numberFromElement = null;
    this._numberToElement = null;
    this._textAreaElement = null;

    this._onByValueSelectorChangedHandler = null;
    this._onNumericInputFocusHandler = null;
    this._onLoadHandler = null;

    super.dispose();
  }


  getIsNull() {
    if (this._inverseCheckBoxElement) {
      return $(this._inverseCheckBoxElement).is(':checked');
    }
    return false;
  }

  _isByValue = true;
  _isByText = false;

  _onNumericInputFocusHandler = null;

  _inverseCheckBoxElement = null;
  _numberFromElement = null;
  _numberToElement = null;
  _textAreaElement = null;
}


import('../BackendArticleSearchBlock').then(() => {
  Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch = IdentifierFieldSearch;
});
