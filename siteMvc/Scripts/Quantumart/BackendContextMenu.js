/* eslint max-lines: 'off' */
import { BackendContextMenuManager } from './Managers/BackendContextMenuManager';
import { GlobalCache } from './Cache';
import { Observable } from './Common/Observable';
import { $q } from './Utils';


window.EVENT_TYPE_CONTEXT_MENU_SHOWING = 'OnContextMenuShowing';
window.EVENT_TYPE_CONTEXT_MENU_HIDING = 'OnContextMenuHiding';
window.EVENT_TYPE_CONTEXT_MENU_HIDDEN = 'OnContextMenuHidden';
window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING = 'OnContextMenuItemClicking';
window.EVENT_TYPE_CONTEXT_MENU_ITEM_HOVERING = 'OnContextMenuItemHoveringHandler';

export class BackendContextMenu extends Observable {
  constructor(contextMenuCode, contextMenuElementId, options) {
    super();

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
  }

  _contextMenuCode = '';
  _contextMenuElementId = '';
  _contextMenuElement = null;
  _contextMenuComponent = null;
  _contextMenuContainerElementId = '';
  _targetElements = null;
  _allowManualShowing = false;
  _contextMenuManagerComponent = null;
  _ctorOptions = null;

  _onContextMenuTuneHandler = null;
  _onContextMenuHidingHandler = null;
  _onContextMenuHiddenHandler = null;
  _onContextMenuItemHoveringHandler = null;
  _onContextMenuItemClickingHandler = null;
  _onCustomActionChangedHandler = null;
  _hideRefreshMenuItem = false;
  _isBindToExternal = false;
  _zIndex = 0;

  get_contextMenuCode() { // eslint-disable-line camelcase
    return this._contextMenuCode;
  }

  set_contextMenuCode(value) { // eslint-disable-line camelcase
    this._contextMenuCode = value;
  }

  get_contextMenuElementId() { // eslint-disable-line camelcase
    return this._contextMenuElementId;
  }

  set_contextMenuElementId(value) { // eslint-disable-line camelcase
    this._contextMenuElementId = value;
  }

  get_contextMenuElement() { // eslint-disable-line camelcase
    return this._contextMenuElement;
  }

  get_contextMenuComponent() { // eslint-disable-line camelcase
    return this._contextMenuComponent;
  }

  set_targetElements(value) { // eslint-disable-line camelcase
    this._targetElements = value;
  }

  get_targetElements() { // eslint-disable-line camelcase
    return this._targetElements;
  }

  set_allowManualShowing(value) { // eslint-disable-line camelcase
    this._allowManualShowing = value;
  }

  get_allowManualShowing() { // eslint-disable-line camelcase
    return this._allowManualShowing;
  }

  get_contextMenuManager() { // eslint-disable-line camelcase
    return this._contextMenuManagerComponent;
  }

  set_contextMenuManager(value) { // eslint-disable-line camelcase
    this._contextMenuManagerComponent = value;
  }

  initialize() {
    if (!this._contextMenuElementId) {
      this._contextMenuElementId = this._contextMenuCode;
    }

    const $menu = $('<ul />', {
      id: this._contextMenuElementId,
      class: 'contextMenu'
    });

    if (this._zIndex) {
      $menu.css('z-index', this._zIndex + 10);
    }

    if ($q.isNullOrWhiteSpace(this._contextMenuContainerElementId)) {
      $('body:first').append($menu);
    } else {
      $(`#${this._contextMenuContainerElementId}`).append($menu);
    }

    this._contextMenuElement = $menu.get(0);
    const contextMenuComponentName = this._getMenuComponentName();
    const contextMenuComponent = $(this._targetElements).jeegoocontext({
      menuElementId: this._contextMenuElementId,
      menuClass: 'contextMenu',
      allowManualShowing: this._allowManualShowing,
      onTune: this._onContextMenuTuneHandler,
      onHide: this._onContextMenuHidingHandler,
      onHid: this._onContextMenuHiddenHandler,
      onHover: this._onContextMenuItemHoveringHandler,
      onSelect: this._onContextMenuItemClickingHandler
    }).data(contextMenuComponentName);

    BackendContextMenuManager.getInstance().attachObserver(
      window.EVENT_TYPE_CUSTOM_ACTION_CHANGED, this._onCustomActionChangedHandler
    );
    this._contextMenuComponent = contextMenuComponent;
  }

