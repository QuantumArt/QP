// eslint-disable-next-line max-params
Quantumart.QP8.BackendEntityDropDownList = function (
  listGroupCode,
  listElementId,
  entityTypeCode,
  parentEntityId,
  entityId,
  listType,
  options
) {
  Quantumart.QP8.BackendEntityDropDownList.initializeBase(
    this,
    [
      listGroupCode,
      listElementId,
      entityTypeCode,
      parentEntityId,
      entityId,
      listType,
      options
    ]
  );

  this._allowMultipleItemSelection = false;
  this._selectionMode = Quantumart.QP8.Enums.ListSelectionMode.AllItems;
};

Quantumart.QP8.BackendEntityDropDownList.prototype = {
  initialize() {
    Quantumart.QP8.BackendEntityDropDownList.callBaseMethod(this, 'initialize');
    this._addReadButtonToToolbar();
    this._addNewButtonToToolbar();
    this._attachListItemEventHandlers();
  },

  _attachListItemEventHandlers() {
    $(this._listElement).bind('change', $.proxy(this._onSelectedItemChangeHandler, this));
  },

  _detachListItemEventHandlers() {
    $(this._listElement).unbind('change');
  },

  getSelectedListItems() {
    return $(this._listElement).find("OPTION[value!='']:selected");
  },

  getSelectedEntities() {
    const entities = [];
    const $selectedListItems = this.getSelectedListItems();
    if ($selectedListItems.length > 0) {
      const $selectedListItem = $selectedListItems.eq(0);
      const entityId = $q.toString($selectedListItem.val());
      const entityName = $q.toString($selectedListItem.text(), '');
      Array.add(entities, { Id: entityId, Name: entityName });
    }

    return entities;
  },

  getSelectedEntityIDs() {
    return $.grep(this.getSelectedEntities().map(item => $q.toString(item.Id)), i => i);
  },

  selectEntities(entityID) {
    this.deselectAllListItems();
    if (!$q.isNullOrEmpty(entityID)) {
      if ($q.isArray(entityID) && entityID.length > 0) {
        this.selectEntities(entityID[0]);
      } else {
        $(this._listElement).find(`OPTION[value="${$q.toString(entityID, '')}"]`).prop('selected', true).change();
      }
    }
  },

  deselectAllListItems() {
    $(this._listElement).find('OPTION:selected').prop('selected', false).change();
  },

  _checkAllowShowingToolbar() {
    return this._addNewActionCode !== window.ACTION_CODE_NONE || this._readActionCode !== window.ACTION_CODE_NONE;
  },

  _refreshListInner(dataItems, refreshOnly) {
    const $list = $(this._listElement);
    const oldValue = $list.val();
    const markChanged = !refreshOnly;
    const selectedValue = markChanged ? oldValue : '';
    const listState = { selectedValue, isChanged: markChanged };
    $list.find("OPTION[value!='']").remove();

    const listItemHtml = new $.telerik.stringBuilder();
    for (let dataItemIndex = 0; dataItemIndex < dataItems.length; dataItemIndex++) {
      const dataItem = dataItems[dataItemIndex];
      this._getDropDownListItemHtml(listItemHtml, dataItem, markChanged, listState);
    }

    $list.append(listItemHtml.string());
    const value = $list.val();
    $q.warnIfEqDiff(oldValue, value);
    if (oldValue !== value) {
      $list.addClass(window.CHANGED_FIELD_CLASS_NAME);
      const operation = refreshOnly ? 'addClass' : 'removeClass';
      $list[operation](window.REFRESHED_FIELD_CLASS_NAME);
      $list.trigger(
        window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED,
        { fieldName: $list.data('list_item_name'), value, contentFieldName: $list.data('content_field_name') }
      );
    }
  },

  enableList() {
    $(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME).prop('disabled', false);
    this._enableAllToolbarButtons();
  },

  disableList() {
    $(this._listElement).addClass(this.LIST_DISABLED_CLASS_NAME).prop('disabled', true);
    this._disableAllToolbarButtons();
  },

  makeReadonly() {
    const $listElement = $(this._listElement);
    const selectedVal = $listElement.find('OPTION:selected').val();
    if (!$q.isNullOrEmpty(selectedVal)) {
      const $hidden = $listElement.siblings(`input[name="${$listElement.prop('name')}"]:hidden`);
      if ($hidden.length > 0) {
        $hidden.val(selectedVal);
      } else {
        $listElement.after(`<input type="hidden" name="${$listElement.prop('name')}" value="${selectedVal}" />`);
      }
    }

    this.disableList();
  },

  _getDropDownListItemHtml(html, dataItem, saveChanges, listState) {
    const itemValue = dataItem.Value;
    let itemText = dataItem.Text;
    if (this._showIds) {
      itemText = `(#${itemValue}) - ${itemText}`;
    }

    let isSelected = false;
    if (saveChanges) {
      $q.warnIfEqDiff(listState.selectedValue, itemValue);
      isSelected = listState.selectedValue === itemValue;
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

  isListChanged() {
    return $(this._listElement).hasClass(window.CHANGED_FIELD_CLASS_NAME);
  },

  _onSelectedItemChangeHandler() {
    if (!this.isListDisabled()) {
      this._refreshReadToolbarButton(true);
    }
  },

  getListItems() {
    return $(this._listElement).find('OPTION');
  },

  dispose() {
    this._stopDeferredOperations = true;
    this._detachListItemEventHandlers();
    Quantumart.QP8.BackendEntityDropDownList.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendEntityDropDownList.registerClass(
  'Quantumart.QP8.BackendEntityDropDownList', Quantumart.QP8.BackendEntityDataListBase
);
