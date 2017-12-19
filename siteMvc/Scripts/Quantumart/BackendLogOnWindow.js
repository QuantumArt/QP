Quantumart.QP8.BackendLogOnWindow = function () {
  Quantumart.QP8.BackendLogOnWindow.initializeBase(this);
};

Quantumart.QP8.BackendLogOnWindow.prototype = {
  _windowComponent: null,
  _isAuthenticated: null,
  _userName: null,

  FORM_SELECTOR: 'form#auth',
  LOADING_SELECTOR: '#authLoading',
  USERNAME_SELECTOR: '#UserName',
  PASSWORD_SELECTOR: '#Password',
  CUSTOMERCODE_SELECTOR: '#CustomerCode',
  Z_INDEX: 50000,
  AJAX_EVENT: 'AjaxEvent',

  _onLogonHandler: null,
  _onCloseWindowHandler: null,

  _getServerContent(data) {
    if (data.success) {
      return data.view;
    }

    return data.message;
  },

  _createWindow(serverContent) {
    const that = this;
    this._isAuthenticated = false;
    this._userName = null;

    this._windowComponent = $.telerik.window.create({
      title: $l.Common.ajaxUserSessionExpiredErrorMessage,
      html: serverContent,
      width: 500,
      height: 265,
      modal: true,
      resizable: false,
      draggable: false,
      visible: true
    }).data('tWindow').center();

    this._onLogonHandler = function (evt) {
      that._disableWindow();
      evt.preventDefault();
      let content;
      const currentUserName = that._getGurrentUserName();
      const currentCustomerCode = that._getGurrentCustomerCode();
      const userName = $(that.USERNAME_SELECTOR).val();
      const password = $(that.PASSWORD_SELECTOR).val();
      const customerCode = $(that.CUSTOMERCODE_SELECTOR).val();
      let method = 'GET';
      let useAutoLogin = that._getUseAutoLogin();
      const url = that._getUrl();
      let setDefaultValues = false;

      if (evt.type === 'submit') {
        method = 'POST';
      } else {
        setDefaultValues = useAutoLogin;
        useAutoLogin = !useAutoLogin;
      }

      $q.getJsonFromUrl(
        method,
        url,
        {
          UseAutoLogin: useAutoLogin,
          UserName: userName,
          Password: password,
          CustomerCode: customerCode
        },
        true,
        false,
        data => {
          if (data.isAuthenticated) {
            that._isAuthenticated = true;
            that._userName = data.userName;
          } else {
            content = that._getServerContent(data);
          }

          if (that._isAuthenticated) {
            that._closeWindow();

            const needRefresh = that._userName !== currentUserName || customerCode !== currentCustomerCode;

            if (needRefresh) {
              location.reload();
            }
          } else {
            that._updateWindow(content);

            if (setDefaultValues) {
              that._setDefaultValues();
            }
          }
        },
        (jqXHR, textStatus, errorThrown) => {
          that._updateWindow(errorThrown);
        }
      );
    };

    this._onCloseWindowHandler = function () {
      that._triggerDeferredCallcacks(that._isAuthenticated);
      that.dispose();
    };

    jQuery(this._windowComponent.element).bind('close', this._onCloseWindowHandler);
  },

  _updateWindow(serverContent) {
    this._detachEvents();
    this._windowComponent.content(serverContent);
    this._attachEvents();
    this._enableWindow();
  },

  _setDefaultValues() {
    const currentUserName = this._getGurrentUserName();
    const currentCustomerCode = this._getGurrentCustomerCode();
    $(this.USERNAME_SELECTOR).val(currentUserName);
    $(this.CUSTOMERCODE_SELECTOR).val(currentCustomerCode);
  },

  _showWindow(data) {
    if (!this._windowComponent) {
      this._createWindow(this._getServerContent(data));
      this._attachEvents();
      this._setDefaultValues();
      this._enableWindow();
    }
  },

  _getGurrentUserName() {
    return $('span.userName').text();
  },

  _getGurrentCustomerCode() {
    return $('span.t-in').first().text();
  },

  _getUrl() {
    let url = $(this.FORM_SELECTOR).attr('action');

    if (!url.endsWith('/')) {
      url += '/';
    }

    return url;
  },

  _updateZindex() {
    if (this._windowComponent) {
      $(this._windowComponent.element).css('z-index', this.Z_INDEX);
    }
  },

  _getUseAutoLogin() {
    return $('input#UseAutoLogin').val().toUpperCase() === 'TRUE';
  },

  _disableWindow() {
    $(this.LOADING_SELECTOR).show();
    $(this.FORM_SELECTOR).find(':input:not(:disabled)').prop('disabled', true);
  },

  _enableWindow() {
    $(this.FORM_SELECTOR).find(':input:disabled').prop('disabled', false);
    $(this.LOADING_SELECTOR).hide();
  },

  _closeWindow() {
    this._windowComponent.close();
  },

  _attachEvents() {
    $(this.FORM_SELECTOR).submit(this._onLogonHandler);
    $(this.FORM_SELECTOR).find('a').click(this._onLogonHandler);
  },

  _detachEvents() {
    $(this.FORM_SELECTOR).off();
    $(this.FORM_SELECTOR).find('a').off();
  },

  _triggerDeferredCallcacks(isAuthenticated) {
    $(this).triggerHandler({
      type: this.AJAX_EVENT,

      // @ts-ignore: JQueryEvent is used as business logic event
      value: isAuthenticated
    });
  },

  _addDeferredCallcack(callback, settings) {
    this._updateZindex();
    $(this).on(this.AJAX_EVENT, e => {
      // @ts-ignore: JQueryEvent is used as business logic event
      const isAuthenticated = e.value;
      if (isAuthenticated) {
        jQuery.ajax(settings).done(callback);
      } else {
        callback({ success: true });
      }
    });
  },

  _clearDeferredCallcacks() {
    $(this).off(this.AJAX_EVENT);
  },

  showLogonForm(data, callback, settings) {
    this._addDeferredCallcack(callback, settings);
    this._showWindow(data);
  },

  needLogon(jqXHR, url) {
    if (
      url.toUpperCase() === window.CONTROLLER_URL_LOGON.toUpperCase()
      || url.toUpperCase() === window.CONTROLLER_URL_WINLOGON.toUpperCase()) {
      return false;
    } else if (jqXHR.getResponseHeader('QP-Not-Authenticated')) {
      return true;
    }
    return false;
  },

  dispose() {
    Quantumart.QP8.BackendLogOnWindow.callBaseMethod(this, 'dispose');
    this._clearDeferredCallcacks();

    if (this._windowComponent) {
      this._detachEvents();
      let windowComponent = this._windowComponent;
      let $window = jQuery(windowComponent.element);
      $window
        .unbind('close', this._onCloseWindowHandler)
      ;

      $window = null;
      $c.destroyPopupWindow(windowComponent);
      windowComponent = null;
      this._windowComponent = null;
    }

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendLogOnWindow._instance = null;
Quantumart.QP8.BackendLogOnWindow.deferredExecution = function (data, jqXHR, callback, settings) {
  const logon = Quantumart.QP8.BackendLogOnWindow.getInstance();

  if (logon.needLogon(jqXHR, settings.url)) {
    logon.showLogonForm(data, callback, settings);
    return true;
  }

  return false;
};

Quantumart.QP8.BackendLogOnWindow.getInstance = function () {
  if (Quantumart.QP8.BackendLogOnWindow._instance === null) {
    Quantumart.QP8.BackendLogOnWindow._instance = new Quantumart.QP8.BackendLogOnWindow();
  }

  return Quantumart.QP8.BackendLogOnWindow._instance;
};

Quantumart.QP8.BackendLogOnWindow.destroyInstance = function () {
  if (Quantumart.QP8.BackendLogOnWindow._instance) {
    Quantumart.QP8.BackendLogOnWindow._instance.dispose();
    Quantumart.QP8.BackendLogOnWindow._instance = null;
  }
};

Quantumart.QP8.BackendLogOnWindow.registerClass('Quantumart.QP8.BackendLogOnWindow', Quantumart.QP8.Observable);