  _getMenuComponentName() {
    return `jeegoocontext_${this._contextMenuElementId}`;
  }

  showMenu(e, targetElem) {
    if (this._contextMenuComponent._menuElement.childNodes.length > 0) {
      this._contextMenuComponent.show(e, targetElem);
    }
  }

  showMenuAt(e, targetElem, xc, yc) {
    if (this._contextMenuComponent._menuElement.childNodes.length > 0) {
      this._contextMenuComponent.showAt(e, targetElem, xc, yc);
    }
  }

  hideMenu(e) {
    this._contextMenuComponent.hide(e);
  }

  refresh(hideRefreshMenuItem, successHandler, errorHandler) {
    const cacheKey = BackendContextMenu.getCacheKey(this._contextMenuCode, true, this._isBindToExternal);
    GlobalCache.removeItem(cacheKey);
    this.addMenuItemsToMenu(hideRefreshMenuItem, successHandler, errorHandler);
  }

  addMenuItemsToMenu(hideRefreshMenuItem, successHandler, errorHandler) {
    const that = this;
    this._hideRefreshMenuItem = hideRefreshMenuItem;
    const $menu = $(this._contextMenuElement);
    $menu.empty();

    BackendContextMenu.getContextMenuByCode(
      this._contextMenuCode, true, this._isBindToExternal, data => {
        const menu = data;
        if (!$q.isNull(menu)) {
          let menuItems = menu.Items;
          let menuItemCount = 0;
          if (!$q.isNull(menuItems)) {
            menuItems = $.grep(menuItems, menuItem => !hideRefreshMenuItem
            || menuItem.ActionTypeCode !== window.ACTION_TYPE_CODE_REFRESH);

            menuItemCount = menuItems.length;
          }

          if (menuItemCount > 0) {
            const menuItemsHtml = new $.telerik.stringBuilder();
            for (let menuItemIndex = 0; menuItemIndex < menuItemCount; menuItemIndex++) {
              const menuItem = menuItems[menuItemIndex];
              that._getMenuItemHtml(menuItemsHtml, menuItem);
              if (menuItem.BottomSeparator) {
                that._getSeparatorHtml(menuItemsHtml);
              }
            }

            $menu.html(menuItemsHtml.string());
            that._extendMenuItemElements(menuItems);
          }

          $q.clearArray(menuItems);
        }

        $q.callFunction(successHandler);
      }, jqXHR => {
        $q.processGenericAjaxError(jqXHR);
        $q.callFunction(errorHandler);
      });
  }

