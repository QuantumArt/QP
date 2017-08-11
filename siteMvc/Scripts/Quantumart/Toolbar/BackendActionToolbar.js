var EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKING = 'OnActionToolbarButtonClicking';
var EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED = 'OnActionToolbarButtonClicked';

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
  _actionCode: '', // код действия, для которого создается панель инструментов
  _entityId: 0, // ID сущности, для которой создается тулбар
  _parentEntityId: 0, // ID родительской сущности
  _alwaysEnabledRefreshButton: true, // признак, разрешающий всегда оставлять активной кнопку 'Обновить'
  _stopDeferredOperations: false, // признак, отвечающий за остановку все отложенных операций
  _disabledActionCodes: null, // список запрещенных операций

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
      queryParams = jQuery.extend(queryParams, { boundToExternal: true });
    }

    $q.getJsonFromUrl('GET', CONTROLLER_URL_TOOLBAR + 'GetToolbarButtonListByActionCode', queryParams, false, false, function (data, textStatus, jqXHR) {
        if (self._stopDeferredOperations) {
          return;
        }

        var actionToolbarItems = data;
        if (!$q.isNullOrEmpty(self.getDisabledActionCodes())) {
          actionToolbarItems = jQuery.grep(actionToolbarItems, function (itm) {
            return (self.getDisabledActionCodes().indexOf(itm.ActionCode) == -1);
          });
        }

        var items = self._getToolbarItemsFromResult(actionToolbarItems);
        Quantumart.QP8.BackendActionToolbar.callBaseMethod(self, 'addToolbarItemsToToolbar', [items, count]);

        $q.clearArray(items);
        $q.clearArray(actionToolbarItems);

        return;
      },

      function (jqXHR, textStatus, errorThrown) {
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
      queryParams = jQuery.extend(queryParams, { boundToExternal: true });
    }

    if (entityId != 0) {
      $q.getJsonFromUrl('GET', CONTROLLER_URL_BACKEND_ACTION + 'GetStatusesList', queryParams, true, false).done(function (data) {
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
          window.alert(data.Text);
        }
      }).fail(function (jqXHR, textStatus, errorThrown) {
        if (self._stopDeferredOperations) {
          return;
        }

        $q.processGenericAjaxError(jqXHR);
      });
    }
  },

  notifyToolbarButtonClicked: function (eventArgs) {
    this.notify(EVENT_TYPE_ACTION_TOOLBAR_BUTTON_CLICKED, eventArgs);
  },

  setDisabledActionCodes: function (value) {
    this._disabledActionCodes = value;
  },
  getDisabledActionCodes: function () {
    return this._disabledActionCodes;
  },

  _getToolbarItemsFromResult: function (items) {
    var self = this;
    var dataItems = [];
    jQuery.each(items, function (index, item) {
      var dataItem = new Object();
      dataItem.Type = TOOLBAR_ITEM_TYPE_BUTTON;
      dataItem.Value = item.ActionCode;
      dataItem.Text = item.Name;
      dataItem.Tooltip = item.Name;
      dataItem.ItemsAffected = item.ItemsAffected;
      dataItem.Icon = item.Icon;
      dataItem.AlwaysEnabled = item.ItemsAffected == 0;
      dataItem.CheckOnClick = false;
      dataItem.Checked = false;
      dataItem.IconChecked = null;

      Array.add(dataItems, dataItem);
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
