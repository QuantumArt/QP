window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGING = 'OnViewToolbarViewsDropDownListSelectedIndexChanging';
window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED = 'OnViewToolbarViewsDropDownListSelectedIndexChanged';
window.EVENT_TYPE_VIEW_TOOLBAR_PREVIEW_BUTTON_CLICKING = 'OnViewToolbarPreviewButtonClicking';
window.EVENT_TYPE_VIEW_TOOLBAR_PREVIEW_BUTTON_CLICKED = 'OnViewToolbarPreviewButtonClicked';
window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKING = 'OnViewToolbarSearchButtonClicking';
window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED = 'OnViewToolbarSearchButtonClicked';
window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKING = 'OnViewToolbarContextButtonClicking';
window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED = 'OnViewToolbarContextButtonClicked';

Quantumart.QP8.BackendViewToolbar = function (toolbarElementId, actionCode, options) {
  Quantumart.QP8.BackendViewToolbar.initializeBase(this, [toolbarElementId, options]);

  this._actionCode = actionCode;

  if ($q.isObject(options)) {
    if (options.viewTypeCode) {
      this._viewTypeCode = options.viewTypeCode;
    }
  }
};

Quantumart.QP8.BackendViewToolbar.prototype = {
  _actionCode: '',
  _parentEntityId: 0,
  _viewTypeCode: '',

  get_actionCode() {
    return this._actionCode;
  },
  set_actionCode(value) {
    this._actionCode = value;
  },
  get_parentEntityId() {
    return this._parentEntityId;
  },
  set_parentEntityId(value) {
    this._parentEntityId = value;
  },

  VIEWS_DROPDOWN_CODE: 'views',
  PREVIEW_BUTTON_CODE: 'preview',
  SEARCH_BUTTON_CODE: 'search',
  CONTEXT_BUTTON_CODE: 'context',

  _getCurrentAction() {
    return $a.getBackendActionByCode(this._actionCode);
  },

  addToolbarItemsToToolbar(count) {
    const action = this._getCurrentAction();

    const items = this._getToolbarItemCollectionFromAction(action);
    Quantumart.QP8.BackendViewToolbar.callBaseMethod(this, 'addToolbarItemsToToolbar', [items, count]);

    $q.clearArray(items);
  },

  getSelectedViewTypeCode() {
    let result = '';
    const $list = this.getToolbarItem(this.VIEWS_DROPDOWN_CODE);
    if ($list) {
      result = jQuery('li.selected', $list).attr('code');
    }
    return result;
  },

  _getToolbarItemCollectionFromAction(action) {
    const self = this;
    const dataItems = [];

    if ($o.checkEntityForVariations(action.EntityType.Code, self.get_parentEntityId())) {
      const variationButton = {
        Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
        Value: this.CONTEXT_BUTTON_CODE,
        Text: '',
        Tooltip: $l.Toolbar.contextButtonTooltip,
        ItemsAffected: 1,
        Icon: 'context.png',
        AlwaysEnabled: true,
        CheckOnClick: true,
        Checked: false,
        TooltipChecked: $l.Toolbar.contextButtonTooltipChecked,
        IconChecked: null
      };

      Array.add(dataItems, variationButton);
    }

    const defaultViewTypeCode = action.DefaultViewType ? action.DefaultViewType.Code : '';
    const selectedViewTypeCode = this._viewTypeCode ? this._viewTypeCode : defaultViewTypeCode;

    const viewsDropDown = {
      Type: window.TOOLBAR_ITEM_TYPE_DROPDOWN,
      Value: this.VIEWS_DROPDOWN_CODE,
      Tooltip: $l.Toolbar.viewsDropDownTooltip,
      ShowButtonText: false,
      ArrowTooltip: $l.Toolbar.viewsDropDownArrowTooltip,
      ItemsAffected: 1,
      AlwaysEnabled: true,
      SelectedSubItemValue: selectedViewTypeCode,
      Items: []
    };

    if (!$q.isNullOrEmpty(action.Views)) {
      jQuery.each(action.Views,
        (index, actionView) => {
          const parentEntityTypeCode = Quantumart.QP8.BackendEntityType.getParentEntityTypeCodeByCode(action.EntityType.Code);
          const view = actionView.ViewType;
          if (view.Code != window.VIEW_TYPE_CODE_TREE || $o.checkEntityForPresenceSelfRelations(parentEntityTypeCode, self.get_parentEntityId())) {
            const dataItem = { Value: view.Code, Text: view.Name, Tooltip: view.Name, Icon: view.Icon };
            Array.add(viewsDropDown.Items, dataItem);
          }
        }
      );
    }

    Array.add(dataItems, viewsDropDown);

    if (action.AllowPreview) {
      const previewButton = {
        Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
        Value: this.PREVIEW_BUTTON_CODE,
        Text: '',
        Tooltip: $l.Toolbar.previewButtonTooltip,
        ItemsAffected: 1,
        Icon: 'show_preview_pane.gif',
        AlwaysEnabled: true,
        CheckOnClick: true,
        Checked: false,
        TooltipChecked: $l.Toolbar.previewButtonTooltipChecked,
        IconChecked: 'hide_preview_pane.gif'
      };

      Array.add(dataItems, previewButton);
    }

    if (action.AllowSearch) {
      const searchButton = {
        Type: window.TOOLBAR_ITEM_TYPE_BUTTON,
        Value: this.SEARCH_BUTTON_CODE,
        Text: '',
        Tooltip: $l.Toolbar.searchButtonTooltip,
        ItemsAffected: 1,
        Icon: 'search.gif',
        AlwaysEnabled: true,
        CheckOnClick: true,
        Checked: false,
        TooltipChecked: $l.Toolbar.searchButtonTooltipChecked,
        IconChecked: null
      };

      Array.add(dataItems, searchButton);
    }

    return dataItems;
  },

  notifyToolbarButtonClicking(eventArgs) {
    const itemValue = eventArgs.get_value();
    let newArgs = Quantumart.QP8.BackendViewToolbarButtonEventArgs.getViewToolbarButtonEventArgsFromToolbarButtonEventArgs(eventArgs);

    if (itemValue == this.PREVIEW_BUTTON_CODE) {
      this.notify(window.EVENT_TYPE_VIEW_TOOLBAR_PREVIEW_BUTTON_CLICKING, newArgs);
    } else if (itemValue == this.SEARCH_BUTTON_CODE) {
      this.notify(window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKING, newArgs);
    } else if (itemValue == this.CONTEXT_BUTTON_CODE) {
      this.notify(window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKING, newArgs);
    }


    newArgs = null;
  },

  notifyToolbarButtonClicked(eventArgs) {
    const itemValue = eventArgs.get_value();
    let newArgs = Quantumart.QP8.BackendViewToolbarButtonEventArgs.getViewToolbarButtonEventArgsFromToolbarButtonEventArgs(eventArgs);

    if (itemValue == this.PREVIEW_BUTTON_CODE) {
      this.notify(window.EVENT_TYPE_VIEW_TOOLBAR_PREVIEW_BUTTON_CLICKED, newArgs);
    } else if (itemValue == this.SEARCH_BUTTON_CODE) {
      this.notify(window.EVENT_TYPE_VIEW_TOOLBAR_SEARCH_BUTTON_CLICKED, newArgs);
    } else if (itemValue == this.CONTEXT_BUTTON_CODE) {
      this.notify(window.EVENT_TYPE_VIEW_TOOLBAR_CONTEXT_BUTTON_CLICKED, newArgs);
    }

    newArgs = null;
  },

  notifyDropDownSelectedIndexChanging(eventArgs) {
    const itemValue = eventArgs.get_itemValue();
    const subItemValue = eventArgs.get_newSubItemValue();

    let newArgs = Quantumart.QP8.BackendViewToolbarButtonEventArgs.getViewToolbarDropDownListEventArgsFromToolbarDropDownListEventArgs(eventArgs);
    let actionView = $a.getActionViewByViewTypeCode(this._getCurrentAction(), subItemValue);
    if (actionView) {
      newArgs.set_controllerActionUrl(actionView.ControllerActionUrl);
      newArgs.set_preventDefaultBehavior(actionView.PreventDefaultBehavior);
      newArgs.set_code(eventArgs.get_newSubItemValue());

      actionView = null;
    }

    if (itemValue == this.VIEWS_DROPDOWN_CODE) {
      this.notify(window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGING, newArgs);
    }

    newArgs = null;
  },

  notifyDropDownSelectedIndexChanged(eventArgs) {
    const itemValue = eventArgs.get_itemValue();
    const subItemValue = eventArgs.get_newSubItemValue();

    let newArgs = Quantumart.QP8.BackendViewToolbarButtonEventArgs.getViewToolbarDropDownListEventArgsFromToolbarDropDownListEventArgs(eventArgs);
    let actionView = $a.getActionViewByViewTypeCode(this._getCurrentAction(), subItemValue);
    if (actionView) {
      newArgs.set_controllerActionUrl(actionView.ControllerActionUrl);
      newArgs.set_preventDefaultBehavior(actionView.PreventDefaultBehavior);
      newArgs.set_code(subItemValue);

      actionView = null;
    }

    if (itemValue == this.VIEWS_DROPDOWN_CODE) {
      this.notify(window.EVENT_TYPE_VIEW_TOOLBAR_VIEWS_DROPDOWN_SELECTED_INDEX_CHANGED, newArgs);
    }

    newArgs = null;
  },

  dispose() {
    Quantumart.QP8.BackendViewToolbar.callBaseMethod(this, 'dispose');

    $q.collectGarbageInIE();
  }
};

