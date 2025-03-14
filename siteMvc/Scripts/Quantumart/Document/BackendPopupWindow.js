/* eslint max-lines: 'off' */
import { BackendActionToolbar } from '../Toolbar/BackendActionToolbar';
import { BackendBreadCrumbsManager } from '../Managers/BackendBreadCrumbsManager';
import { BackendDocumentHost } from './BackendDocumentHost';
import { BackendEventArgs } from '../Common/BackendEventArgs';
import { BackendLibrary } from '../Library/BackendLibrary';
import { BackendBrowserHistoryManager } from '../Managers/BackendBrowserHistoryManager';
import { BackendSearchBlockManager } from '../Managers/BackendSearchBlockManager';
import { BackendViewToolbar } from '../Toolbar/BackendViewToolbar';
import { $a } from '../BackendActionExecutor';
import { $c } from '../ControlHelpers';
import { $o } from '../Info/BackendEntityObject';
import { $q } from '../Utils';


window.EVENT_TYPE_POPUP_WINDOW_ACTION_EXECUTING = 'OnPopupWindowActionExecuting';
window.EVENT_TYPE_POPUP_WINDOW_ENTITY_READED = 'OnPopupWindowEntityReaded';
window.EVENT_TYPE_POPUP_WINDOW_CLOSED = 'OnPopupWindowClosed';

export class BackendPopupWindow extends BackendDocumentHost {
  static get isWindow() {
    return true;
  }

  _backendBrowserHistoryManager = BackendBrowserHistoryManager.getInstance();

  // eslint-disable-next-line max-statements, complexity
  constructor(popupWindowId, eventArgs, options) {
    super(eventArgs, options);

    this._popupWindowId = '';
    this._showBreadCrumbs = false;
    this._saveSelectionWhenChangingView = false;
    this._title = '';
    this._width = 400;
    this._height = 300;
    this._minWidth = 400;
    this._minHeight = 300;
    this._isModal = true;
    this._allowResize = true;
    this._allowDrag = true;
    this._showRefreshButton = false;
    this._showCloseButton = true;
    this._showMaximizeButton = true;
    this._zIndex = 0;
    this._isMultiOpen = false;
    this._closeWithoutCheck = false;

    const $currentWindow = $(window);
    const currentWindowWidth = $currentWindow.width();
    const currentWindowHeight = $currentWindow.height();

    this._popupWindowId = popupWindowId;
    if ($q.isObject(eventArgs)) {
      this._applyEventArgs(eventArgs, true);
      this.bindExternalCallerContext(eventArgs);
    }

    this._loadDefaultSearchBlockState();
    if ($q.isObject(options)) {
      if (!$q.isNull(options.showBreadCrumbs)) {
        this._showBreadCrumbs = options.showBreadCrumbs;
      }

      if (options.customActionToolbarComponent) {
        this._actionToolbarComponent = options.customActionToolbarComponent;
        this._useCustomActionToolbar = true;
      }

      if (!$q.isNull(options.saveSelectionWhenChangingView)) {
        this._saveSelectionWhenChangingView = options.saveSelectionWhenChangingView;
      }

      if (options.title) {
        this._title = options.title;
      }

      if (options.width) {
        this._width = options.width;
      } else {
        this._width = Math.floor(currentWindowWidth * 0.8);
      }

      if (options.height) {
        this._height = options.height;
      } else {
        this._height = Math.floor(currentWindowHeight * 0.8);
      }

      if (options.minWidth) {
        this._minWidth = options.minWidth;
      } else {
        this._minWidth = Math.floor(currentWindowWidth * 0.2);
      }

      if (options.minHeight) {
        this._minHeight = options.minHeight;
      } else {
        this._minHeight = Math.floor(currentWindowHeight * 0.2);
      }

      if (!$q.isNull(options.isModal)) {
        this._isModal = options.isModal;
      }

      if (!$q.isNull(options.allowResize)) {
        this._allowResize = options.allowResize;
      }

      if (!$q.isNull(options.allowDrag)) {
        this._allowDrag = options.allowDrag;
      }

      if (!$q.isNull(options.showCloseButton)) {
        this._showCloseButton = options.showCloseButton;
      }

      if (!$q.isNull(options.showMinimizeButton)) {
        this._showMinimizeButton = options.showMinimizeButton;
      }

      if (!$q.isNull(options.showMaximizeButton)) {
        this._showMaximizeButton = options.showMaximizeButton;
      }

      if (options.additionalUrlParameters) {
        this._additionalUrlParameters = options.additionalUrlParameters;
      }
      if (eventArgs.get_context() && eventArgs.get_context().additionalUrlParameters) {
        this._additionalUrlParameters = Object.assign(
          {},
          this._additionalUrlParameters,
          eventArgs.get_context().additionalUrlParameters
        );
      }

      if (options.zIndex) {
        this._zIndex = $q.toInt(options.zIndex);
      }

      if (options.filter) {
        this._filter = options.filter;
      }

      if (options.isMultiOpen) {
        this._isMultiOpen = options.isMultiOpen;
      }
    }

    this._selectedEntities = [];
    this._onPopupWindowResizeHandler = $.proxy(this._onPopupWindowResize, this);
    this._onPopupWindowOpenHandler = $.proxy(this._onPopupWindowOpen, this);
    this._onPopupWindowCloseHandler = $.proxy(this._onPopupWindowClose, this);
    this._onPopupWindowActivatedHandler = $.proxy(this._onPopupWindowActivated, this);
  }

