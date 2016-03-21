//#region class BackendDocumentContext
// === Cодержимое документа ===
Quantumart.QP8.BackendDocumentContext = function(params, options) {
  Quantumart.QP8.BackendDocumentContext.initializeBase(this);
  this._hostId = params.hostId;
  if ($q.isBoolean(params.isWindow)) {
    this._isWindow = params.isWindow;
  }

  this._params = params;

  if (params.entityId) {
    this._entityId = params.entityId;
  }

  if (params.entityName) {
    this._entityName = params.entityName;
  }

  if (params.entities) {
    this._entities = params.entities;
  }

  if (params.parentEntityId) {
    this._parentEntityId = params.parentEntityId;
  }

  if (params.entityTypeCode) {
    this._entityTypeCode = params.entityTypeCode;
  }

  if (params.actionCode) {
    this._actionCode = params.actionCode;
  }

  if (params.previousActionCode) {
    this._previousActionCode = params.previousActionCode;
  }

  if (params.state) {
    this._state = params.state;
  }

  if (params.mainComponentType) {
    this._mainComponentType = params.mainComponentType;
  }

  if (params.mainComponentId) {
    this._mainComponentId = params.mainComponentId;
  }

  this._options = options;
  this._options.isWindow = this._isWindow;

  this._customLinkButtonsSettings = [];
  this._customButtonsSettings = [];
};

