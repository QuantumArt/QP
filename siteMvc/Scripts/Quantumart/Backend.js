/* eslint max-lines: 'off' */
import { BackendActionPermissionViewManager } from './Managers/BackendActionPermissionViewManager';
import { BackendBreadCrumbsManager } from './Managers/BackendBreadCrumbsManager';
import { BackendBrowserHistoryManager } from './Managers/BackendBrowserHistoryManager';
import { BackendContextMenuManager } from './Managers/BackendContextMenuManager';
import { BackendCustomActionHostManager } from './Managers/BackendCustomActionHostManager';
import { BackendEditingArea } from './Document/BackendEditingArea';
import { BackendEntityDataListManager } from './Managers/BackendEntityDataListManager';
import { BackendEntityEditorManager } from './Managers/BackendEntityEditorManager';
import { BackendEntityGridManager } from './Managers/BackendEntityGridManager';
import { BackendEntityTreeManager } from './Managers/BackendEntityTreeManager';
import { BackendLibraryManager } from './Managers/BackendLibraryManager';
import { BackendLogin } from './BackendLogin';
import { BackendPopupWindowManager } from './Managers/BackendPopupWindowManager';
import { BackendSplitter } from './BackendSplitter';
import { BackendTabStrip } from './BackendTabStrip';
import { BackendTreeMenu } from './Tree/BackendTreeMenu';
import { BackendTreeMenuContextMenuManager } from './Managers/BackendTreeMenuContextMenuManager';
import { DirectLinkExecutor } from './BackendDirectLinkExecutor';
import { EntityEditorAutoSaver } from './Editor/BackendEditorsAutoSaver';
import { BackendChangePasswordWindow } from './BackendChangePasswordWindow';
import { GlobalCache } from './Cache';
import { $a, BackendActionExecutor, BackendActionParameters } from './BackendActionExecutor';
import { $q } from './Utils';


export class Backend {
  constructor(isDebugMode, options) {
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

      if (options.mustChangePassword) {
        this._userMustChangePassword = options.mustChangePassword;
      }
    }

    this._loadHandler = $.proxy(this._initialize, this);
    this._unloadHandler = $.proxy(this._dispose, this);
    this._errorHandler = $.proxy(this._error, this);

    this._onResizeSplitterHandler = this._onResizeSplitter.bind(this);
    this._onDragStartSplitterHandler = this._onDragStartSplitter.bind(this);
    this._onDropSplitterHandler = this._onDropSplitter.bind(this);
    this._onEditingAreaEventHandler = this._onEditingAreaEvent.bind(this);
    this._onActionExecutingHandler = this._onActionExecuting.bind(this);
    this._onActionExecutedHandler = this._onActionExecuted.bind(this);
    this._onEntityReadedHandler = this._onEntityReaded.bind(this);
    this._onHostExternalCallerContextsUnbindedHandler = this._onHostExternalCallerContextsUnbinded.bind(this);

    $(document).bind('click', e => {
      if (e.which === 2) {
        e.preventDefault();
      }
    });