  // eslint-disable-next-line camelcase
  get_popupWindowId() {
    return this._popupWindowId;
  }

  // eslint-disable-next-line camelcase
  get_showBreadCrumbs() {
    return this._showBreadCrumbs;
  }

  // eslint-disable-next-line camelcase
  get_saveSelectionWhenChangingView() {
    return this._saveSelectionWhenChangingView;
  }

  // eslint-disable-next-line camelcase
  set_saveSelectionWhenChangingView(value) {
    this._saveSelectionWhenChangingView = value;
  }

  // eslint-disable-next-line camelcase
  get_title() {
    return this._title;
  }

  // eslint-disable-next-line camelcase
  set_title(value) {
    this._title = value;
  }

  // eslint-disable-next-line camelcase
  get_width() {
    return this._width;
  }

  // eslint-disable-next-line camelcase
  set_width(value) {
    this._width = value;
  }

  // eslint-disable-next-line camelcase
  get_height() {
    return this._height;
  }

  // eslint-disable-next-line camelcase
  set_height(value) {
    this._height = value;
  }

  // eslint-disable-next-line camelcase
  get_minWidth() {
    return this._minWidth;
  }

  // eslint-disable-next-line camelcase
  set_minWidth(value) {
    this._minWidth = value;
  }

  // eslint-disable-next-line camelcase
  get_minHeight() {
    return this._minHeight;
  }

  // eslint-disable-next-line camelcase
  set_minHeight(value) {
    this._minHeight = value;
  }

  // eslint-disable-next-line camelcase
  get_isModal() {
    return this._isModal;
  }

  // eslint-disable-next-line camelcase
  set_isModal(value) {
    this._isModal = value;
  }

  // eslint-disable-next-line camelcase
  get_allowResize() {
    return this._allowResize;
  }

  // eslint-disable-next-line camelcase
  set_allowResize(value) {
    this._allowResize = value;
  }

  // eslint-disable-next-line camelcase
  get_allowDrag() {
    return this._allowDrag;
  }

  // eslint-disable-next-line camelcase
  set_allowDrag(value) {
    this._allowDrag = value;
  }

  // eslint-disable-next-line camelcase
  get_showRefreshButton() {
    return this._showRefreshButton;
  }

  // eslint-disable-next-line camelcase
  set_showRefreshButton(value) {
    this._showRefreshButton = value;
  }

