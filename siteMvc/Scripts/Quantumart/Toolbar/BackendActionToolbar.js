window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKING = 'OnActionToolbarButtonClicking';
window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED = 'OnActionToolbarButtonClicked';

Quantumart.QP8.BackendActionToolbar = function (toolbarElementId, actionCode, parentEntityId, options) {
  Quantumart.QP8.BackendActionToolbar.initializeBase(this, [toolbarElementId, options]);

  this._actionCode = actionCode;
  this._parentEntityId = parentEntityId;
  if ($q.isObject(options)) {
    if (options.alwaysEnabledRefreshButton) {
      this._alwaysEnabledRefreshButton = options.alwaysEnabledRefreshButton;
    }

    if (options.disabledActionCodes) {
      this._disabledActionCodes = options.disabledActionCodes;
    }
  }
};

Quantumart.QP8.BackendActionToolbar.prototype = {
  _actionCode: '',
  _entityId: 0,
  _parentEntityId: 0,
  _alwaysEnabledRefreshButton: true,
  _stopDeferredOperations: false,
  _disabledActionCodes: null,

  // eslint-disable-next-line camelcase
  get_actionCode() {
    return this._actionCode;
  },

  // eslint-disable-next-line camelcase
  set_actionCode(value) {
    this._actionCode = value;
  },

  // eslint-disable-next-line camelcase
  get_entityId() {
    return this._entityId;
  },

  // eslint-disable-next-line camelcase
  set_entityId(value) {
    this._entityId = value;
  },

  // eslint-disable-next-line camelcase
  get_parentEntityId() {
    return this._parentEntityId;
  },

  // eslint-disable-next-line camelcase
  set_parentEntityId(value) {
    this._parentEntityId = value;
  },

  // eslint-disable-next-line camelcase
  get_alwaysEnabledRefreshButton() {
    return this._alwaysEnabledRefreshButton;
  },

  // eslint-disable-next-line camelcase
  set_alwaysEnabledRefreshButton(value) {
    this._alwaysEnabledRefreshButton = value;
  },

  addToolbarItemsToToolbar(count) {
    const that = this;
    let queryParams = {
      actionCode: this._actionCode,
      entityId: this._entityId,
      parentEntityId: this._parentEntityId
    };

    if (this.get_isBindToExternal()) {
      queryParams = Object.assign({}, queryParams, { boundToExternal: true });
    }

    $q.getJsonFromUrl(
      'GET',
      `${window.CONTROLLER_URL_TOOLBAR}GetToolbarButtonListByActionCode`,
      queryParams,
      false,
      false,
      data => {
        if (!that._stopDeferredOperations) {
          let actionToolbarItems = data;
          if (!$q.isNullOrEmpty(that.getDisabledActionCodes())) {
            actionToolbarItems = $.grep(actionToolbarItems, itm =>
              that.getDisabledActionCodes().indexOf(itm.ActionCode) === -1
            );
          }

          const items = that._getToolbarItemsFromResult(actionToolbarItems);
          Quantumart.QP8.BackendActionToolbar.callBaseMethod(that, 'addToolbarItemsToToolbar', [items, count]);

          $q.clearArray(items);
          $q.clearArray(actionToolbarItems);
        }
      }, jqXHR => {
        if (!that._stopDeferredOperations) {
          $q.processGenericAjaxError(jqXHR);
        }
      }
    );
  },

  tuneToolbarItems(entityId, parentEntityId) {
    const that = this;
    let queryParams = { actionCode: this._actionCode, entityId, parentEntityId };
    if (this.get_isBindToExternal()) {
      queryParams = Object.assign({}, queryParams, { boundToExternal: true });
    }

    $q.warnIfEqDiff(entityId, 0);
    if (entityId !== 0) {
      $q.getJsonFromUrl(
        'GET',
        `${window.CONTROLLER_URL_BACKEND_ACTION}GetStatusesList`,
        queryParams,
        true,
        false
      ).done(data => {
        if (!that._stopDeferredOperations) {
          if (data.success) {
            const actionStatuses = data.actionStatuses;
            if (!$q.isNullOrEmpty(actionStatuses)) {
              Quantumart.QP8.BackendActionToolbar.callBaseMethod(that, 'tuneToolbarItems', [actionStatuses]);
              $q.clearArray(actionStatuses);
            }
          } else {
            $q.alertError(data.Text);
          }
        }
      }).fail(jqXHR => {
        if (!that._stopDeferredOperations) {
          $q.processGenericAjaxError(jqXHR);
        }
      });
    }
  },

  notifyToolbarButtonClicked(eventArgs) {
    this.notify(window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED, eventArgs);
  },

  setDisabledActionCodes(value) {
    this._disabledActionCodes = value;
  },
  getDisabledActionCodes() {
    return this._disabledActionCodes;
  },

  _getToolbarItemsFromResult(items) {
    const dataItems = [];
    $.each(items, (index, item) => {
      Array.add(dataItems, {
        Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
        Value: item.ActionCode,
        Text: item.Name,
        Tooltip: item.Name,
        ItemsAffected: item.ItemsAffected,
        Icon: item.Icon,
        AlwaysEnabled: item.ItemsAffected === 0,
        CheckOnClick: false,
        Checked: false,
        IconChecked: null
      });
    });

    return dataItems;
  },

  dispose() {
    this._stopDeferredOperations = true;
    Quantumart.QP8.BackendActionToolbar.callBaseMethod(this, 'dispose');
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendActionToolbar.registerClass('Quantumart.QP8.BackendActionToolbar', Quantumart.QP8.BackendToolbar);
