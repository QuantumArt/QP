/* eslint-disable max-lines */
import { Observable } from '../Common/Observable';
import { $q } from '../Utils';


window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKING = 'OnToolbarButtonClicking';
window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKED = 'OnToolbarButtonClicked';
window.EVENT_TYPE_TOOLBAR_DROPDOWN_SELECTED_INDEX_CHANGING = 'OnToolbarDropDownListSelectedIndexChanging';
window.EVENT_TYPE_TOOLBAR_DROPDOWN_SELECTED_INDEX_CHANGED = 'OnToolbarDropDownListSelectedIndexChanged';
window.TOOLBAR_ITEM_TYPE_BUTTON = 'button';
window.TOOLBAR_ITEM_TYPE_DROPDOWN = 'drop_down';

export class BackendToolbar extends Observable {
  constructor(toolbarElementId, options) {
    super();

    this._toolbarElementId = toolbarElementId;
    if ($q.isObject(options)) {
      if (options.toolbarContainerElementId) {
        this._toolbarContainerElementId = options.toolbarContainerElementId;
      }
    }

    this._onToolbarButtonUnhovering = this._onToolbarButtonUnhovering.bind(this);
    this._onToolbarButtonClicking = this._onToolbarButtonClicking.bind(this);
    this._onToolbarButtonClicked = this._onToolbarButtonClicked.bind(this);
    this._onToolbarDropDownArrowUnhovering = this._onToolbarDropDownArrowUnhovering.bind(this);
    this._onToolbarDropDownArrowClicking = this._onToolbarDropDownArrowClicking.bind(this);
    this._onToolbarDropDownArrowClicked = this._onToolbarDropDownArrowClicked.bind(this);
    this._onToolbarDropDownButtonUnhovering = this._onToolbarDropDownButtonUnhovering.bind(this);
    this._onToolbarDropDownButtonClicking = this._onToolbarDropDownButtonClicking.bind(this);
    this._onToolbarDropDownButtonClicked = this._onToolbarDropDownButtonClicked.bind(this);
    this._onToolbarDropDownListItemClicking = this._onToolbarDropDownListItemClicking.bind(this);
    this._onToolbarDropDownListItemClicked = this._onToolbarDropDownListItemClicked.bind(this);
  }

  _toolbarElementId = '';
  _toolbarElement = null;
  _toolbarItemListElement = null;
  _toolbarContainerElementId = '';

  ITEM_DISABLED_CLASS_NAME = 'disabled';
  ITEM_CHECKED_CLASS_NAME = 'checked';
  ITEM_BUSY_CLASS_NAME = 'busy';
  DROPDOWN_LIST_ITEM_SELECTED_CLASS_NAME = 'selected';

  BUTTON_CLICKABLE_SELECTORS = '.toolbar > UL > LI.button';
  DROPDOWN_ARROW_CLICKABLE_SELECTORS = '.toolbar > UL > LI.dropDown SPAN.arrow';
  DROPDOWN_BUTTON_CLICKABLE_SELECTORS = '.toolbar > UL > LI.dropDown SPAN.button';
  DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS = '.toolbar > UL > LI.dropDown .list UL LI.item';

  _isBindToExternal = false;

  // eslint-disable-next-line camelcase
  get_toolbarElementId() {
    return this._toolbarElementId;
  }