  // eslint-disable-next-line camelcase
  get_showCloseButton() {
    return this._showCloseButton;
  }

  // eslint-disable-next-line camelcase
  set_showCloseButton(value) {
    this._showCloseButton = value;
  }

  // eslint-disable-next-line camelcase
  get_showMaximizeButton() {
    return this._showMaximizeButton;
  }

  // eslint-disable-next-line camelcase
  set_showMaximizeButton(value) {
    this._showMaximizeButton = value;
  }

  // eslint-disable-next-line camelcase
  get_popupWindowManager() {
    return this._popupWindowManagerComponent;
  }

  // eslint-disable-next-line camelcase
  set_popupWindowManager(value) {
    this._popupWindowManagerComponent = value;
  }

  // eslint-disable-next-line camelcase, class-methods-use-this
  get_hostType() {
    return window.DOCUMENT_HOST_TYPE_POPUP_WINDOW;
  }

  // eslint-disable-next-line camelcase
  get_zIndex() {
    return parseInt($(this._popupWindowElement).css('z-index'), 10);
  }

  // eslint-disable-next-line camelcase
  get_popupWindowElement() {
    return this._popupWindowElement;
  }

  // eslint-disable-next-line camelcase
  get_selectionContext() {
    return this._selectionContext;
  }

  // eslint-disable-next-line camelcase
  set_selectionContext(value) {
    this._selectionContext = value;
  }

  // eslint-disable-next-line camelcase
  set_closeWithoutCheck(value) {
    this._closeWithoutCheck = value;
  }

  // eslint-disable-next-line camelcase
  get_closeWithoutCheck() {
    return this._closeWithoutCheck;
  }

  initialize() {
    this._initSelectedEntities();
    const action = this.getCurrentAction();
    if ($q.isNullOrWhiteSpace(this._title) && this._popupWindowManagerComponent && action) {
      const eventArgs = new BackendEventArgs();
      eventArgs.set_entityTypeCode(this._entityTypeCode);
      eventArgs.set_entityId(this._entityId);
      eventArgs.set_entityName(this._entityName);
      eventArgs.set_parentEntityId(this._parentEntityId);
      eventArgs.set_actionCode(this._actionCode);
      this._title = BackendDocumentHost.generateTitle(eventArgs, { isTab: false });
    }

    this.createPanels();
    const popupWindowComponent = this._createWindow();
    popupWindowComponent.close = function () {
      $.telerik.trigger(popupWindowComponent.element, 'close');
    };

    const $popupWindow = $(popupWindowComponent.element);
    this._popupWindowElement = $popupWindow.get(0);
    this._popupWindowComponent = popupWindowComponent;
    this._attachPopupWindowEventHandlers();
  }

  _initSelectedEntities() {
    if (this._actionTypeCode === window.ACTION_TYPE_CODE_SELECT
      || this._actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_SELECT
    ) {
      if (this._isMultipleEntities) {
        this._selectedEntities = this._entities.slice();
      } else if (this._entityId && this._entityName) {
        this._selectedEntities = [{ Id: this._entityId, Name: this._entityName }];
      } else {
        this._selectedEntities = [];
      }
    }
  }

  generateDocumentUrl(options) {
    const isSelectAction = this._actionTypeCode === window.ACTION_TYPE_CODE_SELECT
      || this._actionTypeCode === window.ACTION_TYPE_CODE_MULTIPLE_SELECT;

    const entityIDs = this._isMultipleEntities
      ? $o.getEntityIDsFromEntities(isSelectAction ? this._selectedEntities : this._entities)
      : [this._entityId];

    const extraOptions = {
      additionalUrlParameters: this._additionalUrlParameters,
      controllerActionUrl: this.getCurrentViewActionUrl()
    };

    if (this.get_isBindToExternal()) {
      extraOptions.additionalUrlParameters = Object.assign(
        {},
        extraOptions.additionalUrlParameters,
        { boundToExternal: true }
      );
    }

    const newOptions = $q.isObject(options) ? Object.assign({}, options, extraOptions) : extraOptions;
    this._documentUrl = $a.generateActionUrl(
      this._isMultipleEntities,
      entityIDs,
      this._parentEntityId,
      this._popupWindowId,
      this.getCurrentAction(),
      newOptions
    );

    const params = {};
    if (this._isMultipleEntities || this._isCustomAction) {
      params.IDs = entityIDs;
    }
    if (this._isCustomAction) {
      params.actionCode = this._actionCode;
    }

    this._documentPostParams = params;
  }

