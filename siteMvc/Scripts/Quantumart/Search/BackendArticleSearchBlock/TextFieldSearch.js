//#region class BackendArticleSearchBlock.TextFieldSearch
// === Класс блока текстового поиска по полю
Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
    Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);
	this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
};

Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.prototype = {
	initialize: function () {

	    // получить разметку с сервера
		var serverContent;

		$q.getJsonFromUrl(
			"GET",
			CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + "TextSearch",
			{
			    "elementIdPrefix": this._elementIdPrefix,
			    "fieldID": this._fieldID
			},
			false,
			false,
			function (data, textStatus, jqXHR) {
			    if (data.success)
			        serverContent = data.view;
			    else
			        alert(data.message);
			},
		    function(jqXHR, textStatus, errorThrown) {
		        serverContent = null;
		        $q.processGenericAjaxError(jqXHR);
		    }
		);
	    if (!$q.isNullOrWhiteSpace(serverContent)) {

	        var queryTextBoxID = this._elementIdPrefix + '_textBox';
	        var inverseCheckBoxID = this._elementIdPrefix + '_inverseCheckBox';
	        var isNullCheckBoxID = this._elementIdPrefix + '_isNullCheckBox';

	        var $containerElement = jQuery(this._containerElement);

	        // добавить разметку на страницу
	        $containerElement.append(serverContent);

	        // назначить обработчик события change чекбоксу
	        var $isNullCheckBoxElement = $containerElement.find("#" + isNullCheckBoxID);
	        $isNullCheckBoxElement.bind("change", this._onIsNullCheckBoxChangeHandler);
	        // запомнить ссылку на dom-элемент чекбокса
	        this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

	        var $inverseCheckBoxElement = $containerElement.find("#" + inverseCheckBoxID);
	        this._inverseCheckBoxElement = $inverseCheckBoxElement.get(0);

	        // запомнить ссылку на dom-элемент текстового поля
	        this._queryTextBoxElement = $containerElement.find('#' + queryTextBoxID).get(0);

	        $containerElement = null;
	        $isNullCheckBoxElement = null;
	    }
	},

	get_searchQuery: function () {
		return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(Quantumart.QP8.Enums.ArticleFieldSearchType.Text,
	        this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, this.get_IsNull(),
	        jQuery(this._queryTextBoxElement).val(), this.get_Inverse());
	},

	get_blockState: function () {
	    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(Quantumart.QP8.Enums.ArticleFieldSearchType.Text, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
		{
		    isNull: this.get_IsNull(),
		    text: jQuery(this._queryTextBoxElement).val(),
		    inverse: this.get_Inverse()
		});
	},

	get_filterDetails: function () {
	    var stateData = this.get_blockState().data;
	    var result;
		if (stateData.isNull) {
			result = $l.SearchBlock.isNullCheckBoxLabelText;
		}
		else if (stateData.text) {
		    result = '"' + $q.cutShort(stateData.text, 8) + '"';
		}
		else {
		    result = '""';
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

			jQuery(this._queryTextBoxElement).val(state.text);
		}
	},

	_onIsNullCheckBoxChange: function () {
		// дизейблим текст бокс если пользователь выбрал IS NULL
		jQuery(this._queryTextBoxElement).prop("disabled", this.get_IsNull());
	},

	dispose: function () {
		// отвязать обработчик события change чекбоксу
		if (this._isNullCheckBoxElement) {
			var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
			$isNullCheckBoxElement.unbind("change", this._onIsNullCheckBoxChangeHandler);
			$isNullCheckBoxElement = null;
		}

		this._isNullCheckBoxElement = null;
		this._inverseCheckBoxElement = null;
		this._queryTextBoxElement = null;
		this._onIsNullCheckBoxChangeHandler = null;

		Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.callBaseMethod(this, "dispose");
	},

	_onIsNullCheckBoxChangeHandler: null, // обработчик клика на чекбоксе IS NULL

	get_IsNull: function () {
		if (this._isNullCheckBoxElement)
			return jQuery(this._isNullCheckBoxElement).is(":checked");
		else
			return false;
	},

	get_Inverse: function () {
	    if (this._inverseCheckBoxElement)
	        return jQuery(this._inverseCheckBoxElement).is(":checked");
	    else
	        return false;
	},


	_queryTextBoxElement: null, // dom-элемент текстового поля
	_isNullCheckBoxElement: null, // dom-элемент чекбокса isNull
    _inverseCheckBoxElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch.registerClass("Quantumart.QP8.BackendArticleSearchBlock.TextFieldSearch", Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
//#endregion