  // eslint-disable-next-line camelcase
  set_toolbarElementId(value) {
    this._toolbarElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_toolbarElement() {
    return this._toolbarElement;
  }

  // eslint-disable-next-line camelcase
  get_toolbarContainerElementId() {
    return this._toolbarContainerElementId;
  }

  // eslint-disable-next-line camelcase
  set_toolbarContainerElementId(value) {
    this._toolbarContainerElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_isBindToExternal() {
    return this._isBindToExternal;
  }

  // eslint-disable-next-line camelcase
  set_isBindToExternal(value) {
    this._isBindToExternal = value;
  }

  initialize() {
    let $toolbar = $(`#${this._toolbarElementId}`);
    let $toolbarItemList = null;
    const wasToolbarExist = !!$toolbar.length;
    if (!wasToolbarExist) {
      $toolbar = $('<div />', { id: this._toolbarElementId, class: 'toolbar', css: { display: 'none' } });
    }

    $toolbarItemList = $toolbar.find('UL:first');
    if ($q.isNullOrEmpty($toolbarItemList)) {
      $toolbarItemList = $('<ul />');
    }

    if (!wasToolbarExist) {
      $toolbar.append($toolbarItemList);
      if ($q.isNullOrWhiteSpace(this._toolbarContainerElementId)) {
        $('BODY:first').append($toolbar);
      } else {
        $(`#${this._toolbarContainerElementId}`).append($toolbar);
      }
    }

    this._toolbarElement = $toolbar.get(0);
    this._toolbarItemListElement = $toolbarItemList.get(0);

    if (!wasToolbarExist) {
      this._attachToolbarEventHandlers();
    }
  }

  showToolbar(callback) {
    $(this._toolbarElement).show();
    $q.callFunction(callback);
  }

  hideToolbar(callback) {
    $(this._toolbarElement).hide();
    $q.callFunction(callback);
  }

  markToolbarAsBusy() {
    $(this._toolbarItemListElement).addClass(this.ITEM_BUSY_CLASS_NAME);
  }

  unmarkToolbarAsBusy() {
    $(this._toolbarItemListElement).removeClass(this.ITEM_BUSY_CLASS_NAME);
  }

  isToolbarBusy() {
    return $(this._toolbarItemListElement).hasClass(this.ITEM_BUSY_CLASS_NAME);
  }

  getToolbarItems() {
    return $('> LI.item', this._toolbarItemListElement);
  }

  getToolbarItem(item) {
    let $item = null;
    if ($q.isObject(item)) {
      return $q.toJQuery(item);
    } else if ($q.isString(item)) {
      $item = $(`LI[code='${item}']`, this._toolbarItemListElement);
      if ($item.length === 0) {
        $item = null;
      }

      return $item;
    }

    return undefined;
  }

  _getToolbarItemByDropDownListItem(listItemElem) {
    const $listItem = this._getToolbarDropDownListItem(listItemElem);
    let $item = null;

    if ($listItem) {
      $item = $listItem.parent().parent().parent();
      if ($item.length === 0) {
        $item = null;
      }
    }

    return $item;
  }

  getToolbarItemValue(itemElem) {
    const $item = this.getToolbarItem(itemElem);
    let itemValue = '';

    if ($item && $item.length) {
      itemValue = $item.attr('code');
    } else {
      $q.alertFail($l.Toolbar.itemNotSpecified);
      throw new Error($l.Toolbar.itemNotSpecified);
    }

    return itemValue;
  }

  getToolbarItemText(item) {
    const $item = this.getToolbarItem(item);
    let itemText = '';

    if ($item && $item.length) {
      itemText = $('SPAN.text', $item).text();
    }

    return itemText;
  }

  _getToolbarItemsHtml(items) {
    const itemsHtml = new $.telerik.stringBuilder();
    for (let itemIndex = 0; itemIndex < items.length; itemIndex++) {
      const item = items[itemIndex];
      if (item.Type === window.TOOLBAR_ITEM_TYPE_BUTTON) {
        this._getToolbarButtonHtml(itemsHtml, item);
      } else if (item.Type === window.TOOLBAR_ITEM_TYPE_DROPDOWN) {
        this._getToolbarDropDownHtml(itemsHtml, item);
      }
    }

    return itemsHtml.string();
  }

  addToolbarItemsToToolbar(items, count) {
    if (!$q.isNull(items)) {
      $(this._toolbarItemListElement).html(this._getToolbarItemsHtml(items));
      this._extendToolbarItemElements(items);
      this.refreshToolbarItems(count);
    }
  }

  tuneToolbarItems(tbStatuses) {
    for (let statusIndex = 0; statusIndex < tbStatuses.length; statusIndex++) {
      const tbStatus = tbStatuses[statusIndex];
      const $item = this.getToolbarItem(tbStatus.Code);
      if ($item && $item.length) {
        this.setVisibleState($item, tbStatus.Visible);
      }
    }
  }

  removeToolbarItemsFromToolbar() {
    const $toolbarItemList = $(this._toolbarItemListElement);
    $toolbarItemList.empty();
  }

  refreshToolbarItems(selectedEntitiesCount) {
    const that = this;
    const $items = this.getToolbarItems();
    const entitiesCount = +selectedEntitiesCount || 0;
    $items.each((index, elem) => {
      const $item = $(elem);
      const itemsAffected = +$item.data('items_affected') || 0;
      const state = entitiesCount === itemsAffected
        || (itemsAffected === window.MAX_ITEMS_AFFECTED_NUMBER && entitiesCount >= 1);

      that.setEnableState($item, state);
    });
  }

  _getToolbarDropDownList(item) {
    const $item = this.getToolbarItem(item);
    return $item ? $item.find('.list:first') : undefined;
  }

  _updateDropDownListButton(item, listItemElem) {
    const $item = this.getToolbarItem(item);
    const $listItem = this._getToolbarDropDownListItem(listItemElem);
    if ($item && $listItem) {
      const $icon = $item.find('SPAN.button > SPAN.icon');
      const $text = $item.find('SPAN.button > SPAN.text');
      const icon = $listItem.data('icon');
      const text = this._getToolbarDropDownListItemText($listItem);
      const showButtonText = $item.data('show_button_text');

      $listItem
        .siblings()
        .removeClass(this.DROPDOWN_LIST_ITEM_SELECTED_CLASS_NAME)
        .end()
        .addClass(this.DROPDOWN_LIST_ITEM_SELECTED_CLASS_NAME);

      $item.data(
        'selected_sub_item_value',
        this._getToolbarDropDownListItemValue($listItem)
      );

      $icon.css('backgroundImage', String.format("url('{0}')", window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + icon));
      if (text && showButtonText) {
        $text.text(text);
      }
    }
  }

  _getToolbarDropDownListItem(listItemElem) {
    let $listItem = null;
    if ($q.isObject(listItemElem)) {
      $listItem = $q.toJQuery(listItemElem);
      if (!$listItem.length) {
        $listItem = null;
      }
    }

    return $listItem;
  }

  _getToolbarDropDownListNextItem(listItemElem) {
    const $listItem = this._getToolbarDropDownListItem(listItemElem);
    let $nextListItem = null;

    if ($listItem) {
      $nextListItem = $listItem.next();
      if ($q.isNullOrEmpty($nextListItem)) {
        $nextListItem = $listItem
          .siblings()
          .first();
      }

      if (!$nextListItem.length) {
        $nextListItem = null;
      }
    }

    return $nextListItem;
  }

  _getToolbarDropDownListItemByValue(item, listItemValue) {
    const $item = this.getToolbarItem(item);
    let $listItem = null;
    if ($item && $item.length) {
      const $list = this._getToolbarDropDownList($item);
      if ($list.length) {
        $listItem = $item.find(`LI[code='${listItemValue}']`);
        if (!$listItem.length) {
          $listItem = null;
        }
      }
    }

    return $listItem;
  }

  _getToolbarDropDownListItemValue(listItemElem) {
    const $listItem = this._getToolbarDropDownListItem(listItemElem);
    return $listItem ? $listItem.attr('code') : '';
  }

  _getToolbarDropDownListItemText(listItemElem) {
    const $listItem = this._getToolbarDropDownListItem(listItemElem);
    return $listItem ? $listItem.find('SPAN.text').text() : '';
  }

  _toggleDropDownList(item) {
    const $item = this.getToolbarItem(item);
    if ($item && $item.length) {
      if (this._isDropDownListVisible($item)) {
        this._hideDropDownList($item);
      } else {
        this._showDropDownList($item);
      }
    }
  }

  _showDropDownList(item) {
    const $item = this.getToolbarItem(item);
    if ($item && $item.length) {
      const $list = this._getToolbarDropDownList($item);
      if ($list.length) {
        const windowWidth = $(window).width();
        const listWidth = $list.outerWidth();
        const listTop = $item.offset().top + $item.height() + $item.borderTopWidth() + $item.borderBottomWidth() - 1;
        let listLeft = $item.offset().left;
        if ((listLeft + listWidth) > windowWidth) {
          listLeft = windowWidth - listWidth;
        }

        $list.css('top', `${listTop}px`);
        $list.css('left', `${listLeft}px`);
        $list.show();
      }
    }
  }

  _hideDropDownList(item) {
    const $item = this.getToolbarItem(item);
    if ($item && $item.length) {
      const $list = this._getToolbarDropDownList($item);
      if ($list) {
        $list.hide();
      }

      this._cancelClickedStyleToToolbarDropDown($item);
    }
  }

  _isDropDownListVisible(item) {
    const $item = this.getToolbarItem(item);
    let isVisible = false;
    if ($item && $item.length) {
      const $list = this._getToolbarDropDownList($item);
      if ($list) {
        isVisible = $list.is(':visible');
      }
    }

    return isVisible;
  }

  _getToolbarButtonHtml(html, dataItem) {
    const iconUrl = dataItem.Icon.left(7).toLowerCase() === 'http://'
      ? dataItem.Icon
      : window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + dataItem.Icon;

    html
      .cat(`<li code="${$q.htmlEncode(dataItem.Value)}" class="item button">\n`)
      .cat(`  <a href="javascript:void(0);" class="link${dataItem.Checked ? ` ${this.ITEM_CHECKED_CLASS_NAME}` : ''}">`)
      .cat('<span class="outerWrapper">')
      .cat('<span class="innerWrapper">')

      .catIf(`<span class="icon" style="background-image: url('${iconUrl}')"${
        dataItem.Tooltip ? ` title="${$q.htmlEncode(dataItem.Tooltip)}"` : ''}>`
        + `<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif" /></span>`, dataItem.Icon)

      .catIf(
        `<span class="text${dataItem.Icon ? '' : ' textOnly'}">${$q.htmlEncode(dataItem.Text)}</span>`,
        dataItem.Text
      )
      .cat('</span>')
      .cat('</span>')
      .cat('</a>\n')
      .cat('</li>\n');
  }

  _getToolbarDropDownHtml(html, dataItem) {
    const that = this;
    const selectedSubItemValue = dataItem.SelectedSubItemValue;
    const subItems = dataItem.Items;

    if (subItems.length > 1) {
      let [selectedSubItem] = $.grep(subItems, subItem => subItem.Value === selectedSubItemValue);
      if (!selectedSubItem) {
        [selectedSubItem] = subItems;
      }

      html
        .cat(`<li code="${$q.htmlEncode(dataItem.Value)}" class="item dropDown">\n`)
        .cat('  <a href="javascript:void(0);" class="link">')
        .cat('<span class="outerWrapper">')
        .cat('<span class="innerWrapper">')
        .cat(`<span class="button"${dataItem.Tooltip ? ` title="${$q.htmlEncode(dataItem.Tooltip)}"` : ''}>`)
        .catIf('<span class="icon"'
          + `style="background-image: url('${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS}${selectedSubItem.Icon}')">`
          + `<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif" /></span>`, selectedSubItem.Icon)
        .catIf(`<span class="text${selectedSubItem.Icon ? '' : ' textOnly'}">
          ${$q.htmlEncode(selectedSubItem.Text)}</span>`, selectedSubItem.Text && dataItem.ShowButtonText)
        .cat('</span>')
        .cat(`<span class="arrow"${dataItem.ArrowTooltip
          ? ` title="${$q.htmlEncode(dataItem.ArrowTooltip)}"`
          : ''}><img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif" /></span>`)
        .cat('</span>')
        .cat('</span>')
        .cat('</a>\n');

      html.cat('<div class="list">\n');
      html.cat('<ul>\n');
      $.each(subItems, (index, subItem) => {
        that._getToolbarDropDownItemHtml(html, subItem, selectedSubItemValue);
      });

      html.cat('</ul>\n');
      html.cat('</div>\n');
      html.cat('</li>\n');
    }
  }

  _getToolbarDropDownItemHtml(html, dataItem, selectedSubItemValue) {
    const isSelected = dataItem.Value === selectedSubItemValue;
    html
      .cat(`<li code="${$q.htmlEncode(dataItem.Value)}" class="item
        ${isSelected ? ` ${this.DROPDOWN_LIST_ITEM_SELECTED_CLASS_NAME}` : ''}
      ">\n`)
      .cat('  <div class="outerWrapper">\n')
      .cat('      <div class="innerWrapper">\n')
      .cat('          <span class="icon"')
      .catIf(
        ` style="background-image: url('${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS}${dataItem.Icon}')"`,
        dataItem.Icon
      )
      .cat('>')
      .cat(`<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif" width="16px" height="16px" />`)
      .cat('</span>\n')
      .cat(`          <span class="text">${$q.htmlEncode(dataItem.Text)}</span>\n`)
      .cat('      </div>\n')
      .cat('  </div>\n')
      .cat('</li>\n');
  }

  _getSeparatorHtml(html) {
    html.cat('<li class="separator"></li>\n');
  }

  _extendToolbarItemElement(itemElem, item) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      $item.data('tooltip', item.Tooltip);
      $item.data('items_affected', item.ItemsAffected);
      $item.data('always_enabled', item.AlwaysEnabled);
      if (item.Type === window.TOOLBAR_ITEM_TYPE_BUTTON) {
        $item.data('icon', item.Icon);
        $item.data('check_on_click', item.CheckOnClick);
        $item.data('icon_checked', item.IconChecked);
        $item.data('tooltip_checked', item.TooltipChecked);
        this._extendToolbarDropDownListElements($item, item.Items);
      } else if (item.Type === window.TOOLBAR_ITEM_TYPE_DROPDOWN) {
        $item.data('show_button_text', item.ShowButtonText);
        $item.data('selected_sub_item_value', item.SelectedSubItemValue);
        this._extendToolbarDropDownListElements($item, item.Items);
      }
    }
  }

