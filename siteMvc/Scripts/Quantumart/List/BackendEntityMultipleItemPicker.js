// eslint-disable-next-line max-params
Quantumart.QP8.BackendEntityMultipleItemPicker = function (
  listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
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

  OVERFLOW_HIDDEN_CLASS: 'overflowHiddenValue',

  // eslint-disable-next-line camelcase
  getMaxListWidth() {
    return this._maxListWidth;
  },

  // eslint-disable-next-line camelcase
  setMaxListWidth(value) {
    this._maxListWidth = value;
  },

  // eslint-disable-next-line camelcase
  getMaxListHeight() {
    return this._maxListHeight;
  },

  // eslint-disable-next-line camelcase
  setMaxListHeight(value) {
    this._maxListHeight = value;
  },

  // eslint-disable-next-line max-statements
  initialize() {
    Quantumart.QP8.BackendEntityMultipleItemPicker.callBaseMethod(this, 'initialize');

    let $copyButton, $pasteButton;
    let $list = $(this._listElement);
    const countOverflowElement = $list.find(`.${this.OVERFLOW_HIDDEN_CLASS}`);
    if (countOverflowElement.length > 0) {
      this._countOverflowElement = countOverflowElement.get(0);
    }

    this._fixListOverflow();

    const count = this.getSelectedEntities().length;
    const hidden = count <= 1 || this._isCountOverflow();
    this._addCountDiv(count, false);
    this._addGroupCheckbox(hidden);
    this._syncGroupCheckbox();

    let $pickButton = this._createToolbarButton(
      `${this._listElementId}_PickButton`, $l.EntityDataList.pickLinkButtonText, 'pick'
    );
    this._addButtonToToolbar($pickButton);

    let $clearButton = this._createToolbarButton(
      `${this._listElementId}_ClearButton`, $l.EntityDataList.clearUnmarkedLinkButtonText, 'deselectAll'
    );
    this._addButtonToToolbar($clearButton);

    if (this._enableCopy) {
      $copyButton = this._createToolbarButton(
        `${this._listElementId}_CopyButton`, $l.EntityDataList.copyLinkButtonText, 'copy'
      );
      this._addButtonToToolbar($copyButton);

      $pasteButton = this._createToolbarButton(
        `${this._listElementId}_PasteButton`, $l.EntityDataList.pasteLinkButtonText, 'paste'
      );
      this._addButtonToToolbar($pasteButton);
    }

    this._addNewButtonToToolbar();

    this._pickButtonElement = $pickButton.get(0);
    $pickButton.bind('click', $.proxy(this._onPickButtonClickHandler, this));

    this._clearButtonElement = $clearButton.get(0);
    $clearButton.bind('click', $.proxy(this._onClearButtonClickHandler, this));
    this._refreshClearButton();

    if (this._enableCopy) {
      this._copyButtonElement = $copyButton.get(0);
      $copyButton.bind('click', $.proxy(this._onCopyButtonClickHandler, this));

      this._pasteButtonElement = $pasteButton.get(0);
      $pasteButton.bind('click', $.proxy(this._onPasteButtonClickHandler, this));
    }

    $list.delegate('LI INPUT:checkbox', 'change', $.proxy(this._onSelectedItemChangeHandler, this));
    $list.delegate('LI A', 'click', $.proxy(this._onItemClickHandler, this));
    $list.delegate('LI A', 'mouseup', $.proxy(this._onItemClickHandler, this));

    $pickButton = null;
    $clearButton = null;
    $list = null;
  },

  getListItems() {
    return $(this._listElement).find('UL LI');
  },

  getSelectedListItems() {
    return $(this._listElement).find('UL LI:has(INPUT:checkbox:checked)');
  },

  getSelectedEntities() {
    let result;
    if (this._isCountOverflow()) {
      const ids = $(this._countOverflowElement).val().split(',');
      result = ids.map(id => {
        return { Id: id, Name: '' };
      }) || [];
    } else {
      const $selectedListItems = this.getSelectedListItems();
      result = $selectedListItems.map((i, item) => {
        const $item = $(item);
        return {
          Id: +$item.find('INPUT:checkbox').val(),
          Name: $item.find('LABEL').text()
        };
      }).get() || [];
    }

    return result;
  },

  _isCountOverflow() {
    return !$q.isNull(this._countOverflowElement);
  },

  _refreshListInner(dataItems, refreshOnly) {
    const newSelectedIds = $.grep(dataItems, di => di.Value).map(di => $q.toInt(di.Value));
    const currentSelectedIds = this.getSelectedEntityIDs();

    const [
      { length: newSelectedIdsLength },
      { length: currentSelectedIdsLength }
    ] = [newSelectedIds, currentSelectedIds];

    const checkedStateWasChanged = newSelectedIdsLength !== currentSelectedIdsLength;
    const somethingNotUnderstandable = newSelectedIdsLength !== newSelectedIdsLength + currentSelectedIdsLength;
    const selectedItemsIsChanged = checkedStateWasChanged || somethingNotUnderstandable;

    if (selectedItemsIsChanged) {
      const $list = $(this._listElement);
      const $ul = $list.find('UL');

      if (newSelectedIdsLength < this._countLimit) {
        $ul.empty().html(this._getCheckBoxListHtml(dataItems));
        $(this._countOverflowElement).remove();
        this._countOverflowElement = null;
      } else {
        $ul.empty();
        const value = newSelectedIds.join();
        if (this._countOverflowElement) {
          $(this._countOverflowElement).val(value);
        } else {
          const ln = $list.data('list_item_name');
          const { OVERFLOW_HIDDEN_CLASS: overflowClass } = this;
          const html = `<input type="hidden" class="${overflowClass}" name="${ln}" value = "${value}" />`;
          $ul.before(html);

          this._countOverflowElement = $list.find(`.${this.OVERFLOW_HIDDEN_CLASS}`).get(0);
        }
      }

      this._setAsChanged(refreshOnly);
      this._refreshGroupCheckbox(dataItems.length);
      this._syncCountSpan(dataItems.length);
      this._refreshClearButton();
    }
  },

  appendEntities(entityIds) {
    if (entityIds && entityIds.length) {
      const selectedEntities = entityIds.map(i => {
        return { Id: i };
      });

      this._loadSelectedItems(selectedEntities);
    }
  },

  selectEntities(entityIds) {
    this.deselectAllListItems();
    if (entityIds && entityIds.length) {
      const selectedEntities = entityIds.map(i => {
        return { Id: i };
      });

      this._loadSelectedItems(selectedEntities);
    }
  },

  selectAllListItems() {
    this._changeAllListItemsSelection(true);
    this._refreshClearButton();
  },

  deselectAllListItems() {
    this._changeAllListItemsSelection(false);
    this._refreshClearButton();
  },

  removeAllListItems() {
    this.deselectAllListItems();
    this._onClearButtonClickHandler();
  },

  _changeAllListItemsSelection(isSelect) {
    this.getListItems().find('INPUT:checkbox').prop('checked', isSelect);
    this._setAsChanged();
  },

  enableList() {
    $(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME);

    this.getListItems().find('INPUT:checkbox').prop('disabled', false);
    this._getGroupCheckbox().prop('disabled', false);
    this._enableAllToolbarButtons();
    this._refreshClearButton();
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

  isListChanged() {
    return $(this._listElement).hasClass(window.CHANGED_FIELD_CLASS_NAME);
  },

  _setAsChanged(refreshOnly) {
    const $list = $(this._listElement);
    $list.addClass(window.CHANGED_FIELD_CLASS_NAME);

    const operation = refreshOnly ? 'addClass' : 'removeClass';
    $list[operation](window.REFRESHED_FIELD_CLASS_NAME);

    const value = this.getSelectedEntityIDs();
    $list.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, {
      fieldName: $list.data('list_item_name'), value, contentFieldName: $list.closest('dl').data('field_name')
    });
  },

  _getCheckBoxListHtml(dataItems) {
    const html = new $.telerik.stringBuilder();
    for (let dataItemIndex = 0; dataItemIndex < dataItems.length; dataItemIndex++) {
      const dataItem = dataItems[dataItemIndex];
      this._getCheckBoxListItemHtml(html, dataItem, dataItemIndex);
    }

    return html.string();
  },

  _getCheckBoxListItemHtml(html, dataItem, dataItemIndex) {
    const itemElementName = this._listItemName;
    const itemElementId = String.format('{0}_{1}', this._listElementId, dataItemIndex);
    const itemValue = dataItem.Value;
    const itemText = dataItem.Text;

    html
      .cat('<li>')
      .cat('<input type="checkbox"')
      .cat(` name="${$q.htmlEncode(itemElementName)}"`)
      .cat(` id="${$q.htmlEncode(itemElementId)}"`)
      .cat(` value="${$q.htmlEncode(itemValue)}"`)
      .cat(' class="checkbox multi-picker-item qp-notChangeTrack"')
      .cat(' checked="checked"')
      .catIf(' disabled ', this.isListDisabled())
      .cat('/> ')
      .cat(`<input type="hidden" value="false" name="${$q.htmlEncode(itemElementName)}"/>`)
      .cat(this._getIdLinkCode(itemValue))
      .cat(`<label for="${$q.htmlEncode(itemElementId)}">${itemText}</label>`)
      .cat('</li>');
  },

  _onPickButtonClickHandler() {
    if (!this.isListDisabled()) {
      this._openPopupWindow();
    }
  },

  _onClearButtonClickHandler() {
    $(this._listElement).find('LI:has(INPUT:checkbox:not(:checked))').remove();

    const newCount = this.getListItemCount();
    this._refreshGroupCheckbox(newCount);
    this._syncCountSpan(newCount);
    this._refreshClearButton();
    this._fixListOverflow();
  },

  _checkAllowShowingToolbar() {
    return this._selectActionCode !== window.ACTION_CODE_NONE;
  },

  _onSelectedItemChangeHandler() {
    this._syncGroupCheckbox();
    this._syncCountSpan();
    this._refreshClearButton();
    this._setAsChanged();
    this.notify(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, new Quantumart.QP8.BackendEventArgs());
  },

  _refreshClearButton() {
    if (this._clearButtonElement) {
      this._changeToolbarButtonState(
        $(this._clearButtonElement), !this._isCountOverflow()
        && this.getSelectedListItemCount() !== this.getListItemCount()
      );
    }
  },

  dispose() {
    this._stopDeferredOperations = true;

    $(this._pickButtonElement).unbind('click');
    $(this._clearButtonElement).unbind('click');
    if (this._enableCopy) {
      $(this._copyButtonElement).unbind('click');
      $(this._pasteButtonElement).unbind('click');
    }

    $(this._listElement).undelegate('LI INPUT:checkbox', 'change');
    $(this._listElement).undelegate('LI A', 'click');
    $(this._listElement).undelegate('LI A', 'mouseup');

    this._pickButtonElement = null;
    this._clearButtonElement = null;
    this._copyButtonElement = null;
    this._pasteButtonElement = null;
    this._listElement = null;

    Quantumart.QP8.BackendEntityMultipleItemPicker.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendEntityMultipleItemPicker.registerClass(
  'Quantumart.QP8.BackendEntityMultipleItemPicker', Quantumart.QP8.BackendEntityDataListBase
);
