window.EVENT_TYPE_ENTITY_LIST_ACTION_EXECUTING = 'OnEntityListActionExecuting';
window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED = 'OnEntityListSelectionChanged';

Quantumart.QP8.Enums.DataListType = function () { };
Quantumart.QP8.Enums.DataListType.prototype = {
  None: 0,
  DropDownList: 1,
  RadioButtonList: 2,
  CheckBoxList: 3,
  BulletedList: 4,
  SingleItemPicker: 5,
  MultipleItemPicker: 6
};

Quantumart.QP8.Enums.DataListType.registerEnum('Quantumart.QP8.Enums.DataListType');
Quantumart.QP8.BackendEntityDataListBase = function (listGroupCode, listElementId, entityTypeCode, parentEntityId, entityId, listType, options) {
  Quantumart.QP8.BackendEntityDataListBase.initializeBase(this);
  this._listGroupCode = listGroupCode;
  this._listElementId = listElementId;
  this._entityTypeCode = entityTypeCode;
  this._parentEntityId = parentEntityId;
  this._entityId = entityId;
  this._listType = listType;

  if ($q.isObject(options)) {
    this._listId = options.listId;
    this._listItemName = options.listItemName;
    this._addNewActionCode = options.addNewActionCode;
    this._readActionCode = options.readActionCode;
    this._selectActionCode = options.selectActionCode;
    this._maxListWidth = options.maxListWidth;
    this._maxListHeight = options.maxListHeight;
    this._showIds = options.showIds;
    this._filter = options.filter;
    this._initFilter = this._filter;
    this._hostIsWindow = options.hostIsWindow;
    this._isCollapsable = options.isCollapsable;
    this._enableCopy = options.enableCopy;
    this._readDataOnInsert = options.readDataOnInsert;
    this._countLimit = options.countLimit;
  }
};

