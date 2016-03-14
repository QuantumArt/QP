//#region class BackendEntityMultipleItemPicker
// === Класс "Cписок сущностей в виде элемента управления множественного выбора" ===
Quantumart.QP8.BackendEntityMultipleItemPicker = function (listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
	Quantumart.QP8.BackendEntityMultipleItemPicker.initializeBase(this,
		[listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options]);

	this._allowMultipleItemSelection = true;
	this._selectionMode = Quantumart.QP8.Enums.ListSelectionMode.OnlySelectedItems;
};

Quantumart.QP8.BackendEntityMultipleItemPicker.prototype = {
	_pickButtonElement: null, // DOM-элемент, образующий кнопку выбора элементов
	_clearButtonElement: null,
	_copyButtonElement: null,
	_pasteButtonElement: null,
	_countOverflowElement: null,

	OVERFLOW_HIDDEN_CLASS: "overflowHiddenValue",

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
		Quantumart.QP8.BackendEntityMultipleItemPicker.callBaseMethod(this, "initialize");

		var $list = jQuery(this._listElement);
		var countOverflowElement = $list.find("." + this.OVERFLOW_HIDDEN_CLASS);
		if (countOverflowElement.length > 0)
		{
			this._countOverflowElement = countOverflowElement.get(0);
		}
	
		this._fixListOverflow();

		var count = this.getSelectedEntities().length;
		var hidden = count <= 1 || this._isCountOverflow();
		this._addCountDiv(count, false);
		this._addGroupCheckbox(hidden);
		this._syncGroupCheckbox();
	
		var $pickButton = this._createToolbarButton(this._listElementId + "_PickButton", $l.EntityDataList.pickLinkButtonText, "pick");
		this._addButtonToToolbar($pickButton);
	
		var $clearButton = this._createToolbarButton(this._listElementId + "_ClearButton", $l.EntityDataList.clearUnmarkedLinkButtonText, "deselectAll");
		this._addButtonToToolbar($clearButton);


		if (this._enableCopy)
		{
		    var $copyButton = this._createToolbarButton(this._listElementId + "_CopyButton", $l.EntityDataList.copyLinkButtonText, "copy");
		    this._addButtonToToolbar($copyButton);

		    var $pasteButton = this._createToolbarButton(this._listElementId + "_PasteButton", $l.EntityDataList.pasteLinkButtonText, "paste");
		    this._addButtonToToolbar($pasteButton);
		}
	
		this._addNewButtonToToolbar();
	
		this._pickButtonElement = $pickButton.get(0);
		$pickButton.bind("click", jQuery.proxy(this._onPickButtonClickHandler, this));

		this._clearButtonElement = $clearButton.get(0);
		$clearButton.bind("click", jQuery.proxy(this._onClearButtonClickHandler, this));
		this._refreshClearButton();

		if (this._enableCopy) {

		    this._copyButtonElement = $copyButton.get(0);
		    $copyButton.bind("click", jQuery.proxy(this._onCopyButtonClickHandler, this));

		    this._pasteButtonElement = $pasteButton.get(0);
		    $pasteButton.bind("click", jQuery.proxy(this._onPasteButtonClickHandler, this));
		}
	

		$list.delegate("LI INPUT:checkbox", "change", jQuery.proxy(this._onSelectedItemChangeHandler, this));
		$list.delegate("LI A", "click", jQuery.proxy(this._onItemClickHandler, this));
		$list.delegate("LI A", "mouseup", jQuery.proxy(this._onItemClickHandler, this));

		$pickButton = null;
		$clearButton = null;
		$list = null;
	
	},

	getListItems: function () {
		var $listItems = jQuery(this._listElement).find("UL LI");
	
		return $listItems;
	},

	getSelectedListItems: function () {
		var $selectedListItems = jQuery(this._listElement).find("UL LI:has(INPUT:checkbox:checked)");
	
		return $selectedListItems;
	},

	getSelectedEntities: function () {
		var entities;
		if (!this._isCountOverflow())
		{
			var $selectedListItems = this.getSelectedListItems();
			entities = $selectedListItems.map(function (i, item) {
				var $item = jQuery(item);
				var id = $q.toInt($item.find("INPUT:checkbox").val(), 0);
				var name = $q.toString($item.find("LABEL").text());
				return { Id : id, Name : name };
			}).get();
		}
		else
		{
			var ids = jQuery(this._countOverflowElement).val().split(",");
			entities = jQuery.map(ids, function (id) {
				return { Id: id, Name: "" };
			});
		}

		return (entities || []);
	},

	_isCountOverflow : function()
	{
		return this._countOverflowElement != null;
	},

	_refreshListInner: function (dataItems, refreshOnly) {
		//#region Определяем, изменились ли значения
		var newSelectedIDs = jQuery.map(
			jQuery.grep(dataItems, function (di) { return di.Value; }),
			function (di) { return $q.toInt(di.Value) }
		);
		var currentSelectedIDs = this.getSelectedEntityIDs();

		var selectedItemsIsChanged = (newSelectedIDs.length != currentSelectedIDs.length
			|| newSelectedIDs.length != _.union(newSelectedIDs, currentSelectedIDs).length
		);

		if (selectedItemsIsChanged) {
			var oldCount = this.getSelectedEntities().length;
			var $list = jQuery(this._listElement)
			var $ul = $list.find("UL");

			if (newSelectedIDs.length < this._countLimit)
			{
				$ul.empty().html(this._getCheckBoxListHtml(dataItems));
				jQuery(this._countOverflowElement).remove();
				this._countOverflowElement = null;
			}
			else
			{
				$ul.empty();
				var value = newSelectedIDs.join();
				if (this._countOverflowElement) {
					jQuery(this._countOverflowElement).val(value);
				}
				else {
					var name = $list.data("list_item_name");
					var html = '<input type="hidden" class="' + this.OVERFLOW_HIDDEN_CLASS + '" name="' + name + '" value = "' + value + '" />';
					$ul.before(html);
					this._countOverflowElement = $list.find("." + this.OVERFLOW_HIDDEN_CLASS).get(0);
				}
			}
			this._setAsChanged(refreshOnly);
			this._refreshGroupCheckbox(dataItems.length);
			this._syncCountSpan(dataItems.length);
			this._refreshClearButton();
		}
	},

	selectAllListItems: function () {
	    this._changeAllListItemsSelection(true);
	    this._refreshClearButton();
	},

	selectEntities: function (entityIDs) {
		this.deselectAllListItems();
		if (!$q.isNullOrEmpty(entityIDs) && $q.isArray(entityIDs)) {
			var selectedEntityIDs = jQuery.map(entityIDs, function (id) {
				return { Id: id };
			});
			this._loadSelectedItems(selectedEntityIDs);
			selectedEntityIDs = null;
		}
	},

	deselectAllListItems: function () {
	    this._changeAllListItemsSelection(false);
	    this._refreshClearButton();
	},

	removeAllListItems: function () {
		this.deselectAllListItems();
		this._onClearButtonClickHandler();
	},

	_changeAllListItemsSelection: function (isSelect) {
		this.getListItems()
			.find("INPUT:checkbox")
			.prop("checked", isSelect)								
		this._setAsChanged();
	},

	enableList: function () {
		jQuery(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME);
			
		this.getListItems().find("INPUT:checkbox").prop("disabled", false);
		this._getGroupCheckbox().prop("disabled", false);
	
		this._enableAllToolbarButtons();
		this._refreshClearButton();
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

	isListChanged: function () {
		var $list = jQuery(this._listElement);
		var result = $list.hasClass(CHANGED_FIELD_CLASS_NAME);
		$list = null;
		return result;
	},

	_setAsChanged: function (refreshOnly) {
		var $list = jQuery(this._listElement);
		$list.addClass(CHANGED_FIELD_CLASS_NAME);
		var operation = (refreshOnly) ? "addClass" : "removeClass";
		$list[operation](REFRESHED_FIELD_CLASS_NAME)
		var value = this.getSelectedEntityIDs();
		$list.trigger(JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { "fieldName": $list.data("list_item_name"), "value": value });
	},

	_getCheckBoxListHtml: function (dataItems) {
		var html = new $.telerik.stringBuilder();
		for (var dataItemIndex = 0; dataItemIndex < dataItems.length; dataItemIndex++) {
			var dataItem = dataItems[dataItemIndex];
			this._getCheckBoxListItemHtml(html, dataItem, dataItemIndex);
		}
		return html.string();
	},

	_getCheckBoxListItemHtml: function (html, dataItem, dataItemIndex) {
		var itemElementName = this._listItemName;
		var itemElementId = String.format("{0}_{1}", this._listElementId, dataItemIndex);
		var itemValue = dataItem.Value;
		var itemText = dataItem.Text;
	
		html
			.cat('<li>')
			.cat('<input type="checkbox"')
			.cat(' name="' + $q.htmlEncode(itemElementName) + '"')
			.cat(' id="' + $q.htmlEncode(itemElementId) + '"')
			.cat(' value="' + $q.htmlEncode(itemValue) + '"')
			.cat(' class="checkbox multi-picker-item qp-notChangeTrack"')
			.cat(' checked="checked"')
			.catIf(' disabled ', this.isListDisabled())
			.cat('/> ')
			.cat('<input type="hidden" value="false" name="' + $q.htmlEncode(itemElementName) + '"/>')
			.cat(this._getIdLinkCode(itemValue))
			.cat('<label for="' + $q.htmlEncode(itemElementId) + '">' + itemText + '</label>')
			.cat('</li>')
			;
	},

	_onPickButtonClickHandler: function () {
		if (!this.isListDisabled()) {
			this._openPopupWindow();
		}
	},

	_onClearButtonClickHandler: function () {
		jQuery(this._listElement).find("LI:has(INPUT:checkbox:not(:checked))").remove();
		var newCount = this.getListItemCount();
		this._refreshGroupCheckbox(newCount);
		this._syncCountSpan(newCount);
		this._refreshClearButton();
		this._fixListOverflow();
	},

	_checkAllowShowingToolbar: function () {
		return (this._selectActionCode != ACTION_CODE_NONE);
	},

	_onSelectedItemChangeHandler: function () {		
	    this._syncGroupCheckbox();
	    this._syncCountSpan();
		this._refreshClearButton();
		this._setAsChanged();
		var eventArgs = new Quantumart.QP8.BackendEventArgs();
		this.notify(EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, eventArgs);
	},

	_refreshClearButton: function () {
		if (this._clearButtonElement) {
			var $button = jQuery(this._clearButtonElement);
			this._changeToolbarButtonState($button, !this._isCountOverflow() && this.getSelectedListItemCount() != this.getListItemCount());
			$button = null;
		}
	},

	dispose: function () {
		this._stopDeferredOperations = true;
	
		jQuery(this._pickButtonElement).unbind("click");
		jQuery(this._clearButtonElement).unbind("click");
		if (this._enableCopy) {
		    jQuery(this._copyButtonElement).unbind("click");
		    jQuery(this._pasteButtonElement).unbind("click");
		}
		jQuery(this._listElement).undelegate("LI INPUT:checkbox", "change");
		jQuery(this._listElement).undelegate("LI A", "click");
		jQuery(this._listElement).undelegate("LI A", "mouseup");
	
	
		this._pickButtonElement = null;
		this._clearButtonElement = null;
		this._copyButtonElement = null;
		this._pasteButtonElement = null;

		this._listElement = null;
	
		Quantumart.QP8.BackendEntityMultipleItemPicker.callBaseMethod(this, "dispose");
	}
};

Quantumart.QP8.BackendEntityMultipleItemPicker.registerClass("Quantumart.QP8.BackendEntityMultipleItemPicker", Quantumart.QP8.BackendEntityDataListBase);
//#endregion