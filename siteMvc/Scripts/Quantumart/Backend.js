// #region class Backend
// === Класс "Backend" ===
Quantumart.QP8.Backend = function (isDebugMode, options) {
  Quantumart.QP8.Backend.initializeBase(this);
  this._isDebugMode = isDebugMode;

  if (options) {
    if (options.currentCustomerCode) {
      this._currentCustomerCode = options.currentCustomerCode;
    }

    if (options.currentUserId) {
      this._currentUserId = options.currentUserId;
    }

    if (options.directLinkOptions) {
      this._directLinkOptions = options.directLinkOptions;
    }

    if (options.autoLoadHome) {
      this._autoLoadHome = options.autoLoadHome;
    }
  }

  this._loadHandler = $.proxy(this._initialize, this);
  this._unloadHandler = $.proxy(this._dispose, this);
  this._errorHandler = $.proxy(this._error, this);

  $q.bindProxies.call(this, [
    '_onResizeSplitter',
    '_onDragStartSplitter',
    '_onDropSplitter',
    '_onEditingAreaEvent',
    '_onActionExecuting',
    '_onActionExecuted',
    '_onEntityReaded',
    '_onHostExternalCallerContextsUnbinded'
  ]);

  $(document).bind('click', function (e) {
    if (e.which == 2) {
 e.preventDefault(); 
}
  });

  $(window).bind('load', this._loadHandler);
  if (window.attachEvent) {
    $(window).bind('unload', this._unloadHandler);
  } else {
    window.onbeforeunload = this._unloadHandler;
  }
};

