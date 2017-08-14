window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKING = 'OnToolbarButtonClicking';
window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKED = 'OnToolbarButtonClicked';
window.EVENT_TYPE_TOOLBAR_DROPDOWN_SELECTED_INDEX_CHANGING = 'OnToolbarDropDownListSelectedIndexChanging';
window.EVENT_TYPE_TOOLBAR_DROPDOWN_SELECTED_INDEX_CHANGED = 'OnToolbarDropDownListSelectedIndexChanged';
window.TOOLBAR_ITEM_TYPE_BUTTON = 'button';
window.TOOLBAR_ITEM_TYPE_DROPDOWN = 'drop_down';

Quantumart.QP8.BackendToolbar = function (toolbarElementId, options) {
  Quantumart.QP8.BackendToolbar.initializeBase(this);

  this._toolbarElementId = toolbarElementId;
  if ($q.isObject(options)) {
    if (options.toolbarContainerElementId) {
      this._toolbarContainerElementId = options.toolbarContainerElementId;
    }
  }

  this._onToolbarButtonUnhoveringHandler = $.proxy(this._onToolbarButtonUnhovering, this);
  this._onToolbarButtonClickingHandler = $.proxy(this._onToolbarButtonClicking, this);
  this._onToolbarButtonClickedHandler = $.proxy(this._onToolbarButtonClicked, this);
  this._onToolbarDropDownArrowUnhoveringHandler = $.proxy(this._onToolbarDropDownArrowUnhovering, this);
  this._onToolbarDropDownArrowClickingHandler = $.proxy(this._onToolbarDropDownArrowClicking, this);
  this._onToolbarDropDownArrowClickedHandler = $.proxy(this._onToolbarDropDownArrowClicked, this);
  this._onToolbarDropDownButtonUnhoveringHandler = $.proxy(this._onToolbarDropDownButtonUnhovering, this);
  this._onToolbarDropDownButtonClickingHandler = $.proxy(this._onToolbarDropDownButtonClicking, this);
  this._onToolbarDropDownButtonClickedHandler = $.proxy(this._onToolbarDropDownButtonClicked, this);
  this._onToolbarDropDownListItemClickingHandler = $.proxy(this._onToolbarDropDownListItemClicking, this);
  this._onToolbarDropDownListItemClickedHandler = $.proxy(this._onToolbarDropDownListItemClicked, this);
};

