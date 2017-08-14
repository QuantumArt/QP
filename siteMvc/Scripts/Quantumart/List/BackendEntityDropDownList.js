Quantumart.QP8.BackendEntityDropDownList = function (listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
  Quantumart.QP8.BackendEntityDropDownList.initializeBase(this, [listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options]);
  this._allowMultipleItemSelection = false;
  this._selectionMode = Quantumart.QP8.Enums.ListSelectionMode.AllItems;
};

Quantumart.QP8.BackendEntityDropDownList.prototype = {
  initialize: function () {
    Quantumart.QP8.BackendEntityDropDownList.callBaseMethod(this, 'initialize');
    this._addReadButtonToToolbar();
    this._addNewButtonToToolbar();
    this._attachListItemEventHandlers();
  },

  _attachListItemEventHandlers: function () {
    $(this._listElement).bind('change', $.proxy(this._onSelectedItemChangeHandler, this));
  },

  _detachListItemEventHandlers: function () {
    $(this._listElement).unbind('change');
  },

  getSelectedListItems: function () {
    return $(this._listElement).find("OPTION[value!='']:selected");
  },

  getSelectedEntities: function () {
    let entities = [];
    let $selectedListItems = this.getSelectedListItems();
    if ($selectedListItems.length > 0) {
      let $selectedListItem = $selectedListItems.eq(0);
      let entityId = $q.toString($selectedListItem.val());
      let entityName = $q.toString($selectedListItem.text(), '');
      Array.add(entities, { Id: entityId, Name: entityName });
    }

    return entities;
  },

  getSelectedEntityIDs: function () {
    return $.grep(
      $.map(this.getSelectedEntities(), item => $q.toString(item.Id)), item => item
    );
  },

  selectEntities: function (entityID) {
    this.deselectAllListItems();
    if (!$q.isNullOrEmpty(entityID)) {
      if ($q.isArray(entityID) && entityID.length > 0) {
        this.selectEntities(entityID[0]);
      } else {
        $(this._listElement).find(`OPTION[value="${$q.toString(entityID, '')}"]`).prop('selected', true).change();
      }
    }
  },

  deselectAllListItems: function () {
    $(this._listElement).find('OPTION:selected').prop('selected', false).change();
  },

  _checkAllowShowingToolbar: function () {
    return this._addNewActionCode != window.ACTION_CODE_NONE || this._readActionCode != window.ACTION_CODE_NONE;
  },

  _refreshListInner: function (dataItems, refreshOnly) {
    let $list = $(this._listElement);
    let oldValue = $list.val();
    let markChanged = !refreshOnly;
    let selectedValue = markChanged ? oldValue : '';
    let listState = { selectedValue: selectedValue, isChanged: markChanged };
    $list.find("OPTION[value!='']").remove();

    let listItemHtml = new $.telerik.stringBuilder();
    for (let dataItemIndex = 0; dataItemIndex < dataItems.length; dataItemIndex++) {
      let dataItem = dataItems[dataItemIndex];
      this._getDropDownListItemHtml(listItemHtml, dataItem, markChanged, listState);
    }

    $list.append(listItemHtml.string());
    let value = $list.val();
    if (oldValue != value) {
      $list.addClass(window.CHANGED_FIELD_CLASS_NAME);
      let operation = refreshOnly ? 'addClass' : 'removeClass';
      $list[operation](window.REFRESHED_FIELD_CLASS_NAME);
      $list.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { fieldName: $list.data('list_item_name'), value: value, contentFieldName: $list.data('content_field_name') });
    }
  },

  enableList: function () {
    $(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME).prop('disabled', false);
    this._enableAllToolbarButtons();
  },

  disableList: function () {
    $(this._listElement).addClass(this.LIST_DISABLED_CLASS_NAME).prop('disabled', true);
    this._disableAllToolbarButtons();
  },

  makeReadonly: function () {
    let $listElement = $(this._listElement);
    let selectedVal = $listElement.find('OPTION:selected').val();
    if (!$q.isNullOrEmpty(selectedVal)) {
      let $hidden = $listElement.siblings(`input[name="${$listElement.prop('name')}"]:hidden`);
      if ($hidden.length > 0) {
        $hidden.val(selectedVal);
      } else {
        $listElement.after(`<input type="hidden" name="${$listElement.prop('name')}" value="${selectedVal}" />`);
      }
    }

    this.disableList();
  },

  _getDropDownListItemHtml: function (html, dataItem, saveChanges, listState) {
    let itemValue = dataItem.Value;
    let itemText = dataItem.Text;
    if (this._showIds) {
      itemText = `(#${itemValue}) - ${itemText}`;
    }

    let isSelected = false;
    if (saveChanges) {
      isSelected = listState.selectedValue == itemValue;
    } else {
      isSelected = dataItem.Selected;
    }

    html
      .cat('<option')
      .cat(` value="${$q.htmlEncode(itemValue)}"`)
      .catIf(' selected="selected"', isSelected)
      .cat('>')
      .cat(itemText)
      .cat('</option>\n');
  },

  isListChanged: function () {
    return $(this._listElement).hasClass(window.CHANGED_FIELD_CLASS_NAME);
  },

  _onSelectedItemChangeHandler: function () {
    if (!this.isListDisabled()) {
      this._refreshReadToolbarButton(true);
    }
  },

  getListItems: function () {
    return $(this._listElement).find('OPTION');
  },

  dispose: function () {
    this._stopDeferredOperations = true;
    this._detachListItemEventHandlers();
    Quantumart.QP8.BackendEntityDropDownList.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendEntityDropDownList.registerClass('Quantumart.QP8.BackendEntityDropDownList', Quantumart.QP8.BackendEntityDataListBase);