Quantumart.QP8.Backend.prototype = {
  _isDebugMode: false,
  _busy: false,
  _backendActionExecutor: null,
  _backendSplitter: null,
  _backendTreeMenu: null,
  _backendTreeMenuContextMenuManager: null,
  _backendContextMenuManager: null,
  _backendBreadCrumbsManager: null,
  _backendTabStrip: null,
  _backendEditingArea: null,
  _backendPopupWindowManager: null,
  _backendEntityGridManager: null,
  _backendEntityTreeManager: null,
  _backendEntityEditorManager: null,
  _backendLibraryManager: null,
  _backendEntityDataListManager: null,
  _backendActionPermissionViewManager: null,
  _backendCustomActionHostManager: null,

  _currentCustomerCode: null,
  _currentUserId: null,
  _autoLoadHome: null,
  _directLinkOptions: null,
  _directLinkExecutor: null,

  _entityEditorAutoSaver: null,

  _loadHandler: null,
  _unloadHandler: null,
  _errorHandler: null,
  _onResizeSplitterHandler: null,
  _onEditingAreaEventHandler: null,
  _onActionExecutingHandler: null,
  _onActionExecutedHandler: null,
  _onEntityReadedHandler: null,
  _onHostExternalCallerContextsUnbindedHandler: null,

  _initialize: function () {
    this._directLinkExecutor = new Quantumart.QP8.DirectLinkExecutor(this._currentCustomerCode, this._directLinkOptions);
    this._directLinkExecutor.ready(jQuery.proxy(function (openByDirectLink) {
      this._directLinkExecutor.attachObserver(window.EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING, this._onActionExecutingHandler);
      this._backendActionExecutor = Quantumart.QP8.BackendActionExecutor.getInstance();
      this._backendActionExecutor.attachObserver(window.EVENT_TYPE_BACKEND_ACTION_EXECUTED, this._onActionExecutedHandler);
      this._backendSplitter = new Quantumart.QP8.BackendSplitter('splitter', {
          firstPaneWidth: 270,
          minFirstPaneWidth: 50,
          maxFirstPaneWidth: 400,
          stateCookieName: 'leftMenuSize',
          toWindowResize: true
        });

      this._backendTreeMenuContextMenuManager = Quantumart.QP8.BackendTreeMenuContextMenuManager.getInstance();
      this._backendContextMenuManager = Quantumart.QP8.BackendContextMenuManager.getInstance();
      this._backendBreadCrumbsManager = Quantumart.QP8.BackendBreadCrumbsManager.getInstance('breadCrumbs');
      this._backendTreeMenu = new Quantumart.QP8.BackendTreeMenu('MainTreeMenu', {
        treeContainerElementId: 'tree',
        contextMenuManager: this._backendTreeMenuContextMenuManager
      });

      this._backendTreeMenu.initialize(3);
      this._backendTreeMenu.attachObserver(window.EVENT_TYPE_TREE_MENU_ACTION_EXECUTING, this._onActionExecutingHandler);

      this._backendTabStrip = Quantumart.QP8.BackendTabStrip.getInstance('MainTabStrip', { maxTabTextLength: 35, maxTabMenuItemTextLength: 33 });
      this._backendTabStrip.initialize();

      this._backendEditingArea = Quantumart.QP8.BackendEditingArea.getInstance('editingArea', {
        documentsContainerElementId: 'document',
        breadCrumbsContainerElementId: 'breadCrumbs',
        actionToolbarContainerElementId: 'actionToolbar',
        viewToolbarContainerElementId: 'viewToolbar',
        searchBlockContainerElementId: 'search',
        contextBlockContainerElementId: 'context',
        documentsContainerHeightDifference: 118,
        tabStrip: this._backendTabStrip,
        currentCustomerCode: this._currentCustomerCode,
        currentUserId: this._currentUserId
      });

      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING, this._onEditingAreaEventHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED, this._onEditingAreaEventHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING, this._onEditingAreaEventHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED, this._onEditingAreaEventHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR, this._onEditingAreaEventHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE, this._onEditingAreaEventHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_CLOSED, this._onEditingAreaEventHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_ACTION_EXECUTING, this._onActionExecutingHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_EDITING_AREA_ENTITY_READED, this._onEntityReadedHandler);
      this._backendEditingArea.attachObserver(window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, this._onHostExternalCallerContextsUnbindedHandler);

      this._backendPopupWindowManager = Quantumart.QP8.BackendPopupWindowManager.getInstance();
      this._backendPopupWindowManager.attachObserver(window.EVENT_TYPE_POPUP_WINDOW_ACTION_EXECUTING, this._onActionExecutingHandler);
      this._backendPopupWindowManager.attachObserver(window.EVENT_TYPE_POPUP_WINDOW_ENTITY_READED, this._onEntityReadedHandler);
      this._backendPopupWindowManager.attachObserver(window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, this._onHostExternalCallerContextsUnbindedHandler);

      this._backendEntityGridManager = Quantumart.QP8.BackendEntityGridManager.getInstance();
      this._backendEntityTreeManager = Quantumart.QP8.BackendEntityTreeManager.getInstance();

      this._backendEntityEditorManager = Quantumart.QP8.BackendEntityEditorManager.getInstance();
      this._backendEntityEditorManager.attachObserver(window.EVENT_TYPE_ENTITY_EDITOR_IS_READY, jQuery.proxy(this._onEntityEditorReady, this));
      this._backendEntityEditorManager.attachObserver(window.EVENT_TYPE_ENTITY_EDITOR_DISPOSED, jQuery.proxy(this._onEntityEditorDisposed, this));
      this._backendEntityEditorManager.attachObserver(window.EVENT_TYPE_ENTITY_EDITOR_FIELD_CHANGED, jQuery.proxy(this._onEntityEditorFieldChanged, this));
      this._backendEntityEditorManager.attachObserver(window.EVENT_TYPE_ENTITY_EDITOR_ALL_FIELD_INVALIDATE, jQuery.proxy(this._onEntityEditorAllFieldInvalidate, this));

      this._backendEntityDataListManager = Quantumart.QP8.BackendEntityDataListManager.getInstance();
      this._backendLibraryManager = Quantumart.QP8.BackendLibraryManager.getInstance();
      this._backendActionPermissionViewManager = Quantumart.QP8.BackendActionPermissionViewManager.getInstance();

      this._backendCustomActionHostManager = Quantumart.QP8.BackendCustomActionHostManager.getInstance();
      this._backendCustomActionHostManager.attachObserver(window.EVENT_TYPE_CLOSE_HOST_MESSAGE_RECEIVED, jQuery.proxy(this._onCloseHostMessageReceived, this));

      this._backendSplitter.attachObserver(window.EVENT_TYPE_SPLITTER_RESIZED, this._onResizeSplitterHandler);
      this._backendSplitter.attachObserver(window.EVENT_TYPE_SPLITTER_INITIALIZED, this._onResizeSplitterHandler);
      this._backendSplitter.attachObserver(window.EVENT_TYPE_SPLITTER_DRAG_START, this._onDragStartSplitterHandler);
      this._backendSplitter.attachObserver(window.EVENT_TYPE_SPLITTER_DROP, this._onDropSplitterHandler);

      this._backendSplitter.initialize();

      if (this._autoLoadHome) {
        this._loadHome();
      }

      this._entityEditorAutoSaver = new Quantumart.QP8.EntityEditorAutoSaver({
        currentCustomerCode: this._currentCustomerCode,
        currentUserId: this._currentUserId
      });

      this._entityEditorAutoSaver.attachObserver(window.EVENT_TYPE_AUTO_SAVER_ACTION_EXECUTING, this._onActionExecutingHandler);
      if (openByDirectLink) {
        this._entityEditorAutoSaver.start();
      } else {
        this._entityEditorAutoSaver.restore();
      }

      this._initializeSignOut();
    }, this));

    this._initializeSignalrHubs();
  },

  _error: function () { },

  _initializeSignOut: function () {
    $('.signOut').bind('click', function () {
      Quantumart.QP8.BackendLogin.removeCustomerCode();
    });
  },

  _terminateSignOut: function () {
    $('.signOut').unbind('click');
  },

  _initializeSignalrHubs: function () {
    const that = this;
    $.connection.hub.logging = false;
    $.connection.communication.client.send = function (key, data) {
      if (key === 'singleusermode') {
        that._updateSingleUserMode(data);
      } else {
        $('.' + key).text(data);
      }
    };

    $.connection.singleUserMode.client.send = this._updateSingleUserMode;
    $.connection.hub.start().done(function () {
      var hash = $('body').data('dbhash');
      $.connection.communication.server.addHash(hash);
    });
  },

  _updateSingleUserMode: function (data) {
    var $elem = $('.singleusermode');
    var userId = $('.userName').data('userid');
    var message = '';

    if (data == null) {
      $('form :input').prop('disabled', false);
      $elem.hide();
    } else {
      if (data.userId == userId) {
        message = $l.Communacation.singleUserModeMessage;
        $elem.addClass('info');
        $elem.removeClass('warning');
      } else {
        message = $l.Communacation.singleUserModeMessageFor + data.userName;
        $elem.addClass('warning');
        $elem.removeClass('info');
      }

      $elem.show();
    }

    $elem.text(message);
  },

  _markAsBusy: function () {
    this._busy = true;
    if (this._backendTreeMenu) {
      this._backendTreeMenu.markTreeAsBusy();
    }

    if (this._backendEditingArea) {
      this._backendEditingArea.markAsBusy();
    }
  },

  _unmarkAsBusy: function () {
    this._busy = false;
    if (this._backendTreeMenu) {
      this._backendTreeMenu.unmarkTreeAsBusy();
    }

    if (this._backendEditingArea) {
      this._backendEditingArea.unmarkAsBusy();
    }
  },

  _onResizeSplitter: function (eventType, sender, eventArgs) {
    this._backendTreeMenu.fixTreeHeight(eventArgs.get_firstPaneHeight());
    this._backendEditingArea.get_tabStrip().fixTabStripWidth(eventArgs.get_firstPaneWidth());

    var selDoc = this._backendEditingArea.getSelectedDocument();
    if (selDoc) {
      selDoc.fixActionToolbarWidth();
    }
  },

  _onDragStartSplitter: function () {
    if (!this._busy) {
      this._backendEditingArea.showAjaxLoadingLayer();
    }
  },

  _onDropSplitter: function () {
    if (!this._busy) {
      this._backendEditingArea.hideAjaxLoadingLayer();
    }
  },

  _onEditingAreaEvent: function (eventType, sender, eventArgs) {
    if (eventType == EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING) {
      this._backendTreeMenu.markTreeAsBusy();
    } else if (eventType == EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED) {
      this._backendTreeMenu.unmarkTreeAsBusy();
      if (eventArgs.get_isSelected()) {
        this._backendTreeMenu.highlightNodeWithEventArgs(eventArgs);
      }
    } else if (eventType == EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE) {
      this._backendTreeMenu.unmarkTreeAsBusy();
      this._backendTreeMenu.highlightNodeWithEventArgs(eventArgs);
    } else if (eventType == EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING) {
      this._backendTreeMenu.markTreeAsBusy();
    } else if (eventType == EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED) {
      this._backendTreeMenu.unmarkTreeAsBusy();
    } else if (eventType == EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR) {
      this._backendTreeMenu.unmarkTreeAsBusy();
    } else if (eventType == EVENT_TYPE_EDITING_AREA_CLOSED) {
      this._backendTreeMenu.unhighlightAllNodes();
    }
  },

  _isMultistep: function (action, eventArgs) {
    var entities = eventArgs.get_entities();
    var limit = 1;
    if (action.EntityLimit) {
      limit = action.EntityLimit;
    }

    return action.IsMultistep || (action.AdditionalControllerActionUrl && entities.length > limit);
  },

  _onActionExecuting: function (eventType, sender, eventArgs) {
    var status = this._backendActionExecutor.executeSpecialAction(eventArgs),
    that = this;

    if (status == BACKEND_ACTION_EXECUTION_STATUS_NOT_STARTING) {
      var actionCode = eventArgs.get_actionCode();
      var action = $a.getSelectedAction(actionCode);

      if ($q.isObject(action)) {
        if (action.IsInterface) {
          if (eventArgs.get_isWindow()) {
            this._backendPopupWindowManager.openPopupWindow(eventArgs);
          } else {
            this._backendEditingArea.addDocument(eventArgs);
          }
        } else if (this._isMultistep(action, eventArgs)) {
          jQuery.when(this._backendActionExecutor.executeMultistepAction(eventArgs)).done(function (status) {
            if (status == BACKEND_ACTION_EXECUTION_STATUS_SUCCESS) {
              that._onActionExecuted(eventArgs);
            }
          }).fail(function () { });
        } else {
          this._markAsBusy();
          var callback = jQuery.proxy(function (status, eventArgs) {
            if (status == BACKEND_ACTION_EXECUTION_STATUS_SUCCESS) {
              this._onActionExecuted(eventArgs);
            }

            this._unmarkAsBusy();
          }, this);

          this._backendActionExecutor.executeNonInterfaceAction(eventArgs, callback);
        }
      }
    } else if (status == BACKEND_ACTION_EXECUTION_STATUS_SUCCESS) {
 this._onActionExecuted(eventArgs); 
}
  },

  _onActionExecuted: function (eventArgs) {
    this._backendCustomActionHostManager.onActionExecuted(eventArgs);
    this._backendBreadCrumbsManager.onActionExecuted(eventArgs);
    this._backendTreeMenu.onActionExecuted(eventArgs);
    this._backendEditingArea.onActionExecuted(eventArgs);
    this._backendPopupWindowManager.onActionExecuted(eventArgs);
    this._backendEntityGridManager.onActionExecuted(eventArgs);
    this._backendEntityDataListManager.onActionExecuted(eventArgs);
    this._backendLibraryManager.onActionExecuted(eventArgs);
    this._backendEntityEditorManager.onActionExecuted(eventArgs);
    this._backendEntityTreeManager.onActionExecuted(eventArgs);
    this._backendContextMenuManager.onActionExecuted(eventArgs);
    this._backendActionPermissionViewManager.onActionExecuted(eventArgs);

    if (eventArgs.get_isUpdated() && eventArgs.get_entityTypeCode() == ENTITY_TYPE_CODE_CUSTOM_ACTION) {
 $cache.clear(); 
}
  },

  _onEntityReaded: function (eventType, sender, eventArgs) {
    this._onActionExecuted(eventArgs);
  },

  _unlockAllEntities: function () {
    $q.postDataToUrl(CONTROLLER_URL_ENTITY_OBJECT + 'UnlockAllEntities', null, false);
  },

  _onCloseHostMessageReceived: function (eventType, sender, message) {
    this._backendEditingArea.onCloseHostMessageReceived(message);
  },

  _onHostExternalCallerContextsUnbinded: function (eventType, sender, message) {
    Quantumart.QP8.BackendCustomActionHostManager.getInstance().onExternalCallerContextsUnbinded(message);
  },

  // #region EntityEditor Event Handlers
  _onEntityEditorReady: function (eventType, sender, eventArgs) {
    this._entityEditorAutoSaver.onEntityEditorReady(eventArgs.documentWrapperElementId);
  },

  _onEntityEditorDisposed: function (eventType, sender, eventArgs) {
    this._entityEditorAutoSaver.onEntityEditorDisposed(eventArgs.documentWrapperElementId);
  },

  _onEntityEditorFieldChanged: function (eventType, sender, eventArgs) {
    this._entityEditorAutoSaver.onFieldChanged(eventArgs.fieldChangeInfo);
  },

  _onEntityEditorAllFieldInvalidate: function (eventType, sender, eventArgs) {
    this._entityEditorAutoSaver.onAllFieldInvalidate(eventArgs.documentWrapperElementId);
  },

  // #endregion

  checkOpenDocumentByEventArgs: function (eventArgs) {
    var tabsForEntity = this._backendTabStrip.getTabsByEventArgs(eventArgs);
    var entityIsOpenedInTab = tabsForEntity && tabsForEntity.length > 0;

    var windowsForEntity = this._backendPopupWindowManager.getPopupWindowByEventArgs(eventArgs);
    var entityIsOpenedInWindow = windowsForEntity && windowsForEntity.length > 0;

    if (entityIsOpenedInTab) {
      return String.format($l.Action.selectedDocumentIsOpenedInTab, eventArgs.get_entityId());
    } else if (entityIsOpenedInWindow) {
      return String.format($l.Action.selectedDocumentIsOpenedInWindow, eventArgs.get_entityId());
    } else {
      return '';
    }
  },

  _loadHome: function () {
    var action = $a.getBackendActionByCode('home');
    var params = new Quantumart.QP8.BackendActionParameters({
      entityTypeCode: 'db',
      entityId: 1,
      entityName: this._currentCustomerCode,
      parentEntityId: 0
    });

    var eventArgs = $a.getEventArgsFromActionWithParams(action, params);

    this._onActionExecuting(null, this, eventArgs);
  },

  _dispose: function () {
    try {
      this._unlockAllEntities();

      if (this._entityEditorAutoSaver) {
        this._entityEditorAutoSaver.detachObserver(EVENT_TYPE_AUTO_SAVER_ACTION_EXECUTING);
        this._entityEditorAutoSaver.dispose();
        this._entityEditorAutoSaver = null;
      }

      if (this._backendPopupWindowManager) {
        this._backendPopupWindowManager.detachObserver(EVENT_TYPE_POPUP_WINDOW_ACTION_EXECUTING, this._onActionExecutingHandler);
        this._backendPopupWindowManager.detachObserver(EVENT_TYPE_POPUP_WINDOW_ENTITY_READED, this._onEntityReadedHandler);
        this._backendPopupWindowManager.detachObserver(EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, this._onHostExternalCallerContextsUnbindedHandler);
        this._backendPopupWindowManager.dispose();
        this._backendPopupWindowManager = null;
      }

      if (this._backendEntityDataListManager) {
        this._backendEntityDataListManager.dispose();
        this._backendEntityDataListManager = null;
      }

      if (this._backendEntityEditorManager) {
        this._backendEntityEditorManager.detachObserver(EVENT_TYPE_ENTITY_EDITOR_IS_READY);
        this._backendEntityEditorManager.detachObserver(EVENT_TYPE_ENTITY_EDITOR_DISPOSED);
        this._backendEntityEditorManager.detachObserver(EVENT_TYPE_ENTITY_EDITOR_FIELD_CHANGED);
        this._backendEntityEditorManager.detachObserver(EVENT_TYPE_ENTITY_EDITOR_ALL_FIELD_INVALIDATE);
        this._backendEntityEditorManager.dispose();
        this._backendEntityEditorManager = null;
      }

      if (this._backendEntityTreeManager) {
        this._backendEntityTreeManager.dispose();
        this._backendEntityTreeManager = null;
      }

      if (this._backendEntityGridManager) {
        this._backendEntityGridManager.dispose();
        this._backendEntityGridManager = null;
      }

      if (this._backendActionPermissionViewManager) {
        this._backendActionPermissionViewManager.dispose();
        this._backendActionPermissionViewManager = null;
      }

      if (this._backendEditingArea) {
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING, this._onEditingAreaEventHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED, this._onEditingAreaEventHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING, this._onEditingAreaEventHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED, this._onEditingAreaEventHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR, this._onEditingAreaEventHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE, this._onEditingAreaEventHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_CLOSED, this._onEditingAreaEventHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_ACTION_EXECUTING, this._onActionExecutingHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_EDITING_AREA_ENTITY_READED, this._onEntityReadedHandler);
        this._backendEditingArea.detachObserver(EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, this._onHostExternalCallerContextsUnbindedHandler);
        this._backendEditingArea.dispose();
        this._backendEditingArea = null;
      }

      if (this._backendTabStrip) {
        this._backendTabStrip.dispose();
        this._backendTabStrip = null;
      }

      if (this._backendTreeMenu) {
        this._backendTreeMenu.detachObserver(EVENT_TYPE_TREE_MENU_ACTION_EXECUTING, this._onActionExecutingHandler);
        this._backendTreeMenu.dispose();
        this._backendTreeMenu = null;
      }

      if (this._backendBreadCrumbsManager) {
        this._backendBreadCrumbsManager.dispose();
        this._backendBreadCrumbsManager = null;
      }

      if (this._backendTreeMenuContextMenuManager) {
        this._backendTreeMenuContextMenuManager.dispose();
        this._backendTreeMenuContextMenuManager = null;
      }

      if (this._backendContextMenuManager) {
        this._backendContextMenuManager.dispose();
        this._backendContextMenuManager = null;
      }

      if (this._backendCustomActionHostManager) {
        this._backendCustomActionHostManager.detachObserver(EVENT_TYPE_CLOSE_HOST_MESSAGE_RECEIVED);
        this._backendCustomActionHostManager.dispose();
        this._backendCustomActionHostManager = null;
      }

      if (this._backendSplitter) {
        this._backendSplitter.detachObserver(EVENT_TYPE_SPLITTER_RESIZED, this._onResizeSplitterHandler);
        this._backendSplitter.detachObserver(EVENT_TYPE_SPLITTER_INITIALIZED, this._onResizeSplitterHandler);
        this._backendSplitter.detachObserver(EVENT_TYPE_SPLITTER_DRAG_START, this._onDragStartSplitterHandler);
        this._backendSplitter.detachObserver(EVENT_TYPE_SPLITTER_DROP, this._onDropSplitterHandler);
        this._backendSplitter.dispose();
        this._backendSplitter = null;
      }

      if (this._backendActionExecutor) {
        this._backendActionExecutor.detachObserver(EVENT_TYPE_BACKEND_ACTION_EXECUTED, this._onActionExecutedHandler);
        this._backendActionExecutor.dispose();
        this._backendActionExecutor = null;
      }

      this._terminateSignOut();
      $(document).unbind('click');
      $(window).unbind('load', this._loadHandler);

      if (window.attachEvent) {
        $(window).unbind('unload', this._unloadHandler);
      } else {
        window.onbeforeunload = null;
      }

      $cache.dispose();
      this._loadHandler = null;
      this._unloadHandler = null;
      this._errorHandler = null;
      this._onResizeSplitterHandler = null;
      this._onDragStartSplitterHandler = null;
      this._onDropSplitterHandler = null;
      this._onEditingAreaEventHandler = null;
      this._onActionExecutingHandler = null;
      this._onActionExecutedHandler = null;
      this._onEntityReadedHandler = null;
    } finally {
      if (this._directLinkExecutor) {
        this._directLinkExecutor.detachObserver(EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING);
        this._directLinkExecutor.dispose();
      }
    }
  }
};

Quantumart.QP8.Backend._instance = null;

Quantumart.QP8.Backend.getInstance = function (isDebugMode, options) {
  var instance = Quantumart.QP8.Backend._instance;

  if (instance == null) {
    instance = new Quantumart.QP8.Backend(isDebugMode, options);
    Quantumart.QP8.Backend._instance = instance;
  }

  return instance;
};

Quantumart.QP8.Backend.registerClass('Quantumart.QP8.Backend');

// #endregion