  // eslint-disable-next-line max-statements
  _createWindow() {
    const popupWindowId = this._popupWindowId;
    const actions = [];
    if (this._showRefreshButton) {
      Array.add(actions, 'Refresh');
    }

    if (this._showMaximizeButton) {
      Array.add(actions, 'Maximize');
    }

    if (this._showCloseButton) {
      Array.add(actions, 'Close');
    }

    const breadCrumbsWrapperId = `breadCrumbsWrapper_${popupWindowId}`;
    const toolbarWrapperId = `toolbarWrapper_${popupWindowId}`;
    const actionToolbarWrapperId = `actionToolbarWrapper_${popupWindowId}`;
    const viewToolbarWrapperId = `viewToolbarWrapper_${popupWindowId}`;
    const searchBlockWrapperId = `searchBlockWrapper_${popupWindowId}`;
    const contextBlockWrapperId = `contextBlockWrapper_${popupWindowId}`;
    const documentAreaId = `documentArea_${popupWindowId}`;
    const documentWrapperId = `document_${popupWindowId}`;
    const windowContentHtml = new $.telerik.stringBuilder();
    windowContentHtml
      .catIf(`<div id="${breadCrumbsWrapperId}" class="breadCrumbsWrapper"></div>`, this._showBreadCrumbs)
      .cat(`<div id="${toolbarWrapperId}" class="toolbarWrapper">\n`)
      .cat(`  <div id="${actionToolbarWrapperId}" class="actionToolbarWrapper"></div>\n`)
      .cat(`  <div id="${viewToolbarWrapperId}" class="viewToolbarWrapper"></div>\n`)
      .cat('</div>\n')
      .cat(`<div id="${documentAreaId}" class="area vertical-layout vertical-layout__main">`)
      .cat(`  <div id="${searchBlockWrapperId}" class="searchWrapper"></div>`)
      .cat(`  <div id="${contextBlockWrapperId}" class="contextWrapper"></div>`)
      .cat(`  <div id="${documentWrapperId}" class="documentWrapper vertical-layout__main"></div>`)
      .cat('</div>\n');

    const popupWindowComponent = $.telerik.window.create({
      title: $('<div/>').text(this._title).html(),
      html: windowContentHtml.string(),
      width: this._width,
      height: this._height,
      minWidth: this._minWidth,
      minHeight: this._minHeight,
      modal: this._isModal,
      actions,
      resizable: this._allowResize,
      draggable: this._allowDrag,
      effects: {
        list: [{ name: 'toggle' }, {
          name: 'property', properties: ['opacity']
        }],
        openDuration: 'fast',
        closeDuration: 'fast'
      },
      onOpen: this._backendBrowserHistoryManager.handleModalWindowOpen,
      onClose: this._backendBrowserHistoryManager.handleModalWindowClose
    }).data('tWindow').center();

    const $popupWindow = $(popupWindowComponent.element);
    $popupWindow.addClass('popupWindow').css('display', 'none');

    if (this._zIndex) {
      $popupWindow.css('z-index', this._zIndex);
    }

    const $content = $popupWindow.find('DIV.t-window-content:first');
    $content.addClass('vertical-layout');
    $content.css('padding-bottom', '4px');

    let $breadCrumbsWrapper = null;
    if (this._breadCrumbsComponent) {
      const $breadCrumbs = $(this._breadCrumbsComponent.get_breadCrumbsElement());
      $breadCrumbsWrapper = $popupWindow.find(`#${breadCrumbsWrapperId}`);
      $breadCrumbsWrapper.append($breadCrumbs);
    }

    const $actionToolbar = $(this._actionToolbarComponent.get_toolbarElement());
    const $viewToolbar = $(this._viewToolbarComponent.get_toolbarElement());
    const $toolbarWrapper = $popupWindow.find(`#${toolbarWrapperId}`);

    const $actionToolbarWrapper = $popupWindow.find(`#${actionToolbarWrapperId}`);
    $actionToolbarWrapper.append($actionToolbar);

    const $viewToolbarWrapper = $popupWindow.find(`#${viewToolbarWrapperId}`);
    $viewToolbarWrapper.append($viewToolbar);

    const $searchBlockWrapper = $popupWindow.find(`#${searchBlockWrapperId}`);
    const $contextBlockWrapper = $popupWindow.find(`#${contextBlockWrapperId}`);
    const $documentArea = $popupWindow.find(`#${documentAreaId}`);

    const $loadingLayer = $('<div />', { class: 'loadingLayer', css: { display: 'none' } });
    $documentArea.prepend($loadingLayer);

    const $documentWrapper = $popupWindow.find(`#${documentWrapperId}`);
    if (!$q.isNullOrEmpty($breadCrumbsWrapper)) {
      this._breadCrumbsWrapperElement = $breadCrumbsWrapper.get(0);
    }

    this._toolbarWrapperElement = $toolbarWrapper.get(0);
    this._actionToolbarWrapperElement = $actionToolbarWrapper.get(0);
    this._viewToolbarWrapperElement = $viewToolbarWrapper.get(0);
    this._searchBlockWrapperElement = $searchBlockWrapper.get(0);
    this._contextBlockWrapperElement = $contextBlockWrapper.get(0);
    this._documentAreaElement = $documentArea.get(0);
    this._loadingLayerElement = $loadingLayer.get(0);
    this._documentWrapperElementId = documentWrapperId;
    this._documentWrapperElement = $documentWrapper.get(0);

    return popupWindowComponent;
  }

