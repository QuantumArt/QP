// #region class BackendUserSearchBlock
// === Класс "Блок поиска пользователей" ===
Quantumart.QP8.BackendUserSearchBlock = function (searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options) {
	Quantumart.QP8.BackendUserSearchBlock.initializeBase(this, [searchBlockGroupCode, searchBlockElementId, entityTypeCode, parentEntityId, options]);
};

Quantumart.QP8.BackendUserSearchBlock.prototype = {
	_minSearchBlockHeight: 145, // минимальная высота блока поиска
	_maxSearchBlockHeight: 145, // максимальная высота блока поиска

	get_searchQuery: function () {
		var $root = jQuery(this._concreteSearchBlockElement);

		var login = jQuery('.login', $root).val();
		var email = jQuery('.email', $root).val();
		var firstName = jQuery('.firstName', $root).val();
		var lastName = jQuery('.lastName', $root).val();

		var query = JSON.stringify({
			"Login": login,
			"Email": email,
			"FirstName": firstName,
			"LastName": lastName
		});

		$root = null;

		return query;
	},

	renderSearchBlock: function () {
		if (this.get_isRendered() !== true) {
			$q.getJsonFromUrl(
				"GET",
				CONTROLLER_URL_USER + "SearchBlock/" + this._parentEntityId,
				{
					"hostId": this._hostId
				},
				false,
				false
			)
			.done(jQuery.proxy(function (data) {
				if (data.success) {
					var serverContent = data.view;
					jQuery(this._concreteSearchBlockElement).html(serverContent);
					this.set_isRendered(true);
				}
				else
					{alert(data.message);}
			}, this))
			.fail(function (jqXHR, textStatus, errorThrown) {
				$q.processGenericAjaxError(jqXHR);
			});
		}
	},

	_onFindButtonClick: function () {
		var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, this.get_searchQuery());
		this.notify(EVENT_TYPE_SEARCH_BLOCK_FIND_START, eventArgs);
		eventArgs = null;
	},

	_onResetButtonClick: function () {
		// очистить блоки поиска
		jQuery(".csFilterTextbox", this._concreteSearchBlockElement).val('');

		var eventArgs = new Quantumart.QP8.BackendSearchBlockEventArgs(0, null);
		this.notify(EVENT_TYPE_SEARCH_BLOCK_RESET_START, eventArgs);
		eventArgs = null;
	},

	dispose: function () {
		Quantumart.QP8.BackendUserSearchBlock.callBaseMethod(this, "dispose");
	}
};

Quantumart.QP8.BackendUserSearchBlock.registerClass("Quantumart.QP8.BackendUserSearchBlock", Quantumart.QP8.BackendSearchBlockBase);
