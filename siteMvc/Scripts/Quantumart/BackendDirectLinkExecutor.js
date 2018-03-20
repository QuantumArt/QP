import { Observable } from './Common/Observable';
import { $a, BackendActionParameters } from './BackendActionExecutor';
import { $q } from './Utils';

window.EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING = 'OnDirectLinkActionExecuting';

const SESSION_STORAGE_INSTANCE_ID = 'Quantumart.QP8.DirectLinkExecutor.InstanceId';
const LOCAL_STORAGE_PRIMARY_ID = 'Quantumart.QP8.DirectLinkExecutor.PrimaryId';
const LOCAL_STORAGE_DIRECT_LINK = 'Quantumart.QP8.DirectLinkExecutor.DirectLink';

export class DirectLinkExecutor extends Observable {
  _currentCustomerCode = null;
  _directLinkOptions = null;
  _isPrimaryInstance = false;
  _instanceId = null;

  constructor(currentCustomerCode, directLinkOptions) {
    super();

    this._currentCustomerCode = currentCustomerCode;
    this._directLinkOptions = directLinkOptions;

    const instanceId = window.sessionStorage.getItem(SESSION_STORAGE_INSTANCE_ID);
    if (instanceId) {
      this._instanceId = Number(instanceId);
    } else {
      this._instanceId = Number(new Date());
      window.sessionStorage.setItem(SESSION_STORAGE_INSTANCE_ID, String(this._instanceId));
    }

    this._onStorage = this._onStorage.bind(this);
    window.addEventListener('storage', this._onStorage, false);
  }

  /**
   * @param {StorageEvent} e
   */
  _onStorage(e) {
    if (!e.newValue) {
      return;
    }
    switch (e.key) {
      case LOCAL_STORAGE_PRIMARY_ID: {
        const primaryId = Number(e.newValue);
        if (primaryId > this._instanceId) {
          window.localStorage.setItem(LOCAL_STORAGE_PRIMARY_ID, String(this._instanceId));
        }
        break;
      }
      case LOCAL_STORAGE_DIRECT_LINK: {
        const message = JSON.parse(e.newValue);
        if ($q.isObject(message) && message.instanceId !== this._instanceId) {
          window.localStorage.removeItem(LOCAL_STORAGE_DIRECT_LINK);
          this._executeAction(message.directLinkOptions, true);
        }
        break;
      }
      default:
        break;
    }
  }

  /** @returns {Promise<boolean>} */
  _checkIsPrimaryInstance() {
    window.localStorage.setItem(LOCAL_STORAGE_PRIMARY_ID, String(this._instanceId));
    return new Promise(resolve => {
      setTimeout(() => {
        const primaryId = Number(window.localStorage.getItem(LOCAL_STORAGE_PRIMARY_ID));
        resolve(primaryId === this._instanceId);
      }, 500);
    });
  }

  /**
   * @param {function(boolean): void} callback
   */
  async ready(callback) {
    this._isPrimaryInstance = await this._checkIsPrimaryInstance();
    console.log({ _isPrimaryInstance: this._isPrimaryInstance });

    if (this._isPrimaryInstance) {
      const openByDirectLink = !!this._directLinkOptions;
      if ($q.isFunction(callback)) {
        callback(openByDirectLink);
      }
      if (openByDirectLink) {
        this._executeAction(this._directLinkOptions, false);
      }
    } else if (this._directLinkOptions) {
      if ($q.confirmMessage($l.BackendDirectLinkExecutor.WillBeRunInFirstInstanceConfirmation)) {
        window.localStorage.setItem(LOCAL_STORAGE_DIRECT_LINK, JSON.stringify({
          instanceId: this._instanceId,
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