  tuneMenuItems(entityId, parentEntityId, callback) {
    const that = this;
    let params = {
      menuCode: this._contextMenuCode,
      entityId,
      parentEntityId: parentEntityId || 0
    };

    if (this._isBindToExternal) {
      params = Object.assign({}, params, { boundToExternal: true });
    }

    $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_CONTEXT_MENU}GetStatusesList`, params, false, false, data => {
      const statuses = data;
      if (statuses) {
        const statusCount = statuses.length;
        if (statusCount > 0) {
          for (let statusIndex = 0; statusIndex < statusCount; statusIndex++) {
            const currentStatus = statuses[statusIndex];
            const $menuItem = that.getMenuItem(currentStatus.Code);
            if (!$q.isNullOrEmpty($menuItem)) {
              that.setVisibleState($menuItem, currentStatus.Visible);
            }
          }
        }

        $q.clearArray(statuses);
      }

      that._tuneMenuSeparators();
      $q.callFunction(callback);
    }, jqXHR => {
      $q.processGenericAjaxError(jqXHR);
      $q.callFunction(callback);
    }
    );
  }

  _tuneMenuSeparators() {
    $('li.separator', this._contextMenuComponent._menuElement).each(function () {
      const $separator = $(this);
      let toHide = true;
      $separator.nextAll('li.item, li.separator').each(function () {
        const $ci = $(this);

        if ($ci.hasClass('separator')) {
          return false;
        }

        if ($ci.hasClass('item') && $ci.css('display') !== 'none') {
          toHide = false;
          return false;
        }
        return true;
      });

      if (toHide) {
        $separator.hide();
      } else {
        $separator.show();
      }
    });

    const $firstVisibleSeparator = $('li.separator', this._contextMenuComponent._menuElement)
      .filter(function () {
        return $(this).css('display') !== 'none';
      }).first();

    const toHideFirstVisibleSeparator = $firstVisibleSeparator.prevAll('li.item').filter(function () {
      return $(this).css('display') !== 'none';
    }).length === 0;

    if (toHideFirstVisibleSeparator) {
      $firstVisibleSeparator.hide();
    }
  }

  _getMenuItemHtml(html, dataItem) {
    html
      .cat(`<li code="${$q.htmlEncode(dataItem.ActionCode)}" class="item">\n`)
      .cat('  <div class="outerWrapper">\n')
      .cat('      <div class="innerWrapper">\n')
      .cat('          <span class="icon"')
      .catIf(` style="background-image: url('${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS}${dataItem.Icon}')"`,
        !$q.isNullOrWhiteSpace(dataItem.Icon) && dataItem.Icon.left(7).toLowerCase() !== 'http://')
      .catIf(` style="background-image: url('${dataItem.Icon}')"`,
        !$q.isNullOrWhiteSpace(dataItem.Icon) && dataItem.Icon.left(7).toLowerCase() === 'http://')
      .cat('>')
      .cat(`<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif" width="16px" height="16px" />`)
      .cat('</span>\n')
      .cat(`          <span class="text">${$q.htmlEncode(dataItem.Name)}</span>\n`)
      .cat('      </div>\n')
      .cat('  </div>\n')
      .cat('</li>\n');
  }

  _getSeparatorHtml(html) {
    html.cat('<li class="separator"></li>\n');
  }

  _extendMenuItemElement(menuItemElem, menuItem) {
    const $menuItem = this.getMenuItem(menuItemElem);
    if (!$q.isNullOrEmpty($menuItem)) {
      $menuItem.data('action_code', menuItem.ActionCode);
      $menuItem.data('action_type_code', menuItem.ActionTypeCode);
    }
  }

  _extendMenuItemElements(menuItems) {
    const that = this;
    $.each(menuItems, (index, menuItem) => {
      const $menuItem = that.getMenuItem(menuItem.ActionCode);
      that._extendMenuItemElement($menuItem, menuItem);
    }
    );
  }

  getMenuItemCount() {
    return $('> LI', this._contextMenuElement).length;
  }

  getMenuItem(menuItem) {
    let $menuItem = null;

    if ($q.isObject(menuItem)) {
      return $q.toJQuery(menuItem);
    } else if ($q.isString(menuItem)) {
      $menuItem = $(`LI[code='${menuItem}']`, this._contextMenuElement);
      if ($menuItem.length === 0) {
        $menuItem = null;
      }

      return $menuItem;
    }
    return undefined;
  }

  getMenuItemValue(menuItemElem) {
    const $menuItem = this.getMenuItem(menuItemElem);
    if ($q.isNullOrEmpty($menuItem)) {
      $q.alertFail($l.ContextMenu.menuItemNotSpecified);
      return undefined;
    }
    return $menuItem.attr('code');
  }

  getMenuItemText(menuItem) {
    const $menuItem = this.getMenuItem(menuItem);
    let menuItemText = '';

    if (!$q.isNullOrEmpty($menuItem)) {
      menuItemText = $('SPAN.text', $menuItem).text();
    }

    return menuItemText;
  }

  setVisibleState(menuItem, state) {
    const $menuItem = this.getMenuItem(menuItem);
    if (state) {
      $menuItem.show();
    } else {
      $menuItem.hide();
    }
  }

  setEnableState(menuItem, state) {
    const $menuItem = this.getMenuItem(menuItem);
    if (state) {
      $menuItem.removeClass('disabled');
    } else {
      $menuItem.addClass('disabled');
    }
  }

  _onContextMenuTune(e, context) {
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendContextMenuEventArgs();
    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_SHOWING, eventArgs);
    }

    if ($('LI.item', this._contextMenuElement).filter(function () {
      return $(this).css('display') !== 'none';
    }).length === 0) {
      return false;
    }
    return undefined;
  }

  _onCustomActionChanged() {
    this.refresh(this._hideRefreshMenuItem);
  }

  _onContextMenuHiding(e, context) {
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendContextMenuEventArgs();

    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_HIDING, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_HIDING, eventArgs);
    }
  }

  _onContextMenuHidden(e, context) {
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendContextMenuEventArgs();

    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_HIDDEN, eventArgs);
    }
  }

  _onContextMenuItemHovering(e, context) {
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendContextMenuItemEventArgs();

    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);
    eventArgs.set_menuItem(e.currentTarget);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_ITEM_HOVERING, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_ITEM_HOVERING, eventArgs);
    }
  }

  _onContextMenuItemClicking(e, context) {
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendContextMenuItemEventArgs();

    eventArgs.set_menu(this);
    eventArgs.set_targetElement(context);
    eventArgs.set_menuItem(e.currentTarget);

    this.notify(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, eventArgs);
    if (this.get_contextMenuManager()) {
      this.get_contextMenuManager().notify(window.EVENT_TYPE_CONTEXT_MENU_ITEM_CLICKING, eventArgs);
    }
  }

  dispose() {
    super.dispose();

    if (this._contextMenuManagerComponent) {
      const contextMenuCode = this._contextMenuCode;

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
      const contextMenuComponentName = this._getMenuComponentName();

      $(this._targetElements).each((index, targetElem) => {
        $(targetElem).removeData(contextMenuComponentName);
      });

      this._targetElements = null;
    }

    if (this._contextMenuElement) {
      let $contextMenu = $(this._contextMenuElement);

      $contextMenu.empty().remove();
      $contextMenu = null;
      this._contextMenuElement = null;
    }

    BackendContextMenuManager.getInstance().detachObserver(
      window.EVENT_TYPE_CUSTOM_ACTION_CHANGED, this._onCustomActionChangedHandler
    );

    this._onContextMenuTuneHandler = null;
    this._onContextMenuHidingHandler = null;
    this._onContextMenuHiddenHandler = null;
    this._onContextMenuItemHoveringHandler = null;
    this._onContextMenuItemClickingHandler = null;
    this._onCustomActionChangedHandler = null;

    $q.collectGarbageInIE();
  }
}


