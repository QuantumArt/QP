Quantumart.QP8.BackendChangePasswordWindow = function () {
  Quantumart.QP8.BackendChangePasswordWindow.initializeBase(this);
};
Quantumart.QP8.BackendChangePasswordWindow.prototype = {
  WINDOW_TITLE: 'Change Password',
  NEW_PASSWORD_SELECTOR: '#Data_NewPassword',
  NEW_PASSWORD_COPY_SELECTOR: '#Data_NewPasswordCopy',
  CONTAINER_SELECTOR: '.changePasswordContainerContent',
  _popupWindowComponent: null,
  _popupWidth: 500,
  _popupHeight: 235,

  _changePassword() {
    const html = this._getHtml();

    this._popupWindowComponent = $.telerik.window.create({
      title: this.WINDOW_TITLE,
      html,
      width: this._popupWidth,
      height: this._popupHeight,
      modal: true,
      resizable: false,
      draggable: true,
      visible: true,
      actions: []
    }).data('tWindow').center();


    $('.changePassword', this._popupWindowComponent.element).click($.proxy(this._onCloseAndApplyWndClick, this));
    $('form', this._popupWindowComponent.element).submit($.proxy(this._onFilterFormSubmitted, this));


    this._contentContainerElement = $(this.CONTAINER_SELECTOR, this._popupWindowComponent.element).get(0);
    let serverContent;
    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_USER}ChangePassword`,
      {},
      false,
      false,
      data => {
        if (data.success) {
          serverContent = data.view;
        } else {
          $q.alertFail(data.message);
        }
      },
      jqXHR => {
        serverContent = null;
        $q.processGenericAjaxError(jqXHR);
      }
    );
    if (!$q.isNullOrWhiteSpace(serverContent)) {
      $(this._contentContainerElement).html(serverContent);
    }
  },

  _getHtml() {
    const inputCloseAndApplyHtml = `<input class="button changePassword" type="button" value="Change Password">`;

    const html = new $.telerik.stringBuilder()
      .cat('<div class="changePasswordContainerContent"></div>')
      .cat('<div>')
      .cat(inputCloseAndApplyHtml)
      .string();

    return html;
  },

  _onCloseAndApplyWndClick() {
    const that = this;


    const newPasswordElem = $(that.NEW_PASSWORD_SELECTOR).val();
    const newPasswordCopyElem = $(that.NEW_PASSWORD_COPY_SELECTOR).val();

    let content;
    $q.getJsonFromUrl(
      'POST',
      `${window.CONTROLLER_URL_USER}ChangePassword`,
      {
        NewPassword: newPasswordElem,
        NewPasswordCopy: newPasswordCopyElem
      },
      true,
      false,
      data => {
        if (data.isChanging) {
          this._userMustChangePassword = false;
          that._closeWindow();
        } else if (data.success) {
          content = that._getServerContent(data);
          this._contentContainerElement = $(this.CONTAINER_SELECTOR, this._popupWindowComponent.element).get(0);
          that._updateWindow(content);
        }
      },
      (jqXHR, textStatus, errorThrown) => {
        that._updateWindow(errorThrown);
      }
    );
  },

  _getServerContent(data) {
    if (data.success) {
      return data.view;
    }

    return data.message;
  },

  _updateWindow(serverContent) {
    this._contentContainerElement = $(this.CONTAINER_SELECTOR, this._popupWindowComponent.element).get(0);
    $(this._contentContainerElement).html(serverContent);
  },

  _closeWindow() {
    this._popupWindowComponent.close();
  }
};

Quantumart.QP8.BackendChangePasswordWindow.changePassword = function () {
  const popup = new Quantumart.QP8.BackendChangePasswordWindow();
  popup._changePassword();
};

Quantumart.QP8.BackendChangePasswordWindow.registerClass('Quantumart.QP8.BackendChangePasswordWindow');

