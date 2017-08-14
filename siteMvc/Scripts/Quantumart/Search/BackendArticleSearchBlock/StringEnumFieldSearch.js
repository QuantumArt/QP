Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID) {
    Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);
    this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
};

Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch.prototype = {
    initialize: function () {
        var queryDropDownListID = `${this._elementIdPrefix  }_queryDropDownList`;
        var isNullCheckBoxID = `${this._elementIdPrefix  }_isNullCheckBox`;
        var serverContent;

        $q.getJsonFromUrl(
            'GET',
            `${window.CONTROLLER_URL_ARTICLE_SEARCH_BLOCK  }StringEnum`,
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
            var $containerElement = $(this._containerElement);
            $containerElement.html(serverContent);
            var $isNullCheckBoxElement = $containerElement.find(`#${  isNullCheckBoxID}`);
            $isNullCheckBoxElement.bind('change', this._onIsNullCheckBoxChangeHandler);
            this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);
            this._queryDropDownListElement = $containerElement.find(`#${  queryDropDownListID}`).get(0);

            $containerElement = null;
            $isNullCheckBoxElement = null;
        }
    },

    get_searchQuery: function () {
        return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(Quantumart.QP8.Enums.ArticleFieldSearchType.StringEnum,
	        this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, this.get_IsNull(),
	        $(this._queryDropDownListElement).val());
    },

    get_blockState: function () {
        return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(Quantumart.QP8.Enums.ArticleFieldSearchType.StringEnum, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
		{
		    isNull: this.get_IsNull(),
		    text: $(this._queryDropDownListElement).val(),
		    alias: $(this._queryDropDownListElement).find('option:selected').text()
		});
    },

    get_filterDetails: function () {
        var stateData = this.get_blockState().data;
        if (stateData.isNull) {
            return $l.SearchBlock.isNullCheckBoxLabelText;
        } else if (stateData.text) {
            return `"${  $q.cutShort(stateData.alias, 8)  }"`;
        }
            return '""';

    },

    restore_blockState: function (state) {
        if (state) {
            if (this._isNullCheckBoxElement) {
                var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
                $isNullCheckBoxElement.prop('checked', state.isNull);
                $isNullCheckBoxElement.trigger('change');
                $isNullCheckBoxElement = null;
            }

            $(this._queryDropDownListElement).val(state.text);
        }
    },

    _onIsNullCheckBoxChange: function () {
        $(this._queryDropDownListElement).prop('disabled', this.get_IsNull());
    },

    dispose: function () {
        if (this._isNullCheckBoxElement) {
            var $isNullCheckBoxElement = $(this._isNullCheckBoxElement);
            $isNullCheckBoxElement.unbind('change', this._onIsNullCheckBoxChangeHandler);
            $isNullCheckBoxElement = null;
        }

        this._isNullCheckBoxElement = null;
        this._queryDropDownListElement = null;
        this._onIsNullCheckBoxChangeHandler = null;

        Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch.callBaseMethod(this, 'dispose');
    },

    _onIsNullCheckBoxChangeHandler: null,
    get_IsNull: function () {
        if (this._isNullCheckBoxElement) {
 return $(this._isNullCheckBoxElement).is(':checked');
}
 return false;

    },
    _queryDropDownListElement: null,
    _isNullCheckBoxElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch.registerClass('Quantumart.QP8.BackendArticleSearchBlock.StringEnumFieldSearch', Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
