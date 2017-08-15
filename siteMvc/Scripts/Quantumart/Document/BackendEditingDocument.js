Quantumart.QP8.BackendEditingDocument = function (tabId, editingArea, eventArgs, options) {
  if (!tabId) {
    throw new Error($l.EditingArea.tabIdNotSpecifiedInConstructor);
  }

  if (!editingArea) {
    throw new Error($l.EditingArea.editingAreaNotSpecifiedInConstructor);
  }

  Quantumart.QP8.BackendEditingDocument.initializeBase(this, [eventArgs, options]);

  this._editingArea = editingArea;
  this._tabId = tabId;
  this._id = this.getDocumentIdByTabId(this._tabId);
  if ($q.isObject(eventArgs)) {
    this._applyEventArgs(eventArgs, true);
    this.bindExternalCallerContext(eventArgs);
    this._selectedEntities = [];
    if (eventArgs.get_context() && eventArgs.get_context().additionalUrlParameters) {
      this._additionalUrlParameters = eventArgs.get_context().additionalUrlParameters;
    }
  }
};

Quantumart.QP8.BackendEditingDocument.prototype = {
  _id: '',
  _tabId: '',
  _editingArea: null,
  _additionalUrlParameters: null,

  get_id: function () {
    return this._id;
  },

  get_tabId: function () {
    return this._tabId;
  },

  set_tabId: function (value) {
    this._tabId = value;
  },

  get_editingArea: function () {
    return this._editingArea;
  },

  get_hostType: function () {
    return window.DOCUMENT_HOST_TYPE_EDITING_DOCUMENT;
  },

  get_zIndex: function () {
    return 0;
  },

  initialize: function (callback) {
    this.generateDocumentUrl();
    this.createPanels();
    this.renderPanels();
    this.addNewDocumentWrapper();
    this._loadDefaultSearchBlockState();
    this.loadHtmlContentToDocumentWrapper(callback);
  },

  getDocumentIdByTabId: function (tabId) {
    return this._editingArea.getDocumentIdByTabId(tabId);
  },

  generateDocumentWrapperId: function () {
    return String.format('doc_{0}', this._tabId);
  },

  addNewDocumentWrapper: function () {
    let documentWrapperElementId = this._documentWrapperElementId;
    if ($q.isNullOrWhiteSpace(documentWrapperElementId)) {
      documentWrapperElementId = this.generateDocumentWrapperId();
    }

    let $documentWrapper = $(`#${documentWrapperElementId}`);
    if ($q.isNullOrEmpty($documentWrapper)) {
      $documentWrapper = $('<div />', { id: documentWrapperElementId, class: 'documentWrapper' });

      const $documentsContainer = $(this._editingArea.get_documentsContainerElement());
      $documentsContainer.append($documentWrapper);
    }

    this._documentWrapperElementId = documentWrapperElementId;
    this._documentWrapperElement = $documentWrapper.get(0);

    return documentWrapperElementId;
  },

  generateDocumentUrl: function (options) {
    const entityIDs = this._isMultipleEntities ? $o.getEntityIDsFromEntities(this._entities) : [this._entityId];
    const extraOptions = {
      additionalUrlParameters: this._additionalUrlParameters,
      controllerActionUrl: this.getCurrentViewActionUrl()
    };

    if (this.get_isBindToExternal() === true) {
      extraOptions.additionalUrlParameters = Object.assign({}, extraOptions.additionalUrlParameters, { boundToExternal: true });
    }

    options = !$q.isObject(options) ? extraOptions : Object.assign({}, options, extraOptions);
    const url = $a.generateActionUrl(this._isMultipleEntities, entityIDs, this._parentEntityId, this._tabId, this.getCurrentAction(), options);
    this._documentUrl = url;

    const params = {};
    if (this._isMultipleEntities || this._isCustomAction) {
      params.IDs = entityIDs;
    }

    if (this._isCustomAction) {
      params.actionCode = this._actionCode;
    }

    this._documentPostParams = params;
  },

  htmlLoadingMethod: function () {
    return this._isMultipleEntities || this._isCustomAction ? 'POST' : 'GET';
  },

  showErrorMessageInDocumentWrapper: function (status) {
    const $documentWrapper = $(this._documentWrapperElement);
    $documentWrapper.html($q.generateErrorMessageText());
  },

  removeDocumentWrapper: function (callback) {
    const $documentWrapper = $(this._documentWrapperElement);
    $documentWrapper.empty().remove();
    $q.callFunction(callback);
  },

  createPanels: function () {
    const action = this.getCurrentAction();
    const breadCrumbsComponent = Quantumart.QP8.BackendBreadCrumbsManager.getInstance().createBreadCrumbs(`breadCrumbs_${this._tabId}`, {
      documentHost: this,
      breadCrumbsContainerElementId: this._editingArea.get_breadCrumbsContainerElementId(),
      contextMenuManager: new Quantumart.QP8.BackendBreadMenuContextMenuManager()
    });

    breadCrumbsComponent.attachObserver(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK, this._onGeneralEventHandler);
    breadCrumbsComponent.attachObserver(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK, this._onGeneralEventHandler);
    breadCrumbsComponent.attachObserver(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CONTEXT_CLICK, this._onGeneralEventHandler);
    this._breadCrumbsComponent = breadCrumbsComponent;

    const actionToolbarOptions = {
      toolbarContainerElementId: this._editingArea.get_actionToolbarContainerElementId()
    };

    const eventArgsAdditionalData = this.get_eventArgsAdditionalData();
    if (eventArgsAdditionalData && eventArgsAdditionalData.disabledActionCodes) {
      actionToolbarOptions.disabledActionCodes = eventArgsAdditionalData.disabledActionCodes;
    }

    const actionToolbarComponent = new Quantumart.QP8.BackendActionToolbar(`actionToolbar_${this._tabId}`, this._actionCode, this._parentEntityId, actionToolbarOptions);
    actionToolbarComponent.initialize();
    actionToolbarComponent.attachObserver(window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED, this._onGeneralEventHandler);
    this._actionToolbarComponent = actionToolbarComponent;
    const viewToolbarOptions = {
      toolbarContainerElementId: this._editingArea.get_viewToolbarContainerElementId()
    };

    const state = this.loadHostState();
    if (state && state.viewTypeCode) {
      viewToolbarOptions.viewTypeCode = state.viewTypeCode;
    }

    const viewToolbarComponent = new Quantumart.QP8.BackendViewToolbar(`viewToolbar_${this._tabId}`, this._actionCode, viewToolbarOptions);
    viewToolbarComponent.initialize();

    viewToolbarComponent.attachObserver(window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED, this._onGeneralEventHandler);
    viewToolbarComponent.attachObserver(window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED, this._onGeneralEventHandler);
    viewToolbarComponent.attachObserver(window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED, this._onGeneralEventHandler);

    this._viewToolbarComponent = viewToolbarComponent;
  },

  showPanels: function (callback) {
    const $breadCrumbsContainer = $(this._editingArea.get_breadCrumbsContainerElement());
    $breadCrumbsContainer.find('> *.breadCrumbs:visible').hide(0);
    this._breadCrumbsComponent.showBreadCrumbs();

    const $actionToolbarContainer = $(this._editingArea.get_actionToolbarContainerElement());
    $actionToolbarContainer.find('> *.toolbar:visible').hide(0);
    this._actionToolbarComponent.showToolbar(callback);

    const $viewToolbarContainer = $(this._editingArea.get_viewToolbarContainerElement());
    $viewToolbarContainer.find('> *.toolbar:visible').hide(0);
    this._viewToolbarComponent.showToolbar();
    this.fixActionToolbarWidth();

    if (this.get_isSearchBlockVisible() && this._searchBlockComponent) {
      const $searchBlockContainer = $(this._editingArea.get_searchBlockContainerElement());
      $searchBlockContainer.find('> *.searchBlock:visible').hide(0);
      this._searchBlockComponent.showSearchBlock();
    }

    if (this._isContextBlockVisible && this._contextBlockComponent) {
      const $contextBlockContainer = $(this._editingArea.get_contextBlockContainerElement());
      $contextBlockContainer.find('> *.contextBlock:visible').hide(0);
      this._contextBlockComponent.showSearchBlock();
    }
  },

  hidePanels: function () {
    this._breadCrumbsComponent.hideBreadCrumbs();
    this._actionToolbarComponent.hideToolbar();
    this._viewToolbarComponent.hideToolbar();
    if (this.get_isSearchBlockVisible() && this._searchBlockComponent) {
      this._searchBlockComponent.hideSearchBlock();
    }

    if (this._isContextBlockVisible && this._contextBlockComponent) {
      this._contextBlockComponent.hideSearchBlock();
    }
  },

  createSearchBlock: function () {
    const searchBlockComponent = Quantumart.QP8.BackendSearchBlockManager.getInstance().createSearchBlock(`searchBlock_${this._tabId}`, this._entityTypeCode, this._parentEntityId, this, {
      searchBlockContainerElementId: this._editingArea.get_searchBlockContainerElementId(),
      tabId: this._tabId,
      actionCode: this._actionCode,
      searchBlockState: this.getHostStateProp('searchBlockState')
    });

    searchBlockComponent.attachObserver(window.EVENT_TYPE_SEARCH_BLOCK_FIND_START, this._onSearchHandler);
    searchBlockComponent.attachObserver(window.EVENT_TYPE_SEARCH_BLOCK_RESET_START, this._onSearchHandler);
    searchBlockComponent.attachObserver(window.EVENT_TYPE_SEARCH_BLOCK_RESIZED, this._onSearchBlockResizeHandler);
    this._searchBlockComponent = searchBlockComponent;
  },

  createContextBlock: function () {
    const contextBlockComponent = Quantumart.QP8.BackendSearchBlockManager.getInstance().createSearchBlock(`contextBlock_${this._tabId}`, this._entityTypeCode, this._parentEntityId, this, {
      searchBlockContainerElementId: this._editingArea.get_contextBlockContainerElementId(),
      tabId: this._tabId,
      actionCode: this._actionCode,
      contextSearch: true,
      hideButtons: true,
      searchBlockState: this.get_contextState()
    });

    contextBlockComponent.initialize();
    contextBlockComponent.attachObserver(window.EVENT_TYPE_CONTEXT_BLOCK_FIND_START, this._onContextSwitchingHandler);
    this._contextBlockComponent = contextBlockComponent;
  },

  destroySearchBlock: function () {
    const searchBlockComponent = this._searchBlockComponent;
    if (searchBlockComponent) {
      searchBlockComponent.hideSearchBlock();
      searchBlockComponent.detachObserver(window.EVENT_TYPE_SEARCH_BLOCK_FIND_START, this._onSearchHandler);
      searchBlockComponent.detachObserver(window.EVENT_TYPE_SEARCH_BLOCK_RESET_START, this._onSearchHandler);
      searchBlockComponent.detachObserver(window.EVENT_TYPE_SEARCH_BLOCK_RESIZED, this._onSearchBlockResizeHandler);

      const searchBlockElementId = searchBlockComponent.get_searchBlockElementId();
      Quantumart.QP8.BackendSearchBlockManager.getInstance().destroySearchBlock(searchBlockElementId);
      this._searchBlockComponent = null;
    }
  },

  destroyContextBlock: function () {
    const contextBlockComponent = this._contextBlockComponent;
    if (contextBlockComponent) {
      contextBlockComponent.hideSearchBlock();
      contextBlockComponent.detachObserver(window.EVENT_TYPE_CONTEXT_BLOCK_FIND_START, this._onContextSwitchingHandler);

      const contextBlockElementId = contextBlockComponent.get_searchBlockElementId();
      Quantumart.QP8.BackendSearchBlockManager.getInstance().destroySearchBlock(contextBlockElementId);
      this._contextBlockComponent = null;
      this._isContextBlockVisible = false;
    }
  },

  showLoadingLayer: function () {
    if (this._editingArea) {
      if (this.isSelected()) {
        this._editingArea.showAjaxLoadingLayer();
      }
    }
  },

  hideLoadingLayer: function () {
    if (this._editingArea) {
      if (this.isSelected()) {
        this._editingArea.hideAjaxLoadingLayer();
      }
    }
  },

  destroyPanels: function () {
    if (this._breadCrumbsComponent) {
      this._breadCrumbsComponent.detachObserver(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CLICK, this._onGeneralEventHandler);
      this._breadCrumbsComponent.detachObserver(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CTRL_CLICK, this._onGeneralEventHandler);
      this._breadCrumbsComponent.detachObserver(window.EVENT_TYPE_BREAD_CRUMBS_ITEM_CONTEXT_CLICK, this._onGeneralEventHandler);

      const breadCrumbsElementId = this._breadCrumbsComponent.get_breadCrumbsElementId();
      Quantumart.QP8.BackendBreadCrumbsManager.getInstance().destroyBreadCrumbs(breadCrumbsElementId);
      this._breadCrumbsComponent = null;
    }

    if (this._actionToolbarComponent) {
      this._actionToolbarComponent.detachObserver(window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED, this._onGeneralEventHandler);
      this._actionToolbarComponent.dispose();
      this._actionToolbarComponent = null;
    }

    if (this._viewToolbarComponent) {
      this._viewToolbarComponent.detachObserver(window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED, this._onGeneralEventHandler);
      this._viewToolbarComponent.detachObserver(window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED, this._onGeneralEventHandler);
      this._viewToolbarComponent.detachObserver(window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED, this._onGeneralEventHandler);
      this._viewToolbarComponent.dispose();
      this._viewToolbarComponent = null;
    }

    let editingArea = this._editingArea;
    if (editingArea) {
      if (!$q.isNullOrWhiteSpace(this._id)) {
        this._editingArea.removeDocument(this._id);
      }

      editingArea = null;
      this._editingArea = null;
    }
  },

  isSelected: function () {
    return this._id == this._editingArea.getSelectedDocumentId();
  },

  updateTitle: function (eventArgs) {
    this._editingArea.updateTab(this.get_tabId(), eventArgs);
  },

  onDocumentError: function () {
    this._isCloseForced = false;
    this._editingArea.onDocumentError(this._id);
  },

  onChangeContent: function (eventType, sender, eventArgs) {
    const tabId = this._editingArea.getExistingTabId(eventArgs);
    if (tabId != 0) {
      const selectedDocument = this._editingArea.selectDocument(tabId);
      if (selectedDocument) {
        selectedDocument.onSelectedThroughExecution(eventArgs);
      }
    } else {
      this.changeContent(eventArgs);
    }
  },

  saveSelectionContext: function (eventArgs) {
    this._selectedParentEntityId = eventArgs.get_parentEntityId();
  },

  onActionExecuting: function (eventArgs) {
    this._copyCurrentContextToEventArgs(eventArgs);
    return this._editingArea.onActionExecuting(eventArgs);
  },

  onEntityReaded: function (eventArgs) {
    return this._editingArea.onEntityReaded(eventArgs);
  },

  onDocumentChanging: function (isLocal) {
    this._editingArea.onDocumentLoading(this._id, isLocal);
  },

  onDocumentChanged: function (isLocal) {
    this._editingArea.onDocumentLoaded(this._id, isLocal);
  },

  onNeedUp: function (eventArgs) {
    let tabId = 0;
    if (this._callerContext && this._callerContext.eventArgs) {
      tabId = this._editingArea.getExistingTabId(this._callerContext.eventArgs);
      if (tabId != 0) {
        this._editingArea.selectDocument(tabId);
        this._editingArea.closeDocument(this.get_tabId(), false, true);
        return;
      }
    }

    if (tabId == 0) {
      let bcEventArgs;
      const bcItem = this._breadCrumbsComponent.getLastItemButOne();
      if (bcItem) {
        bcEventArgs = this._breadCrumbsComponent.getItemActionEventArgs(bcItem);
        if (bcEventArgs) {
          tabId = this._editingArea.getExistingTabId(bcEventArgs);
        }
      }
      if (tabId != 0) {
        this._editingArea.selectDocument(tabId);
        this._editingArea.closeDocument(this.get_tabId(), false, true);
      } else {
        this.changeContent(bcEventArgs, true);
      }
    }
  },

  saveAndCloseRequest: function (eventArgs) {
    Sys.Debug.trace(`saveAndCloseRequest: ${eventArgs._tabId}`);
    const context = this.get_documentContext();
    if (context && context._options.saveAndCloseActionCode) {
      const main = this.get_mainComponent();
      if (main != null && Quantumart.QP8.BackendEntityEditor.isInstanceOfType(main) && main.isFieldsChanged()) {
        this._isCloseForced = true;
        this.executeAction(context._options.saveAndCloseActionCode);
      }
    }
  },

  onSaveAndClose: function () {
    if (this._isCloseForced) {
      this._isCloseForced = false;
      const context = this.get_documentContext();
      if (context && context.get_mainComponentType() == $e.MainComponentType.Editor && !context.get_mainComponent()._formHasErrors) {
        this._editingArea.closeDocument(this.get_tabId(), true);
      }
    }
  },

  resetSelectedEntities: function () {
    $q.clearArray(this._selectedEntities);
  },

  _onLibraryResized: function () {
    let $docContainer = $(this._editingArea.get_documentsContainerElement());
    let $docWrp = $(this._documentWrapperElement);
    $docWrp.height($docContainer.height());
    $docContainer = null;
    $docWrp = null;
  },

  _onExternalCallerContextsUnbinded: function (unbindingEventArgs) {
    this._editingArea.hostExternalCallerContextsUnbinded(unbindingEventArgs);
  },

  _isWindow: function () {
    return false;
  },

  dispose: function () {
    Quantumart.QP8.BackendEditingDocument.callBaseMethod(this, 'dispose');

    this.destroyPanels();
    this.destroySearchBlock();
    this.destroyContextBlock();

    this.removeDocumentWrapper();

    $q.clearArray(this._entities);
    $q.clearArray(this._selectedEntities);

    this._documentContext = null;
    this._documentWrapperElement = null;

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEditingDocument.registerClass('Quantumart.QP8.BackendEditingDocument', Quantumart.QP8.BackendDocumentHost);
