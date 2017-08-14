Quantumart.QP8.BackendArticleSearchBlock.FullTextBlock = function (fullTextBlockElement, parentEntityId) {
  this._fullTextBlockElement = fullTextBlockElement;
  this._parentEntityId = parentEntityId;
  this._elementIdPrefix = Quantumart.QP8.BackendSearchBlockBase.generateElementPrefix();
};

Quantumart.QP8.BackendArticleSearchBlock.FullTextBlock.prototype = {
  _fullTextBlockElement: null,
  _textFieldsComboElement: null,
  _queryTextBoxElement: null,
  _parentEntityId: 0,
  _elementIdPrefix: '',

  initialize: function () {
    var serverContent;

    $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK  }FullTextBlock`, {
      parentEntityId: this._parentEntityId,
      elementIdPrefix: this._elementIdPrefix
    }, false, false, function (data) {
      if (data.success) {
        serverContent = data.view;
      } else {
        $q.alertError(data.message);
      }
    }, function (jqXHR) {
      serverContent = null;
      $q.processGenericAjaxError(jqXHR);
    });

    if (!$q.isNullOrWhiteSpace(serverContent)) {
      var $fullTextBlockElement = jQuery(this._fullTextBlockElement);

      $fullTextBlockElement.html(serverContent);
      this._textFieldsComboElement = $fullTextBlockElement.find(`#${  this._elementIdPrefix  }_TextFieldsCombo`).get(0);
      jQuery(this._textFieldsComboElement).bind('change', jQuery.proxy(this.onFieldChanged, this));
      this._queryTextBoxElement = $fullTextBlockElement.find(`#${  this._elementIdPrefix  }_QueryTextBox`).get(0);
      $fullTextBlockElement = null;
    }
  },

  get_searchQuery: function () {
    var data = this._get_searchData();

    if (data) {
      return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(Quantumart.QP8.Enums.ArticleFieldSearchType.FullText, data.fieldID, data.fieldColumn, data.contentID, data.referenceFieldID, data.text);
    }
    return null;

  },

  get_blockState: function () {
    var data = this._get_searchData();

    if (data) {
      var state = {};

      if (data.fieldID) {
        state.fieldID = data.fieldID;
      }

      if (data.text) {
        state.text = data.text;
      }

      if (jQuery.isEmptyObject(state)) {
        return null;
      }
      return state;

    }
    return null;

  },

  restore_blockState: function (state) {
    if (state) {
      jQuery(this._queryTextBoxElement).prop('value', state.text);

      var $selectedField = null;

      if (state.fieldID) {
        $selectedField = jQuery(`option[data-field_id='${  state.fieldID  }']`, this._textFieldsComboElement);
      } else {
        $selectedField = jQuery('option[data-field_id=]', this._textFieldsComboElement);
      }

      $selectedField.prop('selected', true);
    }
  },

  _get_searchData: function () {
    if (this._textFieldsComboElement) {
      var $selectedField = jQuery(this._textFieldsComboElement).find('option:selected');

      if ($selectedField) {
        var fieldValue = $selectedField.val();
        var fieldID = $selectedField.data('field_id');
        var contentID = $selectedField.data('content_id');
        var fieldColumn = $selectedField.attr('field_column');
        var referenceFieldID = $selectedField.data('reference_field_id');

        return {
          fieldID: fieldID,
          contentID: contentID,
          referenceFieldID: referenceFieldID,
          fieldColumn: fieldColumn,
          fieldValue: fieldValue,
          text: jQuery(this._queryTextBoxElement).val()
        };
      }
      return null;

    }
    return null;

  },
  clear: function () {
    if (this._textFieldsComboElement) {
      var $resetElem = jQuery(this._textFieldsComboElement).find("option[data-field_is_title='True']");

      if (!$resetElem.length) {
        $resetElem = jQuery(this._textFieldsComboElement).find("option[value='']");
      }
    }

    $resetElem.prop('selected', true);

    // очистить текстовое поле
    if (this._queryTextBoxElement) {
      jQuery(this._queryTextBoxElement).val('');
    }
  },

  onFieldChanged: function () {
    if (jQuery(this._queryTextBoxElement).val()) {
      jQuery(this._fullTextBlockElement).closest('form').trigger('submit');
    }
  },

  dispose: function () {
    jQuery(this._textFieldsComboElement).unbind('change');
    this._fullTextBlockElement = null;
    this._queryTextBoxElement = null;
    this._textFieldsComboElement = null;
  }
};

Quantumart.QP8.BackendArticleSearchBlock.FullTextBlock.registerClass('Quantumart.QP8.BackendArticleSearchBlock.FullTextBlock', null, Sys.IDisposable);
