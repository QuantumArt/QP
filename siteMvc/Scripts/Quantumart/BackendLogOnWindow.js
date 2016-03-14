// ****************************************************************************
// *** Компонент "Форма авторизации"										***
// ****************************************************************************

//#region class BackendLogOnWindow
// === Класс "Форма авторизации" ===
Quantumart.QP8.BackendLogOnWindow = function () {
    Quantumart.QP8.BackendLogOnWindow.initializeBase(this);
};

Quantumart.QP8.BackendLogOnWindow.prototype = {
    //#region fields
    _windowComponent: null, // компонент "Всплывающее окно"
    _isAuthenticated: null,
    _userName: null,
    //#endregion

    //#region constants
    FORM_SELECTOR: "form#auth",
    LOADING_SELECTOR: "#authLoading",
    USERNAME_SELECTOR: "#UserName",
    PASSWORD_SELECTOR: "#Password",
    CUSTOMERCODE_SELECTOR: "#CustomerCode",
    Z_INDEX: 50000,
    AJAX_EVENT: "AjaxEvent",
    //#endregion

    //#region internal
    _onLogonHandler: null,
    _onCloseWindowHandler: null,    

    _getServerContent: function(data){
        if (data.success){
            return data.view;
        }
        else{
            return data.message;
        }
    },

    _createWindow: function (serverContent) {
        var that = this;
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
            visible: true,
        }).data("tWindow").center();        

        this._onLogonHandler = function (event) {
            that._disableWindow();
            event.preventDefault();
            var serverContent;
            var currentUserName = that._getGurrentUserName();
            var currentCustomerCode = that._getGurrentCustomerCode();
            var userName = $(that.USERNAME_SELECTOR).val();
            var password = $(that.PASSWORD_SELECTOR).val();
            var customerCode = $(that.CUSTOMERCODE_SELECTOR).val();            
            var method = "GET";
            var useAutoLogin = that._getUseAutoLogin();
            var url = that._getUrl();
            var setDefaultValues = false;

            if (event.type == "submit") {
                method = "POST";
            }
            else {
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
                    CustomerCode: customerCode,
                },
                true,
                false,
                function (data, textStatus, jqXHR) {
                    if (data.isAuthenticated) {
                        that._isAuthenticated = true;
                        that._userName = data.userName;
                    }
                    else {
                        serverContent = that._getServerContent(data)
                    }

                    if (that._isAuthenticated) {

                        that._closeWindow();

                        var needRefresh = that._userName != currentUserName || customerCode != currentCustomerCode;

                        if (needRefresh) {
                            location.reload();
                        }
                    }
                    else {
                        that._updateWindow(serverContent);

                        if (setDefaultValues) {
                            that._setDefaultValues();
                        }
                    }
                },
                function (jqXHR, textStatus, errorThrown) {
                    that._updateWindow(errorThrown)
                }
            );
        };

        this._onCloseWindowHandler = function (event) {
            that._triggerDeferredCallcacks(that._isAuthenticated);
            that.dispose();
        };

        jQuery(this._windowComponent.element).bind("close", this._onCloseWindowHandler);
    },

    _updateWindow: function (serverContent) {
        this._detachEvents();
        this._windowComponent.content(serverContent);
        this._attachEvents();
        this._enableWindow();
    },

    _setDefaultValues: function() {
        var currentUserName = this._getGurrentUserName();
        var currentCustomerCode = this._getGurrentCustomerCode();
        $(this.USERNAME_SELECTOR).val(currentUserName);
        $(this.CUSTOMERCODE_SELECTOR).val(currentCustomerCode);
    },

    _showWindow: function (data) {
        if (!this._windowComponent){
            this._createWindow(this._getServerContent(data));
            this._attachEvents();
            this._setDefaultValues();
            this._enableWindow();
        }
    },

    _getGurrentUserName: function () {
        return $("span.userName").text();
    },

    _getGurrentCustomerCode: function () {
        return $("span.t-in").first().text();
    },

    _getUrl: function () {
        var url = $(this.FORM_SELECTOR).attr("action");

        if (!url.endsWith("/")) {
            url += "/";
        }

        return url;
    },

    _updateZindex: function () {
        if (this._windowComponent) {
            $(this._windowComponent.element).css("z-index", this.Z_INDEX);
        }
    },

    _getUseAutoLogin: function () {
        return $("input#UseAutoLogin").val().toUpperCase() == "TRUE";
    },

    _disableWindow: function () {       
        $(this.LOADING_SELECTOR).show();
        $(this.FORM_SELECTOR).find(':input:not(:disabled)').prop('disabled', true);
    },
    
    _enableWindow: function () {
        $(this.FORM_SELECTOR).find(':input:disabled').prop('disabled', false);
        $(this.LOADING_SELECTOR).hide();
    },

    _closeWindow: function () {        
        this._windowComponent.close();
    },

    //#region window enents
    _attachEvents: function () {
        $(this.FORM_SELECTOR).submit(this._onLogonHandler);
        $(this.FORM_SELECTOR).find("a").click(this._onLogonHandler);
    },

    _detachEvents: function () {
        $(this.FORM_SELECTOR).off();
        $(this.FORM_SELECTOR).find("a").off();
    },
    //#endregion

    //#region object enents
    _triggerDeferredCallcacks: function (isAuthenticated) {
        $(this).triggerHandler({
            type: this.AJAX_EVENT,
            value: isAuthenticated
        });
    },

    _addDeferredCallcack: function (callback, settings) {
        this._updateZindex();
        $(this).on(this.AJAX_EVENT, function (e) {            
            var isAuthenticated = e.value;
            if (isAuthenticated) {
                jQuery.ajax(settings).done(callback);
            }
            else {
                callback({ "success": true });
            }
        });
    },

    _clearDeferredCallcacks: function(){
        $(this).off(this.AJAX_EVENT);
    },
    //#endregion
    //#endregion

    //#region public
    showLogonForm: function (data, callback, settings) {
        this._addDeferredCallcack(callback, settings);
        this._showWindow(data);
    },

    needLogon: function (jqXHR, url) {
        if (
            url.toUpperCase() == CONTROLLER_URL_LOGON.toUpperCase() ||
            url.toUpperCase() == CONTROLLER_URL_WINLOGON.toUpperCase() ) {
            return false;
        }
        else if (jqXHR.getResponseHeader("QP-Not-Authenticated")) {
            return true;
        }
        else {
            return false;
        }
    },

    dispose: function () {
        Quantumart.QP8.BackendLogOnWindow.callBaseMethod(this, "dispose");
        this._clearDeferredCallcacks();

        if (this._windowComponent) {
            this._detachEvents();
            var windowComponent = this._windowComponent;
            var $window = jQuery(windowComponent.element);
            $window
				.unbind("close", this._onCloseWindowHandler)
            ;

            $window = null;
            $c.destroyPopupWindow(windowComponent);
            windowComponent = null;
            this._windowComponent = null;
        }

        Quantumart.QP8.BackendLogOnWindow._instance = null;

        $q.collectGarbageInIE();
    }
    //#endregion
};