BackendContextMenu.getCacheKey = function (menuCode, loadItems, isBindToExternal) {
  return `contextMenuCachedData${menuCode}${loadItems}${isBindToExternal}`;
};

BackendContextMenu.getContextMenuByCode = function (
  menuCode, loadItems, isBindToExternal, successHandler, errorHandler) {
  const cacheKey = BackendContextMenu.getCacheKey(menuCode, loadItems, isBindToExternal);
  const contextMenuCachedData = GlobalCache.getItem(cacheKey);

  if (!contextMenuCachedData) {
    const actionUrl = `${window.CONTROLLER_URL_CONTEXT_MENU}GetByCode`;
    let params = { menuCode, loadItems };

    if (isBindToExternal) {
      params = Object.assign({}, params, { boundToExternal: true });
    }

    if ($q.isFunction(successHandler)) {
      $q.getJsonFromUrl('GET', actionUrl, params, false, false, (data, textStatus, jqXHR) => {
        GlobalCache.addItem(cacheKey, data);
        successHandler(data, textStatus, jqXHR);
      }, errorHandler);
    } else {
      let menu = null;

      $q.getJsonFromUrl('GET', actionUrl, params, false, false, data => {
        GlobalCache.addItem(cacheKey, data);
        menu = data;
      }, jqXHR => {
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
  return undefined;
};


export class BackendContextMenuEventArgs extends Sys.EventArgs {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    super();
  }

  _menuComponent = null;
  _targetElement = null;

  get_menu() { // eslint-disable-line camelcase
    return this._menuComponent;
  }

  set_menu(value) { // eslint-disable-line camelcase
    this._menuComponent = value;
  }

  get_targetElement() { // eslint-disable-line camelcase
    return this._targetElement;
  }

  set_targetElement(value) { // eslint-disable-line camelcase
    this._targetElement = value;
  }
}


export class BackendContextMenuItemEventArgs extends BackendContextMenuEventArgs {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    super();
  }

  _menuItemElement = null;
  get_menuItem() { // eslint-disable-line camelcase
    return this._menuItemElement;
  }

  set_menuItem(value) { // eslint-disable-line camelcase
    this._menuItemElement = value;
  }
}


Quantumart.QP8.BackendContextMenu = BackendContextMenu;
Quantumart.QP8.BackendContextMenuEventArgs = BackendContextMenuEventArgs;
Quantumart.QP8.BackendContextMenuItemEventArgs = BackendContextMenuItemEventArgs;
