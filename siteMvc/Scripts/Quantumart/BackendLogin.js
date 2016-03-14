Quantumart.QP8.BackendLogin = function (options) {
    Quantumart.QP8.BackendLogin.initializeBase(this);

    if (options) {
        if (options.useSavedCustomerCode) {
            this._useSavedCustomerCode = options.useSavedCustomerCode;
        }
    }

    jQuery(document).ready(jQuery.proxy(this._initialize, this));
}

Quantumart.QP8.BackendLogin.storageKey = "Quantumart.QP8.BackendLogin.CustomerCode";

Quantumart.QP8.BackendLogin.removeCustomerCode = function () {
    localStorage.removeItem(Quantumart.QP8.BackendLogin.storageKey);
}

Quantumart.QP8.BackendLogin.prototype = {
    storageKey: Quantumart.QP8.BackendLogin.storageKey,
    _useSavedCustomerCode: false,

    _initialize: function ()
    {
        jQuery('#IsSilverlightInstalled').val((Silverlight.isInstalled(null) == true) ? 'True' : 'False');

        if (this._useSavedCustomerCode)
        {
            var isValid = (jQuery(".validation-summary-errors").length == 0);
            if (!isValid) {
                this.removeCustomerCode();
            }
            else {
                var value = localStorage.getItem(this.storageKey);
                var isIntegratedLogin = jQuery("#UserName").length == 0;
                if (isIntegratedLogin) {
                    if (!value) {
                        jQuery("#Login").click(jQuery.proxy(this._loginClick, this));
                    }
                    else {
                        jQuery("#CustomerCode").val(value);
                        jQuery("form").submit();
                    }
                }
            }
        }
    },

    _loginClick: function ()
    {
        localStorage.setItem(this.storageKey, jQuery("#CustomerCode").val());
    },

    removeCustomerCode: Quantumart.QP8.BackendLogin.removeCustomerCode
}

Quantumart.QP8.BackendLogin.registerClass("Quantumart.QP8.BackendLogin");
