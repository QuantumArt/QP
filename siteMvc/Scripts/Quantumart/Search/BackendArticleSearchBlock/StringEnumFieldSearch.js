// #region class BackendArticleSearchBlock.StringEnumFieldSearch
// === Класс блока текстового поиска по полю
Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
    Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);
    this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
};

Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch.prototype = {
    initialize: function () {

        var queryDropDownListID = this._elementIdPrefix + '_queryDropDownList';
        var isNullCheckBoxID = this._elementIdPrefix + '_isNullCheckBox';
        var serverContent;

        // получить разметку с сервера
        $q.getJsonFromUrl(
            "GET",
            CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + "StringEnum",
            {
                "elementIdPrefix": this._elementIdPrefix,
                "fieldID": this._fieldID
            },
            false,
            false,
            function (data, textStatus, jqXHR) {
                if (data.success)
                    {serverContent = data.view;}
                else
                    {alert(data.message);}
            },
            function (jqXHR, textStatus, errorThrown) {
                serverContent = null;
                $q.processGenericAjaxError(jqXHR);
            }
        );

        if (!$q.isNullOrWhiteSpace(serverContent)) {
            var $containerElement = jQuery(this._containerElement);

            // добавить разметку на страницу
            // $containerElement.append(html.string());
            $containerElement.html(serverContent);

            // назначить обработчик события change чекбоксу
            var $isNullCheckBoxElement = $containerElement.find("#" + isNullCheckBoxID);
            $isNullCheckBoxElement.bind("change", this._onIsNullCheckBoxChangeHandler);
            // запомнить ссылку на dom-элемент чекбокса
            this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

            // запомнить ссылку на dom-элемент текстового поля
            this._queryDropDownListElement = $containerElement.find('#' + queryDropDownListID).get(0);

            $containerElement = null;
            $isNullCheckBoxElement = null;
        }
    },

    get_searchQuery: function () {
        return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(Quantumart.QP8.Enums.ArticleFieldSearchType.StringEnum,
	        this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, this.get_IsNull(),
	        jQuery(this._queryDropDownListElement).val());
    },

    get_blockState: function () {
        return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(Quantumart.QP8.Enums.ArticleFieldSearchType.StringEnum, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
		{
		    isNull: this.get_IsNull(),
		    text: jQuery(this._queryDropDownListElement).val(),
		    alias: jQuery(this._queryDropDownListElement).find("option:selected").text()
		});
    },

    get_filterDetails: function () {
        var stateData = this.get_blockState().data;
        if (stateData.isNull) {
            return $l.SearchBlock.isNullCheckBoxLabelText;
        }
        else if (stateData.text) {
            return '"' + $q.cutShort(stateData.alias, 8) + '"';
        }
        else {
            return '""';
        }
    },

    restore_blockState: function (state) {
        if (state) {
            if (this._isNullCheckBoxElement) {
                var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
                $isNullCheckBoxElement.prop("checked", state.isNull);
                $isNullCheckBoxElement.trigger("change");
                $isNullCheckBoxElement = null;
            }

            jQuery(this._queryDropDownListElement).val(state.text);
        }
    },

    _onIsNullCheckBoxChange: function () {
        // дизейблим текст бокс если пользователь выбрал IS NULL
        jQuery(this._queryDropDownListElement).prop("disabled", this.get_IsNull());
    },

    dispose: function () {
        // отвязать обработчик события change чекбоксу
        if (this._isNullCheckBoxElement) {
            var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
            $isNullCheckBoxElement.unbind("change", this._onIsNullCheckBoxChangeHandler);
            $isNullCheckBoxElement = null;
        }

        this._isNullCheckBoxElement = null;
        this._queryDropDownListElement = null;
        this._onIsNullCheckBoxChangeHandler = null;

        Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch.callBaseMethod(this, "dispose");
    },

    _onIsNullCheckBoxChangeHandler: null, // обработчик клика на чекбоксе IS NULL
    get_IsNull: function () {
        if (this._isNullCheckBoxElement)
            {return jQuery(this._isNullCheckBoxElement).is(":checked");}
        else
            {return false;}
    },


    _queryDropDownListElement: null, // dom-элемент текстового поля
    _isNullCheckBoxElement: null // dom-элемент чекбокса isNull
};

Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch.registerClass("Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch", Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
// #endregion
