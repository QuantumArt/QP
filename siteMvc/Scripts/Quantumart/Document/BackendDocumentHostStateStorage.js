Quantumart.QP8.BackendDocumentHostStateStorage = function (options) {
  if (options) {
    if (options.currentCustomerCode) {
      this._currentCustomerCode = options.currentCustomerCode;
    }

    if (options.currentUserId) {
      this._currentUserId = options.currentUserId;
    }
  }

  this._keyPrefix = `${Quantumart.QP8.BackendDocumentHostStateStorage._keyNameRoot}.${this._currentCustomerCode}.${this._currentUserId}`;
};

Quantumart.QP8.BackendDocumentHostStateStorage._keyNameRoot = 'Quantumart.QP8.BackendDocumentHostStateStorage';
Quantumart.QP8.BackendDocumentHostStateStorage.prototype = {
  _currentCustomerCode: '',
  _currentUserId: '',
  _keyPrefix: '',

  loadHostState(hostParams) {
    const key = this.getHostKey(hostParams);
    if (key) {
      return JSON.parse(localStorage.getItem(key));
    }
    return undefined;
  },

  saveHostState(hostParams, hostState) {
    const key = this.getHostKey(hostParams);
    if (key) {
      if ($.isEmptyObject(hostState)) {
        localStorage.removeItem(key);
      } else {
        localStorage.setItem(key, JSON.stringify(hostState));
      }
    }
  },

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
};
