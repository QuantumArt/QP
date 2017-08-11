// #region class BackendArticleSearchBlock.IdentifierFieldSearch
// === Класс блока поиска по числовому полю
Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
    Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);
	this._onByValueSelectorChangedHandler = jQuery.proxy(this._onByValueSelectorChanged, this);
	this._onNumericInputFocusHandler = jQuery.proxy(this._onNumericInputFocus, this);
	this._onLoadHandler = jQuery.proxy(this._onLoad, this);
};

Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch.prototype = {
	initialize: function () {
		// получить разметку с сервера
		var serverContent;

		$q.getJsonFromUrl(
			"GET",
			CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + "Identifier",
			{
				elementIdPrefix: this._elementIdPrefix
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
		    var inverseCheckBoxID = this._elementIdPrefix + '_inverseCheckBox';
			var numberFromID = this._elementIdPrefix + '_numberFrom';
			var numberToID = this._elementIdPrefix + '_numberTo';

			// полученную с сервера разметку добавить на страницу
			var $containerElement = jQuery(this._containerElement);
			$containerElement.html(serverContent);


			var $numberFrom = $containerElement.find("#" + numberFromID);
			var $numberTo = $containerElement.find("#" + numberToID);
			$numberFrom.focus(this._onNumericInputFocusHandler);
			$numberTo.focus(this._onNumericInputFocusHandler);

			// получить ссылки на dom-элеметы со значениями
			this._numberFromElement = $numberFrom.get(0);
			this._numberToElement = $numberTo.get(0);
			this._textAreaElement = $containerElement.find("#" + this._elementIdPrefix + "_text").get(0);

		    // запомнить ссылку на dom-элемент чекбокса
			var $inverseCheckBoxElement = $containerElement.find("#" + inverseCheckBoxID);
			this._inverseCheckBoxElement = $inverseCheckBoxElement.get(0);

			jQuery(".radioButtonsList input[type='radio']", $containerElement).click(this._onByValueSelectorChangedHandler);

			jQuery(document).ready(this._onLoadHandler);

			$numberTo.data("tTextBox").disable();

			$numberFrom = null;
			$numberTo = null;
			$inverseCheckBoxElement = null;
			$containerElement = null;
		}
	},

	get_searchQuery: function () {
	    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(Quantumart.QP8.Enums.ArticleFieldSearchType.Identifier, this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID,
	                this.get_IsNull(),
	                jQuery(this._numberFromElement).data("tTextBox").value(),
	                jQuery(this._numberToElement).data("tTextBox").value(),
					this._isByValue,
                    this._getIds(jQuery(this._textAreaElement).val()),
                    this._isByText);
	},

	get_blockState: function () {
	    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(Quantumart.QP8.Enums.ArticleFieldSearchType.Identifier, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
		{
		    inverse: this.get_IsNull(),
			from: jQuery(this._numberFromElement).data("tTextBox").value(),
			to: jQuery(this._numberToElement).data("tTextBox").value(),
			text: jQuery(this._textAreaElement).val(),
			isByValue: this._isByValue,
			isByText: this._isByText
		});
	},

	get_filterDetails: function () {
	    var stateData = this.get_blockState().data;
	    var result;

		if (stateData.isByText) {
		    var ids = this._getIds(stateData.text);
		    result = ids.length == 0 ? "?" : this._getText(ids);
		} else if (stateData.isByValue) {
		    result = stateData.from ? stateData.from : "?";
		} else {
		    result = "[" + (stateData.from ? stateData.from : "?") + ".." + (stateData.to ? stateData.to : "?") + "]";
		}

		if (stateData.inverse) {
		    result = $l.SearchBlock.notText + "(" + result + ")";
		}

		return result;
	},

	restore_blockState: function (state) {
		if (state) {
			if (this._inverseCheckBoxElement) {
			    var $inverseCheckBoxElement = jQuery(this._inverseCheckBoxElement);
			    $inverseCheckBoxElement.prop("checked", state.inverse);
			    $inverseCheckBoxElement = null;
			}

			if (!$q.isNull(state.isByValue) && !$q.isNull(state.isByText)) {
			    var value = state.isByText ? 2 : state.isByValue ? 0 : 1;
			    jQuery(".radioButtonsList input:radio[value=" + value + "]", this._containerElement).prop("checked", true).trigger('click');
			}

			jQuery(this._numberFromElement).data("tTextBox").value(state.from);
			jQuery(this._numberToElement).data("tTextBox").value(state.to);
			jQuery(this._textAreaElement).val(state.text);
		}
	},

	_onByValueSelectorChanged: function (e) {
	    this._isByValue = jQuery(e.currentTarget).val() == 0;
	    this._isByText = jQuery(e.currentTarget).val() == 2;

	    if (this._isByText) {
	        jQuery(this._textAreaElement).closest(".row").show();
	        jQuery(this._numberFromElement).closest(".row").hide();
	        jQuery(this._numberToElement).closest(".row").hide();
	    } else if (this._isByValue) {
			jQuery(this._numberToElement).data("tTextBox").disable();
			jQuery(this._textAreaElement).closest(".row").hide();
			jQuery(this._numberFromElement).closest(".row").show();
			jQuery(this._numberToElement).closest(".row").hide();
			jQuery("label[for='" + jQuery(this._numberFromElement).attr('id') + "']", this._containerElement).text($l.SearchBlock.valueText);
		} else {
			jQuery(this._numberToElement).data("tTextBox").enable();
			jQuery("label[for='" + jQuery(this._numberFromElement).attr('id') + "']", this._containerElement).text($l.SearchBlock.fromText);
			jQuery(this._textAreaElement).closest(".row").hide();
			jQuery(this._numberFromElement).closest(".row").show();
			jQuery(this._numberToElement).closest(".row").show();
        }
	},

	// перенести значение из одного numeric textbox в другой если другой - пустой
	_onNumericInputFocus: function (e) {
		var focusedNumeric = jQuery(e.currentTarget).data("tTextBox");
		var otherNumeric = null;
		if (e.currentTarget === this._numberFromElement) {
 otherInput = jQuery(this._numberToElement).data("tTextBox");
} else if (e.currentTarget === this._numberToElement) {
 otherInput = jQuery(this._numberFromElement).data("tTextBox");
}

		if (otherInput && otherInput.value() && focusedNumeric && !focusedNumeric.value()) {
			focusedNumeric.value(otherInput.value());
		}
	},

	_onLoad: function () {
		$c.initAllNumericTextBoxes(this._containerElement);
	},

	dispose: function () {
		if (this._numberFromElement) {
 jQuery(this._numberFromElement).unbind("focus", this._onNumericInputFocusHandler);
}
		if (this._numberToElement) {
 jQuery(this._numberToElement).unbind("focus", this._onNumericInputFocusHandler);
}

		// удаляем все NumericTextBoxes
		$c.destroyAllNumericTextBoxes(this._containerElement);

		var $containerElement = jQuery(this._containerElement);
		jQuery(".radioButtonsList input[type='radio']", $containerElement).unbind();
		$containerElement = null;

		this._inverseCheckBoxElement = null;
		this._numberFromElement = null;
		this._numberToElement = null;
		this._textAreaElement = null;

		this._onByValueSelectorChangedHandler = null;
		this._onNumericInputFocusHandler = null;
		this._onLoadHandler = null;

		Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch.callBaseMethod(this, "dispose");
	},


	get_IsNull: function () {
		if (this._inverseCheckBoxElement) {
 return jQuery(this._inverseCheckBoxElement).is(":checked");
} else {
 return false;
}
	},

	_isByValue: true,
    _isByText: false,

	_onNumericInputFocusHandler: null, // обработчик потери фокуса

	_inverseCheckBoxElement: null, // dom-элемент чекбокса inverse
	_numberFromElement: null, // dom-элемент поля "От"
	_numberToElement: null, // dom-элемент поля "От"
    _textAreaElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch.registerClass("Quantumart.QP8.BackendArticleSearchBlock.IdentifierFieldSearch", Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);

// #endregion
