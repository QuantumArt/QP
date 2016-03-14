//#region class BackendArticleSearchBlock.ClassifierFieldSearch
// === Класс блока поиска по числовому полю
Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID, searchType) {
    Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID, searchType]);

    this._searchType = searchType;
    this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);

};
Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch.prototype = {
    _contentElement: null,

    initialize: function () {

        // получить разметку с сервера
        var serverContent;

        $q.getJsonFromUrl(
			"GET",
			CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + "ContentsListForClassifier",
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
			function (jqXHR, textStatus, errorThrown) {
			    serverContent = null;
			    $q.processGenericAjaxError(jqXHR);
			}
		);
        if (!$q.isNullOrWhiteSpace(serverContent)) {
            var isNullCheckBoxID = this._elementIdPrefix + '_isNullCheckBox';
            var contentID = this._elementIdPrefix + '_contentID';

            // полученную с сервера разметку добавить на страницу
            var $containerElement = jQuery(this._containerElement);
            $containerElement.html(serverContent);

            var $content = $containerElement.find("#" + contentID);

            // назначить обработчик события change чекбоксу
            var $isNullCheckBoxElement = $containerElement.find("#" + isNullCheckBoxID);
            $isNullCheckBoxElement.bind("change", this._onIsNullCheckBoxChangeHandler);

            // запомнить ссылку на dom-элементы
            this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);
            this._contentElement = $content.get(0);

            jQuery(document).ready(this._onLoadHandler);

            $isNullCheckBoxElement = null;
            $containerElement = null;
        }
    },
    //формирование запроса
    get_searchQuery: function () {
        var contentObj = new Array(jQuery(this._contentElement).val());
        return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(this._searchType, this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, contentObj, this.get_IsNull(), false);
    },

    //сохраняем состояние
    get_blockState: function () {
        return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(this._searchType, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
		{
		    isNull: this.get_IsNull(),
		    contentID: jQuery(this._contentElement).val()
		});
    },


    get_filterDetails: function () {
    	var stateData = this.get_blockState().data;
    	if (stateData.isNull) {
    		return $l.SearchBlock.isNullCheckBoxLabelText;
    	}
    	else if (stateData.contentID) {
    	    return $q.cutShort(jQuery(this._contentElement).find("[value=" + stateData.contentID + "]").text(), 12);
    	}
    	else {
    		return '';
    	}
    },

    //восстаналиваем состояние
    restore_blockState: function (state) {
        if (state) {
            if (this._isNullCheckBoxElement) {
                var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
                $isNullCheckBoxElement.prop("checked", state.isNull);
                $isNullCheckBoxElement.trigger("change");
                $isNullCheckBoxElement = null;
            }

            Sys.Debug.trace(state.contentID);
            if (!$q.isNull(state.contentID)) {
                jQuery(this._contentElement).val(state.contentID);
            }
        }
    },

    _onIsNullCheckBoxChangeHandler: null, // обработчик клика на чекбоксе IS NULL
    get_IsNull: function () {
        if (this._isNullCheckBoxElement)
            return jQuery(this._isNullCheckBoxElement).is(":checked");
        else
            return false;
    },

    _onIsNullCheckBoxChange: function () {
        if (this.get_IsNull()) {
            $(this._contentElement).prop("disabled", true);
        }
        else
        {
            $(this._contentElement).prop("disabled", false);
        }
    },
    dispose: function () {
        // отвязать обработчик события change чекбоксу
        if (this._isNullCheckBoxElement) {
            var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
            $isNullCheckBoxElement.unbind("change", this._onIsNullCheckBoxChangeHandler);
            $isNullCheckBoxElement = null;
        }

        //уничтожаем выпадающий список
        $c.destroyAllNumericTextBoxes(this._contentElement);

        $containerElement = null;

        this._isNullCheckBoxElement = null;
        this._onIsNullCheckBoxChangeHandler = null;
        this._contentElement = null;

        Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch.callBaseMethod(this, "dispose");
    },

    _searchType: 0, //тип поиска Classifier

    _isNullCheckBoxElement: null, // dom-элемент чекбокса isNull        
    _contentID: null // 
};

Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch.registerClass("Quantumart.QP8.BackendArticleSearchBlock.ClassifierFieldSearch", Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
//#endregion