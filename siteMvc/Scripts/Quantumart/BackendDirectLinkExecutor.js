window.EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING = 'OnDirectLinkActionExecuting';

Quantumart.QP8.DirectLinkExecutor = function (currentCustomerCode, directLinkOptions) {
  Quantumart.QP8.DirectLinkExecutor.initializeBase(this);

  this._currentCustomerCode = currentCustomerCode;
  this._urlLinkParams = directLinkOptions;
  this._uid = new Date().getTime();
};

Quantumart.QP8.DirectLinkExecutor.prototype = {
  LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG: 'Quantumart.QP8.DirectLinkExecutor.BackendIsAllreadyExists',
  LOCAL_STORAGE_KEY_OBSERVABLE_ITEM: 'Quantumart.QP8.DirectLinkExecutor.DirectLinkData',
  LOCAL_STORAGE_KEY_INSTANCE_EXISTING_RESPONSE: 'Quantumart.QP8.DirectLinkExecutor.ExistingRespose',
  LOCAL_STORAGE_KEY_SENT_KEY_NAME: 'Quantumart.QP8.DirectLinkExecutor.LastKeyName',

  _currentCustomerCode: null,
  _urlLinkParams: null,
  _imFirst: false,
  _uid: null,

  _onDirectLinkOpenRequested: function (e) {
    if ($q.isNullOrEmpty(e.key)) {
      var key = window.localStorage.getItem(this.LOCAL_STORAGE_KEY_SENT_KEY_NAME);
      if (key) {
        e.key = key;
        e.newValue = window.localStorage.getItem(key);
      }
    }

    if (e.key == this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM && $q.isString(e.newValue)) {
      var actionParams = jQuery.parseJSON(e.newValue);
      this._executeAction(actionParams, true);
    } else if (e.key == this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG && $.isNumeric(e.newValue)) {
      if (e.newValue != this._uid) {
        this._send(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_RESPONSE, 'true');
      }
    }
  },

  _onDirectLinkOpenRequestedHandler: null,

  _executeAction: function (actionParams, byRequest) {
    if (actionParams) {
      if ($q.isNullOrEmpty(actionParams.customerCode)) {
 actionParams.customerCode = this._currentCustomerCode; 
}
      if (actionParams.customerCode.toLowerCase() == this._currentCustomerCode.toLowerCase()) {
        if (!byRequest || window.confirm($l.BackendDirectLinkExecutor.OpenDirectLinkConfirmation)) {
          var action = $a.getBackendActionByCode(actionParams.actionCode);
          if (!action) {
            $q.alertError($l.Common.ajaxDataReceivingErrorMessage);
          } else {
            var actionTypeCode = action.ActionType.Code;
            var params = new Quantumart.QP8.BackendActionParameters({
              entityTypeCode: actionParams.entityTypeCode,
              entityId: actionParams.entityId,
              parentEntityId: actionParams.parentEntityId
            });

            params.correct(action);
            var eventArgs = $a.getEventArgsFromActionWithParams(action, params);
            this.notify(EVENT_TYPE_DIRECT_LINK_ACTION_EXECUTING, eventArgs);
          }
        }
      } else {
        if (window.confirm($l.BackendDirectLinkExecutor.ReloginRequestConfirmation)) {
          window.location.href = CONTROLLER_URL_LOGON + 'LogOut/?' + jQuery.param(actionParams);
        }
      }

      if (byRequest) {
        window.localStorage.removeItem(this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM);
      }
    }
  },

  _send: function (type, message) {
    window.localStorage.removeItem(this.LOCAL_STORAGE_KEY_SENT_KEY_NAME);
    window.localStorage.setItem(type, message);
    if ('onstorage' in document) {
      window.localStorage.setItem(this.LOCAL_STORAGE_KEY_SENT_KEY_NAME, type);
    }
  },

  _instanceExistenceCheck: function () {
    var dfr = new jQuery.Deferred();
    if (window.localStorage.getItem(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG)) {
      this._send(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG, this._uid);
      var that = this;
      setTimeout(function () {
        var testResponse = !!window.localStorage.getItem(that.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_RESPONSE);
        window.localStorage.removeItem(that.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_RESPONSE);
        dfr.resolveWith(that, [testResponse]);
      }, 500);
    } else {
      dfr.resolveWith(this, [false]);
    }

    return dfr.promise();
  },

  _registerInstance: function () {
    this._send(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG, 'true');
  },

  _unregisterInstance: function () {
    window.localStorage.removeItem(this.LOCAL_STORAGE_KEY_INSTANCE_EXISTING_FLAG);
  },

  ready: function (callback) {
    this._instanceExistenceCheck().done(function (instanceExists) {
      if (!instanceExists) {
        this._imFirst = true;
        this._registerInstance();
        this._send(this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM, '');
        this._onDirectLinkOpenRequestedHandler = jQuery.proxy(this._onDirectLinkOpenRequested, this);

        if (window.addEventListener) {
          window.addEventListener('storage', this._onDirectLinkOpenRequestedHandler, false);
        } else if (document.attachEvent) {
          document.attachEvent('onstorage', this._onDirectLinkOpenRequestedHandler);
        }

        var openByDirectLink = !!this._urlLinkParams;
        if ($q.isFunction(callback)) {
          callback(openByDirectLink);
        }

        if (openByDirectLink) {
          this._executeAction(this._urlLinkParams, false);
        }
      } else {
        this._imFirst = false;
        if (this._urlLinkParams) {
          if (window.confirm($l.BackendDirectLinkExecutor.WillBeRunInFirstInstanceConfirmation)) {
            this._send(this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM, JSON.stringify(this._urlLinkParams));
          }
        } else {
          $q.alertFail($l.BackendDirectLinkExecutor.InstanceIsAllreadyOpen);
        }
      }
    });
  },

  dispose: function () {
    if (this._imFirst) {
      if (!('onstorage' in document)) {
        this._unregisterInstance();
      }

      window.localStorage.removeItem(this.LOCAL_STORAGE_KEY_OBSERVABLE_ITEM);
      if (window.removeEventListener) {
        window.removeEventListener('storage', this._onDirectLinkOpenRequestedHandler);
      } else if (document.detachEvent) {
        document.detachEvent('onstorage', this._onDirectLinkOpenRequestedHandler);
      }
    }
  }
};

Quantumart.QP8.DirectLinkExecutor.registerClass('Quantumart.QP8.DirectLinkExecutor', Quantumart.QP8.Observable);

