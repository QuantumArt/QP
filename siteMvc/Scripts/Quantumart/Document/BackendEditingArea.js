// ****************************************************************************
// *** Компонент "Область редактирования"                 ***
// ****************************************************************************

//#region event types of editing area
// === Типы событий области редактирования ===
var EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING = "OnEditingAreaDocumentLoading";
var EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED = "OnEditingAreaDocumentLoaded";
var EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING = "OnEditingAreaDocumentClosing";
var EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED = "OnEditingAreaDocumentClosed";
var EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR = "OnEditingAreaDocumentError";
var EVENT_TYPE_EDITING_AREA_CLOSED = "OnEditingAreaClosed";
var EVENT_TYPE_EDITING_AREA_ACTION_EXECUTING = "OnEditingAreaActionExecuting";
var EVENT_TYPE_EDITING_AREA_ENTITY_READED = "OnEditingAreaEntityReaded";
var EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE = "OnEditingAreaFindTabInTree";
//#endregion

//#region class BackendEditingArea
// === Класс "Область редактирования" ===
Quantumart.QP8.BackendEditingArea = function (editingAreaElementId, options) {
  Quantumart.QP8.BackendEditingArea.initializeBase(this);

  this._hostStateStorage = new Quantumart.QP8.BackendDocumentHostStateStorage({
    currentCustomerCode: options.currentCustomerCode,
    currentUserId: options.currentUserId
  });

  this._editingAreaElementId = editingAreaElementId;
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
      this._onTabStripCloseRequestHandler = jQuery.proxy(this.onTabStripCloseRequest, this);
      this._onTabStripSelectRequestHandler = jQuery.proxy(this.onTabStripSelectRequest, this);
      this._tabStrip.attachObserver(EVENT_TYPE_TAB_STRIP_TAB_CLOSE_REQUEST, this._onTabStripCloseRequestHandler);
      this._tabStrip.attachObserver(EVENT_TYPE_TAB_STRIP_TAB_SELECT_REQUEST, this._onTabStripSelectRequestHandler);
      this._tabStrip.attachObserver(EVENT_TYPE_TAB_STRIP_TAB_SAVE_CLOSE_REQUEST, jQuery.proxy(this.onTabStripSaveAndCloseRequest, this));
      this._tabStrip.attachObserver(EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST, jQuery.proxy(this.onFindTabInTreeRequest, this));
    }
  }

  this._onWindowResizedHandler = jQuery.proxy(this.onWindowResized, this);
};

