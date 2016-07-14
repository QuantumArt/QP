var EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK = "OnBreadCrumbsItemClick";
var EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK = "OnBreadCrumbsItemCtrlClick";

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
  }

  $q.bindProxies.call(this, [
    '_onBreadCrumbsItemClick',
    '_onContextMenuShow',
    '_onGetBreadCrumbsListSuccess',
    '_onGetBreadCrumbsListError'
  ]);
};

Quantumart.QP8.BackendBreadCrumbs.prototype = {
  _breadCrumbsElementId: '', // клиентский идентификатор хлебных крошек
  _breadCrumbsElement: null, // DOM-элемент, образующий хлебные крошки
  _breadCrumbsItemListElement: null, // DOM-элемент, образующий список элементов хлебных крошек
  _breadCrumbsContainerElementId: '', // клиентский идентификатор контейнера, в котором располагаются хлебные крошки
  _documentHost: null, // документ, для которого создаются хлебные крошки
  _stopDeferredOperations: false, // признак, отвечающий за остановку всех отложенных операций
  _addItemsCallback: null,
  _maxTitleLength: 60,
  _contextMenuComponent: null,

  ITEM_CLASS_NAME: 'item',
  ITEM_BUSY_CLASS_NAME: 'busy',

  ITEM_SELECTORS: 'li.item',
  ITEM_WITH_LINK_SELECTORS: 'li.item:has(a)',
  ITEM_LAST_WITH_LINK_SELECTORS: 'li.item:has(a):last',

  _onBreadCrumbsItemClickHandler: null,
  _onContextMenuShowHandler: null,
  _onGetBreadCrumbsListSuccessHandler: null,
  _onGetBreadCrumbsListErrorHandler: null,

  _manager: null,
  get_manager: function () { return this._manager; },
  set_manager: function (value) { this._manager = value; },
  get_breadCrumbsElementId: function () { return this._breadCrumbsElementId; },
  set_breadCrumbsElementId: function (value) { this._breadCrumbsElementId = value; },
  get_breadCrumbsElement: function () { return this._breadCrumbsElement; },
  get_breadCrumbsContainerElementId: function () { return this._breadCrumbsContainerElementId; },
  set_breadCrumbsContainerElementId: function (value) { this._breadCrumbsContainerElementId = value; },

  initialize: function () {
    var $breadCrumbs = $('#' + this._breadCrumbsElementId);
    var $breadCrumbsItemList = $breadCrumbs.find('ul:first');
    if (!$breadCrumbsItemList.length) {
      $breadCrumbsItemList = $('<ul />');
    }

    if (!$breadCrumbs.length) {
      $breadCrumbs = $('<div />', { 'id': this._breadCrumbsElementId, 'class': 'breadCrumbs', 'css': { 'display': 'none'} });
      $breadCrumbs.append($breadCrumbsItemList);
      if (!$q.isNullOrWhiteSpace(this._breadCrumbsContainerElementId)) {
        $('#' + this._breadCrumbsContainerElementId).append($breadCrumbs);
      } else {
        $('body:first').append($breadCrumbs);
      }

      setTimeout(this._attachBreadCrumbsEventHandlers.bind(this), 0);
    }

    this._breadCrumbsElement = $breadCrumbs.get(0);
    this._breadCrumbsItemListElement = $breadCrumbsItemList.get(0);
    this._createContextMenu();
  },

  _createContextMenu: function() {
    console.log('xxx');
  },

  _attachBreadCrumbsEventHandlers: function() {
    $(this._breadCrumbsItemListElement)
      .on('click', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .on('mouseup', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .on($.fn['jeegoocontext'].getContextMenuEventType(), this.TAB_CLICKABLE_SELECTORS, this._onContextMenuShow);
  },

  _detachBreadCrumbsHandlers: function () {
    $(this._breadCrumbsItemListElement)
      .off('click', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .off('mouseup', this.ITEM_WITH_LINK_SELECTORS, this._onBreadCrumbsItemClickHandler)
      .off($.fn['jeegoocontext'].getContextMenuEventType(), this.TAB_CLICKABLE_SELECTORS, this._onContextMenuShow);
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
      $item = $("LI[code='" + item + "']", this._breadCrumbsItemListElement);
      if ($item.length === 0) {
        $item = null;
      }

      return $item;
    }
  },

  getItemValue: function (itemElem) {
    var $item = this.getItem(itemElem);
    var itemValue = "";

    if (!$q.isNullOrEmpty($item)) {
      itemValue = $item.attr('code');
    } else {
      window.alert('Ошибка!');
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

  addItemsToBreadCrumbs: function (callback) {
    var host = this._documentHost;
    this._addItemsCallback = callback;
    Quantumart.QP8.BackendBreadCrumbs.getBreadCrumbsList(
      host.get_entityTypeCode(), host.get_entityId(), host.get_parentEntityId(), host.get_actionCode(),
      this._onGetBreadCrumbsListSuccessHandler, this._onGetBreadCrumbsListErrorHandler
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
    html.cat('<li code="' + $q.htmlEncode(itemCode) + '" class="' + this.ITEM_CLASS_NAME + '"></li>\n');
  },

  _extendItemElement: function (itemElem, dataItem, isSelectedItem) {
    var $item = this.getItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {

      var html = new $.telerik.stringBuilder();
      html
      .catIf('  <a href="javascript:void(0);">', !isSelectedItem)
      .cat('<span class="text"')
        .cat(" title='(")
        .cat(dataItem.Id).cat(') ')
        .cat(dataItem.EntityTypeName)
        .cat(' \"').cat($q.htmlEncode(dataItem.Title))
        .cat('\"')
        .cat("'")
      .cat('>')
      .cat(dataItem.EntityTypeName)
      .cat(" \"")
      .cat($q.middleCutShort($q.htmlEncode(dataItem.Title), this._maxTitleLength))
      .cat('\"</span>')
      .catIf('</a>', !isSelectedItem)
      $item.html(html.string());

      $item.data('entity_type_code', dataItem.Code);
      $item.data('entity_id', dataItem.Id);
      $item.data('entity_name', dataItem.Title);
      $item.data('parent_entity_id', dataItem.ParentId);
      $item.data('action_code', dataItem.ActionCode);
      $item.data('action_code', dataItem.ActionCode);
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

      this._extendItemElement($item, dataItem, index == 0);
    }
  },

  _onBreadCrumbsItemClick: function (e) {
    this._contextMenuComponent.hide(e);
    e.preventDefault();

    var isLeftClick = e.type === 'click' && (e.which == 1 || e.which == 0);
    var isMiddleClick = e.type === 'mouseup' && e.which == 2;
    if (!(isLeftClick || isMiddleClick)) {
      return false;
    }

    var eventArgs = this.getItemActionEventArgs(e.currentTarget);
    if (eventArgs) {
      if (e.ctrlKey || e.shiftKey || isMiddleClick) {
        eventArgs.set_context($.extend({
          'ctrlKey': e.ctrlKey || isMiddleClick
        }, eventArgs.get_context()));
        this.notify(EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK, eventArgs);
      } else {
        this.notify(EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK, eventArgs);
      }
    }
  },

  _onContextMenuShow: function(e) {
    this._contextMenuComponent.show(e, e.currentTarget);
    e.preventDefault();
  },

  getItemActionEventArgs: function (item) {
    var $item = this.getItem(item);
    var actionCode = $item.data('action_code');
    if (!$q.isNullOrWhiteSpace(actionCode)) {
      var action = $a.getBackendActionByCode(actionCode);
      if (action == null) {
        alert($l.Common.ajaxDataReceivingErrorMessage);
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
          var eventArgs = $a.getEventArgsFromActionWithParams(action, params);
          return eventArgs;
        }
      }
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
      '_contextMenuComponent',
      '_onContextMenuShowHandler',
      '_breadCrumbsItemListElement',
      '_onBreadCrumbsItemClickHandler',
      '_onGetBreadCrumbsListErrorHandler',
      '_onGetBreadCrumbsListSuccessHandler'
    ]);
  }
};

Quantumart.QP8.BackendBreadCrumbs.getBreadCrumbsList = function (entityTypeCode, entityId, parentEntityId, actionCode, successHandler, errorHandler) {
  var actionUrl = CONTROLLER_URL_ENTITY_OBJECT + 'GetBreadCrumbsList';
  var params = {
    'entityTypeCode': entityTypeCode,
    'entityId': entityId,
    'parentEntityId': parentEntityId,
    'actionCode': actionCode
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
