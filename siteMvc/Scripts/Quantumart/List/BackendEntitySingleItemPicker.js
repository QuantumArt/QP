Quantumart.QP8.BackendEntitySingleItemPicker = function (listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
  Quantumart.QP8.BackendEntitySingleItemPicker.initializeBase(this,
    [listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options]);

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

  initialize: function () {
    Quantumart.QP8.BackendEntitySingleItemPicker.callBaseMethod(this, 'initialize');

    var $list = $(this._listElement);
    var $displayField = $list.find(`.${  this.DISPLAY_FIELD_CLASS_NAME}`);
    var $stateField = $list.find(`INPUT.${  this.STATE_FIELD_CLASS_NAME  }:first`);
    $stateField.addClass('qp-notChangeTrack');

    var $pickButton = this._createToolbarButton(`${this._listElementId  }_PickButton`, $l.EntityDataList.pickSingleLinkButtonText, 'pick');
    this._addButtonToToolbar($pickButton);

    var $deselectButton = this._createToolbarButton(`${this._listElementId  }_DeselectButton`, $l.EntityDataList.deselectLinkButtonText, 'deselectAll');
    this._addButtonToToolbar($deselectButton);

    if (this._enableCopy) {
      var $copyButton = this._createToolbarButton(`${this._listElementId  }_CopyButton`, $l.EntityDataList.copyLinkButtonText, 'copy');
      this._addButtonToToolbar($copyButton);

      var $pasteButton = this._createToolbarButton(`${this._listElementId  }_PasteButton`, $l.EntityDataList.pasteLinkButtonText, 'paste');
      this._addButtonToToolbar($pasteButton);
    }

    this._displayFieldElement = $displayField.get(0);
    this._stateFieldElement = $stateField.get(0);
    this._pickButtonElement = $pickButton.get(0);
    this._deselectButtonElement = $deselectButton.get(0);

    $stateField.bind('change', jQuery.proxy(this._onSelectedItemChangeHandler, this));
    $pickButton.bind('click', jQuery.proxy(this._onPickButtonClickHandler, this));
    $deselectButton.bind('click', jQuery.proxy(this._onDeselectButtonClickHandler, this));
    $displayField.delegate('A', 'click', jQuery.proxy(this._onItemClickHandler, this));
    $displayField.delegate('A', 'mouseup', jQuery.proxy(this._onItemClickHandler, this));

    if (this._enableCopy) {
      this._copyButtonElement = $copyButton.get(0);
      $copyButton.bind('click', jQuery.proxy(this._onCopyButtonClickHandler, this));

      this._pasteButtonElement = $pasteButton.get(0);
      $pasteButton.bind('click', jQuery.proxy(this._onPasteButtonClickHandler, this));
    }

    if (!this._showIds) {
      this._addReadButtonToToolbar();
    }

    this._addNewButtonToToolbar();

    $list = null;
    $displayField = null;
    $stateField = null;
    $pickButton = null;
    $deselectButton = null;
  },

  getListItemCount: function () {
    var listItemCount = 1;

    return listItemCount;
  },

  getSelectedListItemCount: function () {
    var selectedListItemCount = 0;
    var $stateField = $(this._stateFieldElement);

    if (!$q.isNullOrEmpty($stateField)) {
      var itemValue = +$stateField.val() || 0;
      if (itemValue != 0) {
        selectedListItemCount = 1;
      }
    }

    return selectedListItemCount;
  },

  getSelectedEntities: function () {
    var entities = [];
    var $stateField = $(this._stateFieldElement);
    var $displayField = $(this._displayFieldElement);

    if (!$q.isNullOrEmpty($stateField) && !$q.isNullOrEmpty($displayField)) {
      var entityId = +$stateField.val() || 0;
      var entityName = $q.toString($displayField.find('.title').html(), '');

      if (entityId != 0) {
        Array.add(entities, { Id: entityId, Name: entityName });
      }
    }

    return entities;
  },

  getStateFieldElement: function () {
    return this._stateFieldElement;
  },

  _refreshListInner: function (dataItems, refreshOnly) {
    var html = '';
    var value = '';
    if (!$q.isNullOrEmpty(dataItems)) {
      value = dataItems[0].Value;
      html = `${this._getIdLinkCode(value)  }<span class="title">${  dataItems[0].Text  }</span>`;
    }

    var $displayField = $(this._displayFieldElement);
    var $stateField = $(this._stateFieldElement);

    $displayField.html(html);
    var oldValue = $stateField.val();
    $stateField.val(value);

    if (oldValue != value) {
      $stateField.addClass(window.CHANGED_FIELD_CLASS_NAME);
      var operation = refreshOnly ? 'addClass' : 'removeClass';
      $stateField[operation](window.REFRESHED_FIELD_CLASS_NAME);
      $stateField.trigger(window.JQ_CUSTOM_EVENT_ON_FIELD_CHANGED, { fieldName: $stateField.attr('name'), value: value, contentFieldName: $stateField.data('content_field_name') });
      $stateField.change();
    }

    $displayField = null;
    $stateField = null;
  },

  deselectAllListItems: function () {
    this._refreshListInner([]);
    var eventArgs = new Quantumart.QP8.BackendEventArgs();
    this.notify(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, eventArgs);
  },

  selectEntities: function (entityId) {
    this.deselectAllListItems();
    if (!$q.isNullOrEmpty(entityId)) {
      if ($q.isArray(entityId) && entityId.length > 0) {
        this.selectEntities(entityId[0]);
      } else if ($.isNumeric(entityId)) {
        var selectedEntityIds = $.map([entityId], (id) => {
          return { Id: id };
        });

        this._loadSelectedItems(selectedEntityIds);
      }
    }
  },

  enableList: function () {
    $(this._listElement).removeClass(this.LIST_DISABLED_CLASS_NAME);
    $(this._stateFieldElement).prop('disabled', false);
    this._enableAllToolbarButtons();
  },

  disableList: function () {
    $(this._listElement).addClass(this.LIST_DISABLED_CLASS_NAME);
    $(this._stateFieldElement).prop('disabled', true);
    this._disableAllToolbarButtons();
  },

  makeReadonly: function () {
    this.disableList();
    $(this._stateFieldElement).prop('disabled', false);
  },

  isListChanged: function () {
    var $stateField = $(this._stateFieldElement);
    var result = $stateField.hasClass(window.CHANGED_FIELD_CLASS_NAME);
    $stateField = null;
    return result;
  },

  _onSelectedItemChangeHandler: function () {
    if (!this.isListDisabled()) {
      this._refreshReadToolbarButton(true);
    }
  },

  _onPickButtonClickHandler: function (event) {
    if (!this.isListDisabled()) {
      this._openPopupWindow();
    }
    event.stopImmediatePropagation();
  },

  _onDeselectButtonClickHandler: function (event) {
    if (!this.isListDisabled()) {
      this.deselectAllListItems();
    }
    event.stopImmediatePropagation();
  },

  dispose: function () {
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

Quantumart.QP8.BackendEntitySingleItemPicker.registerClass('Quantumart.QP8.BackendEntitySingleItemPicker', Quantumart.QP8.BackendEntityDataListBase);
