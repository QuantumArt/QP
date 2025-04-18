import { BackendBrowserHistoryManager } from './Managers/BackendBrowserHistoryManager';
import { Observable } from './Common/Observable';
import { $c } from './ControlHelpers';
import { $q } from './Utils';

export class BackendLogOnWindow extends Observable {
  _backendBrowserHistoryManager = BackendBrowserHistoryManager.getInstance();

  _windowComponent = null;
  _isAuthenticated = null;
  _userName = null;

  FORM_SELECTOR = 'form#auth';
  LOADING_SELECTOR = '#authLoading';
  USERNAME_SELECTOR = '#UserName';
  PASSWORD_SELECTOR = '#Password';
  CUSTOMERCODE_SELECTOR = '#CustomerCode';
  SSO_SELECTOR = '#SSO';
  Z_INDEX = 50000;
  AJAX_EVENT = 'AjaxEvent';
  KEYCLOAK_RESPONSE_CHECK_INTERVAL = 1000;

  _onLogonHandler = null;
  _onCloseWindowHandler = null;
  _onSsoHandler = null;
  _intervalId = null;

  _getServerContent(data) {
    if (data.success) {
      return data.view;
    }

    return data.message;
  }

  _createWindow(serverContent) {
    const that = this;
    this._isAuthenticated = false;
    this._userName = null;

    this._windowComponent = $.telerik.window.create({
      title: $l.Common.ajaxUserSessionExpiredErrorMessage,
      html: serverContent,
      width: 700,
      height: 350,
      modal: true,
      resizable: false,
      draggable: false,
      visible: true,
      onOpen: this._backendBrowserHistoryManager.handleModalWindowOpen,
      onClose: this._backendBrowserHistoryManager.handleModalWindowClose
    }).data('tWindow').center();

    this._onLogonHandler = function (evt) {
      that._disableWindow();
      evt.preventDefault();
      let content;
      const currentUserName = that._getCurrentUserName();
      const currentCustomerCode = that._getCurrentCustomerCode();
      const userName = $(that.USERNAME_SELECTOR).val();
      const password = $(that.PASSWORD_SELECTOR).val();
      const customerCode = $(that.CUSTOMERCODE_SELECTOR).val();
      let method = 'GET';
      let useAutoLogin = that._getUseAutoLogin();
      const url = that._getUrl(evt.type === 'submit');
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

    this._onSsoHandler = function (event) {
      that._disableWindow();
      event.preventDefault();
      const currentUserName = that._getCurrentUserName();
      const currentCustomerCode = that._getCurrentCustomerCode();
      const customerCode = $(that.CUSTOMERCODE_SELECTOR).val();
      const newUrl = `/LogOn/KeyCloakSsoPopup?customerCode=${encodeURIComponent(customerCode)}`;
      window.open(newUrl);
      let lastMessage = localStorage.getItem('keyCloakResult');
      that._intervalId = setInterval(() => {
        const newMessage = localStorage.getItem('keyCloakResult');
        if (newMessage !== lastMessage) {
          lastMessage = newMessage;
          const data = JSON.parse(lastMessage);
          if (data.isAuthenticated) {
            that._isAuthenticated = true;
            that._closeWindow();
            const needRefresh = data.userName !== currentUserName || customerCode !== currentCustomerCode;

            if (needRefresh) {
              location.reload();
            }
          } else {
            const content = that._getServerContent(data);
            that._updateWindow(content);
          }
          localStorage.removeItem('keyCloakResult');
          lastMessage = null;
        }
      }, this.KEYCLOAK_RESPONSE_CHECK_INTERVAL);
    };

    this._onCloseWindowHandler = function () {
      that._triggerDeferredCallbacks(that._isAuthenticated);
      that.dispose();
    };

    jQuery(this._windowComponent.element).bind('close', this._onCloseWindowHandler);
  }

  _updateWindow(serverContent) {
    this._detachEvents();
    this._windowComponent.content(serverContent);
    this._attachEvents();
    if (this._intervalId !== null) {
      clearInterval(this._intervalId);
    }
    this._enableWindow();
  }

  _setDefaultValues() {
    const currentUserName = this._getCurrentUserName();
    const currentCustomerCode = this._getCurrentCustomerCode();
    $(this.USERNAME_SELECTOR).val(currentUserName);
    $(this.CUSTOMERCODE_SELECTOR).val(currentCustomerCode);
  }

  _showWindow(data, location) {
    if (!this._windowComponent) {
      let winData = data;
      if (location) {
        winData = $q.getJsonSync(location);
      }
      this._createWindow(this._getServerContent(winData));
      this._attachEvents();
      this._setDefaultValues();
      this._enableWindow();
    }
  }

  _getCurrentUserName() {
    return $('span.userName').text();
  }

  _getCurrentCustomerCode() {
    return $('span.t-in').first().text();
  }

  _getUrl(isSubmit) {
    let url = $(this.FORM_SELECTOR).attr('action');
    if (isSubmit) {
      url = url.replace('?', '/JsonIndex?');
    }
    return url;
  }

  _updateZindex() {
    if (this._windowComponent) {
      $(this._windowComponent.element).css('z-index', this.Z_INDEX);
    }
  }

  _getUseAutoLogin() {
    return $('input#UseAutoLogin').val().toUpperCase() === 'TRUE';
  }

  _disableWindow() {
    $(this.LOADING_SELECTOR).show();
    $(this.FORM_SELECTOR).find(':input:not(:disabled)').prop('disabled', true);
  }

  _enableWindow() {
    $(this.FORM_SELECTOR).find(':input:disabled').prop('disabled', false);
    $(this.LOADING_SELECTOR).hide();
  }

  _closeWindow() {
    this._windowComponent.close();
    if (this._intervalId !== null) {
      clearInterval(this._intervalId);
    }
  }

  _attachEvents() {
    $(this.FORM_SELECTOR).submit(this._onLogonHandler);
    $(this.FORM_SELECTOR).find('a').click(this._onLogonHandler);
    $(this.SSO_SELECTOR).click(this._onSsoHandler);
  }

  _detachEvents() {
    $(this.FORM_SELECTOR).off();
    $(this.FORM_SELECTOR).find('a').off();
    $(this.SSO_SELECTOR).off();
  }

  _triggerDeferredCallbacks(isAuthenticated) {
    $(this).triggerHandler({
      type: this.AJAX_EVENT,

      // @ts-ignore JQueryEvent is used as business logic event
      value: isAuthenticated
    });
  }

  _addDeferredCallback(jqXHR, callback, settings) {
    this._updateZindex();
    $(this).on(this.AJAX_EVENT, e => {
      // @ts-ignore JQueryEvent is used as business logic event
      const isAuthenticated = e.value;
      if (isAuthenticated) {
        jQuery.ajax(settings).done(callback);
      } else {
        callback(jqXHR);
      }
    });
  }

  _clearDeferredCallbacks() {
    $(this).off(this.AJAX_EVENT);
  }

  showLogonForm(data, jqXHR, callback, settings, location) {
    this._addDeferredCallback(jqXHR, callback, settings);
    this._showWindow(data, location);
  }

  needLogon(jqXHR, url, location) {
    if (this.isLogonUrl(url) || this.isWinlogonUrl(url)) {
      return false;
    } else if (jqXHR.getResponseHeader('QP-Not-Authenticated')) {
      return true;
    } else if (this.isLogonUrl(location) || this.isWinlogonUrl(location)) {
      return true;
    }
    return false;
  }

  isLogonUrl(url) {
    const urlForTest = window.CONTROLLER_URL_LOGON.toUpperCase();
    return this.testUrl(url, urlForTest);
  }

  isWinlogonUrl(url) {
    const urlForTest = window.CONTROLLER_URL_WINLOGON.toUpperCase();
    return this.testUrl(url, urlForTest);
  }

  testUrl(url, urlForTest) {
    if (!url || !urlForTest) {
      return false;
    }
    let resultUrl = urlForTest;
    if (urlForTest.endsWith('/')) {
      resultUrl = urlForTest.left(urlForTest.length - 1);
    }

    if (url.replace(window.location.href, '/').toUpperCase().indexOf(resultUrl) === 0) {
      return true;
    }

    const urlBase = this.getBaseUrl(url);
    const locationBase = this.getBaseUrl(window.location.href);
    return urlBase.toUpperCase() === locationBase.toUpperCase()
      && url.replace(urlBase, '/').toUpperCase().indexOf(resultUrl) === 0;
  }

  getBaseUrl(url) {
    const pathArray = url.split('/');
    return `${pathArray[0]}//${pathArray[2]}/`;
  }

  dispose() {
    super.dispose();
    this._clearDeferredCallbacks();

    if (this._windowComponent) {
      this._detachEvents();
      let windowComponent = this._windowComponent;
      let $window = jQuery(windowComponent.element);
      $window.unbind('close', this._onCloseWindowHandler);

      $window = null;
      $c.destroyPopupWindow(windowComponent);
      windowComponent = null;
      this._windowComponent = null;
    }

    $q.collectGarbageInIE();
  }
}


BackendLogOnWindow._instance = null;
BackendLogOnWindow.deferredExecution = function (data, jqXHR, callback, settings) {
  const logon = BackendLogOnWindow.getInstance();
  const location = jqXHR.getResponseHeader('Location');
  if (logon.needLogon(jqXHR, settings.url, location)) {
    logon.showLogonForm(data, jqXHR, callback, settings, location);
    return true;
  }

  return false;
};

BackendLogOnWindow.getInstance = function () {
  if (BackendLogOnWindow._instance === null) {
    BackendLogOnWindow._instance = new BackendLogOnWindow();
  }

  return BackendLogOnWindow._instance;
};

BackendLogOnWindow.destroyInstance = function () {
  if (BackendLogOnWindow._instance) {
    BackendLogOnWindow._instance.dispose();
    BackendLogOnWindow._instance = null;
  }
};


Quantumart.QP8.BackendLogOnWindow = BackendLogOnWindow;
