Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
  Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);

  this._onIsNullCheckBoxChangeHandler = $.proxy(this._onIsNullCheckBoxChange, this);
};

Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch.prototype = {
  initialize: function () {
    var isNullCheckBoxID = `${this._elementIdPrefix}_isNullCheckBox`;
    var radioGroupName = `${this._elementIdPrefix}_radioGroup`;
    var disablingContainerID = `${this._elementIdPrefix}_disablingContainer`;
    var radioTrueID = `${this._elementIdPrefix}_radioTrue`;
    var radioFalseID = `${this._elementIdPrefix}_radioFalse`;

    var html = new $.telerik.stringBuilder();
    html
      .cat('<div class="row">')
      .cat('  <div ')
      .cat(` id="${$q.htmlEncode(disablingContainerID)}"`)
      .cat(' class="radioButtonsList horizontalDirection"')
      .cat('>\n')
      .cat('    <ul>\n')
      .cat('      <li>\n')
      .cat('        <input type="radio" ')
      .cat(` id="${$q.htmlEncode(radioTrueID)}"`)
      .cat(` name="${$q.htmlEncode(radioGroupName)}"`)
      .cat(' value="true" checked="checked"')
      .cat(' />\n')
      .cat(`        <label for="${$q.htmlEncode(radioTrueID)}">${$l.SearchBlock.trueText}</label>\n`)
      .cat('      </li>\n')
      .cat('      <li>\n')
      .cat('        <input type="radio"')
      .cat(` id="${$q.htmlEncode(radioFalseID)}"`)
      .cat(` name="${$q.htmlEncode(radioGroupName)}"`)
      .cat(' />\n')
      .cat(`        <label for="${$q.htmlEncode(radioFalseID)}">${$l.SearchBlock.falseText}</label>`)
      .cat('      </li>\n')
      .cat('    </ul>\n')
      .cat('  </div>\n')
      .cat('</div>\n')
      .cat('<div class="row">')
      .cat('  <span class="checkbox">\n')
      .cat('    <input type="checkbox"')
      .cat(` id="${$q.htmlEncode(isNullCheckBoxID)}"`)
      .cat(' />\n')
      .cat(`    <label for="${$q.htmlEncode(isNullCheckBoxID)}">${$l.SearchBlock.isNullCheckBoxLabelText}</label>`)
      .cat('  </span>')
      .cat('</div>')
    ;

    // добавить разметку на страницу
    var $containerElement = $(this._containerElement);
    $containerElement.append(html.string());

    // назначить обработчик события change чекбоксу
    var $isNullCheckBoxElement = $containerElement.find(`#${isNullCheckBoxID}`);
    $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

    // запомнить ссылку на dom-элементы
    this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

    $isNullCheckBoxElement = null;
    $containerElement = null;
    html = null;
  },

  get_searchQuery: function () {
    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(Quantumart.QP8.Enums.ArticleFieldSearchType.Boolean, this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID,
      this.get_IsNull(), this._getValue());
  },

  get_blockState: function () {
    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(Quantumart.QP8.Enums.ArticleFieldSearchType.Boolean, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
      {
        isNull: this.get_IsNull(),
        value: this._getValue()
      });
  },

  get_filterDetails: function () {
    var stateData = this.get_blockState().data;
    if (stateData.isNull) {
      return $l.SearchBlock.isNullCheckBoxLabelText;
    }
    return stateData.value === true ? $l.SearchBlock.trueText : $l.SearchBlock.falseText;

  },

  restore_blockState: function (state) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
        $isNullCheckBoxElement.prop('checked', state.isNull);
        $isNullCheckBoxElement.trigger('change');
        $isNullCheckBoxElement = null;
      }
      if (!$q.isNull(state.value)) {
        if (state.value === true) {
          $('input:radio[value=true]', this._containerElement).prop('checked', true);
        } else if (state.value === false) {
          $('input:radio[value!=true]', this._containerElement).prop('checked', true);
        }
      }
    }
  },

  _getValue: function () {
    var result = null;
    var val = $(this._containerElement).find('input:radio:checked').val();
    if (val == 'true') {
      result = true;
    } else {
      result = false;
    }
    return result;
  },

  _onIsNullCheckBoxChange: function () {
    $(this._containerElement)
      .find(`#${this._elementIdPrefix}_disablingContainer *`)
      .prop('disabled', this.get_IsNull());
  },

  dispose: function () {
    if (this._isNullCheckBoxElement) {
      var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
      $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
      $isNullCheckBoxElement = null;
    }

    this._isNullCheckBoxElement = null;
    this._onIsNullCheckBoxChangeHandler = null;

    Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch.callBaseMethod(this, 'dispose');
  },

  _onIsNullCheckBoxChangeHandler: null,
  get_IsNull: function () {
    if (this._isNullCheckBoxElement) {
      return $(this._isNullCheckBoxElement).is(':checked');
    }
    return false;

  },

  _isNullCheckBoxElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch.registerClass('Quantumart.QP8.BackendArticleSearchBlock.BooleanFieldSearch', Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
