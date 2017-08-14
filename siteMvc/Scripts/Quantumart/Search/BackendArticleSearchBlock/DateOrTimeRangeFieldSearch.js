Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID, rangeType) {
    Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);

	this._rangeType = rangeType;

	this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
	this._onByValueSelectorChangedHandler = jQuery.proxy(this._onByValueSelectorChanged, this);
	this._onLoadHandler = jQuery.proxy(this._onLoad, this);
};

Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch.prototype = {
	initialize: function () {
		var serverContent;
		var url = window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK;
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
			var isNullCheckBoxID = this._elementIdPrefix + '_isNullCheckBox';
			var dateFromID = this._elementIdPrefix + '_dateFrom';
			var dateToID = this._elementIdPrefix + '_dateTo';

			// полученную с сервера разметку добавить на страницу
			var $containerElement = $(this._containerElement);
			$containerElement.html(serverContent);

			// получить ссылки на dom-элеметы со значениями
			this._dateFromElement = $containerElement.find('#' + dateFromID).get(0);
			this._dateToElement = $containerElement.find('#' + dateToID).get(0);
			$c.disableDateTimePicker(this._dateToElement);

			// назначить обработчик события change чекбоксу
			var $isNullCheckBoxElement = $containerElement.find('#' + isNullCheckBoxID);
			$isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);

			// запомнить ссылку на dom-элемент чекбокса
			this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);


			$(".radioButtonsList input[type='radio']", $containerElement).click(this._onByValueSelectorChangedHandler);

			$(document).ready(this._onLoadHandler);

			$isNullCheckBoxElement = null;
			$containerElement = null;
		}
	},

	get_searchQuery: function () {
	    return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(this._rangeType, this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID,
	                this.get_IsNull(),
	                $c.getDateTimePickerValue(this._dateFromElement),
	                $c.getDateTimePickerValue(this._dateToElement),
					this._isByValue);
	},

	get_blockState: function () {
	    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(this._rangeType, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
		{
			isNull: this.get_IsNull(),
			from: $c.getDateTimePickerValue(this._dateFromElement),
			to: $c.getDateTimePickerValue(this._dateToElement),
			isByValue: this._isByValue
		});
	},

	get_filterDetails: function () {
		var stateData = this.get_blockState().data;
		if (stateData.isNull) {
			return $l.SearchBlock.isNullCheckBoxLabelText;
		} else if (stateData.isByValue) {
			return stateData.from ? stateData.from : '?';
		}
			return (stateData.from ? stateData.from : '?') + ' - ' + (stateData.to ? stateData.to : '?');

	},

	restore_blockState: function (state) {
		if (state) {
			if (this._isNullCheckBoxElement) {
				var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
				$isNullCheckBoxElement.prop('checked', state.isNull);
				$isNullCheckBoxElement.trigger('change');
				$isNullCheckBoxElement = null;
			}

			if (!$q.isNull(state.isByValue)) {
				if (state.isByValue === true) {
					$('.radioButtonsList input:radio[value=0]', this._containerElement)
						.prop('checked', true)
						.trigger('click');
				} else if (state.isByValue === false) {
					$('.radioButtonsList input:radio[value=1]', this._containerElement)
						.prop('checked', true)
						.trigger('click');
				}
			}

			$c.setDateTimePickerValue(this._dateFromElement, state.from);
			$c.setDateTimePickerValue(this._dateToElement, state.to);
		}
	},

	_onIsNullCheckBoxChange: function () {
		// дизейблим текст бокс если пользователь выбрал IS NULL
		if (this.get_IsNull()) {
			$c.disableDateTimePicker(this._dateFromElement);
			$c.disableDateTimePicker(this._dateToElement);
		} else {
			$c.enableDateTimePicker(this._dateFromElement);
			if (!this._isByValue) {
 $c.enableDateTimePicker(this._dateToElement);
}
		}
	},

	_onByValueSelectorChanged: function (e) {
		this._isByValue = $(e.currentTarget).val() == 0;

		if (this._isByValue == true) {
			$c.disableDateTimePicker(this._dateToElement);
			$(this._dateToElement).closest('.row').hide();
			$("label[for='" + $(this._dateFromElement).attr('id') + "']", this._containerElement).text($l.SearchBlock.valueText);
		} else {
			if (!this.get_IsNull()) {
 $c.enableDateTimePicker(this._dateToElement);
}
			$("label[for='" + $(this._dateFromElement).attr('id') + "']", this._containerElement).text($l.SearchBlock.fromText);
			$(this._dateToElement).closest('.row').show();
		}
	},

	_onLoad: function () {
		$c.initAllDateTimePickers(this._containerElement);
	},

	dispose: function () {
		if (this._isNullCheckBoxElement) {
			var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
			$isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
			$isNullCheckBoxElement = null;
		}

		var $containerElement = $(this._containerElement);
		$(".radioButtonsList input[type='radio']", $containerElement).unbind();
		$containerElement = null;

		$c.destroyAllDateTimePickers(this._containerElement);

		this._isNullCheckBoxElement = null;
		this._dateFromElement = null;
		this._dateToElement = null;

		this._onIsNullCheckBoxChangeHandler = null;
		this._onByValueSelectorChangedHandler = null;
		this._onLoadHandler = null;

		Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch.callBaseMethod(this, 'dispose');
	},

	_onIsNullCheckBoxChangeHandler: null,
	get_IsNull: function () {
		if (this._isNullCheckBoxElement) {
 return $(this._isNullCheckBoxElement).is(':checked');
}
 return false;

	},

	_rangeType: null,

	_isByValue: true,
	_isNullCheckBoxElement: null,
	_dateFromElement: null,
	_dateToElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch.registerClass('Quantumart.QP8.BackendArticleSearchBlock.DateOrTimeRangeFieldSearch', Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
