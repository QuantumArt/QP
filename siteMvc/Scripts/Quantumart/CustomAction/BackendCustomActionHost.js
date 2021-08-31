import { BackendEventArgs } from '../Common/BackendEventArgs';
import { BackendSelectPopupWindow } from '../List/BackendSelectPopupWindow';
import { Observable } from '../Common/Observable';
import { BackendLibrary } from '../Library/BackendLibrary';
import { $a, BackendActionParameters } from '../BackendActionExecutor';
import { $c } from '../ControlHelpers';
import { $o } from '../Info/BackendEntityObject';
import { $q } from '../Utils';
import './QP8BackendApi.Interaction';

window.EVENT_TYPE_EXTERNAL_ACTION_EXECUTING = 'OnExternalActionExecuting';

export class BackendCustomActionHost extends Observable {
  _previewWindowComponent = null;

  constructor(hostId, options, manager) {
    super();

    this._hostId = hostId;
    this._options = options;
    this._manager = manager;
  }

  initialize() {
    pmrpc.register({
      publicProcedureName: this._getExecuteActionProcedureName(),
      procedure: $.proxy(this._onExternalMessageReceived, this),
      isAsynchronous: true
    });

    $(`#${this._options.iframeElementId}`).attr('src', this._generateActionUrl());
  }

