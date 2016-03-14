//#region class BackendArticleSearchBlock.RelationFieldSearch
// === Класс блока поиска по числовому полю
Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch = function (containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID, searchType) {
    Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.initializeBase(this, [containerElement, parentEntityId, fieldID, contentID, fieldColumn, fieldName, fieldGroup, referenceFieldID]);

	this._searchType = searchType;

	this._onLoadHandler = jQuery.proxy(this._onLoad, this);
	this._onIsNullCheckBoxChangeHandler = jQuery.proxy(this._onIsNullCheckBoxChange, this);
	this._onSelectorChangeHandler = jQuery.proxy(this._onSelectorChange, this);

};

Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.prototype = {
	initialize: function () {

		// получить разметку с сервера
		var serverContent;

		$q.getJsonFromUrl(
			"POST",
			CONTROLLER_URL_ARTICLE_SEARCH_BLOCK + "RelationSearch",
			{
				"elementIdPrefix": this._elementIdPrefix,
				"fieldID": this._fieldID,
				"parentEntityId": this._parentEntityId,
				"IDs": this._selectedEntitiesIDs
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

			// полученную с сервера разметку добавить на страницу
			var $containerElement = jQuery(this._containerElement);
			$containerElement.html(serverContent);


			// назначить обработчик события change чекбоксу
			var $isNullCheckBoxElement = $containerElement.find("#" + isNullCheckBoxID);
			$isNullCheckBoxElement.bind("change", this._onIsNullCheckBoxChangeHandler);

			// запомнить ссылку на dom-элементы
			this._isNullCheckBoxElement = $isNullCheckBoxElement.get(0);

			this._entityContainerElement = $containerElement.find("#EntityContainer").get(0);
			this._textAreaContainerElement = $containerElement.find("#TextAreaContainer").get(0);
			this._textAreaElement = $containerElement.find("#" + this._elementIdPrefix + "_relationTextArea").get(0);

			var inverseCheckBoxID = this._elementIdPrefix + '_inverseCheckBox';
			this._inverseCheckBoxElement = $containerElement.find("#" + inverseCheckBoxID).get(0);

			jQuery(".radioButtonsList input[type='radio']", $containerElement).click(this._onSelectorChangeHandler);
			jQuery(document).ready(this._onLoadHandler);

			$isNullCheckBoxElement = null;
			$containerElement = null;
		}
	},

	get_searchQuery: function () {
	    var ids = null;
	    if (this._isEntity) {
	        ids = jQuery.map(this._getSelectedEntities(), function (item) { return item.Id; });
	    }
	    else {
	        ids = this._getIds(jQuery(this._textAreaElement).val());
	    }
		return Quantumart.QP8.BackendArticleSearchBlock.createFieldSearchQuery(this._searchType, this._fieldID, this._fieldColumn, this._contentID, this._referenceFieldID, ids, this.get_IsNull(), this.get_Inverse());
	},

	get_blockState: function () {
	    return new Quantumart.QP8.BackendArticleSearchBlock.FieldSearchState(this._searchType, this._fieldID, this._contentID, this._fieldColumn, this._fieldName, this._fieldGroup, this._referenceFieldID,
		{
			isNull: this.get_IsNull(),
			inverse: this.get_Inverse(),
			entities: this._getSelectedEntities(),
		    text: jQuery(this._textAreaElement).val(),
	        isEntity: this._isEntity
		});
	},

	_selectedEntitiesIDs: null,
	set_blockState: function (state) {
		if (state && !$q.isNullOrEmpty(state.entities)) {
			this._selectedEntitiesIDs = jQuery.map(state.entities, function (item) { return item.Id; });
		}
		else {
			this._selectedEntitiesIDs = null;
		}
	},

	get_filterDetails: function () {
	    var stateData = this.get_blockState().data;
	    var builder;
	    var result;
	    if (stateData.isNull) {
	        result = $l.SearchBlock.isNullCheckBoxLabelText;
	    }
	    else if (!stateData.isEntity) {

	        var ids = this._getIds(stateData.text);
	        result = this._getText(ids);
	    }
	    else if (!$q.isNullOrEmpty(stateData.entities)) {
	        result = this._getText(stateData.entities, function (e) { return $q.cutShort(e.Name, 10) });

	    }
	    else {
	        result = '';
	    }
	    if (stateData.inverse && result != '') {
	        result = $l.SearchBlock.notText + "(" + result + ")";
	    }
	    return result;

	},

	restore_blockState: function (state, isRestoreByClose) {
		if (state) {
			if (this._isNullCheckBoxElement) {
				var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
				$isNullCheckBoxElement.prop("checked", state.isNull);
				$isNullCheckBoxElement.trigger("change");
				$isNullCheckBoxElement = null;
			}

			if (this._inverseCheckBoxElement)
			{
				var $inverseCheckBoxElement = jQuery(this._inverseCheckBoxElement);
				$inverseCheckBoxElement.prop("checked", state.inverse);
			}

			if (isRestoreByClose)
			{
			    this._selectedEntitiesIDs = jQuery.map(state.entities, function (item) { return item.Id; });
			    if (this._entityDataListElement) {
			        jQuery(this._entityDataListElement).data("entity_data_list_component").selectEntities(this._selectedEntitiesIDs);
			    }
			}

			jQuery(this._textAreaElement).val(state.text);
			jQuery(".radioButtonsList input:radio[value=" + (state.isEntity ? 0 : 1) + "]", this._containerElement).prop("checked", true).trigger('click');
		}
	},

	_getSelectedEntities: function () {
		var edlComponent = jQuery(this._entityDataListElement).data("entity_data_list_component");
		var entities = edlComponent.getSelectedEntities();
		edlComponent = null;
		return entities;
	},

	_onIsNullCheckBoxChange: function () {
		var edlComponent = jQuery(this._entityDataListElement).data("entity_data_list_component");
		if (this.get_IsNull()) {
		    edlComponent.disableList();
		    jQuery(this._textAreaElement).prop("disabled", true);
		}
		else {
		    edlComponent.enableList();
		    jQuery(this._textAreaElement).prop("disabled", false);
		}

		edlComponent = null;
	},

	_onLoad: function () {
		$c.initAllEntityDataLists(this._containerElement)		
		this._entityDataListElement = $c.getAllEntityDataLists(this._containerElement).get(0);
	},

	_onSelectorChange: function (e) {
	    this._isEntity = jQuery(e.currentTarget).val() == 0;

	    if (this._isEntity) {
	        jQuery(this._entityContainerElement).show();
	        jQuery(this._textAreaContainerElement).hide();
	        jQuery(this._entityDataListElement).data("entity_data_list_component").refreshList();
	    }
	    else {
	        jQuery(this._entityContainerElement).hide();
	        jQuery(this._textAreaContainerElement).show();
	    }
	},

	onOpen: function () {
		$c.fixAllEntityDataListsOverflow(this._containerElement);
	},

	dispose: function () {
		$c.destroyAllEntityDataLists(this._containerElement)

		// отвязать обработчик события change чекбоксу
		if (this._isNullCheckBoxElement) {
			var $isNullCheckBoxElement = jQuery(this._isNullCheckBoxElement);
			$isNullCheckBoxElement.unbind("change", this._onIsNullCheckBoxChangeHandler);
			$isNullCheckBoxElement = null;
		}

		this._isNullCheckBoxElement = null;
		this._inverseCheckBoxElement = null;
		this._entityDataListElement = null;
		this._entityContainerElement = null;
		this._textAreaContainerElement = null;
		this._textAreaElement = null;
		this._onIsNullCheckBoxChangeHandler = null;
		this._onLoadHandler = null;

		Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.callBaseMethod(this, "dispose");
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


	_searchType: 0, //тип поиска (Many2Nany или One2Many)
	_isEntity: true,

	_isNullCheckBoxElement: null, // dom-элемент чекбокса isNull 
	_inverseCheckBoxElement: null,
	_entityDataListElement: null, //

    _entityContainerElement: null,
    _textAreaContainerElement: null,
    _textAreaElement: null
};

Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch.registerClass("Quantumart.QP8.BackendArticleSearchBlock.RelationFieldSearch", Quantumart.QP8.BackendArticleSearchBlock.FieldSearchBase);
//#endregion