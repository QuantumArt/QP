// #region class BackendArticleSearchBlock.NumericRangeFieldSearch
// === Класс блока поиска по числовому полю
Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
    Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);
	this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
	this._onByValueSelectorChangedHandler = jQuery.proxy(this._onByValueSelectorChanged, this);
	this._onNumericInputFocusHandler = jQuery.proxy(this._onNumericInputFocus, this);
	this._onLoadHandler = jQuery.proxy(this._onLoad, this);
};

Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch.prototype = {
	initialize: function () {
		// получить разметку с сервера
		var serverContent;

		$q.getJsonFromUrl(
			"GET",
			CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + "NumericRange",
			{
				elementIdPrefix: this._elementIdPrefix
			},
			false,
			false,
			function (data, textStatus, jqXHR) {
				if (data.success)
					{ serverContent = data.view; }
				else
					{ alert(data.message); }
			},
			function (jqXHR, textStatus, errorThrown) {
				serverContent = null;
				$q.processGenericAjaxError(jqXHR);
			}
		);
		if (!$q.isNullOrWhiteSpace(serverContent)) {
			var isNullCheckBoxID = this._elementIdPrefix + '_isNullCheckBox';
			var numberFromID = this._elementIdPrefix + '_numberFrom';
			var numberToID = this._elementIdPrefix + '_numberTo';
			var inverseCheckBoxID = this._elementIdPrefix + '_inverseCheckBox';

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

			// назначить обработчик события change чекбоксу
			var $isNullCheckBoxElement = $containerElement.find("#" + isNullCheckBoxID);
			$isNullCheckBoxElement.bind("change", this._onIsNullCheckBoxChangeHandler);
			// запомнить ссылку на dom-элемент чекбокса
			this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

			var $inverseCheckBoxElement = $containerElement.find("#" + inverseCheckBoxID);
			this._inverseCheckBoxElement = $inverseCheckBoxElement.get(0);

			jQuery(".radioButtonsList input[type='radio']", $containerElement).click(this._onByValueSelectorChangedHandler);

			jQuery(document).ready(this._onLoadHandler);

			$numberTo.data("tTextBox").disable();

			$numberFrom = null;
			$numberTo = null;
			$isNullCheckBoxElement = null;
			$containerElement = null;
		}
	},

	get_searchQuery: function () {
	    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(Quantumart.QP8.Enums.ArticleFieldSearchType.NumericRange, this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID,
	                this.get_IsNull(),
	                jQuery(this._numberFromElement).data("tTextBox").value(),
	                jQuery(this._numberToElement).data("tTextBox").value(),
					this._isByValue,
                    this.get_Inverse()
                );
	},

	get_blockState: function () {
	    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(Quantumart.QP8.Enums.ArticleFieldSearchType.NumericRange, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
		{
		    isNull: this.get_IsNull(),
			from: jQuery(this._numberFromElement).data("tTextBox").value(),
			to: jQuery(this._numberToElement).data("tTextBox").value(),
			isByValue: this._isByValue,
			inverse: this.get_Inverse()
		});
	},

	get_filterDetails: function () {
	    var stateData = this.get_blockState().data;
	    var result;
		if (stateData.isNull) {
			result = $l.SearchBlock.isNullCheckBoxLabelText;
		}
		else if (stateData.isByValue) {
		    result = $.isNumeric(stateData.from) ? stateData.from : "?";
		}
		else {
		    result = "[" + ($.isNumeric(stateData.from) ? stateData.from : "?") + ".." + ($.isNumeric(stateData.to) ? stateData.to : "?") + "]";
		}

		if (stateData.inverse) {
		    result = $l.SearchBlock.notText + "(" + result + ")";
		}
		return result;
	},

	restore_blockState: function (state) {
		if (state) {
			if (this._isNullCheckBoxElement) {
				var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
				$isNullCheckBoxElement.prop("checked", state.isNull);
				$isNullCheckBoxElement.trigger("change");
				$isNullCheckBoxElement = null;
			}

			if (this._inverseCheckBoxElement) {
			    var $inverseCheckBoxElement = jQuery(this._inverseCheckBoxElement);
			    $inverseCheckBoxElement.prop("checked", state.inverse);
			}

			if (!$q.isNull(state.isByValue)) {
				if (state.isByValue === true) {
					jQuery(".radioButtonsList input:radio[value=0]", this._containerElement)
						.prop("checked", true)
						.trigger('click');
				}
				else if (state.isByValue === false) {
					jQuery(".radioButtonsList input:radio[value=1]", this._containerElement)
						.prop("checked", true)
						.trigger('click');
				}
			}

			jQuery(this._numberFromElement).data("tTextBox").value(state.from);
			jQuery(this._numberToElement).data("tTextBox").value(state.to);
		}
	},

	_onIsNullCheckBoxChange: function () {
		// дизейблим текст бокс если пользователь выбрал IS NULL
		if (this.get_IsNull()) {
			jQuery(this._numberFromElement).data("tTextBox").disable();
			jQuery(this._numberToElement).data("tTextBox").disable();
		}
		else {
			jQuery(this._numberFromElement).data("tTextBox").enable();
			if (!this._isByValue)
				{ jQuery(this._numberToElement).data("tTextBox").enable(); }
		}
	},

	_onByValueSelectorChanged: function (e) {
		this._isByValue = jQuery(e.currentTarget).val() == 0;

		if (this._isByValue == true) {
			jQuery(this._numberToElement).data("tTextBox").disable();
			jQuery(this._numberToElement).closest(".row").hide();
			jQuery("label[for='" + jQuery(this._numberFromElement).attr('id') + "']", this._containerElement).text($l.SearchBlock.valueText);
		}
		else {
			if (!this.get_IsNull())
				{ jQuery(this._numberToElement).data("tTextBox").enable(); }
			jQuery("label[for='" + jQuery(this._numberFromElement).attr('id') + "']", this._containerElement).text($l.SearchBlock.fromText);
			jQuery(this._numberToElement).closest(".row").show();
		}
	},

	// перенести значение из одного numeric textbox в другой если другой - пустой
	_onNumericInputFocus: function (e) {
		var focusedNumeric = jQuery(e.currentTarget).data("tTextBox");
		var otherNumeric = null;
		if (e.currentTarget === this._numberFromElement)
			{ otherInput = jQuery(this._numberToElement).data("tTextBox"); }
		else if (e.currentTarget === this._numberToElement)
			{ otherInput = jQuery(this._numberFromElement).data("tTextBox"); }

		if (otherInput && otherInput.value() && focusedNumeric && !focusedNumeric.value()) {
			focusedNumeric.value(otherInput.value());
		}
	},

	_onLoad: function () {
		$c.initAllNumericTextBoxes(this._containerElement);
	},

	dispose: function () {
		// отвязать обработчик события change чекбоксу
		if (this._isNullCheckBoxElement) {
			var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
			$isNullCheckBoxElement.unbind("change", this._onIsNullCheckBoxChangeHandler);
			$isNullCheckBoxElement = null;
		}

		if (this._numberFromElement)
			{ jQuery(this._numberFromElement).unbind("focus", this._onNumericInputFocusHandler); }
		if (this._numberToElement)
			{ jQuery(this._numberToElement).unbind("focus", this._onNumericInputFocusHandler); }

		// удаляем все NumericTextBoxes
		$c.destroyAllNumericTextBoxes(this._containerElement);

		var $containerElement = jQuery(this._containerElement);
		jQuery(".radioButtonsList input[type='radio']", $containerElement).unbind();
		$containerElement = null;

		this._isNullCheckBoxElement = null;
		this._inverseCheckBoxElement = null;
		this._numberFromElement = null;
		this._numberToElement = null;

		this._onIsNullCheckBoxChangeHandler = null;
		this._onByValueSelectorChangedHandler = null;
		this._onNumericInputFocusHandler = null;
		this._onLoadHandler = null;

		Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch.callBaseMethod(this, "dispose");
	},


	get_IsNull: function () {
		if (this._isNullCheckBoxElement)
			{ return jQuery(this._isNullCheckBoxElement).is(":checked"); }
		else
			{ return false; }
	},

	get_Inverse: function () {
	    if (this._inverseCheckBoxElement)
	        { return jQuery(this._inverseCheckBoxElement).is(":checked"); }
	    else
	        { return false; }
	},

	_isByValue: true,

	_onIsNullCheckBoxChangeHandler: null, // обработчик клика на чекбоксе IS NULL
	_onNumericInputFocusHandler: null, // обработчик потери фокуса

	_isNullCheckBoxElement: null, // dom-элемент чекбокса isNull
	_numberFromElement: null, // dom-элемент поля "От"
	_numberToElement: null, // dom-элемент поля "От"
	_inverseCheckBoxElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch.registerClass("Quantumart.QP8.BackendArticleSearchBlock.NumericRangeFieldSearch", Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
// #endregion