Quantumart.QP8.BackendEntityDataListBase.prototype = {
  _listGroupCode: '',
  _listElementId: '',
  _listElement: null,
  _expandLinkElement: null,
  _collapseLinkElement: null,
  _listWrapperElement: null,
  _toolbarElement: null,
  _entityTypeCode: '',
  _entityId: 0,
  _parentEntityId: 0,
  _listType: 0,
  _listId: 0,
  _listItemName: '',
  _allowMultipleItemSelection: false,
  _selectionMode: null,
  _maxListWidth: 0,
  _maxListHeight: 0,
  _addNewActionCode: '',
  _readActionCode: '',
  _selectActionCode: '',
  _addNewButtonElement: null,
  _readButtonElement: null,
  _stopDeferredOperations: false,
  _listManagerComponent: null,
  _selectPopupWindowComponent: null,
  _showIds: false,
  _filter: '',
  _initFilter: '',
  _hostIsWindow: false,
  _isCollapsable: false,
  _enableCopy: true,
  _readDataOnInsert: false,

  TOOLBAR_BUTTON_DISABLED_CLASS_NAME: 'disabled',
  LIST_WRAPPER_CLASS_NAME: 'dataListWrapper',
  LINK_BUTTON_CLASS_NAME: 'linkButton',
  ACTION_LINK_CLASS_NAME: 'actionLink',
  LINK_BUTTON_LIST_CLASS_NAME: 'linkButtons',
  OVERFLOW_LIST_CLASS_NAME: 'overflow',
  LIST_DISABLED_CLASS_NAME: 'disabled',
  SELF_CLEAR_FLOATS_CLASS_NAME: 'group',

  get_listElementId: function () {
    return this._listElementId;
  },

  set_listElementId: function (value) {
    this._listElementId = value;
  },

  get_entityTypeCode: function () {
    return this._entityTypeCode;
  },

  set_entityTypeCode: function (value) {
    this._entityTypeCode = value;
  },

  get_parentEntityId: function () {
    return this._parentEntityId;
  },

  set_parentEntityId: function (value) {
    this._parentEntityId = value;
  },

  get_entityId: function () {
    return this._entityId;
  },

  set_entityId: function (value) {
    this._entityId = value;
  },

  get_listType: function () {
    return this._listType;
  },

  set_listType: function (value) {
    this._listType = value;
  },

  get_listId: function () {
    return this._listId;
  },

  set_listId: function (value) {
    this._listId = value;
  },

  get_addNewActionCode: function () {
    return this._addNewActionCode;
  },

  set_addNewActionCode: function (value) {
    this._addNewActionCode = value;
  },

  get_readActionCode: function () {
    return this._readActionCode;
  },

  set_readActionCode: function (value) {
    this._readActionCode = value;
  },

  get_selectActionCode: function () {
    return this._selectActionCode;
  },

  set_selectActionCode: function (value) {
    this._selectActionCode = value;
  },

  get_listGroupCode: function () {
    return this._listGroupCode;
  },

  get_listManagerComponent: function () {
    return this._listManagerComponent;
  },

  set_listManagerComponent: function (value) {
    this._listManagerComponent = value;
  },


  initialize: function () {
    if (!this._listElementId) {
      $q.alertFail('_listElementId');
    }
    var $list = $(`#${  this._listElementId}`);
    $list.wrap('<div />');

    var $listWrapper = $list.parent();
    $listWrapper.addClass(this.LIST_WRAPPER_CLASS_NAME);

    var $toolbar = $('<ul />', { class: this.LINK_BUTTON_LIST_CLASS_NAME });
    $toolbar.addClass(this.SELF_CLEAR_FLOATS_CLASS_NAME);
    $toolbar.prependTo($listWrapper);

    $listWrapper.wrap('<div />');
    if (this._isCollapsable) {
      this._createCollapsingToolbar($listWrapper.parent());
    }

    this._listElement = $list.get(0);
    this._listWrapperElement = $listWrapper.get(0);
    this._toolbarElement = $toolbar.get(0);

    if (this._checkAllowShowingToolbar()) {
      this._showToolbar();
    } else {
      this._hideToolbar();
    }
  },

  _createCollapsingToolbar: function (mainWrapperElement) {
    if (mainWrapperElement) {
      var html = new $.telerik.stringBuilder();
      html
        .cat("<div class='collapsingToolbar'>")
        .cat("<ul class='linkButtons group'>")
        .cat("<li style='display: block;' class='expand'>")
        .cat("<span class='linkButton actionLink'>")
        .cat("<a href='javascript:void(0);'>")
        .cat("<span class='icon expand'><img src='/Backend/Content/Common/0.gif'></span>")
        .cat("<span class='text'>")
        .cat($l.EntityDataList.showListLinkButtonText)
        .cat('</span>')
        .cat('</a>')
        .cat('</span>')
        .cat('</li>')
        .cat("<li style='display: none;' class='collapse'>")
        .cat("<span class='linkButton actionLink'>")
        .cat("<a href='javascript:void(0);'>")
        .cat("<span class='icon collapse'><img src='/Backend/Content/Common/0.gif'></span>")
        .cat("<span class='text'>")
        .cat($l.EntityDataList.hideListLinkButtonText)
        .cat('</span>')
        .cat('</a>')
        .cat('</span>')
        .cat('</li>')
        .cat('</ul>')
        .cat('</div>');

      var $collapsingToolbar = $(html.string());
      var that = this;
      this._expandLinkElement = $collapsingToolbar.find('LI.expand').click(function (e) {
        $(this).hide();
        $(that._listWrapperElement).show();
        that._fixListOverflow();
        $(that._collapseLinkElement).show();
        e.preventDefault();
      }).get(0);

      this._collapseLinkElement = $collapsingToolbar.find('LI.collapse').click(function (e) {
        $(this).hide();
        $(that._listWrapperElement).hide();
        $(that._expandLinkElement).show();
        e.preventDefault();
      }).get(0);

      $(mainWrapperElement).prepend($collapsingToolbar).find(`.${  this.LIST_WRAPPER_CLASS_NAME}`).hide();
      $collapsingToolbar = null;
    }
  },

  _fixListOverflow: function () {
    var $list = $(this._listElement);
    $list.removeClass(this.OVERFLOW_LIST_CLASS_NAME);

    var $ul = $('UL', $list);
    if ($ul.length > 0) {
      $list.height($ul.height());
      var contentHeight = $list.get(0).scrollHeight;
      if (contentHeight > $list.height()) {
        if (this._maxListHeight > 0 && contentHeight > this._maxListHeight) {
          $list.height(this._maxListHeight);
          $list.addClass(this.OVERFLOW_LIST_CLASS_NAME);
        } else {
          $list.height(contentHeight);
        }
      }
    }

    $ul = null;
    $list = null;
  },

  getListItems: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  getListItemCount: function () {
    return this.getListItems().length;
  },

  getSelectedListItems: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  getSelectedListItemCount: function () {
    return this.getSelectedListItems().length;
  },

  getSelectedEntities: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  getSelectedEntityIDs: function () {
    return $.grep($.map(this.getSelectedEntities(), function (item) {
      return $q.toInt(item.Id);
    }), function (item) {
      return item;
    });
  },

  selectEntities: function (entityIDs) {
    Sys.Debug.trace(entityIDs);
  },

  refreshList: function (testEntityId) {
    var selectedEntitiesIDs = _.pluck(this.getSelectedEntities(), 'Id');
    var dataItems = $o.getSimpleEntityList(this._entityTypeCode, this._parentEntityId, this._entityId, this._listId, this._selectionMode, selectedEntitiesIDs, this._filter, testEntityId);
    if (dataItems) {
      this._refreshListInner(dataItems, true);
      this._fixListOverflow();
    }
  },

  _refreshListInner: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  selectAllListItems: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  deselectAllListItems: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  enableList: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  disableList: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  makeReadonly: function () {},

  isListChanged: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  isListOverflow: function () {
    throw new Error($l.Common.methodNotImplemented);
  },

  isListDisabled: function () {
    return $(this._listElement).hasClass(this.LIST_DISABLED_CLASS_NAME);
  },

  _showToolbar: function () {
    $(this._toolbarElement).css('display', 'block');
  },

  _hideToolbar: function () {
    $(this._toolbarElement).css('display', 'none');
  },

  _checkAllowShowingToolbar: function () {
    return true;
  },

  _getToolbarButtons: function () {
    return $(`.${  this.LINK_BUTTON_CLASS_NAME}`, this._toolbarElement);
  },

  _createToolbarButton: function (id, text, cssClassName) {
    return this._createLinkButton(id, text, cssClassName);
  },

  _isToolbarButtonDisabled: function (buttonElem) {
    var isDisabled;
    var $linkButton = $q.toJQuery(buttonElem);
    var actionLink = $linkButton.data('action_link_component');

    if (actionLink) {
      isDisabled = actionLink.isActionLinkDisabled();
    } else {
      isDisabled = $linkButton.find('A:first').hasClass(this.TOOLBAR_BUTTON_DISABLED_CLASS_NAME);
    }

    return isDisabled;
  },

  _changeToolbarButtonState: function (buttonElem, state) {
    var $linkButton = $q.toJQuery(buttonElem);
    var actionLink = $linkButton.data('action_link_component');

    if (actionLink) {
      if (state) {
        actionLink.enableActionLink();
      } else {
        actionLink.disableActionLink();
      }
    } else if (state) {
      $linkButton.find('A:first').removeClass(this.TOOLBAR_BUTTON_DISABLED_CLASS_NAME);
    } else {
      $linkButton.find('A:first').addClass(this.TOOLBAR_BUTTON_DISABLED_CLASS_NAME);
    }
  },

  _createLinkButton: function (id, text, cssClassName) {
    var $linkButton = $('<span />', { id: id, class: `${this.LINK_BUTTON_CLASS_NAME  } ${  this.ACTION_LINK_CLASS_NAME}` });
    var linkContentHtml = new $.telerik.stringBuilder();

    linkContentHtml
      .cat('<a href="javascript:void(0);">')
      .cat(`<span class="icon${  !$q.isNullOrWhiteSpace(cssClassName) ? ` ${  $q.htmlEncode(cssClassName)}` : ''  }">`)
      .cat(`<img src="${  window.COMMON_IMAGE_FOLDER_URL_ROOT  }0.gif" />`)
      .cat('</span>')
      .cat(`<span class="text">${  text  }</span>`)
      .cat('</a>');

    $linkButton.html(linkContentHtml.string());
    return $linkButton;
  },

  _extendLink: function ($linkButton, actionCode) {
    if (!$q.isNullOrEmpty($linkButton)) {
      var entityTypeCode = this._entityTypeCode;
      var entityId = 0;
      var entityName = '';
      var parentEntityId = this._parentEntityId;
      var actionTypeCode = Quantumart.QP8.BackendActionType.getActionTypeCodeByActionCode(actionCode);
      if (actionTypeCode == window.ACTION_TYPE_CODE_READ) {
        var entities = this.getSelectedEntities();
        if (entities.length > 0) {
          var entity = entities[0];

          entityId = entity.Id;
          entityName = $o.getEntityName(entityTypeCode, entityId, parentEntityId);

          entity = null;
        }

        $q.clearArray(entities);
      }

      $linkButton.data('entity_id', entityId);
      $linkButton.data('entity_name', entityName);
      $linkButton.data('parent_entity_id', parentEntityId);
      $linkButton.data('action_type_code', actionTypeCode);
      $linkButton.data('action_code', actionCode);
      if (actionTypeCode == window.ACTION_TYPE_CODE_ADD_NEW && entityTypeCode == window.ENTITY_TYPE_CODE_ARTICLE) {
        $linkButton.data('context', {
          additionalUrlParameters: {
            fieldId: this._listId,
            articleId: this._entityId
          }
        });
      }
      var actionTarget = this._hostIsWindow ? Quantumart.QP8.Enums.ActionTargetType.NewWindow : Quantumart.QP8.Enums.ActionTargetType.NewTab;
      $linkButton.data('action_target_type', actionTarget);
    }
  },

  _addButtonToToolbar: function (buttonElem) {
    var $button = $q.toJQuery(buttonElem);
    $(this._toolbarElement).append($button);
    $button.wrap('<li />');
  },

  _addLinkToToolbar: function (linkElem, actionCode) {
    var $link = $q.toJQuery(linkElem);
    this._addButtonToToolbar($link);
    this._extendLink($link, actionCode);
  },

  _addNewButtonToToolbar: function () {
    if (this._addNewActionCode != window.ACTION_CODE_NONE) {
      var $addNewButton = this._createToolbarButton(`${this._listElementId  }_AddNewButton`, $l.EntityDataList.addNewActionLinkButtonText, 'add');
      this._addLinkToToolbar($addNewButton, this._addNewActionCode);
      this._addNewButtonElement = $addNewButton.get(0);
    }
  },

  _addReadButtonToToolbar: function () {
    if (this._readActionCode != window.ACTION_CODE_NONE) {
      var $readActionButton = this._createToolbarButton(`${this._listElementId  }_ReadButton`, $l.EntityDataList.readActionLinkButtonText, 'edit');
      this._changeToolbarButtonState($readActionButton, this.getSelectedListItemCount() > 0);
      this._addLinkToToolbar($readActionButton, this._readActionCode);
      this._readButtonElement = $readActionButton.get(0);
    }
  },

  _refreshReadToolbarButton: function (refreshActionLinkProperties) {
    if ($q.isNull(refreshActionLinkProperties)) {
      refreshActionLinkProperties = false;
    }

    if (this._readButtonElement) {
      var $readActionButton = $(this._readButtonElement);
      this._changeToolbarButtonState($readActionButton, this.getSelectedListItemCount() > 0);

      if (refreshActionLinkProperties) {
        var actionLink = $readActionButton.data('action_link_component');
        if (actionLink) {
          var entityId = 0;
          var entityName = '';

          var entities = this.getSelectedEntities();
          if (entities.length > 0) {
            var entity = entities[0];

            entityId = entity.Id;
            entityName = $o.getEntityName(this._entityTypeCode, entityId, this._parentEntityId);

            entity = null;
          }

          $q.clearArray(entities);

          actionLink.set_entityId(entityId);
          actionLink.set_entityName(entityName);
        }
      }

      $readActionButton = null;
    }
  },

  _enableAllToolbarButtons: function () {
    var $linkButtons = this._getToolbarButtons();
    var linkButtonCount = $linkButtons.length;

    for (var linkButtonIndex = 0; linkButtonIndex < linkButtonCount; linkButtonIndex++) {
      var $linkButton = $linkButtons.eq(linkButtonIndex);
      this._changeToolbarButtonState($linkButton, true);
    }

    $linkButtons = null;

    this._refreshReadToolbarButton();
  },

  _disableAllToolbarButtons: function () {
    var $linkButtons = this._getToolbarButtons();
    var linkButtonCount = $linkButtons.length;

    for (var linkButtonIndex = 0; linkButtonIndex < linkButtonCount; linkButtonIndex++) {
      var $linkButton = $linkButtons.eq(linkButtonIndex);
      if ($linkButton.data('action_code') !== this._readActionCode) {
        this._changeToolbarButtonState($linkButton, false);
      }
    }

    $linkButtons = null;
  },

  _openPopupWindow: function () {
    var eventArgs = new Quantumart.QP8.BackendEventArgs();
    eventArgs.set_isMultipleEntities(this._allowMultipleItemSelection);
    eventArgs.set_parentEntityId(this._parentEntityId);
    eventArgs.set_entityTypeCode(this._entityTypeCode);
    eventArgs.set_actionCode(this._selectActionCode);
    var entities = this.getSelectedEntities();
    if (entities.length > 0) {
      if (this._allowMultipleItemSelection) {
        eventArgs.set_entities(entities);
      } else {
        eventArgs.set_entityId(entities[0].Id);
        eventArgs.set_entityName(entities[0].Name);
      }
    }

    this._selectPopupWindowComponent = new Quantumart.QP8.BackendSelectPopupWindow(eventArgs, {
      filter: this._filter
    });

    this._selectPopupWindowComponent.attachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, $.proxy(this._popupWindowSelectedHandler, this));
    this._selectPopupWindowComponent.attachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, $.proxy(this._popupWindowClosedHandler, this));
    this._selectPopupWindowComponent.openWindow();

    eventArgs = null;
  },

  _popupWindowSelectedHandler: function (eventType, sender, args) {
    this._loadSelectedItems(args.entities);
    this._destroyPopupWindow();
  },

  _loadSelectedItems: function (entities) {
    var dataItems;
    if (($o.checkEntitiesForPresenceEmptyNames(entities) || this._readDataOnInsert) && entities.length <= this._countLimit) {
      var selectedEntitiesIDs = _.pluck(entities, 'Id');
      dataItems = $o.getSimpleEntityList(this._entityTypeCode, this._parentEntityId, this._entityId, this._listId, this._selectionMode, selectedEntitiesIDs, this._filter, 0);
    } else {
      dataItems = $c.getListItemCollectionFromEntities(entities);
    }

    if (dataItems) {
      this._refreshListInner(dataItems, false);
      this._fixListOverflow();
    }

    $q.clearArray(dataItems);
    this.notify(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, new Quantumart.QP8.BackendEventArgs());
  },

  _popupWindowClosedHandler: function () {
    this._destroyPopupWindow();
  },

  _destroyPopupWindow: function () {
    if (this._selectPopupWindowComponent) {
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      this._selectPopupWindowComponent.closeWindow();
      this._selectPopupWindowComponent.dispose();
      this._selectPopupWindowComponent = null;
    }
  },

  _getGroupCheckboxHtml: function (hidden) {
    var idFormat = '{0}_{1}';
    var idPrefix = 'group';
    var itemElementName = String.format(idFormat, idPrefix, this._listItemName);
    var itemElementId = String.format(idFormat, idPrefix, this._listElementId);
    var itemText = $l.EntityDataList.selectDeselectAllText;
    var style = hidden ? 'style="display:none"' : '';

    var html = new $.telerik.stringBuilder();
    html
      .cat(`<div class="groupCheckbox" ${  style  }>`)
      .cat('<input type="checkbox"')
      .cat(` name="${  $q.htmlEncode(itemElementName)  }"`)
      .cat(` id="${  $q.htmlEncode(itemElementId)  }"`)
      .cat(' value="1"')
      .cat(' class="checkbox group qp-notChangeTrack"')
      .cat('/>')
      .cat(' ')
      .cat(`<label for="${  $q.htmlEncode(itemElementId)  }">${  itemText  }</label>`)
      .cat('</div>');

    return html.string();
  },

  _getCountDivHtml: function (newCount, hidden) {
    var html = new $.telerik.stringBuilder();
    var style = hidden ? 'style="display:none"' : '';
    var countText = String.format($l.EntityDataList.countText, `<span class="countItems">${  newCount  }</span>`);
    var overflowText = `<span class="overflowText">${  this._getOverflowText()  }</span>`;
    html.cat(`<div class="countItemsBlock" ${  style  }>`).cat(countText).cat(overflowText).cat('</div>');

    return html.string();
  },

  _refreshGroupCheckbox: function (newCount) {
    if (newCount > 1 && !this._isCountOverflow()) {
      this._showGroupCheckbox();
    } else {
      this._hideGroupCheckbox();
    }

    this._syncGroupCheckbox();
  },

  _addGroupCheckbox: function (hidden) {
    $(this._toolbarElement).after(this._getGroupCheckboxHtml(hidden));
    this._getGroupCheckbox().bind('click', $.proxy(this._groupCheckboxClickHandler, this));
  },

  _showGroupCheckbox: function () {
    this._getGroupCheckbox().parent().show();
  },

  _hideGroupCheckbox: function (newCount) {
    this._getGroupCheckbox().parent().hide();
  },

  _destroyGroupCheckbox: function () {
    this._getGroupCheckbox().unbind('click').parent().remove();
  },

  _addCountDiv: function (count, hidden) {
    $(this._toolbarElement).after(this._getCountDivHtml(count, hidden));
  },

  _destroyCountDiv: function () {
    this._getCountSpan().parent().remove();
  },

  _getGroupCheckbox: function () {
    return $(this._listWrapperElement).find('.groupCheckbox INPUT:checkbox');
  },

  _getCountSpan: function () {
    return $(this._listWrapperElement).find('.countItems');
  },

  _getOverflowSpan: function () {
    return $(this._listWrapperElement).find('.overflowText');
  },

  _groupCheckboxClickHandler: function () {
    if (this._getGroupCheckbox().is(':checked')) {
      this.selectAllListItems();
    } else {
      this.deselectAllListItems();
    }

    this._syncCountSpan();
  },

  _syncGroupCheckbox: function () {
    this._getGroupCheckbox().prop('checked', this.getListItemCount() == this.getSelectedListItemCount());
  },

  _syncCountSpan: function (count) {
    this._getCountSpan().html(`${  count || this.getSelectedEntities().length}`);
    this._getOverflowSpan().html(this._getOverflowText());
  },

  _getOverflowText: function () {
    return this._isCountOverflow() ? `. ${  String.format($l.EntityDataList.overFlowText, this._countLimit)}` : '';
  },

  _onItemClickHandler: function (e) {
    e.preventDefault();
    var isLeftClick = e.type == 'click' && (e.which == 1 || e.which == 0);
    var isMiddleClick = e.type == 'mouseup' && e.which == 2;
    if (!isLeftClick && !isMiddleClick) {
      return false;
    }

    var $obj = $(e.target);
    var eventArgs = new Quantumart.QP8.BackendEventArgs();
    eventArgs.set_actionCode(this._readActionCode);
    eventArgs.set_entityId($obj.html());
    eventArgs.set_entityName($obj.closest('LI').children('LABEL').html());
    eventArgs.set_entityTypeCode(this._entityTypeCode);
    eventArgs.set_parentEntityId(this._parentEntityId);

    if (this._hostIsWindow) {
      var message = Quantumart.QP8.Backend.getInstance().checkOpenDocumentByEventArgs(eventArgs);
      if (message) {
        $q.alertError(message);
      } else {
        eventArgs.set_isWindow(true);
        this.notify(window.EVENT_TYPE_ENTITY_LIST_ACTION_EXECUTING, eventArgs);
      }
    } else {
      eventArgs.set_context({ ctrlKey: e.ctrlKey || isMiddleClick });
      this.notify(window.EVENT_TYPE_ENTITY_LIST_ACTION_EXECUTING, eventArgs);
    }
  },

  _getIdLinkCode: function (value) {
    return !this._showIds ? '' : `<span class="idLink">(<a class="js" href="javascript:void(0)">${  $q.htmlEncode(value)  }</a>)</span>`;
  },

  _getValueStorage: function () {
    return this._listManagerComponent.getValueStorage();
  },

  _onCopyButtonClickHandler: function () {
    var storage = this._getValueStorage();
    storage.parentEntityId = this._parentEntityId;
    storage.entityTypeCode = this._entityTypeCode;
    storage.filter = this._filter;
    storage.entities = this.getSelectedEntities();
  },
  _onPasteButtonClickHandler: function () {
    var storage = this._getValueStorage();
    if (storage.entities) {
      if (storage.entityTypeCode != this._entityTypeCode) {
        $q.alertError($l.EntityDataList.typesDoesNotMatchMessage);
      } else if (storage.parentEntityId != this._parentEntityId) {
        $q.alertError($l.EntityDataList.parentsDoesNotMatchMessage);
      } else if (storage.filter != this._filter) {
        $q.alertError($l.EntityDataList.filtersDoesNotMatchMessage);
      } else {
        this._loadSelectedItems(storage.entities);
      }
    }
  },

  _isCountOverflow: function () {
    return false;
  },

  setFilter: function (filter) {
    this._filter = filter;
  },

  applyFilter: function (filter) {
    var result = this._initFilter;
    if (filter) {
      result = result && result.trim() ? `${result  } and ${  filter}` : filter;
    }

    if (result != this._filter) {
      this._filter = result;
      if (this.getListItemCount() > 0) {
        this.refreshList();
      }
    }
  },

  getFilter: function () {
    return this._filter;
  },

  dispose: function () {
    Quantumart.QP8.BackendEntityDataListBase.callBaseMethod(this, 'dispose');

    this._destroyPopupWindow();

    if (this._addNewButtonElement) {
      this._addNewButtonElement = null;
    }

    if (this._readButtonElement) {
      this._readButtonElement = null;
    }

    if (this._toolbarElement) {
      var $toolbar = $(this._toolbarElement);
      $toolbar
        .empty()
        .remove()
      ;

      $toolbar = null;
      this._toolbarElement = null;
    }

    this._destroyGroupCheckbox();
    this._destroyCountDiv();

    if (this._listWrapperElement) {
      $(this._listElement).unwrap(this._listWrapperElement);
      this._listWrapperElement = null;
    }

    if (this._listManagerComponent) {
      var listElementId = this._listElementId;
      if (!$q.isNullOrWhiteSpace(listElementId)) {
        this._listManagerComponent.removeList(listElementId);
      }

      this._listManagerComponent = null;
    }

    if (this._expandLinkElement) {
      $(this._expandLinkElement).off();
      this._expandLinkElement = null;
    }
    if (this._collapseLinkElement) {
      $(this._collapseLinkElement).off();
      this._collapseLinkElement = null;
    }

    this._listElement = null;
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEntityDataListBase.registerClass('Quantumart.QP8.BackendEntityDataListBase', Quantumart.QP8.Observable);
