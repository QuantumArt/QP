/* eslint max-lines: 'off' */
import { BackendDocumentHostStateStorage } from './BackendDocumentHostStateStorage';
import { BackendEditingDocument } from './BackendEditingDocument';
import { BackendEventArgs } from '../Common/BackendEventArgs';
import { Observable } from '../Common/Observable';
import { $q } from '../Utils';


window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING = 'OnEditingAreaDocumentLoading';
window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED = 'OnEditingAreaDocumentLoaded';
window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING = 'OnEditingAreaDocumentClosing';
window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED = 'OnEditingAreaDocumentClosed';
window.EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR = 'OnEditingAreaDocumentError';
window.EVENT_TYPE_EDITING_AREA_CLOSED = 'OnEditingAreaClosed';
window.EVENT_TYPE_EDITING_AREA_ACTION_EXECUTING = 'OnEditingAreaActionExecuting';
window.EVENT_TYPE_EDITING_AREA_ENTITY_READED = 'OnEditingAreaEntityReaded';
window.EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE = 'OnEditingAreaFindTabInTree';

export class BackendEditingArea extends Observable {
  constructor(editingAreaElementId, options) {
    super();

    this._hostStateStorage = new BackendDocumentHostStateStorage({
      currentCustomerCode: options.currentCustomerCode,
      currentUserId: options.currentUserId
    });

    this._editingAreaElementId = editingAreaElementId;
    /** @type {{ [x: string]: BackendEditingDocument }} */
    this._documents = {};
    this._customVars = {};
    this._customScripts = {};
    if ($q.isObject(options)) {
      if (options.documentsContainerElementId) {
        this._documentsContainerElementId = options.documentsContainerElementId;
      }

      if (options.breadCrumbsContainerElementId) {
        this._breadCrumbsContainerElementId = options.breadCrumbsContainerElementId;
      }

      if (options.actionToolbarContainerElementId) {
        this._actionToolbarContainerElementId = options.actionToolbarContainerElementId;
      }

      if (options.viewToolbarContainerElementId) {
        this._viewToolbarContainerElementId = options.viewToolbarContainerElementId;
      }

      if (options.searchBlockContainerElementId) {
        this._searchBlockContainerElementId = options.searchBlockContainerElementId;
      }

      if (options.contextBlockContainerElementId) {
        this._contextBlockContainerElementId = options.contextBlockContainerElementId;
      }

      if (options.documentsContainerHeightDifference) {
        this._documentsContainerHeightDifference = options.documentsContainerHeightDifference;
      }

      if (options.tabStrip) {
        this._tabStrip = options.tabStrip;
        this._onTabStripCloseRequestHandler = $.proxy(this.onTabStripCloseRequest, this);
        this._onTabStripSelectRequestHandler = $.proxy(this.onTabStripSelectRequest, this);

        this._tabStrip.attachObserver(
          window.EVENT_TYPE_TAB_STRIP_TAB_CLOSE_REQUEST,
          this._onTabStripCloseRequestHandler);

        this._tabStrip.attachObserver(
          window.EVENT_TYPE_TAB_STRIP_TAB_SELECT_REQUEST,
          this._onTabStripSelectRequestHandler);

        this._tabStrip.attachObserver(
          window.EVENT_TYPE_TAB_STRIP_TAB_SAVE_CLOSE_REQUEST,
          $.proxy(this.onTabStripSaveAndCloseRequest, this));

        this._tabStrip.attachObserver(
          window.EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST,
          $.proxy(this.onFindTabInTreeRequest, this));
      }
    }

    this._onWindowResizedHandler = $.proxy(this.onWindowResized, this);
  }

  _editingAreaElementId = '';
  _editingAreaElement = null;
  _documentsContainerElementId = '';
  _documentsContainerElement = null;
  _loadingLayerElement = null;
  _documentsContainerHeightDifference = 0;
  _selectedDocumentId = '';
  _breadCrumbsContainerElementId = '';
  _breadCrumbsContainerElement = null;
  _actionToolbarContainerElementId = '';
  _actionToolbarContainerElement = null;
  _viewToolbarContainerElementId = '';
  _viewToolbarContainerElement = null;
  _searchBlockContainerElementId = '';
  _searchBlockContainerElement = null;
  _contextBlockContainerElementId = '';
  _contextBlockContainerElement = null;
  _tabStrip = null;
  _hostStateStorage = null;
  _customVars = null;
  _customScripts = null;
  _onWindowResizedHandler = null;

