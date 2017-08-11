Quantumart.QP8.BackendEntityMultipleItemPicker = function (listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
  Quantumart.QP8.BackendEntityMultipleItemPicker.initializeBase(this,
    [listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options]);

  this._allowMultipleItemSelection = true;
  this._selectionMode = Quantumart.QP8.Enums.ListSelectionMode.OnlySelectedItems;
};

Quantumart.QP8.BackendEntityMultipleItemPicker.prototype = {
  _pickButtonElement: null,
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
    if (countOverflowElement.length > 0) {
      this._countOverflowElement = countOverflowElement.get(0);
    }

    this._fixListOverflow();

    var count = this.getSelectedEntities().length;
    var hidden = count <= 1 || this._isCountOverflow();
    this._addCountDiv(count, false);
    this._addGroupCheckbox(hidden);
    this._syncGroupCheckbox();

    var $pickButton = this._createToolbarButton(this._listElementId + '_PickButton', $l.EntityDataList.pickLinkButtonText, 'pick');
    this._addButtonToToolbar($pickButton);

    var $clearButton = this._createToolbarButton(this._listElementId + '_ClearButton', $l.EntityDataList.clearUnmarkedLinkButtonText, 'deselectAll');
    this._addButtonToToolbar($clearButton);

    if (this._enableCopy) {
      var $copyButton = this._createToolbarButton(this._listElementId + '_CopyButton', $l.EntityDataList.copyLinkButtonText, 'copy');
      this._addButtonToToolbar($copyButton);

      var $pasteButton = this._createToolbarButton(this._listElementId + '_PasteButton', $l.EntityDataList.pasteLinkButtonText, 'paste');
      this._addButtonToToolbar($pasteButton);
    }

    this._addNewButtonToToolbar();

    this._pickButtonElement = $pickButton.get(0);
    $pickButton.bind('click', jQuery.proxy(this._onPickButtonClickHandler, this));

    this._clearButtonElement = $clearButton.get(0);
    $clearButton.bind('click', jQuery.proxy(this._onClearButtonClickHandler, this));
    this._refreshClearButton();

    if (this._enableCopy) {
        this._copyButtonElement = $copyButton.get(0);
        $copyButton.bind('click', jQuery.proxy(this._onCopyButtonClickHandler, this));

        this._pasteButtonElement = $pasteButton.get(0);
        $pasteButton.bind('click', jQuery.proxy(this._onPasteButtonClickHandler, this));
    }

    $list.delegate("LI INPUT:checkbox", "change", jQuery.proxy(this._onSelectedItemChangeHandler, this));
    $list.delegate("LI A", "click", jQuery.proxy(this._onItemClickHandler, this));
    $list.delegate("LI A", "mouseup", jQuery.proxy(this._onItemClickHandler, this));

    $pickButton = null;
    $clearButton = null;
    $list = null;
  },

  getListItems: function () {
    return $(this._listElement).find("UL LI");
  },

  getSelectedListItems: function () {
    return $(this._listElement).find("UL LI:has(INPUT:checkbox:checked)");
  },

  getSelectedEntities: function () {
    var result;
    if (!this._isCountOverflow()) {
      var $selectedListItems = this.getSelectedListItems();
      result = $selectedListItems.map(function (i, item) {
        var $item = $(item);
        return {
          Id: +$item.find("INPUT:checkbox").val(),
          Name: $item.find("LABEL").text()
        };
      }).get() || [];
    } else {
      var ids = $(this._countOverflowElement).val().split(",");
      result = $.map(ids, function (id) {
        return { Id: id, Name: '' };
      }) || [];
    }

    return result;
  },

  _isCountOverflow: function () {
    return this._countOverflowElement != null;
  },

  _refreshListInner: function (dataItems, refreshOnly) {
    var newSelectedIDs = $.map($.grep(dataItems, function (di) {
      return di.Value;
    }), function (di) {
      return $q.toInt(di.Value);
    });

    var currentSelectedIDs = this.getSelectedEntityIDs();
    var selectedItemsIsChanged = (newSelectedIDs.length != currentSelectedIDs.length || newSelectedIDs.length != _.union(newSelectedIDs, currentSelectedIDs).length);

    if (selectedItemsIsChanged) {
      var oldCount = this.getSelectedEntities().length;
      var $list = $(this._listElement);
      var $ul = $list.find("UL");

      if (newSelectedIDs.length < this._countLimit) {
        $ul.empty().html(this._getCheckBoxListHtml(dataItems));
        $(this._countOverflowElement).remove();
        this._countOverflowElement = null;
      } else {
        $ul.empty();
        var value = newSelectedIDs.join();
        if (this._countOverflowElement) {
          $(this._countOverflowElement).val(value);
        } else {
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

  appendEntities: function (entityIds) {
    if(entityIds && entityIds.length) {
      var selectedEntities = entityIds.map(function (i) {
        return { Id: i };
      });

      this._loadSelectedItems(selectedEntities);
    }
  },

  selectEntities: function (entityIds) {
    this.deselectAllListItems();
    if(entityIds && entityIds.length) {
      var selectedEntities = entityIds.map(function (i) {
        return { Id: i };
      });

      this._loadSelectedItems(selectedEntities);
    }
  },

  selectAllListItems: function () {
    this._changeAllListItemsSelection(true);
    this._refreshClearButton();
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
    this.getListItems().find("INPUT:checkbox").prop("checked", isSelect);
    this._setAsChanged();
  },

  enableList: function () {
    $(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME);

    this.getListItems().find("INPUT:checkbox").prop("disabled", false);
    this._getGroupCheckbox().prop("disabled", false);
    this._enableAllToolbarButtons();
    this._refreshClearButton();
  },

  disableList: function () {
    $(this._listElement).addClass(this.LIST_DISABLED_CLASS_NAME);

    this.getListItems().find("INPUT:checkbox").prop("disabled", true);
    this._getGroupCheckbox().prop("disabled", true);
    this._disableAllToolbarButtons();
  },

  makeReadonly: function () {
    this.disableList();
    var $checked = this.getListItems().find("INPUT:checkbox:checked");
    $checked.each(function (i, cb) {
      var $cb = $(cb);
      $cb.siblings('input[name="' + $cb.prop("name") + '"]:hidden').val($cb.val());
    });
  },

  isListChanged: function () {
    return $(this._listElement).hasClass(window.CHANGED_FIELD_CLASS_NAME);
  },

  _setAsChanged: function (refreshOnly) {
    var $list = $(this._listElement);
    $list.addClass(window.CHANGED_FIELD_CLASS_NAME);

    var operation = refreshOnly ? "addClass" : "removeClass";
    $list[operation](window.REFRESHED_FIELD_CLASS_NAME);

    var value = this.getSelectedEntityIDs();
    $list.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { fieldName: $list.data("list_item_name"), value: value, contentFieldName: $list.closest("dl").data("field_name")});
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
      .cat('</li>');
  },

  _onPickButtonClickHandler: function () {
    if (!this.isListDisabled()) {
      this._openPopupWindow();
    }
  },

  _onClearButtonClickHandler: function () {
    $(this._listElement).find("LI:has(INPUT:checkbox:not(:checked))").remove();

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
    this.notify(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, new Quantumart.QP8.BackendEventArgs());
  },

  _refreshClearButton: function () {
    if (this._clearButtonElement) {
      this._changeToolbarButtonState($(this._clearButtonElement), !this._isCountOverflow() && this.getSelectedListItemCount() != this.getListItemCount());
    }
  },

  dispose: function () {
    this._stopDeferredOperations = true;

    $(this._pickButtonElement).unbind("click");
    $(this._clearButtonElement).unbind("click");
    if (this._enableCopy) {
      $(this._copyButtonElement).unbind("click");
      $(this._pasteButtonElement).unbind("click");
    }

    $(this._listElement).undelegate("LI INPUT:checkbox", "change");
    $(this._listElement).undelegate("LI A", "click");
    $(this._listElement).undelegate("LI A", "mouseup");

    this._pickButtonElement = null;
    this._clearButtonElement = null;
    this._copyButtonElement = null;
    this._pasteButtonElement = null;
    this._listElement = null;

    Quantumart.QP8.BackendEntityMultipleItemPicker.callBaseMethod(this, "dispose");
  }
};

Quantumart.QP8.BackendEntityMultipleItemPicker.registerClass("Quantumart.QP8.BackendEntityMultipleItemPicker", Quantumart.QP8.BackendEntityDataListBase);
