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

  get_actionCode: function () {
    return this._actionCode;
  },
  set_actionCode: function (value) {
    this._actionCode = value;
  },
  get_entityId: function () {
    return this._entityId;
  },
  set_entityId: function (value) {
    this._entityId = value;
  },
  get_parentEntityId: function () {
    return this._parentEntityId;
  },
  set_parentEntityId: function (value) {
    this._parentEntityId = value;
  },
  get_alwaysEnabledRefreshButton: function () {
    return this._alwaysEnabledRefreshButton;
  },
  set_alwaysEnabledRefreshButton: function (value) {
    this._alwaysEnabledRefreshButton = value;
  },
  addToolbarItemsToToolbar: function (count) {
    var self = this;
    var queryParams = {
      actionCode: this._actionCode,
      entityId: this._entityId,
      parentEntityId: this._parentEntityId
    };

    if (this.get_isBindToExternal() === true) {
      queryParams = Object.assign({}, queryParams, { boundToExternal: true });
    }

    $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_TOOLBAR}GetToolbarButtonListByActionCode`, queryParams, false, false, (data, textStatus, jqXHR) => {
      if (self._stopDeferredOperations) {
        return;
      }

      var actionToolbarItems = data;
      if (!$q.isNullOrEmpty(self.getDisabledActionCodes())) {
        actionToolbarItems = jQuery.grep(actionToolbarItems, (itm) => {
          return self.getDisabledActionCodes().indexOf(itm.ActionCode) == -1;
        });
      }

      var items = self._getToolbarItemsFromResult(actionToolbarItems);
      Quantumart.QP8.BackendActionToolbar.callBaseMethod(self, 'addToolbarItemsToToolbar', [items, count]);

      $q.clearArray(items);
      $q.clearArray(actionToolbarItems);

      return;
    },

    (jqXHR, textStatus, errorThrown) => {
      if (self._stopDeferredOperations) {
        return;
      }

      $q.processGenericAjaxError(jqXHR);
    }
    );
  },

  tuneToolbarItems: function (entityId, parentEntityId) {
    var self = this;

    var queryParams = { actionCode: this._actionCode, entityId: entityId, parentEntityId: parentEntityId };
    if (this.get_isBindToExternal() === true) {
      queryParams = Object.assign({}, queryParams, { boundToExternal: true });
    }

    if (entityId != 0) {
      $q.getJsonFromUrl('GET', `${window.CONTROLLER_URL_BACKEND_ACTION}GetStatusesList`, queryParams, true, false).done((data) => {
        if (self._stopDeferredOperations) {
          return;
        }

        if (data.success) {
          var actionStatuses = data.actionStatuses;
          if (!$q.isNullOrEmpty(actionStatuses)) {
            Quantumart.QP8.BackendActionToolbar.callBaseMethod(self, 'tuneToolbarItems', [actionStatuses]);
            $q.clearArray(actionStatuses);
            return;
          }
        } else {
          $q.alertError(data.Text);
        }
      }).fail((jqXHR, textStatus, errorThrown) => {
        if (self._stopDeferredOperations) {
          return;
        }

        $q.processGenericAjaxError(jqXHR);
      });
    }
  },

  notifyToolbarButtonClicked: function (eventArgs) {
    this.notify(window.EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED, eventArgs);
  },

  setDisabledActionCodes: function (value) {
    this._disabledActionCodes = value;
  },
  getDisabledActionCodes: function () {
    return this._disabledActionCodes;
  },

  _getToolbarItemsFromResult: function (items) {
    var dataItems = [];
    jQuery.each(items, (index, item) => {
      Array.add(dataItems, {
        Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
        Value: item.ActionCode,
        Text: item.Name,
        Tooltip: item.Name,
        ItemsAffected: item.ItemsAffected,
        Icon: item.Icon,
        AlwaysEnabled: item.ItemsAffected == 0,
        CheckOnClick: false,
        Checked: false,
        IconChecked: null
      });
    });

    return dataItems;
  },

  dispose: function () {
    this._stopDeferredOperations = true;
    Quantumart.QP8.BackendActionToolbar.callBaseMethod(this, 'dispose');
    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendActionToolbar.registerClass('Quantumart.QP8.BackendActionToolbar', Quantumart.QP8.BackendToolbar);