Quantumart.QP8.BackendToolbar.prototype = {
  _toolbarElementId: '',
  _toolbarElement: null,
  _toolbarItemListElement: null,
  _toolbarContainerElementId: '',

  ITEM_DISABLED_CLASS_NAME: 'disabled',
  ITEM_CHECKED_CLASS_NAME: 'checked',
  ITEM_BUSY_CLASS_NAME: 'busy',
  DROPDOWN_LIST_ITEM_SELECTED_CLASS_NAME: 'selected',

  BUTTON_CLICKABLE_SELECTORS: '.toolbar > UL > LI.button',
  DROPDOWN_ARROW_CLICKABLE_SELECTORS: '.toolbar > UL > LI.dropDown SPAN.arrow',
  DROPDOWN_BUTTON_CLICKABLE_SELECTORS: '.toolbar > UL > LI.dropDown SPAN.button',
  DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS: '.toolbar > UL > LI.dropDown .list UL LI.item',

  _onToolbarButtonUnhoveringHandler: null,
  _onToolbarButtonClickingHandler: null,
  _onToolbarButtonClickedHandler: null,
  _onToolbarDropDownArrowUnhoveringHandler: null,
  _onToolbarDropDownArrowClickingHandler: null,
  _onToolbarDropDownArrowClickedHandler: null,
  _onToolbarDropDownButtonUnhoveringHandler: null,
  _onToolbarDropDownButtonClickingHandler: null,
  _onToolbarDropDownButtonClickedHandler: null,
  _onToolbarDropDownListItemClickingHandler: null,
  _onToolbarDropDownListItemClickedHandler: null,
  _isBindToExternal: false,

  get_toolbarElementId: function () {
    return this._toolbarElementId;
  },

  set_toolbarElementId: function (value) {
    this._toolbarElementId = value;
  },

  get_toolbarElement: function () {
    return this._toolbarElement;
  },

  get_toolbarContainerElementId: function () {
    return this._toolbarContainerElementId;
  },

  set_toolbarContainerElementId: function (value) {
    this._toolbarContainerElementId = value;
  },

  get_isBindToExternal: function () {
    return this._isBindToExternal;
  },

  set_isBindToExternal: function (value) {
    this._isBindToExternal = value;
  },

  initialize: function () {
    let $toolbar = $(`#${this._toolbarElementId}`);
    let $toolbarItemList = null;

    let isToolbarExist = !$q.isNullOrEmpty($toolbar);
    if (!isToolbarExist) {
      $toolbar = $('<div />', { id: this._toolbarElementId, class: 'toolbar', css: { display: 'none' } });
    }

    $toolbarItemList = $toolbar.find('UL:first');
    if ($q.isNullOrEmpty($toolbarItemList)) {
      $toolbarItemList = $('<ul />');
    }

    if (!isToolbarExist) {
      $toolbar.append($toolbarItemList);

      if (!$q.isNullOrWhiteSpace(this._toolbarContainerElementId)) {
        $(`#${this._toolbarContainerElementId}`).append($toolbar);
      } else {
        $('BODY:first').append($toolbar);
      }
    }

    this._toolbarElement = $toolbar.get(0);
    this._toolbarItemListElement = $toolbarItemList.get(0);

    if (!isToolbarExist) {
      this._attachToolbarEventHandlers();
    }
  },

  showToolbar: function (callback) {
    $(this._toolbarElement).show();
    $q.callFunction(callback);
  },

  hideToolbar: function (callback) {
    $(this._toolbarElement).hide();
    $q.callFunction(callback);
  },

  markToolbarAsBusy: function () {
    $(this._toolbarItemListElement).addClass(this.ITEM_BUSY_CLASS_NAME);
  },

  unmarkToolbarAsBusy: function () {
    $(this._toolbarItemListElement).removeClass(this.ITEM_BUSY_CLASS_NAME);
  },

  isToolbarBusy: function () {
    return $(this._toolbarItemListElement).hasClass(this.ITEM_BUSY_CLASS_NAME);
  },

  getToolbarItems: function () {
    return $('> LI.item', this._toolbarItemListElement);
  },

  getToolbarItem: function (item) {
    let $item = null;
    if ($q.isObject(item)) {
      return $q.toJQuery(item);
    } else if ($q.isString(item)) {
      $item = $(`LI[code='${item}']`, this._toolbarItemListElement);
      if ($item.length == 0) {
        $item = null;
      }

      return $item;
    }
  },

  _getToolbarItemByDropDownListItem: function (listItemElem) {
    let $listItem = this._getToolbarDropDownListItem(listItemElem);
    let $item = null;

    if (!$q.isNullOrEmpty($listItem)) {
      $item = $listItem.parent().parent().parent();
      if ($item.length == 0) {
        $item = null;
      }
    }

    return $item;
  },

  getToolbarItemValue: function (itemElem) {
    let $item = this.getToolbarItem(itemElem);
    let itemValue = '';

    if (!$q.isNullOrEmpty($item)) {
      itemValue = $item.attr('code');
    } else {
      $q.alertFail($l.Toolbar.itemNotSpecified);
      return;
    }

    $item = null;

    return itemValue;
  },

  getToolbarItemText: function (item) {
    let $item = this.getToolbarItem(item);
    let itemText = '';

    if (!$q.isNullOrEmpty($item)) {
      itemText = $('SPAN.text', $item).text();
    }

    return itemText;
  },

  _getToolbarItemsHtml: function (items) {
    let itemCount = items.length;
    let itemsHtml = new $.telerik.stringBuilder();

    for (let itemIndex = 0; itemIndex < itemCount; itemIndex++) {
      let item = items[itemIndex];

      if (item.Type == window.TOOLBAR_ITEM_TYPE_BUTTON) {
        this._getToolbarButtonHtml(itemsHtml, item);
      } else if (item.Type == window.TOOLBAR_ITEM_TYPE_DROPDOWN) {
        this._getToolbarDropDownHtml(itemsHtml, item);
      }
    }
    return itemsHtml.string();
  },

  addToolbarItemsToToolbar: function (items, count) {
    if (!$q.isNull(items)) {
      $(this._toolbarItemListElement).html(this._getToolbarItemsHtml(items));
      this._extendToolbarItemElements(items);
      this.refreshToolbarItems(count);
    }
  },

  tuneToolbarItems: function (statuses) {
    let statusCount = statuses.length;
    for (let statusIndex = 0; statusIndex < statusCount; statusIndex++) {
      let status = statuses[statusIndex];
      let $item = this.getToolbarItem(status.Code);

      if (!$q.isNullOrEmpty($item)) {
        this.setVisibleState($item, status.Visible);
      }
    }
  },

  removeToolbarItemsFromToolbar: function () {
    let $toolbarItemList = $(this._toolbarItemListElement);
    $toolbarItemList.empty();
  },

  refreshToolbarItems: function (selectedEntitiesCount) {
    if (selectedEntitiesCount == null) {
      selectedEntitiesCount = 0;
    }

    let self = this;
    let $items = this.getToolbarItems();
    $items.each(
      (index, elem) => {
        let $item = $(elem);
        let itemsAffected = +$item.data('items_affected') || 0;
        let state = selectedEntitiesCount == itemsAffected || (itemsAffected == window.MAX_ITEMS_AFFECTED_NUMBER && selectedEntitiesCount >= 1);
        self.setEnableState($item, state);
      }
    );

    $items = null;
  },

  _getToolbarDropDownList: function (item) {
    let $list;
    let $item = this.getToolbarItem(item);
    if (!$q.isNullOrEmpty($item)) {
      $list = $item.find('.list:first');
    }

    return $list;
  },

  _updateDropDownListButton: function (item, listItemElem) {
    let $item = this.getToolbarItem(item);
    let $listItem = this._getToolbarDropDownListItem(listItemElem);

    if (!$q.isNullOrEmpty($item) && !$q.isNullOrEmpty($listItem)) {
      let $icon = $item.find('SPAN.button > SPAN.icon');
      let $text = $item.find('SPAN.button > SPAN.text');

      let icon = $listItem.data('icon');
      let text = this._getToolbarDropDownListItemText($listItem);
      let showButtonText = $item.data('show_button_text');

      $listItem
        .siblings().removeClass(this.DROPDOWN_LIST_ITEM_SELECTED_CLASS_NAME).end()
        .addClass(this.DROPDOWN_LIST_ITEM_SELECTED_CLASS_NAME)
      ;
      $item.data('selected_sub_item_value', this._getToolbarDropDownListItemValue($listItem));
      $icon.css('backgroundImage', String.format("url('{0}')", window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + icon));
      if (!$q.isNullOrWhiteSpace(text) && showButtonText) {
        $text.text(text);
      }

      $icon = null;
      $text = null;
    }
  },

  _getToolbarDropDownListItem: function (listItemElem) {
    let $listItem = null;

    if ($q.isObject(listItemElem)) {
      $listItem = $q.toJQuery(listItemElem);
      if ($listItem.length == 0) {
        $listItem = null;
      }
    }

    return $listItem;
  },

  _getToolbarDropDownListNextItem: function (listItemElem) {
    let $listItem = this._getToolbarDropDownListItem(listItemElem);
    let $nextListItem = null;

    if (!$q.isNullOrEmpty($listItem)) {
      $nextListItem = $listItem.next();
      if ($q.isNullOrEmpty($nextListItem)) {
        $nextListItem = $listItem
          .siblings()
          .first()
        ;
      }

      if ($nextListItem.length == 0) {
        $nextListItem = null;
      }
    }

    return $nextListItem;
  },

  _getToolbarDropDownListItemByValue: function (item, listItemValue) {
    let $item = this.getToolbarItem(item);
    let $listItem = null;

    if (!$q.isNullOrEmpty($item)) {
      let $list = this._getToolbarDropDownList($item);
      if (!$q.isNullOrEmpty($list)) {
        $listItem = $item.find(`LI[code='${listItemValue}']`);
        if ($listItem.length == 0) {
          $listItem = null;
        }
      }
    }

    return $listItem;
  },

  _getToolbarDropDownListItemValue: function (listItemElem) {
    let $listItem = this._getToolbarDropDownListItem(listItemElem);
    let listItemValue = '';

    if (!$q.isNullOrEmpty($listItem)) {
      listItemValue = $listItem.attr('code');
    }

    return listItemValue;
  },

  _getToolbarDropDownListItemText: function (listItemElem) {
    let $listItem = this._getToolbarDropDownListItem(listItemElem);
    let listItemText = '';

    if (!$q.isNullOrEmpty($listItem)) {
      listItemText = $listItem.find('SPAN.text').text();
    }

    return listItemText;
  },

  _toggleDropDownList: function (item) {
    let $item = this.getToolbarItem(item);

    if (!$q.isNullOrEmpty($item)) {
      if (!this._isDropDownListVisible($item)) {
        this._showDropDownList($item);
      } else {
        this._hideDropDownList($item);
      }
    }
  },

  _showDropDownList: function (item) {
    let $item = this.getToolbarItem(item);

    if (!$q.isNullOrEmpty($item)) {
      let $list = this._getToolbarDropDownList($item);

      if (!$q.isNullOrEmpty($list)) {
        let windowWidth = $(window).width();
        let windowHeight = $(window).height();
        let listWidth = $list.outerWidth();
        let listHeight = $list.outerHeight();
        let listTop = $item.offset().top + $item.height() + $item.borderTopWidth() + $item.borderBottomWidth() - 1;
        let listLeft = $item.offset().left;
        if ((listLeft + listWidth) > windowWidth) {
          listLeft = windowWidth - listWidth;
        }

        $list.css('top', `${listTop}px`);
        $list.css('left', `${listLeft}px`);
        $list.show();
      }

      $list = null;
    }
  },

  _hideDropDownList: function (item) {
    let $item = this.getToolbarItem(item);

    if (!$q.isNullOrEmpty($item)) {
      let $list = this._getToolbarDropDownList($item);
      if (!$q.isNullOrEmpty($list)) {
        $list.hide();
      }

      $list = null;

      this._cancelClickedStyleToToolbarDropDown($item);
    }
  },

  _isDropDownListVisible: function (item) {
    let $item = this.getToolbarItem(item);
    let isVisible = false;

    if (!$q.isNullOrEmpty($item)) {
      let $list = this._getToolbarDropDownList($item);
      if (!$q.isNullOrEmpty($list)) {
        isVisible = $list.is(':visible');
      }
    }

    return isVisible;
  },

  _getToolbarButtonHtml: function (html, dataItem) {
    let iconUrl = dataItem.Icon.left(7).toLowerCase() !== 'http://' ? window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + dataItem.Icon : dataItem.Icon;

    html
      .cat(`<li code="${$q.htmlEncode(dataItem.Value)}" class="item button">\n`)
      .cat(`  <a href="javascript:void(0);" class="link${dataItem.Checked ? ` ${this.ITEM_CHECKED_CLASS_NAME}` : ''}">`)
      .cat('<span class="outerWrapper">')
      .cat('<span class="innerWrapper">')

      .catIf(`<span class="icon" style="background-image: url('${iconUrl}')"${
        !$q.isNullOrWhiteSpace(dataItem.Tooltip) ? ` title="${$q.htmlEncode(dataItem.Tooltip)}"` : ''}>`
        + `<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}/0.gif" /></span>`, !$q.isNullOrWhiteSpace(dataItem.Icon))

      .catIf(`<span class="text${$q.isNullOrWhiteSpace(dataItem.Icon) ? ' textOnly' : ''}">${$q.htmlEncode(dataItem.Text)}</span>`, !$q.isNullOrWhiteSpace(dataItem.Text))
      .cat('</span>')
      .cat('</span>')
      .cat('</a>\n')
      .cat('</li>\n')
    ;
  },

  _getToolbarDropDownHtml: function (html, dataItem) {
    let self = this;
    let selectedSubItemValue = dataItem.SelectedSubItemValue;
    let subItems = dataItem.Items;

    if (subItems.length > 1) {
      let selectedSubItem = jQuery.grep(subItems, (subItem) => subItem.Value == selectedSubItemValue)[0];
      if (!selectedSubItem) {
        selectedSubItem = subItems[0];
      }

      html
        .cat(`<li code="${$q.htmlEncode(dataItem.Value)}" class="item dropDown">\n`)
        .cat('  <a href="javascript:void(0);" class="link">')
        .cat('<span class="outerWrapper">')
        .cat('<span class="innerWrapper">')
        .cat(`<span class="button"${!$q.isNullOrWhiteSpace(dataItem.Tooltip) ? ` title="${$q.htmlEncode(dataItem.Tooltip)}"` : ''}>`)
        .catIf(`<span class="icon" style="background-image: url('${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS}${selectedSubItem.Icon}')">`
          + `<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}/0.gif" /></span>`, !$q.isNullOrWhiteSpace(selectedSubItem.Icon))
        .catIf(`<span class="text${$q.isNullOrWhiteSpace(selectedSubItem.Icon) ? ' textOnly' : ''}">${$q.htmlEncode(selectedSubItem.Text)}</span>`, !$q.isNullOrWhiteSpace(selectedSubItem.Text) && dataItem.ShowButtonText)
        .cat('</span>')
        .cat(`<span class="arrow"${!$q.isNullOrWhiteSpace(dataItem.ArrowTooltip) ? ` title="${$q.htmlEncode(dataItem.ArrowTooltip)}"` : ''}><img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}/0.gif" /></span>`)
        .cat('</span>')
        .cat('</span>')
        .cat('</a>\n')
      ;
      html.cat('<div class="list">\n');
      html.cat('<ul>\n');
      jQuery.each(subItems,
        (index, subItem) => {
          self._getToolbarDropDownItemHtml(html, subItem, selectedSubItemValue);
        }
      );
      html.cat('</ul>\n');
      html.cat('</div>\n');
      html.cat('</li>\n');
    }
  },

  _getToolbarDropDownItemHtml: function (html, dataItem, selectedSubItemValue) {
    let isSelected = dataItem.Value == selectedSubItemValue;

    html
      .cat(`<li code="${$q.htmlEncode(dataItem.Value)}" class="item${isSelected ? ` ${this.DROPDOWN_LIST_ITEM_SELECTED_CLASS_NAME}` : ''}">\n`)
      .cat('  <div class="outerWrapper">\n')
      .cat('      <div class="innerWrapper">\n')
      .cat('          <span class="icon"')
      .catIf(` style="background-image: url('${window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS}${dataItem.Icon}')"`, !$q.isNullOrWhiteSpace(dataItem.Icon))
      .cat('>')
      .cat(`<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif" width="16px" height="16px" />`)
      .cat('</span>\n')
      .cat(`          <span class="text">${$q.htmlEncode(dataItem.Text)}</span>\n`)
      .cat('      </div>\n')
      .cat('  </div>\n')
      .cat('</li>\n')
    ;
  },

  _getSeparatorHtml: function (html) {
    html.cat('<li class="separator"></li>\n');
  },

  _extendToolbarItemElement: function (itemElem, item) {
    let $item = this.getToolbarItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {
      $item.data('tooltip', item.Tooltip);
      $item.data('items_affected', item.ItemsAffected);
      $item.data('always_enabled', item.AlwaysEnabled);
      if (item.Type == window.TOOLBAR_ITEM_TYPE_BUTTON) {
        $item.data('icon', item.Icon);
        $item.data('check_on_click', item.CheckOnClick);
        $item.data('icon_checked', item.IconChecked);
        $item.data('tooltip_checked', item.TooltipChecked);
        this._extendToolbarDropDownListElements($item, item.Items);
      } else if (item.Type == window.TOOLBAR_ITEM_TYPE_DROPDOWN) {
        $item.data('show_button_text', item.ShowButtonText);
        $item.data('selected_sub_item_value', item.SelectedSubItemValue);
        this._extendToolbarDropDownListElements($item, item.Items);
      }
    }
  },

  _extendToolbarItemElements: function (items) {
    let self = this;

    jQuery.each(items,
      (index, item) => {
        let $item = self.getToolbarItem(item.Value);
        if (!$q.isNullOrEmpty($item)) {
          self._extendToolbarItemElement($item, item);
        }
      }
    );
  },

  _extendToolbarDropDownListItemElement: function (listItem, subItem) {
    let $listItem = this._getToolbarDropDownListItem(listItem);
    if (!$q.isNullOrEmpty($listItem)) {
      $listItem.data('tooltip', subItem.Tooltip);
      $listItem.data('icon', subItem.Icon);
    }
  },

  _extendToolbarDropDownListElements: function (itemElem, subItems) {
    if (!$q.isNullOrEmpty(subItems)) {
      let self = this;
      let $item = this.getToolbarItem(itemElem);

      if (!$q.isNullOrEmpty($item)) {
        jQuery.each(subItems,
          (index, subItem) => {
            let $listItem = self._getToolbarDropDownListItemByValue($item, subItem.Value);
            if (!$q.isNullOrEmpty($listItem)) {
              self._extendToolbarDropDownListItemElement($listItem, subItem);
            }
          }
        );
      }
    }
  },

  _applyClickedStyleToToolbarButton: function (itemElem) {
    let $item = this.getToolbarItem(itemElem);

    if (!$q.isNullOrEmpty($item)) {
      if (!$item.data('check_on_click')) {
        let $link = $(itemElem).find('A:first');
        $link.addClass('clicked');

        $link = null;
      }
    }
  },

  _cancelClickedStyleToToolbarButton: function (itemElem) {
    let $item = this.getToolbarItem(itemElem);

    if (!$q.isNullOrEmpty($item)) {
      let $link = $item.find('A:first');
      $link.removeClass('clicked');

      $link = null;
    }
  },

  _toggleCheckedStyleToToolbarButton: function (itemElem) {
    let $item = this.getToolbarItem(itemElem);

    if (!$q.isNullOrEmpty($item)) {
      if ($item.data('check_on_click')) {
        let $link = $item.find('A.link');
        let $icon = $item.find('SPAN.icon');

        let iconChecked = $item.data('icon_checked');
        let tooltipChecked = $item.data('tooltip_checked');

        if (this.isToolbarButtonChecked($item)) {
          $link.removeClass(this.ITEM_CHECKED_CLASS_NAME);
          if (!$q.isNullOrWhiteSpace(iconChecked)) {
            $icon.css('backgroundImage', String.format('url("{0}")', window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + $item.data('icon')));
          }
          if (!$q.isNullOrWhiteSpace(tooltipChecked)) {
            $icon.attr('title', $item.data('tooltip'));
          }
        } else {
          $link.addClass(this.ITEM_CHECKED_CLASS_NAME);
          if (!$q.isNullOrWhiteSpace(iconChecked)) {
            $icon.css('backgroundImage', String.format('url("{0}")', window.THEME_IMAGE_FOLDER_URL_SMALL_ICONS + iconChecked));
          }
          if (!$q.isNullOrWhiteSpace(tooltipChecked)) {
            $icon.attr('title', tooltipChecked);
          }
        }
      }
    }
  },

  _applyClickedStyleToToolbarDropDownArrow: function (itemElem) {
    let $item = this.getToolbarItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {
      let $link = $item.find('A:first');
      $link.addClass('arrowClicked');
    }
  },

  _applyClickedStyleToToolbarDropDownButton: function (itemElem) {
    let $item = this.getToolbarItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {
      let $link = $item.find('A:first');
      $link.addClass('buttonClicked');

      $link = null;
    }
  },

  _cancelClickedStyleToToolbarDropDown: function (itemElem) {
    let $item = this.getToolbarItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {
      let $link = $item.find('A:first');
      $link
        .removeClass('arrowClicked')
        .removeClass('buttonClicked')
      ;
    }
  },

  isToolbarItemEnabled: function (item) {
    let $item = this.getToolbarItem(item);
    let isEnabled = false;

    if (!$q.isNullOrEmpty($item)) {
      isEnabled = !$item.hasClass(this.ITEM_DISABLED_CLASS_NAME);
    }

    return isEnabled;
  },

  isToolbarButtonChecked: function (item) {
    let $item = this.getToolbarItem(item);
    let isChecked = false;

    if (!$q.isNullOrEmpty($item)) {
      let $link = $item.find('A.link');
      if (!$q.isNullOrEmpty($link)) {
        isChecked = $link.hasClass(this.ITEM_CHECKED_CLASS_NAME);
      }
    }

    return isChecked;
  },

  setVisibleState: function (itemElem, state) {
    let $item = this.getToolbarItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {
      if (state) {
        $item.show();
      } else {
        $item.hide();
      }
    }
  },

  setVisibleStateForAll: function (state) {
    let self = this;
    this.getToolbarItems().each(
      (index, elem) => {
        self.setVisibleState(elem, state);
      }
    );
  },

  setEnableState: function (itemElem, state) {
    let $item = this.getToolbarItem(itemElem);
    if (!$q.isNullOrEmpty($item)) {
      if (state) {
        $item.removeClass(this.ITEM_DISABLED_CLASS_NAME);
      } else if (!$item.data('always_enabled')) {
        $item.addClass(this.ITEM_DISABLED_CLASS_NAME);
      }
    }
  },

  setEnableStateForAll: function (state) {
    let self = this;
    this.getToolbarItems().each(
      (index, elem) => {
        self.setEnableState(elem, state);
      }
    );
  },

  _attachToolbarEventHandlers: function () {
    $(this._toolbarElement)
      .delegate(this.BUTTON_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarButtonUnhoveringHandler)
      .delegate(this.BUTTON_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarButtonClickingHandler)
      .delegate(this.BUTTON_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarButtonClickedHandler)
      .delegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarDropDownArrowUnhoveringHandler)
      .delegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownArrowClickingHandler)
      .delegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownArrowClickedHandler)
      .delegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarDropDownButtonUnhoveringHandler)
      .delegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownButtonClickingHandler)
      .delegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownButtonClickedHandler)
      .delegate(this.DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownListItemClickingHandler)
      .delegate(this.DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownListItemClickedHandler);
  },

  _detachToolbarEventHandlers: function () {
    $(this._toolbarElement)
      .undelegate(this.BUTTON_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarButtonUnhoveringHandler)
      .undelegate(this.BUTTON_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarButtonClickingHandler)
      .undelegate(this.BUTTON_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarButtonClickedHandler)
      .undelegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarDropDownArrowUnhoveringHandler)
      .undelegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownArrowClickingHandler)
      .undelegate(this.DROPDOWN_ARROW_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownArrowClickedHandler)
      .undelegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mouseout', this._onToolbarDropDownButtonUnhoveringHandler)
      .undelegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownButtonClickingHandler)
      .undelegate(this.DROPDOWN_BUTTON_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownButtonClickedHandler)
      .undelegate(this.DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS, 'mouseup', this._onToolbarDropDownListItemClickingHandler)
      .undelegate(this.DROPDOWN_LIST_ITEM_CLICKABLE_SELECTORS, 'mousedown', this._onToolbarDropDownListItemClickedHandler);
  },

  notifyToolbarButtonClicking: function (eventArgs) {
    this.notify(window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKING, eventArgs);
  },

  notifyToolbarButtonClicked: function (eventArgs) {
    this.notify(window.EVENT_TYPE_TOOLBAR_BUTTON_CLICKED, eventArgs);
  },

  notifyDropDownSelectedIndexChanged: function (eventArgs) {
    this.notify(window.EVENT_TYPE_TOOLBAR_DROPDOWN_SELECTED_INDEX_CHANGED, eventArgs);
  },

  notifyDropDownSelectedIndexChanging: function (eventArgs) {
    this.notify(window.EVENT_TYPE_TOOLBAR_DROPDOWN_SELECTED_INDEX_CHANGING, eventArgs);
  },

  _onToolbarButtonUnhovering: function (e) {
    let $button = $(e.currentTarget);
    if (!this.isToolbarItemEnabled($button) || this.isToolbarBusy()) {
      return false;
    }

    this._cancelClickedStyleToToolbarButton($button);
  },

  _onToolbarButtonClicking: function (e) {
    let $button = $(e.currentTarget);
    let isLeftClick = e.which === 1;
    if (!isLeftClick || !this.isToolbarItemEnabled($button) || this.isToolbarBusy()) {
      return false;
    }

    this._applyClickedStyleToToolbarButton($button);

    let value = this.getToolbarItemValue($button);
    let checkOnClick = $button.data('check_on_click');
    let checked = this.isToolbarButtonChecked($button);

    let eventArgs = new Quantumart.QP8.BackendToolbarButtonEventArgs();
    eventArgs.set_value(value);
    eventArgs.set_checkOnClick(checkOnClick);
    eventArgs.set_checked(checked);

    this.notifyToolbarButtonClicking(eventArgs);
  },

  _onToolbarButtonClicked: function (e) {
    let $button = $(e.currentTarget);
    let isLeftClick = e.which === 1;
    if (!isLeftClick || !this.isToolbarItemEnabled($button) || this.isToolbarBusy()) {
      return false;
    }

    this._cancelClickedStyleToToolbarButton($button);
    this._toggleCheckedStyleToToolbarButton($button);

    let value = this.getToolbarItemValue($button);
    let checkOnClick = $button.data('check_on_click');
    let checked = this.isToolbarButtonChecked($button);

    let eventArgs = new Quantumart.QP8.BackendToolbarButtonEventArgs();
    eventArgs.set_value(value);
    eventArgs.set_checkOnClick(checkOnClick);
    eventArgs.set_checked(checked);

    this.notifyToolbarButtonClicked(eventArgs);
  },

  _onToolbarDropDownArrowUnhovering: function (e) {
    let $arrow = $(e.currentTarget);
    let $item = $arrow.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    if (!this._isDropDownListVisible($item)) {
      this._cancelClickedStyleToToolbarDropDown($item);
    }
  },

  _onToolbarDropDownArrowClicking: function (e) {
    let $arrow = $(e.currentTarget);
    let $item = $arrow.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    this._applyClickedStyleToToolbarDropDownArrow($item);
  },

  _onToolbarDropDownArrowClicked: function (e) {
    let $arrow = $(e.currentTarget);
    let $item = $arrow.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    this._toggleDropDownList($item);
  },

  _onToolbarDropDownButtonUnhovering: function (e) {
    let $button = $(e.currentTarget);
    let $item = $button.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    if (!this._isDropDownListVisible($item)) {
      this._cancelClickedStyleToToolbarDropDown($item);
    }
  },

  _onToolbarDropDownButtonClicking: function (e) {
    let $button = $(e.currentTarget);
    let $item = $button.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    this._cancelClickedStyleToToolbarDropDown($item);
  },

  _onToolbarDropDownButtonClicked: function (e) {
    let $button = $(e.currentTarget);
    let $item = $button.parent().parent().parent().parent();

    if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
      return false;
    }

    let selectedSubItemValue = $item.data('selected_sub_item_value');
    let $listItem = this._getToolbarDropDownListItemByValue($item, selectedSubItemValue);

    if (!$q.isNullOrEmpty($listItem)) {
      let $nexListItem = this._getToolbarDropDownListNextItem($listItem);

      if (!$q.isNullOrEmpty($nexListItem)) {
        let itemValue = this.getToolbarItemValue($item);
        let oldSubItemValue = selectedSubItemValue;
        let newSubItemValue = this._getToolbarDropDownListItemValue($nexListItem);

        if (oldSubItemValue != newSubItemValue) {
          let eventArgs = new Quantumart.QP8.BackendToolbarDropDownListEventArgs();
          eventArgs.set_itemValue(itemValue);
          eventArgs.set_oldSubItemValue(oldSubItemValue);
          eventArgs.set_newSubItemValue(newSubItemValue);

          this.notifyDropDownSelectedIndexChanging(eventArgs);
          this._updateDropDownListButton($item, $nexListItem);
          this._applyClickedStyleToToolbarDropDownButton($item);
          this._hideDropDownList($item);

          eventArgs = new Quantumart.QP8.BackendToolbarDropDownListEventArgs();
          eventArgs.set_itemValue(itemValue);
          eventArgs.set_oldSubItemValue(oldSubItemValue);
          eventArgs.set_newSubItemValue(newSubItemValue);

          this.notifyDropDownSelectedIndexChanged(eventArgs);
        } else {
          this._applyClickedStyleToToolbarDropDownButton($item);
          this._hideDropDownList($item);
        }
      }
    }
  },

  _onToolbarDropDownListItemClicking: function (e) {
    let $listItem = $(e.currentTarget);
    let $item = this._getToolbarItemByDropDownListItem($listItem);

    if (!$q.isNullOrEmpty($item)) {
      if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
        return false;
      }

      let itemValue = this.getToolbarItemValue($item);
      let oldSubItemValue = $item.data('selected_sub_item_value');
      let newSubItemValue = this._getToolbarDropDownListItemValue($listItem);

      if (oldSubItemValue != newSubItemValue) {
        let eventArgs = new Quantumart.QP8.BackendToolbarDropDownListEventArgs();
        eventArgs.set_itemValue(itemValue);
        eventArgs.set_oldSubItemValue(oldSubItemValue);
        eventArgs.set_newSubItemValue(newSubItemValue);
        this.notifyDropDownSelectedIndexChanging(eventArgs);
      }
    }
  },

  _onToolbarDropDownListItemClicked: function (e) {
    let $listItem = $(e.currentTarget);
    let $item = this._getToolbarItemByDropDownListItem($listItem);

    if (!$q.isNullOrEmpty($item)) {
      if (!this.isToolbarItemEnabled($item) || this.isToolbarBusy()) {
        return false;
      }

      let itemValue = this.getToolbarItemValue($item);
      let oldSubItemValue = $item.data('selected_sub_item_value');
      let newSubItemValue = this._getToolbarDropDownListItemValue($listItem);

      if (oldSubItemValue != newSubItemValue) {
        this._updateDropDownListButton($item, $listItem);
        this._hideDropDownList($item);

        let eventArgs = new Quantumart.QP8.BackendToolbarDropDownListEventArgs();
        eventArgs.set_itemValue(itemValue);
        eventArgs.set_oldSubItemValue(oldSubItemValue);
        eventArgs.set_newSubItemValue(newSubItemValue);

        this.notifyDropDownSelectedIndexChanged(eventArgs);

        eventArgs = null;
      } else {
        this._hideDropDownList($item);
      }
    }
  },

  dispose: function () {
    Quantumart.QP8.BackendToolbar.callBaseMethod(this, 'dispose');

    this._detachToolbarEventHandlers();

    if (this._toolbarItemListElement) {
      let $toolbarItemList = $(this._toolbarItemListElement);
      $toolbarItemList
        .empty()
        .remove()
      ;

      this._toolbarItemListElement = null;
    }

    if (this._toolbarElement) {
      let $toolbar = $(this._toolbarElement);
      $toolbar
        .empty()
        .remove();

      this._toolbarElement = null;
    }

    this._onToolbarButtonUnhoveringHandler = null;
    this._onToolbarButtonClickingHandler = null;
    this._onToolbarButtonClickedHandler = null;
    this._onToolbarDropDownArrowUnhoveringHandler = null;
    this._onToolbarDropDownArrowClickingHandler = null;
    this._onToolbarDropDownArrowClickedHandler = null;
    this._onToolbarDropDownButtonUnhoveringHandler = null;
    this._onToolbarDropDownButtonClickingHandler = null;
    this._onToolbarDropDownButtonClickedHandler = null;
    this._onToolbarDropDownListItemClickingHandler = null;
    this._onToolbarDropDownListItemClickedHandler = null;

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendToolbar.registerClass('Quantumart.QP8.BackendToolbar', Quantumart.QP8.Observable);
Quantumart.QP8.BackendToolbarButtonEventArgs = function () {
  Quantumart.QP8.BackendToolbarButtonEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendToolbarButtonEventArgs.prototype = {
  _value: '',
  _checkOnClick: false,
  _checked: false,

  get_value: function () {
    return this._value;
  },

  set_value: function (value) {
    this._value = value;
  },

  get_checkOnClick: function () {
    return this._checkOnClick;
  },

  set_checkOnClick: function (value) {
    this._checkOnClick = value;
  },

  get_checked: function () {
    return this._checked;
  },

  set_checked: function (value) {
    this._checked = value;
  }
};

Quantumart.QP8.BackendToolbarButtonEventArgs.registerClass('Quantumart.QP8.BackendToolbarButtonEventArgs', Sys.EventArgs);
Quantumart.QP8.BackendToolbarDropDownListEventArgs = function () {
  Quantumart.QP8.BackendToolbarDropDownListEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendToolbarDropDownListEventArgs.prototype = {
  _itemValue: '',
  _oldSubItemValue: '',
  _newSubItemValue: '',

  get_itemValue: function () {
    return this._itemValue;
  },

  set_itemValue: function (value) {
    this._itemValue = value;
  },

  get_oldSubItemValue: function () {
    return this._oldSubItemValue;
  },

  set_oldSubItemValue: function (value) {
    this._oldSubItemValue = value;
  },

  get_newSubItemValue: function () {
    return this._newSubItemValue;
  },

  set_newSubItemValue: function (value) {
    this._newSubItemValue = value;
  }
};

Quantumart.QP8.BackendToolbarDropDownListEventArgs.registerClass('Quantumart.QP8.BackendToolbarDropDownListEventArgs', Sys.EventArgs);
