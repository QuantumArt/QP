Quantumart.QP8.BackendLogin = function (options) {
  Quantumart.QP8.BackendLogin.initializeBase(this);

  if (options) {
    if (options.useSavedCustomerCode) {
      this._useSavedCustomerCode = options.useSavedCustomerCode;
    }
  }

  $(document).ready($.proxy(this._initialize, this));
};

Quantumart.QP8.BackendLogin.storageKey = 'Quantumart.QP8.BackendLogin.CustomerCode';
Quantumart.QP8.BackendLogin.removeCustomerCode = function () {
  localStorage.removeItem(Quantumart.QP8.BackendLogin.storageKey);
};

Quantumart.QP8.BackendLogin.prototype = {
  storageKey: Quantumart.QP8.BackendLogin.storageKey,
  _useSavedCustomerCode: false,

  _initialize() {
    $('#IsSilverlightInstalled').val(Silverlight.isInstalled(null) ? 'True' : 'False');

    if (this._useSavedCustomerCode) {
      if ($('.validation-summary-errors').length) {
        this.removeCustomerCode();
      } else {
        const value = localStorage.getItem(this.storageKey);
        if (!$('#UserName').length) {
          if (value) {
            $('#CustomerCode').val(value);
            $('form').submit();
          } else {
            $('#Login').click($.proxy(this._loginClick, this));
          }
        }
      }
    }
  },

  _loginClick() {
    localStorage.setItem(this.storageKey, $('#CustomerCode').val());
  },

  removeCustomerCode: Quantumart.QP8.BackendLogin.removeCustomerCode
};

Quantumart.QP8.BackendLogin.registerClass('Quantumart.QP8.BackendLogin');
