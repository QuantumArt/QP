// eslint-disable-next-line max-params
Quantumart.QP8.BackendEntitySingleItemPicker = function (
  listGroupCode,
  listElementId,
  entityTypeCode,
  parentEntityId,
  entityId,
  listType,
  options
) {
  Quantumart.QP8.BackendEntitySingleItemPicker.initializeBase(
    this,
    [listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options]
  );

  this._allowMultipleItemSelection = false;
  this._selectionMode = Quantumart.QP8.Enums.ListSelectionMode.OnlySelectedItems;
};

Quantumart.QP8.BackendEntitySingleItemPicker.prototype = {
  _displayFieldElement: null,
  _stateFieldElement: null,
  _pickButtonElement: null,
  _deselectButtonElement: null,

  DISPLAY_FIELD_CLASS_NAME: 'displayField',
  STATE_FIELD_CLASS_NAME: 'stateField',

  // eslint-disable-next-line max-statements
  initialize() {
    Quantumart.QP8.BackendEntitySingleItemPicker.callBaseMethod(this, 'initialize');

    let $copyButton, $pasteButton;
    const $list = $(this._listElement);
    const $displayField = $list.find(`.${this.DISPLAY_FIELD_CLASS_NAME}`);
    const $stateField = $list.find(`INPUT.${this.STATE_FIELD_CLASS_NAME}:first`);
    $stateField.addClass('qp-notChangeTrack');

    const $pickButton = this._createToolbarButton(
      `${this._listElementId}_PickButton`,
      $l.EntityDataList.pickSingleLinkButtonText,
      'pick'
    );

    this._addButtonToToolbar($pickButton);
    const $deselectButton = this._createToolbarButton(
      `${this._listElementId}_DeselectButton`,
      $l.EntityDataList.deselectLinkButtonText,
      'deselectAll'
    );

    this._addButtonToToolbar($deselectButton);
    if (this._enableCopy) {
      $copyButton = this._createToolbarButton(
        `${this._listElementId}_CopyButton`,
        $l.EntityDataList.copyLinkButtonText,
        'copy'
      );

      this._addButtonToToolbar($copyButton);
      $pasteButton = this._createToolbarButton(
        `${this._listElementId}_PasteButton`,
        $l.EntityDataList.pasteLinkButtonText,
        'paste'
      );

      this._addButtonToToolbar($pasteButton);
    }

    this._displayFieldElement = $displayField.get(0);
    this._stateFieldElement = $stateField.get(0);
    this._pickButtonElement = $pickButton.get(0);
    this._deselectButtonElement = $deselectButton.get(0);

    $stateField.bind('change', $.proxy(this._onSelectedItemChangeHandler, this));
    $pickButton.bind('click', $.proxy(this._onPickButtonClickHandler, this));
    $deselectButton.bind('click', $.proxy(this._onDeselectButtonClickHandler, this));
    $displayField.delegate('A', 'click', $.proxy(this._onItemClickHandler, this));
    $displayField.delegate('A', 'mouseup', $.proxy(this._onItemClickHandler, this));

    if (this._enableCopy) {
      this._copyButtonElement = $copyButton.get(0);
      $copyButton.bind('click', $.proxy(this._onCopyButtonClickHandler, this));

      this._pasteButtonElement = $pasteButton.get(0);
      $pasteButton.bind('click', $.proxy(this._onPasteButtonClickHandler, this));
    }

    if (!this._showIds) {
      this._addReadButtonToToolbar();
    }

    this._addNewButtonToToolbar();
  },

  getListItemCount() {
    return 1;
  },

  getSelectedListItemCount() {
    let selectedListItemCount = 0;
    const $stateField = $(this._stateFieldElement);

    if (!$q.isNullOrEmpty($stateField)) {
      const itemValue = +$stateField.val() || 0;
      if (itemValue !== 0) {
        selectedListItemCount = 1;
      }
    }

    return selectedListItemCount;
  },

  getSelectedEntities() {
    const entities = [];
    const $stateField = $(this._stateFieldElement);
    const $displayField = $(this._displayFieldElement);

    if (!$q.isNullOrEmpty($stateField) && !$q.isNullOrEmpty($displayField)) {
      const entityId = +$stateField.val() || 0;
      const entityName = $q.toString($displayField.find('.title').html(), '');

      if (entityId !== 0) {
        Array.add(entities, { Id: entityId, Name: entityName });
      }
    }

    return entities;
  },

  getStateFieldElement() {
    return this._stateFieldElement;
  },

  _refreshListInner(dataItems, refreshOnly) {
    let html = '';
    let value = '';
    if (!$q.isNullOrEmpty(dataItems)) {
      value = dataItems[0].Value;
      html = `${this._getIdLinkCode(value)}<span class="title">${dataItems[0].Text}</span>`;
    }

    const $displayField = $(this._displayFieldElement);
    const $stateField = $(this._stateFieldElement);

    $displayField.html(html);
    const oldValue = $stateField.val();
    $stateField.val(value);

    if (oldValue !== value) {
      $stateField.addClass(window.CHANGED_FIELD_CLASS_NAME);
      const operation = refreshOnly ? 'addClass' : 'removeClass';
      $stateField[operation](window.REFRESHED_FIELD_CLASS_NAME);
      $stateField.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, {
        fieldName: $stateField.attr('name'),
        value,
        contentFieldName: $stateField.data('content_field_name')
      });

      $stateField.change();
    }
  },

  deselectAllListItems() {
    this._refreshListInner([]);
    const eventArgs = new Quantumart.QP8.BackendEventArgs();
    this.notify(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, eventArgs);
  },

  selectEntities(entityId) {
    this.deselectAllListItems();
    if (!$q.isNullOrEmpty(entityId)) {
      if ($q.isArray(entityId) && entityId.length > 0) {
        this.selectEntities(entityId[0]);
      } else if ($.isNumeric(entityId)) {
        const selectedEntityIds = $.map([entityId], id => {
          return { Id: id };
        });

        this._loadSelectedItems(selectedEntityIds);
      }
    }
  },

  enableList() {
    $(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME);
    $(this._stateFieldElement).prop('disabled', false);
    this._enableAllToolbarButtons();
  },

  disableList() {
    $(this._listElement).addClass(this.LIST_DISABLED_CLASS_NAME);
    $(this._stateFieldElement).prop('disabled', true);
    this._disableAllToolbarButtons();
  },

  makeReadonly() {
    this.disableList();
    $(this._stateFieldElement).prop('disabled', false);
  },

  isListChanged() {
    return $(this._stateFieldElement).hasClass(window.CHANGED_FIELD_CLASS_NAME);
  },

  _onSelectedItemChangeHandler() {
    if (!this.isListDisabled()) {
      this._refreshReadToolbarButton(true);
    }
  },

  _onPickButtonClickHandler(ev) {
    if (!this.isListDisabled()) {
      this._openPopupWindow();
    }

    ev.stopImmediatePropagation();
  },

  _onDeselectButtonClickHandler(ev) {
    if (!this.isListDisabled()) {
      this.deselectAllListItems();
    }

    ev.stopImmediatePropagation();
  },

  dispose() {
    this._stopDeferredOperations = true;

    $(this._stateFieldElement).unbind('change');
    $(this._pickButtonElement).unbind('click');
    $(this._deselectButtonElement).unbind('click');
    $(this._displayFieldElement).undelegate('click');
    $(this._displayFieldElement).undelegate('mouseup');

    if (this._enableCopy) {
      $(this._copyButtonElement).unbind('click');
      $(this._pasteButtonElement).unbind('click');
    }

    this._stateFieldElement = null;
    this._displayFieldElement = null;
    this._pickButtonElement = null;
    this._deselectButtonElement = null;
    this._copyButtonElement = null;
    this._pasteButtonElement = null;

    Quantumart.QP8.BackendEntitySingleItemPicker.callBaseMethod(this, 'dispose');
  }
};

Quantumart.QP8.BackendEntitySingleItemPicker.registerClass(
  'Quantumart.QP8.BackendEntitySingleItemPicker',
  Quantumart.QP8.BackendEntityDataListBase
);
