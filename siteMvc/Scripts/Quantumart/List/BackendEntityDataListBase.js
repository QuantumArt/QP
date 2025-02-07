/* eslint max-lines: 'off' */
import { Backend } from '../Backend';
import { BackendActionType } from '../Info/BackendActionType';
import { BackendEventArgs } from '../Common/BackendEventArgs';
import { BackendSelectPopupWindow } from './BackendSelectPopupWindow';
import { Observable } from '../Common/Observable';
import { $c } from '../ControlHelpers';
import { $o } from '../Info/BackendEntityObject';
import { $q } from '../Utils';


window.EVENT_TYPE_ENTITY_LIST_ACTION_EXECUTING = 'OnEntityListActionExecuting';
window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED = 'OnEntityListSelectionChanged';

Quantumart.QP8.Enums.DataListType = {
  None: 0,
  DropDownList: 1,
  RadioButtonList: 2,
  CheckBoxList: 3,
  BulletedList: 4,
  SingleItemPicker: 5,
  MultipleItemPicker: 6
};

export class BackendEntityDataListBase extends Observable {
  // eslint-disable-next-line max-params
  constructor(
    listGroupCode,
    listElementId,
    entityTypeCode,
    parentEntityId,
    entityId,
    listType,
    options
  ) {
    super();
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

      if ($q.any(options.filter)) {
        this._filter = options.filter.slice(0);
        this._initFilter = this._filter.slice(0);
      }

      this._hostIsWindow = options.hostIsWindow;
      this._isCollapsable = options.isCollapsable;
      this._enableCopy = options.enableCopy;
      this._readDataOnInsert = options.readDataOnInsert;
      this._countLimit = options.countLimit;
    }
  }

  _listGroupCode = '';
  _listElementId = '';
  _listElement = null;
  _expandLinkElement = null;
  _collapseLinkElement = null;
  _listWrapperElement = null;
  _toolbarElement = null;
  _entityTypeCode = '';
  _entityId = 0;
  _parentEntityId = 0;
  _listType = 0;
  _listId = 0;
  _listItemName = '';
  _allowMultipleItemSelection = false;
  _selectionMode = null;
  _maxListWidth = 0;
  _maxListHeight = 0;
  _addNewActionCode = '';
  _readActionCode = '';
  _selectActionCode = '';
  _addNewButtonElement = null;
  _readButtonElement = null;
  _stopDeferredOperations = false;
  _listManagerComponent = null;
  _selectPopupWindowComponent = null;
  _showIds = false;
  _filter = [];
  _initFilter = [];
  _hostIsWindow = false;
  _isCollapsable = false;
  _enableCopy = true;
  _readDataOnInsert = false;

  TOOLBAR_BUTTON_DISABLED_CLASS_NAME = 'disabled';
  LIST_WRAPPER_CLASS_NAME = 'dataListWrapper';
  LINK_BUTTON_CLASS_NAME = 'linkButton';
  ACTION_LINK_CLASS_NAME = 'actionLink';
  LINK_BUTTON_LIST_CLASS_NAME = 'linkButtons';
  OVERFLOW_LIST_CLASS_NAME = 'overflow';
  LIST_DISABLED_CLASS_NAME = 'disabled';
  SELF_CLEAR_FLOATS_CLASS_NAME = 'group';

  // eslint-disable-next-line camelcase
  get_listElementId() {
    return this._listElementId;
  }

  // eslint-disable-next-line camelcase
  set_listElementId(value) {
    this._listElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_entityTypeCode() {
    return this._entityTypeCode;
  }

  // eslint-disable-next-line camelcase
  set_entityTypeCode(value) {
    this._entityTypeCode = value;
  }

  // eslint-disable-next-line camelcase
  get_parentEntityId() {
    return this._parentEntityId;
  }

  // eslint-disable-next-line camelcase
  set_parentEntityId(value) {
    this._parentEntityId = value;
  }

  // eslint-disable-next-line camelcase
  get_entityId() {
    return this._entityId;
  }

  // eslint-disable-next-line camelcase
  set_entityId(value) {
    this._entityId = value;
  }

  // eslint-disable-next-line camelcase
  get_listType() {
    return this._listType;
  }

  // eslint-disable-next-line camelcase
  set_listType(value) {
    this._listType = value;
  }

  // eslint-disable-next-line camelcase
  get_listId() {
    return this._listId;
  }

  // eslint-disable-next-line camelcase
  set_listId(value) {
    this._listId = value;
  }

  // eslint-disable-next-line camelcase
  get_addNewActionCode() {
    return this._addNewActionCode;
  }

  // eslint-disable-next-line camelcase
  set_addNewActionCode(value) {
    this._addNewActionCode = value;
  }

  // eslint-disable-next-line camelcase
  get_readActionCode() {
    return this._readActionCode;
  }

  // eslint-disable-next-line camelcase
  set_readActionCode(value) {
    this._readActionCode = value;
  }

  // eslint-disable-next-line camelcase
  get_selectActionCode() {
    return this._selectActionCode;
  }

  // eslint-disable-next-line camelcase
  set_selectActionCode(value) {
    this._selectActionCode = value;
  }

  // eslint-disable-next-line camelcase
  get_listGroupCode() {
    return this._listGroupCode;
  }

  // eslint-disable-next-line camelcase
  get_listManagerComponent() {
    return this._listManagerComponent;
  }

  // eslint-disable-next-line camelcase
  set_listManagerComponent(value) {
    this._listManagerComponent = value;
  }

  initialize() {
    if (!this._listElementId) {
      $q.alertFail('_listElementId');
    }
    const $list = $(`#${this._listElementId}`);
    $list.wrap('<div />');

    const $listWrapper = $list.parent();
    $listWrapper.addClass(this.LIST_WRAPPER_CLASS_NAME);

    const $toolbar = $('<ul />', { class: this.LINK_BUTTON_LIST_CLASS_NAME });
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
  }

  _createCollapsingToolbar(mainWrapperElement) {
    if (mainWrapperElement) {
      const html = new $.telerik.stringBuilder();
      html
        .cat("<div class='collapsingToolbar'>")
        .cat("<ul class='linkButtons group'>")
        .cat("<li style='display: block;' class='expand'>")
        .cat("<span class='linkButton actionLink'>")
        .cat("<a href='javascript:void(0);'>")
        .cat("<span class='icon expand'><img src='Static/Common/0.gif'></span>")
        .cat("<span class='text'>")
        .cat($l.EntityDataList.showListLinkButtonText)
        .cat('</span>')
        .cat('</a>')
        .cat('</span>')
        .cat('</li>')
        .cat("<li style='display: none;' class='collapse'>")
        .cat("<span class='linkButton actionLink'>")
        .cat("<a href='javascript:void(0);'>")
        .cat("<span class='icon collapse'><img src='Static/Common/0.gif'></span>")
        .cat("<span class='text'>")
        .cat($l.EntityDataList.hideListLinkButtonText)
        .cat('</span>')
        .cat('</a>')
        .cat('</span>')
        .cat('</li>')
        .cat('</ul>')
        .cat('</div>');

      let $collapsingToolbar = $(html.string());
      const that = this;
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

      $(mainWrapperElement).prepend($collapsingToolbar).find(`.${this.LIST_WRAPPER_CLASS_NAME}`).hide();
      $collapsingToolbar = null;
    }
  }

  _fixListOverflow() {
    let $list = $(this._listElement);
    $list.removeClass(this.OVERFLOW_LIST_CLASS_NAME);

    let $ul = $('UL', $list);
    if ($ul.length > 0) {
      $list.height($ul.height());
      const contentHeight = $list.get(0).scrollHeight;
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
  }

  /**
   * @abstract
   * @returns {JQuery}
   */
  getListItems() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @virtual
   * @returns {number}
   */
  getListItemCount() {
    return this.getListItems().length;
  }

  /**
   * @abstract
   * @returns {JQuery}
   */
  getSelectedListItems() {
    throw new Error($l.Common.methodNotImplemented);
  }

  getSelectedListItemCount() {
    return this.getSelectedListItems().length;
  }

  /**
   * @abstract
   * @returns {{Id: number, Name: string}[]}}
   */
  getSelectedEntities() {
    throw new Error($l.Common.methodNotImplemented);
  }

  getSelectedEntityIDs() {
    return this.getSelectedEntities().map(item => $q.toInt(item.Id)).filter(i => i);
  }

  refreshList(testEntityId) {
    const selectedEntitiesIDs = this.getSelectedEntities().map(el => el.Id);
    const dataItems = $o.getSimpleEntityList(
      this._entityTypeCode, this._parentEntityId, this._entityId, this._listId,
      this._selectionMode, selectedEntitiesIDs, this._filter, testEntityId
    );
    if (dataItems) {
      this._refreshListInner(dataItems, true);
      this._fixListOverflow();
    }
  }

  /**
   * @abstract
   * @param {any[]} _dataItems
   * @param {boolean} _refreshOnly
   */
  _refreshListInner(_dataItems, _refreshOnly) {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  selectAllListItems() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  deselectAllListItems() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  enableList() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  disableList() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /** @abstract */
  makeReadonly() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @returns {boolean}
   */
  isListChanged() {
    throw new Error($l.Common.methodNotImplemented);
  }

  /**
   * @abstract
   * @returns {boolean}
   */
  isListOverflow() {
    throw new Error($l.Common.methodNotImplemented);
  }

  isListDisabled() {
    return $(this._listElement).hasClass(this.LIST_DISABLED_CLASS_NAME);
  }

  _showToolbar() {
    $(this._toolbarElement).css('display', 'block');
  }

  _hideToolbar() {
    $(this._toolbarElement).css('display', 'none');
  }

  /**
   * @virtual
   * @returns {boolean}
   */
  _checkAllowShowingToolbar() {
    return true;
  }

  _getToolbarButtons() {
    return $(`.${this.LINK_BUTTON_CLASS_NAME}`, this._toolbarElement);
  }

  _createToolbarButton(id, text, cssClassName) {
    return this._createLinkButton(id, text, cssClassName);
  }

  _isToolbarButtonDisabled(buttonElem) {
    let isDisabled;
    const $linkButton = $q.toJQuery(buttonElem);
    const actionLink = $linkButton.data('action_link_component');

    if (actionLink) {
      isDisabled = actionLink.isActionLinkDisabled();
    } else {
      isDisabled = $linkButton.find('A:first').hasClass(this.TOOLBAR_BUTTON_DISABLED_CLASS_NAME);
    }

    return isDisabled;
  }

  _changeToolbarButtonState(buttonElem, state) {
    const $linkButton = $q.toJQuery(buttonElem);
    const actionLink = $linkButton.data('action_link_component');

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
  }

  _createLinkButton(id, text, cssClassName) {
    const $linkButton = $('<span />', { id, class: `${this.LINK_BUTTON_CLASS_NAME} ${this.ACTION_LINK_CLASS_NAME}` });
    const linkContentHtml = new $.telerik.stringBuilder();

    linkContentHtml
      .cat('<a href="javascript:void(0);">')
      .cat(`<span class="icon${$q.isNullOrWhiteSpace(cssClassName) ? '' : ` ${$q.htmlEncode(cssClassName)}`}">`)
      .cat(`<img src="${window.COMMON_IMAGE_FOLDER_URL_ROOT}0.gif" />`)
      .cat('</span>')
      .cat(`<span class="text">${text}</span>`)
      .cat('</a>');

    $linkButton.html(linkContentHtml.string());
    return $linkButton;
  }

  _extendLink($linkButton, actionCode) {
    if (!$q.isNullOrEmpty($linkButton)) {
      const entityTypeCode = this._entityTypeCode;
      let entityId = 0;
      let entityName = '';
      const parentEntityId = this._parentEntityId;
      const actionTypeCode = BackendActionType.getActionTypeCodeByActionCode(actionCode);
      if (actionTypeCode === window.ACTION_TYPE_CODE_READ) {
        const entities = this.getSelectedEntities();
        if (entities.length > 0) {
          const [entity] = entities;
          entityId = entity.Id;
          entityName = $o.getEntityName(entityTypeCode, entityId, parentEntityId);
        }

        $q.clearArray(entities);
      }

      $linkButton.data('entity_id', entityId);
      $linkButton.data('entity_name', entityName);
      $linkButton.data('parent_entity_id', parentEntityId);
      $linkButton.data('action_type_code', actionTypeCode);
      $linkButton.data('action_code', actionCode);
      if (actionTypeCode === window.ACTION_TYPE_CODE_ADD_NEW && entityTypeCode === window.ENTITY_TYPE_CODE_ARTICLE) {
        $linkButton.data('context', {
          additionalUrlParameters: {
            fieldId: this._listId,
            articleId: this._entityId
          }
        });
      }
      const actionTarget = this._hostIsWindow ? Quantumart.QP8.Enums.ActionTargetType.NewWindow
        : Quantumart.QP8.Enums.ActionTargetType.NewTab;
      $linkButton.data('action_target_type', actionTarget);
    }
  }

  _addButtonToToolbar(buttonElem) {
    const $button = $q.toJQuery(buttonElem);
    $(this._toolbarElement).append($button);
    $button.wrap('<li />');
  }

  _addLinkToToolbar(linkElem, actionCode) {
    const $link = $q.toJQuery(linkElem);
    this._addButtonToToolbar($link);
    this._extendLink($link, actionCode);
  }

  _addNewButtonToToolbar() {
    if (this._addNewActionCode !== window.ACTION_CODE_NONE) {
      const $addNewButton = this._createToolbarButton(
        `${this._listElementId}_AddNewButton`, $l.EntityDataList.addNewActionLinkButtonText, 'add'
      );
      this._addLinkToToolbar($addNewButton, this._addNewActionCode);
      this._addNewButtonElement = $addNewButton.get(0);
    }
  }

  _addReadButtonToToolbar() {
    if (this._readActionCode !== window.ACTION_CODE_NONE) {
      const $readActionButton = this._createToolbarButton(
        `${this._listElementId}_ReadButton`, $l.EntityDataList.readActionLinkButtonText, 'edit'
      );
      this._changeToolbarButtonState($readActionButton, this.getSelectedListItemCount() > 0);
      this._addLinkToToolbar($readActionButton, this._readActionCode);
      this._readButtonElement = $readActionButton.get(0);
    }
  }

  _refreshReadToolbarButton(refreshActionLinkProperties) {
    if (this._readButtonElement) {
      let $readActionButton = $(this._readButtonElement);
      this._changeToolbarButtonState($readActionButton, this.getSelectedListItemCount() > 0);

      const refresh = $q.isNull(refreshActionLinkProperties) ? false : refreshActionLinkProperties;
      if (refresh) {
        const actionLink = $readActionButton.data('action_link_component');
        if (actionLink) {
          let entityId = 0;
          let entityName = '';

          const entities = this.getSelectedEntities();
          if (entities.length > 0) {
            const [entity] = entities;
            entityId = entity.Id;
            entityName = $o.getEntityName(this._entityTypeCode, entityId, this._parentEntityId);
          }

          $q.clearArray(entities);

          actionLink.set_entityId(entityId);
          actionLink.set_entityName(entityName);
        }
      }

      $readActionButton = null;
    }
  }

  _enableAllToolbarButtons() {
    let $linkButtons = this._getToolbarButtons();
    const linkButtonCount = $linkButtons.length;

    for (let linkButtonIndex = 0; linkButtonIndex < linkButtonCount; linkButtonIndex++) {
      const $linkButton = $linkButtons.eq(linkButtonIndex);
      this._changeToolbarButtonState($linkButton, true);
    }

    $linkButtons = null;

    this._refreshReadToolbarButton();
  }

  _disableAllToolbarButtons() {
    let $linkButtons = this._getToolbarButtons();
    const linkButtonCount = $linkButtons.length;

    for (let linkButtonIndex = 0; linkButtonIndex < linkButtonCount; linkButtonIndex++) {
      const $linkButton = $linkButtons.eq(linkButtonIndex);
      if ($linkButton.data('action_code') !== this._readActionCode) {
        this._changeToolbarButtonState($linkButton, false);
      }
    }

    $linkButtons = null;
  }

  _openPopupWindow() {
    let eventArgs = new BackendEventArgs();
    eventArgs.set_isMultipleEntities(this._allowMultipleItemSelection);
    eventArgs.set_parentEntityId(this._parentEntityId);
    eventArgs.set_entityTypeCode(this._entityTypeCode);
    eventArgs.set_actionCode(this._selectActionCode);
    const entities = this.getSelectedEntities();
    if (entities.length > 0) {
      if (this._allowMultipleItemSelection) {
        eventArgs.set_entities(entities);
      } else {
        eventArgs.set_entityId(entities[0].Id);
        eventArgs.set_entityName(entities[0].Name);
      }
    }

    this._selectPopupWindowComponent = new BackendSelectPopupWindow(eventArgs, {
      filter: this._filter
    });

    this._selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, $.proxy(this._popupWindowSelectedHandler, this)
    );
    this._selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, $.proxy(this._popupWindowClosedHandler, this)
    );
    this._selectPopupWindowComponent.openWindow();

    eventArgs = null;
  }

  _popupWindowSelectedHandler(eventType, sender, args) {
    this._loadSelectedItems(args.entities);
    this._destroyPopupWindow();
  }

  _loadSelectedItems(entities) {
    let dataItems;
    if (($o.checkEntitiesForPresenceEmptyNames(entities) || this._readDataOnInsert)
      && entities.length <= this._countLimit) {
      const selectedEntitiesIDs = entities.map(el => el.Id);
      dataItems = $o.getSimpleEntityList(
        this._entityTypeCode, this._parentEntityId, this._entityId, this._listId,
        this._selectionMode, selectedEntitiesIDs, this._filter, 0
      );
    } else {
      dataItems = $c.getListItemCollectionFromEntities(entities);
    }

    if (dataItems) {
      this._refreshListInner(dataItems, false);
      this._fixListOverflow();
    }

    $q.clearArray(dataItems);
    this.notify(window.EVENT_TYPE_ENTITY_LIST_SELECTION_CHANGED, new BackendEventArgs());
  }

  _popupWindowClosedHandler() {
    this._destroyPopupWindow();
  }

  _destroyPopupWindow() {
    if (this._selectPopupWindowComponent) {
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      this._selectPopupWindowComponent.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      this._selectPopupWindowComponent.closeWindow();
      this._selectPopupWindowComponent.dispose();
      this._selectPopupWindowComponent = null;
    }
  }

  _getGroupCheckboxHtml(hidden) {
    const idFormat = '{0}_{1}';
    const idPrefix = 'group';
    const itemElementName = String.format(idFormat, idPrefix, this._listItemName);
    const itemElementId = String.format(idFormat, idPrefix, this._listElementId);
    const itemText = $l.EntityDataList.selectDeselectAllText;
    const style = hidden ? 'style="display:none"' : '';

    const html = new $.telerik.stringBuilder();
    html
      .cat(`<div class="groupCheckbox" ${style}>`)
      .cat('<input type="checkbox"')
      .cat(` name="${$q.htmlEncode(itemElementName)}"`)
      .cat(` id="${$q.htmlEncode(itemElementId)}"`)
      .cat(' value="1"')
      .cat(' class="checkbox group qp-notChangeTrack"')
      .cat('/>')
      .cat(' ')
      .cat(`<label for="${$q.htmlEncode(itemElementId)}">${itemText}</label>`)
      .cat('</div>');

    return html.string();
  }

  _getCountDivHtml(newCount, hidden) {
    const html = new $.telerik.stringBuilder();
    const style = hidden ? 'style="display:none"' : '';
    const countText = String.format($l.EntityDataList.countText, `<span class="countItems">${newCount}</span>`);
    const overflowText = `<span class="overflowText">${this._getOverflowText()}</span>`;
    html.cat(`<div class="countItemsBlock" ${style}>`).cat(countText).cat(overflowText).cat('</div>');

    return html.string();
  }

  _refreshGroupCheckbox(newCount) {
    if (newCount > 1 && !this._isCountOverflow()) {
      this._showGroupCheckbox();
    } else {
      this._hideGroupCheckbox();
    }

    this._syncGroupCheckbox();
  }

  _addGroupCheckbox(hidden) {
    $(this._toolbarElement).after(this._getGroupCheckboxHtml(hidden));
    this._getGroupCheckbox().bind('click', $.proxy(this._groupCheckboxClickHandler, this));
  }

  _showGroupCheckbox() {
    this._getGroupCheckbox().parent().show();
  }

  _hideGroupCheckbox() {
    this._getGroupCheckbox().parent().hide();
  }

  _destroyGroupCheckbox() {
    this._getGroupCheckbox().unbind('click').parent().remove();
  }

  _addCountDiv(count, hidden) {
    $(this._toolbarElement).after(this._getCountDivHtml(count, hidden));
  }

  _destroyCountDiv() {
    this._getCountSpan().parent().remove();
  }

  _getGroupCheckbox() {
    return $(this._listWrapperElement).find('.groupCheckbox INPUT:checkbox');
  }

  _getCountSpan() {
    return $(this._listWrapperElement).find('.countItems');
  }

  _getOverflowSpan() {
    return $(this._listWrapperElement).find('.overflowText');
  }

  _groupCheckboxClickHandler() {
    if (this._getGroupCheckbox().is(':checked')) {
      this.selectAllListItems();
    } else {
      this.deselectAllListItems();
    }

    this._syncCountSpan();
  }

  _syncGroupCheckbox() {
    this._getGroupCheckbox().prop('checked', this.getListItemCount() === this.getSelectedListItemCount());
  }

  _syncCountSpan(count) {
    this._getCountSpan().html(`${count || this.getSelectedEntities().length}`);
    this._getOverflowSpan().html(this._getOverflowText());
  }

  _getOverflowText() {
    return this._isCountOverflow() ? `. ${String.format($l.EntityDataList.overFlowText, this._countLimit)}` : '';
  }

  _onItemClickHandler(e) {
    e.preventDefault();
    const isLeftClick = e.type === 'click' && (e.which === 1 || e.which === 0);
    const isMiddleClick = e.type === 'mouseup' && e.which === 2;
    if (!isLeftClick && !isMiddleClick) {
      return false;
    }

    const $obj = $(e.target);
    const eventArgs = new BackendEventArgs();
    eventArgs.set_actionCode(this._readActionCode);
    eventArgs.set_entityId($obj.html());
    eventArgs.set_entityName($obj.closest('LI').children('LABEL').html());
    eventArgs.set_entityTypeCode(this._entityTypeCode);
    eventArgs.set_parentEntityId(this._parentEntityId);

    if (this._hostIsWindow) {
      const message = Backend.getInstance().checkOpenDocumentByEventArgs(eventArgs);
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
    return undefined;
  }

  _getIdLinkCode(value) {
    return this._showIds
      ? `<span class="idLink">(<a class="js" href="javascript:void(0)">${$q.htmlEncode(value)}</a>)</span>`
      : '';
  }

  _getValueStorage() {
    return this._listManagerComponent.getValueStorage();
  }

  _onCopyButtonClickHandler() {
    const storage = this._getValueStorage();
    storage.parentEntityId = this._parentEntityId;
    storage.entityTypeCode = this._entityTypeCode;
    storage.filter = this._filter;
    storage.entities = this.getSelectedEntities();
  }
  _onPasteButtonClickHandler() {
    const storage = this._getValueStorage();
    $q.warnIfEqDiff(storage.parentEntityId, this._parentEntityId);
    if (storage.entities) {
      if (storage.entityTypeCode !== this._entityTypeCode) {
        $q.alertError($l.EntityDataList.typesDoesNotMatchMessage);
      } else if (storage.parentEntityId !== this._parentEntityId) {
        $q.alertError($l.EntityDataList.parentsDoesNotMatchMessage);
      } else if (JSON.stringify(storage.filter) === JSON.stringify(this._filter)) {
        this._loadSelectedItems(storage.entities);
      } else {
        $q.alertError($l.EntityDataList.filtersDoesNotMatchMessage);
      }
    }
  }

  /**
   * @virtual
   * @returns {boolean}
   */
  _isCountOverflow() {
    return false;
  }

  setFilter(filter) {
    if (filter) {
      const oldValue = JSON.stringify(this._filter);

      if (Array.isArray(filter)) {
        this._filter = filter;
      } else {
        this._filter = [filter];
      }

      const newValue = JSON.stringify(this._filter);

      return oldValue !== '[]' && oldValue !== newValue;
    }

    return false;
  }

  applyFilter(filter) {
    const result = this._initFilter.slice(0);
    if (filter) {
      result.push(filter);
    }

    if (JSON.stringify(result) !== JSON.stringify(this._filter)) {
      this._filter = result;
      if (this.getListItemCount() > 0) {
        this.refreshList();
      }
    }
  }

  getFilter() {
    return this._filter;
  }

  dispose() {
    super.dispose();
    this._destroyPopupWindow();
    if (this._toolbarElement) {
      $(this._toolbarElement).empty().remove();
    }

    this._destroyGroupCheckbox();
    this._destroyCountDiv();

    if (this._listWrapperElement) {
      $(this._listElement).unwrap();
    }

    if (this._listManagerComponent) {
      const listElementId = this._listElementId;
      if (!$q.isNullOrWhiteSpace(listElementId)) {
        this._listManagerComponent.removeList(listElementId);
      }
    }

    if (this._expandLinkElement) {
      $(this._expandLinkElement).off();
    }
    if (this._collapseLinkElement) {
      $(this._collapseLinkElement).off();
    }

    this._addNewButtonElement = null;
    this._readButtonElement = null;
    this._toolbarElement = null;
    this._listWrapperElement = null;
    this._listManagerComponent = null;
    this._expandLinkElement = null;
    this._collapseLinkElement = null;
    this._listElement = null;
    $q.collectGarbageInIE();
  }
}


Quantumart.QP8.BackendEntityDataListBase = BackendEntityDataListBase;
