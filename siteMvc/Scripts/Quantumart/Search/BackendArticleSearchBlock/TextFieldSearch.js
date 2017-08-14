Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
  Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);
  this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
};

Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.prototype = {
  initialize: function () {
    var serverContent;
    $q.getJsonFromUrl(
			'GET',
			window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + 'TextSearch',
			{
			  elementIdPrefix: this._elementIdPrefix,
			  fieldID: this._fieldID
			},
			false,
			false,
			function (data, textStatus, jqXHR) {
			  if (data.success) {
 serverContent = data.view;
} else {
 $q.alertFail(data.message);
}
			},
		    function (jqXHR, textStatus, errorThrown) {
		      serverContent = null;
		      $q.processGenericAjaxError(jqXHR);
		    }
		);
    if (!$q.isNullOrWhiteSpace(serverContent)) {
      var queryTextBoxID = this._elementIdPrefix + '_textBox';
      var inverseCheckBoxID = this._elementIdPrefix + '_inverseCheckBox';
      var isNullCheckBoxID = this._elementIdPrefix + '_isNullCheckBox';
      var exactMatchCheckBoxID = this._elementIdPrefix + '_exactCheckBox';
      var beginningCheckBoxID = this._elementIdPrefix + '_beginningCheckBox';

      var $containerElement = $(this._containerElement);
      $containerElement.append(serverContent);

      var $isNullCheckBoxElement = $containerElement.find('#' + isNullCheckBoxID);
      $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

      this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

      var $inverseCheckBoxElement = $containerElement.find('#' + inverseCheckBoxID);
      this._inverseCheckBoxElement = $inverseCheckBoxElement.get(0);

      var $exactMatchCheckBoxElement = $containerElement.find('#' + exactMatchCheckBoxID);
      this._exactMatchCheckBoxElement = $exactMatchCheckBoxElement.get(0);

      var $beginningStartCheckBoxElement = $containerElement.find('#' + beginningCheckBoxID);
      this._beginningStartChechBoxElement = $beginningStartCheckBoxElement.get(0);
      this._queryTextBoxElement = $containerElement.find('#' + queryTextBoxID).get(0);
    }
  },

  get_searchQuery: function () {
    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(Quantumart.QP8.Enums.ArticleFieldSearchType.Text,
	        this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, this.get_IsNull(),
	        $(this._queryTextBoxElement).val(), this.get_Inverse(), this.get_ExactMatch(), this.get_BeginningStart());
  },

  get_blockState: function () {
    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(Quantumart.QP8.Enums.ArticleFieldSearchType.Text, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
  {
    isNull: this.get_IsNull(),
    text: $(this._queryTextBoxElement).val(),
    inverse: this.get_Inverse(),
    exactMatch: this.get_ExactMatch(),
    beginningStart: this.get_BeginningStart()
  });
  },

  get_filterDetails: function () {
    var stateData = this.get_blockState().data;
    var result;

    if (stateData.text) {
      result = '"' + $q.cutShort(stateData.text, 8) + '"';
    } else {
      result = '""';
    }

    if (stateData.isNull) {
      result = $l.SearchBlock.isNullCheckBoxLabelText;
    } else if (stateData.exactMatch) {
      result = '=' + result;
    } else if (stateData.beginningStart) {
      if (stateData.inverse) {
        return $l.SearchBlock.endText + result;
      }

        return $l.SearchBlock.fromText + result;
    }

    if (stateData.inverse) {
      result = $l.SearchBlock.notText + '(' + result + ')';
    }

    return result;
  },

  restore_blockState: function (state) {
    if (state) {
      if (this._isNullCheckBoxElement) {
        var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
        $isNullCheckBoxElement.prop('checked', state.isNull);
        $isNullCheckBoxElement.trigger('change');
        $isNullCheckBoxElement = null;
      }

      if (this._inverseCheckBoxElement) {
        var $inverseCheckBoxElement = $(this._inverseCheckBoxElement);
        $inverseCheckBoxElement.prop('checked', state.inverse);
      }

      if (this._beginningStartChechBoxElement) {
        var $beginningStartCheckBoxElement = $(this._beginningStartChechBoxElement);
        $beginningStartCheckBoxElement.prop('checked', state.beginningStart);
      }

      if (this._exactMatchCheckBoxElement) {
        var $exactMatchCheckBoxElement = $(this._exactMatchCheckBoxElement);
        $exactMatchCheckBoxElement.prop('checked', state.exactMatch);
      }

      $(this._queryTextBoxElement).val(state.text);
    }
  },

  _onIsNullCheckBoxChange: function () {
    $(this._queryTextBoxElement).prop('disabled', this.get_IsNull());
    $(this._exactMatchCheckBoxElement).prop('disabled', this.get_IsNull());
    $(this._beginningStartChechBoxElement).prop('disabled', this.get_IsNull());
  },

  dispose: function () {
    if (this._isNullCheckBoxElement) {
      var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
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

  get_IsNull: function () {
    if (this._isNullCheckBoxElement) {
 return $(this._isNullCheckBoxElement).is(':checked');
}

 return false;
  },

  get_Inverse: function () {
    if (this._inverseCheckBoxElement) {
 return $(this._inverseCheckBoxElement).is(':checked');
}

 return false;
  },

  get_ExactMatch: function () {
    if (this._exactMatchCheckBoxElement) {
 return $(this._exactMatchCheckBoxElement).is(':checked');
}

 return false;
  },

  get_BeginningStart: function () {
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

Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.registerClass('Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch', Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