  // eslint-disable-next-line camelcase
  get_editingAreaElementId() {
    return this._editingAreaElementId;
  }

  // eslint-disable-next-line camelcase
  set_editingAreaElementId(value) {
    this._editingAreaElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_documentsContainerElementId() {
    return this._documentsContainerElementId;
  }

  // eslint-disable-next-line camelcase
  set_documentsContainerElementId(value) {
    this._documentsContainerElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_documentsContainerElement() {
    return this._documentsContainerElement;
  }

  // eslint-disable-next-line camelcase
  get_documentsContainerHeightDifference() {
    return this._documentsContainerHeightDifference;
  }

  // eslint-disable-next-line camelcase
  set_documentsContainerHeightDifference(value) {
    this._documentsContainerHeightDifference = value;
  }

  // eslint-disable-next-line camelcase
  get_breadCrumbsContainerElementId() {
    return this._breadCrumbsContainerElementId;
  }

  // eslint-disable-next-line camelcase
  set_breadCrumbsContainerElementId(value) {
    this._breadCrumbsContainerElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_breadCrumbsContainerElement() {
    return this._breadCrumbsContainerElement;
  }

  // eslint-disable-next-line camelcase
  get_actionToolbarContainerElementId() {
    return this._actionToolbarContainerElementId;
  }

  // eslint-disable-next-line camelcase
  set_actionToolbarContainerElementId(value) {
    this._actionToolbarContainerElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_actionToolbarContainerElement() {
    return this._actionToolbarContainerElement;
  }

  // eslint-disable-next-line camelcase
  get_viewToolbarContainerElementId() {
    return this._viewToolbarContainerElementId;
  }

  // eslint-disable-next-line camelcase
  set_viewToolbarContainerElementId(value) {
    this._viewToolbarContainerElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_viewToolbarContainerElement() {
    return this._viewToolbarContainerElement;
  }

  // eslint-disable-next-line camelcase
  get_searchBlockContainerElementId() {
    return this._searchBlockContainerElementId;
  }

  // eslint-disable-next-line camelcase
  set_searchBlockContainerElementId(value) {
    this._searchBlockContainerElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_searchBlockContainerElement() {
    return this._searchBlockContainerElement;
  }

  // eslint-disable-next-line camelcase
  get_tabStrip() {
    return this._tabStrip;
  }

  // eslint-disable-next-line camelcase
  set_tabStrip(value) {
    this._tabStrip = value;
  }

  // eslint-disable-next-line camelcase
  get_contextBlockContainerElementId() {
    return this._contextBlockContainerElementId;
  }

  // eslint-disable-next-line camelcase
  set_contextBlockContainerElementId(value) {
    this._contextBlockContainerElementId = value;
  }

  // eslint-disable-next-line camelcase
  get_contextBlockContainerElement() {
    return this._contextBlockContainerElement;
  }

  // eslint-disable-next-line camelcase
  set_contextBlockContainerElement(value) {
    this._contextBlockContainerElement = value;
  }

  // eslint-disable-next-line camelcase
  get_documents() {
    return this._documents;
  }

  initialize() {
    if (!this._documentsContainerElementId) {
      throw new Error($l.EditingArea.documentsContainerElementIdNotSpecified);
    }

    if (!this._breadCrumbsContainerElementId) {
      throw new Error($l.EditingArea.breadCrumbsContainerElementIdNotSpecified);
    }

    if (!this._actionToolbarContainerElementId) {
      throw new Error($l.EditingArea.actionToolbarContainerElementIdNotSpecified);
    }

    if (!this._viewToolbarContainerElementId) {
      throw new Error($l.EditingArea.viewToolbarContainerElementIdNotSpecified);
    }

    if (!this._searchBlockContainerElementId) {
      throw new Error($l.EditingArea.searchBlockContainerElementIdNotSpecified);
    }

    if (!this._contextBlockContainerElementId) {
      throw new Error($l.EditingArea.contextBlockContainerElementIdNotSpecified);
    }

    const $editingArea = $(`#${this._editingAreaElementId}`);
    const $documentsContainer = $(`#${this._documentsContainerElementId}`);
    const $loadingLayer = $('<div />', { class: 'loadingLayer', css: { display: 'none' } });
    const $breadCrumbsContainer = $(this._breadCrumbsContainerElementId);
    const $actionToolbarContainer = $(`#${this._actionToolbarContainerElementId}`);
    const $viewToolbarContainer = $(`#${this._viewToolbarContainerElementId}`);
    const $searchBlockContainer = $(`#${this._searchBlockContainerElementId}`);
    const $contextBlockContainer = $(`#${this._contextBlockContainerElementId}`);

    $documentsContainer.prepend($loadingLayer);

    this._editingAreaElement = $editingArea.get(0);
    this._documentsContainerElement = $documentsContainer.get(0);
    this._breadCrumbsContainerElement = $breadCrumbsContainer.get(0);
    this._actionToolbarContainerElement = $actionToolbarContainer.get(0);
    this._viewToolbarContainerElement = $viewToolbarContainer.get(0);
    this._searchBlockContainerElement = $searchBlockContainer.get(0);
    this._contextBlockContainerElement = $contextBlockContainer.get(0);
    this._loadingLayerElement = $loadingLayer.get(0);

    $(window).resize(this._onWindowResizedHandler);
  }

  fixDocumentsContainerHeight(options) {
    if (options && options.bySplitter) {
      return;
    }

    let $documentsContainer = $(this._documentsContainerElement);
    const oldDocumentsContainerHeight = parseInt(String($documentsContainer.height()), 10);
    // eslint-disable-next-line max-len
    let newDocumentsContainerHeight = parseInt(String($(window).height()), 10) - this._documentsContainerHeightDifference;

    if (oldDocumentsContainerHeight !== newDocumentsContainerHeight) {
      if (newDocumentsContainerHeight < 300) {
        newDocumentsContainerHeight = 300;
      }

      $documentsContainer.css('height', `${newDocumentsContainerHeight}px`);
    }

    $documentsContainer = null;
  }

  openArea() {
    let $editingArea = $(this._editingAreaElement);
    $editingArea.css('display', 'block');

    $editingArea = null;
  }

  closeArea() {
    let $editingArea = $(this._editingAreaElement);
    $editingArea.css('display', 'none');

    $editingArea = null;

    let $documentsContainer = $(this._documentsContainerElement);
    $documentsContainer
      .find('DIV.documentWrapper')
      .empty()
      .remove();

    $documentsContainer = null;
    this._selectedDocumentId = '';
    // eslint-disable-next-line no-use-before-define
    this.notify(window.EVENT_TYPE_EDITING_AREA_CLOSED, new BackendEditingAreaEventArgs());
  }

  getDocumentIdByTabId(tabId) {
    const editingDocumentId = String.format('{0}_document', tabId);

    return editingDocumentId;
  }

  getExistingTabId(eventArgs) {
    return this._tabStrip.getExistingTabId(eventArgs);
  }

  getSelectedDocument() {
    return this.getDocument(this._selectedDocumentId);
  }

  getSelectedDocumentId() {
    return this._selectedDocumentId;
  }

  getDocument(docId) {
    return $q.isNullOrWhiteSpace(docId) ? null : this._documents[docId];
  }

  getDocumentByEventArgs(eventArgs) {
    const tabId = this.getExistingTabId(eventArgs);
    const docId = this.getDocumentIdByTabId(tabId);
    return this.getDocument(docId);
  }

  // eslint-disable-next-line max-statements
  addDocument(eventArgs) {
    if (eventArgs.get_isWindow()) {
      return;
    }

    if (eventArgs.fromHistory) {
      const allTabs = this._tabStrip.getAllTabs().get();

      const sameDocumentTab = allTabs.find(tab => {
        const tabEventArgs = this._tabStrip.getEventArgsFromTab(tab);
        return eventArgs.hasSameDocument(tabEventArgs);
      });

      if (sameDocumentTab) {
        const sameDocumentTabId = this._tabStrip.getTabId(sameDocumentTab);
        const selectedDocument = this.selectDocument(sameDocumentTabId);
        if (selectedDocument) {
          eventArgs.finishExecution();
        }
        return;
      }

      const eventTabId = eventArgs.get_tabId();
      const existingTab = this._tabStrip.getTab(eventTabId);
      if (existingTab) {
        const selectedDocument = this.selectDocument(eventTabId);
        if (selectedDocument) {
          const tabEventArgs = this._tabStrip.getEventArgsFromTab(existingTab);
          if (eventArgs.structurallyEquals(tabEventArgs)) {
            eventArgs.finishExecution();
          } else {
            selectedDocument.onSelectedThroughExecution(eventArgs);
            selectedDocument.changeContent(eventArgs);
          }
        }
        return;
      }
    }

    const tabId = this.getExistingTabId(eventArgs);
    if (tabId === 0) {
      this._addNewDocument(eventArgs);
    } else if ($.isEmptyObject(eventArgs.get_context()) || !eventArgs.get_context().ctrlKey) {
      const selectedDocument = this.selectDocument(tabId);
      if (selectedDocument) {
        selectedDocument.onSelectedThroughExecution(eventArgs);
      }
    }
  }

  _addNewDocument(eventArgs) {
    this.onDocumentLoading();
    if (this._tabStrip.getAllTabsCount() === 0) {
      this.openArea();
    }

    const tabId = this._tabStrip.addNewTab(eventArgs);
    const oldDoc = this.getSelectedDocument();
    if (oldDoc) {
      oldDoc.hidePanels();
      oldDoc.hideDocumentWrapper();
    }

    const doc = new BackendEditingDocument(tabId, this, eventArgs, {
      hostStateStorage: this._hostStateStorage
    });

    const docId = doc.get_id();
    this._documents[docId] = doc;
    this._selectedDocumentId = docId;

    doc.initialize(() => {
      this.onDocumentLoaded(this._selectedDocumentId);
      if (eventArgs.fromHistory) {
        eventArgs.finishExecution();
      }
    });

    if (eventArgs.get_context() && eventArgs.get_context().ctrlKey) {
      this.selectDocument(this._tabStrip._previousSelectedTabId);
    }
  }

  selectDocument(tabId) {
    const docId = this.getDocumentIdByTabId(tabId);
    const doc = this.getDocument(docId);
    const oldDoc = this.getSelectedDocument();
    if (doc) {
      this.onDocumentLoading(docId);

      if (oldDoc) {
        oldDoc.hidePanels();
        oldDoc.hideDocumentWrapper();
      }

      this._tabStrip.selectTab(tabId);
      this._selectedDocumentId = docId;

      doc.showPanels();
      doc.showDocumentWrapper();
      doc.onSelectMainComponent();
      this.onDocumentLoaded(docId);

      return doc;
    }

    return undefined;
  }

  removeDocument(docId) {
    if (this._selectedDocumentId === docId) {
      this._selectedDocumentId = '';
    }

    $q.removeProperty(this._documents, docId);
  }

  updateTab(tabId, eventArgs) {
    this._tabStrip.updateTab(tabId, eventArgs);
  }

  showAjaxLoadingLayer() {
    const $loadingLayer = $(this._loadingLayerElement);
    $loadingLayer.show();
  }

  hideAjaxLoadingLayer() {
    const $loadingLayer = $(this._loadingLayerElement);
    $loadingLayer.hide();
  }

  raiseEvent(eventType, docId) {
    // eslint-disable-next-line no-use-before-define
    const eventArgs = new BackendEditingAreaEventArgs();
    const doc = this.getDocument(docId);
    if ($q.isObject(doc)) {
      BackendEventArgs.fillEventArgsFromOtherEventArgs(eventArgs, doc.getEventArgs());
      eventArgs.set_documentId(docId);
      eventArgs.set_isSelected(doc.isSelected());
    }

    this.notify(eventType, eventArgs);
  }

  markAsBusy() {
    this._tabStrip.markAsBusy();
    Object.values(this._documents).forEach(doc => doc.markPanelsAsBusy());
  }

  unmarkAsBusy() {
    this._tabStrip.unmarkAsBusy();
    Object.values(this._documents).forEach(doc => doc.unmarkPanelsAsBusy());
  }

  onDocumentError(docId) {
    this.unmarkAsBusy();
    this.raiseEvent(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR, docId);
  }

  onDocumentLoading(docId, isLocal) {
    this.markAsBusy();
    if (!isLocal) {
      this.raiseEvent(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING, docId);
    }
  }

  onDocumentLoaded(docId, isLocal) {
    this.unmarkAsBusy();
    if (!isLocal) {
      this.raiseEvent(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED, docId);
    }
  }

  onDocumentClosing(docId) {
    this.markAsBusy();
    this.raiseEvent(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING, docId);
  }

  onDocumentClosed(docId) {
    this.unmarkAsBusy();
    this.raiseEvent(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED, docId);
  }

  onActionExecuting(eventArgs) {
    this.notify(window.EVENT_TYPE_EDITING_AREA_ACTION_EXECUTING, eventArgs);
  }

  onEntityReaded(eventArgs) {
    this.notify(window.EVENT_TYPE_EDITING_AREA_ENTITY_READED, eventArgs);
  }

  onActionExecuted(eventArgs) {
    if (eventArgs.get_entityTypeCode() === window.ENTITY_TYPE_CODE_FIELD) {
      const viewInListAffected = eventArgs.get_context() && eventArgs.get_context().viewInListAffected;
      if (eventArgs.get_isRemoving()
        || (viewInListAffected
          && (eventArgs.get_isUpdated()
            || eventArgs.get_isSaved()))
      ) {
        Object.values(this._documents).forEach(doc => {
          if (
            doc.get_entityTypeCode() === window.ENTITY_TYPE_CODE_ARTICLE
            && doc.get_actionTypeCode() === window.ACTION_TYPE_CODE_LIST
            && doc.get_parentEntityId() === eventArgs.get_parentEntityId()
          ) {
            doc.refresh();
          }
        });
      }
    }

    if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving() || eventArgs.get_isRestoring()) {
      this.closeNonExistentDocuments();
    }

    if (eventArgs.get_isUpdated()) {
      this._tabStrip.updateParentInfo(eventArgs.get_entityTypeCode(), eventArgs.get_entityId());
    } else if (eventArgs.get_isRestored()
      && eventArgs.get_entityTypeCode() === window.ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      this._tabStrip.updateParentInfo(window.ENTITY_TYPE_CODE_ARTICLE, eventArgs.get_parentEntityId());
    } else if (eventArgs.get_actionTypeCode() === window.ACTION_TYPE_SIMPLE_UPDATE) {
      this.getSelectedDocument().refresh();
    }
  }

  onTabStripCloseRequest(eventType, sender, eventArgs) {
    this.closeDocument(eventArgs.get_tabId(), false);
  }

  onTabStripSaveAndCloseRequest(eventType, sender, eventArgs) {
    const docId = this.getDocumentIdByTabId(eventArgs.get_tabId());
    const doc = this.getDocument(docId);
    if (doc) {
      doc.saveAndCloseRequest();
    }
  }

  onTabStripSelectRequest(eventType, sender, eventArgs) {
    this.selectDocument(eventArgs.get_tabId());
  }

  onFindTabInTreeRequest(eventType, sender, eventArgs) {
    if (eventType === window.EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST) {
      this.notify(window.EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE, eventArgs);
    }
  }

  onWindowResized(e, options) {
    this.fixDocumentsContainerHeight(options);
  }

  closeNonExistentDocuments() {
    const that = this;
    const $tabs = this._tabStrip.getAllTabs();
    $tabs.each(
      index => {
        const $tab = $tabs.eq(index);
        if (!that._tabStrip.tabEntityExists($tab)) {
          that.closeDocument(that._tabStrip.getTabId($tab), true);
        }
      }
    );
  }

  closeDocument(tabId, withoutConfirm, isSaveAndUp) {
    const $tab = this._tabStrip.getTab(tabId);
    const docId = this.getDocumentIdByTabId(tabId);
    const doc = this.getDocument(docId);
    if (withoutConfirm || doc.allowClose()) {
      this.onDocumentClosing(docId);
      const $tabToSelect = this._tabStrip.getAnotherTabToSelect($tab);
      const wasSelected = doc.isSelected();
      doc.markMainComponentAsBusy();

      if (!isSaveAndUp) {
        doc.cancel();
      }

      doc.onDocumentUnloaded();
      this._tabStrip.closeTab(tabId);
      if (this._tabStrip.getAllTabsCount() === 0) {
        this.closeArea();
      }

      doc.unbindExternalCallerContexts('closed');

      this.onDocumentClosed(docId);
      this.destroyDocument(docId);
      if (wasSelected && !$q.isNullOrEmpty($tabToSelect)) {
        this.selectDocument(this._tabStrip.getTabId($tabToSelect));
      }
    }
  }

  onCloseHostMessageReceived(message) {
    const tabedDocs = this.get_documents();
    Object.values(tabedDocs).forEach(doc => {
      if (
        $.grep(
          doc.get_externalCallerContexts(),
          ctx => ctx.hostUID === message.hostUID && ctx.data.actionUID === message.data.actionUID
        ).length
      ) {
        this.closeDocument(doc.get_tabId());
      }
    });
  }

  hostExternalCallerContextsUnbinded(unbindingEventArgs) {
    this.notify(window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, unbindingEventArgs);
  }

  getGlobal(key) {
    return this._customVars[key];
  }

  setGlobal(key, value) {
    this._customVars[key] = value;
  }

  getCustomScriptState(key) {
    return this._customScripts[key];
  }

  setCustomScriptState(key, state) {
    this._customScripts[key] = state;
  }

  destroyDocument(docId) {
    const doc = this._documents[docId];
    if (doc && doc.dispose) {
      doc.dispose();
    }
  }

  dispose() {
    super.dispose();
    if (this._documents) {
      Object.keys(this._documents).forEach(this.destroyDocument, this);
    }

    if (this._loadingLayerElement) {
      const $loadingLayer = $(this._loadingLayerElement);
      $loadingLayer.empty();
      $loadingLayer.remove();
    }

    if (this._documentsContainerElement) {
      const $documentsContainer = $(this._documentsContainerElement);
      $documentsContainer.empty();
    }

    this._breadCrumbsContainerElement = null;
    this._actionToolbarContainerElement = null;
    this._viewToolbarContainerElement = null;
    this._searchBlockContainerElement = null;
    this._contextBlockContainerElement = null;
    this._editingAreaElement = null;
    if (this._tabStrip) {
      this._tabStrip.detachObserver(window.EVENT_TYPE_TAB_STRIP_TAB_CLOSE_REQUEST);
      this._tabStrip.detachObserver(window.EVENT_TYPE_TAB_STRIP_TAB_SELECT_REQUEST);
      this._tabStrip.detachObserver(window.EVENT_TYPE_TAB_STRIP_TAB_SAVE_CLOSE_REQUEST);
      this._tabStrip.detachObserver(window.EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST);
    }

    $(window).unbind('resize', this._onWindowResizedHandler);
    $q.collectGarbageInIE();
  }
}


BackendEditingArea._instance = null;
BackendEditingArea.getInstance = function (editingAreaElementId, options) {
  if (!BackendEditingArea._instance) {
    const instance = new BackendEditingArea(editingAreaElementId, options);
    instance.initialize();
    BackendEditingArea._instance = instance;
  }

  return BackendEditingArea._instance;
};

BackendEditingArea.destroyInstance = function () {
  if (BackendEditingArea._instance) {
    BackendEditingArea._instance.dispose();
    BackendEditingArea._instance = null;
  }
};


export class BackendEditingAreaEventArgs extends BackendEventArgs {
  // eslint-disable-next-line no-useless-constructor, FIXME
  constructor() {
    super();
  }

  _documentId = null;
  _isSelected = false;

  // eslint-disable-next-line camelcase
  get_documentId() {
    return this._documentId;
  }

  // eslint-disable-next-line camelcase
  set_documentId(value) {
    this._documentId = value;
  }

  // eslint-disable-next-line camelcase
  get_isSelected() {
    return this._isSelected;
  }

  // eslint-disable-next-line camelcase
  set_isSelected(value) {
    this._isSelected = value;
  }
}


Quantumart.QP8.BackendEditingArea = BackendEditingArea;
Quantumart.QP8.BackendEditingAreaEventArgs = BackendEditingAreaEventArgs;