Quantumart.QP8.BackendViewToolbar.registerClass('Quantumart.QP8.BackendViewToolbar', Quantumart.QP8.BackendToolbar);

Quantumart.QP8.BackendViewToolbarButtonEventArgs = function () {
  Quantumart.QP8.BackendViewToolbarButtonEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendViewToolbarButtonEventArgs.getViewToolbarButtonEventArgsFromToolbarButtonEventArgs = function (toolbarArgs) {
  const viewToolbarArgs = new Quantumart.QP8.BackendViewToolbarButtonEventArgs();
  viewToolbarArgs.set_value(toolbarArgs.get_value());
  viewToolbarArgs.set_checkOnClick(toolbarArgs.get_checkOnClick());
  viewToolbarArgs.set_checked(toolbarArgs.get_checked());

  return viewToolbarArgs;
};

Quantumart.QP8.BackendViewToolbarButtonEventArgs.registerClass('Quantumart.QP8.BackendViewToolbarButtonEventArgs', Quantumart.QP8.BackendToolbarButtonEventArgs);

Quantumart.QP8.BackendViewToolbarDropDownListEventArgs = function () {
  Quantumart.QP8.BackendViewToolbarDropDownListEventArgs.initializeBase(this);
};

Quantumart.QP8.BackendViewToolbarDropDownListEventArgs.prototype = {
  _controllerActionUrl: '',
  _preventDefaultBehavior: false,
  _code: '',

  get_controllerActionUrl() {
    return this._controllerActionUrl;
  },
  set_controllerActionUrl(value) {
    this._controllerActionUrl = value;
  },
  get_preventDefaultBehavior() {
    return this._preventDefaultBehavior;
  },
  set_preventDefaultBehavior(value) {
    this._preventDefaultBehavior = value;
  },
  get_code() {
    return this._code;
  },
  set_code(value) {
    this._code = value;
  }
};

Quantumart.QP8.BackendViewToolbarButtonEventArgs.getViewToolbarDropDownListEventArgsFromToolbarDropDownListEventArgs = function (toolbarArgs) {
  const viewToolbarArgs = new Quantumart.QP8.BackendViewToolbarDropDownListEventArgs();
  viewToolbarArgs.set_itemValue(toolbarArgs.get_itemValue());
  viewToolbarArgs.set_oldSubItemValue(toolbarArgs.get_oldSubItemValue());
  viewToolbarArgs.set_newSubItemValue(toolbarArgs.get_newSubItemValue());

  return viewToolbarArgs;
};

Quantumart.QP8.BackendViewToolbarDropDownListEventArgs.registerClass('Quantumart.QP8.BackendViewToolbarDropDownListEventArgs', Quantumart.QP8.BackendToolbarDropDownListEventArgs);
