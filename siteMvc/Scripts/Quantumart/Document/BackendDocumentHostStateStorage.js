// ********************************************************************************************
// *** Хранилище состояний хостов документов                        ***
// ********************************************************************************************

Quantumart.QP8.BackendDocumentHostStateStorage = function(options) {
  if (options) {
    if (options.currentCustomerCode) {
      this._currentCustomerCode = options.currentCustomerCode;
    }

    if (options.currentUserId) {
      this._currentUserId = options.currentUserId;
    }
  }

  this._keyPrefix = Quantumart.QP8.BackendDocumentHostStateStorage._keyNameRoot + '.' + this._currentCustomerCode + '.' + this._currentUserId;
};

Quantumart.QP8.BackendDocumentHostStateStorage._keyNameRoot = 'Quantumart.QP8.BackendDocumentHostStateStorage';
Quantumart.QP8.BackendDocumentHostStateStorage.prototype = {
  _currentCustomerCode: '',
  _currentUserId: '',
  _keyPrefix: '',

  loadHostState: function(hostParams) {
    var key = this._get_host_key(hostParams);

    if (key) {
      return JSON.parse(localStorage.getItem(key));
    }
  },

  saveHostState: function(hostParams, hostState) {
    var key = this._get_host_key(hostParams);

    if (key) {
      if (jQuery.isEmptyObject(hostState)) {
        localStorage.removeItem(key);
      } else {
        localStorage.setItem(key, JSON.stringify(hostState));
      }
    }
  },

  dispose: function() { },

  _get_host_key: function(hostParams) {
    if (hostParams && !jQuery.isEmptyObject(hostParams)) {
      var key = new $.telerik.stringBuilder();

      key.cat(this._keyPrefix).cat('.')
      .cat(hostParams.actionCode).cat('_')
      .cat(hostParams.entityId).cat('_')
      .cat(hostParams.parentEntityId);
      return key.string();
    }
  }
};
