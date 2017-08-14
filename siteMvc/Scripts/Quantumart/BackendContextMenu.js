window.EVENT_TYPE_CONTEXT_MENU_SHOWING = 'OnContextMenuShowing';
window.EVENT_TYPE_CONTEXT_MENU_HIDING = 'OnContextMenuHiding';
window.EVENT_TYPE_CONTEXT_MENU_HIDDEN = 'OnContextMenuHidden';
window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING = 'OnContextMenuItemClicking';
window.EVENT_TYPE_CONTEXT_MENU_ITEM_HOVERING = 'OnContextMenuItemHoveringHandler';

Quantumart.QP8.BackendContextMenu = function (contextMenuCode, contextMenuElementId, options) {
  Quantumart.QP8.BackendContextMenu.initializeBase(this);

  this._contextMenuCode = contextMenuCode;
  this._contextMenuElementId = contextMenuElementId;
  if ($q.isObject(options)) {
    if (options.targetElements) {
      this._targetElements = options.targetElements;
    }

    if (options.zIndex) {
     this._zIndex = options.zIndex;
    }

    if (!$q.isNull(options.allowManualShowing)) {
      this._allowManualShowing = options.allowManualShowing;
    }

    this._isBindToExternal = $q.toBoolean(options.isBindToExternal, false);
  }

  this._onContextMenuTuneHandler = $.proxy(this._onContextMenuTune, this);
  this._onContextMenuHidingHandler = $.proxy(this._onContextMenuHiding, this);
  this._onContextMenuHiddenHandler = $.proxy(this._onContextMenuHidden, this);
  this._onContextMenuItemHoveringHandler = $.proxy(this._onContextMenuItemHovering, this);
  this._onContextMenuItemClickingHandler = $.proxy(this._onContextMenuItemClicking, this);
  this._onCustomActionChangedHandler = $.proxy(this._onCustomActionChanged, this);
};

