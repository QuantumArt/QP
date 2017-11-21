/* eslint max-lines: 'off' */

window.EVENT_TYPE_TAB_STRIP_TAB_SELECT_REQUEST = 'OnTabSelectRequest';
window.EVENT_TYPE_TAB_STRIP_TAB_CLOSE_REQUEST = 'OnTabCloseRequest';
window.EVENT_TYPE_TAB_STRIP_TAB_SAVE_CLOSE_REQUEST = 'OnTabSaveAndCloseRequest';
window.EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST = 'OnFindTabInTreeRequest';

Quantumart.QP8.BackendTabStrip = function (tabStripElementId, options) {
  Quantumart.QP8.BackendTabStrip.initializeBase(this);

  this._tabStripElementId = tabStripElementId;
  if ($q.isObject(options)) {
    if (options.maxTabTextLength) {
      this._maxTabTextLength = options.maxTabTextLength;
    }

    if (options.maxTabMenuHeight) {
      this._maxTabMenuHeight = options.maxTabMenuHeight;
    }

    if (options.maxTabMenuItemTextLength) {
      this._maxTabMenuItemTextLength = options.maxTabMenuItemTextLength;
    }
  }

  $q.bindProxies.call(this, [
    '_onDocumentBodyClick',
    '_onTabClicking',
    '_onTabMiddleClick',
    '_onContextMenuShow',
    '_onWindowResized',
    '_onTabStripOverflowEvoked',
    '_onTabStripOverflowPrecluded',
    '_onCloseButtonHovering',
    '_onCloseButtonClicking',
    '_onCloseButtonClicked',
    '_onCloseButtonUnhovering',
    '_onTabMenuItemClicking',
    '_onTabMenuButtonHovered',
    '_onTabMenuButtonUnhovered',
    '_onTabMenuButtonClicking',
    '_onTabMenuButtonClicked',
    '_onTabMenuUpArrowHovered',
    '_onTabMenuUpArrowUnhovered',
    '_onTabMenuDownArrowHovered',
    '_onTabMenuDownArrowUnhovered'
  ]);
};

