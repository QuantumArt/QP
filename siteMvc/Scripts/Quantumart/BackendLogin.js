Quantumart.QP8.BackendLogin = function (options) {
  Quantumart.QP8.BackendLogin.initializeBase(this);
  if (options) {
    if (options.useSavedCustomerCode) {
      this._useSavedCustomerCode = options.useSavedCustomerCode;
    }
  }

  jQuery(document).ready(jQuery.proxy(this._initialize, this));
};

Quantumart.QP8.BackendLogin.storageKey = 'Quantumart.QP8.BackendLogin.CustomerCode';
Quantumart.QP8.BackendLogin.removeCustomerCode = function () {
  localStorage.removeItem(Quantumart.QP8.BackendLogin.storageKey);
};

Quantumart.QP8.BackendLogin.prototype = {
  storageKey: Quantumart.QP8.BackendLogin.storageKey,
  _useSavedCustomerCode: false,

  _initialize() {
    if (this._useSavedCustomerCode) {
      const notValid = jQuery('.validation-summary-errors').length > 0;

      if (notValid) {
        this.removeCustomerCode();
      } else {
        const value = localStorage.getItem(this.storageKey);
        const isIntegratedLogin = jQuery('#UserName').length === 0;

        if (isIntegratedLogin) {
          if (value) {
            jQuery('#CustomerCode').val(value);
            jQuery('form').submit();
          } else {
            jQuery('#Login').click(jQuery.proxy(this._loginClick, this));
          }
        }
      }
    }
  },

  _loginClick() {
    localStorage.setItem(this.storageKey, jQuery('#CustomerCode').val());
  },

  removeCustomerCode: Quantumart.QP8.BackendLogin.removeCustomerCode
};

Quantumart.QP8.BackendLogin.registerClass('Quantumart.QP8.BackendLogin');
