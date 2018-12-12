import { Observable } from './Common/Observable';
import { $a, BackendActionParameters } from './BackendActionExecutor';
import { $q } from './Utils';

window.EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING = 'OnDirectLinkActionExecuting';

const SESSION_STORAGE_STATE = 'Quantumart.QP8.DirectLinkExecutor.SessionState';
const LOCAL_STORAGE_PRIMARY_ID = 'Quantumart.QP8.DirectLinkExecutor.PrimaryId';
const LOCAL_STORAGE_DIRECT_LINK = 'Quantumart.QP8.DirectLinkExecutor.DirectLink';

let inBackground = document.visibilityState === 'hidden';

window.addEventListener('focus', () => {
  inBackground = false;
}, false);

window.addEventListener('blur', () => {
  inBackground = true;
}, false);

export class DirectLinkExecutor extends Observable {
  _currentCustomerCode = null;
  _directLinkOptions = null;
  _sessionState = {
    isNew: true,
    isPrimary: false,
    instanceId: Number(new Date())
  };

  constructor(currentCustomerCode, directLinkOptions) {
    super();

    this._currentCustomerCode = currentCustomerCode;
    this._directLinkOptions = directLinkOptions;

    const sessionJson = window.sessionStorage.getItem(SESSION_STORAGE_STATE);
    if (sessionJson) {
      this._sessionState = JSON.parse(sessionJson);
    } else {
      window.sessionStorage.setItem(SESSION_STORAGE_STATE, JSON.stringify(this._sessionState));
    }

    this._onStorage = this._onStorage.bind(this);
    this._onBeforeUnload = this._onBeforeUnload.bind(this);
    window.addEventListener('storage', this._onStorage, false);
    window.addEventListener('beforeunload', this._onBeforeUnload, false);
  }

  _setSessionState(changedState) {
    Object.assign(this._sessionState, changedState);
    window.sessionStorage.setItem(SESSION_STORAGE_STATE, JSON.stringify(this._sessionState));
  }

  /**
   * @param {StorageEvent} e
   */
  _onStorage(e) {
    const { isNew, isPrimary, instanceId } = this._sessionState;
    if (e.newValue) {
      switch (e.key) {
        case LOCAL_STORAGE_PRIMARY_ID: {
          const primaryId = JSON.parse(e.newValue);
          if ((isPrimary && instanceId !== primaryId) || (isNew && instanceId < primaryId)) {
            window.localStorage.setItem(LOCAL_STORAGE_PRIMARY_ID, JSON.stringify(instanceId));
          }
          break;
        }
        case LOCAL_STORAGE_DIRECT_LINK: {
          const message = JSON.parse(e.newValue);
          if (isPrimary && message.instanceId !== instanceId) {
            window.localStorage.removeItem(LOCAL_STORAGE_DIRECT_LINK);

            // https://developers.google.com/web/updates/2017/03/dialogs-policy
            // https://bugs.chromium.org/p/chromium/issues/detail?id=629964
            // `window.confirm()` silently returns `false` in Chromium
            // if current window in background and DevTools are closed
            if (inBackground) {
              const handler = () => {
                window.removeEventListener('focus', handler, false);
                this._executeAction(message.directLinkOptions, true);
              };
              window.addEventListener('focus', handler, false);
            } else {
              this._executeAction(message.directLinkOptions, true);
            }
          }
          break;
        }
        default:
          break;
      }
    }
  }

  _onBeforeUnload() {
    const { isPrimary, instanceId } = this._sessionState;
    if (isPrimary) {
      const primaryIdJson = window.localStorage.getItem(LOCAL_STORAGE_PRIMARY_ID);
      if (primaryIdJson) {
        const primaryId = JSON.parse(primaryIdJson);
        if (primaryId === instanceId) {
          window.localStorage.removeItem(LOCAL_STORAGE_PRIMARY_ID);
        }
      }
    }
  }

  /** @returns {Promise<boolean>} */
  _checkIsPrimaryInstance() {
    const { instanceId } = this._sessionState;

    let primaryIdJson = window.localStorage.getItem(LOCAL_STORAGE_PRIMARY_ID);
    if (primaryIdJson) {
      const primaryId = JSON.parse(primaryIdJson);
      if (primaryId === instanceId) {
        return Promise.resolve(true);
      }
    } else {
      window.localStorage.setItem(LOCAL_STORAGE_PRIMARY_ID, JSON.stringify(instanceId));
      return Promise.resolve(true);
    }

    return new Promise(resolve => {
      window.localStorage.setItem(LOCAL_STORAGE_PRIMARY_ID, JSON.stringify(instanceId));
      setTimeout(() => {
        primaryIdJson = window.localStorage.getItem(LOCAL_STORAGE_PRIMARY_ID);
        if (primaryIdJson) {
          const primaryId = JSON.parse(primaryIdJson);
          resolve(primaryId === instanceId);
        } else {
          window.localStorage.setItem(LOCAL_STORAGE_PRIMARY_ID, JSON.stringify(instanceId));
          resolve(true);
        }
      }, 500);
    });
  }

  /**
   * @param {function(boolean): void} callback
   */
  async ready(callback) {
    const isPrimary = await this._checkIsPrimaryInstance();
    this._setSessionState({ isNew: false, isPrimary });

    if (isPrimary) {
      const openByDirectLink = !!this._directLinkOptions;
      if ($q.isFunction(callback)) {
        callback(openByDirectLink);
      }
      if (openByDirectLink) {
        this._executeAction(this._directLinkOptions, false);
      }
    } else if (this._directLinkOptions) {
      if ($q.confirmMessage($l.BackendDirectLinkExecutor.WillBeRunInFirstInstanceConfirmation)) {
        const { instanceId } = this._sessionState;
        window.localStorage.setItem(LOCAL_STORAGE_DIRECT_LINK, JSON.stringify({
          instanceId,
          directLinkOptions: this._directLinkOptions
        }));
      }
    } else {
      $q.alertFail($l.BackendDirectLinkExecutor.InstanceIsAllreadyOpen);
    }
  }

  _executeAction(actionParams, byMessage) {
    const newParams = { ...actionParams };
    if ($q.isNullOrEmpty(newParams.customerCode)) {
      newParams.customerCode = this._currentCustomerCode;
    }

    if (newParams.customerCode.toLowerCase() === this._currentCustomerCode.toLowerCase()) {
      if (!byMessage || $q.confirmMessage($l.BackendDirectLinkExecutor.OpenDirectLinkConfirmation)) {
        const action = $a.getBackendActionByCode(newParams.actionCode);
        if ($q.isNullOrEmpty(action)) {
          $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
        } else {
          const params = new BackendActionParameters({
            entityTypeCode: newParams.entityTypeCode,
            entityId: newParams.entityId,
            parentEntityId: newParams.parentEntityId
          });

          params.correct(action);
          const eventArgs = $a.getEventArgsFromActionWithParams(action, params);
          this.notify(window.EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING, eventArgs);
        }
      }
    } else if ($q.confirmMessage($l.BackendDirectLinkExecutor.ReloginRequestConfirmation)) {
      window.location.href = `${window.CONTROLLER_URL_LOGON}LogOut/?${jQuery.param(newParams)}`;
    }
  }

  dispose() {
    window.removeEventListener('storage', this._onStorage);
  }
}

Quantumart.QP8.DirectLinkExecutor = DirectLinkExecutor;
