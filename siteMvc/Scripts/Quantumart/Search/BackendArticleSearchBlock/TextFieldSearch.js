// eslint-disable-next-line max-params
Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch = function (
  containerElement,
  parentEntityId,
  fieldID,
  contentID,
  fieldColumn,
  fieldName,
  fieldGroup,
  referenceFieldID
) {
  Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.initializeBase(
    this,
    [
      containerElement,
      parentEntityId,
      fieldID,
      contentID,
      fieldColumn,
      fieldName,
      fieldGroup,
      referenceFieldID
    ]
  );

  this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
};

Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.prototype = {
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

      const $containerElement = $(this._containerElement);
      $containerElement.append(serverContent);

      const $isNullCheckBoxElement = $containerElement.find(`#${isNullCheckBoxID}`);
      $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

      this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

      const $inverseCheckBoxElement = $containerElement.find(`#${inverseCheckBoxID}`);
      this._inverseCheckBoxElement = $inverseCheckBoxElement.get(0);

      const $exactMatchCheckBoxElement = $containerElement.find(`#${exactMatchCheckBoxID}`);
      this._exactMatchCheckBoxElement = $exactMatchCheckBoxElement.get(0);

      const $beginningStartCheckBoxElement = $containerElement.find(`#${beginningCheckBoxID}`);
      this._beginningStartChechBoxElement = $beginningStartCheckBoxElement.get(0);
      this._queryTextBoxElement = $containerElement.find(`#${queryTextBoxID}`).get(0);
    }
  },

  getSearchQuery() {
    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(
      Quantumart.QP8.Enums.ArticleFieldSearchType.Text,
      this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, this.getIsNull(),
      $(this._queryTextBoxElement).val(), this.getInverse(), this.getExactMatch(), this.getBeginningStart());
  },

  getBlockState() {
    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(
      Quantumart.QP8.Enums.ArticleFieldSearchType.Text,
      this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
      {
        isNull: this.getIsNull(),
        text: $(this._queryTextBoxElement).val(),
        inverse: this.getInverse(),
        exactMatch: this.getExactMatch(),
        beginningStart: this.getBeginningStart()
      });
  },

  getFilterDetails() {
    const stateData = this.getBlockState().data;
    let result;

    if (stateData.text) {
      result = `"${$q.cutShort(stateData.text, 8)}"`;
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
  },

  _onIsNullCheckBoxChange() {
    $(this._queryTextBoxElement).prop('disabled', this.getIsNull());
    $(this._exactMatchCheckBoxElement).prop('disabled', this.getIsNull());
    $(this._beginningStartChechBoxElement).prop('disabled', this.getIsNull());
  },

  dispose() {
    if (this._isNullCheckBoxElement) {
      let $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
      $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
      $isNullCheckBoxElement = null;
    }

    this._isNullCheckBoxElement = null;
    this._inverseCheckBoxElement = null;
    this._queryTextBoxElement = null;
    this._beginningStartChechBoxElement = null;
    this._exactMatchCheckBoxElement = null;
    this._onIsNullCheckBoxChangeHandler = null;

    Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.callBaseMethod(this, 'dispose');
  },

  _onIsNullCheckBoxChangeHandler: null,

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

  getExactMatch() {
    if (this._exactMatchCheckBoxElement) {
      return $(this._exactMatchCheckBoxElement).is(':checked');
    }

    return false;
  },

  getBeginningStart() {
    if (this._beginningStartChechBoxElement) {
      return $(this._beginningStartChechBoxElement).is(':checked');
    }

    return false;
  },

  _queryTextBoxElement: null,
  _isNullCheckBoxElement: null,
  _inverseCheckBoxElement: null,
  _beginningStartChechBoxElement: null,
  _exactMatchCheckBoxElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.registerClass(
  'Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch',
  Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase
);