  openWindow(options) {
    if (this._isMultiOpen && this._isContentLoaded()) {
      this._popupWindowComponent.open();
    } else {
      this.onDocumentChanging();
      this._popupWindowComponent.open();
      this.generateDocumentUrl(options);
      this.renderPanels();
      this.loadHtmlContentToDocumentWrapper(this.onDocumentChanged.bind(this), options);
    }
  }

  closeWindow() {
    if (this._isMultiOpen) {
      $c.closePopupWindow(this._popupWindowComponent);
    } else if (this.get_closeWithoutCheck() || this.allowClose()) {
      this.markMainComponentAsBusy();
      this.cancel();
      this.onDocumentUnloaded();
      this.unbindExternalCallerContexts('closed');

      const eventArgs = new BackendEventArgs();
      this.notify(window.EVENT_TYPE_POPUP_WINDOW_CLOSED, eventArgs);
      this.dispose();
    }
  }

  setWindowTitle(titleText) {
    $c.setPopupWindowTitle(this._popupWindowComponent, titleText);
    this._title = titleText;
  }

  _attachPopupWindowEventHandlers() {
    const $popupWindow = $(this._popupWindowElement);
    $popupWindow
      .bind('open', this._onPopupWindowOpenHandler)
      .bind('resize', this._onPopupWindowResizeHandler)
      .bind('close', this._onPopupWindowCloseHandler)
      .bind('activated', this._onPopupWindowActivatedHandler);
  }

  _detachPopupWindowEventHandlers() {
    const $popupWindow = $(this._popupWindowElement);
    $popupWindow
      .unbind('open', this._onPopupWindowOpenHandler)
      .unbind('resize', this._onPopupWindowResizeHandler)
      .unbind('close', this._onPopupWindowCloseHandler)
      .unbind('activated', this._onPopupWindowActivatedHandler);
  }