    $(window).bind('load', this._loadHandler);
    if (window.attachEvent) {
      $(window).bind('unload', this._unloadHandler);
    } else {
      window.onbeforeunload = this._unloadHandler;
    }
  }

  _isDebugMode = false;
  _busy = false;
  _backendActionExecutor = null;
  _backendSplitter = null;
  _backendTreeMenu = null;
  _backendTreeMenuContextMenuManager = null;
  _backendContextMenuManager = null;
  _backendBreadCrumbsManager = null;
  _backendTabStrip = null;
  _backendEditingArea = null;
  _backendPopupWindowManager = null;
  _backendEntityGridManager = null;
  _backendEntityTreeManager = null;
  _backendEntityEditorManager = null;
  _backendLibraryManager = null;
  _backendEntityDataListManager = null;
  _backendActionPermissionViewManager = null;
  _backendCustomActionHostManager = null;

  _currentCustomerCode = null;
  _currentUserId = null;
  _autoLoadHome = null;
  _directLinkOptions = null;
  _directLinkExecutor = null;

  _entityEditorAutoSaver = null;

  _loadHandler = null;
  _unloadHandler = null;
  _errorHandler = null;
  _onResizeSplitterHandler = null;
  _onEditingAreaEventHandler = null;
  _onActionExecutingHandler = null;
  _onActionExecutedHandler = null;
  _onEntityReadedHandler = null;
  _onHostExternalCallerContextsUnbindedHandler = null;
  _userMustChangePassword = false;

  _initialize() {
    BackendBrowserHistoryManager.preventBrowserNavigateBack();

    if (this._userMustChangePassword) {
      const changingWindow = new BackendChangePasswordWindow();
      changingWindow._changePassword();
    }
    this._directLinkExecutor = new DirectLinkExecutor(
      this._currentCustomerCode, this._directLinkOptions
    );

    // eslint-disable-next-line max-statements
    this._directLinkExecutor.ready($.proxy(function (openByDirectLink) {
      this._directLinkExecutor.attachObserver(
        window.EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING, this._onActionExecutingHandler
      );
      this._backendActionExecutor = BackendActionExecutor.getInstance();
      this._backendActionExecutor.attachObserver(
        window.EVENT_TYPE_BACKEND_ACTION_EXECUTED, this._onActionExecutedHandler
      );

      this._backendSplitter = new BackendSplitter('splitter', {
        firstPaneWidth: 250,
        minFirstPaneWidth: 50,
        maxFirstPaneWidth: 400,
        stateCookieName: 'leftMenuSize',
        toWindowResize: true
      });

      this._backendTreeMenuContextMenuManager = BackendTreeMenuContextMenuManager.getInstance();
      this._backendContextMenuManager = BackendContextMenuManager.getInstance();
      this._backendBreadCrumbsManager = BackendBreadCrumbsManager.getInstance('breadCrumbs');
      this._backendTreeMenu = new BackendTreeMenu('MainTreeMenu', {
        treeContainerElementId: 'tree',
        contextMenuManager: this._backendTreeMenuContextMenuManager
      });

      this._backendTreeMenu.initialize(3);
      this._backendTreeMenu.attachObserver(
        window.EVENT_TYPE_TREE_MENU_ACTION_EXECUTING, this._onActionExecutingHandler
      );

      this._backendTabStrip = BackendTabStrip.getInstance(
        'MainTabStrip', { maxTabTextLength: 35, maxTabMenuItemTextLength: 33 }
      );
      this._backendTabStrip.initialize();

      this._backendEditingArea = BackendEditingArea.getInstance('editingArea', {
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

      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING, this._onEditingAreaEventHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED, this._onEditingAreaEventHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING, this._onEditingAreaEventHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED, this._onEditingAreaEventHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR, this._onEditingAreaEventHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE, this._onEditingAreaEventHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_CLOSED, this._onEditingAreaEventHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_ACTION_EXECUTING, this._onActionExecutingHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_EDITING_AREA_ENTITY_READED, this._onEntityReadedHandler
      );
      this._backendEditingArea.attachObserver(
        window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, this._onHostExternalCallerContextsUnbindedHandler
      );

      this._backendPopupWindowManager = BackendPopupWindowManager.getInstance();
      this._backendPopupWindowManager.attachObserver(
        window.EVENT_TYPE_POPUP_WINDOW_ACTION_EXECUTING, this._onActionExecutingHandler
      );
      this._backendPopupWindowManager.attachObserver(
        window.EVENT_TYPE_POPUP_WINDOW_ENTITY_READED, this._onEntityReadedHandler
      );
      this._backendPopupWindowManager.attachObserver(
        window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, this._onHostExternalCallerContextsUnbindedHandler);

      this._backendEntityGridManager = BackendEntityGridManager.getInstance();
      this._backendEntityTreeManager = BackendEntityTreeManager.getInstance();

      this._backendEntityEditorManager = BackendEntityEditorManager.getInstance();
      this._backendEntityEditorManager.attachObserver(
        window.EVENT_TYPE_ENTITY_EDITOR_IS_READY, $.proxy(this._onEntityEditorReady, this)
      );
      this._backendEntityEditorManager.attachObserver(
        window.EVENT_TYPE_ENTITY_EDITOR_DISPOSED, $.proxy(this._onEntityEditorDisposed, this)
      );
      this._backendEntityEditorManager.attachObserver(
        window.EVENT_TYPE_ENTITY_EDITOR_FIELD_CHANGED, $.proxy(this._onEntityEditorFieldChanged, this)
      );
      this._backendEntityEditorManager.attachObserver(
        window.EVENT_TYPE_ENTITY_EDITOR_ALL_FIELD_INVALIDATE, $.proxy(this._onEntityEditorAllFieldInvalidate, this)
      );

      this._backendEntityDataListManager = BackendEntityDataListManager.getInstance();
      this._backendLibraryManager = BackendLibraryManager.getInstance();
      this._backendActionPermissionViewManager = BackendActionPermissionViewManager.getInstance();

      this._backendCustomActionHostManager = BackendCustomActionHostManager.getInstance();
      this._backendCustomActionHostManager.attachObserver(
        window.EVENT_TYPE_CLOSE_HOST_MESSAGE_RECEIVED, $.proxy(this._onCloseHostMessageReceived, this)
      );

      this._backendSplitter.attachObserver(window.EVENT_TYPE_SPLITTER_RESIZED, this._onResizeSplitterHandler);
      this._backendSplitter.attachObserver(window.EVENT_TYPE_SPLITTER_INITIALIZED, this._onResizeSplitterHandler);
      this._backendSplitter.attachObserver(window.EVENT_TYPE_SPLITTER_DRAG_START, this._onDragStartSplitterHandler);
      this._backendSplitter.attachObserver(window.EVENT_TYPE_SPLITTER_DROP, this._onDropSplitterHandler);

      this._backendSplitter.initialize();

      if (this._autoLoadHome) {
        this._loadHome();
      }

      this._entityEditorAutoSaver = new EntityEditorAutoSaver({
        currentCustomerCode: this._currentCustomerCode,
        currentUserId: this._currentUserId
      });

      this._entityEditorAutoSaver.attachObserver(
        window.EVENT_TYPE_AUTO_SAVER_ACTION_EXECUTING, this._onActionExecutingHandler
      );
      if (openByDirectLink) {
        this._entityEditorAutoSaver.start();
      } else {
        this._entityEditorAutoSaver.restore();
      }

      this._initializeSignOut();
    }, this));

    this._initializeSignalrHubs();
  }

  _error() {
    // empty callback
  }

  _initializeSignOut() {
    $('.signOut').bind('click', () => {
      BackendLogin.removeCustomerCode();
    });
  }

  _terminateSignOut() {
    $('.signOut').unbind('click');
  }

  _initializeSignalrHubs() {
    const that = this;
    $.connection.hub.logging = false;
    $.connection.communication.client.send = function (key, data) {
      if (key === 'singleusermode') {
        that._updateSingleUserMode(data);
      } else {
        $(`.${key}`).text(data);
      }
    };

    $.connection.singleUserMode.client.send = this._updateSingleUserMode;
    $.connection.hub.start().done(() => {
      const hash = $('body').data('dbhash');
      $.connection.communication.server.addHash(hash);
    });
  }

  _updateSingleUserMode(data) {
    const $elem = $('.singleusermode');
    const userId = $('.userName').data('userid');
    let message = '';

    if ($q.isNull(data)) {
      $('form :input').prop('disabled', false);
      $elem.hide();
    } else {
      if (data.userId === userId) {
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
  }

  _markAsBusy() {
    this._busy = true;
    if (this._backendTreeMenu) {
      this._backendTreeMenu.markTreeAsBusy();
    }

    if (this._backendEditingArea) {
      this._backendEditingArea.markAsBusy();
    }
  }

  _unmarkAsBusy() {
    this._busy = false;
    if (this._backendTreeMenu) {
      this._backendTreeMenu.unmarkTreeAsBusy();
    }

    if (this._backendEditingArea) {
      this._backendEditingArea.unmarkAsBusy();
    }
  }

  _onResizeSplitter(eventType, sender, eventArgs) {
    this._backendTreeMenu.fixTreeHeight(eventArgs.getFirstPaneHeight());
    this._backendEditingArea.get_tabStrip().fixTabStripWidth(eventArgs.getFirstPaneWidth());

    const selDoc = this._backendEditingArea.getSelectedDocument();
    if (selDoc) {
      selDoc.fixActionToolbarWidth();
    }
  }

  _onDragStartSplitter() {
    if (!this._busy) {
      this._backendEditingArea.showAjaxLoadingLayer();
    }
  }

  _onDropSplitter() {
    if (!this._busy) {
      this._backendEditingArea.hideAjaxLoadingLayer();
    }
  }

  _onEditingAreaEvent(eventType, sender, eventArgs) {
    if (eventType === window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING) {
      this._backendTreeMenu.markTreeAsBusy();
    } else if (eventType === window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED) {
      this._backendTreeMenu.unmarkTreeAsBusy();
      if (eventArgs.get_isSelected()) {
        this._backendTreeMenu.highlightNodeWithEventArgs(eventArgs);
      }
    } else if (eventType === window.EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE) {
      this._backendTreeMenu.unmarkTreeAsBusy();
      this._backendTreeMenu.highlightNodeWithEventArgs(eventArgs);
    } else if (eventType === window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING) {
      this._backendTreeMenu.markTreeAsBusy();
    } else if (eventType === window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED) {
      this._backendTreeMenu.unmarkTreeAsBusy();
    } else if (eventType === window.EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR) {
      this._backendTreeMenu.unmarkTreeAsBusy();
    } else if (eventType === window.EVENT_TYPE_EDITING_AREA_CLOSED) {
      this._backendTreeMenu.unhighlightAllNodes();
    }
  }

  _isMultistep(action, eventArgs) {
    const entities = eventArgs.get_entities();
    let limit = 1;
    if (action.EntityLimit) {
      limit = action.EntityLimit;
    }

    return action.IsMultistep || (action.AdditionalControllerActionUrl && entities.length > limit);
  }

  _onActionExecuting(eventType, sender, eventArgs) {
    const actionStatus = this._backendActionExecutor.executeSpecialAction(eventArgs);
    const that = this;

    if (actionStatus === window.BACKEND_ACTION_EXECUTION_STATUS_NOT_STARTING) {
      const actionCode = eventArgs.get_actionCode();
      const action = $a.getSelectedAction(actionCode);

      if ($q.isObject(action)) {
        if (action.IsInterface) {
          if (eventArgs.get_isWindow()) {
            this._backendPopupWindowManager.openPopupWindow(eventArgs);
          } else {
            this._backendEditingArea.addDocument(eventArgs);
          }
        } else if (this._isMultistep(action, eventArgs)) {
          $.when(this._backendActionExecutor.executeMultistepAction(eventArgs)).done(st => {
            if (st === window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS) {
              that._onActionExecuted(eventArgs);
            }
          }).fail(() => {
            // empty callback
          });
        } else {
          this._markAsBusy();
          const callback = $.proxy(function (st, args) {
            if (st === window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS) {
              this._onActionExecuted(args);
            }

            this._unmarkAsBusy();
          }, this);

          this._backendActionExecutor.executeNonInterfaceAction(eventArgs, callback);
        }
      }
    } else if (actionStatus === window.BACKEND_ACTION_EXECUTION_STATUS_SUCCESS) {
      this._onActionExecuted(eventArgs);
    }
  }

  _onActionExecuted(eventArgs) {
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

    if (eventArgs.get_isUpdated() && eventArgs.get_entityTypeCode() === window.ENTITY_TYPE_CODE_CUSTOM_ACTION) {
      GlobalCache.clear();
    }
  }

  _onEntityReaded(eventType, sender, eventArgs) {
    this._onActionExecuted(eventArgs);
  }

  _unlockAllEntities() {
    $q.postDataToUrl(`${window.CONTROLLER_URL_ENTITY_OBJECT}UnlockAllEntities`, null, false);
  }

  _onCloseHostMessageReceived(eventType, sender, message) {
    this._backendEditingArea.onCloseHostMessageReceived(message);
  }

  _onHostExternalCallerContextsUnbinded(eventType, sender, message) {
    BackendCustomActionHostManager.getInstance().onExternalCallerContextsUnbinded(message);
  }

  _onEntityEditorReady(eventType, sender, eventArgs) {
    this._entityEditorAutoSaver.onEntityEditorReady(eventArgs.documentWrapperElementId);
  }

  _onEntityEditorDisposed(eventType, sender, eventArgs) {
    this._entityEditorAutoSaver.onEntityEditorDisposed(eventArgs.documentWrapperElementId);
  }

  _onEntityEditorFieldChanged(eventType, sender, eventArgs) {
    this._entityEditorAutoSaver.onFieldChanged(eventArgs.fieldChangeInfo);
  }

  _onEntityEditorAllFieldInvalidate(eventType, sender, eventArgs) {
    this._entityEditorAutoSaver.onAllFieldInvalidate(eventArgs.documentWrapperElementId);
  }

  checkOpenDocumentByEventArgs(eventArgs) {
    const tabsForEntity = this._backendTabStrip.getTabsByEventArgs(eventArgs);
    const entityIsOpenedInTab = tabsForEntity && tabsForEntity.length > 0;

    const windowsForEntity = this._backendPopupWindowManager.getPopupWindowByEventArgs(eventArgs);
    const entityIsOpenedInWindow = windowsForEntity && windowsForEntity.length > 0;

    if (entityIsOpenedInTab) {
      return String.format($l.Action.selectedDocumentIsOpenedInTab, eventArgs.get_entityId());
    } else if (entityIsOpenedInWindow) {
      return String.format($l.Action.selectedDocumentIsOpenedInWindow, eventArgs.get_entityId());
    }

    return '';
  }

  _loadHome() {
    const action = $a.getBackendActionByCode('home');
    const params = new BackendActionParameters({
      entityTypeCode: 'db',
      entityId: 1,
      entityName: this._currentCustomerCode,
      parentEntityId: 0
    });

    const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
    this._onActionExecuting(null, this, eventArgs);
  }

  // eslint-disable-next-line max-statements
  _dispose() {
    try {
      this._unlockAllEntities();

      if (this._entityEditorAutoSaver) {
        this._entityEditorAutoSaver.detachObserver(window.EVENT_TYPE_AUTO_SAVER_ACTION_EXECUTING);
        this._entityEditorAutoSaver.dispose();
        this._entityEditorAutoSaver = null;
      }

      if (this._backendPopupWindowManager) {
        this._backendPopupWindowManager.detachObserver(
          window.EVENT_TYPE_POPUP_WINDOW_ACTION_EXECUTING, this._onActionExecutingHandler
        );
        this._backendPopupWindowManager.detachObserver(
          window.EVENT_TYPE_POPUP_WINDOW_ENTITY_READED, this._onEntityReadedHandler
        );
        this._backendPopupWindowManager.detachObserver(
          window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, this._onHostExternalCallerContextsUnbindedHandler
        );
        this._backendPopupWindowManager.dispose();
        this._backendPopupWindowManager = null;
      }

      if (this._backendEntityDataListManager) {
        this._backendEntityDataListManager.dispose();
        this._backendEntityDataListManager = null;
      }

      if (this._backendEntityEditorManager) {
        this._backendEntityEditorManager.detachObserver(window.EVENT_TYPE_ENTITY_EDITOR_IS_READY);
        this._backendEntityEditorManager.detachObserver(window.EVENT_TYPE_ENTITY_EDITOR_DISPOSED);
        this._backendEntityEditorManager.detachObserver(window.EVENT_TYPE_ENTITY_EDITOR_FIELD_CHANGED);
        this._backendEntityEditorManager.detachObserver(window.EVENT_TYPE_ENTITY_EDITOR_ALL_FIELD_INVALIDATE);
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
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADING, this._onEditingAreaEventHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_DOCUMENT_LOADED, this._onEditingAreaEventHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSING, this._onEditingAreaEventHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_DOCUMENT_CLOSED, this._onEditingAreaEventHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_DOCUMENT_ERROR, this._onEditingAreaEventHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_FIND_TAB_IN_TREE, this._onEditingAreaEventHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_CLOSED, this._onEditingAreaEventHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_ACTION_EXECUTING, this._onActionExecutingHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_EDITING_AREA_ENTITY_READED, this._onEntityReadedHandler
        );
        this._backendEditingArea.detachObserver(
          window.EVENT_TYPE_HOST_EXTERNAL_CALLER_CONTEXTS_UNBINDED, this._onHostExternalCallerContextsUnbindedHandler
        );
        this._backendEditingArea.dispose();
        this._backendEditingArea = null;
      }

      if (this._backendTabStrip) {
        this._backendTabStrip.dispose();
        this._backendTabStrip = null;
      }

      if (this._backendTreeMenu) {
        this._backendTreeMenu.detachObserver(
          window.EVENT_TYPE_TREE_MENU_ACTION_EXECUTING, this._onActionExecutingHandler
        );
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
        this._backendCustomActionHostManager.detachObserver(window.EVENT_TYPE_CLOSE_HOST_MESSAGE_RECEIVED);
        this._backendCustomActionHostManager.dispose();
        this._backendCustomActionHostManager = null;
      }

      if (this._backendSplitter) {
        this._backendSplitter.detachObserver(window.EVENT_TYPE_SPLITTER_RESIZED, this._onResizeSplitterHandler);
        this._backendSplitter.detachObserver(window.EVENT_TYPE_SPLITTER_INITIALIZED, this._onResizeSplitterHandler);
        this._backendSplitter.detachObserver(window.EVENT_TYPE_SPLITTER_DRAG_START, this._onDragStartSplitterHandler);
        this._backendSplitter.detachObserver(window.EVENT_TYPE_SPLITTER_DROP, this._onDropSplitterHandler);
        this._backendSplitter.dispose();
        this._backendSplitter = null;
      }

      if (this._backendActionExecutor) {
        this._backendActionExecutor.detachObserver(
          window.EVENT_TYPE_BACKEND_ACTION_EXECUTED, this._onActionExecutedHandler
        );
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

      GlobalCache.dispose();
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
        this._directLinkExecutor.detachObserver(window.EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING);
        this._directLinkExecutor.dispose();
      }
    }
  }
}


Backend._instance = null;

Backend.getInstance = function (isDebugMode, options) {
  let instance = Backend._instance;
  if (instance === null) {
    instance = new Backend(isDebugMode, options);
    Backend._instance = instance;
  }

  return instance;
};


Quantumart.QP8.Backend = Backend;
