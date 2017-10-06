Quantumart.QP8.BackendEntityCheckBoxList = function (
  listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options
) {
  Quantumart.QP8.BackendEntityCheckBoxList.initializeBase(this,
    [listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options]);

  this._allowMultipleItemSelection = true;
  this._selectionMode = Quantumart.QP8.Enums.ListSelectionMode.AllItems;
};

Quantumart.QP8.BackendEntityCheckBoxList.prototype = {

  getMaxListWidth() {
    return this._maxListWidth;
  },

  setMaxListWidth(value) {
    this._maxListWidth = value;
  },

  getMaxListHeight() {
    return this._maxListHeight;
  },

  setMaxListHeight(value) {
    this._maxListHeight = value;
  },

  initialize() {
    Quantumart.QP8.BackendEntityCheckBoxList.callBaseMethod(this, 'initialize');

    this._fixListOverflow();
    this._addGroupCheckbox(this.getListItemCount() <= 1);
    this._syncGroupCheckbox();
    this._addNewButtonToToolbar();
    this._attachListItemEventHandlers();
  },

  _attachListItemEventHandlers() {
    const $list = $(this._listElement);
    $list.delegate('LI INPUT:checkbox', 'change', $.proxy(this._onSelectedItemChangeHandler, this));
    $list.delegate('LI A', 'click', $.proxy(this._onItemClickHandler, this));
    $list.delegate('LI A', 'mouseup', $.proxy(this._onItemClickHandler, this));
  },

  _detachListItemEventHandlers() {
    const $list = $(this._listElement);
    $list.undelegate('LI INPUT:checkbox', 'change');
    $list.undelegate('LI A', 'click');
    $list.undelegate('LI A', 'mouseup');
  },

  getListItems() {
    return $(this._listElement).find('LI');
  },

  getSelectedListItems() {
    return $(this._listElement).find('LI:has(INPUT:checkbox:checked)');
  },

  selectEntities(entityIDs) {
    let isChanged = false;
    this.deselectAllListItems();
    if (!$q.isNullOrEmpty(entityIDs) && $q.isArray(entityIDs)) {
      $(this._listElement).find('LI INPUT:checkbox').each((index, chb) => {
        const $chb = $(chb);
        if (entityIDs.indexOf($q.toInt($chb.val())) !== -1) {
          $chb.prop('checked', true);
          isChanged = true;
        }
      });
      if (isChanged) {
        this._setAsChanged();
      }
    }
  },

  _setAsChanged(refreshOnly) {
    const $list = $(this._listElement);
    $list.addClass(window.CHANGED_FIELD_CLASS_NAME);
    const operation = refreshOnly ? 'addClass' : 'removeClass';
    $list[operation](window.REFRESHED_FIELD_CLASS_NAME);
    const value = this.getSelectedEntityIDs();
    $list.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED,
      { fieldName: $list.data('list_item_name'), value, contentFieldName: $list.closest('dl').data('field_name') }
    );
  },

  getSelectedEntities() {
    const entities = [];
    const $selectedListItems = this.getSelectedListItems();

    $selectedListItems.each(
      (i, listItemElem) => {
        const $listItem = $(listItemElem);
        const $checkbox = $listItem.find('INPUT:checkbox');
        const $label = $listItem.find('LABEL');

        const entityId = +$checkbox.val() || 0;
        const entityName = $q.toString($label.text());

        Array.add(entities, { Id: entityId, Name: entityName });
      }
    );

    return entities;
  },

  _refreshListInner(dataItems, refreshOnly) {
    const newSelectedIDs = $.map(
      $.grep(dataItems, di => di.Selected),
      di => $q.toInt(di.Value)
    );
    const currentSelectedIDs = this.getSelectedEntityIDs();
    const selectedItemsIsChanged = _.union(
      _.difference(newSelectedIDs, currentSelectedIDs),
      _.difference(currentSelectedIDs, newSelectedIDs)
    ).length > 0;

    const $list = $(this._listElement);
    const $ul = $list.find('UL:first');
    const listItemHtml = new $.telerik.stringBuilder();
    for (let dataItemIndex = 0; dataItemIndex < dataItems.length; dataItemIndex++) {
      const dataItem = dataItems[dataItemIndex];
      this._getCheckBoxListItemHtml(listItemHtml, dataItem, dataItemIndex);
    }

    $ul.empty()
      .html(listItemHtml.string());

    this._refreshGroupCheckbox(dataItems.length);
    this._syncCountSpan(dataItems.length);

    if (selectedItemsIsChanged) {
      this._setAsChanged(refreshOnly);
    }
  },

  selectAllListItems() {
    this._changeAllListItemsSelection(true);
  },

  deselectAllListItems() {
    this._changeAllListItemsSelection(false);
  },

  _changeAllListItemsSelection(isSelect) {
    this.getListItems()
      .find('INPUT:checkbox')
      .prop('checked', isSelect);
    this._setAsChanged();
  },

  enableList() {
    $(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME);

    this.getListItems().find('INPUT:checkbox').prop('disabled', false);
    this._getGroupCheckbox().prop('disabled', false);

    this._enableAllToolbarButtons();
  },

  disableList() {
    $(this._listElement).addClass(this.LIST_DISABLED_CLASS_NAME);

    this.getListItems().find('INPUT:checkbox').prop('disabled', true);
    this._getGroupCheckbox().prop('disabled', true);

    this._disableAllToolbarButtons();
  },

  makeReadonly() {
    this.disableList();
    const $checked = this.getListItems().find('INPUT:checkbox:checked');
    $checked.each((i, cb) => {
      const $cb = $(cb);
      $cb.siblings(`input[name="${$cb.prop('name')}"]:hidden`).val($cb.val());
    });
  },

  _getCheckBoxListItemHtml(html, dataItem, dataItemIndex) {
    const itemElementName = this._listItemName;
    const itemElementId = String.format('{0}_{1}', this._listElementId, dataItemIndex);
    const itemValue = dataItem.Value;
    const itemText = dataItem.Text;
    const isChecked = dataItem.Selected;
    html
      .cat('<li>')
      .cat('<input type="checkbox" ')
      .cat(` name="${$q.htmlEncode(itemElementName)}"`)
      .cat(` id="${$q.htmlEncode(itemElementId)}"`)
      .cat(` value="${$q.htmlEncode(itemValue)}"`)
      .cat(' class="checkbox chb-list-item qp-notChangeTrack"')
      .catIf(' checked="checked"', isChecked)
      .catIf(' disabled ', this.isListDisabled())
      .cat('/> ')
      .cat(`<input type="hidden" value="false" name="${$q.htmlEncode(itemElementName)}"/>`)
      .cat(this._getIdLinkCode(itemValue))
      .cat(`<label for="${$q.htmlEncode(itemElementId)}">${itemText}</label>`)
      .cat('</li>');
  },

  isListChanged() {
    return $(this._listElement).hasClass(window.CHANGED_FIELD_CLASS_NAME);
  },

  _checkAllowShowingToolbar() {
    return this._addNewActionCode !== window.ACTION_CODE_NONE;
  },

  _onSelectedItemChangeHandler() {
    this._syncGroupCheckbox();
    this._syncCountSpan();
    this._setAsChanged();
  },

  dispose() {
    this._stopDeferredOperations = true;
    this._detachListItemEventHandlers();
    Quantumart.QP8.BackendEntityCheckBoxList.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendEntityCheckBoxList.registerClass(
  'Quantumart.QP8.BackendEntityCheckBoxList', Quantumart.QP8.BackendEntityDataListBase
);