  _resizeLibraryComponent() {
    const main = this.getMainComponent();
    if (main && (main instanceof BackendLibrary)) {
      main.resize();
    }
  }

  showLoadingLayer() {
    const $loadingLayer = $(this._loadingLayerElement);
    $loadingLayer.show();
  }

  hideLoadingLayer() {
    const $loadingLayer = $(this._loadingLayerElement);
    $loadingLayer.hide();
  }

  htmlLoadingMethod() {
    return this._isMultipleEntities || this._isCustomAction ? 'POST' : 'GET';
  }

  createPanels() {
    if (this._showBreadCrumbs) {
      const breadCrumbsComponent = BackendBreadCrumbsManager.getInstance().createBreadCrumbs(
        `breadCrumbs_${this._popupWindowId}`,
        { documentHost: this }
      );

      breadCrumbsComponent.attachObserver(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK, this._onGeneralEventHandler);
      breadCrumbsComponent.attachObserver(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK, this._onGeneralEventHandler);

      this._breadCrumbsComponent = breadCrumbsComponent;
    }

    if (!this._useCustomActionToolbar) {
      let actionToolbarOptions;
      const eventArgsAdditionalData = this.get_eventArgsAdditionalData();
      if (eventArgsAdditionalData && eventArgsAdditionalData.disabledActionCodes) {
        actionToolbarOptions = { disabledActionCodes: eventArgsAdditionalData.disabledActionCodes };
      }

      const actionToolbarComponent = new BackendActionToolbar(
        `actionToolbar_${this._popupWindowId}`,
        this._actionCode,
        this._parentEntityId,
        actionToolbarOptions
      );

      actionToolbarComponent.initialize();
      actionToolbarComponent.attachObserver(
        window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED,
        this._onGeneralEventHandler
      );

      this._actionToolbarComponent = actionToolbarComponent;
    }

    const viewToolbarOptions = {};
    const state = this.loadHostState();
    if (state && state.viewTypeCode) {
      viewToolbarOptions.viewTypeCode = state.viewTypeCode;
    }

    const viewToolbarComponent = new BackendViewToolbar(
      `viewToolbar_${this._popupWindowId}`,
      this._actionCode,
      viewToolbarOptions
    );

    viewToolbarComponent.initialize();
    viewToolbarComponent.attachObserver(
      window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED,
      this._onGeneralEventHandler
    );

    viewToolbarComponent.attachObserver(
      window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED,
      this._onGeneralEventHandler
    );

    viewToolbarComponent.attachObserver(
      window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED,
      this._onGeneralEventHandler
    );

    this._viewToolbarComponent = viewToolbarComponent;
  }

  hidePanels(callback) {
    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.hideBreadCrumbs();
    }

    this._actionToolbarComponent.hideToolbar(callback);
    this._viewToolbarComponent.hideToolbar();

    if (this.get_isSearchBlockVisible() && this._searchBlockComponent) {
      this._searchBlockComponent.hideSearchBlock();
    }