Quantumart.QP8.BackendTabStrip.prototype = {
  _tabGroups: {},
  _tabTypeCounters: {},

  _tabStripElementId: '',
  _tabStripElement: null,
  _tabStripScrollableElement: null,
  _tabListElement: '',
  _partialRemovedTabsContainerElement: null,
  _selectedTabId: '',
  _previousSelectedTabId: '',
  _leftSplitterPaneWidth: 0,
  _maxTabTextLength: 35,
  _tabMenuElement: null,
  _tabMenuScrollableElement: null,
  _tabMenuItemListElement: null,
  _tabMenuUpArrowButtonElement: null,
  _tabMenuDownArrowButtonElement: null,
  _tabMenuButtonContainerElement: null,
  _tabMenuButtonElement: null,
  _maxTabMenuHeight: 400,
  _maxTabMenuItemTextLength: 35,
  _tabContextMenuComponent: null,

  TAB_STRIP_BUSY_CLASS_NAME: 'busy',
  TAB_SELECTED_CLASS_NAME: 'selected',
  TAB_DISABLED_CLASS_NAME: 'disabled',
  TAB_CLICKABLE_SELECTORS: 'DIV.scrollable UL.tabList > LI.tab',
  CLOSE_BUTTON_CLICKABLE_SELECTORS: 'UL.tabList > LI.tab SPAN.closeButton',

  TAB_MENU_BUSY_CLASS_NAME: 'busy',
  TAB_MENU_TIMER_ID: 'tabMenuTimer',
  TAB_MENU_UP_ARROW_CLASS_NAME: 'upArrow',
  TAB_MENU_UP_ARROW_HOVER_CLASS_NAME: 'upArrowHovered',
  TAB_MENU_DOWN_ARROW_CLASS_NAME: 'downArrow',
  TAB_MENU_DOWN_ARROW_HOVER_CLASS_NAME: 'downArrowHovered',
  TAB_MENU_CLOSE_ALL_ITEM_CODE: 'CloseAll',
  TAB_MENU_SAVE_CLOSE_ALL_ITEM_CODE: 'SaveAndCloseAll',

  _onDocumentBodyClickHandler: null,
  _onTabClickingHandler: null,
  _onTabMiddleClickHandler: null,
  _onContextMenuShowHandler: null,
  _onWindowResizedHandler: null,
  _onTabStripOverflowEvokedHandler: null,
  _onTabStripOverflowPrecludedHandler: null,
  _onCloseButtonHoveringHandler: null,
  _onCloseButtonClickingHandler: null,
  _onCloseButtonClickedHandler: null,
  _onCloseButtonUnhoveringHandler: null,
  _onTabMenuItemClickingHandler: null,
  _onTabMenuButtonHoveredHandler: null,
  _onTabMenuButtonUnhoveredHandler: null,
  _onTabMenuButtonClickingHandler: null,
  _onTabMenuButtonClickedHandler: null,
  _onTabMenuUpArrowHoveredHandler: null,
  _onTabMenuUpArrowUnhoveredHandler: null,
  _onTabMenuDownArrowHoveredHandler: null,
  _onTabMenuDownArrowUnhoveredHandler: null,
  // eslint-disable-next-line camelcase
  get_tabStripElementId() {
    return this._tabStripElementId;
  },

  // eslint-disable-next-line camelcase
  set_tabStripElementId(value) {
    this._tabStripElementId = value;
  },

  // eslint-disable-next-line camelcase
  get_maxTabTextLength() {
    return this._maxTabTextLength;
  },

  // eslint-disable-next-line camelcase
  set_maxTabTextLength(value) {
    this._maxTabTextLength = value;
  },

  // eslint-disable-next-line camelcase
  get_maxTabMenuHeight() {
    return this._maxTabMenuHeight;
  },

  // eslint-disable-next-line camelcase
  set_maxTabMenuHeight(value) {
    this._maxTabMenuHeight = value;
  },

  // eslint-disable-next-line camelcase
  get_maxTabMenuItemTextLength() {
    return this._maxTabMenuItemTextLength;
  },

  // eslint-disable-next-line camelcase
  set_maxTabMenuItemTextLength(value) {
    this._maxTabMenuItemTextLength = value;
  },

  initialize() {
    const $tabStrip = $(`#${this._tabStripElementId}`);
    const $scrollable = $('<div />', { class: 'scrollable' });
    const $tabList = $('<ul />', { class: 'tabList' });
    const $partialRemovedTabsContainer = $('<div />', { id: 'partialRemovedTabs', css: { display: 'none' } });

    $tabStrip.empty();
    $tabStrip.append($scrollable);
    $tabStrip.append($partialRemovedTabsContainer);
    $scrollable.append($tabList);

    this._tabStripElement = $tabStrip.get(0);
    this._tabStripScrollableElement = $scrollable.get(0);
    this._tabListElement = $tabList.get(0);
    this._partialRemovedTabsContainerElement = $partialRemovedTabsContainer.get(0);
    this._attachTabStripEventHandlers();
    this._attachCloseButtonEventHandlers();
    this._createTabContextMenu();
  },

  _attachTabStripEventHandlers() {
    $(this._tabStripElement)
      .on('click', this.TAB_CLICKABLE_SELECTORS, this._onTabClickingHandler)
      .on('mouseup', this.TAB_CLICKABLE_SELECTORS, this._onTabMiddleClickHandler)
      .on($.fn.jeegoocontext.getContextMenuEventType(), this.TAB_CLICKABLE_SELECTORS, this._onContextMenuShowHandler);
  },

  _detachTabStripEventHandlers() {
    $(this._tabStripElement)
      .off('click', this.TAB_CLICKABLE_SELECTORS, this._onTabClickingHandler)
      .off('mouseup', this.TAB_CLICKABLE_SELECTORS, this._onTabMiddleClickHandler)
      .off($.fn.jeegoocontext.getContextMenuEventType(), this.TAB_CLICKABLE_SELECTORS, this._onContextMenuShowHandler);
  },

  fixTabStripWidth() {
    const $tabStrip = $(this._tabStripElement);
    const $menuButtonContainer = $(this._tabMenuButtonContainerElement);
    const newScrollableWidth = $tabStrip.width() - $menuButtonContainer.width();
    $(this._tabStripScrollableElement).css('width', `${newScrollableWidth}px`);
  },

  markAsBusy() {
    $(this._tabListElement).addClass(this.TAB_STRIP_BUSY_CLASS_NAME);
    $(this._tabMenuItemListElement).addClass(this.TAB_MENU_BUSY_CLASS_NAME);
  },

  unmarkAsBusy() {
    $(this._tabListElement).removeClass(this.TAB_STRIP_BUSY_CLASS_NAME);
    $(this._tabMenuItemListElement).removeClass(this.TAB_MENU_BUSY_CLASS_NAME);
  },

  isTabStripOverflow() {
    let result = false;
    const $scrollable = $(this._tabStripScrollableElement);
    const tabStripWidth = $scrollable.width();
    let allTabsWidth = 0;
    const $tabs = this.getAllTabs();
    const tabCount = $tabs.length;
    for (let tabIndex = 0; tabIndex < tabCount; tabIndex++) {
      allTabsWidth += $tabs.eq(tabIndex).width();
    }

    if (allTabsWidth > tabStripWidth) {
      result = true;
    }

    return result;
  },

  isTabStripBusy() {
    return $(this._tabListElement).hasClass(this.TAB_STRIP_BUSY_CLASS_NAME);
  },

  generateTabGroupCode(eventArgs, tabNumber) {
    const associatedAction = $a.getBackendAction(eventArgs.get_actionCode());
    const tabGroupCode = String.format(
      '{0}_{1}',
      associatedAction.Code,
      associatedAction.ActionType.Code === window.ACTION_TYPE_CODE_ADD_NEW
        ? tabNumber : this._getTabEntityId(eventArgs));

    return tabGroupCode;
  },

  getTabGroup(tabGroupCode) {
    if (this._tabGroups[tabGroupCode]) {
      return this._tabGroups[tabGroupCode];
    }

    return null;
  },

  createTabGroup(tabGroupCode) {
    let tabGroup = this.getTabGroup(tabGroupCode);
    if (!tabGroup) {
      tabGroup = [];
      this._tabGroups[tabGroupCode] = tabGroup;
    }

    return tabGroup;
  },

  closeTabGroup(tabGroupCode) {
    const tabGroup = this.getTabGroup(tabGroupCode);
    if ($q.isArray(tabGroup)) {
      const tabCount = tabGroup.length;
      for (let tabIndex = 0; tabIndex < tabCount; tabIndex++) {
        const tabId = tabGroup[tabIndex];
        this.closeTab(tabId);
      }
    }
  },

  removeTabGroup(tabGroupCode) {
    $q.removeProperty(this._tabGroups, tabGroupCode);
  },

  _removeEmptyTabGroup(tabGroupCode) {
    const tabGroup = this.getTabGroup(tabGroupCode);
    if ($q.isArray(tabGroup)) {
      if (tabGroup.length === 0) {
        $q.removeProperty(this._tabGroups, tabGroupCode);
      }
    }
  },

  _addTabToGroup(tab, tabGroupCode) {
    const $tab = this.getTab(tab);
    const tabId = this.getTabId($tab);
    const tabGroup = this.createTabGroup(tabGroupCode);

    if ($q.isArray(tabGroup)) {
      if (!Array.contains(tabGroup, tabId)) {
        Array.add(tabGroup, tabId);
      }
    }
  },

  _moveTabToGroup(tab, oldTabGroupCode, newTabGroupCode) {
    if (oldTabGroupCode !== newTabGroupCode) {
      const $tab = this.getTab(tab);
      const tabId = this.getTabId($tab);
      const oldTabGroup = this.getTabGroup(oldTabGroupCode);
      if ($q.isArray(oldTabGroup)) {
        Array.remove(oldTabGroup, tabId);
        this._removeEmptyTabGroup(oldTabGroupCode);
      }

      this._addTabToGroup(tab, newTabGroupCode);
    }
  },

  _removeTabFromGroup(tab) {
    const $tab = this.getTab(tab);
    const tabId = this.getTabId($tab);
    const tabGroupCode = this.getTabGroupCode($tab);
    const tabGroup = this.getTabGroup(tabGroupCode);

    if ($q.isArray(tabGroup)) {
      Array.remove(tabGroup, tabId);
      this._removeEmptyTabGroup(tabGroupCode);
    }
  },

  generateTabId() {
    let tabNumber = 1;
    const $tabs = this.getAllTabs();
    if ($tabs.length > 0) {
      const $lastTab = $tabs.last();
      const lastTabId = $lastTab.attr('id');
      const numberMatch = lastTabId.match('[0-9]+');
      if (numberMatch.length === 1) {
        tabNumber = parseInt(numberMatch[0], 10) + 1;
      } else {
        $q.alertError($l.TabStrip.tabIdGenerationErrorMessage);
        return null;
      }
    }

    return String.format('tab{0}', tabNumber);
  },

  generateTabText(eventArgs, tabNumber) {
    return Quantumart.QP8.BackendDocumentHost.generateTitle(eventArgs, { isTab: true, tabNumber });
  },

  getAllTabs() {
    return $('> LI.tab', this._tabListElement);
  },

  getAllTabsCount() {
    return this.getAllTabs().length;
  },

  getTabsByGroupCode(tabGroupCode) {
    return $(`> LI[groupCode='${tabGroupCode}'].tab`, this._tabListElement);
  },

  getTabsCountByGroupCode(tabGroupCode) {
    return this.getTabsByGroupCode(tabGroupCode).length;
  },

  getTabsByParentEntityTypeCodeAndParentEntityId(parentEntityTypeCode, parentEntityId) {
    const foundTabElems = [];
    const $tabs = this.getAllTabs();
    for (let tabIndex = 0, tabCount = $tabs.length; tabIndex < tabCount; tabIndex++) {
      const $tab = $tabs.eq(tabIndex);
      if ($tab.data('parent_entity_type_code') === parentEntityTypeCode
         && $tab.data('parent_entity_id') === `${parentEntityId}`) {
        Array.add(foundTabElems, $tab.get(0));
      }
    }

    return $(foundTabElems);
  },

  getTabsByEventArgs(eventArgs) {
    const foundTabElems = [];
    const $tabs = this.getAllTabs();
    for (let tabIndex = 0, tabCount = $tabs.length; tabIndex < tabCount; tabIndex++) {
      const $tab = $tabs.eq(tabIndex);
      if ($tab.data('entity_type_code') === eventArgs.get_entityTypeCode()
          && $tab.data('entity_id') === `${eventArgs.get_entityId()}`
          && $tab.data('action_code') === eventArgs.get_actionCode()) {
        Array.add(foundTabElems, $tab.get(0));
      }
    }

    return $(foundTabElems);
  },

  getFirstTabByGroupCode(tabGroupCode) {
    let $tab = $(`> LI[groupCode='${tabGroupCode}'].tab:first`, this._tabListElement).eq(0);
    if ($tab.length === 0) {
      $tab = null;
    }

    return $tab;
  },

  getTab(tab) {
    let $tab = null;
    if ($q.isObject(tab)) {
      return $q.toJQuery(tab);
    } else if ($q.isString(tab)) {
      $tab = $(this._tabListElement).find(`#${tab}`);
      if ($tab.length === 0) {
        $tab = null;
      }

      return $tab;
    }
    return undefined;
  },

  isLastTab(tab) {
    const $tab = this.getTab(tab);
    return $tab.next('LI').length === 0;
  },

  getTabId(tabElem) {
    if (!$q.isObject(tabElem)) {
      $q.alertError($l.TabStrip.tabNotSpecified);
      return undefined;
    }

    const $tab = $q.toJQuery(tabElem);
    let tabId = '';
    if (!$q.isNullOrEmpty($tab)) {
      tabId = $tab.attr('id');
    }

    return tabId;
  },

  getTabGroupCode(tabElem) {
    if (!$q.isObject(tabElem)) {
      $q.alertError($l.TabStrip.tabNotSpecified);
      return undefined;
    }

    const $tab = $q.toJQuery(tabElem);
    let tabValue = '';
    if (!$q.isNullOrEmpty($tab)) {
      tabValue = $tab.attr('groupCode');
    }

    return tabValue;
  },

  getTabText(tab) {
    const $tab = this.getTab(tab);
    let tabText = '';
    if (!$q.isNullOrEmpty($tab)) {
      tabText = $q.toString($tab.data('tab_text'), '');
    }

    return tabText;
  },

  setTabText(tab, tabText) {
    const $tab = this.getTab(tab);
    if (!$q.isNullOrEmpty($tab)) {
      const processedTabText = $q.toString(tabText, '').trim();
      const truncatedTabText = $q.middleCutShort(processedTabText, this._maxTabTextLength);
      const isTabTextTooLong = processedTabText.length > truncatedTabText.length;
      if (isTabTextTooLong) {
        $tab.attr('title', processedTabText);
      } else {
        $tab.removeAttr('title');
      }

      $tab.data('tab_text', processedTabText).find('SPAN.text').text(truncatedTabText);
    }
  },

  _getTabEntityTypeCode(eventArgs) {
    let result;
    const actionTypeCode = eventArgs.get_actionTypeCode();
    if (actionTypeCode === window.ACTION_TYPE_CODE_LIST) {
      result = Quantumart.QP8.BackendEntityType.getParentEntityTypeCodeByCode(eventArgs.get_entityTypeCode());
    } else {
      result = eventArgs.get_entityTypeCode();
    }

    return result;
  },

  _getTabEntityId(eventArgs) {
    let result;
    const actionTypeCode = eventArgs.get_actionTypeCode();
    if (actionTypeCode === window.ACTION_TYPE_CODE_LIST) {
      result = eventArgs.get_parentEntityId();
    } else if (eventArgs.get_isMultipleEntities()) {
      const entityIDs = $o.getEntityIDsFromEntities(eventArgs.get_entities());
      result = String.format('{0}_{1}', eventArgs.get_parentEntityId(), entityIDs.join('_and_'));
    } else {
      result = eventArgs.get_entityId();
    }

    return result;
  },

  getExistingTabId(eventArgs) {
    let result = 0;
    if (eventArgs.get_actionTypeCode() !== window.ACTION_TYPE_CODE_ADD_NEW) {
      const $tab = this.getFirstTabByGroupCode(this.generateTabGroupCode(eventArgs, 0));
      if ($q.isObject($tab)) {
        result = this.getTabId($tab);
      }
    }

    return result;
  },

  addNewTab(eventArgs) {
    const associatedAction = $a.getBackendAction(eventArgs.get_actionCode());
    const actionTypeCode = associatedAction.ActionType.Code;
    const tabTypeCode = associatedAction.Code;
    const tabNumber = actionTypeCode === window.ACTION_TYPE_CODE_ADD_NEW ? this._getTabTypeCounter(tabTypeCode) + 1 : 0;
    const tabId = this.generateTabId();
    const tabGroupCode = this.generateTabGroupCode(eventArgs, tabNumber);
    const tabText = this.generateTabText(eventArgs, tabNumber);

    let $tabList = $(this._tabListElement);
    $tabList.append(this._getTabHtml(tabId, tabGroupCode));
    const $tab = this.getTab(tabId);
    this._extendTabElement($tab, eventArgs, {
      TabGroupCode: tabGroupCode, TabText: tabText, TabTypeCode: tabTypeCode, TabNumber: tabNumber
    });
    this._addTabToGroup($tab, tabGroupCode);
    $tabList = null;

    if (this.getAllTabsCount() === 1) {
      $(window).resize(this._onWindowResizedHandler);
      this._addTabMenuToTabStrip();
    }

    this._previousSelectedTabId = this._selectedTabId.valueOf();
    this._selectedTabId = tabId;

    if (actionTypeCode === window.ACTION_TYPE_CODE_ADD_NEW) {
      this._increaseTabTypeCounter(tabTypeCode);
    }

    if (this.isTabStripOverflow()) {
      this._onTabStripOverflowEvokedHandler(this, null);
    }

    if (jQuery.isEmptyObject(eventArgs.get_context()) || !eventArgs.get_context().ctrlKey) {
      this.highlightTab($tab);
    }

    this.fixTabStripWidth();
    return tabId;
  },

  updateTab(tab, eventArgs) {
    const $tab = this.getTab(tab);

    const oldAction = $a.getBackendActionByCode($tab.data('action_code'));
    const oldActionTypeCode = oldAction.ActionType.Code;
    const oldActionCode = oldAction.Code;
    const oldTabGroupCode = this.getTabGroupCode($tab);
    const oldTabTypeCode = oldActionCode;
    const oldTabNumber = $tab.data('tab_number');
    if (oldActionTypeCode === window.ACTION_TYPE_CODE_ADD_NEW) {
      this._decreaseTabTypeCounter(oldTabTypeCode, oldTabNumber);
    }

    const newActionCode = eventArgs.get_actionCode();
    const newAction = $a.getBackendAction(newActionCode);
    const newActionTypeCode = newAction.ActionType.Code;
    if (newActionTypeCode === window.ACTION_TYPE_CODE_ADD_NEW) {
      this._increaseTabTypeCounter(newActionCode);
    }
    const newTabNumber = this._getTabTypeCounter(newActionCode);
    const newTabGroupCode = this.generateTabGroupCode(eventArgs, newTabNumber);
    const newTabText = this.generateTabText(eventArgs, newTabNumber);

    $tab.attr('groupCode', newTabGroupCode);
    this._extendTabElement(
      $tab, eventArgs, {
        TabGroupCode: newTabGroupCode, TabText: newTabText, TabTypeCode: newActionCode, TabNumber: newTabNumber
      }
    );

    if (newTabGroupCode !== oldTabGroupCode) {
      this._moveTabToGroup($tab, oldTabGroupCode, newTabGroupCode);
    }
  },

  selfUpdateTab(tab) {
    this.updateTab(tab, this.getEventArgsFromTab(tab));
  },

  selectTab(tab) {
    const $tab = this.getTab(tab);

    // Запоминаем код выбранного таба
    this._previousSelectedTabId = this._selectedTabId.valueOf();
    this._selectedTabId = this.getTabId($tab);

    if (this._selectedTabId !== this._previousSelectedTabId) {
      // Оповещаем о начале выбора компоненты-наблюдатели

      // Выделяем таб
      this.highlightTab($tab);

      // Устанавливаем ширину группы табов
      this.fixTabStripWidth();
    }
  },

  selectTabRequest(tab) {
    let eventArgs = this.getEventArgsFromTab(tab);
    if ($q.isObject(eventArgs)) {
      this.notify(window.EVENT_TYPE_TAB_STRIP_TAB_SELECT_REQUEST, eventArgs);
    }
    eventArgs = null;
  },

  highlightTab(tab) {
    const $tab = this.getTab(tab);
    const $tabs = this.getAllTabs();

    // Снимаем выделение со всех табов
    $tabs.removeClass(this.TAB_SELECTED_CLASS_NAME);

    // Выделяем заданный таб
    $tab.addClass(this.TAB_SELECTED_CLASS_NAME);

    // Прокручиваем группу табов до выделяемого таба
    this.scrollToTab($tab);
  },

  scrollToTab(tab, duration) {
    let $tab = this.getTab(tab);
    if ($tab) {
      let $scrollable = $(this._tabStripScrollableElement);
      const tabStripWidth = $scrollable.width();
      const tabWidth = $tab.width();
      let tabStripOffset = 0;
      tabStripOffset = -1 * (tabStripWidth - tabWidth);
      if (!this.isLastTab($tab)) {
        tabStripOffset += 30;
      }
      $scrollable.scrollTo($tab,
        {
          duration: duration || 400,
          axis: 'x',
          offset: tabStripOffset
        }
      );

      $scrollable = null;
    }

    $tab = null;
  },

  closeTab(tab) {
    let $tab = this.getTab(tab);
    if (!$q.isNullOrEmpty($tab)) {
      const tabTypeCode = $tab.data('tab_type_code');
      const tabNumber = $tab.data('tab_number');

      this._partialRemoveTab($tab);

      if (!this.isTabStripOverflow()) {
        this._onTabStripOverflowPrecludedHandler(this, null);
      }

      this._decreaseTabTypeCounter(tabTypeCode, tabNumber);
      if (this.getAllTabsCount() === 0) {
        this._hideTabMenu();
        this._removeTabMenuFromTabStrip();
        $(window).unbind('resize', this._onWindowResizedHandler);
      }

      this._removeTab($tab);
    }

    $tab = null;
  },

  _partialRemoveTab(tab) {
    const $tab = this.getTab(tab);
    const $partialRemovedTabsContainer = $(this._partialRemovedTabsContainerElement);

    $tab.appendTo($partialRemovedTabsContainer);
  },

  _removeTab(tab) {
    const $tab = this.getTab(tab);
    $tab
      .removeData()
      .empty()
      .remove()
    ;

    this._removeTabFromGroup($tab);
  },

  _getTabHtml(tabId, tabGroupCode) {
    const html = new $.telerik.stringBuilder();
    html
      .cat(`<li id="${$q.htmlEncode(tabId)}" groupCode="${$q.htmlEncode(tabGroupCode)}" class="tab">\n`)
      .cat('  <a class="tabLink" href="javascript:void(0);">')
      .cat('<span class="wrapper">')
      .cat('<span class="text"></span>')
      .cat(`<span class="closeButton"><img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif" /></span>`)
      .cat('</span>')
      .cat('</a>')
      .cat('</li>\n');

    return html.string();
  },

  _extendTabElement(tabElem, eventArgs, params) {
    const $tab = this.getTab(tabElem);
    $tab.data('entity_type_code', eventArgs.get_entityTypeCode());
    $tab.data('entity_id', eventArgs.get_entityId());
    $tab.data('entity_name', eventArgs.get_entityName());
    $tab.data('parent_entity_type_code', Quantumart.QP8.BackendEntityType.getParentEntityTypeCodeByCode(
      eventArgs.get_entityTypeCode())
    );
    $tab.data('parent_entity_id', eventArgs.get_parentEntityId());
    $tab.data('action_type_code', eventArgs.get_actionTypeCode());
    $tab.data('action_code', eventArgs.get_actionCode());
    $tab.data('is_multiple_entities', eventArgs.get_isMultipleEntities());
    $tab.data('entities', eventArgs.get_entities());
    $tab.data('tab_type_code', params.TabTypeCode);
    $tab.data('tab_number', params.TabNumber);
    this.setTabText($tab, params.TabText);
  },

  _getCloseButton(tab) {
    return this.getTab(tab).find('> a.tabLink > span.wrapper > span.closeButton');
  },

  _attachCloseButtonEventHandlers() {
    const $tabStrip = $(this._tabStripElement);
    $tabStrip
      .on('mouseover', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonHoveringHandler)
      .on('mousedown', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonClickingHandler)
      .on('mouseup', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonClickedHandler)
      .on('mouseout', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonUnhoveringHandler);
  },

  _detachCloseButtonEventHandlers() {
    const $tabStrip = $(this._tabStripElement);
    $tabStrip
      .off('mouseover', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonHoveringHandler)
      .off('mousedown', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonClickingHandler)
      .off('mouseup', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonClickedHandler)
      .off('mouseout', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonUnhoveringHandler);
  },

  _applyHoveredStyleToCloseButton(closeButtonElem) {
    $('img', closeButtonElem).removeClass('clicked').addClass('hover');
  },

  _applyClickedStyleToCloseButton(closeButtonElem) {
    $('img', closeButtonElem).removeClass('hover').addClass('clicked');
  },

  _cancelAllStylesForCloseButton(closeButtonElem) {
    $('img', closeButtonElem).removeClass('hover').removeClass('clicked');
  },

  isTabSelected(tab) {
    const $tab = this.getTab(tab);
    return this.getTabId($tab) === this._selectedTabId;
  },

  _getTabTypeCounter(tabTypeCode) {
    let tabTypeCount = 0;
    if (this._tabTypeCounters[tabTypeCode]) {
      tabTypeCount = this._tabTypeCounters[tabTypeCode];
    }

    return tabTypeCount;
  },

  _increaseTabTypeCounter(tabTypeCode) {
    if (this._tabTypeCounters[tabTypeCode]) {
      this._tabTypeCounters[tabTypeCode] += 1;
    } else {
      this._tabTypeCounters[tabTypeCode] = 1;
    }
  },

  _decreaseTabTypeCounter(tabTypeCode, tabNumber) {
    if (this._tabTypeCounters[tabTypeCode]) {
      const $tabs = this.getAllTabs();
      const tabCount = $tabs.length;
      let matchedTabCount = 0;
      for (let tabIndex = 0; tabIndex < tabCount; tabIndex++) {
        if ($tabs.eq(tabIndex).data('tab_type_code') === tabTypeCode) {
          matchedTabCount += 1;
        }
      }

      if (matchedTabCount === 0) {
        this._tabTypeCounters[tabTypeCode] = 0;
      } else if (this._getTabTypeCounter(tabTypeCode) === tabNumber) {
        this._tabTypeCounters[tabTypeCode] -= 1;
      }
    }
  },

  _isTabMenuOverflow() {
    const menuItemListHeight = $(this._tabMenuItemListElement).outerHeight();
    const scrollableHeight = $(this._tabMenuScrollableElement).outerHeight();
    return (menuItemListHeight - scrollableHeight) > 1;
  },

  _isTabMenuTopOverflow() {
    return $(this._tabMenuScrollableElement).scrollTop() > 0;
  },

  _isTabMenuBottomOverflow() {
    const $scrollable = $(this._tabMenuScrollableElement);
    const menuItemListHeight = $(this._tabMenuItemListElement).height();
    const scrollableHeight = $scrollable.height();
    const scrollableTopPosition = $scrollable.scrollTop();

    return scrollableTopPosition < (menuItemListHeight - scrollableHeight);
  },

  _calculateTabMenuScrollingDuration(isUpScrolling) {
    let duration = 0;
    const $scrollable = $(this._tabMenuScrollableElement);
    const menuItemListHeight = $(this._tabMenuItemListElement).height();
    const scrollableHeight = $scrollable.height();
    const scrollableTopPosition = $scrollable.scrollTop();
    let scrollingHeight = 0;
    if (isUpScrolling) {
      if (scrollableTopPosition > 0) {
        scrollingHeight = scrollableTopPosition + (scrollableHeight / 2);
      }
    } else {
      scrollingHeight = menuItemListHeight - scrollableTopPosition - (scrollableHeight / 2);
    }

    if (scrollingHeight > 0) {
      duration = parseFloat(scrollingHeight) * 1500 / 220;
    }

    return duration;
  },

  _addTabMenuToTabStrip() {
    const $tabStrip = $(this._tabStripElement);
    if ($q.isNull(this._tabMenuButtonContainerElement)) {
      const $menuButtonContainer = $('<div />', { class: 'tabMenuButton' });
      const $menuButton = $('<span />');
      const $menuButtonImage = $('<img />', {
        src: `${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif`,
        css: { border: 'none' }
      });

      $menuButton.append($menuButtonImage);
      $menuButtonContainer.append($menuButton);

      this._tabMenuButtonContainerElement = $menuButtonContainer.get(0);
      this._tabMenuButtonElement = $menuButton.get(0);
      this._attachTabMenuButtonEventHandlers();
      $tabStrip.append($menuButtonContainer);
    }

    if ($q.isNull(this._tabMenuElement)) {
      const $menu = $('<div />', { class: 'tabMenu' });
      const $upArrow = $('<div />', { class: this.TAB_MENU_UP_ARROW_CLASS_NAME, css: { display: 'none' } });
      $menu.append($upArrow);

      const $scrollable = $('<div />', { class: 'scrollable' });
      $menu.append($scrollable);

      const $menuItemList = $('<ul />');
      $scrollable.append($menuItemList);

      const $downArrow = $('<div />', { class: this.TAB_MENU_DOWN_ARROW_CLASS_NAME, css: { display: 'none' } });
      $downArrow.hide(0);
      $menu.append($downArrow);

      this._tabMenuElement = $menu.get(0);
      this._tabMenuUpArrowButtonElement = $upArrow.get(0);
      this._tabMenuScrollableElement = $scrollable.get(0);
      this._tabMenuDownArrowButtonElement = $downArrow.get(0);
      this._tabMenuItemListElement = $menuItemList.get(0);
      this._attachTabMenuEventHandlers();
      $tabStrip.append($menu);
    }
  },

  _removeTabMenuFromTabStrip() {
    this._detachTabMenuButtonEventHandlers();
    $(this._tabMenuButtonContainerElement).empty().remove();
    this._detachTabMenuEventHandlers();
    $(this._tabMenuElement).stopTime(this.TAB_MENU_TIMER_ID).empty().remove();

    $q.dispose.call(this, [
      '_tabMenuButtonElement',
      '_tabMenuButtonContainerElement',
      '_tabMenuItemListElement',
      '_tabMenuUpArrowButtonElement',
      '_tabMenuScrollableElement',
      '_tabMenuDownArrowButtonElement',
      '_tabMenuElement'
    ]);
  },

  _attachTabMenuEventHandlers() {
    $(document.body)
      .bind('click', this._onDocumentBodyClickHandler)
      .bind('dblclick', this._onDocumentBodyClickHandler)
      .bind('contextmenu', this._onDocumentBodyClickHandler);

    $(this._tabMenuElement).on('click', 'li.item', this._onTabMenuItemClickingHandler);
    $(this._tabMenuUpArrowButtonElement)
      .bind('mouseover', this._onTabMenuUpArrowHoveredHandler)
      .bind('mouseout', this._onTabMenuUpArrowUnhoveredHandler);

    $(this._tabMenuDownArrowButtonElement)
      .bind('mouseover', this._onTabMenuDownArrowHoveredHandler)
      .bind('mouseout', this._onTabMenuDownArrowUnhoveredHandler);
  },

  _detachTabMenuEventHandlers() {
    $(document.body)
      .unbind('click', this._onDocumentBodyClickHandler)
      .unbind('dblclick', this._onDocumentBodyClickHandler)
      .unbind('contextmenu', this._onDocumentBodyClickHandler);

    $(this._tabMenuElement).off('click', 'li.item', this._onTabMenuItemClickingHandler);
    $(this._tabMenuUpArrowButtonElement)
      .unbind('mouseover', this._onTabMenuUpArrowHoveredHandler)
      .unbind('mouseout', this._onTabMenuUpArrowUnhoveredHandler);

    $(this._tabMenuDownArrowButtonElement)
      .unbind('mouseover', this._onTabMenuDownArrowHoveredHandler)
      .unbind('mouseout', this._onTabMenuDownArrowUnhoveredHandler);
  },

  _getMenuItems() {
    return $('li.item', this._tabMenuItemListElement);
  },

  _changeTabStripOverflowIndicator(isOverflow) {
    const $menuButtonContainer = $(this._tabMenuButtonContainerElement);
    if (isOverflow) {
      $menuButtonContainer.removeClass('tabMenuButton').addClass('tabOverflowMenuButton');
    } else {
      $menuButtonContainer.removeClass('tabOverflowMenuButton').addClass('tabMenuButton');
    }
  },

  _loadItemsToTabMenu() {
    this._removeItemsFromTabMenu();
    const $menuList = $(this._tabMenuItemListElement);
    const $tabs = this.getAllTabs();
    const tabCount = $tabs.length;
    let menuItemsHtml = new $.telerik.stringBuilder();
    for (let tabIndex = 0; tabIndex < tabCount; tabIndex++) {
      const $tab = $tabs.eq(tabIndex);
      if ($tab) {
        const dataItem = {
          TabId: this.getTabId($tab),
          TabText: this.getTabText($tab),
          Selected: this.isTabSelected($tab),
          Icon: ''
        };

        menuItemsHtml = this._getTabMenuItemHtml(menuItemsHtml, dataItem);
      }
    }

    menuItemsHtml.cat('<li class="separator"></li>');
    this._getTabMenuItemHtml(menuItemsHtml, {
      TabId: this.TAB_MENU_SAVE_CLOSE_ALL_ITEM_CODE,
      TabText: $l.TabStrip.saveAndCloseAllTabs,
      Icon: ''
    });

    this._getTabMenuItemHtml(menuItemsHtml, {
      TabId: this.TAB_MENU_CLOSE_ALL_ITEM_CODE,
      TabText: $l.TabStrip.closeAllTabs,
      Icon: ''
    });

    $menuList.html(menuItemsHtml.string());
  },

  _getTabMenuItemHtml(html, dataItem) {
    const processedTabText = $q.toString(dataItem.TabText, '').trim();
    const truncatedTabText = $q.middleCutShort(processedTabText, this._maxTabMenuItemTextLength);
    const isTabTextTooLong = processedTabText.length > truncatedTabText.length;
    let text = $q.htmlEncode(truncatedTabText);
    if (dataItem.Selected) {
      text = `<b>${text}</b>`;
    }

    html
      .cat(`<li code="${$q.htmlEncode(dataItem.TabId)}" class="item"`)
      .catIf(` title="${$q.htmlEncode(processedTabText)}"`, isTabTextTooLong)
      .cat('>\n')
      .cat('  <div class="outerWrapper">\n')
      .cat('      <div class="innerWrapper">\n')
      .cat('          <span class="icon"')
      .catIf(` style="background-image: url(${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS}${dataItem.Icon})"`,
        !$q.isNullOrWhiteSpace(dataItem.Icon))
      .cat('>')
      .cat(`<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}/0.gif" width="16px" height="16px" />`)
      .cat('</span>\n')
      .cat(`          <span class="text">${text}</span>\n`)
      .cat('      </div>\n')
      .cat('  </div>\n')
      .cat('</li>\n');

    return html;
  },

  _removeItemsFromTabMenu() {
    $(this._tabMenuItemListElement).empty();
  },

  _toggleTabMenu() {
    if ($(this._tabMenuElement).is(':hidden')) {
      this._showTabMenu();
    } else {
      this._hideTabMenu();
    }
  },

  _showTabMenu() {
    this._tabContextMenuComponent.hide();
    const $menu = $(this._tabMenuElement);
    $menu.css('height', 'auto');
    this._loadItemsToTabMenu();

    const $menuButton = $(this._tabMenuButtonElement);
    const menuHeight = $menu.outerHeight();
    const menuTop = $menuButton.offset().top + $menuButton.height()
    + $menuButton.borderTopWidth() + $menuButton.borderBottomWidth();
    const menuRight = $(window).width() - $menuButton.offset().left - $menuButton.outerWidth();

    const $scrollable = $(this._tabMenuScrollableElement);
    if (menuHeight > this._maxTabMenuHeight) {
      $scrollable.css({ height: `${this._maxTabMenuHeight}px`, overflow: 'hidden' });
    } else {
      $scrollable.css({ height: 'auto', overflow: 'visible' });
    }

    $menu.css('top', `${menuTop}px`);
    $menu.css('right', `${menuRight}px`);

    $menu.fadeIn(200, () => {
      this._refreshTabMenuArrowButtons();
      $menu.everyTime(30, this.TAB_MENU_TIMER_ID, () => {
        this._refreshTabMenuArrowButtons();
      });
    });
  },

  _hideTabMenu() {
    const $menu = $(this._tabMenuElement);
    const $upArrow = $(this._tabMenuUpArrowButtonElement);
    const $downArrow = $(this._tabMenuDownArrowButtonElement);

    $menu.stopTime(this.TAB_MENU_TIMER_ID);
    $upArrow.fadeOut(50);
    $downArrow.fadeOut(50);

    $menu.fadeOut(200, () => {
      if (this._isTabMenuOverflow()) {
        this._scrollTabMenuToFirstItem(0);
      }
    });

    const $menuButton = $(this._tabMenuButtonElement);
    this._cancelAllStylesForTabMenuButton($menuButton);

    $menuButton
      .bind('mouseover', this._onTabMenuButtonHoveredHandler)
      .bind('mouseout', this._onTabMenuButtonUnhoveredHandler);

    this._removeItemsFromTabMenu();
  },

  _refreshTabMenuArrowButtons() {
    const $upArrow = $(this._tabMenuUpArrowButtonElement);
    const $downArrow = $(this._tabMenuDownArrowButtonElement);
    const isOverflow = this._isTabMenuOverflow();

    if (isOverflow && this._isTabMenuTopOverflow()) {
      $upArrow.fadeIn(30);
    } else {
      $upArrow.fadeOut(30);
    }

    if (isOverflow && this._isTabMenuBottomOverflow()) {
      $downArrow.fadeIn(30);
    } else {
      $downArrow.fadeOut(30);
    }
  },

  _scrollTabMenuToFirstItem(duration) {
    const $scrollable = $(this._tabMenuScrollableElement);
    const $firstMenuItem = this._getMenuItems().first();
    const options = { axis: 'y', easing: 'swing' };
    options.duration = $q.isNull(duration) ? this._calculateTabMenuScrollingDuration(true) : duration;
    $scrollable.scrollTo($firstMenuItem, options);
  },

  _scrollTabMenuToLastItem(duration) {
    const $lastMenuItem = this._getMenuItems().last();
    const options = { axis: 'y', easing: 'swing' };
    options.duration = $q.isNull(duration) ? this._calculateTabMenuScrollingDuration(false) : duration;
    $(this._tabMenuScrollableElement).scrollTo($lastMenuItem, options);
  },

  _stopTabMenuScrolling() {
    $(this._tabMenuScrollableElement).stop();
  },

  _attachTabMenuButtonEventHandlers() {
    $(this._tabMenuButtonElement)
      .bind('mouseover', this._onTabMenuButtonHoveredHandler)
      .bind('mousedown', this._onTabMenuButtonClickingHandler)
      .bind('click', this._onTabMenuButtonClickedHandler)
      .bind('mouseout', this._onTabMenuButtonUnhoveredHandler);
  },

  _detachTabMenuButtonEventHandlers() {
    $(this._tabMenuButtonElement)
      .unbind('mouseover', this._onTabMenuButtonHoveredHandler)
      .unbind('mousedown', this._onTabMenuButtonClickingHandler)
      .unbind('click', this._onTabMenuButtonClickedHandler)
      .unbind('mouseout', this._onTabMenuButtonUnhoveredHandler);
  },

  _applyHoveredStyleToTabMenuButton(buttonElem) {
    $(buttonElem).addClass('hover');
  },

  _applyClickedStyleToTabMenuButton(buttonElem) {
    $(buttonElem).removeClass('hover').addClass('clicked');
  },

  _cancelAllStylesForTabMenuButton(buttonElem) {
    $(buttonElem).removeClass('hover').removeClass('clicked');
  },

  isTabMenuBusy() {
    return $(this._tabMenuItemListElement).hasClass(this.TAB_MENU_BUSY_CLASS_NAME);
  },

  getEventArgsFromTab(tab) {
    const $tab = this.getTab(tab);
    const actionCode = $tab.data('action_code');
    const action = $a.getBackendActionByCode(actionCode);
    const eventArgs = new Quantumart.QP8.BackendTabEventArgs();
    eventArgs.set_entityTypeCode($tab.data('entity_type_code'));
    eventArgs.set_entityId($tab.data('entity_id'));
    eventArgs.set_entityName($tab.data('entity_name'));
    eventArgs.set_parentEntityId($tab.data('parent_entity_id'));
    eventArgs.set_actionCode(actionCode);
    eventArgs.set_actionTypeCode(action.ActionType.Code);
    eventArgs.set_entities($tab.data('entities'));
    eventArgs.set_isMultipleEntities($q.toBoolean($tab.data('is_multiple_entities'), false));
    eventArgs.set_tabId(this.getTabId($tab));
    return eventArgs;
  },

  updateParentInfo(entityTypeCode, entityId) {
    const $tabs = this.getTabsByParentEntityTypeCodeAndParentEntityId(entityTypeCode, entityId);
    for (let tabIndex = 0, tabCount = $tabs.length; tabIndex < tabCount; tabIndex++) {
      const $tab = $tabs.eq(tabIndex);
      this.selfUpdateTab($tab);
    }
  },

  tabEntityExists($tab) {
    let result = true;
    const actionTypeCode = $tab.data('action_type_code');
    if (actionTypeCode !== window.ACTION_TYPE_CODE_ADD_NEW
      && actionTypeCode !== window.ACTION_TYPE_CODE_MULTIPLE_SELECT) {
      const eventArgs = this.getEventArgsFromTab($tab);
      const tabEntityTypeCode = this._getTabEntityTypeCode(eventArgs);
      if ($q.toBoolean($tab.data('is_multiple_entities'), false)) {
        const entities = $tab.data('entities');
        for (let entityIndex = 0; entityIndex < entities.length; entityIndex++) {
          const entity = entities[entityIndex];
          if (entity) {
            if (!$o.checkEntityExistence(tabEntityTypeCode, entity.Id)) {
              result = false;
              break;
            }
          }
        }
      } else {
        result = $o.checkEntityExistence(tabEntityTypeCode, this._getTabEntityId(eventArgs));
      }
    }

    return result;
  },

  getAnotherTabToSelect($tab) {
    let $tabToSelect = null;
    const isSelected = this.isTabSelected($tab);

    if (isSelected) {
      $tabToSelect = $tab.next();
      if ($q.isNullOrEmpty($tabToSelect)) {
        $tabToSelect = $tab.prev();
      }
    } else {
      $tabToSelect = this.getTab(this._selectedTabId);
    }

    return $tabToSelect;
  },

  _onDocumentBodyClick() {
    this._hideTabMenu();
  },

  _onTabClicking(e) {
    const $tab = $(e.currentTarget);
    if (!$tab.hasClass(this.TAB_DISABLED_CLASS_NAME)
      && !this.isTabSelected($tab)
      && !this.isTabStripBusy()) {
      this.selectTabRequest($tab);
    } else {
      e.preventDefault();
    }
  },

  _onTabMiddleClick(e) {
    if (e.which === 2) {
      this._tabContextMenuComponent.hide(e);
      this._closeTabRequest($(e.currentTarget));
      e.preventDefault();
      e.stopPropagation();
    }
  },

  _onContextMenuShow(e) {
    this._tabContextMenuComponent.show(e, e.currentTarget);
    e.preventDefault();
  },

  _onWindowResized() {
    this.fixTabStripWidth();
  },

  _onTabStripOverflowEvoked() {
    this._changeTabStripOverflowIndicator(true);
  },

  _onTabStripOverflowPrecluded() {
    this._changeTabStripOverflowIndicator(false);
  },

  _onCloseButtonHovering(e) {
    this._applyHoveredStyleToCloseButton(e.currentTarget);
  },

  _onCloseButtonClicking(e) {
    if (this.isTabStripBusy()) {
      e.preventDefault();
    } else {
      this._applyClickedStyleToCloseButton(e.currentTarget);
    }
  },

  _onCloseButtonClicked(e) {
    if (this.isTabStripBusy()) {
      e.preventDefault();
    } else {
      this._tabContextMenuComponent.hide(e);

      const $tab = $(e.currentTarget).parent().parent().parent();
      this._closeTabRequest($tab);
      this._applyHoveredStyleToCloseButton(e.currentTarget);

      e.stopPropagation();
    }
  },

  _closeTabRequest(tab) {
    if (!this.isTabStripBusy()) {
      let eventArgs = this.getEventArgsFromTab(tab);
      if ($q.isObject(eventArgs)) {
        this.notify(window.EVENT_TYPE_TAB_STRIP_TAB_CLOSE_REQUEST, eventArgs);
      }
      eventArgs = null;
    }
  },

  _closeAllTabRequest() {
    if (!this.isTabStripBusy()) {
      $(this._tabStripElement)
        .find(this.TAB_CLICKABLE_SELECTORS)
        .each($.proxy(
          function (i, tab) {
            this._closeTabRequest(tab);
          }, this)
        );
    }
  },

  _saveAndCloseAllTabRequest() {
    if (!this.isTabStripBusy()) {
      $(this._tabStripElement)
        .find(this.TAB_CLICKABLE_SELECTORS)
        .each((i, tab) => {
          let eventArgs = this.getEventArgsFromTab(tab);
          if ($q.isObject(eventArgs)) {
            this.notify(window.EVENT_TYPE_TAB_STRIP_TAB_SAVE_CLOSE_REQUEST, eventArgs);
          }
          eventArgs = null;
        }
        );
    }
  },

  _closeButThisTabRequest($tab) {
    if (!this.isTabStripBusy()) {
      $(this._tabStripElement)
        .find(this.TAB_CLICKABLE_SELECTORS)
        .not($tab)
        .each($.proxy(
          function (i, tab) {
            this._closeTabRequest(tab);
          }, this)
        );
      this.scrollToTab($tab, 1);
      this.fixTabStripWidth();
    }
  },

  _findInTreeRequest(tab) {
    let eventArgs = this.getEventArgsFromTab(tab);
    if ($q.isObject(eventArgs)) {
      eventArgs.isExpandRequested = true;
      this.notify(window.EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST, eventArgs);
    }
    eventArgs = null;
  },

  _onCloseButtonUnhovering(e) {
    this._cancelAllStylesForCloseButton(e.currentTarget);
  },

  _onTabMenuItemClicking(e) {
    if (this.isTabMenuBusy()) {
      e.preventDefault();
    } else {
      const $menuItem = $(e.currentTarget);
      const tabId = $menuItem.attr('code');
      if (tabId === this.TAB_MENU_CLOSE_ALL_ITEM_CODE) {
        this._closeAllTabRequest();
      } else if (tabId === this.TAB_MENU_SAVE_CLOSE_ALL_ITEM_CODE) {
        this._saveAndCloseAllTabRequest();
      } else {
        this.selectTabRequest(tabId);
      }
      this._hideTabMenu();
    }
  },

  _onTabMenuButtonHovered(e) {
    this._applyHoveredStyleToTabMenuButton(e.currentTarget);
  },

  _onTabMenuButtonUnhovered(e) {
    this._cancelAllStylesForTabMenuButton(e.currentTarget);
  },

  _onTabMenuButtonClicking(e) {
    if (this.isTabMenuBusy()) {
      e.preventDefault();
    } else {
      this._applyClickedStyleToTabMenuButton(e.currentTarget);

      e.stopPropagation();
    }
  },

  _onTabMenuButtonClicked(e) {
    if (this.isTabMenuBusy()) {
      e.preventDefault();
    } else {
      let $menuButton = $(e.currentTarget);
      $menuButton
        .unbind('mouseover', this._onTabMenuButtonHoveredHandler)
        .unbind('mouseout', this._onTabMenuButtonUnhoveredHandler)
      ;

      $menuButton = null;

      this._toggleTabMenu();

      e.stopPropagation();
    }
  },

  _onTabMenuUpArrowHovered(e) {
    let $upArrow = $(e.currentTarget);
    $upArrow
      .removeClass(this.TAB_MENU_UP_ARROW_CLASS_NAME)
      .addClass(this.TAB_MENU_UP_ARROW_HOVER_CLASS_NAME)
    ;

    this._scrollTabMenuToFirstItem();

    $upArrow = null;
  },

  _onTabMenuUpArrowUnhovered(e) {
    let $upArrow = $(e.currentTarget);
    $upArrow
      .removeClass(this.TAB_MENU_UP_ARROW_HOVER_CLASS_NAME)
      .addClass(this.TAB_MENU_UP_ARROW_CLASS_NAME)
    ;

    this._stopTabMenuScrolling();

    $upArrow = null;
  },

  _onTabMenuDownArrowHovered(e) {
    let $downArrow = $(e.currentTarget);
    $downArrow
      .removeClass(this.TAB_MENU_DOWN_ARROW_CLASS_NAME)
      .addClass(this.TAB_MENU_DOWN_ARROW_HOVER_CLASS_NAME)
    ;

    this._scrollTabMenuToLastItem();

    $downArrow = null;
  },

  _onTabMenuDownArrowUnhovered(e) {
    let $downArrow = $(e.currentTarget);
    $downArrow
      .removeClass(this.TAB_MENU_DOWN_ARROW_HOVER_CLASS_NAME)
      .addClass(this.TAB_MENU_DOWN_ARROW_CLASS_NAME)
    ;

    this._stopTabMenuScrolling();

    $downArrow = null;
  },

  _createTabContextMenu() {
    const CLOSE_CODE = 'close';
    const CLOSE_BUT_THIS_CODE = 'close_but_this';
    const CLOSE_ALL_CODE = 'close_all';
    const FIND_IN_TREE_CODE = 'find_in_tree';

    const tabContextMenuElementId = `${this._tabStripElementId}_tabContextMenu`;
    const $menu = $('<ul />', { id: tabContextMenuElementId, class: 'contextMenu' });
    const menuItemsHtml = new $.telerik.stringBuilder();
    this._getTabMenuItemHtml(menuItemsHtml, {
      TabId: CLOSE_CODE,
      TabText: $l.TabStrip.closeTab,
      Icon: ''
    });

    this._getTabMenuItemHtml(menuItemsHtml, {
      TabId: CLOSE_BUT_THIS_CODE,
      TabText: $l.TabStrip.closeAllButThis,
      Icon: ''
    });

    this._getTabMenuItemHtml(menuItemsHtml, {
      TabId: CLOSE_ALL_CODE,
      TabText: $l.TabStrip.closeAllTabs,
      Icon: ''
    });

    menuItemsHtml.cat('<li class="separator"></li>');
    this._getTabMenuItemHtml(menuItemsHtml, {
      TabId: FIND_IN_TREE_CODE,
      TabText: $l.TabStrip.findInTree,
      Icon: ''
    });

    $menu.html(menuItemsHtml.string());
    $('body:first').append($menu);

    this._tabContextMenuComponent = $(this._tabStripElement).jeegoocontext({
      menuElementId: tabContextMenuElementId,
      menuClass: 'contextMenu',
      allowManualShowing: true,
      onSelect: $.proxy(function (e, context) {
        const code = $(e.currentTarget).attr('code');
        const $tab = $(context);
        if (code === CLOSE_CODE) {
          this._closeTabRequest($tab);
        } else if (code === CLOSE_ALL_CODE) {
          this._closeAllTabRequest();
        } else if (code === CLOSE_BUT_THIS_CODE) {
          this._closeButThisTabRequest($tab);
        } else if (code === FIND_IN_TREE_CODE) {
          this._findInTreeRequest($tab);
        }
      }, this)
    }).data(`jeegoocontext_${tabContextMenuElementId}`);
  },

  dispose() {
    Quantumart.QP8.BackendTabStrip.callBaseMethod(this, 'dispose');
    this._detachCloseButtonEventHandlers();
    this._detachTabStripEventHandlers();

    if (this._partialRemovedTabsContainerElement !== null) {
      $(this._partialRemovedTabsContainerElement).empty().remove();
    }

    if (this._tabListElement !== null) {
      $(this._tabListElement).empty().remove();
    }

    if (this._tabStripScrollableElement !== null) {
      $(this._tabStripScrollableElement).empty().remove();
    }

    if (this._tabContextMenuComponent) {
      this._tabContextMenuComponent.dispose();
    }

    $q.dispose.call(this, [
      '_tabGroups',
      '_tabTypeCounters',
      '_partialRemovedTabsContainerElement',
      '_tabListElement',
      '_tabStripScrollableElement',
      '_tabContextMenuComponent',
      '_tabStripElement',
      '_onDocumentBodyClickHandler',
      '_onTabClickingHandler',
      '_onTabMiddleClickHandler',
      '_onContextMenuShowHandler',
      '_onWindowResizedHandler',
      '_onTabStripOverflowEvokedHandler',
      '_onTabStripOverflowPrecludedHandler',
      '_onCloseButtonHoveringHandler',
      '_onCloseButtonClickingHandler',
      '_onCloseButtonClickedHandler',
      '_onCloseButtonUnhoveringHandler',
      '_onTabMenuItemClickingHandler',
      '_onTabMenuButtonHoveredHandler',
      '_onTabMenuButtonUnhoveredHandler',
      '_onTabMenuButtonClickingHandler',
      '_onTabMenuButtonClickedHandler',
      '_onTabMenuUpArrowHoveredHandler',
      '_onTabMenuUpArrowUnhoveredHandler',
      '_onTabMenuDownArrowHoveredHandler',
      '_onTabMenuDownArrowUnhoveredHandler'
    ]);

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendTabStrip._instance = null;
Quantumart.QP8.BackendTabStrip.getInstance = function (tabStripElementId, options) {
  if (Quantumart.QP8.BackendTabStrip._instance === null) {
    const instance = new Quantumart.QP8.BackendTabStrip(tabStripElementId, options);
    Quantumart.QP8.BackendTabStrip._instance = instance;
  }

  return Quantumart.QP8.BackendTabStrip._instance;
};

Quantumart.QP8.BackendTabStrip.registerClass('Quantumart.QP8.BackendTabStrip', Quantumart.QP8.Observable);
Quantumart.QP8.BackendTabEventArgs = function () {
  Quantumart.QP8.BackendTabEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendTabEventArgs.prototype = {
  _tabId: '',
  // eslint-disable-next-line camelcase
  get_tabId() {
    return this._tabId;
  },

  // eslint-disable-next-line camelcase
  set_tabId(value) {
    this._tabId = value;
  }
};

Quantumart.QP8.BackendTabEventArgs.registerClass('Quantumart.QP8.BackendTabEventArgs', Quantumart.QP8.BackendEventArgs);