Quantumart.QP8.BackendContextMenu.prototype = {
  _contextMenuCode: '',
  _contextMenuElementId: '',
  _contextMenuElement: null,
  _contextMenuComponent: null,
  _contextMenuContainerElementId: '',
  _targetElements: null,
  _allowManualShowing: false,
  _contextMenuManagerComponent: null,
  _ctorOptions: null,

  _onContextMenuTuneHandler: null,
  _onContextMenuHidingHandler: null,
  _onContextMenuHiddenHandler: null,
  _onContextMenuItemHoveringHandler: null,
  _onContextMenuItemClickingHandler: null,
  _onCustomActionChangedHandler: null,
  _hideRefreshMenuItem: false,
  _isBindToExternal: false,
  _zIndex: 0,

  get_contextMenuCode: function () {
    return this._contextMenuCode;
  },

  set_contextMenuCode: function (value) {
    this._contextMenuCode = value;
  },

  get_contextMenuElementId: function () {
    return this._contextMenuElementId;
  },

  set_contextMenuElementId: function (value) {
    this._contextMenuElementId = value;
  },

  get_contextMenuElement: function () {
    return this._contextMenuElement;
  },

  get_contextMenuComponent: function () {
    return this._contextMenuComponent;
  },

  set_targetElements: function (value) {
    this._targetElements = value;
  },

  get_targetElements: function () {
    return this._targetElements;
  },

  set_allowManualShowing: function (value) {
    this._allowManualShowing = value;
  },

  get_allowManualShowing: function () {
    return this._allowManualShowing;
  },

  get_contextMenuManager: function () {
    return this._contextMenuManagerComponent;
  },

  set_contextMenuManager: function (value) {
    this._contextMenuManagerComponent = value;
  },

  initialize: function () {
    if (!this._contextMenuElementId) {
      this._contextMenuElementId = this._contextMenuCode;
    }

    var $menu = $('<ul />', {
      id: this._contextMenuElementId,
      class: 'contextMenu'
    });

    if (this._zIndex) {
      $menu.css('z-index', this._zIndex + 10);
    }

    if (!$q.isNullOrWhiteSpace(this._contextMenuContainerElementId)) {
      $('#' + this._contextMenuContainerElementId).append($menu);
    } else {
      $('body:first').append($menu);
    }

    this._contextMenuElement = $menu.get(0);
    var contextMenuComponentName = this._getMenuComponentName();
    var contextMenuComponent = $(this._targetElements).jeegoocontext({
      menuElementId: this._contextMenuElementId,
      menuClass: 'contextMenu',
      allowManualShowing: this._allowManualShowing,
      onTune: this._onContextMenuTuneHandler,
      onHide: this._onContextMenuHidingHandler,
      onHid: this._onContextMenuHiddenHandler,
      onHover: this._onContextMenuItemHoveringHandler,
      onSelect: this._onContextMenuItemClickingHandler
    }).data(contextMenuComponentName);

    Quantumart.QP8.BackendContextMenuManager.getInstance().attachObserver(window.EVENT_TYPE_CUSTOM_ACTION_CHANGED, this._onCustomActionChangedHandler);
    this._contextMenuComponent = contextMenuComponent;
  },

  _getMenuComponentName: function () {
    return 'jeegoocontext_' + this._contextMenuElementId;
  },

  showMenu: function (e, targetElem) {
    if (this._contextMenuComponent._menuElement.childNodes.length > 0) {
      this._contextMenuComponent.show(e, targetElem);
    }
  },

  showMenuAt: function (e, targetElem, x, y) {
    if (this._contextMenuComponent._menuElement.childNodes.length > 0) {
      this._contextMenuComponent.showAt(e, targetElem, x, y);
    }
  },

  hideMenu: function (e) {
    this._contextMenuComponent.hide(e);
  },

  refresh: function (hideRefreshMenuItem, successHandler, errorHandler) {
    var cacheKey = Quantumart.QP8.BackendContextMenu.getCacheKey(this._contextMenuCode, true, this._isBindToExternal);
    var contextMenuCachedData = Quantumart.QP8.Cache.removeItem(cacheKey);
    this.addMenuItemsToMenu(hideRefreshMenuItem, successHandler, errorHandler);
  },

  addMenuItemsToMenu: function (hideRefreshMenuItem, successHandler, errorHandler) {
    var self = this;
    this._hideRefreshMenuItem = hideRefreshMenuItem;
    var $menu = $(this._contextMenuElement);
    $menu.empty();

    Quantumart.QP8.BackendContextMenu.getContextMenuByCode(this._contextMenuCode, true, this._isBindToExternal, function (data) {
      var menu = data;
      if (menu != null) {
        var menuItems = menu.Items;
        var menuItemCount = 0;
        if (menuItems != null) {
          menuItems = $.grep(menuItems, function (menuItem) {
            return !hideRefreshMenuItem || menuItem.ActionTypeCode != window.ACTION_TYPE_CODE_REFRESH;
          });

          menuItemCount = menuItems.length;
        }

        if (menuItemCount > 0) {
          var menuItemsHtml = new $.telerik.stringBuilder();
          for (var menuItemIndex = 0; menuItemIndex < menuItemCount; menuItemIndex++) {
            var menuItem = menuItems[menuItemIndex];
            self._getMenuItemHtml(menuItemsHtml, menuItem);
            if (menuItem.BottomSeparator) {
              self._getSeparatorHtml(menuItemsHtml);
            }
          }

          $menu.html(menuItemsHtml.string());
          self._extendMenuItemElements(menuItems);
        }

        $q.clearArray(menuItems);
      }

      $q.callFunction(successHandler);
    }, function (jqXHR) {
      $q.processGenericAjaxError(jqXHR);
      $q.callFunction(errorHandler);
    });
  },

  tuneMenuItems: function (entityId, parentEntityId, callback) {
    var self = this;
    var params = {
      menuCode: this._contextMenuCode,
      entityId: entityId,
      parentEntityId: parentEntityId || 0
    };

    if (this._isBindToExternal === true) {
      params = Object.assign({}, params, { boundToExternal: true });
    }

    $q.getJsonFromUrl('GET', window.CONTROLLER_URL_CONTEXT_MENU + 'GetStatusesList', params, false, false, function (data) {
        var statuses = data;
        if (statuses) {
          var statusCount = statuses.length;
          if (statusCount > 0) {
            for (var statusIndex = 0; statusIndex < statusCount; statusIndex++) {
              var status = statuses[statusIndex];
              var $menuItem = self.getMenuItem(status.Code);
              if (!$q.isNullOrEmpty($menuItem)) {
                self.setVisibleState($menuItem, status.Visible);
              }
            }
          }

          $q.clearArray(statuses);
        }

        self._tuneMenuSeparators();
        $q.callFunction(callback);
      }, function (jqXHR) {
        $q.processGenericAjaxError(jqXHR);
        $q.callFunction(callback);
      }
    );
  },

  _tuneMenuSeparators: function () {
    $('li.separator', this._contextMenuComponent._menuElement).each(function () {
      var $separator = $(this);
      var toHide = true;
      $separator.nextAll('li.item, li.separator').each(function () {
        var $ci = $(this);

        if ($ci.hasClass('separator') == true) {
          return false;
        }

        if ($ci.hasClass('item') && $ci.css('display') != 'none') {
          toHide = false;
          return false;
        }
      });

      if (toHide === true) {
        $separator.hide();
      } else {
        $separator.show();
      }
    });

    var $firstVisibleSeparator = $('li.separator', this._contextMenuComponent._menuElement)
    .filter(function () {
      return $(this).css('display') != 'none';
    }).first();

    var toHideFirstVisibleSeparator = $firstVisibleSeparator.prevAll('li.item').filter(function () {
      return $(this).css('display') != 'none';
    }).length == 0;

    if (toHideFirstVisibleSeparator) {
      $firstVisibleSeparator.hide();
    }
  },

  _getMenuItemHtml: function (html, dataItem) {
    html
      .cat('<li code="' + $q.htmlEncode(dataItem.ActionCode) + '" class="item">\n')
      .cat('  <div class="outerWrapper">\n')
      .cat('      <div class="innerWrapper">\n')
      .cat('          <span class="icon"')
      .catIf(' style="background-image: url(\'' + window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + dataItem.Icon + '\')"',
        !$q.isNullOrWhiteSpace(dataItem.Icon) && dataItem.Icon.left(7).toLowerCase() !== 'http://')
      .catIf(' style="background-image: url(\'' + dataItem.Icon + '\')"',
        !$q.isNullOrWhiteSpace(dataItem.Icon) && dataItem.Icon.left(7).toLowerCase() === 'http://')
      .cat('>')
      .cat('<img src="' + window.COMMON_IMAGE_FOLDER_URL_ROOT + '0.gif" width="16px" height="16px" />')
      .cat('</span>\n')
      .cat('          <span class="text">' + $q.htmlEncode(dataItem.Name) + '</span>\n')
      .cat('      </div>\n')
      .cat('  </div>\n')
      .cat('</li>\n');
  },

  _getSeparatorHtml: function (html) {
    html.cat('<li class="separator"></li>\n');
  },

  _extendMenuItemElement: function (menuItemElem, menuItem) {
    var $menuItem = this.getMenuItem(menuItemElem);
    if (!$q.isNullOrEmpty($menuItem)) {
      $menuItem.data('action_code', menuItem.ActionCode);
      $menuItem.data('action_type_code', menuItem.ActionTypeCode);
    }
  },

  _extendMenuItemElements: function (menuItems) {
    var self = this;
    $.each(menuItems, function (index, menuItem) {
        var $menuItem = self.getMenuItem(menuItem.ActionCode);
        self._extendMenuItemElement($menuItem, menuItem);
      }
    );
  },

  getMenuItemCount: function () {
    return $('> LI', this._contextMenuElement).length;
  },

  getMenuItem: function (menuItem) {
    var $menuItem = null;

    if ($q.isObject(menuItem)) {
      return $q.toJQuery(menuItem);
    } else if ($q.isString(menuItem)) {
      $menuItem = $("LI[code='" + menuItem + "']", this._contextMenuElement);
      if ($menuItem.length == 0) {
        $menuItem = null;
      }

      return $menuItem;
    }
  },

  getMenuItemValue: function (menuItemElem) {
    var $menuItem = this.getMenuItem(menuItemElem);
    var menuItemValue = '';

    if (!$q.isNullOrEmpty($menuItem)) {
      menuItemValue = $menuItem.attr('code');
    } else {
      $q.alertFail($l.ContextMenu.menuItemNotSpecified);
      return;
    }

    return menuItemValue;
  },

  getMenuItemText: function (menuItem) {
    var $menuItem = this.getMenuItem(menuItem);
    var menuItemText = '';

    if (!$q.isNullOrEmpty($menuItem)) {
      menuItemText = $('SPAN.text', $menuItem).text();
    }

    return menuItemText;
  },

  setVisibleState: function (menuItem, state) {
    var $menuItem = this.getMenuItem(menuItem);
    if (state) {
      $menuItem.show();
    } else {
      $menuItem.hide();
    }
  },

  setEnableState: function (menuItem, state) {
    var $menuItem = this.getMenuItem(menuItem);
    if (state) {
      $menuItem.removeClass('disabled');
    } else {
      $menuItem.addClass('disabled');
    }
  },

  getContextMenuEventType: function () {
    return $.fn.jeegoocontext.getContextMenuEventType();
  },

  _onContextMenuTune: function (e, context) {
    var eventArgs = new Quantumart.QP8.BackendContextMenuEventArgs();
    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, eventArgs);
    }

    if ($('LI.item', this._contextMenuElement).filter(function () {
      return $(this).css('display') != 'none';
    }).length == 0) {
      return false;
    }
  },

  _onCustomActionChanged: function () {
    this.refresh(this._hideRefreshMenuItem);
  },

  _onContextMenuHiding: function (e, context) {
    var eventArgs = new Quantumart.QP8.BackendContextMenuEventArgs();

    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_HIDING, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_HIDING, eventArgs);
    }
  },

  _onContextMenuHidden: function (e, context) {
    var eventArgs = new Quantumart.QP8.BackendContextMenuEventArgs();

    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, eventArgs);
    }
  },

  _onContextMenuItemHovering: function (e, context) {
    var eventArgs = new Quantumart.QP8.BackendContextMenuItemEventArgs();

    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);
    eventArgs.set_menuItem(e.currentTarget);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_ITEM_HOVERING, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_ITEM_HOVERING, eventArgs);
    }
  },

  _onContextMenuItemClicking: function (e, context) {
    var eventArgs = new Quantumart.QP8.BackendContextMenuItemEventArgs();

    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);
    eventArgs.set_menuItem(e.currentTarget);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, eventArgs);
    }
  },

  dispose: function () {
    Quantumart.QP8.BackendContextMenu.callBaseMethod(this, 'dispose');

    if (this._contextMenuManagerComponent) {
      var contextMenuCode = this._contextMenuCode;

      if (!$q.isNullOrWhiteSpace(contextMenuCode)) {
        this._contextMenuManagerComponent.removeContextMenu(contextMenuCode);
      }

      this._contextMenuManagerComponent = null;
    }

    if (this._contextMenuComponent) {
      this._contextMenuComponent.dispose();
      this._contextMenuComponent = null;
    }

    if (this._targetElements) {
      var contextMenuComponentName = this._getMenuComponentName();

      $(this._targetElements).each(function (index, targetElem) {
        $(targetElem).removeData(contextMenuComponentName);
      });

      this._targetElements = null;
    }

    if (this._contextMenuElement) {
      var $contextMenu = $(this._contextMenuElement);

      $contextMenu.empty().remove();
      $contextMenu = null;
      this._contextMenuElement = null;
    }

    Quantumart.QP8.BackendContextMenuManager.getInstance().detachObserver(window.EVENT_TYPE_CUSTOM_ACTION_CHANGED, this._onCustomActionChangedHandler);

    this._onContextMenuTuneHandler = null;
    this._onContextMenuHidingHandler = null;
    this._onContextMenuHiddenHandler = null;
    this._onContextMenuItemHoveringHandler = null;
    this._onContextMenuItemClickingHandler = null;
    this._onCustomActionChangedHandler = null;

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendContextMenu.getCacheKey = function (menuCode, loadItems, isBindToExternal) {
  return 'contextMenuCachedData' + menuCode + loadItems + isBindToExternal;
};

Quantumart.QP8.BackendContextMenu.getContextMenuByCode = function (menuCode, loadItems, isBindToExternal, successHandler, errorHandler) {
  var cacheKey = Quantumart.QP8.BackendContextMenu.getCacheKey(menuCode, loadItems, isBindToExternal);
  var contextMenuCachedData = Quantumart.QP8.Cache.getItem(cacheKey);

  if (!contextMenuCachedData) {
    var actionUrl = window.CONTROLLER_URL_CONTEXT_MENU + 'GetByCode';
    var params = { menuCode: menuCode, loadItems: loadItems };

    if (isBindToExternal === true) {
      params = Object.assign({}, params, { boundToExternal: true });
    }

    if ($q.isFunction(successHandler)) {
      $q.getJsonFromUrl('GET', actionUrl, params, false, false, function (data, textStatus, jqXHR) {
        Quantumart.QP8.Cache.addItem(cacheKey, data);
        successHandler(data, textStatus, jqXHR);
      }, errorHandler);
    } else {
      var menu = null;

      $q.getJsonFromUrl('GET', actionUrl, params, false, false, function (data) {
        Quantumart.QP8.Cache.addItem(cacheKey, data);
        menu = data;
      }, function (jqXHR) {
        menu = null;
        $q.processGenericAjaxError(jqXHR);
      });

      return menu;
    }
  } else if ($q.isFunction(successHandler)) {
      successHandler(contextMenuCachedData);
    } else {
      return contextMenuCachedData;
    }
};

Quantumart.QP8.BackendContextMenu.registerClass('Quantumart.QP8.BackendContextMenu', Quantumart.QP8.Observable);

Quantumart.QP8.BackendContextMenuEventArgs = function () {
  Quantumart.QP8.BackendContextMenuEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendContextMenuEventArgs.prototype = {
  _menuComponent: null,
  _targetElement: null,

  get_menu: function () {
    return this._menuComponent;
  },

  set_menu: function (value) {
    this._menuComponent = value;
  },

  get_targetElement: function () {
    return this._targetElement;
  },

  set_targetElement: function (value) {
    this._targetElement = value;
  }
};

Quantumart.QP8.BackendContextMenuEventArgs.registerClass('Quantumart.QP8.BackendContextMenuEventArgs', Sys.EventArgs);

Quantumart.QP8.BackendContextMenuItemEventArgs = function () {
  Quantumart.QP8.BackendContextMenuItemEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendContextMenuItemEventArgs.prototype = {
  _menuItemElement: null,
  get_menuItem: function () {
    return this._menuItemElement;
  },

  set_menuItem: function (value) {
    this._menuItemElement = value;
  }
};

Quantumart.QP8.BackendContextMenuItemEventArgs.registerClass('Quantumart.QP8.BackendContextMenuItemEventArgs', Quantumart.QP8.BackendContextMenuEventArgs);
