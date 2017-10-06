Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch = function (
  containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID
) {
  Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch.initializeBase(
    this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]
  );
  this._onIsNullCheckBoxChangeHandler = $.proxy(this._onIsNullCheckBoxChange, this);
  this._onByValueSelectorChangedHandler = $.proxy(this._onByValueSelectorChanged, this);
  this._onNumericInputFocusHandler = $.proxy(this._onNumericInputFocus, this);
  this._onLoadHandler = $.proxy(this._onLoad, this);
};

Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch.prototype = {
  initialize() {
    let serverContent;
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK}NumericRange`,
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
      const numberFromID = `${this._elementIdPrefix}_numberFrom`;
      const numberToID = `${this._elementIdPrefix}_numberTo`;
      const inverseCheckBoxID = `${this._elementIdPrefix}_inverseCheckBox`;

      let $containerElement = $(this._containerElement);
      $containerElement.html(serverContent);

      let $numberFrom = $containerElement.find(`#${numberFromID}`);
      let $numberTo = $containerElement.find(`#${numberToID}`);
      $numberFrom.focus(this._onNumericInputFocusHandler);
      $numberTo.focus(this._onNumericInputFocusHandler);

      this._numberFromElement = $numberFrom.get(0);
      this._numberToElement = $numberTo.get(0);

      let $isNullCheckBoxElement = $containerElement.find(`#${isNullCheckBoxID}`);
      $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

      this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

      const $inverseCheckBoxElement = $containerElement.find(`#${inverseCheckBoxID}`);
      this._inverseCheckBoxElement = $inverseCheckBoxElement.get(0);

      $(".radioButtonsList input[type='radio']", $containerElement).click(this._onByValueSelectorChangedHandler);

      $(document).ready(this._onLoadHandler);

      $numberTo.data('tTextBox').disable();

      $numberFrom = null;
      $numberTo = null;
      $isNullCheckBoxElement = null;
      $containerElement = null;
    }
  },

  getSearchQuery() {
    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(
      Quantumart.QP8.Enums.ArticleFieldSearchType.NumericRange,
      this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID,
      this.getIsNull(),
      $(this._numberFromElement).data('tTextBox').value(),
      $(this._numberToElement).data('tTextBox').value(),
      this._isByValue,
      this.getInverse()
    );
  },

  getBlockState() {
    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(
      Quantumart.QP8.Enums.ArticleFieldSearchType.NumericRange, this._fieldID, this._contentID,
      this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
      {
        isNull: this.getIsNull(),
        from: $(this._numberFromElement).data('tTextBox').value(),
        to: $(this._numberToElement).data('tTextBox').value(),
        isByValue: this._isByValue,
        inverse: this.getInverse()
      });
  },

  getFilterDetails() {
    const stateData = this.getBlockState().data;
    let result;
    if (stateData.isNull) {
      result = $l.SearchBlock.isNullCheckBoxLabelText;
    } else if (stateData.isByValue) {
      result = $.isNumeric(stateData.from) ? stateData.from : '?';
    } else {
      const from = $.isNumeric(stateData.from) ? stateData.from : '?';
      const to = $.isNumeric(stateData.to) ? stateData.to : '?';
      result = `[${from}..${to}]`;
    }

    if (stateData.inverse) {
      result = `${$l.SearchBlock.notText}(${result})`;
    }
    return result;
  },

  restoreBlockState(state) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
        $isNullCheckBoxElement.prop('checked', state.isNull);
        $isNullCheckBoxElement.trigger('change');
        $isNullCheckBoxElement = null;
      }

      if (this._inverseCheckBoxElement) {
        const $inverseCheckBoxElement = $(this._inverseCheckBoxElement);
        $inverseCheckBoxElement.prop('checked', state.inverse);
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

      $(this._numberFromElement).data('tTextBox').value(state.from);
      $(this._numberToElement).data('tTextBox').value(state.to);
    }
  },

  _onIsNullCheckBoxChange() {
    if (this.getIsNull()) {
      $(this._numberFromElement).data('tTextBox').disable();
      $(this._numberToElement).data('tTextBox').disable();
    } else {
      $(this._numberFromElement).data('tTextBox').enable();
      if (!this._isByValue) {
        $(this._numberToElement).data('tTextBox').enable();
      }
    }
  },

  _onByValueSelectorChanged(e) {
    this._isByValue = $(e.currentTarget).val() === 0;

    if (this._isByValue === true) {
      $(this._numberToElement).data('tTextBox').disable();
      $(this._numberToElement).closest('.row').hide();
      $(`label[for='${$(this._numberFromElement).attr('id')}']`, this._containerElement).text($l.SearchBlock.valueText);
    } else {
      if (!this.getIsNull()) {
        $(this._numberToElement).data('tTextBox').enable();
      }
      $(`label[for='${$(this._numberFromElement).attr('id')}']`, this._containerElement).text($l.SearchBlock.fromText);
      $(this._numberToElement).closest('.row').show();
    }
  },

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
  },

  _onLoad() {
    $c.initAllNumericTextBoxes(this._containerElement);
  },

  dispose() {
    if (this._isNullCheckBoxElement) {
      let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
      $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
      $isNullCheckBoxElement = null;
    }

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

    this._isNullCheckBoxElement = null;
    this._inverseCheckBoxElement = null;
    this._numberFromElement = null;
    this._numberToElement = null;

    this._onIsNullCheckBoxChangeHandler = null;
    this._onByValueSelectorChangedHandler = null;
    this._onNumericInputFocusHandler = null;
    this._onLoadHandler = null;

    Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch.callBaseMethod(this, 'dispose');
  },


  getIsNull() {
    if (this._isNullCheckBoxElement) {
      return $(this._isNullCheckBoxElement).is(':checked');
    }
    return false;
  },

  getInverse() {
    if (this._inverseCheckBoxElement) {
      return $(this._inverseCheckBoxElement).is(':checked');
    }
    return false;
  },

  _isByValue: true,

  _onIsNullCheckBoxChangeHandler: null,
  _onNumericInputFocusHandler: null,

  _isNullCheckBoxElement: null,
  _numberFromElement: null,
  _numberToElement: null,
  _inverseCheckBoxElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch.registerClass(
  'Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch',
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase
);