Quantumart.QP8.BackendEditingArea.prototype = {
  _editingAreaElementId: "", // клиентский идентификатор области редактирования
  _editingAreaElement: null, // DOM-элемент, образующий область редактирования
  _documentsContainerElementId: "", // клиентский идентификатор контейнера для документов
  _documentsContainerElement: null, // DOM-элемент, образующий контейнер для документов
  _loadingLayerElement: null, // DOM-элемент, образующий блокирующий слой с индикатором загрузки
  _documentsContainerHeightDifference: 0, // количество пикселей, которое нужно отнять от высоты контейнера для документов
  _selectedDocumentId: "", // идентификатор выбранного документа
  _breadCrumbsContainerElementId: "", // клиентский идентификатор контейнера, в котором располагаются хлебные крошки
  _breadCrumbsContainerElement: null, // DOM-элемент, образующий контейнер, в котором располагаются хлебные крошки
  _actionToolbarContainerElementId: "", // клиентский идентификатор контейнера, в котором располагается панель инструментов для действий
  _actionToolbarContainerElement: null, // DOM-элемент, образующий контейнер, в котором располагается панель инструментов для действий
  _viewToolbarContainerElementId: "", // клиентский идентификатор контейнера, в котором располагается панель инструментов для представлений
  _viewToolbarContainerElement: null, // DOM-элемент, образующий контейнер, в котором располагается панель инструментов для представлений
  _searchBlockContainerElementId: "", // клиентский идентификатор контейнера, в котором располагается блок поиска
  _searchBlockContainerElement: null, // DOM-элемент, образующий контейнер, в котором располагается блок поиска
  _contextBlockContainerElementId: "", // клиентский идентификатор контейнера, в котором располагается блок контекста
  _contextBlockContainerElement: null, // DOM-элемент, образующий контейнер, в котором располагается блок контекста
  _documents: null, // объекты для редатирования
  _tabStrip: null,
  _hostStateStorage: null,
  _customVars: null,
  _customScripts: null,

  _onWindowResizedHandler: null,


  get_editingAreaElementId: function () {
    return this._editingAreaElementId;
  },

  set_editingAreaElementId: function (value) {
    this._editingAreaElementId = value;
  },

  get_documentsContainerElementId: function () {
    return this._documentsContainerElementId;
  },

  set_documentsContainerElementId: function (value) {
    this._documentsContainerElementId = value;
  },

  get_documentsContainerElement: function () {
    return this._documentsContainerElement;
  },

  get_documentsContainerHeightDifference: function () {
    return this._documentsContainerHeightDifference;
  },

  set_documentsContainerHeightDifference: function (value) {
    this._documentsContainerHeightDifference = value;
  },

  get_breadCrumbsContainerElementId: function () {
    return this._breadCrumbsContainerElementId;
  },

  set_breadCrumbsContainerElementId: function (value) {
    this._breadCrumbsContainerElementId = value;
  },

  get_breadCrumbsContainerElement: function () {
    return this._breadCrumbsContainerElement;
  },

  get_actionToolbarContainerElementId: function () {
    return this._actionToolbarContainerElementId;
  },

  set_actionToolbarContainerElementId: function (value) {
    this._actionToolbarContainerElementId = value;
  },

  get_actionToolbarContainerElement: function () {
    return this._actionToolbarContainerElement;
  },

  get_viewToolbarContainerElementId: function () {
    return this._viewToolbarContainerElementId;
  },

  set_viewToolbarContainerElementId: function (value) {
    this._viewToolbarContainerElementId = value;
  },

  get_viewToolbarContainerElement: function () {
    return this._viewToolbarContainerElement;
  },

  get_searchBlockContainerElementId: function () {
    return this._searchBlockContainerElementId;
  },

  set_searchBlockContainerElementId: function (value) {
    this._searchBlockContainerElementId = value;
  },

  get_searchBlockContainerElement: function () {
    return this._searchBlockContainerElement;
  },


  get_tabStrip: function () { return this._tabStrip; },
  set_tabStrip: function (value) { this._tabStrip = value; },

  get_contextBlockContainerElementId: function () { return this._contextBlockContainerElementId; },
  set_contextBlockContainerElementId: function (value) { this._contextBlockContainerElementId = value; },
  get_contextBlockContainerElement: function () { return this._contextBlockContainerElement; },
  set_contextBlockContainerElement: function (value) { this._contextBlockContainerElement = value; },


  get_documents: function () {
    return this._documents;
  },

  initialize: function () {
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

    var $editingArea = jQuery("#" + this._editingAreaElementId);
    var $documentsContainer = jQuery("#" + this._documentsContainerElementId);
    var $loadingLayer = jQuery("<div />", { "class": "loadingLayer", "css": { "display": "none"} });
    var $breadCrumbsContainer = jQuery(this._breadCrumbsContainerElementId);
    var $actionToolbarContainer = jQuery("#" + this._actionToolbarContainerElementId);
    var $viewToolbarContainer = jQuery("#" + this._viewToolbarContainerElementId);
    var $searchBlockContainer = jQuery("#" + this._searchBlockContainerElementId);
    var $contextBlockContainer = jQuery("#" + this._contextBlockContainerElementId);

    $documentsContainer.prepend($loadingLayer);

    this._editingAreaElement = $editingArea.get(0);
    this._documentsContainerElement = $documentsContainer.get(0);
    this._breadCrumbsContainerElement = $breadCrumbsContainer.get(0);
    this._actionToolbarContainerElement = $actionToolbarContainer.get(0);
    this._viewToolbarContainerElement = $viewToolbarContainer.get(0);
    this._searchBlockContainerElement = $searchBlockContainer.get(0);
    this._contextBlockContainerElement = $contextBlockContainer.get(0);
    this._loadingLayerElement = $loadingLayer.get(0);

    jQuery(window).resize(this._onWindowResizedHandler);
  },

  fixDocumentsContainerHeight: function (options) {
    if (options && options.bySplitter == true) return;

    var $documentsContainer = jQuery(this._documentsContainerElement);
    var oldDocumentsContainerHeight = parseInt($documentsContainer.height());
    var newDocumentsContainerHeight = parseInt(jQuery(window).height()) - this._documentsContainerHeightDifference;

    if (oldDocumentsContainerHeight != newDocumentsContainerHeight) {
      if (newDocumentsContainerHeight < 300) {
        newDocumentsContainerHeight = 300;
      }
      $documentsContainer.css("height", newDocumentsContainerHeight + "px");
    }

    $documentsContainer = null;
  },

  openArea: function () {
    var $editingArea = jQuery(this._editingAreaElement);
    $editingArea.css("display", "block");

    $editingArea = null;
  },

  closeArea: function () {
    var $editingArea = jQuery(this._editingAreaElement);
    $editingArea.css("display", "none");

    $editingArea = null;

    var $documentsContainer = jQuery(this._documentsContainerElement);
    $documentsContainer
      .find("DIV.documentWrapper")
        .empty()
        .remove()
        ;

    $documentsContainer = null;

    this._selectedDocumentId = "";
    this.notify(EVENT_TYPE_EDITING_AREA_CLOSED, new Quantumart.QP8.BackendEditingAreaEventArgs());
  },

  getDocumentIdByTabId: function (tabId) {
    var editingDocumentId = String.format("{0}_document", tabId);

    return editingDocumentId;
  },

  getExistingTabId: function (eventArgs) {
    return this._tabStrip.getExistingTabId(eventArgs);
  },

  getSelectedDocument: function () {
    return this.getDocument(this._selectedDocumentId);
  },

  getSelectedDocumentId: function () {
    return this._selectedDocumentId;
  },

  getDocument: function (docId) {
    return ($q.isNullOrWhiteSpace(docId)) ? null : this._documents[docId];
  },

  getDocumentByEventArgs: function (eventArgs) {
    var tabId = this.getExistingTabId(eventArgs);
    var docId = this.getDocumentIdByTabId(tabId);
    return this.getDocument(docId);
  },

  addDocument: function (eventArgs) {
    if (!eventArgs.get_isWindow()) {
      var tabId = this.getExistingTabId(eventArgs);
      if (tabId !== 0) {
        if (jQuery.isEmptyObject(eventArgs.get_context()) || !eventArgs.get_context().ctrlKey) {
          var selectedDocument = this.selectDocument(tabId);
          if (selectedDocument) {
            selectedDocument.onSelectedThroughExecution(eventArgs);
          }
        }
      } else {
        this.onDocumentLoading();
        if (this._tabStrip.getAllTabsCount() == 0) {
          this.openArea();
        }

        var tabId = this._tabStrip.addNewTab(eventArgs);
        var oldDoc = this.getSelectedDocument();
        if (oldDoc) {
          oldDoc.hidePanels();
          oldDoc.hideDocumentWrapper();
        }

        var doc = new Quantumart.QP8.BackendEditingDocument(tabId, this, eventArgs, { hostStateStorage: this._hostStateStorage });
        var docId = doc.get_id();
        this._documents[docId] = doc;
        this._selectedDocumentId = docId;

        var self = this;
        doc.initialize(function () {
          self.onDocumentLoaded(self._selectedDocumentId);
        });

        if (eventArgs.get_context() && eventArgs.get_context().ctrlKey) {
          this.selectDocument(this._tabStrip._previousSelectedTabId);
        }
      }
    }
  },

  selectDocument: function (tabId) {
    var docId = this.getDocumentIdByTabId(tabId);
    var doc = this.getDocument(docId);
    var oldDoc = this.getSelectedDocument();
    if (doc !== null) {
      this.onDocumentLoading(docId);

      if (oldDoc !== null) {
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
  },

  removeDocument: function (docId) {
    if (this._selectedDocumentId == docId) {
      this._selectedDocumentId = "";
    }

    $q.removeProperty(this._documents, docId);
  },

  destroyDocument: function (docId) {
    var doc = this._documents[docId];

    if (doc != null) {
      if (doc.dispose) {
        doc.dispose();
      }
      doc = null;
    }
  },

  updateTab: function (tabId, eventArgs) {
    this._tabStrip.updateTab(tabId, eventArgs);
  },

  showAjaxLoadingLayer: function () {
    var $loadingLayer = jQuery(this._loadingLayerElement);
    $loadingLayer.show();
  },

  hideAjaxLoadingLayer: function () {
    var $loadingLayer = jQuery(this._loadingLayerElement);
    $loadingLayer.hide();
  },

  raiseEvent: function (eventType, docId) {
    var eventArgs = new Quantumart.QP8.BackendEditingAreaEventArgs();
    var doc = this.getDocument(docId);
    if ($q.isObject(doc)) {
      Quantumart.QP8.BackendEventArgs.fillEventArgsFromOtherEventArgs(eventArgs, doc.getEventArgs());
      eventArgs.set_documentId(docId);
      eventArgs.set_isSelected(doc.isSelected());
    }
    this.notify(eventType, eventArgs);
    eventArgs = null;
  },

  markAsBusy: function () {
    this._tabStrip.markAsBusy();
    for (var docId in this._documents) {
      this._documents[docId].markPanelsAsBusy();
    }
  },

  unmarkAsBusy: function () {
    this._tabStrip.unmarkAsBusy();
    for (var docId in this._documents) {
      this._documents[docId].unmarkPanelsAsBusy();
    }
  },

  onDocumentError: function (docId) {
    this.unmarkAsBusy();
    this.raiseEvent(EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR, docId);
  },

  onDocumentLoading: function (docId, isLocal) {
    this.markAsBusy();
    if (!isLocal) {
      this.raiseEvent(EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING, docId);
    }
  },

  onDocumentLoaded: function (docId, isLocal) {
    this.unmarkAsBusy();
    if (!isLocal) {
      this.raiseEvent(EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED, docId);
    }
  },

  onDocumentClosing: function (docId) {
    this.markAsBusy();
    this.raiseEvent(EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING, docId);
  },

  onDocumentClosed: function (docId) {
    this.unmarkAsBusy();
    this.raiseEvent(EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED, docId);
  },

  onActionExecuting: function (eventArgs) {
    this.notify(EVENT_TYPE_EDITING_AREA_ACTION_EXECUTING, eventArgs);
  },

  onEntityReaded: function (eventArgs) {
    this.notify(EVENT_TYPE_EDITING_AREA_ENTITY_READED, eventArgs);
  },

  onActionExecuted: function (eventArgs) {

    if (eventArgs.get_entityTypeCode() == ENTITY_TYPE_CODE_FIELD)
    {
      var viewInListAffected = (eventArgs.get_context() && eventArgs.get_context().viewInListAffected);

      if (eventArgs.get_isRemoving() || (viewInListAffected && (eventArgs.get_isUpdated() || eventArgs.get_isSaved())))
      {
        for (var docId in this._documents) {
          var doc = this._documents[docId];
          if (
            doc.get_entityTypeCode() == ENTITY_TYPE_CODE_ARTICLE
            && doc.get_actionTypeCode() == ACTION_TYPE_CODE_LIST
            && doc.get_parentEntityId() == eventArgs.get_parentEntityId()
          ) {
            doc.refresh();
          }
        }
      }

    }


    if (eventArgs.get_isRemoving() || eventArgs.get_isArchiving() || eventArgs.get_isRestoring()) {
      this.closeNonExistentDocuments();
    }

    if (eventArgs.get_isUpdated()) {
      this._tabStrip.updateParentInfo(eventArgs.get_entityTypeCode(), eventArgs.get_entityId());
    }
    else if (eventArgs.get_isRestored() && eventArgs.get_entityTypeCode() == ENTITY_TYPE_CODE_ARTICLE_VERSION) {
      this._tabStrip.updateParentInfo(ENTITY_TYPE_CODE_ARTICLE, eventArgs.get_parentEntityId());
    }
    else if (eventArgs.get_actionTypeCode() == ACTION_TYPE_SIMPLE_UPDATE) {
      this.getSelectedDocument().refresh();
    }
  },

  onTabStripCloseRequest: function (eventType, sender, eventArgs) {
    this.closeDocument(eventArgs.get_tabId(), false);
  },

  onTabStripSaveAndCloseRequest: function (eventType, sender, eventArgs) {
      var docId = this.getDocumentIdByTabId(eventArgs.get_tabId());
      var doc = this.getDocument(docId);
      if (!!doc) {
          doc.saveAndCloseRequest(eventArgs);
      }
  },

  onTabStripSelectRequest: function (eventType, sender, eventArgs) {
    this.selectDocument(eventArgs.get_tabId());
  },

  onFindTabInTreeRequest: function (eventType, sender, eventArgs) {
      if (eventType === EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST) {
          this.notify(EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE, eventArgs);
      }
  },

  onWindowResized: function (e, options) {
    this.fixDocumentsContainerHeight(options);
  },

  closeNonExistentDocuments: function () {
    var self = this;
    var $tabs = this._tabStrip.getAllTabs();
    $tabs.each(
      function (index) {
        var $tab = $tabs.eq(index);
        if (!self._tabStrip.tabEntityExists($tab)) {
          self.closeDocument(self._tabStrip.getTabId($tab), true);
        }
      }
    );
    $tabs = null;
  },

  closeDocument: function (tabId, withoutConfirm, isSaveAndUp) {
    var $tab = this._tabStrip.getTab(tabId);
    var docId = this.getDocumentIdByTabId(tabId);
    var doc = this.getDocument(docId);
    if (withoutConfirm || doc.allowClose()) {
      this.onDocumentClosing(docId);
      var $tabToSelect = this._tabStrip.getAnotherTabToSelect($tab);
      var wasSelected = doc.isSelected();

      doc.markMainComponentAsBusy();
      if (!isSaveAndUp) {
        doc.cancel();
      }
      doc.onDocumentUnloaded();

      this._tabStrip.closeTab(tabId);
      if (this._tabStrip.getAllTabsCount() == 0) {
        this.closeArea();
      }

      doc.unbindExternalCallerContexts("closed");

      this.onDocumentClosed(docId);
      this.destroyDocument(docId);

      if (wasSelected && !$q.isNullOrEmpty($tabToSelect)) {
        this.selectDocument(this._tabStrip.getTabId($tabToSelect));
      }

    }
    $tab = null;
  },

  onCloseHostMessageReceived: function (message) {
    var tabedDocs = this.get_documents();
    for (var docId in tabedDocs) {
      var doc = tabedDocs[docId];
      if (jQuery.grep(doc.get_externalCallerContexts(), function (c) {
        return c.hostUID == message.hostUID && c.data.actionUID == message.data.actionUID;
      }).length > 0) {
        this.closeDocument(doc.get_tabId());
      }
    }
  },

  hostExternalCallerContextsUnbinded: function (unbindingEventArgs) {
    this.notify(EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, unbindingEventArgs);
  },

  getGlobal: function (key) {
    return this._customVars[key];
  },

  setGlobal: function (key, value) {
    this._customVars[key] = value;
  },

  getCustomScriptState: function(key)
  {
    return this._customScripts[key];
  },

  setCustomScriptState: function(key, state)
  {
    this._customScripts[key] = state;
  },



  dispose: function () {
    Quantumart.QP8.BackendEditingArea.callBaseMethod(this, "dispose");

    if (this._documents) {
      for (var docId in this._documents) {
        this.destroyDocument(docId);
      }
    }

    if (this._loadingLayerElement) {
      var $loadingLayer = jQuery(this._loadingLayerElement);
      $loadingLayer.empty();
      $loadingLayer.remove();

      $loadingLayer = null;
      this._loadingLayerElement = null;
    }

    if (this._documentsContainerElement != null) {
      var $documentsContainer = jQuery(this._documentsContainerElement);
      $documentsContainer.empty();

      $documentsContainer = null;
      this._documentsContainerElement = null;
    }

    this._breadCrumbsContainerElement = null;
    this._actionToolbarContainerElement = null;
    this._viewToolbarContainerElement = null;
    this._searchBlockContainerElement = null;
    this._contextBlockContainerElement = null;
    this._editingAreaElement = null;
    if (this._tabStrip != null) {
      this._tabStrip.detachObserver(EVENT_TYPE_TAB_STRIP_TAB_CLOSE_REQUEST);
      this._tabStrip.detachObserver(EVENT_TYPE_TAB_STRIP_TAB_SELECT_REQUEST);
      this._tabStrip.detachObserver(EVENT_TYPE_TAB_STRIP_TAB_SAVE_CLOSE_REQUEST);
      this._tabStrip.detachObserver(EVENT_TYPE_TAB_STRIP_FIND_IN_TREE_REQUEST);

    }
    this._tabStrip = null;

    jQuery(window).unbind("resize", this._onWindowResizedHandler);

    this._onWindowResizedHandler = null;

    if (this._hostStateStorage) {
      this._hostStateStorage.dispose();
      this._hostStateStorage = null;
    }

    Quantumart.QP8.BackendEditingArea._instance = null;

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendEditingArea._instance = null; // экземпляр класса

// Возвращает экземпляр класса "Область редактирования"
Quantumart.QP8.BackendEditingArea.getInstance = function Quantumart$QP8$BackendEditingArea$getInstance(editingAreaElementId, options) {
  if (Quantumart.QP8.BackendEditingArea._instance == null) {
    var instance = new Quantumart.QP8.BackendEditingArea(editingAreaElementId, options);
    instance.initialize();

    Quantumart.QP8.BackendEditingArea._instance = instance;
  }

  return Quantumart.QP8.BackendEditingArea._instance;
};

// Уничтожает экземпляр класса "Область редактирования"
Quantumart.QP8.BackendEditingArea.destroyInstance = function Quantumart$QP8$BackendEditingArea$destroyInstance() {
  if (Quantumart.QP8.BackendEditingArea._instance) {
    Quantumart.QP8.BackendEditingArea._instance.dispose();
  }
};

Quantumart.QP8.BackendEditingArea.registerClass("Quantumart.QP8.BackendEditingArea", Quantumart.QP8.Observable);
//#endregion

//#region class BackendEditingAreaEventArgs
// === Класс "Аргументы события, вызванного областью редактирования" ===
Quantumart.QP8.BackendEditingAreaEventArgs = function () {
  Quantumart.QP8.BackendEditingAreaEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendEditingAreaEventArgs.prototype = {
  _documentId: null, // идентификатор редактируемого документа
  _isSelected: false,

  get_documentId: function () { return _documentId; },
  set_documentId: function (value) { _documentId = value; },
  get_isSelected: function () { return _isSelected; },
  set_isSelected: function (value) { _isSelected = value; }
};


Quantumart.QP8.BackendEditingAreaEventArgs.registerClass("Quantumart.QP8.BackendEditingAreaEventArgs", Quantumart.QP8.BackendEventArgs);
//#endregion