  _extendToolbarItemElements(items) {
    const that = this;
    $.each(items, (index, item) => {
      const $item = that.getToolbarItem(item.Value);
      if ($item && $item.length) {
        that._extendToolbarItemElement($item, item);
      }
    });
  }

  _extendToolbarDropDownListItemElement(listItem, subItem) {
    const $listItem = this._getToolbarDropDownListItem(listItem);
    if ($listItem.length) {
      $listItem.data('tooltip', subItem.Tooltip);
      $listItem.data('icon', subItem.Icon);
    }
  }

  _extendToolbarDropDownListElements(itemElem, subItems) {
    if (subItems) {
      const that = this;
      const $item = this.getToolbarItem(itemElem);
      if ($item && $item.length) {
        $.each(subItems, (index, subItem) => {
          const $listItem = that._getToolbarDropDownListItemByValue($item, subItem.Value);
          if ($listItem) {
            that._extendToolbarDropDownListItemElement($listItem, subItem);
          }
        });
      }
    }
  }

  _applyClickedStyleToToolbarButton(itemElem) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      if (!$item.data('check_on_click')) {
        const $link = $(itemElem).find('A:first');
        $link.addClass('clicked');
      }
    }
  }

  _cancelClickedStyleToToolbarButton(itemElem) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      const $link = $item.find('A:first');
      $link.removeClass('clicked');
    }
  }

  _toggleCheckedStyleToToolbarButton(itemElem) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      if ($item.data('check_on_click')) {
        const $link = $item.find('A.link');
        const $icon = $item.find('SPAN.icon');
        const iconChecked = $item.data('icon_checked');
        const tooltipChecked = $item.data('tooltip_checked');
        if (this.isToolbarButtonChecked($item)) {
          $link.removeClass(this.ITEM_CHECKED_CLASS_NAME);
          if (iconChecked) {
            $icon.css('backgroundImage', `url("${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + $item.data('icon')}")`);
          }

          if (tooltipChecked) {
            $icon.attr('title', $item.data('tooltip'));
          }
        } else {
          $link.addClass(this.ITEM_CHECKED_CLASS_NAME);
          if (iconChecked) {
            $icon.css('backgroundImage', `url("${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + iconChecked}")`);
          }

          if (tooltipChecked) {
            $icon.attr('title', tooltipChecked);
          }
        }
      }
    }
  }

  _applyClickedStyleToToolbarDropDownArrow(itemElem) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      const $link = $item.find('A:first');
      $link.addClass('arrowClicked');
    }
  }

  _applyClickedStyleToToolbarDropDownButton(itemElem) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      const $link = $item.find('A:first');
      $link.addClass('buttonClicked');
    }
  }

  _cancelClickedStyleToToolbarDropDown(itemElem) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      $item
        .find('A:first')
        .removeClass('arrowClicked')
        .removeClass('buttonClicked');
    }
  }

  isToolbarItemEnabled(item) {
    const $item = this.getToolbarItem(item);
    let isEnabled = false;
    if ($item && $item.length) {
      isEnabled = !$item.hasClass(this.ITEM_DISABLED_CLASS_NAME);
    }

    return isEnabled;
  }

  isToolbarButtonChecked(item) {
    const $item = this.getToolbarItem(item);
    let isChecked = false;
    if ($item && $item.length) {
      const $link = $item.find('A.link');
      if ($link.length) {
        isChecked = $link.hasClass(this.ITEM_CHECKED_CLASS_NAME);
      }
    }

    return isChecked;
  }

  setVisibleState(itemElem, state) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      if (state) {
        $item.show();
      } else {
        $item.hide();
      }
    }
  }

  setVisibleStateForAll(state) {
    const that = this;
    this.getToolbarItems().each((index, elem) => {
      that.setVisibleState(elem, state);
    });
  }

  setEnableState(itemElem, state) {
    const $item = this.getToolbarItem(itemElem);
    if ($item && $item.length) {
      if (state) {
        $item.removeClass(this.ITEM_DISABLED_CLASS_NAME);
      } else if (!$item.data('always_enabled')) {
        $item.addClass(this.ITEM_DISABLED_CLASS_NAME);
      }
    }
  }

  setEnableStateForAll(state) {
    const that = this;
    this.getToolbarItems().each((index, elem) => {
      that.setEnableState(elem, state);
    });
  }

  _attachToolbarEventHandlers() {
    $(this._toolbarElement)
      .delegate(this.BUTTON_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarButtonUnhovering)
      .delegate(this.BUTTON_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarButtonClicking)
      .delegate(this.BUTTON_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarButtonClicked)
      .delegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarDropDownArrowUnhovering)
      .delegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownArrowClicking)
      .delegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownArrowClicked)
      .delegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarDropDownButtonUnhovering)
      .delegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownButtonClicking)
      .delegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownButtonClicked)
      .delegate(this.DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownListItemClicking)
      .delegate(this.DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownListItemClicked);
  }

  _detachToolbarEventHandlers() {
    $(this._toolbarElement)
      .undelegate(this.BUTTON_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarButtonUnhovering)
      .undelegate(this.BUTTON_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarButtonClicking)
      .undelegate(this.BUTTON_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarButtonClicked)
      .undelegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarDropDownArrowUnhovering)
      .undelegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownArrowClicking)
      .undelegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownArrowClicked)
      .undelegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarDropDownButtonUnhovering)
      .undelegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownButtonClicking)
      .undelegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownButtonClicked)
      .undelegate(this.DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownListItemClicking)
      .undelegate(this.DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownListItemClicked);
  }

  notifyToolbarButtonClicking(eventArgs) {
    this.notify(window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKING, eventArgs);
  }

  notifyToolbarButtonClicked(eventArgs) {
    this.notify(window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKED, eventArgs);
  }

  notifyDropDownSelectedIndexChanged(eventArgs) {
    this.notify(window.EVENT_TYPE_TOOLBAR_DROPDOWN_SELECTED_INDEX_CHANGED, eventArgs);
  }

  notifyDropDownSelectedIndexChanging(eventArgs) {
    this.notify(window.EVENT_TYPE_TOOLBAR_DROPDOWN_SELECTED_INDEX_CHANGING, eventArgs);
  }

  _onToolbarButtonUnhovering(e) {
    const $button = $(e.currentTarget);
    if (!this.isToolbarItemEnabled($button) || this.isToolbarBusy()) {
      return false;
    }

    this._cancelClickedStyleToToolbarButton($button);
    return undefined;
  }

  _onToolbarButtonClicking(e) {
    const $button = $(e.currentTarget);
    const isLeftClick = e.which === 1;
    if (!isLeftClick || !this.isToolbarItemEnabled($button) || this.isToolbarBusy()) {
      return false;
    }

    this._applyClickedStyleToToolbarButton($button);

    const value = this.getToolbarItemValue($button);
    const checkOnClick = $button.data('check_on_click');
    const checked = this.isToolbarButtonChecked($button);

    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendToolbarButtonEventArgs();
    eventArgs.set_value(value);
    eventArgs.set_checkOnClick(checkOnClick);
    eventArgs.set_checked(checked);

    this.notifyToolbarButtonClicking(eventArgs);
    return undefined;
  }

  _onToolbarButtonClicked(e) {
    const $button = $(e.currentTarget);
    const isLeftClick = e.which === 1;
    if (!isLeftClick || !this.isToolbarItemEnabled($button) || this.isToolbarBusy()) {
      return false;
    }

    this._cancelClickedStyleToToolbarButton($button);
    this._toggleCheckedStyleToToolbarButton($button);

    const value = this.getToolbarItemValue($button);
    const checkOnClick = $button.data('check_on_click');
    const checked = this.isToolbarButtonChecked($button);
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendToolbarButtonEventArgs();
    eventArgs.set_value(value);
    eventArgs.set_checkOnClick(checkOnClick);
    eventArgs.set_checked(checked);

    this.notifyToolbarButtonClicked(eventArgs);
    return undefined;
  }

  _onToolbarDropDownArrowUnhovering(e) {
    const $arrow = $(e.currentTarget);
    const $item = $arrow.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    if (!this._isDropDownListVisible($item)) {
      this._cancelClickedStyleToToolbarDropDown($item);
    }

    return undefined;
  }

  _onToolbarDropDownArrowClicking(e) {
    const $arrow = $(e.currentTarget);
    const $item = $arrow.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    this._applyClickedStyleToToolbarDropDownArrow($item);
    return undefined;
  }

  _onToolbarDropDownArrowClicked(e) {
    const $arrow = $(e.currentTarget);
    const $item = $arrow.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    this._toggleDropDownList($item);
    return undefined;
  }

  _onToolbarDropDownButtonUnhovering(e) {
    const $button = $(e.currentTarget);
    const $item = $button.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    if (!this._isDropDownListVisible($item)) {
      this._cancelClickedStyleToToolbarDropDown($item);
    }

    return undefined;
  }

  _onToolbarDropDownButtonClicking(e) {
    const $button = $(e.currentTarget);
    const $item = $button.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    this._cancelClickedStyleToToolbarDropDown($item);
    return undefined;
  }

  _onToolbarDropDownButtonClicked(e) {
    const $button = $(e.currentTarget);
    const $item = $button.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    const selectedSubItemValue = $item.data('selected_sub_item_value');
    const $listItem = this._getToolbarDropDownListItemByValue($item, selectedSubItemValue);

    if ($listItem.length) {
      const $nexListItem = this._getToolbarDropDownListNextItem($listItem);
      if ($nexListItem.length) {
        const itemValue = this.getToolbarItemValue($item);
        const oldSubItemValue = selectedSubItemValue;
        const newSubItemValue = this._getToolbarDropDownListItemValue($nexListItem);
        if (oldSubItemValue === newSubItemValue) {
          this._applyClickedStyleToToolbarDropDownButton($item);
          this._hideDropDownList($item);
        } else {
          // eslint-disable-next-line no-use-before-define
          let eventArgs = new BackendToolbarDropDownListEventArgs();
          eventArgs.set_itemValue(itemValue);
          eventArgs.set_oldSubItemValue(oldSubItemValue);
          eventArgs.set_newSubItemValue(newSubItemValue);

          this.notifyDropDownSelectedIndexChanging(eventArgs);
          this._updateDropDownListButton($item, $nexListItem);
          this._applyClickedStyleToToolbarDropDownButton($item);
          this._hideDropDownList($item);

          // eslint-disable-next-line no-use-before-define
          eventArgs = new BackendToolbarDropDownListEventArgs();
          eventArgs.set_itemValue(itemValue);
          eventArgs.set_oldSubItemValue(oldSubItemValue);
          eventArgs.set_newSubItemValue(newSubItemValue);

          this.notifyDropDownSelectedIndexChanged(eventArgs);
        }
      }
    }

    return undefined;
  }

  _onToolbarDropDownListItemClicking(e) {
    const $listItem = $(e.currentTarget);
    const $item = this._getToolbarItemByDropDownListItem($listItem);

    if ($item && $item.length) {
      if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
        return false;
      }

      const itemValue = this.getToolbarItemValue($item);
      const oldSubItemValue = $item.data('selected_sub_item_value');
      const newSubItemValue = this._getToolbarDropDownListItemValue($listItem);

      if (oldSubItemValue !== newSubItemValue) {
        // eslint-disable-next-line no-use-before-define
        const eventArgs = new BackendToolbarDropDownListEventArgs();
        eventArgs.set_itemValue(itemValue);
        eventArgs.set_oldSubItemValue(oldSubItemValue);
        eventArgs.set_newSubItemValue(newSubItemValue);
        this.notifyDropDownSelectedIndexChanging(eventArgs);
      }
    }

    return undefined;
  }

  _onToolbarDropDownListItemClicked(e) {
    const $listItem = $(e.currentTarget);
    const $item = this._getToolbarItemByDropDownListItem($listItem);

    if ($item && $item.length) {
      if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
        return false;
      }

      const itemValue = this.getToolbarItemValue($item);
      const oldSubItemValue = $item.data('selected_sub_item_value');
      const newSubItemValue = this._getToolbarDropDownListItemValue($listItem);

      if (oldSubItemValue === newSubItemValue) {
        this._hideDropDownList($item);
      } else {
        this._updateDropDownListButton($item, $listItem);
        this._hideDropDownList($item);

        // eslint-disable-next-line no-use-before-define
        const eventArgs = new BackendToolbarDropDownListEventArgs();
        eventArgs.set_itemValue(itemValue);
        eventArgs.set_oldSubItemValue(oldSubItemValue);
        eventArgs.set_newSubItemValue(newSubItemValue);

        this.notifyDropDownSelectedIndexChanged(eventArgs);
      }
    }

    return undefined;
  }

  dispose() {
    super.dispose();
    this._detachToolbarEventHandlers();
    if (this._toolbarItemListElement) {
      const $toolbarItemList = $(this._toolbarItemListElement);
      $toolbarItemList.empty().remove();
    }

    if (this._toolbarElement) {
      const $toolbar = $(this._toolbarElement);
      $toolbar.empty().remove();
    }

    this._toolbarItemListElement = null;
    this._toolbarElement = null;
  }
}

