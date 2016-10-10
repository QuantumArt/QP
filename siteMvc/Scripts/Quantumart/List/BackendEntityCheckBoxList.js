//#region class BackendEntityCheckBoxList
// === Класс "Cписок сущностей в виде чекбоксов" ===
Quantumart.QP8.BackendEntityCheckBoxList = function (listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
	Quantumart.QP8.BackendEntityCheckBoxList.initializeBase(this,
		[listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options]);

	this._allowMultipleItemSelection = true;
	this._selectionMode = Quantumart.QP8.Enums.ListSelectionMode.AllItems;
};

Quantumart.QP8.BackendEntityCheckBoxList.prototype = {

	get_maxListWidth: function () {
		return this._maxListWidth;
	},

	set_maxListWidth: function (value) {
		this._maxListWidth = value;
	},

	get_maxListHeight: function () {
		return this._maxListHeight;
	},

	set_maxListHeight: function (value) {
		this._maxListHeight = value;
	},


	initialize: function () {
		Quantumart.QP8.BackendEntityCheckBoxList.callBaseMethod(this, "initialize");

		this._fixListOverflow();

		this._addGroupCheckbox(this.getListItemCount() <= 1);
		this._syncGroupCheckbox();

		this._addNewButtonToToolbar();

		this._attachListItemEventHandlers();
	},

	_attachListItemEventHandlers: function () {
		var $list = jQuery(this._listElement);
		$list.delegate("LI INPUT:checkbox", "change", jQuery.proxy(this._onSelectedItemChangeHandler, this));
		$list.delegate("LI A", "click", jQuery.proxy(this._onItemClickHandler, this));
		$list.delegate("LI A", "mouseup", jQuery.proxy(this._onItemClickHandler, this));
		$list = null;
	},

	_detachListItemEventHandlers: function () {
		var $list = jQuery(this._listElement);
		$list.undelegate("LI INPUT:checkbox", "change");
		$list.undelegate("LI A", "click");
		$list.undelegate("LI A", "mouseup");
		$list = null;
	},

	getListItems: function () {
		var $listItems = jQuery(this._listElement).find("LI");

		return $listItems;
	},

	getSelectedListItems: function () {
		var $selectedListItems = jQuery(this._listElement).find("LI:has(INPUT:checkbox:checked)");

		return $selectedListItems;
	},

	selectEntities: function (entityIDs) {
		var isChanged = false;
		this.deselectAllListItems();
		if (!$q.isNullOrEmpty(entityIDs) && $q.isArray(entityIDs)) {
			jQuery(this._listElement).find("LI INPUT:checkbox").each(function (index, chb) {
				var $chb = jQuery(chb);
				if (entityIDs.indexOf($q.toInt($chb.val())) != -1) {
					$chb.prop("checked", true);
					isChanged = true;
				}
			});
			if (isChanged) {
				this._setAsChanged();
			}
		}
	},

	_setAsChanged: function (refreshOnly) {
		var $list = jQuery(this._listElement);
		$list.addClass(CHANGED_FIELD_CLASS_NAME);
		var operation = (refreshOnly) ? "addClass" : "removeClass";
		$list[operation](REFRESHED_FIELD_CLASS_NAME)
		var value = this.getSelectedEntityIDs();
		$list.trigger(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { "fieldName": $list.data("list_item_name"), "value": value });
	},

	getSelectedEntities: function () {
		var entities = [];
		var $selectedListItems = this.getSelectedListItems();

		$selectedListItems.each(
			function (i, listItemElem) {
				var $listItem = jQuery(listItemElem);
				var $checkbox = $listItem.find("INPUT:checkbox");
				var $label = $listItem.find("LABEL");

				var entityId = $q.toInt($checkbox.val(), 0);
				var entityName = $q.toString($label.text());

				Array.add(entities, { "Id": entityId, "Name": entityName });
			}
		);

		return entities;
	},

	_refreshListInner: function (dataItems, refreshOnly) {

		//#region Определяем, изменились ли значения
		var newSelectedIDs = jQuery.map(
			jQuery.grep(dataItems, function (di) { return di.Selected === true }),
			function (di) { return $q.toInt(di.Value) }
		);
		var currentSelectedIDs = this.getSelectedEntityIDs();
		var selectedItemsIsChanged = _.union(
			_.difference(newSelectedIDs, currentSelectedIDs),
			_.difference(currentSelectedIDs, newSelectedIDs)
		).length > 0;
		//#endregion

		var $list = jQuery(this._listElement);

		var $ul = $list.find("UL:first");

		var listItemHtml = new $.telerik.stringBuilder();

		for (var dataItemIndex = 0; dataItemIndex < dataItems.length; dataItemIndex++) {
			var dataItem = dataItems[dataItemIndex];

			this._getCheckBoxListItemHtml(listItemHtml, dataItem, dataItemIndex);
		}

		$ul.empty()
			.html(listItemHtml.string());

		this._refreshGroupCheckbox(dataItems.length);
		this._syncCountSpan(dataItems.length);

		if (selectedItemsIsChanged === true) {
		    this._setAsChanged(refreshOnly);
		}

		listItemHtml = null;

		$checkboxes = null;
		$list = null;
		$toolbar = null;
	},

	selectAllListItems: function () {
		this._changeAllListItemsSelection(true);
	},

	deselectAllListItems: function () {
		this._changeAllListItemsSelection(false);
	},

	_changeAllListItemsSelection: function (isSelect) {
		this.getListItems()
			.find("INPUT:checkbox")
			.prop("checked", isSelect);
		this._setAsChanged();
	},

	enableList: function () {
		jQuery(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME);

		this.getListItems().find("INPUT:checkbox").prop("disabled", false);
		this._getGroupCheckbox().prop("disabled", false);

		this._enableAllToolbarButtons();
	},

	disableList: function () {
		jQuery(this._listElement).addClass(this.LIST_DISABLED_CLASS_NAME);

		this.getListItems().find("INPUT:checkbox").prop("disabled", true);
		this._getGroupCheckbox().prop("disabled", true);

		this._disableAllToolbarButtons();
	},

	makeReadonly: function () {
		this.disableList();
		var $checked = this.getListItems().find("INPUT:checkbox:checked");
		$checked.each(function (i, cb) {
			var $cb = jQuery(cb);
			$cb.siblings('input[name="' + $cb.prop("name") + '"]:hidden').val($cb.val());
		});
	},

	_getCheckBoxListItemHtml: function (html, dataItem, dataItemIndex, saveChanges, listState) {
		var itemElementName = this._listItemName;
		var itemElementId = String.format("{0}_{1}", this._listElementId, dataItemIndex);
		var itemValue = dataItem.Value;
		var itemText = dataItem.Text;
		var isChecked = dataItem.Selected;
		html
			.cat('<li>')
			.cat('<input type="checkbox" ')
			.cat(' name="' + $q.htmlEncode(itemElementName) + '"')
			.cat(' id="' + $q.htmlEncode(itemElementId) + '"')
			.cat(' value="' + $q.htmlEncode(itemValue) + '"')
			.cat(' class="checkbox chb-list-item qp-notChangeTrack"')
			.catIf(' checked="checked"', isChecked)
			.catIf(' disabled ', this.isListDisabled())
			.cat('/> ')
			.cat('<input type="hidden" value="false" name="' + $q.htmlEncode(itemElementName) + '"/>')
			.cat(this._getIdLinkCode(itemValue))
			.cat('<label for="' + $q.htmlEncode(itemElementId) + '">' + itemText + '</label>')
			.cat('</li>')
			;
	},

	isListChanged: function () {
		var $list = jQuery(this._listElement);
		var result = $list.hasClass(CHANGED_FIELD_CLASS_NAME);
		$list = null;
		return result;
	},

	_checkAllowShowingToolbar: function () {
		return (this._addNewActionCode != ACTION_CODE_NONE);
	},

	_onSelectedItemChangeHandler: function () {
	    this._syncGroupCheckbox();
	    this._syncCountSpan();
		this._setAsChanged();
	},

	dispose: function () {
		this._stopDeferredOperations = true;

		this._detachListItemEventHandlers();

		Quantumart.QP8.BackendEntityCheckBoxList.callBaseMethod(this, "dispose");
	}
};

Quantumart.QP8.BackendEntityCheckBoxList.registerClass("Quantumart.QP8.BackendEntityCheckBoxList", Quantumart.QP8.BackendEntityDataListBase);
//#endregion
