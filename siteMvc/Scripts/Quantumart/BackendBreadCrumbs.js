window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK = 'OnBreadCrumbsItemClick';
window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK = 'OnBreadCrumbsItemCtrlClick';
window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CONTEXT_CLICK = 'OnBreadCrumbsContextMenuItemClick';

Quantumart.QP8.BackendBreadCrumbs = function (breadCrumbsElementId, options) {
  Quantumart.QP8.BackendBreadCrumbs.initializeBase(this);
  this._breadCrumbsElementId = breadCrumbsElementId;
  if ($q.isObject(options)) {
    if (options.breadCrumbsContainerElementId) {
      this._breadCrumbsContainerElementId = options.breadCrumbsContainerElementId;
    }

    if (options.documentHost) {
      this._documentHost = options.documentHost;
    }

    if (options.manager) {
      this._manager = options.manager;
    }

    if (options.contextMenuManager) {
      this._contextMenuManagerComponent = options.contextMenuManager;
    }
  }

  $q.bindProxies.call(this, [
    '_onBreadCrumbsItemClick',
    '_onGetBreadCrumbsListSuccess',
    '_onGetBreadCrumbsListError',

    '_onContextMenu',
    '_onContextMenuShowing',
    '_onContextMenuItemClicking',
    '_onContextMenuHidden'
  ]);
};