    if (this._isContextBlockVisible && this._contextBlockComponent) {
      this._contextBlockComponent.hideSearchBlock();
    }
  }

  showPanels(callback) {
    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.showBreadCrumbs();
    }

    this._actionToolbarComponent.showToolbar(callback);
    this._viewToolbarComponent.showToolbar();
    this.fixActionToolbarWidth();

    if (this.get_isSearchBlockVisible() && this._searchBlockComponent) {
      this._searchBlockComponent.showSearchBlock();
    }

    if (this._isContextBlockVisible && this._contextBlockComponent) {
      this._contextBlockComponent.showSearchBlock();
    }
  }

  destroyPanels() {
    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.detachObserver(
        window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK,
        this._onGeneralEventHandler
      );

      this._breadCrumbsComponent.detachObserver(
        window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK,
        this._onGeneralEventHandler
      );


      const breadCrumbsElementId = this._breadCrumbsComponent.get_breadCrumbsElementId();
      BackendBreadCrumbsManager.getInstance().destroyBreadCrumbs(breadCrumbsElementId);
      this._breadCrumbsComponent = null;
    }

    if (this._actionToolbarComponent) {
      if (!this._useCustomActionToolbar) {
        this._actionToolbarComponent.detachObserver(
          window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED,
          this._onGeneralEventHandler
        );
      }

      this._actionToolbarComponent.dispose();
      this._actionToolbarComponent = null;
    }

    if (this._viewToolbarComponent) {
      this._viewToolbarComponent.detachObserver(
        window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED,
        this._onGeneralEventHandler
      );

      this._viewToolbarComponent.detachObserver(
        window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED,
        this._onGeneralEventHandler
      );

      this._viewToolbarComponent.detachObserver(
        window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED,
        this._onGeneralEventHandler
      );

      this._viewToolbarComponent.dispose();
      this._viewToolbarComponent = null;
    }
  }

  createSearchBlock() {
    const searchBlockComponent = BackendSearchBlockManager.getInstance().createSearchBlock(
      `searchBlock_${this._popupWindowId}`,
      this._entityTypeCode,
      this._parentEntityId,
      this,
      {
        searchBlockContainerElementId: $(this._searchBlockWrapperElement).attr('id'),
        popupWindowId: this._popupWindowId,
        actionCode: this._actionCode,
        searchBlockState: this.getHostStateProp('searchBlockState')
      }
    );

    searchBlockComponent.attachObserver(
      window.EVENT_TYPE_SEARCH_BLOCK_FIND_START,
      this._onSearchHandler
    );

    searchBlockComponent.attachObserver(
      window.EVENT_TYPE_SEARCH_BLOCK_RESET_START,
      this._onSearchHandler
    );

    this._searchBlockComponent = searchBlockComponent;
  }

  createContextBlock() {
    const contextBlockComponent = BackendSearchBlockManager.getInstance().createSearchBlock(
      `contextBlock_${this._popupWindowId}`,
      this._entityTypeCode,
      this._parentEntityId,
      this,
      {
        searchBlockContainerElementId: $(this._searchBlockWrapperElement).attr('id'),
        popupWindowId: this._popupWindowId,
        actionCode: this._actionCode,
        contextSearch: true,
        hideButtons: true,
        searchBlockState: this.get_contextState()
      }
    );

    contextBlockComponent.initialize();
    contextBlockComponent.attachObserver(window.EVENT_TYPE_CONTEXT_BLOCK_FIND_START, this._onContextSwitchingHandler);

    this._contextBlockComponent = contextBlockComponent;
  }

  destroySearchBlock() {
    const searchBlockComponent = this._searchBlockComponent;

    if (searchBlockComponent) {
      searchBlockComponent.hideSearchBlock();
      searchBlockComponent.detachObserver(window.EVENT_TYPE_SEARCH_BLOCK_FIND_START, this._onSearchHandler);
      searchBlockComponent.detachObserver(window.EVENT_TYPE_SEARCH_BLOCK_RESET_START, this._onSearchHandler);

      const searchBlockElementId = searchBlockComponent.get_searchBlockElementId();
      BackendSearchBlockManager.getInstance().destroySearchBlock(searchBlockElementId);

      this._searchBlockComponent = null;
    }
  }

  destroyContextBlock() {
    const searchBlockComponent = this._searchBlockComponent;
    const contextBlockComponent = this._contextBlockComponent;

    if (contextBlockComponent) {
      contextBlockComponent.hideSearchBlock();
      contextBlockComponent.detachObserver(window.EVENT_TYPE_CONTEXT_BLOCK_FIND_START, this._onContextSwitchingHandler);

      const searchBlockElementId = searchBlockComponent.get_searchBlockElementId();
      BackendSearchBlockManager.getInstance().destroySearchBlock(searchBlockElementId);

      this._contextBlockComponent = null;
      this._isContextBlockVisible = false;
    }
  }

  showErrorMessageInDocumentWrapper() {
    const $documentWrapper = $(this._documentWrapperElement);
    $documentWrapper.html($q.generateErrorMessageText());
  }

  updateTitle(eventArgs) {
    this.setWindowTitle(BackendDocumentHost.generateTitle(eventArgs, { isTab: false }));
  }

  onChangeContent(eventType, sender, eventArgs) {
    this.changeContent(eventArgs);
  }

  saveSelectionContext(eventArgs) {
    this._selectionContext = eventArgs.get_context();
  }

  onActionExecuting(eventArgs) {
    this._copyCurrentContextToEventArgs(eventArgs);
    return this._popupWindowManagerComponent.notify(window.EVENT_TYPE_POPUP_WINDOW_ACTION_EXECUTING, eventArgs);
  }

  onEntityReaded(eventArgs) {
    return this._popupWindowManagerComponent.notify(window.EVENT_TYPE_POPUP_WINDOW_ENTITY_READED, eventArgs);
  }

  onDocumentChanging() {
    this.markPanelsAsBusy();
  }

  onDocumentChanged() {
    this.unmarkPanelsAsBusy();
  }

  onNeedUp(eventArgs) {
    this._popupWindowManagerComponent.onNeedUp(eventArgs, this.get_popupWindowId());
  }

  resetSelectedEntities() {
    this._initSelectedEntities();
  }

  _onLibraryResized(eventType, sender) {
    const elHeight = $(this._documentAreaElement).height();
    $(sender._libraryElement).height(elHeight + 8);
  }

  _onPopupWindowResize() {
    this._resizeLibraryComponent();
  }

  _onPopupWindowOpen() {
    this._resizeLibraryComponent();
  }

  _onPopupWindowClose() {
    const $active = $(document.activeElement);
    if ($active) {
      $active.blur();
    }

    this.closeWindow();
  }

  _onPopupWindowActivated() {
    this._resizeLibraryComponent();
  }

  _onExternalCallerContextsUnbinded(unbindingEventArgs) {
    this.get_popupWindowManager().hostExternalCallerContextsUnbinded(unbindingEventArgs);
  }

  _isContentLoaded() {
    const $wrapper = $(this._documentWrapperElement);
    return $wrapper && $wrapper.html();
  }

  onDocumentError() {
    this.closeWindow();
  }

  dispose() {
    super.dispose();

    this._detachPopupWindowEventHandlers();
    if (this._loadingLayerElement) {
      const $loadingLayer = $(this._loadingLayerElement);
      $loadingLayer.empty();
      $loadingLayer.remove();
    }

    this.destroyPanels();
    this.destroySearchBlock();
    this.destroyContextBlock();

    if (this._popupWindowManagerComponent) {
      const popupWindowId = this._popupWindowId;
      if (!$q.isNullOrWhiteSpace(popupWindowId)) {
        this._popupWindowManagerComponent.removePopupWindow(popupWindowId);
      }
    }

    if (this._popupWindowComponent) {
      $c.destroyPopupWindow(this._popupWindowComponent);
    }

    $q.dispose.call(this, [
      '_popupWindowElement',
      '_onPopupWindowResizeHandler',
      '_onPopupWindowOpenHandler',
      '_onPopupWindowCloseHandler',
      '_onPopupWindowActivatedHandler',
      '_breadCrumbsWrapperElement',
      '_toolbarWrapperElement',
      '_actionToolbarWrapperElement',
      '_viewToolbarWrapperElement',
      '_searchBlockWrapperElement',
      '_contextBlockWrapperElement',
      '_documentAreaElement',
      '_documentWrapperElement',
      '_loadingLayerElement',
      '_popupWindowManagerComponent',
      '_popupWindowComponent'
    ]);

    $q.collectGarbageInIE();
  }
}

Quantumart.QP8.BackendPopupWindow = BackendPopupWindow;