Quantumart.QP8.BackendLogOnWindow._instance = null; // экземпляр класса

// Возвращает экземпляр класса "Форма авторизации"
Quantumart.QP8.BackendLogOnWindow.deferredExecution = function Quantumart$QP8$BackendLogOnWindow$deferredExecution(data, jqXHR, callback, settings) {
    var logon = Quantumart.QP8.BackendLogOnWindow.getInstance();

    if (logon.needLogon(jqXHR, settings.url)) {
        logon.showLogonForm(data, callback, settings);
        return true;
    }

    return false;
};

// Возвращает экземпляр класса "Форма авторизации"
Quantumart.QP8.BackendLogOnWindow.getInstance = function Quantumart$QP8$BackendLogOnWindow$getInstance() {
    if (Quantumart.QP8.BackendLogOnWindow._instance == null) {
        Quantumart.QP8.BackendLogOnWindow._instance = new Quantumart.QP8.BackendLogOnWindow();
    }

    return Quantumart.QP8.BackendLogOnWindow._instance;
};

// Уничтожает экземпляр класса "Форма авторизации"
Quantumart.QP8.BackendLogOnWindow.destroyInstance = function Quantumart$QP8$BackendLogOnWindow$destroyInstance() {
    if (Quantumart.QP8.BackendLogOnWindow._instance) {
        Quantumart.QP8.BackendLogOnWindow._instance.dispose();
    }
};

Quantumart.QP8.BackendLogOnWindow.registerClass("Quantumart.QP8.BackendLogOnWindow", Quantumart.QP8.Observable);
//#endregion