Quantumart.QP8.BackendBreadCrumbs.prototype = {
  _breadCrumbsElementId: '',
  _breadCrumbsElement: null,
  _breadCrumbsItemListElement: null,
  _breadCrumbsContainerElementId: '',
  _documentHost: null,
  _stopDeferredOperations: false,
  _addItemsCallback: null,
  _manager: null,

  ITEM_CLASS_NAME: 'item',
  ITEM_BUSY_CLASS_NAME: 'busy',

  ITEM_SELECTORS: 'li.item',
  ITEM_WITH_LINK_SELECTORS: 'li.item:has(a)',
  ITEM_LAST_WITH_LINK_SELECTORS: 'li.item:has(a):last',

  _onBreadCrumbsItemClickHandler: null,
  _onGetBreadCrumbsListSuccessHandler: null,
  _onGetBreadCrumbsListErrorHandler: null,

  _contextMenuManagerComponent: null,
  _onContextMenuHandler: null,
  _onContextMenuShowingHandler: null,
  _onContextMenuItemClickingHandler: null,
  _onContextMenuHiddenHandler: null,

  get_contextMenuManager: function () {
    return this._contextMenuManager;
  },
  set_contextMenuManager: function (value) {
    this._contextMenuManager = value;
  },
  get_manager: function () {
    return this._manager;
  },
  set_manager: function (value) {
    this._manager = value;
  },
  get_breadCrumbsElementId: function () {
    return this._breadCrumbsElementId;
  },
  set_breadCrumbsElementId: function (value) {
    this._breadCrumbsElementId = value;
  },
  get_breadCrumbsElement: function () {
    return this._breadCrumbsElement;
  },
  get_breadCrumbsContainerElementId: function () {
    return this._breadCrumbsContainerElementId;
  },
  set_breadCrumbsContainerElementId: function (value) {
    this._breadCrumbsContainerElementId = value;
  },

  initialize: function () {
    var $breadCrumbs = $(`#${  this._breadCrumbsElementId}`);
    var $breadCrumbsItemList = $breadCrumbs.find('ul:first');
    if (!$breadCrumbs.length) {
      $breadCrumbs = $('<div />', { id: this._breadCrumbsElementId, class: 'breadCrumbs', css: { display: 'none' } });
      $breadCrumbsItemList = $('<ul />');
      $breadCrumbs.append($breadCrumbsItemList);
      if (!$q.isNullOrWhiteSpace(this._breadCrumbsContainerElementId)) {
        $(`#${  this._breadCrumbsContainerElementId}`).append($breadCrumbs);
      } else {
        $('body:first').append($breadCrumbs);
      }

      this._breadCrumbsElement = $breadCrumbs.get(0);
      this._breadCrumbsItemListElement = $breadCrumbsItemList.get(0);
      this._attachBreadCrumbsEventHandlers();
    } else {
      this._breadCrumbsElement = $breadCrumbs.get(0);
      this._breadCrumbsItemListElement = $breadCrumbsItemList.get(0);
    }
  },

  _attachBreadCrumbsEventHandlers: function () {
    if (this._contextMenuManagerComponent != null) {
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onContextMenuShowingHandler);
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onContextMenuItemClickingHandler);
      this._contextMenuManagerComponent.attachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onContextMenuHiddenHandler);
    }

    $(this._breadCrumbsItemListElement)
      .on('click', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .on('mouseup', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .on(this._contextMenuManagerComponent.getContextMenuEventType(), this.ITEM_SELECTORS, this._onContextMenuHandler);
  },

  _detachBreadCrumbsHandlers: function () {
    if (this._contextMenuManagerComponent != null) {
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, this._onContextMenuShowingHandler);
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, this._onContextMenuItemClickingHandler);
      this._contextMenuManagerComponent.detachObserver(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, this._onContextMenuHiddenHandler);
    }

    $(this._breadCrumbsItemListElement)
      .off('click', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .off('mouseup', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .off(this._contextMenuManagerComponent.getContextMenuEventType(), this.ITEM_SELECTORS, this._onContextMenuHandler);
  },

  getItems: function () {
    return $(this._breadCrumbsItemListElement).find(this.ITEM_SELECTORS);
  },

  getLastItemButOne: function () {
    return $(this._breadCrumbsItemListElement).find(this.ITEM_LAST_WITH_LINK_SELECTORS);
  },

  getItem: function (item) {
    var $item = null;
    if ($q.isObject(item)) {
      return $q.toJQuery(item);
    } else if ($q.isString(item)) {
      $item = $(`li[code='${  item  }']`, this._breadCrumbsItemListElement);
      if ($item.length === 0) {
        $item = null;
      }

      return $item;
    }
  },

  getItemValue: function (itemElem) {
    var $item = this.getItem(itemElem);
    var itemValue = '';

    if (!$q.isNullOrEmpty($item)) {
      itemValue = $item.attr('code');
    } else {
      $q.alertError('Ошибка. Отсутствует значение.');
      return;
    }

    return itemValue;
  },

  getItemText: function (item) {
    var $item = this.getItem(item);
    var itemText = '';

    if (!$q.isNullOrEmpty($item)) {
      itemText = $('span.text', $item).text();
    }

    return itemText;
  },

  showBreadCrumbs: function () {
    $(this._breadCrumbsElement).show();
  },

  hideBreadCrumbs: function () {
    $(this._breadCrumbsElement).hide();
  },

  markBreadCrumbsAsBusy: function () {
    $(this._breadCrumbsItemListElement).addClass(this.ITEM_BUSY_CLASS_NAME);
  },

  unmarkBreadCrumbsAsBusy: function () {
    $(this._breadCrumbsItemListElement).removeClass(this.ITEM_BUSY_CLASS_NAME);
  },

  _isBusy: function () {
    $(this._breadCrumbsItemListElement).hasClass(this.ITEM_BUSY_CLASS_NAME);
  },

  addItemsToBreadCrumbs: function (callback) {
    var host = this._documentHost;
    this._addItemsCallback = callback;
    Quantumart.QP8.BackendBreadCrumbs.getBreadCrumbsList(
      host.get_entityTypeCode(),
      host.get_entityId(),
      host.get_parentEntityId(),
      host.get_actionCode(),
      this._onGetBreadCrumbsListSuccessHandler,
      this._onGetBreadCrumbsListErrorHandler
    );
  },

  _onGetBreadCrumbsListError: function (jqXHR, textStatus, errorThrown) {
    if (this._stopDeferredOperations) {
      return;
    }

    $q.processGenericAjaxError(jqXHR);
    $q.callFunction(this._addItemsCallback);
  },

  _onGetBreadCrumbsListSuccess: function (data, textStatus, jqXHR) {
    if (this._stopDeferredOperations) {
      return;
    }

    var items = data;
    if (!$q.isNull(items)) {
      var itemCount = items.length;
      if (itemCount > 0) {
        var $breadCrumbsItemList = $(this._breadCrumbsItemListElement);
        var itemsHtml = new $.telerik.stringBuilder();
        for (var itemIndex = itemCount - 1; itemIndex >= 0; itemIndex--) {
          var item = items[itemIndex];
          this._getItemHtml(itemsHtml, item);
        }

        $breadCrumbsItemList.html(itemsHtml.string());
        this._extendItemElements(items);
        $q.callFunction(this._addItemsCallback);
      } else {
        this.removeItemsFromBreadCrumbs(this._addItemsCallback);
      }
    }
  },

  removeItemsFromBreadCrumbs: function (callback) {
    var $breadCrumbsItemList = $(this._breadCrumbsItemListElement);
    $breadCrumbsItemList.empty();
    $q.callFunction(callback);
  },

  _getItemHtml: function (html, dataItem, isSelectedItem) {
    var itemCode = Quantumart.QP8.BackendBreadCrumbsManager.getInstance().generateItemCode(dataItem.Code, dataItem.ParentId, dataItem.Id);
    html.cat(`<li code="${  $q.htmlEncode(itemCode)  }" class="${  this.ITEM_CLASS_NAME  }"></li>\n`);
  },

  _extendItemElement: function (itemElem, dataItem, isSelectedItem) {
    var $item = this.getItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {
      var html = new $.telerik.stringBuilder();
      html
        .catIf('  <a href="javascript:void(0);">', !isSelectedItem)
        .cat('<span class="text"')
        .cat(" title='(")
        .cat(dataItem.Id)
        .cat(') ')
        .cat(dataItem.EntityTypeName)
        .cat(' \"')
        .cat($q.htmlEncode(dataItem.Title))
        .cat('\"')
        .cat("'")
        .cat('>')
        .cat(dataItem.EntityTypeName)
        .cat(' "')
        .cat($q.middleCutShort($q.htmlEncode(dataItem.Title), this._maxTitleLength))
        .cat('\"</span>')
        .catIf('</a>', !isSelectedItem);

      $item.html(html.string());
      $item.data('entity_type_code', dataItem.Code);
      $item.data('entity_id', dataItem.Id);
      $item.data('entity_name', dataItem.Title);
      $item.data('parent_entity_id', dataItem.ParentId);
      $item.data('action_code', dataItem.ActionCode);

      var contextMenuCode = dataItem.Code;
      if (!$q.isNullOrWhiteSpace(contextMenuCode) && this._contextMenuManagerComponent) {
        var contextMenu = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
        if ($q.isNullOrEmpty(contextMenu)) {
          contextMenu = this._contextMenuManagerComponent.createContextMenu(contextMenuCode, String.format('breadContextMenu_{0}', contextMenuCode), {
            targetElements: this._breadCrumbsElement,
            allowManualShowing: true
          });

          contextMenu.addMenuItemsToMenu(true);
        }
      }
    }
  },

  _extendItemElements: function (dataItems) {
    var breadCrumbsManager = Quantumart.QP8.BackendBreadCrumbsManager.getInstance();
    var count = dataItems.length;
    for (var index = 0; index < dataItems.length; index++) {
      var dataItem = dataItems[index];
      var entityTypeCode = dataItem.Code;
      var entityId = dataItem.Id;
      var parentEntityId = dataItem.ParentId;
      var itemCode = breadCrumbsManager.generateItemCode(entityTypeCode, parentEntityId, entityId);
      var $item = this.getItem(itemCode);

      this._extendItemElement($item, dataItem, index === 0);
    }
  },

  _onBreadCrumbsItemClick: function (e) {
    e.preventDefault();
    var isLeftClick = e.type === 'click' && (e.which === 1 || e.which === 0);
    var isMiddleClick = e.type === 'mouseup' && e.which === 2;
    if (!(isLeftClick || isMiddleClick)) {
      return false;
    }

    var eventArgs = this.getItemActionEventArgs(e.currentTarget);
    if (eventArgs) {
      if (e.ctrlKey || e.shiftKey || isMiddleClick) {
        eventArgs.set_context(Object.assign({
          ctrlKey: e.ctrlKey || isMiddleClick
        }, eventArgs.get_context()));
        this.notify(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK, eventArgs);
      } else {
        this.notify(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK, eventArgs);
      }
    }
  },

  getItemActionEventArgs: function (item, selectedCode) {
    var $item = this.getItem(item);
    var actionCode = selectedCode || $item.data('action_code');
    if (!$q.isNullOrWhiteSpace(actionCode)) {
      var action = $a.getBackendActionByCode(actionCode);
      if (action === null) {
        $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
      } else {
        var host = this._documentHost;
        if (host) {
          var params = new Quantumart.QP8.BackendActionParameters({
            entityTypeCode: $item.data('entity_type_code'),
            entityId: $item.data('entity_id'),
            entityName: $item.data('entity_name'),
            parentEntityId: $item.data('parent_entity_id')
          });

          params.correct(action);
          return $a.getEventArgsFromActionWithParams(action, params);
        }
      }
    }
  },

  _onContextMenu: function (e) {
    var $element = $(e.currentTarget);
    var contextMenuCode = $element.data('entity_type_code');
    if (!this._isBusy()) {
      if (contextMenuCode && this._contextMenuManagerComponent) {
        var contextMenuComponent = this._contextMenuManagerComponent.getContextMenu(contextMenuCode);
        if (!$q.isNullOrEmpty(contextMenuComponent)) {
          contextMenuComponent.showMenu(e, $element.get(0));
        }
      }
    }

    e.preventDefault();
  },

  _onContextMenuShowing: function (eventType, sender, args) {
    var menuComponent = args.get_menu();
    var $item = $(args.get_targetElement());
    if (menuComponent && $item.length) {
      menuComponent.tuneMenuItems($item.data('entity_id'));
    }
  },

  _onContextMenuItemClicking: function (eventType, sender, args) {
    var $menuItem = $(args.get_menuItem());
    if ($menuItem.length) {
      this._contextMenuActionCode = $menuItem.data('action_code');
    }
  },

  _onContextMenuHidden: function (eventType, sender, args) {
    var $item = $(args.get_targetElement());
    if (this._contextMenuActionCode) {
      var eventArgs = this.getItemActionEventArgs($item.get(0), this._contextMenuActionCode);
      this.notify(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CONTEXT_CLICK, eventArgs);
      this._contextMenuActionCode = '';
    }
  },

  dispose: function () {
    this._stopDeferredOperations = true;
    Quantumart.QP8.BackendBreadCrumbs.callBaseMethod(this, 'dispose');
    this._detachBreadCrumbsHandlers();

    if (this._breadCrumbsItemListElement) {
      var $breadCrumbsItemList = $(this._breadCrumbsItemListElement);
      $breadCrumbsItemList.empty().remove();
    }

    if (this._breadCrumbsElement) {
      var $breadCrumbs = $(this._breadCrumbsElement);
      $breadCrumbs.empty().remove();
    }

    if (this._contextMenuComponent) {
      this._contextMenuComponent.dispose();
    }

    $q.dispose.call(this, [
      '_manager',
      '_documentHost',
      '_breadCrumbsElement',
      '_breadCrumbsItemListElement',
      '_onBreadCrumbsItemClickHandler',
      '_onGetBreadCrumbsListErrorHandler',
      '_onGetBreadCrumbsListSuccessHandler',

      '_contextMenuManagerComponent',
      '_onContextMenuHandler',
      '_onContextMenuShowingHandler',
      '_onContextMenuItemClickingHandler',
      '_onContextMenuHiddenHandler'
    ]);
  }
};

Quantumart.QP8.BackendBreadCrumbs.getBreadCrumbsList = function (entityTypeCode, entityId, parentEntityId, actionCode, successHandler, errorHandler) {
  var actionUrl = `${window.CONTROLLER_URL_ENTITY_OBJECT  }GetBreadCrumbsList`;
  var params = {
    entityTypeCode: entityTypeCode,
    entityId: entityId,
    parentEntityId: parentEntityId,
    actionCode: actionCode
  };

  if ($q.isFunction(successHandler)) {
    $q.getJsonFromUrl('GET', actionUrl, params, false, false, successHandler, errorHandler);
  } else {
    var breadCrumbs = null;
    $q.getJsonFromUrl('GET', actionUrl, params, false, false, function (data, textStatus, jqXHR) {
      breadCrumbs = data;
    }, function (jqXHR, textStatus, errorThrown) {
      breadCrumbs = null;
      $q.processGenericAjaxError(jqXHR);
    });

    return breadCrumbs;
  }
};

Quantumart.QP8.BackendBreadCrumbs.registerClass('Quantumart.QP8.BackendBreadCrumbs', Quantumart.QP8.Observable);
