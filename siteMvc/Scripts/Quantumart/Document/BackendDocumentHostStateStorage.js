export class BackendDocumentHostStateStorage {
  constructor(options) {
    if (options) {
      if (options.currentCustomerCode) {
        this._currentCustomerCode = options.currentCustomerCode;
      }

      if (options.currentUserId) {
        this._currentUserId = options.currentUserId;
      }
    }

    const root = BackendDocumentHostStateStorage._keyNameRoot;
    this._keyPrefix = `${root}.${this._currentCustomerCode}.${this._currentUserId}`;
  }

  _currentCustomerCode = '';
  _currentUserId = '';
  _keyPrefix = '';

  loadHostState(hostParams) {
    const key = this.getHostKey(hostParams);
    if (key) {
      return JSON.parse(localStorage.getItem(key));
    }

    return undefined;
  }

  saveHostState(hostParams, hostState) {
    const key = this.getHostKey(hostParams);
    if (key) {
      if ($.isEmptyObject(hostState)) {
        localStorage.removeItem(key);
      } else {
        localStorage.setItem(key, JSON.stringify(hostState));
      }
    }
  }

  getHostKey(hostParams) {
    if (hostParams && !$.isEmptyObject(hostParams)) {
      const key = new $.telerik.stringBuilder();
      if (this._keyPrefix) {
        key
          .cat(this._keyPrefix)
          .cat('.')
          .cat(hostParams.actionCode)
          .cat('_')
          .cat(hostParams.entityId)
          .cat('_')
          .cat(hostParams.parentEntityId);
      }

      return key.string();
    }

    return undefined;
  }
}

BackendDocumentHostStateStorage._keyNameRoot = 'Quantumart.QP8.BackendDocumentHostStateStorage';

Quantumart.QP8.BackendDocumentHostStateStorage = BackendDocumentHostStateStorage;
