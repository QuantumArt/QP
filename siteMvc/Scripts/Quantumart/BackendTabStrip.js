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

  get_tabStripElementId: function () {
    return this._tabStripElementId;
  },

  set_tabStripElementId: function (value) {
    this._tabStripElementId = value;
  },

  get_maxTabTextLength: function () {
    return this._maxTabTextLength;
  },

  set_maxTabTextLength: function (value) {
    this._maxTabTextLength = value;
  },

  get_maxTabMenuHeight: function () {
    return this._maxTabMenuHeight;
  },

  set_maxTabMenuHeight: function (value) {
    this._maxTabMenuHeight = value;
  },

  get_maxTabMenuItemTextLength: function () {
    return this._maxTabMenuItemTextLength;
  },

  set_maxTabMenuItemTextLength: function (value) {
    this._maxTabMenuItemTextLength = value;
  },

  initialize: function () {
    let $tabStrip = $(`#${this._tabStripElementId}`);
    let $scrollable = $('<div />', { class: 'scrollable' });
    let $tabList = $('<ul />', { class: 'tabList' });
    let $partialRemovedTabsContainer = $('<div />', { id: 'partialRemovedTabs', css: { display: 'none'} });

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

  _attachTabStripEventHandlers: function () {
    $(this._tabStripElement)
      .on('click', this.TAB_CLICKABLE_SELECTORS, this._onTabClickingHandler)
      .on('mouseup', this.TAB_CLICKABLE_SELECTORS, this._onTabMiddleClickHandler)
      .on($.fn.jeegoocontext.getContextMenuEventType(), this.TAB_CLICKABLE_SELECTORS, this._onContextMenuShowHandler);
  },

  _detachTabStripEventHandlers: function () {
    $(this._tabStripElement)
      .off('click', this.TAB_CLICKABLE_SELECTORS, this._onTabClickingHandler)
      .off('mouseup', this.TAB_CLICKABLE_SELECTORS, this._onTabMiddleClickHandler)
      .off($.fn.jeegoocontext.getContextMenuEventType(), this.TAB_CLICKABLE_SELECTORS, this._onContextMenuShowHandler);
  },

  fixTabStripWidth: function () {
    let $tabStrip = $(this._tabStripElement);
    let $menuButtonContainer = $(this._tabMenuButtonContainerElement);
    let newScrollableWidth = $tabStrip.width() - $menuButtonContainer.width();
    $(this._tabStripScrollableElement).css('width', `${newScrollableWidth}px`);
  },

  markAsBusy: function () {
    $(this._tabListElement).addClass(this.TAB_STRIP_BUSY_CLASS_NAME);
    $(this._tabMenuItemListElement).addClass(this.TAB_MENU_BUSY_CLASS_NAME);
  },

  unmarkAsBusy: function () {
    $(this._tabListElement).removeClass(this.TAB_STRIP_BUSY_CLASS_NAME);
    $(this._tabMenuItemListElement).removeClass(this.TAB_MENU_BUSY_CLASS_NAME);
  },

  isTabStripOverflow: function () {
    let result = false;
    let $scrollable = $(this._tabStripScrollableElement);
    let tabStripWidth = $scrollable.width();
    let allTabsWidth = 0;
    let $tabs = this.getAllTabs();
    let tabCount = $tabs.length;
    for (let tabIndex = 0; tabIndex < tabCount; tabIndex++) {
      allTabsWidth += $tabs.eq(tabIndex).width();
    }

    if (allTabsWidth > tabStripWidth) {
      result = true;
    }

    return result;
  },

  isTabStripBusy: function () {
    return $(this._tabListElement).hasClass(this.TAB_STRIP_BUSY_CLASS_NAME);
  },

  generateTabGroupCode: function (eventArgs, tabNumber) {
    let associatedAction = $a.getBackendAction(eventArgs.get_actionCode());
    let tabGroupCode = String.format(
      '{0}_{1}',
      associatedAction.Code,
      associatedAction.ActionType.Code === window.ACTION_TYPE_CODE_ADD_NEW ? tabNumber : this._getTabEntityId(eventArgs));

    return tabGroupCode;
  },

  getTabGroup: function (tabGroupCode) {
    if (this._tabGroups[tabGroupCode]) {
      return this._tabGroups[tabGroupCode];
    }

    return null;
  },

  createTabGroup: function (tabGroupCode) {
    let tabGroup = this.getTabGroup(tabGroupCode);
    if (!tabGroup) {
      tabGroup = [];
      this._tabGroups[tabGroupCode] = tabGroup;
    }

    return tabGroup;
  },

  updateTabGroup: function (tabGroupCode, eventArgs) {},
  closeTabGroup: function (tabGroupCode) {
    let tabGroup = this.getTabGroup(tabGroupCode);
    if ($q.isArray(tabGroup)) {
      let tabCount = tabGroup.length;
      for (let tabIndex = 0; tabIndex < tabCount; tabIndex++) {
        let tabId = tabGroup[tabIndex];
        this.closeTab(tabId);
      }
    }
  },

  removeTabGroup: function (tabGroupCode) {
    $q.removeProperty(this._tabGroups, tabGroupCode);
  },

  _removeEmptyTabGroup: function (tabGroupCode) {
    let tabGroup = this.getTabGroup(tabGroupCode);
    if ($q.isArray(tabGroup)) {
      if (tabGroup.length == 0) {
        $q.removeProperty(this._tabGroups, tabGroupCode);
      }
    }
  },

  _addTabToGroup: function (tab, tabGroupCode) {
    let $tab = this.getTab(tab);
    let tabId = this.getTabId($tab);
    let tabGroup = this.createTabGroup(tabGroupCode);

    if ($q.isArray(tabGroup)) {
      if (!Array.contains(tabGroup, tabId)) {
        Array.add(tabGroup, tabId);
      }
    }
  },

  _moveTabToGroup: function (tab, oldTabGroupCode, newTabGroupCode) {
    if (oldTabGroupCode != newTabGroupCode) {
      let $tab = this.getTab(tab);
      let tabId = this.getTabId($tab);
      let oldTabGroup = this.getTabGroup(oldTabGroupCode);
      if ($q.isArray(oldTabGroup)) {
        Array.remove(oldTabGroup, tabId);
        this._removeEmptyTabGroup(oldTabGroupCode);
      }

      this._addTabToGroup(tab, newTabGroupCode);
    }
  },

  _removeTabFromGroup: function (tab) {
    let $tab = this.getTab(tab);
    let tabId = this.getTabId($tab);
    let tabGroupCode = this.getTabGroupCode($tab);
    let tabGroup = this.getTabGroup(tabGroupCode);

    if ($q.isArray(tabGroup)) {
      Array.remove(tabGroup, tabId);
      this._removeEmptyTabGroup(tabGroupCode);
    }
  },

  generateTabId: function () {
    let tabNumber = 1;
    let $tabs = this.getAllTabs();
    if ($tabs.length > 0) {
      let $lastTab = $tabs.last();
      let lastTabId = $lastTab.attr('id');
      let numberMatch = lastTabId.match('[0-9]+');
      if (numberMatch.length == 1) {
        tabNumber = parseInt(numberMatch[0], 10) + 1;
      } else {
        $q.alertError($l.TabStrip.tabIdGenerationErrorMessage);
        return null;
      }
    }

    return String.format('tab{0}', tabNumber);
  },

  generateTabText: function (eventArgs, tabNumber) {
    return Quantumart.QP8.BackendDocumentHost.generateTitle(eventArgs, { isTab: true, tabNumber: tabNumber });
  },

  getAllTabs: function () {
    return $('> LI.tab', this._tabListElement);
  },

  getAllTabsCount: function () {
    return this.getAllTabs().length;
  },

  getTabsByGroupCode: function (tabGroupCode) {
    return $(`> LI[groupCode='${tabGroupCode}'].tab`, this._tabListElement);
  },

  getTabsCountByGroupCode: function (tabGroupCode) {
    return this.getTabsByGroupCode(tabGroupCode).length;
  },

  getTabsByParentEntityTypeCodeAndParentEntityId: function (parentEntityTypeCode, parentEntityId) {
    let foundTabElems = [];
    let $tabs = this.getAllTabs();
    for (let tabIndex = 0, tabCount = $tabs.length; tabIndex < tabCount; tabIndex++) {
      let $tab = $tabs.eq(tabIndex);
      if ($tab.data('parent_entity_type_code') == parentEntityTypeCode && $tab.data('parent_entity_id') == parentEntityId) {
        Array.add(foundTabElems, $tab.get(0));
      }
    }

    return $(foundTabElems);
  },

  getTabsByEventArgs: function (eventArgs) {
    let foundTabElems = [];
    let $tabs = this.getAllTabs();
    for (let tabIndex = 0, tabCount = $tabs.length; tabIndex < tabCount; tabIndex++) {
      let $tab = $tabs.eq(tabIndex);
      if ($tab.data('entity_type_code') == eventArgs.get_entityTypeCode()
          && $tab.data('entity_id') == eventArgs.get_entityId()
          && $tab.data('action_code') == eventArgs.get_actionCode()) {
        Array.add(foundTabElems, $tab.get(0));
      }
    }

    return $(foundTabElems);
  },

  getFirstTabByGroupCode: function (tabGroupCode) {
    let $tab = $(`> LI[groupCode='${tabGroupCode}'].tab:first`, this._tabListElement).eq(0);
    if ($tab.length == 0) {
      $tab = null;
    }

    return $tab;
  },

  getTab: function (tab) {
    let $tab = null;
    if ($q.isObject(tab)) {
      return $q.toJQuery(tab);
    } else if ($q.isString(tab)) {
      $tab = $(this._tabListElement).find(`#${tab}`);
      if ($tab.length == 0) {
        $tab = null;
      }

      return $tab;
    }
  },

  isLastTab: function (tab) {
    let $tab = this.getTab(tab);
    return $tab.next('LI').length == 0;
  },

  getTabId: function (tabElem) {
    if (!$q.isObject(tabElem)) {
      $q.alertError($l.TabStrip.tabNotSpecified);
      return;
    }

    let $tab = $q.toJQuery(tabElem);
    let tabId = '';
    if (!$q.isNullOrEmpty($tab)) {
      tabId = $tab.attr('id');
    }

    return tabId;
  },

  getTabGroupCode: function (tabElem) {
    if (!$q.isObject(tabElem)) {
      $q.alertError($l.TabStrip.tabNotSpecified);
      return;
    }

    let $tab = $q.toJQuery(tabElem);
    let tabValue = '';
    if (!$q.isNullOrEmpty($tab)) {
      tabValue = $tab.attr('groupCode');
    }

    return tabValue;
  },

  getTabText: function (tab) {
    let $tab = this.getTab(tab);
    let tabText = '';
    if (!$q.isNullOrEmpty($tab)) {
      tabText = $q.toString($tab.data('tab_text'), '');
    }

    return tabText;
  },

  setTabText: function (tab, tabText) {
    let $tab = this.getTab(tab);
    if (!$q.isNullOrEmpty($tab)) {
      let processedTabText = $q.toString(tabText, '').trim();
      let truncatedTabText = $q.middleCutShort(processedTabText, this._maxTabTextLength);
      let isTabTextTooLong = processedTabText.length > truncatedTabText.length;
      if (isTabTextTooLong) {
        $tab.attr('title', processedTabText);
      } else {
        $tab.removeAttr('title');
      }

      $tab.data('tab_text', processedTabText).find('SPAN.text').text(truncatedTabText);
    }
  },

  _getTabEntityTypeCode: function (eventArgs) {
    let result;
    let actionTypeCode = eventArgs.get_actionTypeCode();
    if (actionTypeCode == window.ACTION_TYPE_CODE_LIST) {
      result = Quantumart.QP8.BackendEntityType.getParentEntityTypeCodeByCode(eventArgs.get_entityTypeCode());
    } else {
      result = eventArgs.get_entityTypeCode();
    }

    return result;
  },

  _getTabEntityId: function (eventArgs) {
    let result;
    let actionTypeCode = eventArgs.get_actionTypeCode();
    if (actionTypeCode == window.ACTION_TYPE_CODE_LIST) {
      result = eventArgs.get_parentEntityId();
    } else if (eventArgs.get_isMultipleEntities()) {
      let entityIDs = $o.getEntityIDsFromEntities(eventArgs.get_entities());
      result = String.format('{0}_{1}', eventArgs.get_parentEntityId(), entityIDs.join('_and_'));
    } else {
      result = eventArgs.get_entityId();
    }

    return result;
  },

  getExistingTabId: function (eventArgs) {
    let result = 0;
    if (eventArgs.get_actionTypeCode() != window.ACTION_TYPE_CODE_ADD_NEW) {
      let $tab = this.getFirstTabByGroupCode(this.generateTabGroupCode(eventArgs, 0));
      if ($q.isObject($tab)) {
        result = this.getTabId($tab);
      }
    }

    return result;
  },

  addNewTab: function (eventArgs) {
    let associatedAction = $a.getBackendAction(eventArgs.get_actionCode());
    let actionTypeCode = associatedAction.ActionType.Code;
    let tabTypeCode = associatedAction.Code;
    let tabNumber = actionTypeCode == window.ACTION_TYPE_CODE_ADD_NEW ? this._getTabTypeCounter(tabTypeCode) + 1 : 0;
    let tabId = this.generateTabId();
    let tabGroupCode = this.generateTabGroupCode(eventArgs, tabNumber);
    let tabText = this.generateTabText(eventArgs, tabNumber);

    let $tabList = $(this._tabListElement);
    $tabList.append(this._getTabHtml(tabId, tabGroupCode));
    let $tab = this.getTab(tabId);
    this._extendTabElement($tab, eventArgs, { TabGroupCode: tabGroupCode, TabText: tabText, TabTypeCode: tabTypeCode, TabNumber: tabNumber });
    this._addTabToGroup($tab, tabGroupCode);
    $tabList = null;

    if (this.getAllTabsCount() == 1) {
      $(window).resize(this._onWindowResizedHandler);
      this._addTabMenuToTabStrip();
    }

    this._previousSelectedTabId = this._selectedTabId.valueOf();
    this._selectedTabId = tabId;

    if (actionTypeCode == window.ACTION_TYPE_CODE_ADD_NEW) {
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

  updateTab: function (tab, eventArgs) {
    let $tab = this.getTab(tab);

    let oldAction = $a.getBackendActionByCode($tab.data('action_code'));
    let oldActionTypeCode = oldAction.ActionType.Code;
    let oldActionCode = oldAction.Code;
    let oldTabGroupCode = this.getTabGroupCode($tab);
    let oldTabText = this.getTabText($tab);
    let oldTabTypeCode = oldActionCode;
    let oldTabNumber = $tab.data('tab_number');
    if (oldActionTypeCode == window.ACTION_TYPE_CODE_ADD_NEW) {
      this._decreaseTabTypeCounter(oldTabTypeCode, oldTabNumber);
    }

    let newActionCode = eventArgs.get_actionCode();
    let newAction = $a.getBackendAction(newActionCode);
    let newActionTypeCode = newAction.ActionType.Code;
    if (newActionTypeCode == window.ACTION_TYPE_CODE_ADD_NEW) {
      this._increaseTabTypeCounter(newActionCode);
    }
    let newTabNumber = this._getTabTypeCounter(newActionCode);
    let newTabGroupCode = this.generateTabGroupCode(eventArgs, newTabNumber);
    let newTabText = this.generateTabText(eventArgs, newTabNumber);

    $tab.attr('groupCode', newTabGroupCode);
    this._extendTabElement($tab, eventArgs, { TabGroupCode: newTabGroupCode, TabText: newTabText, TabTypeCode: newActionCode, TabNumber: newTabNumber });

    if (newTabGroupCode != oldTabGroupCode) {
      this._moveTabToGroup($tab, oldTabGroupCode, newTabGroupCode);
    }
  },

  selfUpdateTab: function (tab) {
    this.updateTab(tab, this.getEventArgsFromTab(tab));
  },

  selectTab: function (tab) {
    let $tab = this.getTab(tab);

    // Запоминаем код выбранного таба
    this._previousSelectedTabId = this._selectedTabId.valueOf();
    this._selectedTabId = this.getTabId($tab);

    if (this._selectedTabId != this._previousSelectedTabId) {
      // Оповещаем о начале выбора компоненты-наблюдатели

      // Выделяем таб
      this.highlightTab($tab);

      // Устанавливаем ширину группы табов
      this.fixTabStripWidth();
    }
  },

  selectTabRequest: function (tab) {
    let eventArgs = this.getEventArgsFromTab(tab);
    if ($q.isObject(eventArgs)) {
      this.notify(window.EVENT_TYPE_TAB_STRIP_TAB_SELECT_REQUEST, eventArgs);
    }
    eventArgs = null;
  },

  highlightTab: function (tab) {
    let $tab = this.getTab(tab);
    let $tabs = this.getAllTabs();

    // Снимаем выделение со всех табов
    $tabs.removeClass(this.TAB_SELECTED_CLASS_NAME);

    // Выделяем заданный таб
    $tab.addClass(this.TAB_SELECTED_CLASS_NAME);

    // Прокручиваем группу табов до выделяемого таба
    this.scrollToTab($tab);
  },

  scrollToTab: function (tab, duration) {
    let self = this;

    let $tab = this.getTab(tab);
    if ($tab) {
      let $scrollable = $(this._tabStripScrollableElement);
      let tabStripWidth = $scrollable.width();
      let tabWidth = $tab.width();
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

  closeTab: function (tab) {
    let $tab = this.getTab(tab);
    if (!$q.isNullOrEmpty($tab)) {
      let tabId = this.getTabId($tab);
      let tabTypeCode = $tab.data('tab_type_code');
      let tabNumber = $tab.data('tab_number');

      this._partialRemoveTab($tab);

      if (!this.isTabStripOverflow()) {
        this._onTabStripOverflowPrecludedHandler(this, null);
      }

      this._decreaseTabTypeCounter(tabTypeCode, tabNumber);
      if (this.getAllTabsCount() == 0) {
        this._hideTabMenu();
        this._removeTabMenuFromTabStrip();
        $(window).unbind('resize', this._onWindowResizedHandler);
      }

      this._removeTab($tab);
    }

    $tab = null;
  },

  _partialRemoveTab: function (tab) {
    let $tab = this.getTab(tab);
    let $partialRemovedTabsContainer = $(this._partialRemovedTabsContainerElement);

    $tab.appendTo($partialRemovedTabsContainer);
  },

  _removeTab: function (tab) {
    let $tab = this.getTab(tab);
    $tab
      .removeData()
      .empty()
      .remove()
    ;

    this._removeTabFromGroup($tab);
  },

  _getTabHtml: function (tabId, tabGroupCode) {
    let html = new $.telerik.stringBuilder();
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

  _extendTabElement: function (tabElem, eventArgs, params) {
    let $tab = this.getTab(tabElem);
    $tab.data('entity_type_code', eventArgs.get_entityTypeCode());
    $tab.data('entity_id', eventArgs.get_entityId());
    $tab.data('entity_name', eventArgs.get_entityName());
    $tab.data('parent_entity_type_code', Quantumart.QP8.BackendEntityType.getParentEntityTypeCodeByCode(eventArgs.get_entityTypeCode()));
    $tab.data('parent_entity_id', eventArgs.get_parentEntityId());
    $tab.data('action_type_code', eventArgs.get_actionTypeCode());
    $tab.data('action_code', eventArgs.get_actionCode());
    $tab.data('is_multiple_entities', eventArgs.get_isMultipleEntities());
    $tab.data('entities', eventArgs.get_entities());
    $tab.data('tab_type_code', params.TabTypeCode);
    $tab.data('tab_number', params.TabNumber);
    this.setTabText($tab, params.TabText);
  },

  _getCloseButton: function (tab) {
    return this.getTab(tab).find('> a.tabLink > span.wrapper > span.closeButton');
  },

  _attachCloseButtonEventHandlers: function () {
    let $tabStrip = $(this._tabStripElement);
    $tabStrip
      .on('mouseover', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonHoveringHandler)
      .on('mousedown', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonClickingHandler)
      .on('mouseup', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonClickedHandler)
      .on('mouseout', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonUnhoveringHandler);
  },

  _detachCloseButtonEventHandlers: function () {
    let $tabStrip = $(this._tabStripElement);
    $tabStrip
      .off('mouseover', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonHoveringHandler)
      .off('mousedown', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonClickingHandler)
      .off('mouseup', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonClickedHandler)
      .off('mouseout', this.CLOSE_BUTTON_CLICKABLE_SELECTORS, this._onCloseButtonUnhoveringHandler);
  },

  _applyHoveredStyleToCloseButton: function (closeButtonElem) {
    $('img', closeButtonElem).removeClass('clicked').addClass('hover');
  },

  _applyClickedStyleToCloseButton: function (closeButtonElem) {
    $('img', closeButtonElem).removeClass('hover').addClass('clicked');
  },

  _cancelAllStylesForCloseButton: function (closeButtonElem) {
    $('img', closeButtonElem).removeClass('hover').removeClass('clicked');
  },

  isTabSelected: function (tab) {
    let $tab = this.getTab(tab);
    return this.getTabId($tab) === this._selectedTabId;
  },

  _getTabTypeCounter: function (tabTypeCode) {
    let tabTypeCount = 0;
    if (this._tabTypeCounters[tabTypeCode]) {
      tabTypeCount = this._tabTypeCounters[tabTypeCode];
    }

    return tabTypeCount;
  },

  _increaseTabTypeCounter: function (tabTypeCode) {
    if (this._tabTypeCounters[tabTypeCode]) {
      this._tabTypeCounters[tabTypeCode] += 1;
    } else {
      this._tabTypeCounters[tabTypeCode] = 1;
    }
  },

  _decreaseTabTypeCounter: function (tabTypeCode, tabNumber) {
    if (this._tabTypeCounters[tabTypeCode]) {
      let $tabs = this.getAllTabs();
      let tabCount = $tabs.length;
      let matchedTabCount = 0;
      for (let tabIndex = 0; tabIndex < tabCount; tabIndex++) {
        if ($tabs.eq(tabIndex).data('tab_type_code') == tabTypeCode) {
          matchedTabCount += 1;
        }
      }

      if (matchedTabCount != 0) {
        if (this._getTabTypeCounter(tabTypeCode) == tabNumber) {
          this._tabTypeCounters[tabTypeCode] -= 1;
        }
      } else {
        this._tabTypeCounters[tabTypeCode] = 0;
      }
    }
  },

  _isTabMenuOverflow: function () {
    let menuItemListHeight = $(this._tabMenuItemListElement).outerHeight();
    let scrollableHeight = $(this._tabMenuScrollableElement).outerHeight();
    return (menuItemListHeight - scrollableHeight) > 1;
  },

  _isTabMenuTopOverflow: function () {
    return $(this._tabMenuScrollableElement).scrollTop() > 0;
  },

  _isTabMenuBottomOverflow: function () {
    let $scrollable = $(this._tabMenuScrollableElement);
    let menuItemListHeight = $(this._tabMenuItemListElement).height();
    let scrollableHeight = $scrollable.height();
    let scrollableTopPosition = $scrollable.scrollTop();

    return scrollableTopPosition < (menuItemListHeight - scrollableHeight);
  },

  _calculateTabMenuScrollingDuration: function (isUpScrolling) {
    let duration = 0;
    let $scrollable = $(this._tabMenuScrollableElement);
    let menuItemListHeight = $(this._tabMenuItemListElement).height();
    let scrollableHeight = $scrollable.height();
    let scrollableTopPosition = $scrollable.scrollTop();
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

  _addTabMenuToTabStrip: function () {
    let $tabStrip = $(this._tabStripElement);
    if (this._tabMenuButtonContainerElement == null) {
      let $menuButtonContainer = $('<div />', { class: 'tabMenuButton' });
      let $menuButton = $('<span />');
      let $menuButtonImage = $('<img />', {
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

    if (this._tabMenuElement == null) {
      let $menu = $('<div />', { class: 'tabMenu' });
      let $upArrow = $('<div />', { class: this.TAB_MENU_UP_ARROW_CLASS_NAME, css: { display: 'none'} });
      $menu.append($upArrow);

      let $scrollable = $('<div />', { class: 'scrollable' });
      $menu.append($scrollable);

      let $menuItemList = $('<ul />');
      $scrollable.append($menuItemList);

      let $downArrow = $('<div />', { class: this.TAB_MENU_DOWN_ARROW_CLASS_NAME, css: { display: 'none'} });
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

  _removeTabMenuFromTabStrip: function () {
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

  _attachTabMenuEventHandlers: function () {
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

  _detachTabMenuEventHandlers: function () {
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

  _getMenuItems: function () {
    return $('li.item', this._tabMenuItemListElement);
  },

  _changeTabStripOverflowIndicator: function (isOverflow) {
    let $menuButtonContainer = $(this._tabMenuButtonContainerElement);
    if (isOverflow) {
      $menuButtonContainer.removeClass('tabMenuButton').addClass('tabOverflowMenuButton');
    } else {
      $menuButtonContainer.removeClass('tabOverflowMenuButton').addClass('tabMenuButton');
    }
  },

  _loadItemsToTabMenu: function () {
    this._removeItemsFromTabMenu();
    let $menuList = $(this._tabMenuItemListElement);
    let $tabs = this.getAllTabs();
    let tabCount = $tabs.length;
    let menuItemsHtml = new $.telerik.stringBuilder();
    for (let tabIndex = 0; tabIndex < tabCount; tabIndex++) {
      let $tab = $tabs.eq(tabIndex);
      if ($tab) {
        let dataItem = {
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

  _getTabMenuItemHtml: function (html, dataItem) {
    let processedTabText = $q.toString(dataItem.TabText, '').trim();
    let truncatedTabText = $q.middleCutShort(processedTabText, this._maxTabMenuItemTextLength);
    let isTabTextTooLong = processedTabText.length > truncatedTabText.length;
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
      .catIf(` style="background-image: url(${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS}${dataItem.Icon})"`, !$q.isNullOrWhiteSpace(dataItem.Icon))
      .cat('>')
      .cat(`<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}/0.gif" width="16px" height="16px" />`)
      .cat('</span>\n')
      .cat(`          <span class="text">${text}</span>\n`)
      .cat('      </div>\n')
      .cat('  </div>\n')
      .cat('</li>\n');

    return html;
  },

  _removeItemsFromTabMenu: function () {
    $(this._tabMenuItemListElement).empty();
  },

  _toggleTabMenu: function () {
    if ($(this._tabMenuElement).is(':hidden')) {
      this._showTabMenu();
    } else {
      this._hideTabMenu();
    }
  },

  _showTabMenu: function () {
    this._tabContextMenuComponent.hide();
    let $menu = $(this._tabMenuElement);
    $menu.css('height', 'auto');
    this._loadItemsToTabMenu();

    let $menuButton = $(this._tabMenuButtonElement);
    let menuWidth = $menu.outerWidth();
    let menuHeight = $menu.outerHeight();
    let menuTop = $menuButton.offset().top + $menuButton.height() + $menuButton.borderTopWidth() + $menuButton.borderBottomWidth();
    let menuRight = $(window).width() - $menuButton.offset().left - $menuButton.outerWidth();

    let $scrollable = $(this._tabMenuScrollableElement);
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

  _hideTabMenu: function () {
    let $menu = $(this._tabMenuElement);
    let $upArrow = $(this._tabMenuUpArrowButtonElement);
    let $downArrow = $(this._tabMenuDownArrowButtonElement);

    $menu.stopTime(this.TAB_MENU_TIMER_ID);
    $upArrow.fadeOut(50);
    $downArrow.fadeOut(50);

    $menu.fadeOut(200, () => {
      if (this._isTabMenuOverflow()) {
        this._scrollTabMenuToFirstItem(0);
      }
    });

    let $menuButton = $(this._tabMenuButtonElement);
    this._cancelAllStylesForTabMenuButton($menuButton);

    $menuButton
      .bind('mouseover', this._onTabMenuButtonHoveredHandler)
      .bind('mouseout', this._onTabMenuButtonUnhoveredHandler);

    this._removeItemsFromTabMenu();
  },

  _refreshTabMenuArrowButtons: function () {
    let $upArrow = $(this._tabMenuUpArrowButtonElement);
    let $downArrow = $(this._tabMenuDownArrowButtonElement);
    let isOverflow = this._isTabMenuOverflow();

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

  _scrollTabMenuToFirstItem: function (duration) {
    if (duration == null) {
      duration = this._calculateTabMenuScrollingDuration(true);
    }

    let $scrollable = $(this._tabMenuScrollableElement);
    let $firstMenuItem = this._getMenuItems().first();
    let options = { axis: 'y', easing: 'swing' };
    options.duration = duration;
    $scrollable.scrollTo($firstMenuItem, options);
  },

  _scrollTabMenuToLastItem: function (duration) {
    if (duration == null) {
      duration = this._calculateTabMenuScrollingDuration(false);
    }

    let $lastMenuItem = this._getMenuItems().last();
    let options = { axis: 'y', easing: 'swing' };
    options.duration = duration;
    $(this._tabMenuScrollableElement).scrollTo($lastMenuItem, options);
  },

  _stopTabMenuScrolling: function () {
    $(this._tabMenuScrollableElement).stop();
  },

  _attachTabMenuButtonEventHandlers: function () {
    $(this._tabMenuButtonElement)
      .bind('mouseover', this._onTabMenuButtonHoveredHandler)
      .bind('mousedown', this._onTabMenuButtonClickingHandler)
      .bind('click', this._onTabMenuButtonClickedHandler)
      .bind('mouseout', this._onTabMenuButtonUnhoveredHandler);
  },

  _detachTabMenuButtonEventHandlers: function () {
    $(this._tabMenuButtonElement)
      .unbind('mouseover', this._onTabMenuButtonHoveredHandler)
      .unbind('mousedown', this._onTabMenuButtonClickingHandler)
      .unbind('click', this._onTabMenuButtonClickedHandler)
      .unbind('mouseout', this._onTabMenuButtonUnhoveredHandler);
  },

  _applyHoveredStyleToTabMenuButton: function (buttonElem) {
    $(buttonElem).addClass('hover');
  },

  _applyClickedStyleToTabMenuButton: function (buttonElem) {
    $(buttonElem).removeClass('hover').addClass('clicked');
  },

  _cancelAllStylesForTabMenuButton: function (buttonElem) {
    $(buttonElem).removeClass('hover').removeClass('clicked');
  },

  isTabMenuBusy: function () {
    return $(this._tabMenuItemListElement).hasClass(this.TAB_MENU_BUSY_CLASS_NAME);
  },

  getEventArgsFromTab: function (tab) {
    let $tab = this.getTab(tab);
    let actionCode = $tab.data('action_code');
    let action = $a.getBackendActionByCode(actionCode);
    let eventArgs = new Quantumart.QP8.BackendTabEventArgs();
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

  updateParentInfo: function (entityTypeCode, entityId) {
    let $tabs = this.getTabsByParentEntityTypeCodeAndParentEntityId(entityTypeCode, entityId);
    for (let tabIndex = 0, tabCount = $tabs.length; tabIndex < tabCount; tabIndex++) {
      let $tab = $tabs.eq(tabIndex);
      this.selfUpdateTab($tab);
    }
  },

  tabEntityExists: function ($tab) {
    let result = true;
    let actionTypeCode = $tab.data('action_type_code');
    if (actionTypeCode != window.ACTION_TYPE_CODE_ADD_NEW && actionTypeCode != window.ACTION_TYPE_CODE_MULTIPLE_SELECT) {
      let eventArgs = this.getEventArgsFromTab($tab);
      let tabEntityTypeCode = this._getTabEntityTypeCode(eventArgs);
      if ($q.toBoolean($tab.data('is_multiple_entities'), false)) {
        let entities = $tab.data('entities');
        for (let entityIndex = 0; entityIndex < entities.length; entityIndex++) {
          let entity = entities[entityIndex];
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

  getAnotherTabToSelect: function ($tab) {
    let $tabToSelect = null;
    let isSelected = this.isTabSelected($tab);

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

  _onDocumentBodyClick: function (e) {
    this._hideTabMenu();
  },

  _onTabClicking: function (e) {
    let $tab = $(e.currentTarget);
    if (!$tab.hasClass(this.TAB_DISABLED_CLASS_NAME)
      && !this.isTabSelected($tab)
      && !this.isTabStripBusy()) {
      this.selectTabRequest($tab);
    } else {
      e.preventDefault();
    }
  },

  _onTabMiddleClick: function (e) {
    if (e.which === 2) {
      this._tabContextMenuComponent.hide(e);
      this._closeTabRequest($(e.currentTarget));
      e.preventDefault();
      e.stopPropagation();
    }
  },

  _onContextMenuShow: function (e) {
    this._tabContextMenuComponent.show(e, e.currentTarget);
    e.preventDefault();
  },

  _onWindowResized: function () {
    this.fixTabStripWidth();
  },

  _onTabStripOverflowEvoked: function (sender, args) {
    this._changeTabStripOverflowIndicator(true);
  },

  _onTabStripOverflowPrecluded: function (sender, args) {
    this._changeTabStripOverflowIndicator(false);
  },

  _onCloseButtonHovering: function (e) {
    this._applyHoveredStyleToCloseButton(e.currentTarget);
  },

  _onCloseButtonClicking: function (e) {
    if (!this.isTabStripBusy()) {
      this._applyClickedStyleToCloseButton(e.currentTarget);
    } else {
      e.preventDefault();
    }
  },

  _onCloseButtonClicked: function (e) {
    if (!this.isTabStripBusy()) {
      this._tabContextMenuComponent.hide(e);

      let $tab = $(e.currentTarget).parent().parent().parent();
      this._closeTabRequest($tab);
      this._applyHoveredStyleToCloseButton(e.currentTarget);

      e.stopPropagation();
    } else {
      e.preventDefault();
    }
  },

  _closeTabRequest: function (tab) {
    if (!this.isTabStripBusy()) {
      let eventArgs = this.getEventArgsFromTab(tab);
      if ($q.isObject(eventArgs)) {
        this.notify(window.EVENT_TYPE_TAB_STRIP_TAB_CLOSE_REQUEST, eventArgs);
      }
      eventArgs = null;
    }
  },

  _closeAllTabRequest: function () {
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

  _saveAndCloseAllTabRequest: function () {
    if (!this.isTabStripBusy()) {
      $(this._tabStripElement)
        .find(this.TAB_CLICKABLE_SELECTORS)
        .each($.proxy(
          function (i, tab) {
            let eventArgs = this.getEventArgsFromTab(tab);
            if ($q.isObject(eventArgs)) {
              this.notify(window.EVENT_TYPE_TAB_STRIP_TAB_SAVE_CLOSE_REQUEST, eventArgs);
            }
            eventArgs = null;
          }, this)
        );
    }
  },

  _closeButThisTabRequest: function ($tab) {
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

  _findInTreeRequest: function (tab) {
    let eventArgs = this.getEventArgsFromTab(tab);
    if ($q.isObject(eventArgs)) {
      eventArgs.isExpandRequested = true;
      this.notify(window.EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST, eventArgs);
    }
    eventArgs = null;
  },

  _onCloseButtonUnhovering: function (e) {
    this._cancelAllStylesForCloseButton(e.currentTarget);
  },

  _onTabMenuItemClicking: function (e) {
    if (!this.isTabMenuBusy()) {
      let $menuItem = $(e.currentTarget);
      let tabId = $menuItem.attr('code');
      if (tabId === this.TAB_MENU_CLOSE_ALL_ITEM_CODE) {
        this._closeAllTabRequest();
      } else if (tabId === this.TAB_MENU_SAVE_CLOSE_ALL_ITEM_CODE) {
        this._saveAndCloseAllTabRequest();
      } else {
        this.selectTabRequest(tabId);
      }
      this._hideTabMenu();
    } else {
      e.preventDefault();
    }
  },

  _onTabMenuButtonHovered: function (e) {
    this._applyHoveredStyleToTabMenuButton(e.currentTarget);
  },

  _onTabMenuButtonUnhovered: function (e) {
    this._cancelAllStylesForTabMenuButton(e.currentTarget);
  },

  _onTabMenuButtonClicking: function (e) {
    if (!this.isTabMenuBusy()) {
      this._applyClickedStyleToTabMenuButton(e.currentTarget);

      e.stopPropagation();
    } else {
      e.preventDefault();
    }
  },

  _onTabMenuButtonClicked: function (e) {
    if (!this.isTabMenuBusy()) {
      let $menuButton = $(e.currentTarget);
      $menuButton
        .unbind('mouseover', this._onTabMenuButtonHoveredHandler)
        .unbind('mouseout', this._onTabMenuButtonUnhoveredHandler)
      ;

      $menuButton = null;

      this._toggleTabMenu();

      e.stopPropagation();
    } else {
      e.preventDefault();
    }
  },

  _onTabMenuUpArrowHovered: function (e) {
    let $upArrow = $(e.currentTarget);
    $upArrow
      .removeClass(this.TAB_MENU_UP_ARROW_CLASS_NAME)
      .addClass(this.TAB_MENU_UP_ARROW_HOVER_CLASS_NAME)
    ;

    this._scrollTabMenuToFirstItem();

    $upArrow = null;
  },

  _onTabMenuUpArrowUnhovered: function (e) {
    let $upArrow = $(e.currentTarget);
    $upArrow
      .removeClass(this.TAB_MENU_UP_ARROW_HOVER_CLASS_NAME)
      .addClass(this.TAB_MENU_UP_ARROW_CLASS_NAME)
    ;

    this._stopTabMenuScrolling();

    $upArrow = null;
  },

  _onTabMenuDownArrowHovered: function (e) {
    let $downArrow = $(e.currentTarget);
    $downArrow
      .removeClass(this.TAB_MENU_DOWN_ARROW_CLASS_NAME)
      .addClass(this.TAB_MENU_DOWN_ARROW_HOVER_CLASS_NAME)
    ;

    this._scrollTabMenuToLastItem();

    $downArrow = null;
  },

  _onTabMenuDownArrowUnhovered: function (e) {
    let $downArrow = $(e.currentTarget);
    $downArrow
      .removeClass(this.TAB_MENU_DOWN_ARROW_HOVER_CLASS_NAME)
      .addClass(this.TAB_MENU_DOWN_ARROW_CLASS_NAME)
    ;

    this._stopTabMenuScrolling();

    $downArrow = null;
  },

  _createTabContextMenu: function () {
    let CLOSE_CODE = 'close';
    let CLOSE_BUT_THIS_CODE = 'close_but_this';
    let CLOSE_ALL_CODE = 'close_all';
    let FIND_IN_TREE_CODE = 'find_in_tree';

    let tabContextMenuElementId = `${this._tabStripElementId}_tabContextMenu`;
    let $menu = $('<ul />', { id: tabContextMenuElementId, class: 'contextMenu' });
    let menuItemsHtml = new $.telerik.stringBuilder();
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
        let code = $(e.currentTarget).attr('code');
        let $tab = $(context);
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

  dispose: function () {
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
  if (Quantumart.QP8.BackendTabStrip._instance == null) {
    let instance = new Quantumart.QP8.BackendTabStrip(tabStripElementId, options);
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
  get_tabId: function () {
    return this._tabId;
  },
  set_tabId: function (value) {
    this._tabId = value;
  }
};

Quantumart.QP8.BackendTabEventArgs.registerClass('Quantumart.QP8.BackendTabEventArgs', Quantumart.QP8.BackendEventArgs);
