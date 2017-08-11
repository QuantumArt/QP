// #region class BackendContentSearchBlock
// === Класс "Блок поиска статей" ===
Quantumart.QP8.BackendContentSearchBlock = function (searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options) {
	Quantumart.QP8.BackendContentSearchBlock.initializeBase(this, [searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options]);
	this._onChangeComboHandler = jQuery.proxy(this._onChangeCombo, this);
};

Quantumart.QP8.BackendContentSearchBlock.prototype
= {
	_minSearchBlockHeight: 80, // минимальная высота блока поиска
	_maxSearchBlockHeight: 80, // максимальная высота блока поиска
	_contentGroupListElement: null, // dom-элемент списка групп
	_siteListElement: null,
	_contentNameElement: null,

	get_searchQuery: function () {
		var groupId = null;
		var siteId = null;
		var contentName = null;

		if (this._contentGroupListElement) {
 groupId = jQuery(this._contentGroupListElement).find("option:selected").val();
}
		if (this._siteListElement) {
 siteId = jQuery(this._siteListElement).find("option:selected").val();
}
		if (this._contentNameElement) {
 contentName = jQuery(this._contentNameElement).val();
}

		return JSON.stringify({
			GroupId: groupId,
			SiteId: siteId,
			ContentName: contentName
		});
	},

	// возвращает параметры поиска

	renderSearchBlock: function () {
		if (this.get_isRendered() !== true) {
			// получить разметку с сервера
			var serverContent;
			$q.getJsonFromUrl(
			"GET",
			CONTROLLER_URL_CONTENT_SEARCH_BLOCK + "SearchBlock/" + this._parentEntityId,
			{
				actionCode: this._actionCode,
				hostId: this._hostId
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
			});
			if (!$q.isNullOrWhiteSpace(serverContent)) {
			    jQuery(this._concreteSearchBlockElement).html(serverContent);

			    // получить список групп
				this._contentGroupListElement = jQuery(".contentGroupList", this._searchBlockElement).get(0);
				this._siteListElement = jQuery(".siteList", this._searchBlockElement).get(0);
				this._contentNameElement = jQuery(".contentNameText", this._searchBlockElement).get(0);
				jQuery(".csFilterCombo", this._searchBlockElement).bind("change", this._onChangeComboHandler);
			}

			this.set_isRendered(true);
		}
	},


	_onChangeCombo: function () {
		jQuery(this._findButtonElement).trigger("click");
	},

	_onFindButtonClick: function () {
		var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, this.get_searchQuery());
		this.notify(EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);
		eventArgs = null;
	},

	_onResetButtonClick: function () {
		// очистить блоки поиска
		jQuery(".csFilterCombo", this._searchBlockElement).find("option[value='']").prop("selected", true);
		jQuery(".csFilterTextbox", this._searchBlockElement).val('');

		var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, null);
		this.notify(EVENT_TYPE_SEARCH_BLOCK_RESET_START, eventArgs);
		eventArgs = null;
	},

	dispose: function () {
	    jQuery(".csFilterCombo", this._searchBlockElement).unbind();

		this._contentGroupListElement = null;
		this._siteListElement = null;
		this._contentNameElement = null;
		this._onChangeComboHandler = null;

		Quantumart.QP8.BackendContentSearchBlock.callBaseMethod(this, "dispose");
	}
};

Quantumart.QP8.BackendContentSearchBlock.registerClass("Quantumart.QP8.BackendContentSearchBlock", Quantumart.QP8.BackendSearchBlockBase);

// #endregion
