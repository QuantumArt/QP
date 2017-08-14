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

  loadHostState: function (hostParams) {
    let key = this._get_host_key(hostParams);
    if (key) {
      return JSON.parse(localStorage.getItem(key));
    }
  },

  saveHostState: function (hostParams, hostState) {
    let key = this._get_host_key(hostParams);
    if (key) {
      if ($.isEmptyObject(hostState)) {
        localStorage.removeItem(key);
      } else {
        localStorage.setItem(key, JSON.stringify(hostState));
      }
    }
  },

  _get_host_key: function (hostParams) {
    if (hostParams && !$.isEmptyObject(hostParams)) {
      let key = new $.telerik.stringBuilder();
      if (this && this._keyPrefix) {
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
  }
};