Quantumart.QP8.BackendDocumentContext.prototype = {
  test: function() {
    return this;
  },
  _hostId: '',
  _isWindow: false,
  _entityId: 0,
  _entityName: '',
  _entities: null,
  _parentEntityId: 0,
  _entityTypeCode: '',
  _actionCode: '',
  _previousActionCode: '',
  _state: $e.DocumentContextState.None,
  _mainComponentType: $e.MainComponentType.Editor,
  _mainComponentId: '',
  _mainComponent: null,
  _eventArgs: null,
  _initializingCallback: null,
  _initializedCallback: null,
  _terminatingCallback: null,
  _terminatedCallback: null,
  _execSelectCallback: null,
  _hostLoadedCallback: null,

  initHandler: null,
  disposeHandler: null,
  beforeSubmitHandler: null,
  loadHanlder: null,
  fieldValueChangedHandler: null,

  _options: null,
  _params: null,

  _customLinkButtonsSettings: null,
  _customButtonsSettings: null,

  get_mainComponent: function() {
    return this._mainComponent;
  },
  get_mainComponentType: function() {
    return this._mainComponentType;
  },
  get_state: function() {
    return this._state;
  },

  set_initializingCallback: function(value) {
    this._initializingCallback = value;
  },
  set_initializedCallback: function(value) {
    this._initializedCallback = value;
  },
  set_terminatingCallback: function(value) {
    this._terminatingCallback = value;
  },
  set_terminatedCallback: function(value) {
    this._terminatedCallback = value;
  },
  set_execSelectCallback: function(value) {
    this._execSelectCallback = value;
  },
  set_hostLoadedCallback: function(value) {
    this._hostLoadedCallback = value;
  },

  init: function() {
    Sys.Debug.trace('initPage ' + this._hostId);
    if (!this.needUp()) {
      $q.callFunction(this._initializingCallback, this);
    }

    var host = this.getHost();

    if (host) {
      this._mainComponent = this.createMainComponent(host);
      host.set_documentContext(this);
    }

    if (!this.needUp()) {
      $q.callFunction(this._initializedCallback, this);
    }
  },

  getHost: function() {
    if (this._isWindow) {
      return Quantumart.QP8.BackendPopupWindowManager.getInstance().getPopupWindow(this._hostId);
    } else {
      return this._getArea().getDocument(this._hostId);
    }
  },

  _getArea: function() {
    return Quantumart.QP8.BackendEditingArea.getInstance();
  },

  setGlobal: function(key, value) {
    $ctx.setGlobal(key, value);
  },

  getGlobal: function(key) {
    return $ctx.getGlobal(key);
  },

  loadScript: function(url, key) {
    if (!key) {
      key = url;
    }

    if (!this._getArea().getCustomScriptState(key)) {
      var result = jQuery.ajax({
        type: 'get',
        dataType: 'script',
        url: url,
        cache: false,
        async: false
      });

      this._getArea().setCustomScriptState(key, true);
    }
  },

  addCustomButton: function(settings) {
    this._customButtonsSettings.push(settings);
  },

  addCustomLinkButton: function(settings) {
    this._customLinkButtonsSettings.push(settings);
  },

  execSelect: function(eventArgs) {
    if (this._execSelectCallback)
    this._execSelectCallback(eventArgs);
  },

  createMainComponent: function(host) {
    var hostOptions = {
      viewTypeCode: host.get_viewTypeCode(),
      searchQuery: host.get_searchQuery(),
      contextQuery: host.get_contextQuery(),
      filter: host.get_filter(),
      hostType: host.get_hostType(),
      currentPage: host.get_currentPage(),
      orderBy: host.get_orderBy(),
      eventArgsAdditionalData: host.get_eventArgsAdditionalData(),
      isBindToExternal: host.get_isBindToExternal(),
      zIndex: host.get_zIndex()
    };

    var hostHandler = host._onGeneralEventHandler;
    var mainComponent = null;

    if (this._mainComponentType == $e.MainComponentType.Grid) {
      mainComponent = Quantumart.QP8.BackendEntityGridManager.getInstance().createGrid(this._mainComponentId, this._entityTypeCode, this._parentEntityId, this._actionCode, this._options, hostOptions);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_GRID_DATA_BINDING, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_GRID_DATA_BOUND, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_GRID_TITLE_LINK_CLICK, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.Tree) {
      mainComponent = Quantumart.QP8.BackendEntityTreeManager.getInstance().createTree(this._mainComponentId, this._entityTypeCode, this._parentEntityId, this._actionCode, this._options, hostOptions);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_TREE_DATA_BOUND, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.Library) {
      mainComponent = Quantumart.QP8.BackendLibraryManager.getInstance().createLibrary(this._mainComponentId, this._parentEntityId, this._actionCode, this._options, hostOptions);
      mainComponent.attachObserver(EVENT_TYPE_LIBRARY_DATA_BOUND, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_LIBRARY_ENTITY_SELECTED, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_LIBRARY_ACTION_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.Editor) {
      this._options.needUp = this.needUp();
      this._options.customHandlers = this.getCustomHandlers();
      this._options.customButtonsSettings = this._customButtonsSettings;
      this._options.customLinkButtonsSettings = this._customLinkButtonsSettings;
      this._options.notifyCustomButtonExistence = this.notifyCustomButtonExistence;

      mainComponent = Quantumart.QP8.BackendEntityEditorManager.getInstance().createEditor(host.get_documentWrapperElementId(), this._entityTypeCode, this._entityId, '', this._options, hostOptions);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTING, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED_ERROR, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_EDITOR_ENTITY_REFRESH_STARTING, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_ENTITY_EDITOR_ACTION_EXECUTING, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_LOADED, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_CLASSIFIER_FIELD_ARTICLE_UNLOADING, hostHandler);
      mainComponent.attachObserver(EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.CustomActionHost) {
      if (hostOptions.eventArgsAdditionalData) {
        jQuery.extend(this._params, {
          additionalParams: hostOptions.eventArgsAdditionalData.additionalParams
        });
      }

      mainComponent = Quantumart.QP8.BackendCustomActionHostManager.getInstance().createComponent(this._hostId, this._params);
      mainComponent.attachObserver(EVENT_TYPE_EXTERNAL_ACTION_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.ActionPermissionView) {
      mainComponent = Quantumart.QP8.BackendActionPermissionViewManager.getInstance().createView(this._mainComponentId, this._options, hostOptions);
      mainComponent.attachObserver(EVENT_TYPE_ACTION_PERMISSIONS_VIEW_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.Area) {
      mainComponent = {
        initialize: function() { },
        dispose: function() { }
      };
    }

    mainComponent.initialize();
    return mainComponent;
  },

  onHostLoaded: function() {
    $q.callFunction(this._hostLoadedCallback, this);
  },

  get_eventArgs: function() {
    if (!this._eventArgs) {
      var action = $a.getBackendAction(this._actionCode);
      var eventArgs = $a.getEventArgsFromAction(action);

      eventArgs.set_entityId(this._entityId);
      eventArgs.set_entityName(this._entityName);
      eventArgs.set_entities(this._entities);
      eventArgs.set_parentEntityId(this._parentEntityId);
      eventArgs.set_context(this.getEventArgsContext());
      eventArgs.set_previousAction(this.getPreviousAction());
      eventArgs.set_isWindow(this._isWindow);
      this._eventArgs = eventArgs;
    }

    return this._eventArgs;
  },

  getEventArgsContext: function() {
    var context = { orderChanged: false, groupChanged: false, viewInListAffected: false };

    if ($q.isBoolean(this._options.orderChanged)) {
      context.orderChanged = this._options.orderChanged;
    }

    if ($q.isBoolean(this._options.groupChanged)) {
      context.groupChanged = this._options.groupChanged;
    }

    if ($q.isBoolean(this._options.viewInListAffected)) {
      context.viewInListAffected = this._options.viewInListAffected;
    }

    return context;
  },

  modifyEventArgsContext: function(eventArgsContext) {
    return eventArgsContext;
  },

  getPreviousAction: function() {
    var previousAction = null;

    if (this._previousActionCode) {
      var action = $a.getBackendAction(this._previousActionCode);

      if (action) {
        previousAction = new Quantumart.QP8.BackendPreviousAction({
          entityTypeCode: action.EntityType.Code,
          actionTypeCode: action.ActionType.Code,
          actionCode: action.Code,
          isSuccessfullyExecuted: true
        });
      }
    }

    return previousAction;
  },

  get_options: function() {
    return this._options;
  },

  needUp: function() {
    var result = false;

    if (this._mainComponentType == $e.MainComponentType.Editor && this._state != $e.DocumentContextState.Error) {
      var eventArgs = this.get_eventArgs();

      if (eventArgs.get_needUp() && !$q.isNull(eventArgs.get_previousAction())) {
        result = true;
      }
    }

    return result;
  },

  disposeMainComponent: function(host) {
    var hostHandler = host._onGeneralEventHandler;

    if (this._mainComponentType == $e.MainComponentType.Grid) {
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_GRID_DATA_BINDING, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_GRID_DATA_BOUND, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_GRID_ENTITY_SELECTED, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_GRID_TITLE_LINK_CLICK, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_GRID_ACTION_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.Editor) {
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTING, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_EDITOR_ENTITY_SUBMITTED_ERROR, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_EDITOR_ENTITY_REFRESH_STARTING, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_EDITOR_ACTION_EXECUTING, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_LIBRARY_ALL_FILES_UPLOADED, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.Library) {
      this._mainComponent.detachObserver(EVENT_TYPE_LIBRARY_DATA_BOUND, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_LIBRARY_ENTITY_SELECTED, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_LIBRARY_ACTION_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.Tree) {
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_TREE_DATA_BOUND, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_TREE_ENTITY_SELECTED, hostHandler);
      this._mainComponent.detachObserver(EVENT_TYPE_ENTITY_TREE_ACTION_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.CustomActionHost) {
      this._mainComponent.detachObserver(EVENT_TYPE_EXTERNAL_ACTION_EXECUTING, hostHandler);
    } else if (this._mainComponentType == $e.MainComponentType.ActionPermissionView) {
      this._mainComponent.detachObserver(EVENT_TYPE_ACTION_PERMISSIONS_VIEW_EXECUTING, hostHandler);
    }

    this._mainComponent.dispose();
    this._mainComponent = null;
  },

  getCustomHandlers: function() {
    var result = {};

    if (this.initHandler) {
      result.init = jQuery.proxy(this.initHandler, this);
    }

    if (this.disposeHandler) {
      result.dispose = jQuery.proxy(this.disposeHandler, this);
    }

    if (this.beforeSubmitHandler) {
      result.beforeSubmit = jQuery.proxy(this.beforeSubmitHandler, this);
    }

    if (this.fieldValueChangedHandler) {
      result.fieldValueChanged = jQuery.proxy(this.fieldValueChangedHandler, this);
    }

    if (this.loadHandler) {
      result.load = jQuery.proxy(this.loadHandler, this);
    }

    return result;
  },

  dispose: function() {
    Sys.Debug.trace('terminatePage ' + this._hostId);
    if (!this.needUp())
    $q.callFunction(this._terminatingCallback, this);
    var host = this.getHost();

    if (host) {
      this.disposeMainComponent(host);
      host.set_documentContext(null);
    }

    if (!this.needUp())
    $q.callFunction(this._terminatedCallback, this);
  }
};

var $ctx = Quantumart.QP8.BackendDocumentContext;

Quantumart.QP8.BackendDocumentContext.getArea = function() {
  return Quantumart.QP8.BackendEditingArea.getInstance();
};

Quantumart.QP8.BackendDocumentContext.getGlobal = function(key) {
  return $ctx.getArea().getGlobal(key);
};

Quantumart.QP8.BackendDocumentContext.setGlobal = function(key, value) {
  return $ctx.getArea().setGlobal(key, value);
};

Quantumart.QP8.BackendDocumentContext.registerClass('Quantumart.QP8.BackendDocumentContext');

//#endregion