export class BackendToolbarButtonEventArgs extends Sys.EventArgs {
  _value = '';
  _checkOnClick = false;
  _checked = false;

  // eslint-disable-next-line camelcase
  get_value() {
    return this._value;
  }

  // eslint-disable-next-line camelcase
  set_value(value) {
    this._value = value;
  }

  // eslint-disable-next-line camelcase
  get_checkOnClick() {
    return this._checkOnClick;
  }

  // eslint-disable-next-line camelcase
  set_checkOnClick(value) {
    this._checkOnClick = value;
  }

  // eslint-disable-next-line camelcase
  get_checked() {
    return this._checked;
  }

  // eslint-disable-next-line camelcase
  set_checked(value) {
    this._checked = value;
  }
}

export class BackendToolbarDropDownListEventArgs extends Sys.EventArgs {
  _itemValue = '';
  _oldSubItemValue = '';
  _newSubItemValue = '';

  // eslint-disable-next-line camelcase
  get_itemValue() {
    return this._itemValue;
  }

  // eslint-disable-next-line camelcase
  set_itemValue(value) {
    this._itemValue = value;
  }

  // eslint-disable-next-line camelcase
  get_oldSubItemValue() {
    return this._oldSubItemValue;
  }

  // eslint-disable-next-line camelcase
  set_oldSubItemValue(value) {
    this._oldSubItemValue = value;
  }

  // eslint-disable-next-line camelcase
  get_newSubItemValue() {
    return this._newSubItemValue;
  }

  // eslint-disable-next-line camelcase
  set_newSubItemValue(value) {
    this._newSubItemValue = value;
  }
}

Quantumart.QP8.BackendToolbar = BackendToolbar;
Quantumart.QP8.BackendToolbarButtonEventArgs = BackendToolbarButtonEventArgs;
Quantumart.QP8.BackendToolbarDropDownListEventArgs = BackendToolbarDropDownListEventArgs;