  _onExternalMessageReceived(message, successCallback) {
    if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.ExecuteAction) {
      this._onExecuteActionMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.CloseBackendHost) {
      this._onCloseHostMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.OpenSelectWindow) {
      this._onOpenSelectWindowMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.CheckHost) {
      successCallback(this._onCheckHostMessageReceived(message));
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.PreviewImage) {
      this._onPreviewImageMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.DownloadFile) {
      this._onDownloadFileMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.OpenFileLibrary) {
      this._onOpenFileLibraryMessageReceived(message);
      successCallback(0);
    } else if (message.type === Quantumart.QP8.Interaction.ExternalMessageTypes.SendNotification) {
      this._onSendNotificationMessageReceived(message);
      successCallback(0);
    }
  }

  onSelect() {
    const id = this._options.iframeElementId;
    $(`#${id}`).css('marginLeft', '1px');
    setTimeout(() => {
      $(`#${id}`).css('marginLeft', '0');
    }, 0);
  }

  _onCheckHostMessageReceived(_message) {
    return window.BACKEND_VERSION;
  }

  _onCloseHostMessageReceived(message) {
    this._manager.onCloseHostMessageReceived(message);
  }

  _onExecuteActionMessageReceived(message) {
    const action = $a.getBackendActionByCode(message.data.actionCode);
    const params = new BackendActionParameters({
      entityTypeCode: message.data.entityTypeCode,
      entityId: message.data.entityId,
      entityName: $o.getEntityName(message.data.entityTypeCode, message.data.entityId, message.data.parentEntityId),
      parentEntityId: message.data.parentEntityId
    });

    params.correct(action);

    const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
    eventArgs.set_externalCallerContext(message);
    if (!$q.toBoolean(message.data.changeCurrentTab, false) && !$q.isNull(message.data.isWindow)) {
      eventArgs.set_isWindow(
        $q.toBoolean(
          message.data.isWindow,
          eventArgs.get_isWindow()
        )
      );
    }

    if (message.data.options && message.data.options.currentContext) {
      // eslint-disable-next-line no-param-reassign
      message.data.options.contextQuery = JSON.stringify(
        $o.getContextQuery(message.data.parentEntityId, message.data.options.currentContext)
      );
    }

    eventArgs.set_additionalData(message.data.options);
    eventArgs.set_startedByExternal(true);
    this.notify(window.EVENT_TYPE_EXTERNAL_ACTION_EXECUTING, eventArgs);
  }

  _onPreviewImageMessageReceived(message) {
    const { entityId, fieldId, fileName } = message.data;
    if ($q.isNullOrWhiteSpace(fileName)) {
      return;
    }
    $c.destroyPopupWindow(this._previewWindowComponent);
    const urlParams = {
      id: `field_${fieldId}`,
      fileName: encodeURIComponent(fileName),
      isVersion: false,
      entityId
    };
    const testUrl = BackendLibrary.generateActionUrl('GetImageProperties', urlParams);
    this._previewWindowComponent = $c.preview(testUrl);
  }

  _onSendNotificationMessageReceived(message) {
    const { title, icon, body } = message.data;
    if (Notification.permission === 'granted') {
      const notification = new Notification(title, { icon, body });
      console.log(notification);
    } else {
      console.warn('External notification received, but push notifications are not allowed');
    }
  }

  _onDownloadFileMessageReceived(message) {
    const { entityId, fieldId, fileName } = message.data;
    if ($q.isNullOrWhiteSpace(fileName)) {
      return;
    }
    const urlParams = {
      id: `field_${fieldId}`,
      fileName: encodeURIComponent(fileName),
      isVersion: false,
      entityId
    };
    const url = BackendLibrary.generateActionUrl('TestFieldValueDownload', urlParams);
    $c.downloadFileWithChecking(url, fileName);
  }

  _onOpenFileLibraryMessageReceived(message) {
    const eventArgs = new BackendEventArgs();
    eventArgs.set_entityId(message.data.libraryEntityId);
    eventArgs.set_parentEntityId(message.data.libraryParentEntityId);
    eventArgs.set_entityTypeCode(
      message.data.useSiteLibrary ? window.ENTITY_TYPE_CODE_SITE : window.ENTITY_TYPE_CODE_CONTENT
    );
    eventArgs.set_actionCode(
      message.data.useSiteLibrary ? window.ACTION_CODE_POPUP_SITE_LIBRARY : window.ACTION_CODE_POPUP_CONTENT_LIBRARY
    );

    const options = {
      isMultiOpen: false,
      additionalUrlParameters: {
        filterFileTypeId: message.data.isImage ? Quantumart.QP8.Enums.LibraryFileType.Image : '',
        subFolder: message.data.subFolder,
        allowUpload: true
      }
    };

    /** @type {BackendSelectPopupWindow & { callerCallback?: string, selectWindowUID?: string }} */
    const selectPopupWindowComponent = new BackendSelectPopupWindow(eventArgs, options);
    selectPopupWindowComponent.callerCallback = message.data.callerCallback;
    selectPopupWindowComponent.selectWindowUID = message.data.selectWindowUID;
    selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED, (_eventType, sender, args) => {
        const sep = window.DIRECTORY_SEPARATOR_CHAR;
        if (args) {
          const { entities } = args;
          if (entities.length > 0) {
            let url = args.context;
            if (url === sep) {
              url = '';
            }
            const re = new RegExp(`${sep}`, 'g');
            url = url.replace(`${message.data.subFolder || ''}${sep}`, '')
              .replace(re, '/');
            this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.FileSelected, {
              filePath: url + entities[0].Name,
              callerCallback: sender.callerCallback
            });
          }
        }
        this._destroySelectPopupWindow(sender);
      }
    );
    selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED, (_eventType, sender) => {
        this._destroySelectPopupWindow(sender);
      }
    );
    selectPopupWindowComponent.openWindow();
  }

  _onOpenSelectWindowMessageReceived(message) {
    const eventArgs = new BackendEventArgs();
    eventArgs.set_isMultipleEntities(message.data.isMultiple);
    eventArgs.set_parentEntityId(message.data.parentEntityId);
    eventArgs.set_entityTypeCode(message.data.entityTypeCode);
    eventArgs.set_actionCode(message.data.selectActionCode);

    if ($q.isArray(message.data.selectedEntityIDs) && !$q.isNullOrEmpty(message.data.selectedEntityIDs)) {
      const selectedEntities = message.data.selectedEntityIDs.map(id => ({ Id: id }));
      if (message.data.isMultiple) {
        eventArgs.set_entities(selectedEntities);
      } else {
        eventArgs.set_entityId(selectedEntities[0].Id);
      }
    }

    /** @type {BackendSelectPopupWindow & { callerCallback?: string, selectWindowUID?: string }} */
    const selectPopupWindowComponent = new BackendSelectPopupWindow(eventArgs, message.data.options);
    selectPopupWindowComponent.callerCallback = message.data.callerCallback;
    selectPopupWindowComponent.selectWindowUID = message.data.selectWindowUID;
    selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED,
      $.proxy(this._popupWindowSelectedHandler, this)
    );

    selectPopupWindowComponent.attachObserver(
      window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED,
      $.proxy(this._popupWindowClosedHandler, this)
    );

    selectPopupWindowComponent.openWindow();
  }

  _popupWindowSelectedHandler(eventType, sender, args) {
    let { entities: selectedEntities } = args;
    let selectedEntityIDs = selectedEntities.map(el => el.Id);
    if ($o.checkEntitiesForPresenceEmptyNames(selectedEntities)) {
      if (args.entityTypeCode && args.parentEntityId) {
        const dataItems = $o.getSimpleEntityList(
          args.entityTypeCode,
          args.parentEntityId,
          0,
          0,
          window.$e.ListSelectionMode.OnlySelectedItems,
          selectedEntityIDs
        );

        selectedEntities = $c.getEntitiesFromListItemCollection(dataItems);
        selectedEntityIDs = selectedEntities.map(el => el.Id);
      }
    }

    this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.EntitiesSelected, {
      selectedEntityIDs,
      selectedEntities,
      callerCallback: sender.callerCallback,
      selectWindowUID: sender.selectWindowUID
    });

    this._destroySelectPopupWindow(sender);
  }

  _popupWindowClosedHandler(eventType, sender) {
    this._destroySelectPopupWindow(sender);
  }

  _destroySelectPopupWindow(popupWindows) {
    if (!$q.isNull(popupWindows)) {
      popupWindows.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_RESULT_SELECTED);
      popupWindows.detachObserver(window.EVENT_TYPE_SELECT_POPUP_WINDOW_CLOSED);
      popupWindows.closeWindow();
      popupWindows.dispose();

      this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.SelectWindowClosed, {
        callerCallback: popupWindows.callerCallback,
        selectWindowUID: popupWindows.selectWindowUID
      });
    }
  }

  // eslint-disable-next-line camelcase
  get_hostUID() {
    return this._options.hostUID;
  }

  onExternalCallerContextsUnbinded(message) {
    this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.HostUnbinded, message);
  }

  onChildHostActionExecuted(message) {
    this._invokeCallback(Quantumart.QP8.Interaction.BackendEventTypes.ActionExecuted, message);
  }

  _invokeCallback(type, message) {
    const iframe = window.document.getElementById(this._options.iframeElementId);
    if (iframe && iframe.contentWindow) {
      const args = {};
      Object.assign(args, message);
      delete args.callerCallback;
      pmrpc.call({
        destination: iframe.contentWindow,
        publicProcedureName: message.callerCallback,
        params: [type, args]
      });
    }
  }

  _getExecuteActionProcedureName() {
    return this._options.hostUID;
  }

  _generateActionUrl() {
    let resultUrl = $q.updateQueryStringParameter(this._options.actionBaseUrl, 'hostUID', this._options.hostUID);
    if (this._options.additionalParams) {
      resultUrl += `&${$.param(this._options.additionalParams)}`;
    }
    return resultUrl;
  }

  dispose() {
    pmrpc.unregister(this._getExecuteActionProcedureName());
    this._manager.removeComponent(this);
  }
}


Quantumart.QP8.BackendCustomActionHost = BackendCustomActionHost;
